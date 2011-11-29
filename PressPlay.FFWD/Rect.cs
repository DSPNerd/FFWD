﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PressPlay.FFWD
{
    public struct Rect
    {
        public Rect(float left, float top, float w, float h)
        {
            _xMin = x = left;
            _yMin = y = top;
            width = w;
            height = h;
            _xMax = x + width;
            _yMax = y + height;
        }

        public float x;
        public float y;
        public float width;
        public float height;

        private float _xMin;
        public float xMin
        {
            get
            {
                return _xMin;
            }
            set
            {
                _xMin = value;
            }
        }

        private float _yMin;
        public float yMin
        {
            get
            {
                return _yMin;
            }
            set
            {
                _yMin = value;
            }
        }

        private float _xMax;
        public float xMax
        {
            get
            {
                return _xMax;
            }
            set
            {
                _xMax = value;
            }
        }

        private float _yMax;
        public float yMax
        {
            get
            {
                return _yMax;
            }
            set
            {
                _yMax = value;
            }
        }

        public bool Contains(Vector2 point)
        {
            throw new NotImplementedException();
        }

        public bool Contains(Vector3 point)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
