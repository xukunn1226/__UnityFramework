#pragma once

#ifdef _MSC_VER

	#ifndef EXPORT_DLL
	#define EXPORT_DLL __declspec(dllexport) //����dll����
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
//#ifdef _MSC_VER //�����ж��Ƿ��� vs ƽ̨
//	#define CROSS_PLATFORM_HIDDEN_API
//	#ifdef CROSS_PLATFORM_LIBRARY_EXPORTS
//		#define CROSS_PLATFORM_API __declspec(dllexport)
//	#else
//		#define CROSS_PLATFORM_API __declspec(dllimport)
//	#endif
//#else // ˵���� OSX ���� Linux
//	#define CROSS_PLATFORM_API			__attribute((visibility("default")))	// ��ȷָʾ����������ڶ�̬���пɼ�
//	#define CROSS_PLATFORM_HIDDEN_API	__attribute((visibility("hidden")))		// ��ȷָʾ����������ڶ�̬���в��ɼ�
//#endif
//
//
//// ���� CROSS_PLATFORM_API �����
//// �� gcc/clang �����������Ҳ�ᱻ�������ҿ��Ա�����
//// �� vs �����������޷�������
//void name();
//
//// ������ API����̬����Ա��ⲿ������
//CROSS_PLATFORM_API void hello();
//
//// �����㿴�õ������ò��˵ĺ���
//CROSS_PLATFORM_HIDDEN_API void cantHello();


