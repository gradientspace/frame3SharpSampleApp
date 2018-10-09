# frame3SharpSampleApp

Sample Unity applications built using the [frame3Sharp](https://github.com/gradientspace/frame3Sharp) CAD-tool framework.

These samples are released under the MIT License, feel free to use them as a starting point.

Questions? Contact Ryan Schmidt [@rms80](http://www.twitter.com/rms80) / [gradientspace](http://www.gradientspace.com)


# Usage

**CRITICAL: THIS REPO LINKS TO SUBMODULES**. The majority of git clients do not automatically check out submodules!! If when you first check out this project, the **Assets/geometry3Sharp** folder is empty, you need to manually check out the submodules. The git command to grab all of them is:

    git submodule update --init --recursive

Once you have done this, you should be able to open the Unity project without errors, and open and run the sample scenes.


# VRSampleScene 

This is a basic f3Sharp VR scene with a few sample objects and tools accessible via a basic cockpit (implemented in *SetupBasicVRCockpit.cs*). Although several unity scene files are in this subdirectory, likely the only fully functional one is **AutoVRSampleScene**. 

![unity screenshot](https://github.com/gradientspace/frame3SharpSampleApp/raw/master/doc/unity_screenshot.png)


#### VR Controller Interactions

* You can "single-click" the trigger on either controller to do actions like select objects, click on tool buttons, etc. When an object is selected it turns orange, as in the screenshot above, and a 3D manip gizmo appears. Use either trigger to click-drag on the elements of the 3D manip.

* press and hold trigger+shoulder (rift) or trigger+grip (vive) on either hand while pointing at an object to "grab it" and them you can move it with the controller

* press and hold both shoulder/grip buttons at the same time and then move your hands in various ways to do various 3D scene manipulations - see the **View Controls** section of [this page](http://www.gradientspace.com/simplex-touch-controls) page here, the behavior is the same

* you can duplicate objects with 'd' key on keyboard, delete them with Delete, etc. Hotkeys are configured in **SampleKeyHandler** inside *SetupBasicVRCockpit.cs*

#### Tools and Cockpit

In f3Sharp, a Tool is a modal action attached to a hand/controller. When a Tool is actie on a hand, the cursor icon changes from an arrow to a "crayon" shape (green in screenshot below), and that tool button becomes yellow. Note that you can have multiple tools active simultaneously, ie one on each hand. *(however the UI highlight may not update properly in this case...)*

![cockpit image](https://github.com/gradientspace/frame3SharpSampleApp/blob/master/doc/cockpit.png)

The cockpit sits "down" in the VR view. Currently it is a set of circular 3D buttons. On the right are three Tool Buttons:

1) **Draw Primitive Tool** - when active on a hand, click-drag the trigger on that hand to begin drawing a primitive. Most primitives require 3 dimensions so after you set the base size/width, trigger-click again to set the height, and trigger-click a second time to complete the primitive. You can draw primitives on the ground plane or other surfaces. The **selected primitive** is set using the two buttons on the right side of the cockpit (currently cube and cylinder)

2) **Draw 3D Curve Tool** - when active on a hand, click-drag the trigger to draw a 3D polyline at the tip of the controller

3) **Cancel Active Tool** - when a tool is active on a hand, and you click on this button, the tool is deactivated

The Left buttons are used to select the active primitive for the Draw Primitive Tool. In addition, you can trigger-click-drag on these buttons to "drag" that primitive into the scene. You can also trigger-double-click on them to create one of these primitives at the origin.

#### Initial Objects

The initial objects in the **AutoVRSampleScene** are two bunny models with red and green (transparent) shaders. These are converted into frame3Sharp **SceneObject** instances in two ways - first by "wrapping" an arbitrary gameObject using **UnitySceneUtil.WrapAnyGameObject()**, and then by converting the MeshFilter mesh into a **DMeshSO** using **UnitySceneUtil.WrapMeshGameObject**. The latter works much better with f3Sharp but the former shows that it is possible to apply the manips/etc to arbitrary GameObjects. 

When you draw primitives or curves, these become **PrimitiveSO** or **PolyCurveSO** SceneObjects.


#### Unity Scene Files

There are several scenes provided with different configurations. All of these scenes support mouse and gamepad input. Several support spatial input with the Oculus Touch or Vive Wands. For details on how to use those controllers, see the [Simplex Touch Manual](http://www.gradientspace.com/simplex-touch-controls). Vive usage is similar. Not everything is included, but you can Grab Objects using the trigger+shoulder buttons (separate object with each hand!)

**AutoVRSampleScene** supports both Vive and Rift, and auto-configures for either using the [gsUnityVR](https://github.com/gradientspace/gsUnityVR) library. After the auto-configuration, the hand-tracked controllers can be used to interact with the scene. 

**OculusSampleScene** and **ViveSampleScene** support each device separately, using the Camera Rigs provided by the OVR and SteamVR libraries (*OVRCameraRig* or *[CameraRig]*)

**VRSampleScene** is just a generic VR environment, hand-tracked controllers are not supported. 

The Frame3 framework is configured in the **VRSampleSceneConfig.cs** script, and the Cockpit (HUD UI, controller behaviors) is configured in **SetupBasicVRCockpit.cs**.

Several of the scenes have existing 3D objects in them. These are converted into Frame3 *SceneObject*s at the bottom of  VRSampleSceneConfig.cs. 

