using System;
using System.Collections.Generic;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Primitives
{

    public class PlanePrimitive<T> : GeometricPrimitive<T> where T : struct, IEquatable<T>
    {
        public delegate T CreateVertex(float x, float y, int width, int height);

        public PlanePrimitive(
            GraphicsDevice graphicsDevice,
            CreateVertex createVertex,
            int width,
            int height,
            int levels = 1)
        {
            for (var y = 0; y <= height; y++)
                for (var x = 0; x <= width; x++)
                    addVertex(createVertex(x, y, width, height));

            for (var level = 0; level < levels; level++)
            {
                addLevelOfDetail();
                var p = 1 << level;
                for (var y = 0; y < height; y += p)
                    for (var x = 0; x < width; x += p)
                    {
                        var top = y*(width + 1) + x;
                        var bottom = (y + p)*(width + 1) + x;

                        addIndex(top + 0);
                        addIndex(top + p);
                        addIndex(bottom);

                        addIndex(top + p);
                        addIndex(bottom + p);
                        addIndex(bottom);
                    }
            }

            initializePrimitive(graphicsDevice);
        }

    }

}
