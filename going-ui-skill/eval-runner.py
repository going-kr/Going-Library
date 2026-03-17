#!/usr/bin/env python3
"""Going UI Skill — Eval Runner

evals.json의 테스트 케이스를 claude CLI로 실행하고
must_contain / must_not_contain 키워드 검사 결과를 출력한다.

Usage:
    python eval-runner.py                       # 전체 실행
    python eval-runner.py --index 0 2 5         # 특정 인덱스만
    python eval-runner.py --name "GoButton"     # 이름 부분 일치
    python eval-runner.py --dry-run             # 프롬프트만 확인
    python eval-runner.py --output results.json # 결과 JSON 저장
    python eval-runner.py --parallel 3          # 동시 3개 실행
"""

import argparse
import json
import subprocess
import sys
import time
import os
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor, as_completed

SCRIPT_DIR = Path(__file__).parent
EVALS_PATH = SCRIPT_DIR / "evals.json"

# claude -p 에 전달할 시스템 프롬프트 (스킬 로딩 지시)
SYSTEM_SUFFIX = (
    "You are evaluating the Going UI skill. "
    "Answer the user's prompt using the going-ui skill knowledge. "
    "Respond in the same language as the prompt."
)


def load_evals(path: Path) -> list[dict]:
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def filter_evals(evals: list[dict], indices: list[int] | None, name_filter: str | None) -> list[tuple[int, dict]]:
    selected = list(enumerate(evals))
    if indices is not None:
        selected = [(i, e) for i, e in selected if i in indices]
    if name_filter:
        name_lower = name_filter.lower()
        selected = [(i, e) for i, e in selected if name_lower in e["name"].lower()]
    return selected


def run_claude(prompt: str, timeout: int = 120) -> tuple[str, float]:
    """claude -p 로 프롬프트를 실행하고 (응답, 소요시간) 반환."""
    full_prompt = f"{SYSTEM_SUFFIX}\n\n---\n\n{prompt}"
    start = time.time()
    result = subprocess.run(
        ["claude", "-p", full_prompt, "--no-input"],
        capture_output=True,
        text=True,
        timeout=timeout,
        cwd=str(SCRIPT_DIR),
    )
    elapsed = time.time() - start
    output = result.stdout.strip()
    if result.returncode != 0 and not output:
        output = f"[ERROR] returncode={result.returncode}\nstderr: {result.stderr.strip()}"
    return output, elapsed


def check_response(response: str, eval_case: dict) -> dict:
    """must_contain / must_not_contain 검사. 결과 dict 반환."""
    response_lower = response.lower()

    passed_contains = []
    failed_contains = []
    for kw in eval_case.get("must_contain", []):
        if kw.lower() in response_lower:
            passed_contains.append(kw)
        else:
            failed_contains.append(kw)

    passed_not_contains = []
    failed_not_contains = []
    for kw in eval_case.get("must_not_contain", []):
        if kw.lower() in response_lower:
            failed_not_contains.append(kw)
        else:
            passed_not_contains.append(kw)

    total_checks = len(eval_case.get("must_contain", [])) + len(eval_case.get("must_not_contain", []))
    passed_checks = len(passed_contains) + len(passed_not_contains)
    all_passed = not failed_contains and not failed_not_contains

    return {
        "pass": all_passed,
        "score": passed_checks / total_checks if total_checks > 0 else 1.0,
        "must_contain": {"passed": passed_contains, "failed": failed_contains},
        "must_not_contain": {"passed": passed_not_contains, "failed": failed_not_contains},
    }


def run_single_eval(index: int, eval_case: dict, timeout: int) -> dict:
    """단일 eval 실행 + 검사. 결과 dict 반환."""
    try:
        response, elapsed = run_claude(eval_case["prompt"], timeout=timeout)
    except subprocess.TimeoutExpired:
        response, elapsed = "[TIMEOUT]", timeout
    except FileNotFoundError:
        print("ERROR: 'claude' CLI not found. Install Claude Code first.", file=sys.stderr)
        sys.exit(1)

    check = check_response(response, eval_case)
    return {
        "index": index,
        "name": eval_case["name"],
        "pass": check["pass"],
        "score": check["score"],
        "details": check,
        "elapsed_sec": round(elapsed, 1),
        "response_length": len(response),
        "response": response,
    }


