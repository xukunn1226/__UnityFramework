#!/bin/bash


echo "generate proto for csharp"
../tools/ScanProtoMsg/bin/Debug/netcoreapp3.1/ScanProtoMsg.exe -s "../" -d "../../client/Assets/Application/hotfix/Protocol/"
