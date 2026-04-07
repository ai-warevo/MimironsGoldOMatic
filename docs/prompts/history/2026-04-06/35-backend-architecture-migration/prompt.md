User request (2026-04-06)

- Migrate backend to new target architecture (split current monolithic backend into projects):
  - `MimironsGoldOMatic.Backend.Api`
  - `MimironsGoldOMatic.Backend.Abstract`
  - `MimironsGoldOMatic.Backend.Cli`
  - `MimironsGoldOMatic.Backend.DataAccess`
  - `MimironsGoldOMatic.Backend.Domain`
  - `MimironsGoldOMatic.Backend.Infrastructure`
  - `MimironsGoldOMatic.Backend.IntegrationTests`
  - `MimironsGoldOMatic.Backend.Common`
  - `MimironsGoldOMatic.Backend.Services`

Constraints / notes

- Keep namespaces prefixed with `MimironsGoldOMatic.*`.
- Follow repository agent protocol (`AGENTS.md`) with plan/checks/report artifacts.
