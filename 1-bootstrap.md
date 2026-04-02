# Project: Mimiron's Gold-o-Matic (WoW 3.3.5a Gold Distribution System)
# Architecture: Single Solution (.sln) with .NET 10 Shared Logic

## 1. Project Structure (.NET 10 / C# 14)
Initialize a Visual Studio Solution in `/src` with the following:
- **MimironsGoldOMatic.Shared**: Class Library (.NET 10). 
  - Contracts: `PayoutDto`, `PayoutStatus` (Pending, InProgress, Sent).
  - Validation: Shared logic for character names and gold limits.
- **MimironsGoldOMatic.Backend**: ASP.NET Core Web API (.NET 10).
  - Persistence: Entity Framework Core + PostgreSQL.
  - Features: JWT Validation for Twitch, Swagger/OpenAPI.
- **MimironsGoldOMatic.Desktop**: WPF Application (.NET 10).
  - Pattern: MVVM (CommunityToolkit.Mvvm).
  - Features: Win32 API Integration, HttpClient for Payout processing.
- **MimironsGoldOMatic.TwitchExtension**: React + Vite + TypeScript.
- **MimironsGoldOMatic.WoWAddon**: WoW 3.3.5a Lua Addon.

## 2. MimironsGoldOMatic.Backend (Logic)
- **Database Model**: `PayoutEntity` { Guid Id, string TwitchUser, string CharacterName, long GoldAmount, PayoutStatus Status, DateTime CreatedAt }.
- **Endpoints**:
  - `POST /api/payouts/claim`: Entry point for Twitch Extension. Validates request and saves to DB.
  - `GET /api/payouts/pending`: Polling point for WPF Desktop app.
  - `PATCH /api/payouts/{id}`: Update status (e.g., mark as "Sent" after WoW action).

## 3. MimironsGoldOMatic.Desktop (WPF + Win32)
- **UI**: Modern DataGrid, "Sync to WoW" button, status indicators.
- **Win32 Integration**:
  - Implement `WindowFinder` to target `WoW.exe` (WotLK 3.3.5a).
  - **Data Injection**:
    - Build Lua chunks: `/run ReceiveGold("Nick1:100;Nick2:500;")`.
    - Handle 255-character limit per WoW command by splitting lists into multiple chunks.
    - Automation: `SetForegroundWindow` -> `PostMessage` (preferred) or `SendWait` to trigger: `Enter` key -> `Paste command` -> `Enter` key.

## 4. MimironsGoldOMatic.WoWAddon (3.3.5a Lua)
- **TOC**: Interface `30300`.
- **Core**: Global `ReceiveGold(dataString)` to parse semi-colon delimited strings into `MimironsGoldOMaticQueue` table.
- **UI Hook**: 
  - Register `MAIL_SHOW` event.
  - Anchor `MimironsGoldOMaticFrame` to the right of `SendMailFrame`.
  - Feature: One-click "Prepare Mail" which auto-fills `SendMailNameEditBox`, `SendMailSubjectEditBox`, and converts Gold to Copper for `MoneyInputFrame_SetCopper`.

## 5. MimironsGoldOMatic.TwitchExtension (React)
- **Frontend**: Clean UI with Character Name input and "Redeem" button.
- **Twitch Integration**: Use `window.Twitch.ext` for helper functions.

## Implementation Sequence for Cursor:
1. **Phase 1: Shared Logic.** Generate `MimironsGoldOMatic.Shared` with all DTOs and Enums first to establish the data contract.
2. **Phase 2: WoW Addon.** Generate the Lua/TOC files for 3.3.5a to define the "Receiver" end.
3. **Phase 3: Desktop App.** Implement the WPF UI and Win32 automation logic.
4. **Phase 4: Backend API.** Build the ASP.NET Core service and PostgreSQL schema.
5. **Phase 5: Extension.** Scaffold the Vite/React frontend.

**Important**: Maintain "MimironsGoldOMatic" naming convention across all files and namespaces. Use .NET 10 features like Primary Constructors where applicable.
