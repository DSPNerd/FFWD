﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections;

namespace PressPlay.FFWD
{
    public enum Space { World, Self }

    public class Transform : Component, IEnumerable
    {
        #region Constructors
        internal Transform()
        {
            localRotation = Quaternion.Identity;
            localScale = Vector3.one;
        }
        #endregion

        #region Properties
        private Vector3 _localPosition;
        public Vector3 localPosition
        {
            get
            {
                return _localPosition;
            }
            set
            {
                _localPosition = value;
                hasDirtyWorld = true;
            }
        }

        private Vector3 _localScale;
        public Vector3 localScale
        {
            get
            {
                return _localScale;
            }
            set
            {
                _localScale = value;
                hasDirtyWorld = true;
            }
        }

        private Quaternion _localRotation;
        public Quaternion localRotation
        {
            get
            {
                return _localRotation;
            }
            set
            {
                _localRotation = value;
                hasDirtyWorld = true;
            }
        }

        internal Transform _parent;
        [ContentSerializerIgnore]
        public Transform parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if (_parent == value)
                {
                    return;
                }
                if (_parent != null)
                {
                    _parent.children.Remove(gameObject);
                }
                Vector3 pos = position;
                _parent = value;
                if (_parent == null)
                {
                    return;
                }
                if (_parent.children == null)
                {
                    _parent.children = new List<GameObject>();
                }
                _parent.children.Add(gameObject);
                position = pos;
                hasDirtyWorld = true;
            }
        }

        [ContentSerializer(Optional = true, CollectionItemName = "child")]
        internal List<GameObject> children { get; set; }

        private Matrix _world = Matrix.Identity;

        private bool _hasDirtyWorld = true;
        private bool hasDirtyWorld
        {
            get
            {
                return _hasDirtyWorld;
            }
            set
            {
                _hasDirtyWorld = value;
                if (children != null)
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        children[i].transform.hasDirtyWorld = true;
                    }
                }
            }
        }

        [ContentSerializerIgnore]
        internal Matrix world
        {
            get
            {
                if (hasDirtyWorld)
                {
                    calculateWorld();
                }
                return _world;
            }
        }

        [ContentSerializerIgnore]
        public Vector3 position
        {
            get
            {
                return world.Translation;
            }
            set
            {
                if (parent == null)
                {
                    localPosition = value;
                }
                else
                {
                    Vector3 trans = Microsoft.Xna.Framework.Vector3.Transform(value, Matrix.Invert(parent.world));
                    localPosition = trans;
                }
                if (rigidbody != null)
                {
                    rigidbody.MovePosition(position);
                }
            }
        }

        [ContentSerializerIgnore]
        public Vector3 lossyScale
        {
            get
            {
                if (parent == null)
                {
                    return localScale;
                }
                else
                {
                    Microsoft.Xna.Framework.Vector3 scale;
                    Microsoft.Xna.Framework.Quaternion rot;
                    Microsoft.Xna.Framework.Vector3 pos;
                    world.Decompose(out scale, out rot, out pos);
                    return scale;
                }
            }
        }

        [ContentSerializerIgnore]
        public Quaternion rotation
        {
            get
            {
                if (parent == null)
                {
                    return localRotation;
                }
                else
                {
                    Microsoft.Xna.Framework.Vector3 scale;
                    Microsoft.Xna.Framework.Quaternion rot;
                    Microsoft.Xna.Framework.Vector3 pos;
                    world.Decompose(out scale, out rot, out pos);
                    return rot;
                }
            }
            set
            {
                if (parent == null)
                {
                    localRotation = value;
                }
                else
                {
                    // TODO: This does not work yet
                    throw new NotImplementedException("Not implemented yet");
                }
            }
        }

        [ContentSerializerIgnore]
        public Vector3 eulerAngles
        {
            get
            {
                return rotation.eulerAngles;
            }
            set
            {
                rotation = Quaternion.Euler(value);
            }
        }

        [ContentSerializerIgnore]
        public Vector3 localEulerAngles
        {
            get
            {
                return localRotation.eulerAngles;
            }
            set
            {
                localRotation = Quaternion.Euler(value);
            }
        }

        [ContentSerializerIgnore]
        public Vector3 right
        {
            get
            {
                return world.Right;
            }
        }

        [ContentSerializerIgnore]
        public Vector3 forward
        {
            get
            {
                return world.Forward;
            }
        }

        [ContentSerializerIgnore]
        public Vector3 up
        {
            get
            {
                return world.Up;
            }
        }

        [ContentSerializerIgnore]
        public Transform root
        {
            get
            {
                if (parent != null)
                {
                    return parent.root;
                }
                return this;
            }
        }

        public int childCount
        {
            get
            {
                return children.Count;
            }
        }
        #endregion

        #region Private methods
        private void calculateWorld()
        {
            _hasDirtyWorld = false;
            _world = Matrix.CreateScale(localScale) *
                   Matrix.CreateFromQuaternion(localRotation) *
                   Matrix.CreateTranslation(localPosition);
            if (_parent != null)
            {
                _world = _world * _parent.world;
            }
        }

        internal void SetPositionFromPhysics(Vector3 pos, float ang)
        {
            if (parent == null)
            {
                localPosition = pos;
            }
            else
            {
                localPosition = pos - parent.position;
            }
            localRotation = Quaternion.AngleAxis(ang, Vector3.up);
        }

        internal override void SetNewId(Dictionary<int, UnityObject> idMap)
        {
            base.SetNewId(idMap);
            if (children != null)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].SetNewId(idMap);
                }
            }
        }

        internal override void FixReferences(Dictionary<int, UnityObject> idMap)
        {
            base.FixReferences(idMap);
            if (children != null)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].FixReferences(idMap);
                }
            }
        }
        #endregion

        #region Public methods
        public void Translate(Vector3 translation)
        {
            Translate(translation, Space.Self);
        }

        public void Translate(Vector3 translation, Space space)
        {
            if (space == Space.Self)
            {
                localPosition += translation;
            }
            else
            {
                throw new NotImplementedException("Not implemented yet");
            }
        }

        public void Translate(float x, float y, float z)
        {
            Translate(new Vector3(x, y, z), Space.Self);
        }

        public void Translate(float x, float y, float z, Space space)
        {
            Translate(new Vector3(x, y, z), space);
        }

        public void Translate(Vector3 translation, Transform relativeTo)
        {
            // TODO: Implement this method
            throw new NotImplementedException("Not implemented yet");
        }

        public void Translate(float x, float y, float z, Transform relativeTo)
        {
            Translate(new Vector3(x, y, z), relativeTo);
        }

        public void Rotate(Vector3 axis, float angle, Space relativeTo)
        {
            if (relativeTo == Space.World)
            {
                // TODO: This will have issues with parent rotations
                Matrix rot = Matrix.CreateFromAxisAngle(axis, angle);
                Matrix.Multiply(ref _world, ref rot, out _world);
                WorldChanged();
            }
            else
            {
                Quaternion q = Quaternion.AngleAxis(angle, axis);
                localRotation *= q;
            }
        }

        public void Rotate(Vector3 axis, float angle)
        {
            Rotate(axis, angle, Space.Self);
        }

        public void Rotate(Vector3 eulerAngles, Space space)
        {
            Rotate(eulerAngles.x, eulerAngles.y, eulerAngles.z, space);
        }

        public void Rotate(Vector3 eulerAngles)
        {
            Rotate(eulerAngles.x, eulerAngles.y, eulerAngles.z, Space.Self);
        }

        public void Rotate(float x, float y, float z)
        {
            Rotate(x, y, z, Space.Self);
        }

        public void Rotate(float x, float y, float z, Space relativeTo)
        {
            if (relativeTo == Space.World)
            {
                // TODO: This will have issues with parent rotations
                Matrix rot;
                Matrix.CreateFromYawPitchRoll(y, x, z, out rot);
                Matrix.Multiply(ref _world, ref rot, out _world);
                WorldChanged();
            }
            else
            {
                Quaternion q = Quaternion.Euler(x, y, z);
                localRotation *= q;
            }
        }

        public void LookAt(Vector3 worldPosition, Vector3 worldUp)
        {
            Matrix m = Matrix.CreateWorld(position, worldPosition - position, worldUp);
            Microsoft.Xna.Framework.Vector3 scale;
            Microsoft.Xna.Framework.Quaternion rot;
            Microsoft.Xna.Framework.Vector3 pos;
            if (m.Decompose(out scale, out rot, out pos))
            {
                localRotation = new Quaternion(rot);
                if (rigidbody != null)
                {
                    rigidbody.MoveRotation(localRotation);
                }
            }
        }

        //TODO: Implement LookAt
        public void LookAt(Transform target, Vector3 worldUp)
        {

        }

        public void LookAt(Vector3 worldPosition)
        {
            LookAt(worldPosition, Vector3.up);
        }

        private void WorldChanged()
        {
            // TODO: If we have a parent - this method can frak things up proper!
            Microsoft.Xna.Framework.Vector3 scale;
            Microsoft.Xna.Framework.Quaternion rot;
            Microsoft.Xna.Framework.Vector3 pos;
            if (_world.Decompose(out scale, out rot, out pos))
            {
                _localScale = scale;
                _localRotation = new Quaternion(rot);
                _localPosition = pos;
                hasDirtyWorld = false;
            }
        }

        public IEnumerator GetEnumerator()
        {
            if (children == null)
            {
                return (new List<Transform>()).GetEnumerator();
            }
            
            return children.GetEnumerator();
        }

        public Vector3 TransformDirection(Vector3 position)
        {            
            return Microsoft.Xna.Framework.Vector3.Transform(position, rotation.quaternion);
        }

        public Vector3 TransformDirection(float x, float y, float z)
        {
            return Microsoft.Xna.Framework.Vector3.Transform(new Microsoft.Xna.Framework.Vector3(x, y, z), rotation.quaternion);
        }

        public Vector3 InverseTransformDirection(Vector3 position)
        {
            return Microsoft.Xna.Framework.Vector3.Transform(position, Microsoft.Xna.Framework.Quaternion.Inverse(rotation.quaternion));
        }

        public Vector3 InverseTransformDirection(float x, float y, float z)
        {
            return Microsoft.Xna.Framework.Vector3.Transform(new Microsoft.Xna.Framework.Vector3(x, y, z), Microsoft.Xna.Framework.Quaternion.Inverse(rotation.quaternion));
        }

        public Vector3 TransformPoint(Vector3 position)
        {
            return Microsoft.Xna.Framework.Vector3.Transform(position, world);
        }

        public Vector3 TransformPoint(float x, float y, float z)
        {
            return Microsoft.Xna.Framework.Vector3.Transform(new Microsoft.Xna.Framework.Vector3(x, y, z), world);
        }

        public Vector3 InverseTransformPoint(Vector3 position)
        {
            return Microsoft.Xna.Framework.Vector3.Transform(position, Matrix.Invert(world));
        }

        public Vector3 InverseTransformPoint(float x, float y, float z)
        {
            return Microsoft.Xna.Framework.Vector3.Transform(new Microsoft.Xna.Framework.Vector3(x, y, z), Matrix.Invert(world));
        }

        public void DetachChildren()
        {
            // TODO: Implement this method
            throw new NotImplementedException("Not implemented yet");
        }

        public Transform Find(string name)
        {
            // TODO : Add implementation of method
            throw new NotImplementedException("Method not implemented.");
        }

        public bool IsChildOf(Transform parent)
        {
            // TODO : Add implementation of method
            throw new NotImplementedException("Method not implemented.");
        }
        #endregion
    }
}
