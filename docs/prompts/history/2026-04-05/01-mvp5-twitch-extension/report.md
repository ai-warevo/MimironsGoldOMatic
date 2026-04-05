# Report: MVP-5 Twitch Extension

## Summary

Implemented the viewer-facing Twitch Extension panel per `docs/overview/ROADMAP.md` MVP-5, `docs/reference/UI_SPEC.md` UI-101–106, and `docs/overview/SPEC.md` §5.1 / §11: Twitch `onAuthorized` + Bearer JWT to EBS, polling `GET /api/roulette/state`, `GET /api/pool/me`, `GET /api/payouts/my-last`, server-skew countdown, roulette phase UX (`collecting` / `spinning` / `verification` / `completed` / `idle`), winner + `Sent` Russian copy via `rewardSentAnnouncement.ts`, exponential backoff (cap 60s) on 429/503/network, optional Dev-only `POST /api/payouts/claim` test block.

## Modified / added files

- `src/MimironsGoldOMatic.TwitchExtension/package.json` — `axios`, `zustand`
- `src/MimironsGoldOMatic.TwitchExtension/vite.config.ts` — `base: './'`
- `src/MimironsGoldOMatic.TwitchExtension/index.html` — title, Twitch helper script
- `src/MimironsGoldOMatic.TwitchExtension/.env.example` — `VITE_MGM_EBS_BASE_URL`
- `src/MimironsGoldOMatic.TwitchExtension/src/` — App, panel UI, Zustand store, EBS client/repository, hooks, types, styles; removed scaffold `App.css`

## Verification

- `npm run build` — pass
- `npm run lint` — pass
- `dotnet test` — not run (no backend changes)

## Notes / follow-ups

- Configure `VITE_MGM_EBS_BASE_URL` (e.g. in `.env.local`) for Dev Rig; EBS must accept the Extension JWT and CORS from the Twitch panel origin.
- `docs/components/twitch-extension/ReadME.md` still describes pre-MVP scaffolding in places; consider aligning in a docs pass.
- Roadmap checkbox for MVP-5 can be flipped to implemented when the owner reviews UX against Dev Rig.
