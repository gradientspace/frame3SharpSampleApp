using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using f3;
using g3;

class SetupBasicVRCockpit : ICockpitInitializer
{


    public void Initialize(Cockpit cockpit)
    {
        cockpit.Name = "sampleCockpit";


        // Configure how the cockpit moves

        cockpit.PositionMode = Cockpit.MovementMode.TrackPosition;
        // [RMS] use orientation mode to make cockpit follow view orientation.
        //  (however default widgets below are off-screen!)
        //cockpit.PositionMode = Cockpit.MovementMode.TrackOrientation;


        // Add some UI elements to the cockpit 
        //   - cylinder & box buttons - double-click or drag/drop into scene
        //   - button to start and cancel draw-primitives tool


        float fHUDRadius = 1.0f;
        Color bgColor = new Color(0.7f, 0.7f, 1.0f, 0.7f);
        Material bgMaterial = (bgColor.a == 1.0f) ?
            MaterialUtil.CreateStandardMaterial(bgColor) : MaterialUtil.CreateTransparentMaterial(bgColor);
        Material primMaterial = MaterialUtil.CreateStandardMaterial(Color.yellow);

        float fButtonsY = -50.0f; // degrees
        float fButtonsSpacing = 15.0f;
        float fPrimitivesX = -35.0f; 

        DropPrimitiveButton cylinderButton =
            add_primitive_button(cockpit, "create_cylinder", fHUDRadius, fPrimitivesX, fButtonsY, 
                PrimitiveType.Cylinder, SOTypes.Cylinder, 0.7f, bgMaterial, primMaterial,
                () => { return new CylinderSO().Create(cockpit.Scene.DefaultSOMaterial); });
        cockpit.AddUIElement(cylinderButton);

        DropPrimitiveButton boxButton =
            add_primitive_button(cockpit, "create_box", fHUDRadius, fPrimitivesX+fButtonsSpacing, fButtonsY,
                PrimitiveType.Cube, SOTypes.Box, 0.8f, bgMaterial, primMaterial,
                () => { return new BoxSO().Create(cockpit.Scene.DefaultSOMaterial); });
        cockpit.AddUIElement(boxButton);



        float fToolsX = 35.0f;
        float fToolButtonRadius = 0.08f;

        // buttons for draw-primitive tool and cancel-tool button
        
        ActivateToolButton drawPrimButton = add_tool_button(cockpit, DrawPrimitivesTool.Identifier, fHUDRadius,
            fToolsX - fButtonsSpacing, fButtonsY, fToolButtonRadius, bgMaterial, primMaterial,
            new toolInfo() { identifier = DrawPrimitivesTool.Identifier, sMeshPath = "draw_primitive", fMeshScaleFudge = 1.2f });
        cockpit.AddUIElement(drawPrimButton);

        ActivateToolButton cancelButton = add_tool_button(cockpit, "cancel", fHUDRadius,
            fToolsX, fButtonsY, fToolButtonRadius, bgMaterial, primMaterial,
            new toolInfo() { identifier = "cancel", sMeshPath = "cancel", fMeshScaleFudge = 1.2f });
        cockpit.AddUIElement(cancelButton);



        // Configure interaction behaviors
        //   - below we add behaviors for mouse, gamepad, and spatial devices (oculus touch, etc)
        //   - keep in mind that Tool objects will register their own behaviors when active

        // setup key handlers (need to move to behavior...)
        cockpit.AddKeyHandler(new SampleKeyHandler(cockpit.Context));

        // these behaviors let us interact with UIElements (ie left-click/trigger, or either triggers for Touch)
        cockpit.InputBehaviors.Add(new VRMouseUIBehavior(cockpit.Context) { Priority = 0 });
        cockpit.InputBehaviors.Add(new VRGamepadUIBehavior(cockpit.Context) { Priority = 0 });
        cockpit.InputBehaviors.Add(new VRSpatialDeviceUIBehavior(cockpit.Context) { Priority = 0 });

        // spatial device does camera manipulation via Behavior
        //   (mouse/gamepad currently do not, but will in future!)
        cockpit.InputBehaviors.Add(new SpatialDeviceViewManipBehavior(cockpit) { Priority = 2 });
        cockpit.InputBehaviors.Add(new SpatialDeviceGrabBehavior(cockpit) { Priority = 3 });

        // selection / multi-selection behaviors
        cockpit.InputBehaviors.Add(new MouseMultiSelectBehavior(cockpit.Context) { Priority = 10 });
        cockpit.InputBehaviors.Add(new GamepadMultiSelectBehavior(cockpit.Context) { Priority = 10 });
        cockpit.InputBehaviors.Add(new SpatialDeviceMultiSelectBehavior(cockpit.Context) { Priority = 10 });

        // de-selection behaviors
        cockpit.InputBehaviors.Add(new MouseDeselectBehavior(cockpit.Context) { Priority = 999 });
        cockpit.InputBehaviors.Add(new GamepadDeselectBehavior(cockpit.Context) { Priority = 999 });
        cockpit.InputBehaviors.Add(new SpatialDeviceDeselectBehavior(cockpit.Context) { Priority = 999 });

    }







