# SoftObjectPath

## 功能
以序列化资源地址的方式记录对象之间的关联

## 实现
记录被引用资源的引用情况，见<font color=#90C773>**RedirectorDB**</font>

## 使用
* 仅记录资源地址，需要与使用此资源的脚本挂载同一层
* 只能引用**非场景资源**
* 可以挂载到**场景及非场景对象上**

## 更新机制
* 通过监听OnPostprocessAllAssets捕获资源的改动消息
* RefObject_DB：记录资源（Any type）被引用的信息
* UserObject_DB：记录资源（prefab only）引用的其他的资源信息


## SoftObjectAttribute
> [SoftObject] public SoftObjectPath m_BuildingVillage;         // 提示SoftObjectPath指向的对象名称
