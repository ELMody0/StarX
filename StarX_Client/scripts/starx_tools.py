#!/usr/bin/env python3
"""StarX Python engine — scripting/automation layer (stdlib only).

Protocol: every command prints exactly one JSON object to stdout.
  {"ok": true, ...} on success
  {"ok": false, "error": "..."} on failure (exit code != 0)

Commands:
  version            engine + interpreter info
  add <a> <b>        integer addition (cross-checked with the C++ engine)
  analyze <text>     text statistics
"""
import json
import sys


ENGINE_VERSION = "1.0.0"


def out(payload, code=0):
    print(json.dumps(payload, ensure_ascii=False))
    raise SystemExit(code)


def cmd_version(_args):
    out({
        "ok": True,
        "engine": "starx-python",
        "version": ENGINE_VERSION,
        "python": sys.version.split()[0],
    })


def cmd_add(args):
    if len(args) != 2:
        out({"ok": False, "error": "usage: add <a> <b>"}, code=2)
    try:
        a, b = int(args[0]), int(args[1])
    except ValueError:
        out({"ok": False, "error": "add expects two integers"}, code=2)
    out({"ok": True, "a": a, "b": b, "result": a + b})


def cmd_analyze(args):
    text = " ".join(args)
    words = text.split()
    out({
        "ok": True,
        "chars": len(text),
        "words": len(words),
        "lines": text.count("\n") + (1 if text else 0),
    })


COMMANDS = {
    "version": cmd_version,
    "add": cmd_add,
    "analyze": cmd_analyze,
}


def main(argv):
    if len(argv) < 2 or argv[1] not in COMMANDS:
        out({"ok": False,
             "error": "usage: starx_tools.py {version|add|analyze} [...args]"}, code=2)
    COMMANDS[argv[1]](argv[2:])


if __name__ == "__main__":
    main(sys.argv)
