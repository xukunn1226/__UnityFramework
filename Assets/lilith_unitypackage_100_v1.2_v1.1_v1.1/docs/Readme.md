# ！！！Archived ！！！
# 前往https://wiki.lilithgames.com/x/Lsq1Aw

# Lilith SDK Unity 插件接入说明

## 前言
- 本插件旨在统一Android/iOS平台SDK接口并简化Unity接入SDK的流程。

---
## 环境要求
### Android
- `Build System`选择`Gradle(new)`

### iOS
- 无

---
## 插件引入

### 通用
- 双击`.unitypackage`文件导入插件；
- 修改`Assets/LLHSDK/Plugins/Android/LLH.SDK.Android.dll`文件Inspector中platform属性为只对Android平台生效；
	![](./Pic/android_dll_inspector.png)
- 修改`Assets/LLHSDK/Plugins/iOS/LLH.SDK.iOS.dll`文件Inspector中platform属性为只对iOS平台生效；
	![](./Pic/ios_dll_inspector.png)

### Android
- 将运营提供的配置文件复制到`Assets/Plugins/Android/res/values`路径下，若路径不存在请手动新建该路径

### iOS
- 解压iOS SDK，将所有文件/文件夹（除了`lilith_sdk_web.js`）复制到`Assets/LLHSDK/Plugins/iOS/Lilith`目录下，使其目录结构如下图所示；
![](./Pic/ios_sdk_folder.png)
- 请参照配置文件，参照下图填写配置；
![](./Pic/ios_settings_window.png)

---
## 接口

### 命名空间
```
namespace LLH.SDK
```

### 类
```
public class LLHSDK
```

### 初始化
```
public static void LLHStart()
```
*在游戏开始时，回调注册之后调用。*

### 通用数据类型
```
public enum LLHErrorCode
{
    kLLHErrorCodeCancel = -10, // 用户取消
    kLLHErrorCodeNetwork = -2, // 网络错误
    kLLHErrorCodeUnknown = -1, // 未知错误
    kLLHErrorCodeSuccess = 0, // 成功
}

public struct LLHError
{
    public byte success; // 是否成功  0 失败  1成功
    public LLHErrorCode code; // 错误码，见如上枚举
    public string msg; // 错误信息
}
```
*错误，一般用于回调内，error.success为true代表成功。*

```

public struct LLHLoginTypeModel
{
    public LLHLoginTypeValue loginTypeValue; // 登录类型数值
    public LLHAuthTypeValue authTypeValue; // 验证类型数值

    public static bool operator ==(LLHLoginTypeModel t1, LLHLoginTypeModel t2);
    public static bool operator !=(LLHLoginTypeModel t1, LLHLoginTypeModel t2);
}

public static LLHLoginTypeModel kLLHLoginTypeModelNone; // 未知登录类型
public static LLHLoginTypeModel kLLHLoginTypeModelGuest; // 游客登录
public static LLHLoginTypeModel kLLHLoginTypeModelEmail; // 账户密码登录（弃用）
public static LLHLoginTypeModel kLLHLoginTypeModelPhone; // 手机登录
public static LLHLoginTypeModel kLLHLoginTypeModelFacebook; // fb登录
public static LLHLoginTypeModel kLLHLoginTypeModelGameCenter; // GameCenter登录（仅iOS）
public static LLHLoginTypeModel kLLHLoginTypeModelGooglePlus; // GooglePlus登录（仅Android，弃用）
public static LLHLoginTypeModel kLLHLoginTypeModelWechat; // 微信登录
public static LLHLoginTypeModel kLLHLoginTypeModelQQ; // QQ登录
public static LLHLoginTypeModel kLLHLoginTypeModelAuto; // 自动登录，无需判断该类型
public static LLHLoginTypeModel kLLHLoginTypeModelVK; // vk登陆（弃用）
public static LLHLoginTypeModel kLLHLoginTypeModelGoogle; // GoogleSignIn
public static LLHLoginTypeModel kLLHLoginTypeModelSIWA; // sign in with apple
public static LLHLoginTypeModel kLLHLoginTypeModelTwitter; // Twiitter
public static LLHLoginTypeModel kLLHLoginTypeModelLine; // Line
```
*登录类型。*