    class primitiveIconGenerator : IGameObjectGenerator
    {
        public PrimitiveType PrimType { get; set; }
        public Material PrimMaterial { get; set; }
        public float PrimSize { get; set; }
        virtual public List<GameObject> Generate()
        {
            GameObject primGO = UnityUtil.CreatePrimitiveGO("primitive", PrimType, PrimMaterial, true);
            primGO.transform.localScale = new Vector3(PrimSize, PrimSize, PrimSize);
            primGO.transform.Translate(0.0f, 0.0f, -PrimSize);
            primGO.transform.Rotate(-15.0f, 45.0f, 0.0f, Space.Self);
            return new List<GameObject>() { primGO };
        }
    }


    DropPrimitiveButton add_primitive_button(Cockpit cockpit, string sName, float fHUDRadius, float dx, float dy,
        PrimitiveType primType, SOType soType, float fPrimRadiusScale,
        Material bgMaterial, Material primMaterial,
        Func<TransformableSO> CreatePrimitiveF,
        IGameObjectGenerator customGenerator = null
        )
    {
        float fButtonRadius = 0.08f;

        DropPrimitiveButton button = new DropPrimitiveButton() {
            TargetScene = cockpit.Scene,
            CreatePrimitive = CreatePrimitiveF
        };
        button.Create(fButtonRadius, bgMaterial);
        var gen = (customGenerator != null) ? customGenerator :
            new primitiveIconGenerator() { PrimType = primType, PrimMaterial = primMaterial, PrimSize = fButtonRadius * fPrimRadiusScale };
        button.AddVisualElements(gen.Generate(), true);
        HUDUtil.PlaceInSphere(button, fHUDRadius, dx, dy);
        button.Name = sName;
        button.OnClicked += (s, e) => {
            cockpit.Scene.DefaultPrimitiveType = soType;
        };
        button.OnDoubleClicked += (s, e) => {
            // [TODO] could have a lighter record here because we can easily recreate primitive...
            cockpit.Scene.History.PushChange(
                new AddSOChange() { scene = cockpit.Scene, so = CreatePrimitiveF() });
            cockpit.Scene.History.PushInteractionCheckpoint();
        };
        return button;
    }



    struct toolInfo
    {
        public string identifier;
        public string sMeshPath;
        public float fMeshScaleFudge;
    }
    ActivateToolButton[] activeButtons = new ActivateToolButton[2];

    ActivateToolButton add_tool_button(Cockpit cockpit, string sName,
        float fHUDRadius, float dx, float dy,
        float fButtonRadius,
        Material bgMaterial, Material activeMaterial, toolInfo info)
    {
        ActivateToolButton button = new ActivateToolButton() {
            TargetScene = cockpit.Scene,
            ToolType = info.identifier
        };
        button.CreateMeshIconButton(fButtonRadius, info.sMeshPath, bgMaterial, info.fMeshScaleFudge);
        HUDUtil.PlaceInSphere(button, fHUDRadius, dx, dy);
        button.Name = sName;

        if (info.identifier == "cancel") {
            button.OnClicked += (s, e) => {
                int nSide = InputState.IsHandedDevice(e.device) ? (int)e.side : 1;
                cockpit.Context.ToolManager.DeactivateTool((ToolSide)nSide);
                if (activeButtons[nSide] != null) {
                    activeButtons[nSide].SetBackgroundMaterial(bgMaterial);
                    activeButtons[nSide] = null;
                }
            };
        } else {
            button.OnClicked += (s, e) => {
                int nSide = InputState.IsHandedDevice(e.device) ? (int)e.side : 1;
                cockpit.Context.ToolManager.SetActiveToolType(info.identifier, (ToolSide)nSide);
                cockpit.Context.ToolManager.ActivateTool((ToolSide)nSide);
                if (activeButtons[nSide] != null)
                    activeButtons[nSide].SetBackgroundMaterial(bgMaterial);
                activeButtons[nSide] = button;
                button.SetBackgroundMaterial(activeMaterial);
            };
        }
        return button;
    }









