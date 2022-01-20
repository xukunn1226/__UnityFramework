#!/bin/bash
echo "start building...iOS"

# Working directory
BATCH_PATH=$(cd $(dirname $0); pwd)
echo $CURRENT_DIR

BATCH_TO_PROJECT=..
PROJECT_PATH=${BATCH_PATH}/${BATCH_TO_PROJECT}
echo "	[PROJECT PATH]:"	$PROJECT_PATH

BUILD_TARGET=$BUILD_TARGET
BUILD_PROFILE=$BUILD_PROFILE
echo "	[BUILD TARGET]:"	$BUILD_TARGET
echo "	[BUILD PROFILE]:"	$BUILD_PROFILE

########################################################################

BUNDLE_PATH=${PROJECT_PATH}/Deployment/Latest/AssetBundles
echo "	[BUNDLE PATH]:"		${BUILD_PROFILE}

# Player Output
if [ -z "${PLAYER_PATH}" ]; then
    PLAYER_PATH=${PROJECT_PATH}/Deployment/Latest/Player
fi
echo "	[PLAYER PATH]:"		${PLAYER_PATH}

# Log
LOG_PATH=${PROJECT_PATH}/Deployment/Latest/build_log.txt
echo "	[LOG PATH]:"		${LOG_PATH}

# Build Mode: 0(Bundles & Player)、1(Bundles)、2(Player)
BUILD_MODE_PARAMETER=0
echo "	[BUILD MODE]:"		${BUILD_MODE_PARAMETER}

if [ -z "${Development}" ]; then
    Development="true"
fi
echo "	[Development]:"		${Development}

if [ -z "${useIL2CPP}" ]; then
    useIL2CPP="false"
fi
echo "	[useIL2CPP]:"		${useIL2CPP}

if [ -z "${useMTRendering}" ]; then
    useMTRendering="true"
fi
echo "	[useMTRendering]:"		${useMTRendering}

if [ -z "${RebuildBundles}" ]; then
    RebuildBundles="false"
fi
echo "	[RebuildBundles]:"		${RebuildBundles}

MACRODEFINES=${MACRODEFINES}
echo "	[MACRODEFINES]:"	${MACRODEFINES}


# App Version(VersionNoChanged、VersionIncrease、VersionSpecific 1.2.3)

# Fixed Command
FIXED_COMMAND="-batchmode -quit -nographics -projectPath ${PROJECT_PATH} -buildTarget ${BUILD_TARGET} -executeMethod Framework.AssetManagement.GameBuilder.GameBuilder.cmdBuildGame -BuilderProfile ${BUILD_PROFILE}"
echo "	[FIXED COMMAND]:"	${FIXED_COMMAND}

# Optional Command
OVERRIDE_COMMAND="-bundlesOutput ${BUNDLE_PATH} -playerOutput ${PLAYER_PATH} -logFile ${LOG_PATH} -BuildMode ${BUILD_MODE_PARAMETER} -VersionNoChanged -Development ${DEVELOPMENT} -useIL2CPP ${USEIL2CPP} -useMTRendering ${useMTRendering} -RebuildBundles ${RebuildBundles} -MacroDefines" ${MACRODEFINES}
echo "	[OVERRIDE COMMAND]:"	${OVERRIDE_COMMAND}

echo "	[Unity Path]:		D:\Program Files\2021.2.3f1\Editor\Unity.exe"


"D:\Program Files\2021.2.3f1\Editor\Unity.exe" ${FIXED_COMMAND} ${OVERRIDE_COMMAND}


echo "End building...iOS"
