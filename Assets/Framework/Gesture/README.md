# BG Canvas
## 解决什么问题
* 如何优雅的捕获未被UI、可交互对象处理的消息

## 思路

利用EventSystem的拣选排序机制（EventSystem::RaycastComparer），确保参与拣选结果一定是优先UI，其次是场景中可交互对象，最后被捕获的才是未被处理的消息。

## 方案
添加BG Canvas组件用于监听未被UI、场景处理的消息
* Setup "Canvas"
    * Screen Space - Camera
    * Canvas Scaler: same to UICanvas
    * Attach "Graphic Raycaster"
* Setup "BGCanvas"
    * RectTransform: Stretch / Anchor Min[0, 0] / Anchor Max[1, 1]
    * Add "NoDrawingRaycast.cs"
* Setup "Gesture Camera"
    * RenderType：Overlay
    * **Priority=-99 ———— 确保RaycastAll结果排序始终在最后**
    
    以上设置的目的都是为了启用拣选（raycast），禁用渲染

## 使用
把组件BG Canvas拖入场景即可
<br></br>
<br></br>


# Gesture
## 设计目标
* 基于EventSystem之上扩展几个常用Gesture：LongPress、Pinch、Tap、Drag
* Gesture适用于所有对象（2D、3D）
* 易扩展、易使用

## 使用
1. 添加脚本Recognizer(XXXRecognizer.cs)至需要响应手势的对象上
2. 把处理手势事件的脚本继承接口IXXXHandler