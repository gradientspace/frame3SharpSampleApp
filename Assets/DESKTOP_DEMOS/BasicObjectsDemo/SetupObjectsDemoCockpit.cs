using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using f3;
using g3;

class SetupObjectsDemoCockpit : ICockpitInitializer
{
    public void Initialize(Cockpit cockpit)
    {
        cockpit.Name = "sampleCockpit";

        // Configure how the cockpit moves
        cockpit.PositionMode = Cockpit.MovementMode.TrackPosition;


        // initialize layout
        BoxContainer screenContainer = new BoxContainer(new Cockpit2DContainerProvider(cockpit));
        PinnedBoxes2DLayoutSolver screenLayout = new PinnedBoxes2DLayoutSolver(screenContainer);
        PinnedBoxesLayout layout = new PinnedBoxesLayout(cockpit, screenLayout) {
            StandardDepth = 1.0f
        };
        cockpit.AddLayout(layout, "2D", true);



        float pixelScale = cockpit.GetPixelScale();
        float buttonDiam = 150 * pixelScale;

        HUDElementList primitives_list = new HUDElementList() {
            Width = 5*buttonDiam,
            Height = buttonDiam,
            Spacing = 25 * pixelScale,
            Direction = HUDElementList.ListDirection.Horizontal
        };


        // Add some UI elements to the cockpit 
        //   - cylinder & box buttons - double-click or drag/drop into scene
        //   - button to start and cancel draw-primitives tool

        Color bgColor = new Color(0.7f, 0.7f, 1.0f, 0.7f);
        Material bgMaterial = (bgColor.a == 1.0f) ?
            MaterialUtil.CreateStandardMaterial(bgColor) : MaterialUtil.CreateTransparentMaterial(bgColor);
        Material primMaterial = MaterialUtil.CreateStandardMaterial(Color.yellow);

        DropPrimitiveButton cylinderButton =
            create_primitive_button(cockpit, "create_cylinder", buttonDiam / 2,
                PrimitiveType.Cylinder, SOTypes.Cylinder, 0.7f, bgMaterial, primMaterial,
                () => { return new CylinderSO().Create(cockpit.Scene.DefaultSOMaterial); });
        primitives_list.AddListItem(cylinderButton);

        DropPrimitiveButton boxButton =
            create_primitive_button(cockpit, "create_box", buttonDiam / 2,
                PrimitiveType.Cube, SOTypes.Box, 0.8f, bgMaterial, primMaterial,
                () => { return new BoxSO().Create(cockpit.Scene.DefaultSOMaterial); });
        primitives_list.AddListItem(boxButton);


        primitives_list.Create();
        primitives_list.Name = "button_bar";

        // align primitives_list to bottom-left
        layout.Add(primitives_list, new LayoutOptions() {
            Flags = LayoutFlags.None,
            PinSourcePoint2D = LayoutUtil.BoxPointF(primitives_list, BoxPosition.BottomLeft),
            PinTargetPoint2D = LayoutUtil.BoxPointF(screenContainer, BoxPosition.BottomLeft, 25 * pixelScale * Vector2f.One)
        });


        //float fToolsX = 35.0f;
        //float fToolButtonRadius = 0.08f;

        // buttons for draw-primitive tool and cancel-tool button
        
        //ActivateToolButton drawPrimButton = add_tool_button(cockpit, DrawPrimitivesTool.Identifier, fHUDRadius,
        //    fToolsX - fButtonsSpacing, fButtonsY, fToolButtonRadius, bgMaterial, primMaterial,
        //    new toolInfo() { identifier = DrawPrimitivesTool.Identifier, sMeshPath = "draw_primitive", fMeshScaleFudge = 1.2f });
        //cockpit.AddUIElement(drawPrimButton);

        //ActivateToolButton cancelButton = add_tool_button(cockpit, "cancel", fHUDRadius,
        //    fToolsX, fButtonsY, fToolButtonRadius, bgMaterial, primMaterial,
        //    new toolInfo() { identifier = "cancel", sMeshPath = "cancel", fMeshScaleFudge = 1.2f });
        //cockpit.AddUIElement(cancelButton);



        // Configure interaction behaviors
        //   - below we add behaviors for mouse, gamepad, and spatial devices (oculus touch, etc)
        //   - keep in mind that Tool objects will register their own behaviors when active

        // setup key handlers (need to move to behavior...)
        cockpit.AddKeyHandler(new BasicShapesDemo_KeyHandler(cockpit.Context));

        // these behaviors let us interact with UIElements (ie left-click/trigger, or either triggers for Touch)
        cockpit.InputBehaviors.Add(new Mouse2DCockpitUIBehavior(cockpit.Context) { Priority = 0 });
        cockpit.InputBehaviors.Add(new VRMouseUIBehavior(cockpit.Context) { Priority = 1 });

        // selection / multi-selection behaviors
        cockpit.InputBehaviors.Add(new MouseMultiSelectBehavior(cockpit.Context) { Priority = 10 });
        cockpit.InputBehaviors.Add(new GamepadMultiSelectBehavior(cockpit.Context) { Priority = 10 });

        // left click-drag to tumble, and left click-release to de-select
        cockpit.InputBehaviors.Add(new MouseClickDragSuperBehavior() {
            Priority = 100,
            DragBehavior = new MouseViewRotateBehavior(cockpit.Context) { Priority = 100, RotateSpeed = 3.0f },
            ClickBehavior = new MouseDeselectBehavior(cockpit.Context) { Priority = 999 }
        });

        // also right-click-drag to tumble
        cockpit.InputBehaviors.Add(new MouseViewRotateBehavior(cockpit.Context) {
            Priority = 100, RotateSpeed = 3.0f,
            ActivateF = MouseBehaviors.RightButtonPressedF, ContinueF = MouseBehaviors.RightButtonDownF
        });

        // middle-click-drag to pan
        cockpit.InputBehaviors.Add(new MouseViewPanBehavior(cockpit.Context) {
            Priority = 100, PanSpeed = 1.0f,
            ActivateF = MouseBehaviors.MiddleButtonPressedF, ContinueF = MouseBehaviors.MiddleButtonDownF
        });

        cockpit.OverrideBehaviors.Add(new MouseWheelZoomBehavior(cockpit) { Priority = 100, ZoomScale = 10.0f });

        // touch input
        cockpit.InputBehaviors.Add(new TouchUIBehavior(cockpit.Context) { Priority = 1 });
        cockpit.InputBehaviors.Add(new Touch2DCockpitUIBehavior(cockpit.Context) { Priority = 0 });
        cockpit.InputBehaviors.Add(new TouchViewManipBehavior(cockpit.Context) {
            Priority = 999, TouchZoomSpeed = 0.1f, TouchPanSpeed = 0.03f
        });


    }




