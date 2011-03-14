﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using PressPlay.FFWD;

namespace PressPlay.FFWD.Components
{
    public class Rigidbody : Component
    {
        private float _mass = 1.0f;
        public float mass { 
            get
            {
                return _mass;
            }
            set
            {
                _mass = value;
                RescaleMass();
            }
        }

        private float _drag;
        public float drag {
            get 
            {
                if (body != null)
                {
                    _drag = body.LinearDamping; 
                    
                }

                return _drag; 
            }
            set 
            {
                _drag = value;
                if (body != null)
                {
                    body.LinearDamping = value;
                }
            } 
        }
        public float angularDrag { get; set; }
        public bool isKinematic { get; set; }
        public bool freezeRotation { get; set; }

        private Body body;

        public override void Awake()
        {
            if (collider != null)
            {
                Body body = Physics.AddBody();
                body.Position = transform.position;
                body.Rotation = -MathHelper.ToRadians(transform.rotation.eulerAngles.y);
                body.UserData = this;
                body.BodyType = (isKinematic) ? BodyType.Kinematic : BodyType.Dynamic;
                body.Enabled = gameObject.active;
                body.LinearDamping = drag;
                body.AngularDamping = angularDrag;
                body.FixedRotation = freezeRotation;
                body = Physics.AddBody();
                collider.AddCollider(body, mass);
                RescaleMass();
            }
            else
            {
#if DEBUG
                Debug.LogWarning("No collider set on this rigid body " + ToString());
#endif
            }
        }

        private void RescaleMass()
        {
            if (body != null && body.Mass > 0)
            {
                float bodyMass = body.Mass;
                float massRatio = mass / bodyMass;
                for (int i = 0; i < body.FixtureList.Count; i++)
                {
                    Fixture f = body.FixtureList[i];
                    f.Shape.Density *= massRatio;
                }
                body.ResetMassData();
            }
        }

        [ContentSerializerIgnore]
        public Vector3 velocity
        {
            get
            {
                if (body == null)
                {
                    return Vector3.zero;
                }
                return body.LinearVelocity;
            }
            set
            {
                if (body != null)
                {
                    body.LinearVelocity = value;
                }
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
//                body.SetTransform(position, body.GetAngle());
                Microsoft.Xna.Framework.Vector2 pos = position;
                body.SetTransformIgnoreContacts(ref pos, body.Rotation);
                Physics.RemoveStays(collider);
            }
        }

        internal void MoveRotation(Quaternion localRotation)
        {
            // TODO: This does not work yet...
            //body.SetTransform(body.GetPosition(), localRotation.eulerAngles.y);
            //Physics.RemoveStays(collider);
        }
    }
}
