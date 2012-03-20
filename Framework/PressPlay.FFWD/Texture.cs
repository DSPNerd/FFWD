﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PressPlay.FFWD
{
    public class Texture : Asset
    {
        protected Microsoft.Xna.Framework.Graphics.Texture2D tex;

        public Texture()
        {
        }

        internal Texture(Microsoft.Xna.Framework.Graphics.Texture2D t)
        {
            tex = t;
        }

        internal Texture(int _width, int _height)
        {            
            tex = new Microsoft.Xna.Framework.Graphics.Texture2D(Application.Instance.GraphicsDevice, _width, _height, false, Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color);               
        }

        public int Width
        {
            get
            {
                return (tex == null) ? 0 : tex.Width;
            }
        }

        public int Height
        {
            get
            {
                return (tex == null) ? 0 : tex.Height;
            }
        }

        public int width
        {
            get
            {
                return (tex == null) ? 0 : tex.Width;
            }
        }

        public int height
        {
            get
            {
                return (tex == null) ? 0 : tex.Height;
            }
        }

        public void Apply()
        {
            Apply(false, false);
        }

        public void Apply(bool updateMipmaps, bool makeNoLongerReadable)
        {
            // TODO: Implement this
        }

        protected override void DoLoadAsset(AssetHelper assetHelper)
        {
            if (name == null)
            {
                return;
            }
            if (tex == null)
            {
                tex = assetHelper.LoadAsset<Microsoft.Xna.Framework.Graphics.Texture2D>(name);
            }
        }

        public static implicit operator Microsoft.Xna.Framework.Graphics.Texture2D(Texture t)
        {
            if (t == null)
            {
                return null;
            }
            return t.tex;
        }
    }
}
