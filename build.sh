#!/usr/bin/env bash
# Rebuilds action icons, default profiles, localization and the plugin. Uses the user-local .NET 10 SDK in ~/.dotnet:
# the system `dotnet` is 9, and PluginApi.dll from Logi Plugin Service 6.4 requires net10.0.
set -euo pipefail
cd "$(dirname "$0")"

python3 icons/build_icons.py
python3 profiles/build_profile.py
python3 localization/build_xliff.py
DOTNET_ROOT="$HOME/.dotnet" "$HOME/.dotnet/dotnet" build src -nologo "$@"
