# Plan

1. Run `dotnet format` on Shared (baseline).
2. Align enums/records with project conventions: multiline positional records where it improves consistency; trailing comma on enum members only (not on record primary ctor params — invalid C#).
3. Build full solution to verify.
