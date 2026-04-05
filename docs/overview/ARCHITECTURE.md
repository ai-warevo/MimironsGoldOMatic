<!-- Updated: 2026-04-05 (Deduplication pass) -->

# Architecture — Mimiron's Gold-o-Matic

Non-normative overview. **Canonical contracts:** [`docs/overview/SPEC.md`](SPEC.md). **UI:** [`docs/reference/UI_SPEC.md`](../reference/UI_SPEC.md) (hub) · [`docs/components/twitch-extension/UI_SPEC.md`](../components/twitch-extension/UI_SPEC.md) · [`docs/components/desktop/UI_SPEC.md`](../components/desktop/UI_SPEC.md) · [`docs/components/wow-addon/UI_SPEC.md`](../components/wow-addon/UI_SPEC.md).

## System pipeline

```text
Twitch Extension → EBS (MimironsGoldOMatic.Backend / ASP.NET Core) → WPF Desktop (WinAPI / PostMessage) → WoW 3.3.5a Addon (Lua)
```

Viewers use the **Extension** plus **broadcast Twitch chat**; the streamer uses **Desktop** and **WoW**. There is **no** direct peer link between Extension and Desktop.

## Extension Backend Service (EBS)

**`MimironsGoldOMatic.Backend`** is the **EBS**: Twitch Extension **JWT** validation, **EventSub** (`channel.chat.message`) for **`!twgold <CharacterName>`** enrollment, **Helix** (e.g. §11 reward-sent chat line), REST for the Extension (Bearer) and for Desktop (**`X-MGM-ApiKey`**). Single-broadcaster MVP per deployment — see [`docs/overview/SPEC.md`](SPEC.md) deployment scope.

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

- **Extension ↔ EBS:** HTTPS + Bearer JWT (pool/roulette/payout read APIs).
- **Desktop ↔ EBS:** HTTPS + **`X-MGM-ApiKey`** (pending payouts, status, verify-candidate, confirm-acceptance).
- **Desktop ↔ WoW:** Win32 injection of **`/run`** command lines; single tail of **`Logs\WoWChatLog.txt`** for MGM tags.
- **EBS:** Owns authoritative payout lifecycle and pool state; Desktop drives WoW automation and forwards log-derived events.

## Compatibility (WoW 3.3.5a)

- Addon must use correct **FrameXML** mail frame names and events for **3.3.5a**.
- Desktop **WinAPI** focus, **`PostMessage`** vs **`SendInput`**, and **`/run`** chunking (&lt;255 characters) must stay aligned with [`docs/overview/SPEC.md`](SPEC.md) §8–10.
- WoW **addon payload** format stays compatible with Desktop chunking (see [`docs/overview/SPEC.md`](SPEC.md) §9).
