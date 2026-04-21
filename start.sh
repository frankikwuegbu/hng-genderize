#!/usr/bin/env sh
set -eu

if [ -n "${PORT:-}" ] && [ -z "${ASPNETCORE_URLS:-}" ] && [ -z "${ASPNETCORE_HTTP_PORTS:-}" ]; then
  export ASPNETCORE_URLS="http://0.0.0.0:${PORT}"
fi

exec dotnet out/hng-genderizeApp.dll
