#!/usr/bin/env bash
# Release build + Marketplace package: dist/UnityEditorControls_<version>.lplug4, checked with `logiplugintool verify`.
set -euo pipefail
cd "$(dirname "$0")"

DOTNET_ROOT="$HOME/.dotnet" "$HOME/.dotnet/dotnet" clean src -c Release -nologo -v q
./build.sh -c Release

version=$(sed -n 's/^version: *//p' src/package/metadata/LoupedeckPackage.yaml)
package="dist/UnityEditorControls_${version}.lplug4"
mkdir -p dist

# logiplugintool targets .NET 8; roll it forward onto the .NET 10 runtime in ~/.dotnet.
export PATH="$PATH:$HOME/.dotnet/tools" DOTNET_ROOT="$HOME/.dotnet" DOTNET_ROLL_FORWARD=Major
logiplugintool pack bin/Release/ "$package"
logiplugintool verify "$package"
echo "Package: $package"
