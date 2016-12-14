using System;
using UnityEngine;
using System.Collections.Generic;
using f3;

public class VRSampleSceneConfig : MonoBehaviour
{
    public GameObject OculusCameraRig;

    FContext context;

    // Use this for initialization
    void Awake()
    {
        // restore any settings
        SceneGraphConfig.RestorePreferences();

        SceneOptions options = new SceneOptions();
        options.EnableTransforms = true;
        options.EnableCockpit = true;
        options.CockpitInitializer = new SetupBasicVRCockpit();

        options.MouseCameraControls = new MayaCameraHotkeys();
        options.SpatialCameraRig = OculusCameraRig;

        // very verbose
        options.LogLevel = 2;

        context = new FContext();
        context.Start(options);

        // if you had other gizmos, you would register them here
        //context.TransformManager.RegisterGizmoType("snap_drag", new SnapDragGizmoBuilder());
        //controller.TransformManager.SetActiveGizmoType("snap_drag");

        // if you had other tools, you would register them here.
        context.ToolManager.RegisterToolType(DrawPrimitivesTool.Identifier, new DrawPrimitivesToolBuilder());
        context.ToolManager.SetActiveToolType(DrawPrimitivesTool.Identifier, ToolSide.Right);

        // set Scene object in our scripts because I can't seem to do it in Inspector anymore??
        // perhaps we could do the script part of these in code, then 
        // we would just need to know the object names...
        GameObject lighting = GameObject.Find("SceneLighting");
        lighting.GetComponent<SceneLightingSetup>().Scene = context;

        // set up ground plane geometry
        GameObject groundPlane = GameObject.Find("GroundPlane");
        context.Scene.AddWorldBoundsObject(groundPlane);
    }

    // Update is called once per frame
    void Update()
    {
        context.Update();
    }



}