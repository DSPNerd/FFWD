﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using PressPlay.FFWD.Extensions;
using Microsoft.Xna.Framework;

namespace PressPlay.FFWD.Components
{
    internal abstract class RenderItem
    {
        public Material Material;
        public Transform Transform;
        public Bounds? Bounds;
        protected int batches = 0;
        public bool Enabled;
        protected bool UseVertexColor = false;

        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;

#if XBOX
        protected const int MAX_INDEX_BUFFER_SIZE = Int32.MaxValue;
        internal int[] indexData;
#else
        protected const int MAX_INDEX_SIZE = Int16.MaxValue;
        internal short[] indexData;
#endif

        public abstract bool AddMesh(Mesh mesh, Matrix matrix);
        public abstract void Initialize(GraphicsDevice device);

        internal static RenderItem Create(Material material, Mesh mesh, Transform t)
        {
            RenderItem item;

            // TODO: Select actural render item based on what material we are using
            item = new RenderItem<VertexPositionNormalTexture>(material, AddVertexPositionNormalTexture);

            item.Transform = t;
            item.AddMesh(mesh, t.world);

            return item;
        }

        public void Render(GraphicsDevice device, Camera cam)
        {
            device.SetVertexBuffer(VertexBuffer);
            device.Indices = IndexBuffer;

            Effect e = Material.shader.effect;
            Material.shader.ApplyPreRenderSettings(Material, UseVertexColor);
            Material.SetBlendState(device);

            IEffectMatrices ems = e as IEffectMatrices;
            if (ems != null)
            {
                ems.World = Transform.world;
                ems.View = cam.view;
                ems.Projection = cam.projectionMatrix;
            }
            foreach (EffectPass pass in e.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    VertexBuffer.VertexCount,
                    0,
                    IndexBuffer.IndexCount / 3
                );
            }
            RenderStats.AddDrawCall(batches, VertexBuffer.VertexCount, IndexBuffer.IndexCount / 3);
        }

        private static VertexPositionNormalTexture AddVertexPositionNormalTexture(Microsoft.Xna.Framework.Vector3 position, Microsoft.Xna.Framework.Vector3 normal, Microsoft.Xna.Framework.Vector2 tex0, Microsoft.Xna.Framework.Vector2 tex1, Microsoft.Xna.Framework.Color c)
        {
            return new VertexPositionNormalTexture(position, normal, tex0);
        }
    }

    /// <summary>
    /// This contains an item that is to be rendered.
    /// </summary>
    internal class RenderItem<T> : RenderItem where T : struct
    {
        internal T[] vertexData;
        private AddVertex addVertex;

        public RenderItem(Material mat, AddVertex addV)
        {
            Material = mat;
            addVertex = addV;
        }

        public delegate T AddVertex(Microsoft.Xna.Framework.Vector3 position, Microsoft.Xna.Framework.Vector3 normal, Microsoft.Xna.Framework.Vector2 tex0, Microsoft.Xna.Framework.Vector2 tex1, Microsoft.Xna.Framework.Color c);

        /// <summary>
        /// Adds a mesh to be rendered by this render item.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public override bool AddMesh(Mesh mesh, Matrix matrix)
        {
            if (!mesh._vertices.HasElements())
	        {
                return false;
	        }

            int vertexOffset = 0;
            if (vertexData == null)
            {
                if (mesh.vertices.Length > RenderItem.MAX_INDEX_SIZE)
                {
                    return false;
                }
                vertexData = new T[mesh.vertexCount];
            }
            else
            {
                vertexOffset = vertexData.Length;
                if (mesh.vertices.Length + vertexOffset > RenderItem.MAX_INDEX_SIZE)
                {
                    return false;
                }
                T[] oldVerts = vertexData;
                vertexData = new T[vertexOffset + mesh.vertexCount];
                oldVerts.CopyTo(vertexData, 0);
            }
            batches++;

            Microsoft.Xna.Framework.Vector3[] transformedVertices = new Microsoft.Xna.Framework.Vector3[mesh._vertices.Length];
            Microsoft.Xna.Framework.Vector3.Transform(mesh._vertices, ref matrix, transformedVertices);
            Microsoft.Xna.Framework.Vector3[] transformedNormals = new Microsoft.Xna.Framework.Vector3[mesh._normals.Length];
            Microsoft.Xna.Framework.Vector3.TransformNormal(mesh._normals, ref matrix, transformedNormals);

            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                vertexData[i + vertexOffset] = addVertex(transformedVertices[i], transformedNormals[i], mesh._uv[i], (mesh._uv2.HasElements()) ? mesh._uv2[i] : Microsoft.Xna.Framework.Vector2.Zero, (mesh.colors.HasElements()) ? mesh.colors[i] : Color.white);
            }
            return true;
        }

        /// <summary>
        /// Sets up the VertexBuffers and IndexBuffers with the data.
        /// </summary>
        public override void Initialize(GraphicsDevice device)
        {
            VertexBuffer = new VertexBuffer(device, typeof(T), vertexData.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData<T>(vertexData);

            // Add to the global render queue
            Camera.RenderQueue.Add(Material.shader.renderQueue, this);
        }
    }
}
