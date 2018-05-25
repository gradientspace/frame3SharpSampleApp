using System;
using UnityEngine;
using System.Collections.Generic;
using f3;
using g3;
using gs;

public class ObjectsDemoSceneConfig : BaseSceneConfig
{
    FContext context;
    public override FContext Context { get { return context; } }


    // Use this for initialization
    public override void Awake()
    {
        VRPlatform.VREnabled = false;

        // restore any settings
        SceneGraphConfig.RestorePreferences();

        // set up some defaults
        SceneGraphConfig.DefaultSceneCurveVisualDegrees = 0.5f;
        SceneGraphConfig.DefaultPivotVisualDegrees = 1.5f;
        SceneGraphConfig.DefaultAxisGizmoVisualDegrees = 10.0f;


        SceneOptions options = new SceneOptions();
        options.UseSystemMouseCursor = true;
        options.EnableCockpit = true;
        options.Use2DCockpit = true;
        options.ConstantSize2DCockpit = true;    // you usually want this
        options.EnableTransforms = true;
        options.CockpitInitializer = new SetupObjectsDemoCockpit();

        options.MouseCameraControls = new MayaCameraHotkeys();

        // very verbose
        options.LogLevel = 2;

        context = new FContext();
        context.Start(options);

        // register transformation gizmos
        //context.TransformManager.RegisterGizmoType(AxisTransformGizmo.DefaultName, new AxisTransformGizmoBuilder());
        //context.TransformManager.SetActiveGizmoType(AxisTransformGizmo.DefaultName);

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


        Sphere3Generator_NormalizedCube gen = new Sphere3Generator_NormalizedCube() {
            Radius = 1.0f
        };
        DMeshSO meshSO = new DMeshSO();
        meshSO.Create(gen.Generate().MakeDMesh(), Context.Scene.DefaultMeshSOMaterial);
        Context.Scene.AddSceneObject(meshSO);

        //SceneObject focusSO = null;

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

        //// center the camera on the capsule assembly
        //if (focusSO != null) {
        //    Vector3f centerPt = focusSO.GetLocalFrame(CoordSpace.WorldCoords).Origin;
        //    context.ActiveCamera.Manipulator().ScenePanFocus(
        //        context.Scene, context.ActiveCamera, centerPt, true);
        //}

    }



}