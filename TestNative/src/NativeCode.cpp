#include "NativeCode.h"

extern "C"
{
	int MyAddFunc(int _a, int _b)
	{
		return _a + _b;
	}
}