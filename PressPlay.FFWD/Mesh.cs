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
        [ContentSerializer(Optional = true)]
        public string asset { get; set; }

        [ContentSerializerIgnore]
        public Model model; 
        [ContentSerializerIgnore]
        public CpuSkinnedModel skinnedModel;
        private int meshIndex;

        [ContentSerializer(Optional=true)]
        public Microsoft.Xna.Framework.Vector3[] vertices;
        [ContentSerializer(Optional = true)]
        public Microsoft.Xna.Framework.Vector3[] normals;
        [ContentSerializer(Optional = true)]
        public Microsoft.Xna.Framework.Vector2[] uv;
        [ContentSerializer(Optional = true)]
        public short[] triangles;
        private short[][] triangleSets;

        internal BoundingSphere boundingSphere;

        private Bounds? _bounds = null;
        public Bounds bounds
        {
            get
            {
                if (!_bounds.HasValue)
                {
                    _bounds = new Bounds(boundingSphere.Center, new Vector3(boundingSphere.Radius));
                }
                return _bounds.Value;
            }
        }

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
                    }
                    model = data.model;
                    if (model != null)
                    {
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
                            triangleSets = (short[][])part.triangles.Clone();

                            int triCount = 0;
                            int triIndex = 0;
                            for (int i = 0; i < part.triangles.Length; i++)
                            {
                                triCount += part.triangles[i].Length;
                            }
                            triangles = new short[triCount];
                            for (int i = 0; i < part.triangles.Length; i++)
                            {
                                part.triangles[i].CopyTo(triangles, triIndex);
                                triIndex += part.triangles[i].Length;
                            }

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
            triangleSets = null;
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

        public int subMeshCount
        {
            get
            {
                if (triangleSets == null)
                {
                    if (triangles == null)
                    {
                        return 0;
                    }
                    return 1;
                }
                return triangleSets.Length;
            }
        }

        public short[] GetTriangles(int subMeshIndex)
        {
            return triangleSets[subMeshIndex];
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
                clone.triangleSets = (short[][])triangleSets.Clone();
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
