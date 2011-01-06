﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Box2D.XNA;
using PressPlay.FFWD;

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
                def.userData = this;
                def.type = (isKinematic) ? BodyType.Kinematic : BodyType.Dynamic;
                def.active = gameObject.active;
                def.linearDamping = drag;
                def.angularDamping = angularDrag;
                def.fixedRotation = freezeRotation;
                body = Physics.AddBody(def);
                collider.AddCollider(body, mass);
            }
            else
            {
                Debug.LogWarning("No collider set on this rigid body " + ToString());
            }
        }

        [ContentSerializerIgnore]
        public Vector3 velocity
        {
            get
            {
                return body.GetLinearVelocity();
            }
            set
            {
                body.SetLinearVelocity(value);
            }
        }

        public void AddForce(Vector3 elasticityForce)
        {
            AddForce(elasticityForce, ForceMode.Force);
        }

        public void AddForce(Vector3 elasticityForce, ForceMode mode)
        {
            switch (mode)
            {
                case ForceMode.Force:
                    body.ApplyForce(elasticityForce, gameObject.transform.position);
                    break;
                case ForceMode.Acceleration:
                    break;
                case ForceMode.Impulse:
                    body.ApplyLinearImpulse(elasticityForce, gameObject.transform.position);
                    break;
                case ForceMode.VelocityChange:
                    break;
            }
        }

        public void MovePosition(Vector3 position)
        {
            if (body != null)
            {
                body.SetTransform(position, body.GetAngle());
                Physics.RemoveStays(collider);
            }
        }

        internal void MoveRotation(Quaternion localRotation)
        {
            // TODO: This does not work yet...
        }
    }
}
