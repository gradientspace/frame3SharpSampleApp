using System;
using UnityEngine;
using System.Collections.Generic;
using f3;
using g3;
using System.IO;

public class VRPhotoSceneConfig : BaseSceneConfig
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
        SceneGraphConfig.DefaultPivotVisualDegrees = 1.5f;
        SceneGraphConfig.DefaultAxisGizmoVisualDegrees = 10.0f;
        SceneGraphConfig.InitialSceneTranslate = -4 * Vector3f.AxisY;


        SceneOptions options = new SceneOptions();
        options.UseSystemMouseCursor = false;
        options.Use2DCockpit = false;
        options.EnableTransforms = true;
        options.EnableCockpit = true;
        options.CockpitInitializer = new PhotoToolCockpit();

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
        context.ToolManager.RegisterToolType(DrawSurfaceCurveTool.Identifier, new DrawSurfaceCurveToolBuilder() {
            AttachCurveToSurface = true,
            DefaultSamplingRateS = 0.0025f, DefaultSurfaceOffsetS = 0.0025f,
            CurveMaterialF = () => { var mat = context.Scene.DefaultCurveSOMaterial.Clone(); mat.RGBColor = Colorf.VideoRed; return mat; }
        } );
        context.ToolManager.SetActiveToolType(DrawSurfaceCurveTool.Identifier, ToolSide.Right);

        // Set up standard scene lighting if requested
        if ( options.EnableDefaultLighting ) {
            GameObject lighting = GameObject.Find("SceneLighting");
            if (lighting == null)
                lighting = new GameObject("SceneLighting");
            SceneLightingSetup setup = lighting.AddComponent<SceneLightingSetup>();
            setup.Context = context;
            setup.LightDistance = 30.0f; // related to total scene scale...
        }


        Context.Scene.DisableSelectionMaterial = true;


        /*
         * Import elements of Unity scene that already exist into the FScene
         */

        // set up ground plane geometry (optional)
        GameObject groundPlane = GameObject.Find("GroundPlane");
        if ( groundPlane != null && groundPlane.IsVisible() )
            context.Scene.AddWorldBoundsObject(groundPlane);


        float fSquareSize = 1.0f;
        Vector3f eyePos = context.ActiveCamera.GetPosition();
        System.Random rand = new System.Random(31337);

        // [RMS] this path only works in Editor, is relative to top-level project directory
        string sPhotoFolder = "Data\\PhotoSets\\kitchen";

        string[] photos = Directory.GetFiles(sPhotoFolder);
        foreach ( string filename in photos ) {
            Texture2D tex = load_texture(filename);
            if (tex == null)
                continue;

            float fScale = fSquareSize / (float)tex.width;
            if (tex.height > tex.width)
                fScale = fSquareSize / (float)tex.height;

            float w = fScale * (float)tex.width;
            float h = fScale * (float)tex.height;

            TrivialRectGenerator rectgen = new TrivialRectGenerator() {
                Width = w, Height = h
            };
            rectgen.Generate();
            DMesh3 mesh = new DMesh3(MeshComponents.VertexUVs);
            rectgen.MakeMesh(mesh);

            SOMaterial material = new SOMaterial() {
                Name = "photomaterial",
                Type = SOMaterial.MaterialType.TextureMap,
                RGBColor = Colorf.White
            };
            material.MainTexture = tex;

            DMeshSO so = new DMeshSO();
            so.Create(mesh, material);
            context.Scene.AddSceneObject(so);

            float horz = rand.Next(-50, 50);
            float vert = rand.Next(-20, 25);
            int mult = 1000;
            float dist = (float)(rand.Next(2 * mult, 3 * mult)) / (float)mult;

            Ray3f r = VRUtil.MakeRayFromSphereCenter(horz, vert);
            r.Origin += eyePos;
            float fRayT = 0.0f;
            RayIntersection.Sphere(r.Origin, r.Direction, eyePos, dist, out fRayT);
            Vector3f v = r.Origin + fRayT * r.Direction;
            Frame3f f = new Frame3f(v, v.Normalized);

            Vector3f toEye = context.ActiveCamera.GetPosition() - f.Origin;
            toEye.Normalize();
            f.AlignAxis(1, toEye);
            f.ConstrainedAlignAxis(2, Vector3f.AxisY, f.Y);

            so.SetLocalFrame(f, CoordSpace.WorldCoords);
        }

    }




    static Texture2D load_texture(string sPath)
    {
        if (!File.Exists(sPath)) {
            Debug.Log("[SceneImporter] cannot find image " + sPath);
            return null;
        }

        Texture2D tex;
        try {
            var bytes = File.ReadAllBytes(sPath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
        } catch (Exception e) {
            Debug.Log("[SceneImporter] exception loading texure data from " + sPath + " : " + e.Message);
            return null;
        }
        return tex;
    }




}