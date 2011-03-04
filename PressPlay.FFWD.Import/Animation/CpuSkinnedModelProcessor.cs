#region File Description
//-----------------------------------------------------------------------------
// CpuSkinnedModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using PressPlay.FFWD.SkinnedModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;

namespace PressPlay.FFWD.Import.Animation
{
    internal class CpuSkinnedModelProcessor : ContentProcessor<NodeContent, CpuSkinnedModelContent>
    {
        public CpuSkinnedModelProcessor()
        {
            Scale = 1.0f;
        }

        private ContentProcessorContext context;
        private CpuSkinnedModelContent outputModel;

        [DefaultValue(1.0f)]
        [Description("The scale of the model in the game.")]
        public float Scale { get; set; }

        [DisplayName("Rotation X")]
        [DefaultValue(0.0f)]
        [Description("The rotation of the model in the game.")]
        public float RotationX { get; set; }
        [DisplayName("Rotation Y")]
        [DefaultValue(0.0f)]
        [Description("The rotation of the model in the game.")]
        public float RotationY { get; set; }
        [DisplayName("Rotation Z")]
        [DefaultValue(0.0f)]
        [Description("The rotation of the model in the game.")]
        public float RotationZ { get; set; }

        // A single material may be reused on more than one piece of geometry.
        // This dictionary keeps track of materials we have already converted,
        // to make sure we only bother processing each of them once.
        Dictionary<MaterialContent, MaterialContent> processedMaterials = new Dictionary<MaterialContent, MaterialContent>();

        public override CpuSkinnedModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            this.context = context;
            outputModel = new CpuSkinnedModelContent();

            // cpu skinning can support any number of bones, so we'll just use int.MaxValue as our limit.
            outputModel.Scale = Scale;
            outputModel.Rotation = new Vector3(RotationX, RotationY, RotationZ);
            outputModel.SkinningData = SkinningHelpers.GetSkinningData(input, context, int.MaxValue);

            ProcessNode(input);

            return outputModel;
        }

