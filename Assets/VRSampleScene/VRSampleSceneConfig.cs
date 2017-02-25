using System;
using UnityEngine;
using System.Collections.Generic;
using f3;
using g3;

public class VRSampleSceneConfig : MonoBehaviour
{
    public GameObject OculusCameraRig;

    FContext context;

    // Use this for initialization
    void Awake()
    {
        // restore any settings
        SceneGraphConfig.RestorePreferences();

        // don't auto-translate the scene (perhaps this should be the default?)
        SceneGraphConfig.InitialSceneTranslate = Vector3f.Zero;

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


        // wrap existing complex GameObject named capsule1 as a SceneObject
        GameObject capsuleGO = GameObject.Find("capsule1");
        TransformableSO capsuleSO = UnitySceneUtil.WrapAnyGameObject(capsuleGO, context, true);

        // wrap a prefab as a GOWrapperSO
        GameObject prefabGO = GameObject.Find("bunny_prefab");
        /*TransformableSO prefabSO =*/ UnitySceneUtil.WrapAnyGameObject(prefabGO, context, false);

        // convert a mesh GameObject to our DMeshSO
        // Note: any child GameObjects will be lost
        GameObject meshGO = GameObject.Find("bunny_mesh");
        UnitySceneUtil.WrapMeshGameObject(meshGO, context, true);
        //DMeshSO meshSO = UnitySceneUtil.WrapMeshGameObject(meshGO, context, true) as DMeshSO;



        // center the camera on the capsule assembly
        Vector3f centerPt = capsuleSO.GetLocalFrame(CoordSpace.WorldCoords).Origin;
        context.ActiveCamera.Manipulator().ScenePanFocus(
            context.Scene, context.ActiveCamera, centerPt, true);

    }

    // Update is called once per frame
    void Update()
    {
        context.Update();
    }



}