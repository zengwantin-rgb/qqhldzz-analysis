#!/bin/bash
set -euo pipefail
mkdir -p build/runtime-probe/checks
sdk_path=$(xcrun --sdk iphoneos --show-sdk-path)
xcrun --sdk iphoneos clang -dynamiclib -target arm64-apple-ios15.0 -isysroot "$sdk_path" -fobjc-arc -O2 -Wall -Wextra -Wno-unused-parameter -framework Foundation mobile/RuntimeProbe/ProbeStore.m mobile/RuntimeProbe/RuntimeProbe.m -Wl,-install_name,@rpath/MJRuntimeProbe.dylib -o build/runtime-probe/MJRuntimeProbe.dylib
codesign --force --sign - build/runtime-probe/MJRuntimeProbe.dylib
codesign --verify --strict build/runtime-probe/MJRuntimeProbe.dylib
xcrun lipo -info build/runtime-probe/MJRuntimeProbe.dylib > build/runtime-probe/checks/architecture.txt
xcrun vtool -show-build build/runtime-probe/MJRuntimeProbe.dylib > build/runtime-probe/checks/deployment.txt
xcrun otool -L build/runtime-probe/MJRuntimeProbe.dylib > build/runtime-probe/checks/dependencies.txt
shasum -a 256 build/runtime-probe/MJRuntimeProbe.dylib > build/runtime-probe/checks/SHA256SUMS.txt