        void ProcessNode(NodeContent node)
        {
            // Is this node in fact a mesh?
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Reorder vertex and index data so triangles will render in
                // an order that makes efficient use of the GPU vertex cache.
                MeshHelper.OptimizeForCache(mesh);

                // Process all the geometry in the mesh.
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    ProcessGeometry(node.Name, geometry);
                }
            }

            // Recurse over any child nodes.
            foreach (NodeContent child in node.Children)
            {
                ProcessNode(child);
            }
        }

        void ProcessGeometry(string name, GeometryContent geometry)
        {
            // find and process the geometry's bone weights
            for (int i = 0; i < geometry.Vertices.Channels.Count; i++)
            {
                string channelName = geometry.Vertices.Channels[i].Name;
                string baseName = VertexChannelNames.DecodeBaseName(channelName);

                if (baseName == "Weights")
                {
                    ProcessWeightsChannel(geometry, i);
                }
            }

            // retrieve the four vertex channels we require for CPU skinning. we ignore any
            // other channels the model might have.
            string normalName = VertexChannelNames.EncodeName(VertexElementUsage.Normal, 0);
            string texCoordName = VertexChannelNames.EncodeName(VertexElementUsage.TextureCoordinate, 0);
            string blendWeightName = VertexChannelNames.EncodeName(VertexElementUsage.BlendWeight, 0);
            string blendIndexName = VertexChannelNames.EncodeName(VertexElementUsage.BlendIndices, 0);

            VertexChannel<Microsoft.Xna.Framework.Vector3> normals = geometry.Vertices.Channels[normalName] as VertexChannel<Microsoft.Xna.Framework.Vector3>;
            VertexChannel<Microsoft.Xna.Framework.Vector2> texCoords = geometry.Vertices.Channels[texCoordName] as VertexChannel<Microsoft.Xna.Framework.Vector2>;
            VertexChannel<Vector4> blendWeights = geometry.Vertices.Channels[blendWeightName] as VertexChannel<Vector4>;
            VertexChannel<Vector4> blendIndices = geometry.Vertices.Channels[blendIndexName] as VertexChannel<Vector4>;

            // create our array of vertices
            int triangleCount = geometry.Indices.Count / 3;
            CpuVertex[] vertices = new CpuVertex[geometry.Vertices.VertexCount];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new CpuVertex
                {
                    Position = geometry.Vertices.Positions[i],
                    Normal = normals[i],
                    TextureCoordinate = texCoords[i],
                    BlendWeights = blendWeights[i],
                    BlendIndices = blendIndices[i]
                };
            }

            BoundingSphere sphere = BoundingSphere.CreateFromPoints(geometry.Vertices.Positions);

            // Convert the input material.
            MaterialContent material = ProcessMaterial(geometry.Material);

            // Add the new piece of geometry to our output model.
            outputModel.AddModelPart(name, triangleCount, geometry.Indices, vertices, material as BasicMaterialContent, sphere);
        }

        static void ProcessWeightsChannel(GeometryContent geometry, int vertexChannelIndex)
        {
            // create a map of Name->Index of the bones
            BoneContent skeleton = MeshHelper.FindSkeleton(geometry.Parent);
            Dictionary<string, int> boneIndices = new Dictionary<string, int>();
            IList<BoneContent> flattenedBones = MeshHelper.FlattenSkeleton(skeleton);
            for (int i = 0; i < flattenedBones.Count; i++)
            {
                boneIndices.Add(flattenedBones[i].Name, i);
            }

            // convert all of our bone weights into the correct indices and weight values
            VertexChannel<BoneWeightCollection> inputWeights = geometry.Vertices.Channels[vertexChannelIndex] as VertexChannel<BoneWeightCollection>;
            Vector4[] outputIndices = new Vector4[inputWeights.Count];
            Vector4[] outputWeights = new Vector4[inputWeights.Count];
            for (int i = 0; i < inputWeights.Count; i++)
            {
                ConvertWeights(inputWeights[i], boneIndices, outputIndices, outputWeights, i, geometry);
            }

            // create our new channel names
            int usageIndex = VertexChannelNames.DecodeUsageIndex(inputWeights.Name);
            string indicesName = VertexChannelNames.EncodeName(VertexElementUsage.BlendIndices, usageIndex);
            string weightsName = VertexChannelNames.EncodeName(VertexElementUsage.BlendWeight, usageIndex);

            // add in the index and weight channels
            geometry.Vertices.Channels.Insert(vertexChannelIndex + 1, indicesName, outputIndices);
            geometry.Vertices.Channels.Insert(vertexChannelIndex + 2, weightsName, outputWeights);

            // remove the original weights channel
            geometry.Vertices.Channels.RemoveAt(vertexChannelIndex);
        }

        static void ConvertWeights(BoneWeightCollection inputWeights, Dictionary<string, int> boneIndices, Vector4[] outIndices, Vector4[] outWeights, int vertexIndex, GeometryContent geometry)
        {
            // we only handle 4 weights per bone
            const int maxWeights = 4;

            // create some temp arrays to hold our values
            int[] tempIndices = new int[maxWeights];
            float[] tempWeights = new float[maxWeights];

            // cull out any extra bones
            inputWeights.NormalizeWeights(maxWeights);

            // get our indices and weights
            for (int i = 0; i < inputWeights.Count; i++)
            {
                BoneWeight weight = inputWeights[i];
                if (boneIndices.ContainsKey(weight.BoneName))
                {
                    tempIndices[i] = boneIndices[weight.BoneName];
                    tempWeights[i] = weight.Weight;
                }
            }

            // zero out any remaining spaces
            for (int i = inputWeights.Count; i < maxWeights; i++)
            {
                tempIndices[i] = 0;
                tempWeights[i] = 0;
            }

            // output the values
            outIndices[vertexIndex] = new Vector4(tempIndices[0], tempIndices[1], tempIndices[2], tempIndices[3]);
            outWeights[vertexIndex] = new Vector4(tempWeights[0], tempWeights[1], tempWeights[2], tempWeights[3]);
        }

        MaterialContent ProcessMaterial(MaterialContent material)
        {
            if (material == null)
            {
                return new BasicMaterialContent();
            }

            // Have we already processed this material?
            if (!processedMaterials.ContainsKey(material))
            {
                // If not, process it now.
                processedMaterials[material] = context.Convert<MaterialContent, MaterialContent>(material, "MaterialProcessor");
            }

            return processedMaterials[material];
        }
    }
}