#!/usr/bin/env python3
# <!-- Updated: 2026-04-05 (Tier B integration & first run) -->
"""After Tier A enrollment: E2E harness → SyntheticDesktop run-sequence → MockHelix + last-run + pool/me (stdlib only)."""
from __future__ import annotations

import argparse
import json
import ssl
import time
import urllib.error
import urllib.parse
import urllib.request
from typing import Any


def _req(
    method: str,
    url: str,
    *,
    data: bytes | None = None,
    headers: dict[str, str] | None = None,
    timeout: float = 120.0,
) -> tuple[int, str]:
    h = dict(headers or {})
    r = urllib.request.Request(url, data=data, method=method, headers=h)
    try:
        with urllib.request.urlopen(r, context=ssl.create_default_context(), timeout=timeout) as resp:
            return resp.status, resp.read().decode()
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode()


def main() -> int:
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("--backend", default="http://127.0.0.1:8080", help="Backend base URL")
    p.add_argument("--api-key", required=True, dest="api_key", help="Mgm API key (X-MGM-ApiKey)")
    p.add_argument("--mock-helix", default="http://127.0.0.1:9053", dest="mock_helix")
    p.add_argument("--synthetic", default="http://127.0.0.1:9054", dest="synthetic")
    p.add_argument("--mock-jwt", default="http://127.0.0.1:9052", dest="mock_jwt")
    p.add_argument("--twitch-user-id", default="e2e-viewer-1", dest="twitch_user_id")
    p.add_argument("--character-name", default="Etoehero", dest="character_name")
    p.add_argument("--timeout", type=float, default=120.0)
    args = p.parse_args()

    backend = args.backend.rstrip("/")
    t0 = time.perf_counter()

    # 1) Development E2E harness: pool member → Pending payout
    prep_url = f"{backend}/api/e2e/prepare-pending-payout"
    prep_body = json.dumps({"twitchUserId": args.twitch_user_id}, separators=(",", ":")).encode()
    code, text = _req(
        "POST",
        prep_url,
        data=prep_body,
        headers={
            "Content-Type": "application/json",
            "X-MGM-ApiKey": args.api_key,
        },
        timeout=args.timeout,
    )
    if code >= 400:
        print("prepare-pending-payout failed:", code, text[:2000])
        return 1
    prep: dict[str, Any] = json.loads(text)
    payout_id = prep.get("payoutId")
    char = prep.get("characterName")
    if not payout_id or not char:
        print("unexpected prepare response:", text[:2000])
        return 1
    print("Tier B: Pending payout", payout_id, "character", char)

    # 2) SyntheticDesktop: confirm → InProgress → Sent (Backend → MockHelix on Sent)
    seq_url = f'{args.synthetic.rstrip("/")}/run-sequence'
    seq_payload = json.dumps(
        {"payoutId": str(payout_id), "characterName": char},
        separators=(",", ":"),
    ).encode()
    code, text = _req(
        "POST",
        seq_url,
        data=seq_payload,
        headers={"Content-Type": "application/json"},
        timeout=args.timeout,
    )
    if code >= 400:
        print("run-sequence failed:", code, text[:2000])
        return 1
    seq_doc: dict[str, Any] = json.loads(text)
    if not seq_doc.get("ok"):
        print("run-sequence ok=false:", json.dumps(seq_doc, indent=2)[:4000])
        return 1
    print("Tier B: SyntheticDesktop run-sequence OK, steps:", len(seq_doc.get("steps") or []))

    # 3) MockHelixApi captured Send Chat Message
    helix_url = f'{args.mock_helix.rstrip("/")}/last-request'
    code, text = _req("GET", helix_url, timeout=args.timeout)
    if code >= 400:
        print("MockHelix last-request failed:", code, text)
        return 1
    helix_last: dict[str, Any] = json.loads(text)
    if not helix_last.get("captured"):
        print("MockHelix last-request not captured:", text[:2000])
        return 1
    body = helix_last.get("body")
    if not isinstance(body, dict):
        print("MockHelix body not object:", body)
        return 1
    msg = body.get("message")
    expect = f"Награда отправлена персонажу {char} на почту, проверяй ящик!"
    if msg != expect:
        print("MockHelix message mismatch.\nExpected:", expect, "\nGot:", msg)
        return 1
    print("Tier B: MockHelix message OK")

    # 4) SyntheticDesktop last-run audit
    lr_url = f'{args.synthetic.rstrip("/")}/last-run'
    code, text = _req("GET", lr_url, timeout=args.timeout)
    if code >= 400:
        print("Synthetic last-run failed:", code, text)
        return 1
    last_run: dict[str, Any] = json.loads(text)
    if not last_run.get("ok"):
        print("Synthetic last-run not ok:", text[:2000])
        return 1
    print("Tier B: SyntheticDesktop last-run OK")

    # 5) Pool: winner removed after Sent
    token_url = f'{args.mock_jwt.rstrip("/")}/token?userId={urllib.parse.quote(args.twitch_user_id)}&displayName=E2EViewer'
    code, tok_text = _req("GET", token_url, timeout=args.timeout)
    if code >= 400:
        print("MockExtensionJwt token failed:", code, tok_text)
        return 1
    token_doc = json.loads(tok_text)
    token = token_doc.get("access_token")
    if not token:
        print("no access_token in JWT response")
        return 1
    pool_url = f"{backend}/api/pool/me"
    code, pool_text = _req(
        "GET",
        pool_url,
        headers={"Authorization": f"Bearer {token}"},
        timeout=args.timeout,
    )
    if code >= 400:
        print("GET /api/pool/me failed:", code, pool_text)
        return 1
    pool_doc: dict[str, Any] = json.loads(pool_text)
    if pool_doc.get("isEnrolled") is not False:
        print("Expected isEnrolled false after Sent, got:", pool_text)
        return 1
    elapsed = time.perf_counter() - t0
    print(f"E2E Tier B: all checks passed in {elapsed:.2f}s wall time (orchestrator only).")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
