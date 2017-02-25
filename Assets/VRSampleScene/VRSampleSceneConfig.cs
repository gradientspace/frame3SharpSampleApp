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

        // set Scene object in our scripts because I can't seem to do it in Inspector anymore??
        // perhaps we could do the script part of these in code, then 
        // we would just need to know the object names...
        GameObject lighting = GameObject.Find("SceneLighting");
        lighting.GetComponent<SceneLightingSetup>().Scene = context;


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
        TransformableSO prefabSO = UnitySceneUtil.WrapAnyGameObject(prefabGO, context, false);

        // convert a mesh GameObject to our DMeshSO
        // Note: any child GameObjects will be lost
        GameObject meshGO = GameObject.Find("bunny_mesh");
        DMeshSO meshSO = UnitySceneUtil.WrapMeshGameObject(meshGO, context, true) as DMeshSO;



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