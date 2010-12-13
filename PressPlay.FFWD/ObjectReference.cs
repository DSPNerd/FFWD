﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using PressPlay.FFWD;

namespace PressPlay.FFWD
{
    public class ObjectReference : Component
    {
        public int ReferencedId;

        private UnityObject _referencedObject;
        [ContentSerializerIgnore]
        public UnityObject ReferencedObject
        {
            get
            {
                if (_referencedObject == null)
                {
                    _referencedObject = Application.Find(ReferencedId);
                }
                return _referencedObject;
            }
        }

        public override GameObject gameObject
        {
            get
            {
                if (base.gameObject == null && ReferencedObject != null)
                {
                    base.gameObject = (ReferencedObject is GameObject) ? (ReferencedObject as GameObject) : (ReferencedObject as Component).gameObject;
                }
                return base.gameObject;
            }
            internal set
            {
                base.gameObject = value;
            }
        }

        public T Get<T>() where T : UnityObject
        {
            return ReferencedObject as T;
        }
    }
}
