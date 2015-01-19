using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace factor10.VisionThing.Util
{
    public class CollisionHelpers
    {
        private struct Hit<T> : IComparable
        {
            public readonly T Obj;
            public readonly BoundingSphere BoundingSphere;
            private int _distance;

            public Hit(T obj, BoundingSphere boundingSphere )
            {
                Obj = obj;
                BoundingSphere = boundingSphere;
                _distance = 0;
            }

            public void CaclDistance(Vector3 beginning)
            {
                _distance = (int)Vector3.DistanceSquared(beginning, BoundingSphere.Center);
            }

            public int CompareTo(object obj)
            {
                return _distance - ((Hit<T>) obj)._distance;
            }
        }

        public static List<T> HitTest<T>(Ray ray, IEnumerable<T> objects, Func<T, BoundingSphere> toBoundingSphere)
        {
            var hits = objects.Select(_ => new Hit<T>(_, toBoundingSphere(_))).Where(_ => ray.Intersects(_.BoundingSphere)).ToArray();
            for (var i = 0; i < hits.Length; i++)
                hits[i].CaclDistance(ray.Position);
            Array.Sort(hits);
            return hits.Select(_ => _.Obj).ToList();
        }

        public static Vector2? LineLineIntersectionPoint(Vector2 x1, Vector2 x2, Vector2 y1, Vector2 y2)
        {
            var dx = x2.X - x1.X;
            var dy = x2.Y - x1.Y;
            var da = y2.X - y1.X;
            var db = y2.Y - y1.Y;

            if (MathUtil.IsZero(da * dy - db * dx))
                return null; // The segments are parallel

            var s = (dx * (y1.Y - x1.Y) + dy * (x1.X - y1.X)) / (da * dy - db * dx);
            var t = (da * (x1.Y - y1.Y) + db * (y1.X - x1.X)) / (db * dx - da * dy);

            if (s < 0 || s > 1 || t < 0 || t > 1)
                return null;

            return new Vector2(x1.X + t * dx, x1.Y + t * dy);
        }


    }

}
