from __future__ import annotations

import json
import os
import platform
import shutil
import signal
import subprocess
import sys
import time
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Dict, Optional, Tuple

import psutil
import requests


def utc_ts() -> str:
    return time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime())


def ensure_dir(p: Path) -> Path:
    p.mkdir(parents=True, exist_ok=True)
    return p


def read_json(path: Path) -> Dict[str, Any]:
    return json.loads(path.read_text(encoding="utf-8"))


def write_json(path: Path, obj: Any) -> None:
    path.write_text(json.dumps(obj, indent=2), encoding="utf-8")


def is_windows() -> bool:
    return platform.system().lower() == "windows"


@dataclass
class WoWMockProcess:
    proc: subprocess.Popen
    api_base_url: str
    stdout_path: Path
    stderr_path: Path

    def stop(self, timeout_s: float = 10.0) -> None:
        if self.proc.poll() is not None:
            return

        try:
            if is_windows():
                # On Windows, dotnet spawns child processes; kill the tree.
                parent = psutil.Process(self.proc.pid)
                for child in parent.children(recursive=True):
                    child.kill()
                parent.kill()
            else:
                self.proc.send_signal(signal.SIGTERM)
        except Exception:
            pass

        t0 = time.time()
        while time.time() - t0 < timeout_s:
            if self.proc.poll() is not None:
                return
            time.sleep(0.1)

        try:
            self.proc.kill()
        except Exception:
            pass


def wait_for_health(api_base_url: str, timeout_s: float = 30.0) -> Tuple[bool, Optional[Dict[str, Any]]]:
    url = f"{api_base_url}/api/mock/health"
    t0 = time.time()
    last_err: Optional[str] = None
    while time.time() - t0 < timeout_s:
        try:
            r = requests.get(url, timeout=2)
            if r.status_code == 200:
                return True, r.json()
            last_err = f"status={r.status_code} body={r.text[:200]}"
        except Exception as ex:
            last_err = str(ex)
        time.sleep(0.5)
    return False, {"error": last_err} if last_err else None


def start_wowmock(
    repo_root: Path,
    api_port: int,
    log_file_path: Path,
    artifacts_dir: Path,
    configuration: str = "Release",
) -> WoWMockProcess:
    ensure_dir(artifacts_dir)
    stdout_path = artifacts_dir / "wowmock.stdout.log"
    stderr_path = artifacts_dir / "wowmock.stderr.log"

    csproj = repo_root / "src" / "Mocks" / "WoWMock" / "MimironsGoldOMatic.Mocks.WoWMock.csproj"
    if not csproj.exists():
        raise FileNotFoundError(str(csproj))

    # Ensure log directory exists before starting.
    ensure_dir(log_file_path.parent)

    cmd = [
        "dotnet",
        "run",
        "--project",
        str(csproj),
        "-c",
        configuration,
        "--no-build",
        "--",
        f"MockSettings:ApiPort={api_port}",
        f"MockSettings:LogFilePath={str(log_file_path)}",
        "MockSettings:WriteDiagnosticsToFile=true",
        f"MockSettings:DiagnosticsLogPath={str(artifacts_dir / 'WoWMock.log')}",
    ]

    env = os.environ.copy()
    env["ASPNETCORE_ENVIRONMENT"] = "Development"

    with stdout_path.open("wb") as out, stderr_path.open("wb") as err:
        proc = subprocess.Popen(
            cmd,
            cwd=str(repo_root),
            stdout=out,
            stderr=err,
            env=env,
        )

    api_base_url = f"http://127.0.0.1:{api_port}"
    return WoWMockProcess(proc=proc, api_base_url=api_base_url, stdout_path=stdout_path, stderr_path=stderr_path)


def api_post(api_base_url: str, path: str, body: Dict[str, Any], timeout_s: float = 10.0) -> requests.Response:
    url = f"{api_base_url}{path}"
    return requests.post(url, json=body, timeout=timeout_s)


def api_get(api_base_url: str, path: str, timeout_s: float = 10.0) -> requests.Response:
    url = f"{api_base_url}{path}"
    return requests.get(url, timeout=timeout_s)


def tail_contains(path: Path, needle: str, timeout_s: float = 10.0) -> bool:
    t0 = time.time()
    while time.time() - t0 < timeout_s:
        if path.exists():
            try:
                txt = path.read_text(encoding="utf-8", errors="ignore")
                if needle in txt:
                    return True
            except Exception:
                pass
        time.sleep(0.2)
    return False


def copy_tree_best_effort(src: Path, dest: Path) -> None:
    try:
        if dest.exists():
            shutil.rmtree(dest)
        shutil.copytree(src, dest)
    except Exception:
        pass

