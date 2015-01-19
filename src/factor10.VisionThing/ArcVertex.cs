using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ArcVertex : IEquatable<ArcVertex>
    {
        [VertexElement("SV_Position")] 
        public readonly Vector3 Position;

        [VertexElement("COLOR")]
        public readonly Color Color;

        [VertexElement("BLENDWEIGHT")]
        public readonly float A;

        public ArcVertex(Vector3 position, Color color, float a)
        {
            Position = position;
            Color = color;
            A = a;
        }

        public bool Equals(ArcVertex other)
        {
            return Position.Equals(other.Position) && Color.Equals(other.Color) && A.Equals(other.A);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ArcVertex && Equals((ArcVertex) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode*397) ^ Color.GetHashCode();
                hashCode = (hashCode*397) ^ A.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ArcVertex left, ArcVertex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ArcVertex left, ArcVertex right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("Position: {0}, Color: {1}, A: {2}", Position, Color, A);
        }

    }

}