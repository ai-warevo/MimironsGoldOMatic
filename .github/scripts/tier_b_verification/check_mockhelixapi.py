#!/usr/bin/env python3
"""Verify MockHelixApi health, POST /helix/chat/messages, and GET /last-request."""

from __future__ import annotations

import argparse
import json
import logging
import sys
from typing import Any

import requests

log = logging.getLogger("check_mockhelixapi")


def _expect(condition: bool, msg: str) -> None:
    if not condition:
        raise AssertionError(msg)


def run(base_url: str, timeout: float) -> None:
    base = base_url.rstrip("/")
    session = requests.Session()

    log.info("GET %s/health", base)
    r = session.get(f"{base}/health", timeout=timeout)
    r.raise_for_status()
    health: dict[str, Any] = r.json()
    _expect(health.get("status") == "healthy", f"health.status expected 'healthy', got {health!r}")
    _expect(health.get("component") == "MockHelixApi", f"health.component expected MockHelixApi, got {health!r}")

    payload = {
        "broadcaster_id": "bcast-test",
        "sender_id": "bcast-test",
        "message": "Награда отправлена персонажу Etoehero на почту, проверяй ящик!",
    }
    headers = {
        "Authorization": "Bearer e2e-test-token",
        "Client-Id": "e2e-client-id",
        "Content-Type": "application/json",
    }
    log.info("POST %s/helix/chat/messages", base)
    pr = session.post(
        f"{base}/helix/chat/messages",
        data=json.dumps(payload, separators=(",", ":")),
        headers=headers,
        timeout=timeout,
    )
    _expect(pr.status_code in (200, 204), f"POST helix expected 200/204, got {pr.status_code} {pr.text!r}")

    log.info("GET %s/last-request", base)
    lr = session.get(f"{base}/last-request", timeout=timeout)
    lr.raise_for_status()
    last: dict[str, Any] = lr.json()
    _expect(last.get("captured") is True, f"last-request should be captured, got {last!r}")
    body = last.get("body")
    _expect(isinstance(body, dict), f"last body should be object, got {body!r}")
    _expect(body.get("message") == payload["message"], f"message mismatch: {body!r}")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--base-url",
        default="http://127.0.0.1:9053",
        help="MockHelixApi root URL (default: %(default)s)",
    )
    parser.add_argument(
        "--timeout",
        type=float,
        default=10.0,
        help="HTTP timeout seconds (default: %(default)s)",
    )
    parser.add_argument("-v", "--verbose", action="store_true")
    args = parser.parse_args()
    logging.basicConfig(
        level=logging.DEBUG if args.verbose else logging.INFO,
        format="%(levelname)s %(message)s",
    )
    try:
        run(args.base_url, args.timeout)
    except requests.RequestException as e:
        log.error("HTTP error: %s", e)
        return 1
    except AssertionError as e:
        log.error("Check failed: %s", e)
        return 1
    log.info("MockHelixApi checks passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
