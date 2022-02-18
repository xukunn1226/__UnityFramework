﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI.Runtime
{
    public class NoDrawingRaycast : Graphic
    {
        protected NoDrawingRaycast()
        {
            useLegacyMeshGeneration = false;
        }

        public override void SetMaterialDirty()
        {}

        public override void SetVerticesDirty()
        {}

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}