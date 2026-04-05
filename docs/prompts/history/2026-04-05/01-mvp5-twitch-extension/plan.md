# Plan

## Scope

- Replace Vite/React scaffold with production-shaped viewer panel aligned to `docs/reference/UI_SPEC.md` UI-101–106 and `docs/overview/SPEC.md` §5.1 / §11.
- Integrate `window.Twitch.ext` (`onAuthorized`) for Bearer JWT to EBS.
- Poll `GET /api/roulette/state`, `GET /api/pool/me`, `GET /api/payouts/my-last` with exponential backoff on 429/503/network (cap 60s) + Retry.
- Countdown from `nextSpinAt` / `serverNow` with skew correction each poll.
- Map `spinPhase` (`idle`, `collecting`, `spinning`, `verification`, `completed`) to UI-103.
- Winner / payout strip from `my-last` + normative Russian `Sent` line via existing `rewardSentAnnouncement.ts`.
- Optional Dev Rig: `POST /api/payouts/claim` + character input only when `import.meta.env.DEV` and `VITE_MGM_DEV_CLAIM=1`.
- Dependencies: `axios`, `zustand` per extension ReadME; no Tailwind (cohesive CSS in `index.css`).

## Risks

- Local `vite` without Twitch helper: no token → UI-101 unauthenticated; document `VITE_MGM_EBS_BASE_URL` and Twitch helper script for Dev Rig.
- Backend claim path may 403 without `DevSkipSubscriberCheck`; Dev Rig copy notes this.

## Files

- Add: types, Twitch auth hook, EBS client/repository, Zustand store, hooks (polling, countdown), components (panel, roulette, error boundary), update `App.tsx`, `main.tsx`, `index.html`, `index.css`, `vite.config.ts` (`base: './'`), `.env.example`, `package.json`.
