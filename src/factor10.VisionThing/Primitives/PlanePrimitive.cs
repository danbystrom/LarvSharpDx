using System;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Primitives
{

    public class PlanePrimitive<T> : GeometricPrimitive<T> where T : struct, IEquatable<T>
    {
        public PlanePrimitive(
            GraphicsDevice graphicsDevice,
            CreateVertex createVertex,
            int width,
            int height,
            int levels = 1)
        {
            for (var y = 0; y <= height; y++)
                for (var x = 0; x <= width; x++)
                    AddVertex(createVertex(new PositionNormalTangentTexture(
                        new Vector3(x, 0, y),
                        Vector3.Up,
                        Vector3.Right,
                        new Vector2(x/(float) width, y/(float) height))));

            for (var level = 0; level < levels; level++)
            {
                AddLevelOfDetail();
                var p = 1 << level;
                for (var y = 0; y < height; y += p)
                    for (var x = 0; x < width; x += p)
                    {
                        var top = y*(width + 1) + x;
                        var bottom = (y + p)*(width + 1) + x;

                        AddTriangle(top, top + p, bottom);
                        AddTriangle(top + p, bottom + p, bottom);
                    }
            }

            InitializePrimitive(graphicsDevice);
        }

    }

}
