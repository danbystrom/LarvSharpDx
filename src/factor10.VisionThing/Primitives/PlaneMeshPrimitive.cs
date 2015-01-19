using System;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Primitives
{

    public class PlaneMeshPrimitive<T> : GeometricPrimitive<T> where T : struct, IEquatable<T>
    {
        public delegate T CreateVertex(float x, float y, int triangle);

        public PlaneMeshPrimitive(
            GraphicsDevice graphicsDevice,
            CreateVertex createVertex,
            int width,
            int height,
            int levels = 1)
        {
            for (var y = 0; y <= height; y++)
                for (var x = 0; x <= width; x++)
                {
                    addVertex(createVertex(x, y, 0));
                    addVertex(createVertex(x + 1, y, 0));
                    addVertex(createVertex(x, y + 1, 0));

                    addVertex(createVertex(x + 1, y, 1));
                    addVertex(createVertex(x + 1, y + 1, 1));
                    addVertex(createVertex(x, y + 1, 1));
                }

            addNullLevelOfDetail();

            for (var level = 1; level < levels; level++)
            {
                addLevelOfDetail();
                var p = 1 << level;
                var step = p*6;
                for (var y = 0; y < height; y += p)
                {
                    var top = width*6 * y;
                    var bottom = width*6*(y + p - 1) + 2;
                    for (var x = 0; x < width; x += p)
                    {
                        addIndex(top + 0);
                        addIndex(top + step - 5);
                        addIndex(bottom);

                        addIndex(top + step - 5);
                        addIndex(bottom + step - 5);
                        addIndex(bottom);

                        top += step;
                        bottom += step;
                    }
                }
            }

            initializePrimitive(graphicsDevice);
        }

    }

}
