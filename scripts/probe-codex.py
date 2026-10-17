"""Read-only protocol smoke test; never prints account identities or credentials."""
import json
import queue
import shutil
import subprocess
import threading


def main():
    executable = shutil.which("codex.exe")
    if not executable:
        raise RuntimeError("codex.exe was not found in PATH")
    process = subprocess.Popen([executable, "app-server", "--listen", "stdio://"],
                               stdin=subprocess.PIPE, stdout=subprocess.PIPE,
                               stderr=subprocess.DEVNULL, text=True, encoding="utf-8",
                               creationflags=subprocess.CREATE_NO_WINDOW)
    messages = queue.Queue()

    def read():
        for line in process.stdout:
            messages.put(line)
        messages.put(None)

    threading.Thread(target=read, daemon=True).start()

    def send(message):
        process.stdin.write(json.dumps(message) + "\n")
        process.stdin.flush()

    def receive(request_id):
        import time
        deadline = time.monotonic() + 30
        while True:
            line = messages.get(timeout=max(0.01, deadline - time.monotonic()))
            if line is None:
                raise RuntimeError("Codex closed the connection")
            message = json.loads(line)
            if "method" in message and "id" in message:
                send({"id": message["id"], "error": {"code": -32601, "message": "Unsupported client request"}})
            elif message.get("id") == request_id:
                if "error" in message:
                    raise RuntimeError(f"Request rejected; RPC code: {message['error'].get('code')}")
                return message["result"]

    try:
        send({"id": 1, "method": "initialize", "params": {"clientInfo": {
            "name": "codex_token_monitor", "title": "Codex Token Monitor", "version": "1.0.1"}}})
        receive(1)
        print("initialize: OK")
        send({"method": "initialized"})
        send({"id": 2, "method": "account/rateLimits/read"})
        result = receive(2)
        buckets = result.get("rateLimitsByLimitId") or {"legacy": result.get("rateLimits")}
        count = 0
        for bucket in buckets.values():
            for slot in ("primary", "secondary"):
                window = (bucket or {}).get(slot)
                if window and isinstance(window.get("usedPercent"), (int, float)):
                    print(json.dumps({"durationMinutes": window.get("windowDurationMins"),
                                      "usedPercent": window["usedPercent"],
                                      "remainingPercent": max(0, min(100, 100-window["usedPercent"])),
                                      "resetsAt": window.get("resetsAt")}))
                    count += 1
        print(f"account/rateLimits/read: OK; valid windows: {count}")
    finally:
        process.stdin.close()
        try:
            process.wait(timeout=2)
        except subprocess.TimeoutExpired:
            process.kill()
            process.wait()
        process.stdout.close()


if __name__ == "__main__":
    main()
