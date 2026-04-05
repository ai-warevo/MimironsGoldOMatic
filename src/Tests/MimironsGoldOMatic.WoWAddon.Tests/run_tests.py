#!/usr/bin/env python3
"""Run LuaUnit tests for MimironsGoldOMatic.WoWAddon (requires lua on PATH)."""
import os
import shutil
import subprocess
import sys


def main() -> int:
    os.chdir(os.path.dirname(os.path.abspath(__file__)))
    lua = shutil.which("lua")
    if not lua:
        print(
            "lua not found on PATH. Install Lua (e.g. choco install lua on Windows, "
            "apt install lua5.4 on Linux) or add lua to PATH.",
            file=sys.stderr,
        )
        return 1
    cmd = [lua, "run_tests.lua", *sys.argv[1:]]
    return subprocess.run(cmd, check=False).returncode


if __name__ == "__main__":
    raise SystemExit(main())
