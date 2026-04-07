# Plan

1. Introduce folders **`Payouts/`**, **`Gifts/`**, **`Versioning/`** with namespaces `MimironsGoldOMatic.Shared.{Payouts|Gifts|Versioning}` per AGENTS file-structure rules.
2. Move each one-type file; keep public type names and shapes unchanged.
3. Add **`GlobalUsings.cs`** in consuming projects (and direct `Shared` project reference where tests only referenced Desktop) so existing unqualified type usage keeps compiling.
4. Remove `using MimironsGoldOMatic.Shared;` lines (root namespace no longer used for types).
5. Refresh `src/MimironsGoldOMatic.Shared/README.md`; fix incorrect XML `cref` in `TwGoldChatEnrollmentParser`.
