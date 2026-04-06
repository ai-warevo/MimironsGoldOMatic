from __future__ import annotations

import json
import time
from pathlib import Path

import pytest

from lib import api_get, api_post, tail_contains


@pytest.mark.timeout(60)
def test_health(api_base_url: str) -> None:
    r = api_get(api_base_url, "/api/mock/health")
    assert r.status_code == 200
    d = r.json()
    assert d.get("ok") is True


@pytest.mark.timeout(60)
def test_reset_creates_log(api_base_url: str, chat_log_path: Path) -> None:
    r = api_post(api_base_url, "/api/mock/reset", {})
    assert r.status_code == 200
    assert chat_log_path.exists()


@pytest.mark.timeout(60)
def test_add_message_appends_to_log(api_base_url: str, chat_log_path: Path) -> None:
    api_post(api_base_url, "/api/mock/reset", {})
    msg = "[MGM_WHO] " + json.dumps({"candidates": ["Etoehero"]})
    r = api_post(api_base_url, "/api/mock/add-message", {"Content": msg})
    assert r.status_code == 200
    assert tail_contains(chat_log_path, msg, timeout_s=10)


@pytest.mark.timeout(60)
def test_run_command_is_captured(api_base_url: str) -> None:
    api_post(api_base_url, "/api/mock/reset", {})
    cmd = "/run NotifyWinnerWhisper('abc','Etoehero')"
    r = api_post(api_base_url, "/api/mock/add-message", {"Content": cmd})
    assert r.status_code == 200

    r2 = api_get(api_base_url, "/api/mock/commands")
    assert r2.status_code == 200
    cmds = r2.json()
    assert any(c.get("command") == cmd or c.get("Command") == cmd for c in cmds), cmds


@pytest.mark.timeout(60)
def test_auto_confirm_after_accept(api_base_url: str, chat_log_path: Path) -> None:
    api_post(api_base_url, "/api/mock/reset", {})
    api_post(
        api_base_url,
        "/api/mock/set-response",
        {"AutoConfirmAccepts": True, "EchoRunCommandsToChatLog": False, "CommandProcessingDelayMs": 150},
    )
    accept = "[MGM_ACCEPT:11111111-2222-3333-4444-555555555555]"
    api_post(api_base_url, "/api/mock/add-message", {"Content": accept})
    assert tail_contains(chat_log_path, "[MGM_CONFIRM:11111111-2222-3333-4444-555555555555]", timeout_s=10)


@pytest.mark.timeout(60)
def test_malformed_messages_do_not_crash(api_base_url: str, chat_log_path: Path) -> None:
    api_post(api_base_url, "/api/mock/reset", {})
    bad = "[MGM_ACCEPT:not-a-guid]"
    api_post(api_base_url, "/api/mock/set-response", {"AutoConfirmAccepts": True, "EchoRunCommandsToChatLog": False, "CommandProcessingDelayMs": 1})
    r = api_post(api_base_url, "/api/mock/add-message", {"Content": bad})
    assert r.status_code == 200

    # service should remain healthy
    r2 = api_get(api_base_url, "/api/mock/health")
    assert r2.status_code == 200

    # and should have still appended the original message
    assert tail_contains(chat_log_path, bad, timeout_s=5)

