# Task prompt (retroactive)

Session combined several related requests that resulted in commit `2b1eec2` (`docs(spec): align MVP docs with EBS, Helix §11, and roadmap checklist`).

## 1) Final Documentation Alignment

- **§11 Twitch announcement:** Option **(a)** — EBS sends the reward-sent line via **Helix** `Send Chat Message` immediately after status transitions to `Sent`.
- **Terminology:** **`MimironsGoldOMatic.Backend`** is the **EBS (Extension Backend Service)**; use in `docs/overview/SPEC.md` for JWT and Helix credential ownership.
- **EventSub:** In Component Contracts, explicitly name **Twitch EventSub** as transport for **Chat → Backend** enrollment.

## 2) Final Architect Responses (8 decisions)

Apply to `docs/overview/SPEC.md`, `docs/overview/ROADMAP.md`, `CONTEXT.md`, and component ReadMEs:

1. **Outbox vs §11 Helix:** **(C)** Inline try + retry — **no** Outbox table/worker in MVP; EBS calls Helix immediately after DB commit to `Sent`; **3** attempts.
2. **Helix failure when already `Sent`:** **(A)** Accept `Sent` without chat line; **log**; **no** rollback.
3. **Duplicate PATCH → `Sent`:** **Strictly once** per `PayoutId` — e.g. **`IsRewardSentAnnouncedToChat`** or transition guard.
4. **EventSub enrollment:** **EventSub payload only** — no Helix subscriber lookups during enrollment (quota).
5. **§11 wording:** **Must** — EBS **must** **attempt** to post when Helix is available (best-effort after retries).
6. **`docs/overview/ROADMAP.md`:** Align **Backend** → **EBS** in prose; keep project name `MimironsGoldOMatic.Backend`.
7. **Open scenarios (SC-022, etc.):** **Do not implement** speculative APIs; ignore **future / not MVP** scenarios during current phases.
8. **Roadmap process:** Owner supplies mandatory checklist per step (SPEC §, TC-, **no** endpoints outside SPEC).

## 3) Follow-up

- Proceed to **Phase 1: MimironsGoldOMatic.Shared** after docs updated (solution `slnx`, build verification).
- **`/commit`** with conventional message and repo footer trailers.

## References

- Prior related logs: `docs/prompts/history/2026-04-04/04-interactive-mvp-risk-closure/`, `05-second-pass-mvp-locks/`.
