#!/usr/bin/env python3
# <!-- Created: 2026-04-05 (Tier A E2E mocks) -->
"""POST a synthetic Twitch channel.chat.message EventSub notification to MockEventSubWebhook /api/twitch/eventsub with Twitch HMAC headers."""
from __future__ import annotations

import argparse
import hashlib
import hmac
import json
import ssl
import time
import urllib.error
import urllib.request
import uuid


def build_body(message_id: str, user_id: str, login: str, text: str) -> str:
    doc = {
        "subscription": {"type": "channel.chat.message"},
        "event": {
            "message_id": message_id,
            "chatter_user_id": user_id,
            "chatter_user_login": login,
            "message": {"text": text},
            "badges": [{"set_id": "subscriber"}],
        },
    }
    return json.dumps(doc, separators=(",", ":"))


def sign(secret: str, msg_id: str, ts: str, body: str) -> str:
    payload = (msg_id + ts + body).encode("utf-8")
    dig = hmac.new(secret.encode("utf-8"), payload, hashlib.sha256).hexdigest()
    return "sha256=" + dig


def main() -> int:
    p = argparse.ArgumentParser()
    p.add_argument("--url", required=True, help="MockEventSubWebhook base URL")
    p.add_argument("--secret", default="", help="EventSub secret (empty = EBS/mocks skip HMAC)")
    p.add_argument("--user-id", default="e2e-viewer-1", dest="user_id")
    p.add_argument("--login", default="e2eviewer1")
    p.add_argument("--text", default="!twgold Etoehero")
    args = p.parse_args()

    msg_id = str(uuid.uuid4())
    ts = str(int(time.time()))
    body = build_body(msg_id, args.user_id, args.login, args.text)
    sig = sign(args.secret, msg_id, ts, body) if args.secret else "sha256=unused"

    url = args.url.rstrip("/") + "/api/twitch/eventsub"
    req = urllib.request.Request(url, data=body.encode("utf-8"), method="POST")
    req.add_header("Content-Type", "application/json")
    req.add_header("Twitch-Eventsub-Message-Id", msg_id)
    req.add_header("Twitch-Eventsub-Message-Timestamp", ts)
    req.add_header("Twitch-Eventsub-Message-Signature", sig)

    try:
        with urllib.request.urlopen(req, context=ssl.create_default_context(), timeout=120) as resp:
            out = resp.read().decode()
            if out:
                print(out)
            print("send_e2e_eventsub: HTTP", resp.status)
            return 0 if resp.status < 400 else 1
    except urllib.error.HTTPError as e:
        err = e.read().decode()
        print(err)
        print("send_e2e_eventsub: HTTP", e.code)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
