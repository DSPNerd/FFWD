﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PressPlay.FFWD.ScreenManager;
using Microsoft.Xna.Framework;

namespace PressPlay.FFWD.UI.Controls
{
    public class Control : Component, PressPlay.FFWD.Interfaces.IUpdateable
    {

        #region fields
        private Vector2 _size;
        private bool sizeValid = false;
        private bool autoSize = true;
        protected List<Control> children = null;
        public Vector2 drawOffset = Vector2.zero;
        /// <summary>
        /// Event raised when the menu entry is selected.
        /// </summary>
        #endregion

        #region properties
        public Rectangle bounds
        {
            get
            {
                Vector2 pos = transform.position;
                Vector2 scale = transform.lossyScale;
                return new Rectangle((int)pos.x, (int)pos.y, (int)(size.x * scale.x), (int)(size.y * scale.y));
            }
        }

        /// <summary>
        /// Position of this control within its parent control.
        /// </summary>
        public Vector2 position
        {
            get
            {
                return gameObject.transform.localPosition;
            }
            set
            {
                gameObject.transform.localPosition = value;
                if (parent != null)
                {
                    parent.InvalidateAutoSize();
                }
            }
        }

        /// <summary>
        /// Size if this control. See above for a discussion of the layout system.
        /// </summary>
        public Vector2 size
        {
            // Default behavior is for ComputeSize() to determine the size, and then cache it.
            get
            {
                if (!sizeValid)
                {
                    _size = ComputeSize();
                    sizeValid = true;
                }
                return _size;
            }

            // Setting the size overrides whatever ComputeSize() would return, and disables autoSize
            set
            {
                _size = value;
                sizeValid = true;
                autoSize = false;
                if (parent != null)
                {
                    parent.InvalidateAutoSize();
                }
            }
        }

        /// <summary>
        /// Call this method when a control's content changes so that its size needs to be recomputed. This has no
        /// effect if autoSize has been disabled.
        /// </summary>
        protected void InvalidateAutoSize()
        {
            if (autoSize)
            {
                sizeValid = false;
                if (parent != null)
                {
                    parent.InvalidateAutoSize();
                }
            }
        }

        /// <summary>
        /// The control containing this control, if any
        /// </summary>
        public Control parent { get; private set; }

        /// <summary>
        /// Number of child controls of this control
        /// </summary>
        public int childCount { get { return children == null ? 0 : children.Count; } }

        /// <summary>
        /// Indexed access to the children of this control.
        /// </summary>
        public Control this[int childIndex]
        {
            get
            {
                return children[childIndex];
            }
        }
        #endregion


        #region constructors
        public Control()
        {
            if (gameObject == null)
            {
                GameObject go = new GameObject("control");
                go.AddComponent(this);
            }
        }
        #endregion


        #region Handle input
        /// <summary>
        /// Called once per frame to update the control; override this method if your control requires custom updates.
        /// Call base.Update() to update any child controls.
        /// </summary>
        public virtual void HandleInput(InputState input)
        {
            for (int i = 0; i < childCount; i++)
            {
                children[i].HandleInput(input);
            }
        }
        #endregion

        public void SetScale(Vector3 scale)
        {
            transform.localScale = scale;
            InvalidateAutoSize();
        }

        /// <summary>
        /// Called when the Size property is read and sizeValid is false. Call base.ComputeSize() to compute the
        /// size (actually the lower-right corner) of all child controls.
        /// </summary>
        public virtual Vector2 ComputeSize()
        {
            if (children == null || children.Count == 0)
            {
                return Vector2.zero;
            }
            else
            {
                Vector2 childBounds = children[0].position + children[0].size;
                //Debug.Log("childBounds" + childBounds);
                for (int i = 1; i < children.Count; i++)
                {
                    Vector2 corner = children[i].position + children[i].size;
                    //Debug.Log("children[" + i + "]" + corner);
                    childBounds.x = Math.Max(childBounds.x, corner.x);
                    childBounds.y = Math.Max(childBounds.y, corner.y);
                }

                return childBounds;
            }
        }

        protected virtual bool isInputWithinBounds(InputState input)
        {
            return isInputWithinBounds(input, bounds);
        }

        protected virtual bool isInputWithinBounds(InputState input, Rectangle box)
        {
            for (int i = 0; i < input.TouchState.Count; i++)
            {
                //Debug.Log("bounds: " + box + " input.TouchState[" + i + "]: " + input.TouchState[i].Position);
                if (box.Contains((int)input.TouchState[i].Position.X, (int)input.TouchState[i].Position.Y))
                {
                    return true;
                }
            }

            return false;
        }

        #region IUpdateable Members

        public virtual void Update()
        {

        }

        #endregion

        #region Child control API
        public void AddChild(Control child)
        {
            if (child.parent != null)
            {
                child.parent.RemoveChild(child);
            }

            AddChild(child, childCount);
        }

        public void AddChild(Control child, int index)
        {
            if (child.parent != null)
            {
                child.parent.RemoveChild(child);
            }
            child.parent = this;
            child.transform.parent = transform;

            if (children == null)
            {
                children = new List<Control>();
            }

            children.Insert(index, child);
            OnChildAdded(index, child);
        }

        public void RemoveChildAt(int index)
        {
            Control child = children[index];
            child.parent = null;
            child.transform.parent = null;
            children.RemoveAt(index);
            OnChildRemoved(index, child);
        }


        /// <summary>
        /// Remove the given control from this control's list of children.
        /// </summary>
        public void RemoveChild(Control child)
        {
            if (child.parent != this)
                throw new InvalidOperationException();

            RemoveChildAt(children.IndexOf(child));
        }

        /// <summary>
        /// Called after a child control is added to this control. The default behavior is to call InvalidateAutoSize().
        /// </summary>
        protected virtual void OnChildAdded(int index, Control child)
        {
            //InvalidateAutoSize();
        }

        /// <summary>
        /// Called after a child control is removed from this control. The default behavior is to call InvalidateAutoSize().
        /// </summary>
        protected virtual void OnChildRemoved(int index, Control child)
        {
            //InvalidateAutoSize();
        }
        #endregion
    }
}
