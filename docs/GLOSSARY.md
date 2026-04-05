<!-- Updated: 2026-04-05 (Deduplication pass) -->

# Glossary (quick reference)

**Authoritative definitions and rules:** [`docs/SPEC.md`](SPEC.md) §1 and later sections. This table is an **index** only.

| Term | Meaning | Primary spec |
|------|---------|----------------|
| **EBS** | **Extension Backend Service** — `MimironsGoldOMatic.Backend` (JWT, EventSub, Helix, REST). | SPEC (EBS section), [`ARCHITECTURE.md`](ARCHITECTURE.md) |
| **Extension** | Twitch panel (React) calling the EBS with **Bearer** JWT. | SPEC §5, §11; [`MimironsGoldOMatic.TwitchExtension/UI_SPEC.md`](MimironsGoldOMatic.TwitchExtension/UI_SPEC.md) |
| **Desktop** | WPF app: API key, log tail, WoW injection. | SPEC §8–10; [`MimironsGoldOMatic.Desktop/ReadME.md`](MimironsGoldOMatic.Desktop/ReadME.md) |
| **Addon** | WoW 3.3.5a Lua: mail queue, whispers, MGM log tags. | SPEC §8–10; [`MimironsGoldOMatic.WoWAddon/ReadME.md`](MimironsGoldOMatic.WoWAddon/ReadME.md) |
| **Participant pool** | Subscribers enrolled via **`!twgold <CharacterName>`** (and optional claim); unique **CharacterName** among others. | SPEC §1, §5 |
| **Spin / roulette** | Scheduled **5-minute** selection of one candidate; server schedule is source of truth. | SPEC §1, §5.1 |
| **Payout** | Record for a **spin winner** (not created at chat enroll). | SPEC §1, §3 |
| **`[MGM_WHO]`** | Addon line: **`/who`** result JSON for **verify-candidate**. | SPEC §8, §10 |
| **`[MGM_ACCEPT:UUID]`** | Addon line after whisper **`!twgold`** consent. | SPEC §9–10 |
| **`[MGM_CONFIRM:UUID]`** | Addon line after **MGM-armed** mail success → drives **`Sent`**. | SPEC §9–10 |
| **Agent** | AI / human contributor following [`AGENTS.md`](../AGENTS.md) and prompt history layout. | `AGENTS.md` |
