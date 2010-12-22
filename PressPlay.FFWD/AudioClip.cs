﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace PressPlay.FFWD
{
    public class AudioClip : UnityObject
    {
        [ContentSerializer(Optional = true)]
        public string clip;

        [ContentSerializerIgnore]
        public SoundEffect sound;

        public float length
        {
            get
            {
                if (sound == null) return 0.0f;
                return (float)sound.Duration.TotalSeconds;
            }
        }

        public void PrepareLoadContent()
        {
            ContentHelper.LoadSound(clip);
        }

        public void EndLoadContent()
        {
            if (sound == null)
            {
                sound = ContentHelper.GetSound(clip);
            }
        }
    }
}
