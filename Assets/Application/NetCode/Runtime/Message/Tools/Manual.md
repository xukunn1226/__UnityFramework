## How to integrate Protobuf into Unity
1、[从这里](https://github.com/protocolbuffers/protobuf/releases/tag/v3.11.4)下载 protoc-3.11.4-win64.zip & protobuf-csharp-3.11.4.tar.gz

2、编译Google.Protobuf.dll

      打开protobuf-3.11.4\csharp\src\Google.Protobuf.sln编译Google.Protobuf，复制dll(bin\Release\net45\)至工程

3、创建ProtobufTool傻瓜包
![image](https://github.com/xukunn1226/PoolManager/blob/master/Images/image2020-4-15_15-33-45.png)

4、run C#_Generator.bat

     把Proto\下的proto生成相应的cs文件