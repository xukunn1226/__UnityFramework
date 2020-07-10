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