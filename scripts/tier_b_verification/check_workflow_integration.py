#!/usr/bin/env python3
# <!-- Updated: 2026-04-05 (Tier B integration & first run) -->
"""Probe Tier A + Tier B loopback services for reachability and health JSON."""

from __future__ import annotations

import argparse
import logging
import sys
from typing import Any
from urllib.parse import urljoin

import requests

log = logging.getLogger("check_workflow_integration")

DEFAULTS: dict[str, str] = {
    "backend": "http://127.0.0.1:8080",
    "mock_eventsub": "http://127.0.0.1:9051",
    "mock_extension_jwt": "http://127.0.0.1:9052",
    "mock_helix": "http://127.0.0.1:9053",
    "synthetic_desktop": "http://127.0.0.1:9054",
}


def get_json(url: str, timeout: float) -> dict[str, Any]:
    r = requests.get(url, timeout=timeout)
    r.raise_for_status()
    return r.json()


def check_tier_a_mock(name: str, base_url: str, timeout: float, expected_service: str) -> None:
    url = urljoin(base_url.rstrip("/") + "/", "health")
    log.info("GET %s (%s)", url, name)
    data = get_json(url, timeout)
    if data.get("status") != "ok":
        raise AssertionError(f"{name}: expected status ok, got {data!r}")
    if data.get("service") != expected_service:
        raise AssertionError(f"{name}: expected service {expected_service!r}, got {data!r}")


def check_tier_b_component(name: str, base_url: str, timeout: float, expected_component: str) -> None:
    url = urljoin(base_url.rstrip("/") + "/", "health")
    log.info("GET %s (%s)", url, name)
    data = get_json(url, timeout)
    if data.get("status") != "healthy":
        raise AssertionError(f"{name}: expected status healthy, got {data!r}")
    if data.get("component") != expected_component:
        raise AssertionError(f"{name}: expected component {expected_component!r}, got {data!r}")


def check_backend_root(backend_url: str, timeout: float) -> None:
    base = backend_url.rstrip("/") + "/"
    log.info("GET %s (backend root)", base)
    r = requests.get(base, timeout=timeout)
    if r.status_code >= 500:
        raise AssertionError(f"backend root returned {r.status_code}")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    for key, default in DEFAULTS.items():
        arg = "--" + key.replace("_", "-")
        parser.add_argument(arg, default=default, metavar="URL", help=f"{key} base (default: %(default)s)")
    parser.add_argument("--timeout", type=float, default=10.0, help="HTTP timeout seconds")
    parser.add_argument(
        "--skip-tier-b",
        action="store_true",
        help="Only probe Backend + Tier A mocks (9051, 9052).",
    )
    parser.add_argument("-v", "--verbose", action="store_true")
    args = parser.parse_args()
    logging.basicConfig(
        level=logging.DEBUG if args.verbose else logging.INFO,
        format="%(levelname)s %(message)s",
    )

    try:
        check_backend_root(args.backend, args.timeout)
        check_tier_a_mock("MockEventSubWebhook", args.mock_eventsub, args.timeout, "MockEventSubWebhook")
        check_tier_a_mock("MockExtensionJwt", args.mock_extension_jwt, args.timeout, "MockExtensionJwt")
        if not args.skip_tier_b:
            check_tier_b_component("MockHelixApi", args.mock_helix, args.timeout, "MockHelixApi")
            check_tier_b_component("SyntheticDesktop", args.synthetic_desktop, args.timeout, "SyntheticDesktop")
    except requests.RequestException as e:
        log.error("HTTP error: %s", e)
        return 1
    except AssertionError as e:
        log.error("Check failed: %s", e)
        return 1

    log.info("Workflow integration checks passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
