# Plan

1. Fix `Program.cs`: `using Marten.Events` for `StreamIdentity`, FluentValidation `AddValidatorsFromAssemblyContaining`, remove insecure JWT stub, dev SHA256 fallback when `ExtensionSecret` empty in Development only.
2. Harden controllers: `[AllowAnonymous]` on EventSub webhook.
3. `PayoutExpirationHostedService`: run expiration sweep before hourly delay.
4. Restore global rate limiting (~5/min) with `/api/twitch/eventsub` excluded.
5. Populate `appsettings.json` / `appsettings.Development.json` with `ConnectionStrings`, `Mgm`, `Twitch` keys.
6. Update `docs/overview/ROADMAP.md` (MVP-2 status), `docs/reference/IMPLEMENTATION_READINESS.md`, `docs/components/backend/ReadME.md`.
7. Verify `dotnet build` on `src/MimironsGoldOMatic.slnx`.
