## Plan

1. **SPEC §4 / MVP-1:** Replace broad `char.IsLetter` checks with shared `CharacterNameRules` enforcing length 2–12 after trim and Latin/Cyrillic script letter blocks only; wire FluentValidation to use it.
2. **SPEC §6 / `PayoutDto`:** Add `IsRewardSentAnnouncedToChat` to `PayoutDto` with default `false` so Extension/read APIs can match the normative read model.
3. **Docs:** Update `docs/MimironsGoldOMatic.Shared/ReadME.md`, `docs/ROADMAP.md` (MVP-1 bullets), and `docs/IMPLEMENTATION_READINESS.md` (matrix + parity table) so written contracts match code.
4. **Verify:** `dotnet build` on `src/MimironsGoldOMatic.slnx`; note absence of test projects in solution (MVP-6).

## Risks

- Script block lists may omit rare Unicode Latin/Cyrillic code points; can extend ranges later if product requires.
