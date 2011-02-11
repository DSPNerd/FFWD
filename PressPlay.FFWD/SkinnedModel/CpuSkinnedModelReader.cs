#region File Description
//-----------------------------------------------------------------------------
// CpuSkinnedModelReader.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework;
using PressPlay.FFWD;

namespace PressPlay.FFWD.SkinnedModel
{
    /// <summary>
    /// A custom reader to read our CpuSkinnedModel from an XNB.
    /// </summary>
    public class CpuSkinnedModelReader : ContentTypeReader<CpuSkinnedModel>
    {
        protected override CpuSkinnedModel Read(ContentReader input, CpuSkinnedModel existingInstance)
        {
            // read in the model parts
            List<CpuSkinnedModelPart> modelParts = input.ReadObject<List<CpuSkinnedModelPart>>();

            // read in the skinning data
            SkinningData skinningData = input.ReadObject<SkinningData>();

            CpuSkinnedModel model = new CpuSkinnedModel(modelParts, skinningData);
            model.BakedTransform = input.ReadMatrix();
            model.BoundingSphere = input.ReadObject<BoundingSphere>();

            return model;
        }
    }

    /// <summary>
    /// A custom reader to read a CpuSkinnedModelPart from an XNB.
    /// </summary>
    public class CpuSkinnedModelPartReader : ContentTypeReader<CpuSkinnedModelPart>
    {
        protected override CpuSkinnedModelPart Read(ContentReader input, CpuSkinnedModelPart existingInstance)
        {
            // read in all of our data
            string name = input.ReadString();
            int triangleCount = input.ReadInt32();
            CpuVertex[] cpuVertices = input.ReadObject<CpuVertex[]>();
            IndexBuffer indexBuffer = input.ReadObject<IndexBuffer>();

            // create the model part from this data
            CpuSkinnedModelPart modelPart = new CpuSkinnedModelPart(name, triangleCount, cpuVertices, indexBuffer);

            // read in the BasicEffect as a shared resource
            input.ReadSharedResource<BasicEffect>(fx => modelPart.Effect = fx);

            return modelPart;
        }
    }
}