    class primitiveIconGenerator : IGameObjectGenerator
    {
        public PrimitiveType PrimType { get; set; }
        public Material PrimMaterial { get; set; }
        public float PrimSize { get; set; }
        virtual public List<fGameObject> Generate()
        {
            GameObject primGO = UnityUtil.CreatePrimitiveGO("primitive", PrimType, PrimMaterial, true);
            primGO.transform.localScale = new Vector3(PrimSize, PrimSize, PrimSize);
            primGO.transform.Translate(0.0f, 0.0f, -PrimSize);
            primGO.transform.Rotate(-15.0f, 45.0f, 0.0f, Space.Self);
            return new List<fGameObject>() { primGO };
        }
    }


    DropPrimitiveButton create_primitive_button(Cockpit cockpit, string sName, float fRadius,
        PrimitiveType primType, SOType soType, float fPrimRadiusScale,
        Material bgMaterial, Material primMaterial,
        Func<SceneObject> CreatePrimitiveF,
        IGameObjectGenerator customGenerator = null
        )
    {
        DropPrimitiveButton button = new DropPrimitiveButton() {
            TargetScene = cockpit.Scene,
            CreatePrimitive = CreatePrimitiveF
        };
        button.Create(fRadius, bgMaterial);
        var gen = (customGenerator != null) ? customGenerator :
            new primitiveIconGenerator() { PrimType = primType, PrimMaterial = primMaterial, PrimSize = fRadius * fPrimRadiusScale };
        button.AddVisualElements(gen.Generate(), true);
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









    public class BasicShapesDemo_KeyHandler : IShortcutKeyHandler
    {
        FContext context;
        public BasicShapesDemo_KeyHandler(FContext c)
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
                    context.ActiveCamera.Animator().AnimatePanFocus(hit.hitPos, CoordSpace.WorldCoords, 0.3f);
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
                    SceneUtil.CreateGroupSO(selected[0], selected[1]);

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

