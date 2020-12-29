include $(CLEAR_VARS)

# override strip command to strip all symbols from output library; no need to ship with those..
# cmd-strip = $(TOOLCHAIN_PREFIX)strip $1 

LOCAL_ARM_MODE  := arm
LOCAL_PATH      := $(NDK_PROJECT_PATH)
LOCAL_MODULE    := TestNative
LOCAL_CFLAGS    := -Werror
LOCAL_LDLIBS    := -llog



#########1. 指定源文件
#LOCAL_SRC_FILES := ../src/NativeCode.cpp






#########2. 获取某目录下（包含子目录）的所有文件
#traverse all the directory and subdirectory
define walk
  $(wildcard $(1)) $(foreach e, $(wildcard $(1)/*), $(call walk, $(e)))
endef

#find all the file recursively under jni/
ALLFILES = $(call walk, $(LOCAL_PATH)/../src)
FILE_LIST := $(filter %.cpp, $(ALLFILES))







########3. 获取某目录下（不包含子目录）的所有文件
#FILE_LIST := $(wildcard $(LOCAL_PATH)/../src/*.cpp)








LOCAL_SRC_FILES := $(FILE_LIST:$(LOCAL_PATH)/%=%)
$(warning $(LOCAL_SRC_FILES))

include $(BUILD_SHARED_LIBRARY)
