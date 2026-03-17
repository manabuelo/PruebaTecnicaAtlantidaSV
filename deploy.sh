#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

if ! command -v docker >/dev/null 2>&1; then
  echo "Error: docker is not installed or not in PATH."
  exit 1
fi

if docker compose version >/dev/null 2>&1; then
  COMPOSE_CMD="docker compose"
elif command -v docker-compose >/dev/null 2>&1; then
  COMPOSE_CMD="docker-compose"
else
  echo "Error: Docker Compose is not available."
  exit 1
fi

echo "Building and deploying containers..."
$COMPOSE_CMD up --build -d

echo "Containers are up. Current status:"
$COMPOSE_CMD ps

echo "Deployment complete."
echo "API: http://localhost:8080"
echo "Swagger: http://localhost:8080/swagger"
echo "PostgreSQL: localhost:5432 (db: mundial_corporativo, user: mundial_user)"
