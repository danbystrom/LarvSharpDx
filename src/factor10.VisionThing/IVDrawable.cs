using System;
using factor10.VisionThing.Effects;
using SharpDX;

namespace factor10.VisionThing
{
    public interface IVDrawable : IDisposable
    {
        void Draw(IVEffect effect, int lod = 0);
    }

    public interface IPosition
    {
        Vector3 Position { get; }
    }

    public interface IDurable
    {
        bool Do(float time);
    }

    public struct PositionHolder : IPosition
    {
        public Vector3 Position { get; set; }

        public PositionHolder(Vector3 position) : this()
        {
            Position = position;
        }

    }

}
