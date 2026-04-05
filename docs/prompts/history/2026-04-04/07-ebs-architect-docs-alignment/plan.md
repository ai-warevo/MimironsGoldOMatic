# Plan

## Goal

Encode architect-approved MVP behavior in **normative docs** so implementation cannot drift: **EBS** naming, **Helix §11** inline delivery (no Outbox), **EventSub-only** subscriber checks for chat enrollment, **once-per-payout** chat announcement, **roadmap mandatory checklist**, and explicit **non-implementation** of placeholder scenarios (e.g. SC-022 retry tokens).

## Files to update

- `docs/overview/SPEC.md` — EBS section extensions, §5/§6/§11, glossary, Outbox paragraph.
- `docs/overview/ROADMAP.md` — EBS terminology, MVP-0 `slnx`, MVP-2 Helix bullet, mandatory checklist section.
- `CONTEXT.md`, `AGENTS.md`, `docs/overview/INTERACTION_SCENARIOS.md`, `docs/reference/IMPLEMENTATION_READINESS.md`, `docs/ReadME.md`.
- `docs/MimironsGoldOMatic.*/ReadME.md` — EBS alignment.

## Risks

- Contradiction between “no Outbox” and earlier “Outbox when first external effect” — resolved by stating Helix is **inline** and Outbox is **post-MVP** for other channels.
- `.sln` vs `.slnx` — document `dotnet` commands with **`MimironsGoldOMatic.slnx`** for .NET 10.

## Out of scope

- Implementing EBS code (MVP-2); only doc + solution scaffold as noted in session.
