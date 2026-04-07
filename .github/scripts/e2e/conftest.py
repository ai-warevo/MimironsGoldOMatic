from __future__ import annotations

import os
from pathlib import Path

import pytest


@pytest.fixture(scope="session")
def artifacts_dir() -> Path:
    p = Path(os.environ.get("WOWMOCK_E2E_ARTIFACTS", ".e2e-artifacts")).resolve()
    p.mkdir(parents=True, exist_ok=True)
    return p


@pytest.fixture(scope="session")
def api_base_url() -> str:
    return os.environ.get("WOWMOCK_E2E_URL", "http://127.0.0.1:5001")


@pytest.fixture(scope="session")
def chat_log_path(artifacts_dir: Path) -> Path:
    # Tests run against a log under artifacts to simplify upload.
    return artifacts_dir / "Logs" / "WoWChatLog.txt"

