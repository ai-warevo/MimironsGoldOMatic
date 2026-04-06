## Plan

1. Add Desktop update-check abstractions and implementation:
   - `IAppVersionProvider` (assembly-based)
   - `IUpdateService` + `UpdateService` with semantic version comparison and graceful failure result.
2. Extend Desktop EBS API client to call `GET /api/version` using shared `VersionInfoDto`.
3. Integrate into Desktop UI:
   - New update state properties in `MainViewModel`
   - Startup async check + user command for manual check
   - Release notes command and status bar/menu bindings in `MainWindow.xaml`.
4. Extend WoW tail parsing with additive `[MGM_UPDATE_CHECK]` support:
   - Trigger update check
   - Build RU user-facing message
   - Inject via existing WoW command strategy when available.
5. Add/extend unit tests for:
   - Version comparison and error paths
   - `GET /api/version` client behavior
   - WoW log update-check tag behavior.
