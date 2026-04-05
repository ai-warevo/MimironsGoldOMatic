# User request

Fix GitHub Actions backend job failure on `ubuntu-latest`:

`NETSDK1100: To build a project targeting Windows on this operating system, set the EnableWindowsTargeting property to true` during `dotnet restore` on full solution — caused by `MimironsGoldOMatic.Desktop` (`net10.0-windows` / WPF) in `MimironsGoldOMatic.slnx`.

Reference: `.github/workflows/unit-integration-tests.yml` backend step (restore + test).
