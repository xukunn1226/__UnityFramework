#!/bin/sh
echo ""
echo "Compiling NativeCode.c..."
ANDROID_NDK_ROOT="G:/android-ndk-r21d"
$ANDROID_NDK_ROOT/ndk-build.cmd NDK_PROJECT_PATH=. NDK_APPLICATION_MK=Application.mk $*
mv libs/armeabi-v7a/libTestNative.so 	./armeabi-v7a/libTestNative.so
mv libs/arm64-v8a/libTestNative.so 		./arm64-v8a/libTestNative.so
mv libs/x86/libTestNative.so 			./x86/libTestNative.so

echo ""
echo "Cleaning up / removing build folders..."  #optional..
rm -rf libs
rm -rf obj

echo ""
echo "Done!"