    public class SampleKeyHandler : IShortcutKeyHandler
    {
        FContext context;
        public SampleKeyHandler(FContext c)
        {
            context = c;
        }
        public bool HandleShortcuts()
        {
            bool bShiftDown = Input.GetKey(KeyCode.LeftShift);
            bool bCtrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            // ESCAPE CLEARS ACTIVE TOOL OR SELECTION
            if (Input.GetKeyUp(KeyCode.Escape)) {
                if (context.ToolManager.HasActiveTool(0) || context.ToolManager.HasActiveTool(1)) {
                    context.ToolManager.DeactivateTool(0);
                    context.ToolManager.DeactivateTool(1);
                } else if (context.Scene.Selected.Count > 0) {
                    context.Scene.ClearSelection();
                }
                return true;


                // CENTER TARGET (??)
            } else if (Input.GetKeyUp(KeyCode.C)) {
                Ray3f cursorRay = context.MouseController.CurrentCursorWorldRay();
                AnyRayHit hit = null;
                if (context.Scene.FindSceneRayIntersection(cursorRay, out hit)) {
                    context.ActiveCamera.Manipulator().ScenePanFocus(context.Scene, context.ActiveCamera, hit.hitPos, true);
                }
                return true;

                // TOGGLE FRAME TYPE
            } else if (Input.GetKeyUp(KeyCode.F)) {
                FrameType eCur = context.TransformManager.ActiveFrameType;
                context.TransformManager.ActiveFrameType = (eCur == FrameType.WorldFrame)
                    ? FrameType.LocalFrame : FrameType.WorldFrame;
                return true;

                // DROP A COPY
            } else if (Input.GetKeyUp(KeyCode.D)) {
                foreach (SceneObject so in context.Scene.Selected) {
                    SceneObject copy = so.Duplicate();
                    if (copy != null) {
                        // [TODO] could have a lighter record here because we can just re-run Duplicate() ?
                        context.Scene.History.PushChange(
                            new AddSOChange() { scene = context.Scene, so = copy });
                        context.Scene.History.PushInteractionCheckpoint();
                    }
                }
                return true;

                // VISIBILITY  (V HIDES, SHIFT+V SHOWS)
            } else if (Input.GetKeyUp(KeyCode.V)) {
                // show/hide (should be abstracted somehow?? instead of directly accessing GOs?)
                if (bShiftDown) {
                    foreach (SceneObject so in context.Scene.SceneObjects)
                        so.RootGameObject.Show();
                } else {
                    foreach (SceneObject so in context.Scene.Selected)
                        so.RootGameObject.Hide();
                    context.Scene.ClearSelection();
                }
                return true;

                // UNDO
            } else if (bCtrlDown && Input.GetKeyUp(KeyCode.Z)) {
                context.Scene.History.InteractiveStepBack();
                return true;

                // REDO
            } else if (bCtrlDown && Input.GetKeyUp(KeyCode.Y)) {
                context.Scene.History.InteractiveStepForward();
                return true;

 

                // APPLY CURRENT TOOL IF POSSIBLE
            } else if (Input.GetKeyUp(KeyCode.Return)) {
                if (((context.ActiveInputDevice & InputDevice.Mouse) != 0)
                    && context.ToolManager.HasActiveTool(ToolSide.Right)
                    && context.ToolManager.ActiveRightTool.CanApply) {
                    context.ToolManager.ActiveRightTool.Apply();
                }
                return true;

             } else if (bCtrlDown && Input.GetKeyUp(KeyCode.M)) {
                List<SceneObject> selected = new List<SceneObject>(context.Scene.Selected);

                // combine two selected SOs into a GroupSO
                if (selected.Count == 2)
                    SceneUtil.CreateGroupSO(selected[0] as TransformableSO, selected[1] as TransformableSO);

                // [RMS] alternative, does deep-copy of internal GOs. This works OK except
                //   that the frame of the combined object ends up at the origin...
                //if (selected.Count == 2)
                //    SceneUtil.CombineAnySOs(selected[0], selected[1]);
                return true;

            } else if (Input.GetKeyUp(KeyCode.M)) {
                // combine two DMeshSOs into a Dnew MeshSO
                List<SceneObject> selected = new List<SceneObject>(context.Scene.Selected);
                if (selected.Count == 2 && selected[0] is DMeshSO && selected[1] is DMeshSO)
                    SceneUtil.AppendMeshSO(selected[0] as DMeshSO, selected[1] as DMeshSO);
                return true;

            } else
                return false;
        }
    }

}

