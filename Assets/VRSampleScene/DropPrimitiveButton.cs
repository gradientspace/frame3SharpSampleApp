using System;
using System.Collections.Generic;
using UnityEngine;
using g3;

namespace f3
{
    //
    // This class extends a standard HUDButton with a drag action that drops the input
    //  pivot in the scene
    //
    public class DropPrimitiveButton : HUDButton
    {
        public FScene TargetScene { get; set; }
        public Func<SceneObject> CreatePrimitive { get; set; }

        public DropPrimitiveButton()
        {
        }


        // creates a button with a floating primitive in front of the button shape
        public void Create(float fRadius, Material bgMaterial)
        {
            Shape = new HUDShape(HUDShapeType.Disc, fRadius );
            base.Create(bgMaterial);
        }


        enum CaptureState
        {
            ClickType,
            DragType
        }
        CaptureState eState;

        Frame3f lastHitF;
        SceneObject lastHitObject;
        SceneObject newPrimitive;
        float fPrimShift;
        float fPrimScale;

        void update_position(AnyRayHit hit)
        {
            int nNormalAxis = 1;
            int nUpAxis = 2;

            // as we drag object we will align Y with hit surface normal, but
            // we also want to constrain rotation so it is stable. Hence, we are
            // going to use world or local frame of target object to stabilize
            // rotation around normal. 
            Frame3f hitF = TargetScene.SceneFrame;
            Vector3 targetAxis = hitF.GetAxis(1);
            if (hit.hitSO is SceneObject)
                hitF = (hit.hitSO as SceneObject).GetLocalFrame(CoordSpace.WorldCoords);
            bool bUseLocal = 
                (TargetScene.Context.TransformManager.ActiveFrameType == FrameType.LocalFrame);
            if (bUseLocal && hit.hitSO is SceneObject) {
                hitF = (hit.hitSO as SceneObject).GetLocalFrame(CoordSpace.WorldCoords);
                targetAxis = hitF.GetAxis(1);
            }
            // if normal is parallel to target, this would become unstable, so use another axis
            if (Vector3.Dot(targetAxis, hit.hitNormal) > 0.99f)
                targetAxis = hitF.GetAxis(0);

            if ( lastHitObject == null || hit.hitSO != lastHitObject ) {
                lastHitF = new Frame3f(hit.hitPos, hit.hitNormal, nNormalAxis);
                lastHitF.ConstrainedAlignAxis(nUpAxis, targetAxis, lastHitF.GetAxis(nNormalAxis));
            } else {
                lastHitF.Origin = hit.hitPos;
                lastHitF.AlignAxis(nNormalAxis, hit.hitNormal);
                lastHitF.ConstrainedAlignAxis(nUpAxis, targetAxis, lastHitF.GetAxis(nNormalAxis));
            }
            lastHitObject = hit.hitSO;
        }

        override public bool WantsCapture(InputEvent e)
        {
            return (Enabled && HasGO(e.hit.hitGO));
        }

        override public bool BeginCapture(InputEvent e)
        {
            eState = CaptureState.ClickType;
            lastHitObject = null;
            return true;
        }

        override public bool UpdateCapture(InputEvent e)
        {
            if (eState == CaptureState.ClickType && FindHitGO(e.ray) != null)
                return true;

            // otherwise we fall into drag state
            eState = CaptureState.DragType;

            if (newPrimitive == null) {
                newPrimitive = CreatePrimitive();

                fPrimScale = 1.0f;

                if (newPrimitive is PivotSO) {
                    fPrimShift = 0.0f;
                } else {
                    if (newPrimitive is PrimitiveSO) {
                        if (SavedSettings.Restore("DropPrimButton_scale") != null) {
                            fPrimScale = (float)SavedSettings.Restore("DropPrimButton_scale");
                            newPrimitive.SetLocalScale(fPrimScale * Vector3f.One);
                        }
                    }
                    fPrimShift = newPrimitive.GetLocalBoundingBox().Extents[1] * TargetScene.GetSceneScale();
                }

                // [RMS] this is kind of cheating - we are going to tell this SO
                //   it is part of the scene, but not actually put it in the scene.
                //   This is because sometimes the SO needs to query the scene/camera
                //   (eg for pivot resizing)
                TargetScene.ReparentSceneObject(newPrimitive, false);
                newPrimitive.SetScene(TargetScene);

                lastHitF = UnityUtil.GetGameObjectFrame(TargetScene.RootGameObject, CoordSpace.WorldCoords);
                newPrimitive.SetLocalFrame(lastHitF.Translated(fPrimShift,1), CoordSpace.WorldCoords);
            }

            // [RMS] only Touch for this??
            if ( InputState.IsDevice(e.device, InputDevice.OculusTouch) && newPrimitive is PrimitiveSO) {
                Vector2f vStick = e.input.StickDelta2D((int)e.side);
                if (vStick[1] != 0) {
                    fPrimScale = fPrimScale * (1.0f + vStick[1] * 0.1f);
                    fPrimScale = MathUtil.Clamp(fPrimScale, 0.01f, 10.0f);
                    newPrimitive.SetLocalScale(fPrimScale * Vector3f.One);
                    fPrimShift = newPrimitive.GetLocalBoundingBox().Extents[1] * TargetScene.GetSceneScale();
                }
            }

            AnyRayHit hit = null;
            if (TargetScene.FindSceneRayIntersection(e.ray, out hit)) {
                update_position(hit);
                newPrimitive.SetLocalFrame(lastHitF.Translated(fPrimShift, 1), CoordSpace.WorldCoords);
            }

            // [RMS] have to do this because prim is not part of scene yet,
            //   and things like pivots need to be resized
            if ( newPrimitive != null )
                newPrimitive.PreRender();

            return true;
        }


        override public bool EndCapture(InputEvent e)
        {
            if (eState == CaptureState.ClickType)
                return base.EndCapture(e);

            if (newPrimitive is PrimitiveSO)
                SavedSettings.Save("DropPrimButton_scale", fPrimScale);

            // store undo/redo record for new primitive
            TargetScene.History.PushChange(
                new AddSOChange() { scene = TargetScene, so = newPrimitive, bKeepWorldPosition = true });
            TargetScene.History.PushInteractionCheckpoint();
            newPrimitive = null;

            return true;
        }

    }
}
