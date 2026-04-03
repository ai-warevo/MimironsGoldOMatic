# Plan: Documentation update (roulette + !twgold confirmation)

## Scope

Update all repository `*.md` files outside `.cursor/**` and `docs/prompts/**` to describe:

- Participant list (no instant payout on redeem).
- Visual roulette; default spin every 5 minutes; minimum pool size 1.
- Non-winners stay on the list (not removed when someone else wins).
- Channel Points reward **Switch to instant spin** to trigger the next spin without waiting.
- Gold delivery only after recipient is online and confirms via whisper `!twgold` to the streamer; addon intercepts, notifies Desktop utility, server confirms **Sent**.

## Files to touch

- Root: `README.md`, `CONTEXT.md`, `AGENTS.md`
- `docs/`: `SPEC.md`, `ROADMAP.md`, `ReadME.md`, `IMPLEMENTATION_READINESS.md`, component `ReadME.md` files
- `src/MimironsGoldOMatic.TwitchExtension/README.md` — add project-specific note (keep Vite boilerplate minimal addition)

## Risks

- `SPEC.md` is canonical; large narrative change must stay internally consistent with status/API placeholders until implementation catches up.
