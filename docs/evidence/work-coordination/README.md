# Local parallel-work coordination — October 25, 2026

Scope: development workflow only. No runtime, scene, asset, Unity package, plugin configuration, or public deployment changes. The original checkout's modified EditorPipelineConfig.json is left intact. No Unity editor was opened, closed, recompiled, or otherwise controlled for this verification.

Initial refreshed remote base: `64384a04e956f623a38c07b7bfa611c941fe231b`. Implementation lives in an isolated `parallel-coordination` worktree. The initial read-only Unity inventory listed five project instances. The existing MCP connections targeted the original Terra checkout, explaining why source isolation alone did not isolate editor control. Installed Unity CLI `1.0.0-beta.14` matched its update check; local command/recompile help confirms absolute `--project-path` support.

## Verification

`python3 -m unittest discover -s tests/coordination -v`: **17 tests pass**, using temporary SQLite databases, disposable Git repositories/worktrees, concurrent command processes and a fake Unity CLI. Covered behaviors:

- File/directory/metadata claim conflicts and duplicate worktree ownership.
- Rejection of repository escapes, globs and whole-repository claims.
- FIFO exclusion for shared resources, atomic multi-resource acquisition, and concurrent independent Editor reservations.
- Shared heavy-job capacity across repository identities; common claims across real Git worktrees.
- No automatic expiry; rejection of release from another task's worktree.
- No holding one reservation while queueing for another; task completion requires closed reservations.
- Commands require held resources and serialize within a reservation; active runner recovery/release is refused.
- Explicit recovery retains its reason; process failures propagate their exit code while retaining the reservation.
- Unity target override rejection and actual wrapper argument/cwd verification through a fake executable.

`git diff --check`: passes. Python standard library only; the tests do not contact Unity or require an Editor/desktop slot. This is workflow validation, not game or visual acceptance.

## Rollout

The shared live board is `~/.local/state/terra/coordination.sqlite3`. The coordination task registered its own six source/documentation areas there. With explicit user authorization, the tested workflow was sent to the five active Terra chats: annual completion, native atlas, visual rebuild, rendering/performance integration, and main/Vercel release. The release owner reported all build, Chrome and local-server work finished. Atlas and visual owners began registering actual claims and queued reservations during rollout. Existing in-flight work retains priority during adoption; a new atlas request was asked to yield behind the visual owner's pre-existing session drain.

The live board, not this snapshot, is authoritative for current reservations. Initial adoption requires each chat to read the protocol; raw tool calls remain able to bypass it. No background scheduler or automatic reassignment is installed. Locks never expire or kill jobs on their own. Main publication of this tooling uses the shared integration reservation and a normal fast-forward push, preserving incoming commits.
