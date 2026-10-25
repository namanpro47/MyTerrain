#!/bin/sh
set -eu
cd "$(dirname "$0")/.."
# An explicit frozen export avoids deploying a later build by accident.
build_dir=${1:-Builds/Web}
build_dir=$(cd "$build_dir" && pwd)
test -f "$build_dir/index.html"
test -f "$build_dir/vercel.json"
test -d "$build_dir/Build"
# Link only the selected static export, never the Unity repository.
vercel link --cwd "$build_dir" --yes --project terra-browser --scope namanpro47s-projects
vercel deploy --cwd "$build_dir" --prod --yes --scope namanpro47s-projects
