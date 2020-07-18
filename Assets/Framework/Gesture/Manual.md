# Scalable Gesture Module Based On Event System

## WHAT
* The Event System is a way of sending events to objects in the application based on input, be it keyboard, mouse, touch, or custom input
* Supported Events：
    * IPointerEnterHandler
    * IPointerExitHandler
    * IPointerDownHandler
    * IPointerUpHandler
    * IPointerClickHandler
    * IInitializePotentialDragHandler
    * IBeginDragHandler
    * IDragHandler
    * IEndDragHandler
    * IDropHandler
    * IScrollHandler
    * IUpdateSelectedHandler
    * ISelectHandler
    * IDeselectHandler
    * IMoveHandler
    * ISubmitHandler
    * ICancelHandler

## Problem
* No Gesture Support（LongPress, ScreenDrag, Pinch, Tap）
* No Event Flow

## HOW
* Build new event flow: 2D(UI) -> 3D(Player/Tree/Interactive Objects) -> Screen(Drag/Pinch)


# Gesture
## Goal
* Extend universal gestures based on event system：LongPress、Pinch、Tap、Drag
* Gesture components for all objects（2D/3D），be similar to IXXXHandler
* Scalable
* Easy to use
* Compatible with StandaloneInputModule and InputSystemUIInputModule

# Usage
* Replace StandaloneInputModule with MyStandaloneInputModule on EventSystem
</br>
</br>
</br>
</br>
</br>
</br>

# Reference
## GestureEventData
手势数据，PointerEventData的集合
## Properties
Proprety | Description
--|--
Position|当前位置（屏幕坐标），所有PointerEventData.position的平均值
PressPosition|起始位置（屏幕坐标），所有PointerEventData.pressPosition的平均值
StartTime|起始时间
ElapsedTime|手势开始后的时长

## IDiscreteGestureHandler
Function | Description
--|--
OnGestureReady|
OnGestureRecognized|
OnGestureFailed|

## IContinuousGestureHandler
Function | Description
--|--
OnGestureReady|
OnGestureStarted|
OnGestureProgress|
OnGestureEnded|
OnGestureFailed|


## GestureEvent
Send specified event to gameobject




# Extension
1、Customize GestureEventData
