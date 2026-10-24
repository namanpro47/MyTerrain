#!/bin/sh
set -eu
cd "$(dirname "$0")/.."
test -f Builds/Web/index.html
test -f Builds/Web/vercel.json
# Link only the generated static export, never the Unity repository.
vercel link --cwd Builds/Web --yes --project terra-browser --scope namanpro47s-projects
vercel deploy --cwd Builds/Web --prod --yes --scope namanpro47s-projects
