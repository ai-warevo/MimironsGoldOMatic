## MimironsGoldOMatic.Shared

Cross-cutting **API contract** types (DTOs, enums, request/response records) consumed by **Backend** and **Desktop** so JSON and MediatR payloads stay aligned.

Types are grouped by bounded context; folder names match namespaces:

| Folder | Namespace | Contents |
|--------|-----------|----------|
| `Payouts/` | `MimironsGoldOMatic.Shared.Payouts` | Roulette/payout pipeline: `PayoutStatus`, `PayoutDto`, `CreatePayoutRequest`, `PayoutEconomics`, desktop↔EBS payout/who payloads |
| `Gifts/` | `MimironsGoldOMatic.Shared.Gifts` | `!twgift` queue: `GiftRequestState`, gift request DTOs and patch/select/confirm records |
| `Versioning/` | `MimironsGoldOMatic.Shared.Versioning` | `VersionInfoDto` for `/api/version` and Desktop update checks |

Downstream apps often use `GlobalUsings.cs` to import these three namespaces.

Server-side **FluentValidation** and character-name rules live in `MimironsGoldOMatic.Backend.Common`. **Backend** appsettings POCOs are in `MimironsGoldOMatic.Backend.Configuration`.
