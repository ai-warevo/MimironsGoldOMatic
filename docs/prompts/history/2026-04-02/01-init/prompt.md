# Project: Mimiron's Gold-o-Matic (WoW 3.3.5a Gold Distribution System)
# Architecture: Single Solution (.sln) with Shared Logic

Referencing my previous project @C:\dev\ai-warevo\RPG\, please initialize the new workspace. Use its boilerplate style, directory naming conventions, and CI/CD patterns, but adapt them to the new expanded stack.

## 1. Project Infrastructure
Create the following root directories and files:
- `/docs`: Technical documentation and flowcharts.
- `/.cursor`: Project-specific AI rules (including rules.md).
- `/.github/workflows`: CI/CD pipelines (Build/Test for .NET & React).
- `/src`: Source code root.
- `.gitignore`: Unified template for .NET, Node.js (Vite), and WoW Addons.
- `README.md`: English description of Mimiron's Gold-o-Matic (Twitch-to-WoW 3.3.5a ecosystem).
- `CONTEXT.md`: High-level architecture: "Twitch Extension -> ASP.NET Core API -> WPF App (WinAPI/PostMessage) -> WoW 3.3.5a Addon (Lua)".
- `AGENTS.md`: Define 4 AI Specialist roles:
  1. [Backend/API Expert]: ASP.NET Core, PostgreSQL, EF Core, JWT.
  2. [WPF/WinAPI Expert]: C#, MVVM, Win32 API (PostMessage, SetForegroundWindow).
  3. [WoW Addon/Lua Expert]: WoW API 3.3.5a, FrameXML, Mail Interface Hooking.
  4. [Frontend/Twitch Expert]: React, Vite, TypeScript, Twitch Extension Helper/EBS.

## 2. Solution & Source Setup (.NET 10)
Inside `/src`, create a new Visual Studio Solution (.sln) and projects:
- `MimironsGoldOMatic.Shared`: Class Library (Shared DTOs: PayoutRequest, PayoutResponse; Enums: PayoutStatus { Pending, InProgress, Sent }).
- `MimironsGoldOMatic.Backend`: ASP.NET Core Web API (PostgreSQL integration).
- `MimironsGoldOMatic.Desktop`: WPF Application (Modern UI, MVVM).

## 3. Frontend & Game Modules
- `/src/MimironsGoldOMatic.TwitchExtension`: Initialize a Vite + React (TypeScript) project structure.
- `/src/MimironsGoldOMatic.WoWAddon`: Create `MimironsGoldOMatic.toc` (Interface: 30300) and `MimironsGoldOMatic.lua`.

## 4. Key Configurations
- Setup a GitHub Action `.yml` for building the .NET solution based on the old project's workflow.
- Create a `rules.md` in `/.cursor` enforcing strict naming conventions: "MimironsGoldOMatic" namespace prefix for all modules.
- Ensure all WinAPI logic in WPF is documented for WoW 3.3.5a compatibility.

Start by creating the folder structure and core documentation files (README, CONTEXT, AGENTS).