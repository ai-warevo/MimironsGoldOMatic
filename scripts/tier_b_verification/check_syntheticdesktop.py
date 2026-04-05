#!/usr/bin/env python3
"""Verify SyntheticDesktop health and optionally POST /run-sequence against Backend."""

from __future__ import annotations

import argparse
import logging
import sys
from typing import Any

import requests

log = logging.getLogger("check_syntheticdesktop")


def _expect(condition: bool, msg: str) -> None:
    if not condition:
        raise AssertionError(msg)


def check_health(base_url: str, timeout: float) -> None:
    base = base_url.rstrip("/")
    log.info("GET %s/health", base)
    r = requests.get(f"{base}/health", timeout=timeout)
    r.raise_for_status()
    data: dict[str, Any] = r.json()
    _expect(data.get("status") == "healthy", f"health.status expected 'healthy', got {data!r}")
    _expect(data.get("component") == "SyntheticDesktop", f"health.component mismatch: {data!r}")


def check_run_sequence(base_url: str, payout_id: str, character_name: str, timeout: float) -> None:
    base = base_url.rstrip("/")
    payload = {"payoutId": payout_id, "characterName": character_name}
    log.info("POST %s/run-sequence payoutId=%s", base, payout_id)
    r = requests.post(f"{base}/run-sequence", json=payload, timeout=timeout)
    try:
        body: dict[str, Any] = r.json()
    except ValueError:
        log.error("Non-JSON response %s: %s", r.status_code, r.text[:500])
        raise
    if not r.ok:
        log.error("run-sequence failed: %s %s", r.status_code, body)
        raise AssertionError(f"run-sequence HTTP {r.status_code}")
    _expect(body.get("ok") is True, f"last-run ok=false: {body!r}")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--base-url",
        default="http://127.0.0.1:9054",
        help="SyntheticDesktop root URL (default: %(default)s)",
    )
    parser.add_argument(
        "--timeout",
        type=float,
        default=60.0,
        help="HTTP timeout seconds (default: %(default)s)",
    )
    parser.add_argument(
        "--payout-id",
        help="If set, POST /run-sequence with this payout GUID (Backend must have Pending payout).",
    )
    parser.add_argument(
        "--character-name",
        default="Etoehero",
        help="Character name for confirm-acceptance (default: %(default)s)",
    )
    parser.add_argument("-v", "--verbose", action="store_true")
    args = parser.parse_args()
    logging.basicConfig(
        level=logging.DEBUG if args.verbose else logging.INFO,
        format="%(levelname)s %(message)s",
    )
    try:
        check_health(args.base_url, args.timeout)
        if args.payout_id:
            check_run_sequence(args.base_url, args.payout_id, args.character_name, args.timeout)
        else:
            log.info("Skipping run-sequence (pass --payout-id for full Desktop API choreography).")
    except requests.RequestException as e:
        log.error("HTTP error: %s", e)
        return 1
    except AssertionError as e:
        log.error("Check failed: %s", e)
        return 1
    log.info("SyntheticDesktop checks passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
