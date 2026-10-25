#!/usr/bin/env python3
"""Cooperative task claims and exclusive test reservations for this Mac.

No daemon, external dependencies, auto-expiring locks, or Unity side effects.
SQLite serializes decisions across processes and Git worktrees. This is a
coordination protocol, not an OS sandbox: raw tools can bypass it.
"""
import argparse
from contextlib import contextmanager
import json
import os
from pathlib import Path
import sqlite3
import subprocess
import sys
import time


class Conflict(Exception):
    pass


def git(root, *args):
    return subprocess.check_output(["git", "-C", str(root), *args], text=True).strip()


def identity(root):
    root = Path(git(root, "rev-parse", "--show-toplevel")).resolve()
    common = Path(git(root, "rev-parse", "--git-common-dir"))
    return str(root), str((root / common).resolve())


def scope_path(value, root):
    if any(c in value for c in "*?[") or Path(value).is_absolute():
        raise Conflict("Use literal repository-relative file/directory paths, not globs.")
    path = (Path(root) / value).resolve()
    try:
        normalized = path.relative_to(root).as_posix()
    except ValueError:
        raise Conflict("Claim must stay inside the worktree.")
    if normalized == "." or normalized.split("/")[0] == ".git":
        raise Conflict("Claim specific source areas, not the whole repository or .git.")
    # An asset and its Unity metadata have the same owner.
    return normalized.removesuffix(".meta")


def overlaps(a, b):
    return a == b or a.startswith(b + "/") or b.startswith(a + "/")


