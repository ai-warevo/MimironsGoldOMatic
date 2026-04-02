## MimironsGoldOMatic.Backend (ASP.NET Core)

- **Role:** Orchestrates the payout queue and manages persistent storage.
- **Stack:** ASP.NET Core (.NET 10), Entity Framework Core, PostgreSQL.

## Key Functions

- **Authentication:** Validates Twitch JWTs so requests come from the legitimate Extension.
- **Database:** Persists payout payloads via `PayoutEntity` (mapped to PostgreSQL via EF Core).

## API Endpoints

- **POST** `/api/payouts/claim`: Receives redemptions from Twitch. Validates inputs, saves as `Pending`.
- **GET** `/api/payouts/pending`: Fetched by the Desktop App. Returns all `Pending` or `InProgress` payouts.
- **PATCH** `/api/payouts/{id}/status`: Updates status. The Desktop App calls this to mark a payout as `Sent` after the WoW action completes.

## Additional Libraries

- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