### 登录
```
public static void LLHLogin()
```
*拉起登录界面。*

```
public delegate void LLHLoginDelegate(LLHError error, LLHLoginTypeModel loginType, string appUid, string appToken); // error：错误，loginType：登录类型，appUid：用户uid，appToken：用户登录token
public static event LLHLoginDelegate OnLLHLoginCompleted;
```
*登录回调，游戏服务器需使用`appUid`和`appToken`和SDK服务器验证登录结果。*

### 切换与绑定
```
public static void LLHSwitchOrBind();
```
*拉起切换/绑定界面。*

```
public delegate void LLHSwitchDelegate(LLHError error, LLHLoginTypeModel loginType, string appUid, string appToken); // error：错误，loginType：登录类型，appUid：用户uid，appToken：用户登录token
public static event LLHSwitchDelegate OnLLHSwitchCompleted;
```
*切换回调，游戏服务器需使用`appUid`和`appToken`和SDK服务器验证登录结果。*

```
public delegate void LLHBindDelegate(LLHError error, LLHLoginTypeModel loginType, string appUid, string appToken); // error：错误，loginType：登录类型，appUid：用户uid，appToken：用户登录token
public static event LLHBindDelegate OnLLHBindCompleted;
```
*绑定回调。*

### 支付
```
public enum LLHProductItemType
{
    kLLHProductItemTypeUnknown = -1, // 未知类型
    kLLHProductItemTypeInApp = 0, // 应用内购买
    kLLHProductItemTypeSubscription = 1, // 订阅
}
```
*商品类型枚举。*

```
public static void LLHPay(string productId, string ext, LLHProductItemType type) // productId：商品id（配置在Itunes Connect/Google Play后台），ext：透传字段，type：商品类型
```
*支付接口。*

```
public delegate void LLHPayDelegate(LLHError error, int payValue, string productId, LLHPayType payType); // error：错误，payValue:支付金额（仅在国内Android支付生效，其他情形均返回0），payType：支付类型
public static event LLHPayDelegate OnLLHPayCompleted;
```
*支付回调。*

```
public static void LLHQuerySkus(string[] productIds) // productIds：需查询的商品id数组
public static void LLHQuerySkusWithSuks(string[] productIds) // productIds：需查询的商品id数组
```
*查询商品详情接口。*

```
public struct LLHSkuItem
{
    public string title; // 商品本地化名称
    public string description; // 商品本地化描述
    public string formatPrice; // 商品本地化价格描述
    public string productId; // 商品id
    public String currency; //货币类型代码
    public String priceAmountMicros; //price 的 ISO 4217 货币代码。例如，如果用英镑指定 price，则 price_currency_code 为 "GBP"。
    public String sdkConvertSymbol; //sdk根据currenty转换后的货币符号
    public LLHProductItemType itemType; // 商品类型
}
```
*商品描述信息结构。*

```
public delegate void LLHSkuDelegate(LLHSkuItem[] skuList, string[] invalidProductIds); // skuList: 商品描述信息数组，invalidProductIds：不合法的商品id
public static event LLHSkuDelegate OnLLHSkuQueryCompleted;
```
*商品描述信息回调。*

### 客服

```
 public static void LLHCustomerServiceConversation(Hashtable attrs, string[] tags) // attrs：客服自定义参数，tags：客服自定义标签
```
*客服会话接口。*

```
 public static void LLHCustomerServiceFAQ(Hashtable attrs, string[] tags) // attrs: 客服自定义参数，tags：客服自定义标签
```
*客服FAQ接口。*

```
public delegate void LLHCSUnreadMsgDelegate(int msgCount); // msgCount:未读消息数
public static event LLHCSUnreadMsgDelegate OnLLHCSUnreadMsgReceived;
```
*客服系统未读消息数回调。*

### UA数据上报

**事件名称/参数请参照运营所提供的表格。**

```
public static void LLHReport(string name, string token, params string[] args) // name：事件名，token：事件token，args：参数
```
*UA数据上报接口，用于一般事件。*

