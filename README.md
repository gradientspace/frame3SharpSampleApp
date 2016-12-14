# frame3SharpSampleApp

Sample Unity applications built using the frame3Sharp CAD-tool framework.

These samples are released under the MIT License, feel free to use them as a starting point.

Questions? Contact Ryan Schmidt [@rms80](http://www.twitter.com/rms80) / [gradientspace](http://www.gradientspace.com)


# VRSampleScene 

Basic VR interface where you can add Boxes and Cylinders via either double-clicking or drag-dropping from the cockpit HUD. You can also use a Draw Primitive Tool to draw Boxes and Cylinders interactively (click on the drag-drop buttons  during the tool to change the current primitive type).

You can also select primitives in the scene and move them using the 3D manipulator gizmo. Undo/Redo is supported with Ctrl+Z.

Oculus Touch controllers are supported, however not in the Scene named **VRSampleScene**. You must open **OculusSampleScene** instead. It uses the *OVRCameraRig* setup from Oculus Utilities for Unity 5, instead of the standard Unity camera. This is because the OVRCameraRig provides the hand-tracked positions.

The framework is configured in the **VRSampleSceneConfig.cs** script, and the Cockpit (HUD UI, controller behaviors) is configured in **SetupBasicVRCockpit.cs**.
