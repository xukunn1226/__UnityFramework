#!/bin/sh
echo ""
echo "Compiling NativeCode.c..."
ANDROID_NDK_ROOT="G:/android-ndk-r21d"
$ANDROID_NDK_ROOT/ndk-build.cmd NDK_PROJECT_PATH=. NDK_APPLICATION_MK=Application.mk $*
mv libs/armeabi-v7a/libnative.so ./armeabi-v7a/libnative.so
mv libs/arm64-v8a/libnative.so ./arm64-v8a/libnative.so
mv libs/x86/libnative.so ./x86/libnative.so

echo ""
echo "Cleaning up / removing build folders..."  #optional..
rm -rf libs
rm -rf obj

echo ""
echo "Done!"
