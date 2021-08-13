* bundletool
	1、生成apks，两种方法
		a：根据json配置：java -jar bundletool-all-1.7.1.jar build-apks --bundle=ZGame.aab --output=ZGame.apks --overwrite --ks=..\android.keystore --ks-pass=pass:lilith 
			--ks-key-alias=android --key-pass=pass:lilith --device-spec=config.json
		b：根据连接设备：java -jar bundletool-all-1.7.1.jar build-apks --bundle=ZGame.aab --output=ZGame_Spec.apks --overwrite --ks=..\android.keystore --ks-pass=pass:lilith 
			--ks-key-alias=android.keystore --key-pass=pass:lilith --connected-device
	2、安装apks
		java -jar bundletool-all-1.7.1.jar install-apks --apks=ZGame.apks
		
* pepk.jar
	java -jar pepk.jar --keystore=..\..\android.keystore --alias=android --output=output.zip --include-cert 
	--encryptionkey=eb10fe8f7c7c9df715022017b00c6471f8ba8170b13049a11e6c09ffe3056a104a3bbe4ac5a955f4ba4fe93fc8cef27558a3eb9d2a529a2092761fb833b656cd48b9de6a