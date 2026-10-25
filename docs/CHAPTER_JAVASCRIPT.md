# JavaScript chapter backend

The deployed backend lives in the separate private repository [lakys-openclaw/terra-backend](https://github.com/lakys-openclaw/terra-backend), connected to Peter's Vercel project `peterpeng0618-3221/terra-chapters`.

Unity retains C# for the simulation, financial ledger, deterministic outcomes, receipt validation and local saves. The hosted service uses plain JavaScript / Node 24 with zero runtime dependencies. It receives the compact `terra-narration-v1` envelope from `AnnualChapters.NarrationRequest`, writes optional Gemini prose and persists the first valid receipt in Redis. It cannot alter game outcomes. Client facts are consistency-checked, not independently established by the server; this is a local-save educational game, not a server-authoritative economy or anti-cheat service.

See [hosted game wiring](HOSTED_GAME.md) for build configuration, public sessions, fallback behavior and verification boundaries. Backend source, deployment configuration and provider/storage credentials belong in the separate backend repo. The retained C# prototype under `services/chapters` supports existing local checks and an explicit `useJavaScriptBackend=false` developer setting; it is not deployed to Vercel.
