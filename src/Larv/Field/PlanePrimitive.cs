using System;
using SharpDX.Toolkit.Graphics;

namespace Larv.Field
{

    public class PlanePrimitive<T> : factor10.VisionThing.Primitives.GeometricPrimitive<T> where T : struct, IEquatable<T>
    {
        public delegate T CreateVertex(float x, float y, int width, int height);

        public PlanePrimitive(
            GraphicsDevice graphicsDevice,
            CreateVertex createVertex,
            int width,
            int height)
        {
            for (var y = 0; y <= height; y++)
                for (var x = 0; x <= width; x++)
                    addVertex(createVertex(x, y, width, height));

            for (var y = 0; y < height; y ++)
                for (var x = 0; x < width; x ++)
                {
                    var top = y*(width + 1) + x;
                    var bottom = (y + 1)*(width + 1) + x;

                    addIndex(top + 0);
                    addIndex(top + 1);
                    addIndex(bottom);

                    addIndex(top + 1);
                    addIndex(bottom + 1);
                    addIndex(bottom);
                }

            initializePrimitive(graphicsDevice);
        }

    }

}
