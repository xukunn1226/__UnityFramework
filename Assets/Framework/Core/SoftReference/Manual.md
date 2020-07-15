# SoftObjectPath
## 功能
以序列化资源地址的方式记录对象之间的关联

## 实现
记录被引用资源的引用情况，见<font color=#90C773>**SoftRefRedirector**</font>

## 使用
* 仅记录资源地址，需要与使用此资源的脚本挂载同一层
* 只能引用**非场景资源**
* 可以挂载到**场景及非场景对象上**

## 更新机制
* 操纵面板上的SoftObjectPath引用对象时
    * 更新之前引用资源的DB
    * 更新当前引用资源的DB
    * SoftObjectPath.m_AssetPath & m_GUID
* 资源发生任意改动时，检查DB中是否有被使用，更新引用者数据





## SoftObjectPath

## SoftObjectAttribute
> [SoftObject] public SoftObjectPath m_BuildingVillage;         // 提示SoftObjectPath指向的对象名称
