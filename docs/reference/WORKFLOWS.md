<!-- Updated: 2026-04-05 (Deduplication pass) -->

# Workflows — end-to-end (MVP)

Operational sequence at product level. **Normative detail:** [`docs/overview/SPEC.md`](../overview/SPEC.md), [`docs/overview/INTERACTION_SCENARIOS.md`](../overview/INTERACTION_SCENARIOS.md).

## 1. Enrollment

A **subscriber** types **`!twgold <CharacterName>`** in **broadcast** Twitch chat (**prefix case-insensitive**). The **EBS** ingests via **EventSub** and adds or updates that viewer in the **participant pool** if **`CharacterName`** is valid and **unique** among other viewers. Channel Points are **not** used. Optional Extension **`POST /api/payouts/claim`** mirrors pool rules for Dev Rig — see [`docs/overview/SPEC.md`](../overview/SPEC.md) §5.

`!twgift <CharacterName>` is a separate command and flow for the gift queue (not roulette), see [`docs/overview/SPEC.md`](../overview/SPEC.md) §12.

## 2. Roulette spin

On each **5-minute** boundary only, the EBS runs a spin: **one** candidate from the pool (**uniform random**), **minimum pool size 1**, **no** early spin. **Non-winners** remain in the pool.

## 3. Online verification (`/who`)

The candidate must be **online** in-game. The addon runs **`/who`**, emits **`[MGM_WHO]`** + JSON to **`WoWChatLog.txt`**; Desktop **POST**s **`/api/roulette/verify-candidate`**. If **`online: false`** or late/invalid payload, **no** **`Pending`** payout this cycle (**no** re-draw same window).

## 4. Winner notification (WoW + Extension)

When the EBS creates a **`Pending`** payout, the **Extension** can show **“You won”**. Desktop injects **`/run NotifyWinnerWhisper("<payoutId>","<CharacterName>")`**; the **addon** sends the §9 **winner notification whisper** (Russian body per [`docs/overview/SPEC.md`](../overview/SPEC.md) §9).

## 5. Consent

The winner replies in WoW with **`!twgold`** (whisper, **case-insensitive**). The addon prints **`[MGM_ACCEPT:UUID]`** → log → Desktop → **`POST .../confirm-acceptance`**. This is **not** mail-sent proof.

## 6. Mail preparation

Streamer uses Desktop: **`GET /api/payouts/pending`**, then **Sync/Inject** only after **WoW** target is found → **`PATCH`** → **`InProgress`** → chunked **`/run ReceiveGold("...")`** → addon mail queue / **Prepare Mail**.

## 7. Mail sent → `Sent`

On **MGM-armed** **`MAIL_SEND_SUCCESS`**, the addon prints **`[MGM_CONFIRM:UUID]`** and whispers the completion Russian line. Desktop tails the log and **`PATCH`**es **`Sent`**. EBS may post the §11 **Helix** chat line (best-effort). **Winner removed from pool** on **`Sent`**.

## 8. Development / agent workflow

Contributors and AI agents follow [`AGENTS.md`](../../AGENTS.md): history under **`docs/prompts/history/`**, **plan** / **checks** / **report** per task. **MVP** steps and prompts: [`docs/overview/ROADMAP.md`](../overview/ROADMAP.md).

## 9. Setup (roles)

- **Developers:** [`SETUP-for-developer.md`](../setup/SETUP-for-developer.md) (after shared prerequisites in [`SETUP.md`](../setup/SETUP.md)).
- **Streamers / operators:** [`SETUP-for-streamer.md`](../setup/SETUP-for-streamer.md).
