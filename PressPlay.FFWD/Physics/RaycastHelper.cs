using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using PressPlay.FFWD.Components;
using PressPlay.FFWD;

namespace PressPlay.FFWD
{
    /// <summary>
    /// This is a helper class that reduces the amount of garbage that is created when doing raycasts and making delegates.
    /// </summary>
    internal static class RaycastHelper
    {
        private static bool _findClosest;
        private static float _distance;
        private static int _layerMask;
        private static List<RaycastHit> _hits = new List<RaycastHit>(10);
        private static RaycastHit _hit;
        private static float _nearest = float.PositiveInfinity;
        private static bool _didHit = false;

        public static void SetValues(float distance, bool findClosest, int layerMask)
        {
            _distance = distance;
            _findClosest = findClosest;
            _layerMask = layerMask;

            _hits.Clear();
            _nearest = float.PositiveInfinity;
            _didHit = false;;
        }

        internal static float rayCastCallback(Fixture fixture, Microsoft.Xna.Framework.Vector2 point, Microsoft.Xna.Framework.Vector2 normal, float fraction)
        {
            float dist = _distance * fraction;
            Component uo = fixture.Body.UserData;
            Collider coll = uo as Collider;

            if (coll == null && (uo is Rigidbody))
            {
                coll = (uo as Rigidbody).collider;
            }
            if ((coll != null) && (coll.gameObject != null) && (_layerMask & (1 << coll.gameObject.layer)) > 0)
            {
                if (_findClosest)
                {
                    if (dist < _nearest)
                    {
                        _nearest = dist;
                        _hit.body = fixture.Body;
                        _hit.point = point;
                        _hit.normal = normal;
                        _hit.distance = dist;
                        _hit.collider = coll;
                        _didHit = true;
                    }
                    return fraction;
                }
                else
                {
                    // TODO: Consider making this an array
                    _hits.Add(new RaycastHit() { body = fixture.Body, point = point, normal = normal, distance = dist, collider = coll });
                    return 1;
                }
            }
            return -1;
        }   

        internal static int HitCount
        {
            get
            {
                if (_findClosest && _didHit)
                {
                    return 1;
                }

                return _hits.Count;
            }
        }

        internal static RaycastHit[] Hits
        {
            get
            {
                return _hits.ToArray();
            }
        }

        internal static RaycastHit ClosestHit()
        {
            return _hit;
        }

        public static bool pointCastCallback(Fixture fixture)
        {
            Component uo = fixture.Body.UserData;
            Collider coll = uo as Collider;
            if (coll == null && (uo is Rigidbody))
            {
                coll = (uo as Rigidbody).collider;
            }
            if ((coll != null) && (coll.gameObject != null) && (_layerMask & (1 << coll.gameObject.layer)) > 0)
            {
                _didHit = true;
                _hit = new RaycastHit() { body = fixture.Body, collider = coll };
            }
            return true;
        }

    }
}