class Board:
    def __init__(self, path):
        path = Path(path).expanduser()
        path.parent.mkdir(parents=True, exist_ok=True)
        self.db = sqlite3.connect(path, timeout=10, isolation_level=None)
        self.db.row_factory = sqlite3.Row
        self.db.executescript("""
            CREATE TABLE IF NOT EXISTS tasks (
                repo TEXT, name TEXT, owner TEXT, root TEXT, base TEXT,
                paths TEXT, state TEXT, note TEXT, updated REAL,
                PRIMARY KEY (repo, name));
            CREATE TABLE IF NOT EXISTS reservations (
                id INTEGER PRIMARY KEY, repo TEXT, task TEXT,
                resources TEXT, purpose TEXT, state TEXT,
                created REAL, updated REAL, runner INTEGER, note TEXT);
        """)

    @contextmanager
    def transaction(self):
        self.db.execute("BEGIN IMMEDIATE")
        try:
            yield
        except BaseException:
            self.db.rollback()
            raise
        else:
            self.db.commit()

    def task(self, repo, name, root):
        row = self.db.execute("SELECT * FROM tasks WHERE repo=? AND name=?", (repo, name)).fetchone()
        if not row or row["state"] != "active":
            raise Conflict("No active task with that name. Register it first.")
        if row["root"] != root:
            raise Conflict("Task belongs to another worktree: " + row["root"])
        return row

    def register(self, repo, root, name, owner, paths, base, note):
        paths = sorted(set(scope_path(p, root) for p in paths))
        if not paths:
            raise Conflict("Claim at least one file/directory.")
        with self.transaction():
            rows = self.db.execute("SELECT * FROM tasks WHERE repo=? AND state='active'", (repo,)).fetchall()
            for row in rows:
                if row["name"] == name:
                    raise Conflict("Task already registered; finish it before using a new task ID.")
                if row["root"] == root:
                    raise Conflict("Another task owns this worktree: " + row["name"])
                hits = [(a, b) for a in paths for b in json.loads(row["paths"]) if overlaps(a, b)]
                if hits:
                    raise Conflict("Overlapping claim with " + row["name"] + ": " + repr(hits))
            self.db.execute("INSERT INTO tasks VALUES (?,?,?,?,?,?,?,?,?)",
                            (repo, name, owner, root, base, json.dumps(paths), "active", note, time.time()))

    def resources(self, repo, root, names):
        result = set()
        for name in names:
            if name == "editor":
                result.add("editor:" + root)
            elif name == "integration":
                result.add("integration:" + repo)
            elif name in ("desktop", "unity-heavy"):
                result.add(name)
            else:
                raise Conflict("Unknown resource: " + name)
        if not result:
            raise Conflict("Request at least one resource.")
        return sorted(result)

    def request(self, repo, root, task, resources, purpose):
        resources = self.resources(repo, root, resources)
        with self.transaction():
            self.task(repo, task, root)
            row = self.db.execute("SELECT id FROM reservations WHERE repo=? AND task=? AND state IN ('waiting','held')",
                                  (repo, task)).fetchone()
            if row:
                raise Conflict(f"Task already has reservation {row['id']}; request all needed resources together.")
            now = time.time()
            cur = self.db.execute("INSERT INTO reservations (repo,task,resources,purpose,state,created,updated) VALUES (?,?,?,?,?,?,?)",
                                  (repo, task, json.dumps(resources), purpose, "waiting", now, now))
            return cur.lastrowid

    def reservation(self, repo, root, rid):
        row = self.db.execute("SELECT * FROM reservations WHERE id=? AND repo=?", (rid, repo)).fetchone()
        if not row:
            raise Conflict("Reservation not found in this repository.")
        self.task(repo, row["task"], root)
        return row

    def acquire(self, repo, root, rid):
        with self.transaction():
            row = self.reservation(repo, root, rid)
            if row["state"] == "held":
                return
            if row["state"] != "waiting":
                raise Conflict("Reservation is no longer waiting.")
            needed = set(json.loads(row["resources"]))
            others = self.db.execute("SELECT * FROM reservations WHERE id!=? AND state IN ('held','waiting')", (rid,)).fetchall()
            blockers = [r for r in others if needed.intersection(json.loads(r["resources"]))
                        and (r["state"] == "held" or r["id"] < rid)]
            if blockers:
                raise Conflict("Busy/queued behind " + ", ".join(f"#{r['id']} {r['task']} ({r['state']})" for r in blockers))
            self.db.execute("UPDATE reservations SET state='held', updated=? WHERE id=?", (time.time(), rid))

    def release(self, repo, root, rid, note):
        with self.transaction():
            row = self.reservation(repo, root, rid)
            if row["runner"]:
                raise Conflict("A command is still registered as running; verify it ended before recovery.")
            if row["state"] not in ("waiting", "held"):
                raise Conflict("Reservation already closed.")
            state = "done" if row["state"] == "held" else "cancelled"
            self.db.execute("UPDATE reservations SET state=?,note=?,updated=? WHERE id=?", (state, note, time.time(), rid))

    def touch(self, repo, root, rid):
        with self.transaction():
            row = self.reservation(repo, root, rid)
            if row["state"] != "held":
                raise Conflict("Only a held reservation can be refreshed.")
            self.db.execute("UPDATE reservations SET updated=? WHERE id=?", (time.time(), rid))

    def finish(self, repo, root, name, note):
        with self.transaction():
            self.task(repo, name, root)
            if self.db.execute("SELECT id FROM reservations WHERE repo=? AND task=? AND state IN ('held','waiting')", (repo, name)).fetchone():
                raise Conflict("Release/cancel outstanding reservations first.")
            self.db.execute("UPDATE tasks SET state='done',note=?,updated=? WHERE repo=? AND name=?", (note, time.time(), repo, name))

    def recover(self, rid, note):
        # Explicit operator action only. Elapsed time never makes a lock safe to steal.
        with self.transaction():
            row = self.db.execute("SELECT * FROM reservations WHERE id=?", (rid,)).fetchone()
            if not row or row["state"] not in ("waiting", "held"):
                raise Conflict("No open reservation to recover.")
            if row["runner"]:
                try:
                    os.kill(row["runner"], 0)
                except ProcessLookupError:
                    pass
                else:
                    raise Conflict("Recorded runner still exists; cannot recover.")
            self.db.execute("UPDATE reservations SET state='recovered',runner=NULL,note=?,updated=? WHERE id=?",
                            (note, time.time(), rid))

    def start_command(self, repo, root, rid, required):
        with self.transaction():
            row = self.reservation(repo, root, rid)
            if row["state"] != "held" or row["runner"]:
                raise Conflict("Command requires an acquired, idle reservation.")
            if not set(required).issubset(json.loads(row["resources"])):
                raise Conflict("Reservation is missing required resources: " + repr(required))
            self.db.execute("UPDATE reservations SET runner=?,updated=? WHERE id=?", (os.getpid(), time.time(), rid))

    def end_command(self, rid, returncode):
        with self.transaction():
            self.db.execute("UPDATE reservations SET runner=NULL,note=?,updated=? WHERE id=?",
                            (f"Command exited {returncode}; reservation retained until explicit release.", time.time(), rid))

    def status(self, repo):
        now = time.time()
        with self.transaction():
            tasks = [dict(r) for r in self.db.execute("SELECT * FROM tasks WHERE repo=? AND state='active' ORDER BY name", (repo,))]
            # Include other repositories' reservations: desktop/build capacity is machine-wide.
            rows = [dict(r) for r in self.db.execute("SELECT * FROM reservations WHERE state IN ('waiting','held') ORDER BY id")]
        for row in tasks:
            row["paths"] = json.loads(row["paths"])
        for row in rows:
            row["resources"] = json.loads(row["resources"])
            row["idle_minutes"] = round((now - row["updated"]) / 60, 1)
            row["needs_check"] = row["idle_minutes"] >= 30
        return {"tasks": tasks, "reservations": rows,
                "notice": "Cooperative reservations only. Check pre-existing Unity jobs and desktop owners before first use. Stale reservations never expire automatically."}


