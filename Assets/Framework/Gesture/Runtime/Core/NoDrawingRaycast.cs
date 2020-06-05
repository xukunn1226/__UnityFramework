using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Gesture.Runtime
{
    public class NoDrawingRaycast : Graphic
    {
        public override void SetMaterialDirty()
        {}

        public override void SetVerticesDirty()
        {}

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);
        }
    }
}