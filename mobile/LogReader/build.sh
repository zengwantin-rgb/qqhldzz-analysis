#!/bin/bash
set -euo pipefail
mkdir -p build/Payload/MJLogReader.app build/checks
app=build/Payload/MJLogReader.app
sdk_path=$(xcrun --sdk iphoneos --show-sdk-path)
xcrun --sdk iphoneos clang -target arm64-apple-ios15.0 -isysroot "$sdk_path" -fobjc-arc -O2 -Wall -Wextra -Wno-unused-parameter -framework UIKit -framework Foundation -lz mobile/LogReader/main.m mobile/LogReader/Scanner.m -o "$app/MJLogReader"
cp mobile/LogReader/Info.plist "$app/Info.plist"
codesign --force --sign - --entitlements mobile/LogReader/Entitlements.plist "$app"
codesign --verify --deep --strict "$app"
codesign -d --entitlements :- "$app" > build/checks/device-entitlements.plist
plutil -lint "$app/Info.plist" build/checks/device-entitlements.plist
xcrun lipo -info "$app/MJLogReader" > build/checks/architecture.txt
xcrun vtool -show-build "$app/MJLogReader" > build/checks/deployment.txt
(cd build && /usr/bin/zip -q -r MJLogReader.ipa Payload)
shasum -a 256 build/MJLogReader.ipa > build/checks/SHA256SUMS.txt
