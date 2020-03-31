# **AssetBrowser Tool**
此工具将帮助你分析资产（AssetBundle）的各项数据，以解决因资产设置不合理导致的**内存增长、加载卡顿、包体冗余**等问题。

* 基于Asset分析出Bundle数据，不需要真正打AssetBundle
* 打开方式：**AssetModule > Asset Browser**
* 使用前提：资产已设置好assetbundle name

场景资产不同于非场景资产，分两个Tab独立分析

![Toolbar](https://github.com/xukunn1226/ResourcesManager/blob/master/Assets/AssetModule/AssetBrowser/Documentation/images/Toolbar.png "Toolbar")


## **Bundle Tab**
窗口以类浏览器的方式展示资产属性，并提示资产的**健康**状况，以便开发者快速准确定位问题。窗口被分为五个部分：Bundle List、Bundle Detail、Asset List、ReferencedObject List、Summary
![Bundle Browser](https://github.com/xukunn1226/ResourcesManager/blob/master/Assets/AssetModule/AssetBrowser/Documentation/images/Browser.png "Bundle Browser")

### **Bundle List**
树状结构展示所有AssetBundle：
* Bundle：显示AssetBundle Name
* Size：ab包含的所有资产大小，非真正ab包大小
* Count：依赖其他AB的数量，具体哪些AB见Bundle Detail
* Warning，Error or Info：健康状况，详见Asset List
* 彩色Icon表示AssetBundle，灰色Icon表示不是AssetBundle
* Alt + Enter：展开/收起选中节点

### **Bundle Detail**
* 显示Bundle依赖的其他Bundle
* Alt + Right Click，快速跳转到此Bundle node

### **Asset List**
显示Bundle List选中的Bundle包含的所有资产列表
* Name： 资产名称
* Size： 资产文件大小，此时尚未打包，数据仅供参考不具实际意义
* Missing Reference Hint：提示是否有missing reference，tips显示详细信息
* Warning、Error or Info：提示资产引用的其他资产是否合理（含内置资产或外部资产视为不合理）

### **ReferencedObject List**
显示Asset List选中的资产引用到的所有资产列表
* Name： 资产名称
* Type： 资产类型
* AssetPath： 资产地址
* isBuiltIn： 是否是内置资产
* isExternal： 是否是外部资产（即没有被打为Bundle，造成同一份数据多次被打包）
