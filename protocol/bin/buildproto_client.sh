#!/bin/bash


echo "generate proto for csharp"
./protoc.exe --proto_path=../ ../*.proto --csharp_out=../../client/Assets/Application/hotfix/Protocol/
./buildproto_client_proto_extension.sh