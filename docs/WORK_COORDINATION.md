# Parallel work without competing for Unity

Adopted October 25, 2026. This operational workflow supplements the historical package boundaries in [PARALLEL_WORK.md](PARALLEL_WORK.md). It does not change game scope, visual acceptance, deployment authorization, or the direct-to-main delivery policy.

## The rule

**Implement in parallel; reserve shared tools; integrate one tested change at a time.**

Each task gets its own worktree and explicit owned files/directories. Unity commands target that worktree by absolute path. Desktop interaction, heavy Unity jobs, and main integration have separate queues. A task waiting for a resource continues independent source work or hands off a precise test request.

Worktrees isolate files, not application focus or editor connections. [Codex worktrees](https://learn.chatgpt.com/docs/environments/git-worktrees) supply separate checkouts. Unity's [command-line reference](https://docs.unity3d.com/6000.0/Documentation/Manual/EditorCommandLineArguments.html) requires a distinct project for concurrent batch work: a project already open in an Editor cannot also be opened in batch mode.

At inspection, five Unity instances were registered, and the Codex Unity MCP processes all targeted `/Users/naman/Documents/GitHub/Terra`. An isolated checkout did not retarget that plugin. There were concurrent exports, shared foreground Chrome tests, and broad assignments whose narrower boundaries lived in chat messages. Preserve the divisions the owners already negotiated. This document does not authorize taking over tasks, stopping their builds, or messaging other chats.

## Shared board and file claims

`scripts/terra-work.py` uses Python 3.9+ and standard-library SQLite. No daemon, subscription, package install, or running coordinator chat is required. State lives at `~/.local/state/terra/coordination.sqlite3`, outside the worktrees. All local worktrees see the same board; repository identity uses the resolved Git common directory. Desktop/heavy reservations also conflict across repositories using the tool. File claims and integration reservations are repository-specific.

```sh
python3 scripts/terra-work.py status
python3 scripts/terra-work.py register hud-20261025 --owner 'chat ID or human name' \
  --paths Assets/Terra/Scripts/UI/TerraHud.AnnualLayout.cs docs/evidence/hud-20261025 \
  --note 'Layout only; shared TerraHud.cs needs a separate handoff'
```

Run from the task's worktree, or pass `--worktree /absolute/task/worktree` before the subcommand. An older worktree may call the script by absolute path from a newer checkout; the tool uses the invoking worktree, not its own directory. Do not pull/reset a dirty or open project just to obtain the tool. Use unique task names for new slices. Register before editing; duplicate checkout owners and overlapping paths are refused atomically, including parent/child paths and `.meta` companions. Paths are literal files/directories, not globs. Claim the smallest coherent slice, including tests/evidence. A conflict means split the task or explicitly transfer ownership.

This is cooperative enforcement, not an OS sandbox. The wrapper rejects conflicting reservations and wrong Unity targets; raw MCP, shell, file edits and computer-use calls can bypass it. Every participating chat must adopt the rules. It does not detect or stop all existing activity. Separate clones on different machines need a cross-machine workflow; this tool coordinates local work.

## Resource reservations

| Resource | Reserve for | Concurrency |
|---|---|---|
| `editor` | Scene operations, Play Mode, imports, recompilation, Editor tests, opening/closing the task's project | One controller per worktree; distinct project editors may coexist |
| `desktop` | Foreground Chrome/Unity input, camera navigation, recordings, native dialogs, visual/performance captures | One participating chat on this Mac |
| `unity-heavy` | Unity exports, cold imports, batch tests, or a performance measurement needing quiet CPU/GPU | One participating heavy job on this Mac |
| `integration` | Fetch/reconcile/test/push a candidate to this repository's main; release/deployment when separately authorized | One integrator at a time |

Request all needed resources together, then acquire. Requests are FIFO among overlapping resources; unrelated work proceeds. A task cannot keep one reservation while queueing for another, avoiding circular waits. An idle open Editor does not need a perpetual reservation; nobody else should operate it without ownership transfer. Leaving Play Mode running counts as active use.

Keep reservations scoped to one stage, not an entire feature pipeline. A background import/build normally needs editor + unity-heavy; release those after completion, then queue a short editor + desktop inspection (or desktop alone for Chrome). Do not hold desktop for a 20-minute build merely because a screenshot is needed afterward. If launching an editor requires foreground interaction, reserve that short stage separately. Performance measurements remain the exception: reserve desktop + unity-heavy together for the quiet timed window.

```sh
# request prints an integer ID; substitute the returned value for 12 below.
python3 scripts/terra-work.py request hud-20261025 \
  --resources editor desktop --purpose 'Five-minute annual HUD capture at 1280x720'
python3 scripts/terra-work.py acquire 12

# Only after acquisition succeeds. Target is pinned to the registered worktree.
python3 scripts/terra-work.py unity 12 -- status --format json
python3 scripts/terra-work.py unity 12 -- command editor.playmode start --format json
# Perform only your claimed work, then save evidence and end the session.
python3 scripts/terra-work.py unity 12 -- command editor.playmode stop --format json
python3 scripts/terra-work.py release 12 --note 'Capture saved; Play Mode and all jobs ended'
```

Exit 3 indicates a coordination/usage failure: read the reason and do not proceed. `status` shows owners, purposes, resources, worktrees, registered base SHAs and the queue. Check after useful independent work; avoid tight polling. Cancel an unneeded waiting request with `release ID --note 'reason'` so it does not block others.

The Unity wrapper supports `status`, `command`, `recompile` and `job`, and pins `--project-path` to the task worktree. Target overrides are rejected. Use read-only `unity status --format json` directly to inventory actual instances before acquiring. Never infer that MCP follows a chat's worktree. Use the wrapper unless the MCP binding is verified to match the owned project and the appropriate reservation is held. Do not change global MCP configuration to suit one task.

For explicit-path CLI build/test/open/close operations, `run` invokes a command without shell interpolation. The caller must request the correct resources and supply the correct path; this generic runner does not infer them:

```sh
# A newly acquired reservation with BOTH editor and unity-heavy is required.
python3 scripts/terra-work.py run 13 -- unity test /absolute/task/worktree \
  --mode EditMode --output /absolute/task/worktree/Builds/test-results.xml
```

A CLI test/build requiring a closed project uses a dedicated validation worktree or waits for its owner to close it. Do not close another owner's editor, share `Library/`, copy source into their open project, or delete Unity lockfiles. Each task owns its Library/Temp/Logs/builds, output files, server ports and Chrome tabs. Distinct ports also give separate local web origins for save-state isolation. Prefer existing pure .NET/Python checks for logic changes.

## Test and integration handoffs

Before entering the queue, commit a coherent candidate and provide:

- Owner/chat, task/worktree, candidate SHA and changed paths.
- Checks already run with results; requested Editor/Chrome steps and expected behavior.
- Build/output location, unique local URL/port, fixture/save, viewport and evidence path.
- Rough slot length. A test request is not a passing result.

Prefer one integration/test owner for combined candidates. Implementation chats submit small slices and continue independent work. The tester uses a dedicated worktree, records the exact tested SHA, and reserves resources. Asset previews can use a separate Editor; foreground testing remains exclusive. A performance slot reserves **both desktop and unity-heavy**, and coordinates with owners to stop active Play Mode/GPU workloads. The tool cannot silence existing jobs or unrelated applications; record actual background conditions.

For main delivery, acquire integration before fetching/reconciling. Follow the direct-main policy and normal fast-forward pushes, preserving incoming work. If remote main advances, fetch/reconcile and rerun affected checks; never force-push. Unity-dependent validation reserves the relevant editor/heavy resources in the same request. Release the integration slot if substantial rework is needed. Deployment retains its existing authorization requirements and uses a frozen tested export. Never rebuild into another task's served output.

Shared TerraMain.cs, TerraHud.cs, scenes, Packages/, ProjectSettings/, build settings and central status documents need explicit ownership. Prefer additive partial files/modules and task-specific evidence while working in parallel; the integrator applies small shared glue/status edits. Avoid assigning several chats “finish everything” without distinct owned slices.

```sh
python3 scripts/terra-work.py finish hud-20261025 \
  --note 'Commit <SHA>; checks <results>; evidence <path>; merged or handed to <owner>'
```

`finish` requires all reservations/requests closed, then releases file claims. It does not delete branches/worktrees. Completed rows remain in SQLite for audit. For a new scope, finish the previous slice after handing off its work, then register a new unique task name; do not silently expand an active claim.

## Recovery and adoption

Reservations never expire automatically. `touch ID` refreshes activity during a long manual session. `status` flags records idle for 30 minutes for investigation, not theft. Commands retain reservations after exit because detached Unity jobs or timed-out editor requests may still run. The runner serializes commands within one reservation, records its PID and refuses release while a command is active.

If a chat/command dies, inspect its owner, process, Unity job status and desktop first. After handoff or verified inactivity, an operator may use `recover ID --confirmed-idle --note 'who checked what and why'`. Recovery refuses while the recorded runner PID exists. It does not kill anything or release the task's file claims. Never recover solely because time elapsed; ask the owner/user if activity is uncertain.

For current chats: let in-flight operations finish; each owner registers its existing narrow slice/worktree before its next operation. Existing agreements outrank a new claimant racing to register. Register ongoing resource use before admitting contenders. An empty initial board does **not** prove Unity/Chrome is free. Existing chats need a steering message or user instruction to adopt this workflow; committing AGENTS.md does not update loaded instructions by itself.

Observed divisions to preserve during rollout: **“Plan a UI/UX quality phase”** owns the visual rebuild; **“Complete the implementation plan”** handles rendering/performance integration; **“Build animated globe map selection”** owns atlas work; **“Complete remaining implementation”** handles annual guidance/recovery and final public walkthroughs; **“Push main and sync Vercel”** handles the current release. These are observed chats, not permanent file grants. Owners register their actual current files. Read the board for live ownership after adoption.

## Verification

Run `python3 -m unittest discover -s tests/coordination -v`. Tests use temporary databases/repos, real separate Git worktrees, concurrent subprocesses and a fake Unity executable. They cover conflicting claims, FIFO acquisition, atomic resource bundles, independent editor slots, wrong-worktree rejection, recovery safeguards, command serialization and project targeting without controlling a real Editor. These tests do not establish gameplay or visual acceptance.

Use `--db` / `TERRA_COORDINATION_DB` only for isolated tests. Real tasks must use the default database; separate databases defeat coordination. Never delete/reset the live database to clear contention.
