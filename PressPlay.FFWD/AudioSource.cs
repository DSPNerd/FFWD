﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using PressPlay.FFWD.Interfaces;

namespace PressPlay.FFWD
{
    public class AudioSource : Behaviour, IUpdateable
    {
        private AudioClip _clip;
        public AudioClip clip
        {
            get
            {
                return _clip;
            }
            set
            {
                _clip = value;
                SetSoundEffect(_clip.sound.CreateInstance());
            }
        }

        private float _minVolume = 0;
        public float minVolume
        {
            get { return _minVolume; }
            set { _minVolume = Mathf.Clamp01(value); }
        }

        private float _maxVolume = 1;
        public float maxVolume
        {
            get { return _maxVolume; }
            set { _maxVolume = Mathf.Clamp01(value); }
        }

        private float _volume = 0;
        public float volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = Mathf.Clamp01(value);
                _volume = Mathf.Max(_volume, _minVolume);
                _volume = Mathf.Min(_volume, minVolume);

                if (soundEffect != null)
                {
                    soundEffect.Volume = _volume;
                }
            }
        }

        public bool isPlaying
        {
            get
            {
                return (soundEffect != null && soundEffect.State == SoundState.Playing);
            }
        }

        public float pitch = 0;
        public bool loop = false;
        public bool playOnAwake;
        public float time = 0;

        private SoundEffectInstance soundEffect;

        private void SetSoundEffect(SoundEffectInstance sfx)
        {
            soundEffect = sfx;
            soundEffect.IsLooped = loop;
            soundEffect.Volume = volume;
            time = 0;
        }

        public void Play()
        {
            if (soundEffect == null) return;
            soundEffect.Play();
        }

        public void PlayOneShot(AudioClip clip, float volumeScale)
        {
            throw new NotImplementedException();
        }

        public void PlayOneShot(AudioClip clip)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            if (soundEffect == null) return;
            soundEffect.Stop();
            time = 0;
        }

        public void Pause()
        {
            if (soundEffect == null) return;
            soundEffect.Pause();
        }

        public static void PlayClipAtPoint(AudioClip clip, Vector3 position)
        {
            PlayClipAtPoint(clip, position, 1);
        }

        public static void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            if (soundEffect != null && soundEffect.State == SoundState.Playing)
            {
                time += Time.deltaTime;
                if (time > clip.length)
                {
                    time = time - clip.length;
                }
            }
        }
    }
}
