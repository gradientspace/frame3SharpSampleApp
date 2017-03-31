# frame3SharpSampleApp

Sample Unity applications built using the [frame3Sharp](https://github.com/gradientspace/frame3Sharp) CAD-tool framework.

These samples are released under the MIT License, feel free to use them as a starting point.

Questions? Contact Ryan Schmidt [@rms80](http://www.twitter.com/rms80) / [gradientspace](http://www.gradientspace.com)


# VRSampleScene 

Basic VR interface where you can add Boxes and Cylinders via either double-clicking or drag-dropping from the cockpit HUD. You can also use a Draw Primitive Tool to draw Boxes and Cylinders interactively (click on the drag-drop buttons  during the tool to change the current primitive type).

You can also select primitives in the scene and move them using the 3D manipulator gizmo. Undo/Redo is supported with Ctrl+Z.

There are several scenes provided with different configurations. All of these scenes support mouse and gamepad input.

**AutoVRSampleScene** supports both Vive and Rift, and auto-configures for either using the [gsUnityVR](https://github.com/gradientspace/gsUnityVR) library. After the auto-configuration, the hand-tracked controllers can be used to interact with the scene. 

**OculusSampleScene** and **ViveSampleScene** support each device separately, using the Camera Rigs provided by the OVR and SteamVR libraries (*OVRCameraRig* or *[CameraRig]*)

**VRSampleScene** is just a generic VR environment, hand-tracked controllers are not supported. 

The Frame3 framework is configured in the **VRSampleSceneConfig.cs** script, and the Cockpit (HUD UI, controller behaviors) is configured in **SetupBasicVRCockpit.cs**.

Several of the scenes have existing 3D objects in them. These are converted into Frame3 *SceneObject*s at the bottom of  VRSampleSceneConfig.cs. 

