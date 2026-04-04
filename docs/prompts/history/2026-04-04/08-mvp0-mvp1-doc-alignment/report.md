## Report — MVP-0/MVP-1 documentation alignment

### Modified files

- `src/MimironsGoldOMatic.Shared/CharacterNameRules.cs` (new)
- `src/MimironsGoldOMatic.Shared/PayoutDto.cs`
- `src/MimironsGoldOMatic.Shared/PayoutDtoValidator.cs`
- `src/MimironsGoldOMatic.Shared/CreatePayoutRequestValidator.cs`
- `docs/MimironsGoldOMatic.Shared/ReadME.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_READINESS.md`

### Verification

- `dotnet build src/MimironsGoldOMatic.slnx` — succeeded (0 warnings).
- `dotnet test src/MimironsGoldOMatic.slnx` — no test projects in solution; command completed with no tests run.

### MVP-2 readiness (no MVP-2 work started)

- Shared contracts and §4 validation are aligned with `docs/SPEC.md` for MVP-1 scope.
- Backend remains scaffold-only; next milestone is still MVP-2 (EBS, Marten, endpoints, EventSub, etc.) per `docs/ROADMAP.md`.

### Technical debt / follow-ups

- `CharacterNameRules` uses explicit Latin/Cyrillic BMP ranges; extend if WoW/product adds naming rules outside those blocks.
- Add automated tests for `CharacterNameRules` when a test project is introduced (e.g. MVP-6).
