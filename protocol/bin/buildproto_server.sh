#!/bin/bash

echo "generate proto for golang"

./protoc.exe --proto_path=../ ../*.proto --gofast_out=../../server/protocol/
./protoc.exe --proto_path=../ ../server/*.proto --gofast_out=../../server/protocol/
#./protoc.exe --proto_path=../ ../rpc/*.proto --gofast_out=../../server/protocol/
./protoc.exe --proto_path=../ ../rpc/*.proto --gofast_out=plugins=grpc:./
 
#./protoc.exe --proto_path=../ ../*.proto --go_out=../../server/protocol/


