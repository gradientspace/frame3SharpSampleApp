using System;
using System.Collections.Generic;
using UnityEngine;
using g3;

namespace f3
{
    //
    //
    public class ActivateToolButton : HUDButton
    {
        public FScene TargetScene { get; set; }
        public string ToolType { get; set; }

        public ActivateToolButton()
        {
        }


        // creates a button with a floating primitive in front of the button shape
        public void CreateMeshIconButton(float fRadius, string sMeshPath, Material bgMaterial, 
            float fMeshScaleFudge )
        {
            Shape = new HUDShape() { Type = HUDShapeType.Disc, Radius = fRadius };

            Mesh iconmesh = null;
            Material meshMaterial = null;
            try {
                iconmesh = Resources.Load<Mesh>(sMeshPath);
                meshMaterial = MaterialUtil.CreateStandardVertexColorMaterial(Color.white);
            } catch { }
            if ( iconmesh == null ) {
                iconmesh = UnityUtil.GetPrimitiveMesh(PrimitiveType.Sphere);
                meshMaterial = MaterialUtil.CreateStandardMaterial(Color.red);
            }

            float fMeshRadius = fRadius * fMeshScaleFudge;
            base.Create(bgMaterial, iconmesh, meshMaterial, fMeshRadius,
                Frame3f.Identity.Translated(-fMeshRadius * 0.25f, 2).Rotated(-15.0f, 1));
        }


        public void SetBackgroundMaterial(Material m)
        {
            MaterialUtil.SetMaterial(buttonMesh, m);
        }


        override public bool WantsCapture(InputEvent e)
        {
            return (Enabled && HasGO(e.hit.hitGO));
        }

        override public bool BeginCapture(InputEvent e)
        {
            return (Enabled && HasGO(e.hit.hitGO));
        }

        override public bool UpdateCapture(InputEvent e)
        {
            return true;
        }

        override public bool EndCapture(InputEvent e)
        {
            return base.EndCapture(e);
        }

    }
}
