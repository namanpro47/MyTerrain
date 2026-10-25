# MyTerrain production release — October 25, 2026

The user explicitly requested committing/pushing all ready work and updating Vercel. This release builds refreshed main `67c0141`: native Unity globe and MyTerrain branding, the integrated play-first HUD, existing annual gameplay, and committed Wisconsin weather reference preparation. The weather artifact is preparation, not new runtime weather behavior.

Older audio, learning and runoff branch commits have patch-equivalent changes on main (`git cherry` reports `-`). Unaccepted visual/crop experiments and the active older-UI guidance branch remain with their owners; generated Unity font/debug metadata and local Pipeline configuration are not product changes. No working tree was reset or overwritten.

The optimized Unity 6000.6.6f1 WebGL export succeeded through Unity CLI 1.0.0-beta.14. Frozen output: `Builds/Verified/myterrain-67c0141`, **20 files / 40,106,177 bytes**. Exact source inputs, CLI provenance and export hashes are recorded alongside this note. The build used the same committed runtime as the 36-assertion combined native-atlas/play-first Chrome receipt under `../play-first/integrated/`; generated Unity debug metadata was recorded and restored.

Automated release checks pass: 94 annual-town + 15 annual-gameplay + 42 chapter + 608 full-game assertions, 19 weather tests, 33 native-atlas package checks, 4 compression tests and browser-wrapper syntax. Chapters used no live provider.

The existing production site was confirmed to serve the earlier HTML selector (`Terra · A world of possibilities`) before this release. Vercel project `terra-browser`, team `namanpro47s-projects`, is the existing destination. The connector returned a team-scope 403; the existing authenticated CLI successfully verified that exact project. Production publication and live verification follow this source checkpoint.
