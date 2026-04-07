from __future__ import annotations

import os
import sys
from pathlib import Path

import pytest

from lib import ensure_dir, start_wowmock, wait_for_health


def main() -> int:
    repo_root = Path(__file__).resolve().parents[3]
    artifacts_dir = Path(os.environ.get("WOWMOCK_E2E_ARTIFACTS", ".e2e-artifacts")).resolve()
    ensure_dir(artifacts_dir)
    log_dir = Path(os.environ.get("WOWMOCK_E2E_LOG_DIR", str(artifacts_dir / "Logs"))).resolve()
    ensure_dir(log_dir)

    port = int(os.environ.get("WOWMOCK_E2E_PORT", "5001"))
    api_base_url = f"http://127.0.0.1:{port}"
    chat_log_path = log_dir / "WoWChatLog.txt"

    # Ensure the WoWMock test env points at the same locations.
    os.environ["WOWMOCK_E2E_URL"] = api_base_url
    os.environ["WOWMOCK_E2E_ARTIFACTS"] = str(artifacts_dir)

    # Build in CI before this script; locally it is ok to run Debug.
    configuration = os.environ.get("WOWMOCK_E2E_CONFIGURATION", "Release")

    wowmock = start_wowmock(
        repo_root=repo_root,
        api_port=port,
        log_file_path=chat_log_path,
        artifacts_dir=artifacts_dir,
        configuration=configuration,
    )

    try:
        ok, info = wait_for_health(api_base_url, timeout_s=45)
        if not ok:
            print(f"WoWMock failed health check: {info}", file=sys.stderr)
            return 2

        junit = artifacts_dir / "pytest-junit.xml"
        args = [
            str(Path(__file__).parent),
            "-q",
            f"--junitxml={junit}",
            "--timeout=60",
        ]
        return int(pytest.main(args))
    finally:
        wowmock.stop()


if __name__ == "__main__":
    raise SystemExit(main())

