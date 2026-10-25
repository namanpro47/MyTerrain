"""Run with: python3 -m unittest discover -s tests/coordination -v"""
import concurrent.futures
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

SCRIPT = Path(__file__).resolve().parents[2] / "scripts/terra-work.py"
spec = importlib.util.spec_from_file_location("terra_work", SCRIPT)
work = importlib.util.module_from_spec(spec)
spec.loader.exec_module(work)


class BoardTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.db = Path(self.temp.name) / "board.sqlite3"
        self.board = work.Board(self.db)
        self.repo = "/repo/.git"
        self.a, self.b = "/worktrees/a", "/worktrees/b"
        self.board.register(self.repo, self.a, "a", "chat-a", ["Assets/Rendering"], "sha-a", "")
        self.board.register(self.repo, self.b, "b", "chat-b", ["Assets/UI"], "sha-b", "")

    def tearDown(self):
        self.board.db.close()
        self.temp.cleanup()

    def request(self, task, resources):
        return self.board.request(self.repo, self.a if task == "a" else self.b, task, resources, "verify")

    def test_parent_child_and_meta_claims_conflict(self):
        for path in ("Assets", "Assets/Rendering/Terrain.cs", "Assets/Rendering.meta"):
            with self.subTest(path=path), self.assertRaises(work.Conflict):
                self.board.register(self.repo, "/c", "c", "chat-c", [path], "sha-c", "")

    def test_same_worktree_disallowed_even_with_disjoint_files(self):
        with self.assertRaises(work.Conflict):
            self.board.register(self.repo, self.a, "c", "chat-c", ["docs"], "sha-c", "")

    def test_scope_escape_globs_and_whole_repo_disallowed(self):
        for path in ("../outside", "/absolute", "Assets/*.cs", ".", ".git/config"):
            with self.subTest(path=path), self.assertRaises(work.Conflict):
                work.scope_path(path, self.a)

    def test_fifo_for_overlapping_resources_and_explicit_release(self):
        first = self.request("a", ["desktop"])
        second = self.request("b", ["desktop"])
        with self.assertRaises(work.Conflict):
            self.board.acquire(self.repo, self.b, second)
        self.board.acquire(self.repo, self.a, first)
        with self.assertRaises(work.Conflict):
            self.board.acquire(self.repo, self.b, second)
        self.board.release(self.repo, self.a, first, "capture saved")
        self.board.acquire(self.repo, self.b, second)

    def test_disjoint_editor_reservations_can_run_together(self):
        a = self.request("a", ["editor"])
        b = self.request("b", ["editor"])
        self.board.acquire(self.repo, self.a, a)
        self.board.acquire(self.repo, self.b, b)
        self.assertEqual(2, sum(r["state"] == "held" for r in self.board.status(self.repo)["reservations"]))

    def test_multi_resource_acquire_is_atomic(self):
        a = self.request("a", ["desktop"])
        b = self.request("b", ["editor", "desktop", "unity-heavy"])
        self.board.acquire(self.repo, self.a, a)
        with self.assertRaises(work.Conflict):
            self.board.acquire(self.repo, self.b, b)
        self.assertEqual("waiting", self.board.reservation(self.repo, self.b, b)["state"])

    def test_unrelated_waiters_do_not_block_other_resources(self):
        self.request("a", ["desktop"])
        b = self.request("b", ["unity-heavy"])
        self.board.acquire(self.repo, self.b, b)

    def test_machine_resources_conflict_across_repositories(self):
        a = self.request("a", ["unity-heavy"])
        self.board.acquire(self.repo, self.a, a)
        self.board.register("/other/.git", "/other", "c", "chat-c", ["src"], "sha", "")
        c = self.board.request("/other/.git", "/other", "c", ["unity-heavy"], "test")
        with self.assertRaises(work.Conflict):
            self.board.acquire("/other/.git", "/other", c)

    def test_no_automatic_expiry_or_wrong_worktree_release(self):
        a = self.request("a", ["desktop"])
        self.board.acquire(self.repo, self.a, a)
        self.board.db.execute("UPDATE reservations SET updated=0 WHERE id=?", (a,))
        row = self.board.status(self.repo)["reservations"][0]
        self.assertTrue(row["needs_check"])
        self.assertEqual("held", row["state"])
        with self.assertRaises(work.Conflict):
            self.board.release(self.repo, self.b, a, "wrong owner")

    def test_finish_requires_reservations_closed_and_releases_claims(self):
        a = self.request("a", ["desktop"])
        with self.assertRaises(work.Conflict):
            self.board.finish(self.repo, self.a, "a", "done")
        self.board.release(self.repo, self.a, a, "cancelled test")
        self.board.finish(self.repo, self.a, "a", "commit and evidence")
        self.board.register(self.repo, "/c", "c", "chat-c", ["Assets/Rendering"], "sha-c", "")

    def test_task_cannot_hold_one_resource_while_queueing_for_another(self):
        a = self.request("a", ["editor"])
        self.board.acquire(self.repo, self.a, a)
        with self.assertRaises(work.Conflict):
            self.request("a", ["desktop"])

    def test_commands_require_correct_held_resources_and_serialize(self):
        a = self.request("a", ["editor"])
        required = self.board.resources(self.repo, self.a, ["editor"])
        with self.assertRaises(work.Conflict):
            self.board.start_command(self.repo, self.a, a, required)
        self.board.acquire(self.repo, self.a, a)
        with self.assertRaises(work.Conflict):
            self.board.start_command(self.repo, self.a, a, ["desktop"])
        self.board.start_command(self.repo, self.a, a, required)
        with self.assertRaises(work.Conflict):
            self.board.start_command(self.repo, self.a, a, required)
        with self.assertRaises(work.Conflict):
            self.board.release(self.repo, self.a, a, "still running")
        with self.assertRaises(work.Conflict):
            self.board.recover(a, "runner still exists")
        self.board.end_command(a, 6)
        self.assertEqual("held", self.board.reservation(self.repo, self.a, a)["state"])
        self.board.release(self.repo, self.a, a, "checked timed-out Unity job ended")

    def test_explicit_recovery_is_audited(self):
        a = self.request("a", ["desktop"])
        self.board.acquire(self.repo, self.a, a)
        self.board.recover(a, "owner confirmed no remaining desktop work")
        row = self.board.db.execute("SELECT * FROM reservations WHERE id=?", (a,)).fetchone()
        self.assertEqual("recovered", row["state"])
        self.assertIn("owner confirmed", row["note"])

    def test_unity_target_overrides_rejected(self):
        for flag in ("--project-path", "--port=7800", "--runtime", "--pid", "--instance"):
            with self.subTest(flag=flag), self.assertRaises(work.Conflict):
                work.unity_argv(self.a, ["command", "script", "eval", "return 1;", flag])
        self.assertEqual(["unity", "status", "--format", "json", "--project-path", self.a],
                         work.unity_argv(self.a, ["status", "--format", "json"]))


class ProcessTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name).resolve()
        self.repo, self.other = self.root / "repo", self.root / "worktree"
        self.db = self.root / "board.sqlite3"
        self.git("init", "-q", str(self.repo))
        self.git("-C", str(self.repo), "-c", "user.name=Test", "-c", "user.email=test@example.invalid", "commit", "--allow-empty", "-qm", "test")
        self.git("-C", str(self.repo), "worktree", "add", "--detach", "-q", str(self.other))

    def tearDown(self):
        self.temp.cleanup()

    def git(self, *args):
        subprocess.run(["git", *args], capture_output=True, text=True, check=True)

    def cli(self, root, *args):
        return subprocess.run([sys.executable, str(SCRIPT), "--db", str(self.db), "--worktree", str(root), *args], capture_output=True, text=True)

    def register(self, root, task, path):
        result = self.cli(root, "register", task, "--owner", task, "--paths", path)
        self.assertEqual(0, result.returncode, result.stderr)

    def test_real_worktrees_share_claims_and_atomic_process_queue(self):
        self.register(self.repo, "a", "Assets/Render")
        self.assertEqual(3, self.cli(self.other, "register", "bad", "--owner", "bad", "--paths", "Assets/Render/x.cs").returncode)
        self.register(self.other, "b", "Assets/UI")
        a = self.cli(self.repo, "request", "a", "--resources", "desktop", "--purpose", "test").stdout.strip()
        b = self.cli(self.other, "request", "b", "--resources", "desktop", "--purpose", "test").stdout.strip()
        with concurrent.futures.ThreadPoolExecutor() as pool:
            futures = [pool.submit(self.cli, root, "acquire", rid) for root, rid in ((self.other, b), (self.repo, a))]
            results = [f.result() for f in futures]
        self.assertEqual([3, 0], [r.returncode for r in results])
        status = json.loads(self.cli(self.other, "status").stdout)
        self.assertEqual(2, len(status["tasks"]))
        self.assertEqual(1, sum(r["state"] == "held" for r in status["reservations"]))

    def test_run_forwards_failure_and_retains_reservation(self):
        self.register(self.repo, "a", "docs")
        rid = self.cli(self.repo, "request", "a", "--resources", "integration", "--purpose", "test").stdout.strip()
        self.assertEqual(0, self.cli(self.repo, "acquire", rid).returncode)
        result = self.cli(self.repo, "run", rid, "--", sys.executable, "-c", "import sys; sys.exit(7)")
        self.assertEqual(7, result.returncode, result.stderr)
        status = json.loads(self.cli(self.repo, "status").stdout)
        self.assertEqual("held", status["reservations"][0]["state"])
        self.assertIsNone(status["reservations"][0]["runner"])

    def test_unity_wrapper_uses_task_worktree_not_script_location(self):
        self.register(self.other, "a", "docs")
        rid = self.cli(self.other, "request", "a", "--resources", "editor", "--purpose", "test").stdout.strip()
        self.cli(self.other, "acquire", rid)
        fake_bin = self.root / "bin"
        fake_bin.mkdir()
        fake = fake_bin / "unity"
        fake.write_text('#!' + sys.executable + '\nimport json, os, sys\nprint(json.dumps({"args":sys.argv[1:],"cwd":os.getcwd()}))\n')
        fake.chmod(0o755)
        env = dict(os.environ, PATH=str(fake_bin) + os.pathsep + os.environ["PATH"])
        result = subprocess.run([sys.executable, str(SCRIPT), "--db", str(self.db), "--worktree", str(self.other), "unity", rid, "--", "status", "--format", "json"], env=env, capture_output=True, text=True)
        self.assertEqual(0, result.returncode, result.stderr)
        output = json.loads(result.stdout)
        self.assertEqual(str(self.other), output["cwd"])
        self.assertEqual(["--project-path", str(self.other)], output["args"][-2:])


if __name__ == "__main__":
    unittest.main()
