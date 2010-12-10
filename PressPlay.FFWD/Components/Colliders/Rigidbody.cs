﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Box2D.XNA;

namespace PressPlay.FFWD.Components
{
    public class Rigidbody : Component
    {
        public float mass { get; set; }
        public float drag { get; set; }
        public float angularDrag { get; set; }
        public bool isKinematic { get; set; }
        public bool freezeRotation { get; set; }

        private Body body;

        public override void Awake()
        {
            if (collider != null)
            {
                BodyDef def = collider.GetBodyDefinition();
                def.type = BodyType.Dynamic;
                body = Physics.AddBody(def);
                collider.AddCollider(body);
            }
            else
            {
                Debug.LogWarning("No collider set on this rigid body " + ToString());
            }
        }

        [ContentSerializerIgnore]
        public float velocity { get; set; }

        public void AddForce(Vector3 elasticityForce)
        {
            
        }
    }
}
