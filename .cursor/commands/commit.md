---
description: Generates a conventional git commit command based on all local changes (staged + unstaged) with custom footer
---

# Comprehensive Conventional Commit Generator

1. **Analyze Changes**: Run `git diff HEAD` to see all current changes (staged and unstaged).
2. **Standard**: Use Conventional Commits: `<type>(<scope>): <description>`.
3. **Types**:
    - **feat**: A new feature (e.g., a new MediatR command or UI component).
    - **fix**: A bug fix (e.g., fixing WinAPI injection or Lua parsing).
    - **docs**: Documentation only changes (updates in `/docs` or READMEs).
    - **refactor**: Code change that neither fixes a bug nor adds a feature (e.g., moving to Primary Constructors).
    - **style**: Changes that do not affect the meaning of the code (white-space, formatting).
    - **test**: Adding missing tests or correcting existing tests.
    - **chore**: Updating build tasks, package manager configs, or GitHub Actions.
4. **Scopes**:
    - **shared**: Logic in `MimironsGoldOMatic.Shared` (DTOs, Enums, FluentValidation).
    - **backend**: Web API logic, Marten Event Store, or PostgreSQL configurations.
    - **desktop**: WPF UI, MVVM logic, or HttpClient polling.
    - **addon**: WoW Lua code, FrameXML, or Event Dispatcher logic.
    - **extension**: React/Vite frontend or Twitch Helper integration.
    - **winapi**: Specific `PostMessage`, `SetForegroundWindow`, or process finding logic.
    - **domain**: DDD Aggregate logic, Specifications, or Domain Events.
    - **infra**: Database migrations, Outbox worker, or DI setup.
    - **ci**: GitHub Workflows or deployment scripts.
5. **Examples**:
    - `feat(shared): add PayoutStatus.Expired to domain contracts`
    - `fix(addon): resolve MAIL_SHOW event race condition in 3.3.5a`
    - `feat(backend): implement Marten projections for Payout read-model`
    - `refactor(desktop): extract PostMessage logic into IWoWInputStrategy`
    - `fix(winapi): adjust 30ms delay between keypresses for high-latency clients`
    - `docs(readme): update project description and Boosty goal`
    - `feat(extension): implement Zustand store for claim status persistence`
6. **Rules**:
   - **Language**: English only.
   - **Mood**: Imperative (e.g., "add").
   - **Footer**: Every commit message MUST end with these two lines separated by a blank line from the description:
     
     Made-with: Cursor
     Co-authored-by: Cursor Agent <cursoragent@cursor.com>
7. **Multi-commit Logic**: If changes are too large or cover unrelated topics, suggest splitting them into multiple commands.
8. **Workflow**:
   - Generate and run `git add .` first.
   - Then output the command: `git commit -m "<type>(<scope>): <description>
   
Made-with: Cursor

Co-Authored-By: Cursor Agent <cursoragent@cursor.com>"`
