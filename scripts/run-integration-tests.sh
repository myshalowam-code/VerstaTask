#!/usr/bin/env bash
set -Eeuo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repository_root="$(cd "$script_dir/.." && pwd)"
compose_file="$repository_root/docker-compose.integration.yml"
project_name="${VERSTA_INTEGRATION_PROJECT:-versta-integration-$$}"
results_dir="${VERSTA_INTEGRATION_RESULTS_DIR:-$repository_root/TestResults/integration}"
test_container="$project_name-tests"

cleanup() {
  exit_code=$?
  trap - EXIT INT TERM
  if (( exit_code != 0 )); then
    docker compose \
      --project-name "$project_name" \
      --file "$compose_file" \
      logs --no-color --tail 200 \
      auth-migrations auth-api orders-migrations orders-api orders-projection gateway || true
  fi
  docker compose \
    --project-name "$project_name" \
    --file "$compose_file" \
    down --volumes --remove-orphans --rmi local --timeout 10
  docker rm --force "$test_container" >/dev/null 2>&1 || true
  exit "$exit_code"
}

trap cleanup EXIT INT TERM

docker compose \
  --project-name "$project_name" \
  --file "$compose_file" \
  up --detach --build gateway orders-projection

set +e
docker compose \
  --project-name "$project_name" \
  --file "$compose_file" \
  run --build --no-deps --name "$test_container" integration-tests
test_exit_code=$?
set -e

mkdir -p "$results_dir"
docker cp "$test_container:/test-results/integration-tests.xml" "$results_dir/integration-tests.xml" || true
docker rm "$test_container" >/dev/null 2>&1 || true

exit "$test_exit_code"
