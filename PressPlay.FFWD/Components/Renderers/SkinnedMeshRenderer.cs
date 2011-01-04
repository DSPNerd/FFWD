﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PressPlay.FFWD.Components
{
    public class SkinnedMeshRenderer : Renderer
    {

        public Mesh sharedMesh { get; set; }

        private Animation animation;

        public override void Awake()
        {
            base.Awake();
            if (sharedMesh != null)
            {
                sharedMesh.Awake();
            }
        }

        public override void Start()
        {
            base.Start();
            if (sharedMesh != null)
            {
                sharedMesh.Start();
            }

            // Create animation players/clips for the rigid model
            ModelData modelData = sharedMesh.model.Tag as ModelData;
            animation = GetComponentInParents<Animation>();
            if (modelData != null)
            {
                if (modelData.ModelAnimationClips != null)
                {
                    animation.Initialize(modelData);
                }
            }
        }

        #region IRenderable Members
        public override void Draw(GraphicsDevice device, Camera cam)
        {
            if (sharedMesh == null || sharedMesh.model == null)
            {
                return;
            }
            
            // Do we have negative scale - if so, switch culling
            RasterizerState oldRaster = device.RasterizerState;
            BlendState oldBlend = device.BlendState;
            SamplerState oldSample = device.SamplerStates[0];
            if (transform.lossyScale.x < 0 || transform.lossyScale.y < 0 || transform.lossyScale.z < 0)
            {
                device.RasterizerState = new RasterizerState() { FillMode = oldRaster.FillMode, CullMode = CullMode.CullClockwiseFace };
            }
            if (material.IsAdditive())
            {
                device.BlendState = BlendState.Additive;
                device.SamplerStates[0] = SamplerState.LinearClamp;
            }

            // Draw the model.


            ModelMesh mesh = sharedMesh.GetModelMesh();
            for (int e = 0; e < mesh.Effects.Count; e++)
            {
                Matrix[] boneTransforms = null;
                if (animation != null)
                {
                    boneTransforms = animation.GetTransforms();
                }

                SkinnedEffect sEffect = mesh.Effects[e] as SkinnedEffect;
                if (sEffect != null)
                {
                    if (boneTransforms != null)
                    {
                        sEffect.SetBoneTransforms(boneTransforms);
                    }
                    sEffect.World = Matrix.CreateScale(0.01f) * transform.world;
                    sEffect.View = cam.View();
                    sEffect.Projection = cam.projectionMatrix;
                    sEffect.AmbientLightColor = new Vector3(1);
                    if (material.texture != null)
                    {
                        sEffect.Texture = material.texture;
                    }
                    mesh.Draw();
                }
            }

            if (transform.lossyScale.x < 0 || transform.lossyScale.y < 0 || transform.lossyScale.z < 0)
            {
                device.RasterizerState = oldRaster;
            }
            if (material.IsAdditive())
            {
                device.BlendState = oldBlend;
                device.SamplerStates[0] = oldSample;
            }
        }
        #endregion
    }
}
