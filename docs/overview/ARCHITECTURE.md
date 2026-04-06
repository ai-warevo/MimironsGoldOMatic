<!-- Updated: 2026-04-05 (Deduplication pass) -->

# Architecture — Mimiron's Gold-o-Matic

This page is a non-normative architecture map. For contracts, payloads, and lifecycle rules, use [`docs/overview/SPEC.md`](SPEC.md) as the source of truth. UI behavior and screen inventory live in [`docs/reference/UI_SPEC.md`](../reference/UI_SPEC.md) (hub) and component specs for [Twitch Extension](../components/twitch-extension/UI_SPEC.md), [Desktop](../components/desktop/UI_SPEC.md), and [WoW Addon](../components/wow-addon/UI_SPEC.md).

## System pipeline

```text
Twitch Extension → EBS (MimironsGoldOMatic.Backend / ASP.NET Core) → WPF Desktop (WinAPI / PostMessage) → WoW 3.3.5a Addon (Lua)
```

Viewers interact through the **Twitch Extension** and **broadcast Twitch chat**. The streamer operates through **Desktop** and the **WoW client**. The Extension and Desktop do not communicate directly; the EBS is the only integration hub.

## Extension Backend Service (EBS)

**`MimironsGoldOMatic.Backend`** is the **EBS**. It validates Twitch Extension JWTs, ingests EventSub (`channel.chat.message`) for **`!twgold <CharacterName>`** enrollment, executes Helix actions (including §11 reward-sent chat announcements), and exposes REST APIs for both the Extension (Bearer JWT) and Desktop (`X-MGM-ApiKey`). MVP deployment scope is single-broadcaster per environment (see [`docs/overview/SPEC.md`](SPEC.md)).

## Runtime components

| Component | Role |
|-----------|------|
| **Twitch Extension** | Viewer panel: roulette, pool hints, winner status; calls EBS with Extension JWT. |
| **EBS** | Pool, roulette, payouts, chat ingestion, persistence (Marten / PostgreSQL). |
| **Desktop (WPF)** | Polls pending payouts, tails **`WoWChatLog.txt`**, injects **`/run`** into WoW, calls EBS with API key. |
| **WoW addon** | Mail UI queue, whispers, **`[MGM_WHO]`** / **`[MGM_ACCEPT:UUID]`** / **`[MGM_CONFIRM:UUID]`** in the chat log. |
| **Shared (.NET)** | DTOs and validation shared by EBS and Desktop. |

## Architectural patterns (MVP)

- **DDD:** Core rules (limits, state transitions) live in the domain layer (aggregates / value objects).
- **CQRS:** Commands vs queries; **MediatR** dispatches handlers **in the EBS only** (not in `Shared`).
- **Event sourcing:** **Marten** on **PostgreSQL** is the write-side source of truth; payout changes are persisted as events.
- **EF Core (optional):** Read-model / projections only — not the canonical write store.

## Key relationships

- **Extension ↔ EBS:** HTTPS with Bearer JWT for pool, roulette, and payout status reads.
- **Desktop ↔ EBS:** HTTPS with **`X-MGM-ApiKey`** for payout operations and roulette verification calls.
- **Desktop ↔ WoW:** Win32 command injection (`/run ...`) plus a single `Logs\WoWChatLog.txt` tail for MGM tags.
- **EBS authority:** payout lifecycle and pool state remain authoritative on the backend; Desktop acts as a bridge for WoW actions and log-derived confirmations.

## Compatibility (WoW 3.3.5a)

- Addon must use correct **FrameXML** mail frame names and events for **3.3.5a**.
- Desktop WinAPI strategy (**`PostMessage`** primary, **`SendInput`** fallback), focus behavior, and **`/run`** chunking (&lt;255 characters) must stay aligned with [`docs/overview/SPEC.md`](SPEC.md) §8–10.
- WoW **addon payload** format stays compatible with Desktop chunking (see [`docs/overview/SPEC.md`](SPEC.md) §9).
