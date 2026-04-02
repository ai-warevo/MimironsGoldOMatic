# Mimiron's Gold-o-Matic

Mimiron's Gold-o-Matic is a Twitch-to-World-of-Warcraft (WoW) ecosystem built for the 3.3.5a gold distribution workflow.
It connects a Twitch Extension UI to an ASP.NET Core API, a local WPF desktop app, and finally a WoW 3.3.5a addon (Lua) that delivers the gold via the in-game mail interface.

## High-level Architecture

`Twitch Extension -> ASP.NET Core API -> WPF App (WinAPI/PostMessage) -> WoW 3.3.5a Addon (Lua)`

## What you get

- A shared contract library for payout requests/responses (so all modules agree on the same data model).
- A backend API that validates claims and persists payout state.
- A WPF desktop client that prepares and injects WoW-compatible mail instructions.
- A WoW 3.3.5a addon that hooks mail UI events and fills mail fields from a queued payout payload.

## Repo layout (expanded stack)

- `/src` contains:
  - `.NET 10` solution/projects (Shared DTOs, ASP.NET Core API, WPF desktop client)
  - `Vite + React + TypeScript` Twitch Extension scaffold
  - `WoW 3.3.5a` addon scaffold (`.toc` + Lua)
- `/.github/workflows` will contain CI build/test pipelines for both .NET and the React frontend.
- `/.cursor` contains project-specific AI rules to keep naming and compatibility consistent.

## Notes

WoW 3.3.5a compatibility relies on:

- specific addon frame names / mail hooks
- careful WinAPI focus and message timing from the WPF app
- chunking constraints for command injection into `/run`