```
public static extern void LLHReportWithRevenue(string currency, double revenue, string token) // currency：货币编码（一般使用美元价格上报，eg."USD", "CNY"），revenue：事件价值（如，充值的数额，单位为美元/人民币元），token：事件token
```
*UA充值数据上报接口，用于充值事件，该接口上报事件的默认事件名为Purchase。*

```
public static void LLHReportWithRevenueAndName(string name, string currency, double revenue, string token, params string[] args) // name：事件名，currency：货币编码（一般使用美元价格上报，eg."USD", "CNY"），revenue：事件价值（如，充值的数额，单位为美元/人民币元），token：事件token，args：参数
```
*UA充值数据上报接口，用于事件名不为Purchase的充值事件。*

### Webview

```
public static void LLHVisitWeb(string url); // url: 需要访问的url地址
```
*访问Webview接口。*

### 设备信息

```
public static string LLHGetPackageName(); // package name/bundle id
public static string LLHGetFireBaseToken(); // firebasetoken
public static string LLHGetIdfa(); // idfa
public static string LLHGetAndroidId(); // android id
public static string LLHGetOSType(); // 系统类型(ios/android)
public static string LLHGetOSVersion(); // 系统版本
public static string LLHGetAppVersion(); // App版本
public static string LLHGetDeviceModel(); // 设备型号
public static string LLHGetGameId(); // game id
public static string LLHGetChannelId(); // channel id
public static string LLHGetSystemLanguage(); // 手机系统语言
```
*可以同步获取的信息。*

```
public static void LLHGetGoogleAid(); // 获取google aid

/*
 * 获取google aid回调
 */
public delegate void LLHGoogleAidRequestDelegate(string googleAid); // googleAid: google aid
public static event LLHGoogleAidRequestDelegate OnLLHGoogleAidRequested;
```
*异步获取的信息。*

### 自定义数据上报

```
/*
 * 上报自定义数据
 */
public static void LLHReportToLilith(string name, params string[] args);
```
*自定义数据上报。*

```
/*
 * 打开V2协议页面 
 */
public static void LLHStartTermsV2ViewOk ();

public static void LLHStartTermsV2ViewConfirm ();
```
Confirm页面会有两个回调
LLHSDK.OnLLHUserConfirm += LLHSDK_OnLLHUserConfirm;
LLHSDK_OnLLHUserConfirm (LLHSDK.LLHError error)
error.success == 1 说明用户点击同意
error.success == 0 说明用户点击取消


```
/**
  * 获取AppId
  */
public static string LLHGetAppId();
```
*获取AppId*

### Vip相关

```
/**
  * 是否打开vip模块  1 打开  0 关闭
  */
public static void LLHOpenVipMode(int open)

/**
  * eventType 事件时机， 0 进入游戏，1 创建角色，2 角色升级
  * 
  * roleId 用户的Id
  * roleName 用户名称
  * level 用户等级
  * vipLevel 用户vip等级
  * serverId 用户所在服务器Id
  * serverIdForReport 用于上报日志可与 serverId 一致
  * serverName 用户所在服务器名称
  * guildId 用户所在工会Id 
  * guildName 用户所在工会名称
  * balance 用户钱包余额
  * createRoleTime 用户创角时间(毫秒时间戳)
  */
public static void LLHReportRoleInfo(int eventType, 
                                                    string roleId, 
                                                    string roleName, 
                                                    int level, 
                                                    int vipLevel,
                                                    Int64 serverId,
                                                    string serverIdForReport,
                                                    string serverName, 
                                                    Int64 guildId,
                                                    string guildName,
                                                    Int64 balance, 
                                                    Int64 createRoleTime);
回调
用户未领取礼包数量
LLHSDK.OnVipRedPointUpdate += LLHSDK_OnVipRedPointUpdate;

void LLHSDK_OnVipRedPointUpdate(int count)
{

}

```

#### Google 应用商店评分接口
```
/**
  * 调用Google 商店评分接口
  */
public static void LLHAppReviewerGooglePlay()

/**
 * 国际版 Google 评分事件完成回调 （根据需求选接）
 * 回调包含事件 用户取消和评分完成
 */
  
private static extern void LLHRegisterGoogleReviewCallback(LLHGooglePlayReviewFinishCallback callback)


```
