<!-- Updated: 2026-04-05 (Deduplication pass) -->

# Setup overview

This repository has two setup guides, depending on who you are:

| Guide | Audience |
|--------|----------|
| **[docs/SETUP_COMMON.md](docs/SETUP_COMMON.md)** | Shared: prerequisites, clone, first **dotnet** / **npm** build (referenced from the developer guide). |
| **[SETUP-for-developer.md](SETUP-for-developer.md)** | Contributors: PostgreSQL, Backend configuration (including Twitch keys), running projects from source. |
| **[SETUP-for-streamer.md](SETUP-for-streamer.md)** | Streamers/operators: installing the WoW addon, Twitch Extension, and Desktop app, and what still requires a developer build today. |

Start with the table above; the developer guide explains every `appsettings` field (including `Twitch` and `Mgm`) and where to obtain values from Twitch.
