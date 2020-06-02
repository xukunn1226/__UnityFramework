## 设计目标
* 基于EventSystem之上扩展几个常用Gesture：LongPress、Pinch、Tap
* Gesture适用于所有对象（2D、3D）


## 思路

基于EventSystem的消息分层处理，消息先UI层处理，然后是场景层。

思考：当消息不被UI层处理，场景层该如何截获呢？

方案：添加Gesture Canvas组件用于监听传输到场景层的消息
* Setup "Canvas"
    * Screen Space - Camera
    * Canvas Scaler: Scale Mode "Scale With Screen Size"
    * Attach "Graphic Raycaster"
* Setup "BGCanvas"
    * RectTransform: Stretch ———— 确保始终与Parent Canvas同大小
    * Add "NoDrawingRaycast.cs"
* Setup "Gesture Camera"
    * RenderType：Overlay
    * Priority=-99 ———— 确保RaycastAll结果排序始终在最后
    
    以上设置的目的都是为了启用拣选（raycast），禁用渲染

## 使用
把组件Gesture Canvas拖入场景即可，