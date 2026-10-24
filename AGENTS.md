# Project reference

Read [TERRA_PROJECT_SPEC.md](TERRA_PROJECT_SPEC.md) before planning features or making product, gameplay, architecture, or scope decisions. It is Terra's north star for the intended vision, priorities, and constraints.

Keep work aligned with that vision within the user's requested scope. The spec's implementation directives and task lists describe the roadmap; they are not authorization to execute the entire roadmap on every request. Follow the current user's instructions if they change or narrow the scope, and surface material conflicts with the spec.

Preserve the spec as the canonical vision document and update it when the user explicitly requests or agrees to a vision change. Keep setup instructions and implementation status in their corresponding documentation so intended features are not presented as completed work.

For implementation work, consult [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) and [docs/PHASE_STATUS.md](docs/PHASE_STATUS.md). Work within the requested phase and record verification evidence. Phases 0–1 are local-only; later parallel packages follow the ownership boundaries in [docs/PARALLEL_WORK.md](docs/PARALLEL_WORK.md). Prioritize polished, playable hackathon outcomes and simple implementations over production scalability. The plan prepares future goal loops; it does not automatically start a goal or authorize all phases.

Current scope is **one real rural location across the full game**, selected during phase 1. Rural land use is the primary use case. Keep every active phase and parallel package focused on that same location, refining it as mechanics and UI change. Do not create additional levels, city content, or a live location generator until the first location meets the full-game quality gate and the user explicitly chooses expansion. Small technical test fixtures are allowed; they are not extra authored levels.
