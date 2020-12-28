#pragma once

#ifdef _MSC_VER

	#ifndef EXPORT_DLL
	#define EXPORT_DLL __declspec(dllexport) //导出dll声明
	#endif

#else

	#define EXPORT_DLL __attribute((visibility("default")))

#endif


extern "C"
{
	EXPORT_DLL int MyAddFunc(int _a, int _b);
}


//
//#define CROSS_PLATFORM_LIBRARY_EXPORTS
//
//#ifdef _MSC_VER //用于判断是否是 vs 平台
//	#define CROSS_PLATFORM_HIDDEN_API
//	#ifdef CROSS_PLATFORM_LIBRARY_EXPORTS
//		#define CROSS_PLATFORM_API __declspec(dllexport)
//	#else
//		#define CROSS_PLATFORM_API __declspec(dllimport)
//	#endif
//#else // 说明是 OSX 或者 Linux
//	#define CROSS_PLATFORM_API			__attribute((visibility("default")))	// 明确指示，这个函数在动态库中可见
//	#define CROSS_PLATFORM_HIDDEN_API	__attribute((visibility("hidden")))		// 明确指示，这个函数在动态库中不可见
//#endif
//
//
//// 不加 CROSS_PLATFORM_API 的情况
//// 在 gcc/clang 编译的情形下也会被导出并且可以被调用
//// 在 vs 编译的情况下无法被调用
//void name();
//
//// 公开的 API，动态库可以被外部所调用
//CROSS_PLATFORM_API void hello();
//
//// 我是你看得到，调用不了的函数
//CROSS_PLATFORM_HIDDEN_API void cantHello();


