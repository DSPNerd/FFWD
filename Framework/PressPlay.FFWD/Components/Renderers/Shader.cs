﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace PressPlay.FFWD.Components
{
    /// <summary>
    /// A shader encapsulates Effects in XNA and gives us a way of rendering meshes.
    /// </summary>
    public abstract class Shader
    {
        public string name;
        public int renderQueue;

        private static Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();

        /// <summary>
        /// Finds the appropriate shader for the given material. Typically by name. 
        /// 
        /// It can also load shaders from file if it is supported.
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        internal static Shader GetShader(Material mat)
        {
            if (!shaders.ContainsKey(mat.shaderName))
            {
                // TODO: Choose the correct type of Shader to use
                Shader s;
                switch (mat.shaderName)
                {
                    default:
                        s = new BasicEffectShader();
                        break;
                }
                shaders[mat.shaderName] = s;
            }
            return shaders[mat.shaderName];
        }

        internal Effect effect { get; set; }

        internal abstract void ApplyPreRenderSettings(Material mat, bool useVertexColor);

        public bool supportsTextures { get; protected set; }
        public bool supportsLights { get; protected set; }
        public bool supportsVertexColor { get; protected set; }
    }
}
