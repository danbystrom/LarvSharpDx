using System;
using System.Collections.Generic;
using SharpDX;

namespace factor10.VisionThing
{
    public class ArcGenerator
    {
        public readonly Vector3[] Points;
        private readonly int _iterations;

        public ArcGenerator(
            int iterations)
        {
            _iterations = iterations;
            Points = new Vector3[(1 << iterations + 1) + 1];
        }

        private void createArc(
            ref int i,
            int iteration,
            Vector3 start,
            Vector3 end,
            Vector3 bendDirection,
            float bendLength)
        {
            var direction = end - start;
            var bend = Vector3.Cross(Vector3.Cross(direction, bendDirection), direction);
            bend.Normalize();
            bend *= bendLength;

            var middle = (start + end)/2 + bend;
            if (iteration == 0)
            {
                Points[i++] = start;
                Points[i++] = middle;
                return;
            }

            var a = direction.Length()/2;
            var a2 = a*a;
            var b2 = bendLength*bendLength;
            var c2 = (a2 + b2)/4;
            var r = c2*2/bendLength;
            var d = r - (float) Math.Sqrt(r*r - c2);

            iteration--;
            createArc(ref i, iteration, start, middle, bend, d);
            createArc(ref i, iteration, middle, end, bend, d);
        }

        public void CreateArc(
            Vector3 start,
            Vector3 end,
            Vector3 bendDirection,
            float bendLength)
        {
            var i = 0;
            if (Points.Length == 2 || bendLength < 0.000001f)
                Points[i++] = start;
            else
                createArc(ref i, _iterations, start, end, bendDirection, bendLength);
            Points[i] = end;
        }

        public void StoreVertices<T>(List<T> result, Func<Vector3, float, int, T> func)
        {
            var i = 0;
            var max = Points.Length - 1;
            foreach (var pt in Points)
            {
                var where = (float) i/max;
                if (i != 0 && i != max)
                    result.Add(func(pt, where, result.Count));
                result.Add(func(pt, where, result.Count));
                i++;
            }
        }

    }

}
