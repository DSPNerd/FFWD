﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PressPlay.FFWD.SkinnedModel;

namespace PressPlay.FFWD
{
    public class Mesh : Asset
    {
        public string asset { get; set; }

        [ContentSerializerIgnore]
        public Model model; 
        [ContentSerializerIgnore]
        public CpuSkinnedModel skinnedModel;
        private int meshIndex;

        [ContentSerializerIgnore]
        public Microsoft.Xna.Framework.Vector3[] vertices;
        [ContentSerializerIgnore]
        public Microsoft.Xna.Framework.Vector3[] normals;
        [ContentSerializerIgnore]
        public Microsoft.Xna.Framework.Vector2[] uv;
        [ContentSerializerIgnore]
        public short[] triangles;

        internal BoundingSphere boundingSphere;

        protected override void DoLoadAsset(AssetHelper assetHelper)
        {
            // TODO: Optimize this by bundling everything into the same structure.
            if (!String.IsNullOrEmpty(asset))
            {
                MeshData data = assetHelper.Load<MeshData>("Models/" + asset);
                if (data != null)
                {
                    boundingSphere = data.boundingSphere;

                    skinnedModel = data.skinnedModel;
                    if (skinnedModel != null)
                    {
                        for (int i = 0; i < skinnedModel.Parts.Count; i++)
                        {
                            if (skinnedModel.Parts[i].name == name)
                            {
                                meshIndex = i;

                                skinnedModel.Parts[i].InitializeMesh(this);

                                break;
                            }
                        }
                        // HACK : We should do something else to get the correct sphere size.
                        //boundingSphere.Radius *= 3.5f;
                    }
                    model = data.model;
                    if (model != null)
                    {
                        Debug.Log("Non batchable mesh:", asset, name);
                        for (int i = 0; i < model.Meshes.Count; i++)
                        {
                            if (model.Meshes[i].Name == name)
                            {
                                meshIndex = i;
                                boundingSphere = model.Meshes[i].BoundingSphere;
                                break;
                            }
                        }
                    }

                    if (data.meshParts.Count > 0)
                    {
                        MeshDataPart part = data.meshParts[name];
                        if (part != null)
                        {
                            vertices = (Microsoft.Xna.Framework.Vector3[])part.vertices.Clone();
                            triangles = (short[])part.triangles.Clone();
                            uv = (Microsoft.Xna.Framework.Vector2[])part.uv.Clone();
                            if (part.normals != null)
                            {
                                normals = (Microsoft.Xna.Framework.Vector3[])part.normals.Clone();
                            }
                            boundingSphere = part.boundingSphere;
                        }
                    }
                }
#if DEBUG
                else
                {
                    Debug.LogWarning("Cannot find a way to load the mesh " + asset);
                }
#endif
            }
        } 

        public void Clear()
        {
            vertices = null;
            normals = null;
            uv = null;
            triangles = null;
        }

        internal ModelMesh GetModelMesh()
        {
            if (model != null)
            {
                return model.Meshes[meshIndex];
            }
            return null;
        }

        public CpuSkinnedModelPart GetSkinnedModelPart()
        {
            if (skinnedModel != null)
            {
                return skinnedModel.Parts[meshIndex];
            }
            return null;
        }

        #region ICloneable Members
        internal override UnityObject Clone()
        {
            Mesh clone = new Mesh();
            clone.skinnedModel = skinnedModel;
            clone.model = model;
            clone.meshIndex = meshIndex;

            if (vertices != null)
            {
                clone.vertices = (Microsoft.Xna.Framework.Vector3[])vertices.Clone();
                clone.triangles = (short[])triangles.Clone();
                clone.uv = (Microsoft.Xna.Framework.Vector2[])uv.Clone();
                if (normals != null)
                {
                    clone.normals = (Microsoft.Xna.Framework.Vector3[])normals.Clone();
                }
            }
            clone.boundingSphere = boundingSphere;
            return clone;
        }
        #endregion

        public override string ToString()
        {
            return String.Format("{0} - {1} ({2})", GetType().Name, asset, GetInstanceID());
        }
    }
}
