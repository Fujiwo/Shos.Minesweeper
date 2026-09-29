#!/bin/sh
# テストを流し、そのときのファイルの一覧と結果を test-log.txt に追記する
cd "$(dirname "$0")"
n=$(grep -c '^=== run' test-log.txt 2>/dev/null || echo 0)
n=$((n + 1))
out=$(python3 -m unittest discover -p 'test_*.py' 2>&1)
status=$?
{
  echo "=== run $n (exit $status)"
  echo "files: $(ls *.py 2>/dev/null | tr '\n' ' ')"
  echo "$out" | tail -n 3
} >> test-log.txt
echo "$out"
exit $status
