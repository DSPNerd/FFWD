﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PressPlay.FFWD;
using PressPlay.FFWD.SkinnedModel;

namespace PressPlay.FFWD.Components
{
    public class SkinnedMeshRenderer : Renderer
    {
        public Mesh sharedMesh;

        private Animation animation;

        public override void Awake()
        {
            base.Awake();
            animation = GetComponentInParents<Animation>();
        }

        #region IRenderable Members
        public override int Draw(GraphicsDevice device, Camera cam)
        {
            if (sharedMesh == null || sharedMesh.skinnedModel == null)
            {
                return 0;
            }

            // Check the frustum of the camera
            BoundingSphere sphere = new BoundingSphere(transform.position, sharedMesh.boundingSphere.Radius * transform.lossyScale.sqrMagnitude);
            if (cam.DoFrustumCulling(ref sphere))
            {
#if DEBUG
                if (Camera.logRenderCalls)
                {
                    Debug.LogFormat("VP cull {0} with radius {1} pos {2} cam {3} at {4}", gameObject, sharedMesh.boundingSphere.Radius, transform.position, cam.gameObject, cam.transform.position);
                }
#endif
                return 0;
            }

            // Draw the model.
            CpuSkinnedModelPart modelPart = sharedMesh.GetSkinnedModelPart();
            Matrix world = transform.world;
            modelPart.SetBones(animation.GetTransforms(), ref world, sharedMesh);
            return cam.BatchRender(sharedMesh, materials, null);
        }
        #endregion
    }
}