def unity_argv(root, args):
    if not args or args[0] not in ("status", "command", "recompile", "job"):
        raise Conflict("Unity wrapper supports status, command, recompile and job; use run for explicit-path build/test/open/close.")
    forbidden = ("--project-path", "--project", "--port", "--pid", "--instance", "--runtime", "--runtime-path")
    if any(a.split("=", 1)[0] in forbidden for a in args):
        raise Conflict("Unity target is pinned to the registered worktree; remove target overrides.")
    return ["unity", *args, "--project-path", root]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--worktree", default=os.getcwd(), help="Task worktree (default: cwd); never the script's directory")
    parser.add_argument("--db", default=os.environ.get("TERRA_COORDINATION_DB", str(Path.home() / ".local/state/terra/coordination.sqlite3")), help="Override for isolated tests only; all real tasks must use the default")
    sub = parser.add_subparsers(dest="action", required=True)
    sub.add_parser("status")
    p = sub.add_parser("register")
    p.add_argument("task")
    p.add_argument("--owner", required=True, help="Chat ID/title or human owner")
    p.add_argument("--paths", nargs="+", required=True)
    p.add_argument("--note", default="")
    p = sub.add_parser("request")
    p.add_argument("task")
    p.add_argument("--resources", nargs="+", required=True, choices=("editor", "desktop", "unity-heavy", "integration"))
    p.add_argument("--purpose", required=True)
    for action in ("acquire", "touch", "release", "recover", "run", "unity"):
        p = sub.add_parser(action)
        p.add_argument("reservation", type=int)
        if action in ("release", "recover"):
            p.add_argument("--note", required=True)
        if action == "recover":
            p.add_argument("--confirmed-idle", required=True, action="store_true", help="Assert owner/job/desktop checked idle; never assume from age")
        if action in ("run", "unity"):
            p.add_argument("command", nargs=argparse.REMAINDER)
    p = sub.add_parser("finish")
    p.add_argument("task")
    p.add_argument("--note", required=True, help="Commit, checks/evidence, integration or handoff result")
    args = parser.parse_args()
    root, repo = identity(args.worktree)
    board = Board(args.db)
    try:
        if args.action == "status":
            print(json.dumps(board.status(repo), indent=2))
        elif args.action == "register":
            board.register(repo, root, args.task, args.owner, args.paths, git(root, "rev-parse", "HEAD"), args.note)
            print("Registered " + args.task)
        elif args.action == "request":
            print(board.request(repo, root, args.task, args.resources, args.purpose))
        elif args.action in ("acquire", "touch"):
            getattr(board, args.action)(repo, root, args.reservation)
            print(args.action + " OK")
        elif args.action == "release":
            board.release(repo, root, args.reservation, args.note)
            print("Released")
        elif args.action == "recover":
            board.recover(args.reservation, args.note)
            print("Recovered; original task's file claims remain until finish")
        elif args.action == "finish":
            board.finish(repo, root, args.task, args.note)
            print("Finished " + args.task)
        elif args.action in ("run", "unity"):
            command = args.command
            if command[:1] == ["--"]:
                command = command[1:]
            if not command:
                raise Conflict("Supply a command after --")
            required = []
            if args.action == "unity":
                command = unity_argv(root, command)
                required = board.resources(repo, root, ["editor"])
            board.start_command(repo, root, args.reservation, required)
            # Leave the reservation held on interruption/unknown outcome. A timeout
            # or detached Unity command can leave work running in the Editor.
            try:
                result = subprocess.run(command, cwd=root)
            except OSError:
                board.end_command(args.reservation, "failed to launch")
                raise
            board.end_command(args.reservation, result.returncode)
            return result.returncode
        return 0
    except (Conflict, sqlite3.Error, OSError) as error:
        print(str(error), file=sys.stderr)
        return 3
    finally:
        board.db.close()


if __name__ == "__main__":
    try:
        sys.exit(main())
    except KeyboardInterrupt:
        print("Interrupted. Reservation retained; inspect the job before recovering it.", file=sys.stderr)
        sys.exit(130)