def print_result_line(r: dict):
    status = "✅ PASS" if r["pass"] else "❌ FAIL"
    score_pct = int(r["score"] * 100)
    print(f"  [{r['index']:2d}] {status} ({score_pct:3d}%) {r['name']}  ({r['elapsed_sec']}s)")
    if not r["pass"]:
        if r["details"]["must_contain"]["failed"]:
            print(f"       missing: {r['details']['must_contain']['failed']}")
        if r["details"]["must_not_contain"]["failed"]:
            print(f"       unwanted: {r['details']['must_not_contain']['failed']}")


def main():
    parser = argparse.ArgumentParser(description="Going UI Skill Eval Runner")
    parser.add_argument("--index", type=int, nargs="+", help="실행할 eval 인덱스 (0-based)")
    parser.add_argument("--name", type=str, help="이름 부분 일치 필터")
    parser.add_argument("--output", "-o", type=str, help="결과 JSON 저장 경로")
    parser.add_argument("--dry-run", action="store_true", help="프롬프트만 출력, 실행 안 함")
    parser.add_argument("--timeout", type=int, default=120, help="케이스당 타임아웃 (초, 기본 120)")
    parser.add_argument("--parallel", type=int, default=1, help="동시 실행 수 (기본 1)")
    parser.add_argument("--no-response", action="store_true", help="결과 JSON에서 response 본문 제외")
    args = parser.parse_args()

    evals = load_evals(EVALS_PATH)
    selected = filter_evals(evals, args.index, args.name)

    if not selected:
        print("No matching eval cases found.")
        return

    print(f"Going UI Skill Eval Runner")
    print(f"{'=' * 60}")
    print(f"Total cases: {len(evals)} | Selected: {len(selected)} | Parallel: {args.parallel}")
    print(f"{'=' * 60}")

    if args.dry_run:
        for i, case in selected:
            print(f"\n[{i:2d}] {case['name']}")
            print(f"     prompt: {case['prompt'][:100]}...")
            print(f"     must_contain: {case.get('must_contain', [])}")
            print(f"     must_not_contain: {case.get('must_not_contain', [])}")
        print(f"\n(dry-run: {len(selected)} cases listed, nothing executed)")
        return

    results = []
    total_start = time.time()

    if args.parallel <= 1:
        for i, case in selected:
            print(f"\n▶ Running [{i}] {case['name']}...")
            r = run_single_eval(i, case, args.timeout)
            results.append(r)
            print_result_line(r)
    else:
        with ThreadPoolExecutor(max_workers=args.parallel) as pool:
            futures = {pool.submit(run_single_eval, i, case, args.timeout): (i, case) for i, case in selected}
            for future in as_completed(futures):
                r = future.result()
                results.append(r)
                print_result_line(r)
        results.sort(key=lambda x: x["index"])

    total_elapsed = time.time() - total_start

    # Summary
    passed = sum(1 for r in results if r["pass"])
    failed = len(results) - passed
    avg_score = sum(r["score"] for r in results) / len(results) if results else 0

    print(f"\n{'=' * 60}")
    print(f"Results: {passed} passed, {failed} failed / {len(results)} total")
    print(f"Average score: {avg_score:.1%}")
    print(f"Total time: {total_elapsed:.1f}s")
    print(f"{'=' * 60}")

    if failed > 0:
        print(f"\nFailed cases:")
        for r in results:
            if not r["pass"]:
                print_result_line(r)

    # Save JSON
    if args.output:
        output_data = {
            "timestamp": time.strftime("%Y-%m-%dT%H:%M:%S"),
            "total": len(results),
            "passed": passed,
            "failed": failed,
            "average_score": round(avg_score, 4),
            "elapsed_sec": round(total_elapsed, 1),
            "results": results if not args.no_response else [
                {k: v for k, v in r.items() if k != "response"} for r in results
            ],
        }
        out_path = Path(args.output)
        with open(out_path, "w", encoding="utf-8") as f:
            json.dump(output_data, f, ensure_ascii=False, indent=2)
        print(f"\nResults saved to: {out_path}")


if __name__ == "__main__":
    main()
