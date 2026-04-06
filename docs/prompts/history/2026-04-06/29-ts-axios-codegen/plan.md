## Implementation plan

1. Add audit artifacts and progress tracking for this task.
2. Create a Roslyn-based generator tool under `src/tools/MimironsGoldOMatic.ApiTsGen`.
3. Parse controller routes + HTTP methods and request/response DTO references.
4. Parse DTO records/classes/enums from backend + shared source roots.
5. Map C# types to TypeScript and generate deterministic `models.ts`.
6. Generate Axios client class + endpoint methods into deterministic `client.ts`.
7. Wire generator execution into backend MSBuild `BeforeBuild`.
8. Add documentation for usage and debugging.
9. Validate with backend and extension builds; confirm idempotency by rerun.
