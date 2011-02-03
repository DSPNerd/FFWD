#region File Description
//-----------------------------------------------------------------------------
// AnimationPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PressPlay.FFWD
{
    /// <summary>
    /// The animation player is in charge of decoding bone position
    /// matrices from an animation clip.
    /// 
    /// This class was taken from the original Skinned Model Sample:
    /// http://creators.xna.com/en-US/sample/skinnedmodel 
    /// </summary>
    public class SkinnedAnimationPlayer
    {
        // Information about the currently playing animation clip.
        private AnimationClip currentClipValue;
        private AnimationState currentStateValue;
        private TimeSpan currentTimeValue;
        private int currentKeyframe;
        
        // Current animation transform matrices.
        private Matrix[] boneTransforms;
        private Matrix[] worldTransforms;
        private Matrix[] skinTransforms;
        private Matrix bakedTransform;

        // Backlink to the bind pose and skeleton hierarchy data.
        private SkinningData skinningDataValue;
        
        /// <summary>
        /// Gets the current bone transform matrices, relative to their parent bones.
        /// </summary>
        public Matrix[] BoneTransforms
        {
            get { return boneTransforms; }
        }

        /// <summary>
        /// Gets the current bone transform matrices, in absolute format.
        /// </summary>
        public Matrix[] WorldTransforms
        {
            get { return worldTransforms; }
        }

        /// <summary>
        /// Gets the current bone transform matrices,
        /// relative to the skinning bind pose.
        /// </summary>
        public Matrix[] SkinTransforms
        {
            get { return skinTransforms; }
        }

        /// <summary>
        /// Gets the clip currently being decoded.
        /// </summary>
        public AnimationClip CurrentClip
        {
            get { return currentClipValue; }
        }

        /// <summary>
        /// Gets the current play position.
        /// </summary>
        public TimeSpan CurrentTime
        {
            get { return currentTimeValue; }
        }

        /// <summary>
        /// Constructs a new animation player.
        /// </summary>
        public SkinnedAnimationPlayer(SkinningData skinningData, Matrix bakedTransform)
        {
            if (skinningData == null)
                throw new ArgumentNullException("skinningData");

            skinningDataValue = skinningData;

            boneTransforms = new Matrix[skinningData.BindPose.Count];
            worldTransforms = new Matrix[skinningData.BindPose.Count];
            skinTransforms = new Matrix[skinningData.BindPose.Count];
            this.bakedTransform = bakedTransform;
        }

        /// <summary>
        /// Starts decoding the specified animation clip.
        /// </summary>
        public void StartClip(AnimationClip clip, AnimationState state)
        {
            if (clip == null)
                throw new ArgumentNullException("clip");

            if (currentClipValue == clip)
            {
                return;
            }

            currentClipValue = clip;
            currentStateValue = state;
            state.time = 0.0f;
            state.enabled = true;

            currentTimeValue = TimeSpan.FromSeconds(state.time);

            currentKeyframe = 0;

            // Initialize bone transforms to the bind pose.
            skinningDataValue.BindPose.CopyTo(boneTransforms, 0);
        }
        
        /// <summary>
        /// Advances the current animation position.
        /// </summary>
        public void Update(Matrix rootTransform)
        {
            UpdateBoneTransforms(TimeSpan.FromSeconds(Time.deltaTime * currentStateValue.speed));
            UpdateWorldTransforms(rootTransform);
            UpdateSkinTransforms();
        }

        /// <summary>
        /// Helper used by the Update method to refresh the BoneTransforms data.
        /// </summary>
        public void UpdateBoneTransforms(TimeSpan time)
        {
            if (currentClipValue == null)
                throw new InvalidOperationException("AnimationPlayer.Update was called before StartClip");

            if (!currentStateValue.enabled)
            {
                return;
            }

            //set the current time of the animation, to what is set in the current AnimationState. This is how we scrub through animations
            currentTimeValue = TimeSpan.FromSeconds(currentStateValue.time);
            time += currentTimeValue;

            // See if we should terminate
            if (time.TotalSeconds > currentStateValue.length)
            {
                switch (currentStateValue.wrapMode)
                {
                    case WrapMode.Once:
                        currentStateValue.enabled = false;
                        return;
                    case WrapMode.Loop:
                        time = TimeSpan.FromSeconds(currentStateValue.startTime);
                        break;
                    case WrapMode.PingPong:
                        currentStateValue.speed *= -1;
                        break;
                    case WrapMode.Default:
                        break;
                    case WrapMode.Clamp:
                        time = TimeSpan.FromSeconds(currentStateValue.length);
                        break;
                    default:
                        throw new NotImplementedException("What to do here?");
                        break;
                }
            }

            // If the position moved backwards, reset the keyframe index.
            if (time < currentTimeValue)
            {
                currentKeyframe = 0;
                skinningDataValue.BindPose.CopyTo(boneTransforms, 0);
            }

            //set current time values, both locally and in AnimationState
            currentTimeValue = time;
            currentStateValue.time = (float)currentTimeValue.TotalSeconds;

            // Read keyframe matrices.
            IList<Keyframe> keyframes = currentClipValue.Keyframes;

            while (currentKeyframe < keyframes.Count)
            {
                Keyframe keyframe = keyframes[currentKeyframe];

                // Stop when we've read up to the current time position.
                if (keyframe.Time > currentTimeValue)
                {
                    break;
                }

                // Use this keyframe.
                boneTransforms[keyframe.Bone] = keyframe.Transform;

                currentKeyframe++;
            }
            //Debug.Log(currentClipValue.name + ": Stop at keyframe " + currentKeyframe + " with time " + keyframes[currentKeyframe].Time + " on " + currentTimeValue);
            
        }
        
        /// <summary>
        /// Helper used by the Update method to refresh the WorldTransforms data.
        /// </summary>
        public void UpdateWorldTransforms(Matrix rootTransform)
        {
            // Root bone.
            worldTransforms[0] = boneTransforms[0] * rootTransform;

            // Child bones.
            for (int bone = 1; bone < worldTransforms.Length; bone++)
            {
                int parentBone = skinningDataValue.SkeletonHierarchy[bone];

                worldTransforms[bone] = boneTransforms[bone] * worldTransforms[parentBone];
            }
        }
        
        /// <summary>
        /// Helper used by the Update method to refresh the SkinTransforms data.
        /// </summary>
        public void UpdateSkinTransforms()
        {
            for (int bone = 0; bone < skinTransforms.Length; bone++)
            {
                skinTransforms[bone] = skinningDataValue.InverseBindPose[bone] * worldTransforms[bone];
            }
        }

        internal bool WorldTransformForBone(string boneName, out Matrix m)
        {
            if (currentClipValue != null)
            {
                if (skinningDataValue.BoneMap.ContainsKey(boneName))
                {
                    m = worldTransforms[skinningDataValue.BoneMap[boneName]];
                    // TODO: This is a very brutal hardcoded hack as the animation does not work very well with hiearchical scales
                    m.Translation = m.Translation * 0.01f;
                    
                    return true;
                }
            }
            m = Matrix.Identity;
            return false;
        }
    }
}
