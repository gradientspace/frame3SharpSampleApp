using System;
using UnityEngine;
using System.Collections.Generic;
using f3;
using g3;

public class VRSampleSceneConfig : BaseSceneConfig
{
    public GameObject VRCameraRig;

    FContext context;
    public override FContext Context { get { return context; } }

    // Use this for initialization
    public override void Awake()
    {
        // if we need to auto-configure Rift vs Vive vs (?) VR, we need
        // to do this before any other F3 setup, because MainCamera will change
        // and we are caching that in a lot of places...
        if (AutoConfigVR) {
            VRCameraRig = gs.VRPlatform.AutoConfigureVR();
        }

        // restore any settings
        SceneGraphConfig.RestorePreferences();

        // set up some defaults
        // this will move the ground plane down, but the bunnies will be floating...
        //SceneGraphConfig.InitialSceneTranslate = -4.0f * Vector3f.AxisY;
        SceneGraphConfig.DefaultSceneCurveVisualDegrees = 0.5f;
        SceneGraphConfig.DefaultPivotVisualDegrees = 2.3f;
        SceneGraphConfig.DefaultAxisGizmoVisualDegrees = 25.0f;


        SceneOptions options = new SceneOptions();
        options.UseSystemMouseCursor = false;
        options.Use2DCockpit = false;
        options.EnableTransforms = true;
        options.EnableCockpit = true;
        options.CockpitInitializer = new SetupBasicVRCockpit();

        options.MouseCameraControls = new MayaCameraHotkeys();
        options.SpatialCameraRig = VRCameraRig;

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

        // Set up standard scene lighting if requested
        if ( options.EnableDefaultLighting ) {
            GameObject lighting = GameObject.Find("SceneLighting");
            if (lighting == null)
                lighting = new GameObject("SceneLighting");
            SceneLightingSetup setup = lighting.AddComponent<SceneLightingSetup>();
            setup.Context = context;
            setup.LightDistance = 30.0f; // related to total scene scale...
        }



        /*
         * Import elements of Unity scene that already exist into the FScene
         */

        // set up ground plane geometry (optional)
        GameObject groundPlane = GameObject.Find("GroundPlane");
        context.Scene.AddWorldBoundsObject(groundPlane);

        SceneObject focusSO = null;

        // wrap existing complex GameObject named capsule1 as a SceneObject
        GameObject capsuleGO = GameObject.Find("capsule1");
        if (capsuleGO != null) {
            SceneObject capsuleSO = UnitySceneUtil.WrapAnyGameObject(capsuleGO, context, true);
            focusSO = capsuleSO;
        }

        // wrap a prefab as a GOWrapperSO
        GameObject prefabGO = GameObject.Find("bunny_prefab");
        if ( prefabGO != null ) 
            UnitySceneUtil.WrapAnyGameObject(prefabGO, context, false);

        // convert a mesh GameObject to our DMeshSO
        // Note: any child GameObjects will be lost
        GameObject meshGO = GameObject.Find("bunny_mesh");
        if (meshGO != null) {
            //DMeshSO meshSO = UnitySceneUtil.WrapMeshGameObject(meshGO, context, true) as DMeshSO;
            UnitySceneUtil.WrapMeshGameObject(meshGO, context, true);
        }

        GameObject meshGO2 = GameObject.Find("bunny_mesh2");
        if (meshGO2 != null) {
            //DMeshSO meshSO = UnitySceneUtil.WrapMeshGameObject(meshGO, context, true) as DMeshSO;
            UnitySceneUtil.WrapMeshGameObject(meshGO2, context, true);
        }

        // center the camera on the capsule assembly
        if (focusSO != null) {
            Vector3f centerPt = focusSO.GetLocalFrame(CoordSpace.WorldCoords).Origin;
            context.ActiveCamera.Animator().AnimatePanFocus(centerPt, CoordSpace.WorldCoords, 0.3f);
        }

    }



}