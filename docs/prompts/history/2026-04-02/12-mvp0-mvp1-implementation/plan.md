## Plan

1. Scaffold MVP-0 under `src/`:
   - solution + .NET projects + TwitchExtension scaffold + WoWAddon placeholders.
2. Add `MVP-1` shared contracts in `MimironsGoldOMatic.Shared`:
   - `PayoutStatus`, `CreatePayoutRequest`, `PayoutDto`, FluentValidation validator.
3. Wire dependencies for Shared validation.
4. Verify with `dotnet sln list` and `dotnet build`.
5. Update `checks.md` and prepare `report.md` including DoD and potential technical debt.
