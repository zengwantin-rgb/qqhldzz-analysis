#!/bin/bash
set -euo pipefail
mkdir -p build/message-observer-device/checks
sdk_path=$(xcrun --sdk iphoneos --show-sdk-path)
xcrun --sdk iphoneos clang -dynamiclib -target arm64-apple-ios15.0 -isysroot "$sdk_path" -fobjc-arc -O2 -Wall -Wextra -Wno-unused-parameter -framework Foundation -I build/message-observer \
 -DMJ_PROBE_DIRECTORY_NAME='"MJMessageObserver"' -DMJ_PROBE_FILE_NAME='"observer-status.log"' -DMJ_PROBE_HEADER='"MJMessageObserverStatus/1\n"' \
 mobile/RuntimeProbe/ProbeStore.m mobile/MessageObserver/ObserverFiles.m mobile/MessageObserver/NativeObserver.m \
 -Wl,-install_name,@rpath/MJMessageObserver.dylib -o build/message-observer-device/MJMessageObserver.dylib
codesign --force --sign - build/message-observer-device/MJMessageObserver.dylib
codesign --verify --strict build/message-observer-device/MJMessageObserver.dylib
xcrun lipo -info build/message-observer-device/MJMessageObserver.dylib > build/message-observer-device/checks/architecture.txt
xcrun vtool -show-build build/message-observer-device/MJMessageObserver.dylib > build/message-observer-device/checks/deployment.txt
xcrun otool -L build/message-observer-device/MJMessageObserver.dylib > build/message-observer-device/checks/dependencies.txt
shasum -a 256 build/message-observer-device/MJMessageObserver.dylib > build/message-observer-device/checks/SHA256SUMS.txt
