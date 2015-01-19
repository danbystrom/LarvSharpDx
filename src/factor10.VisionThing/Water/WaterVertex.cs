using System;
using System.Runtime.InteropServices;
using factor10.VisionThing.Terrain;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Water
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WaterVertex : IEquatable<WaterVertex>
    {
        public static readonly int Size = sizeof(float) * (3+2+2);

        [VertexElement("SV_Position")]
        public readonly Vector3 Position;

        [VertexElement("TEXCOORD0")]
        public readonly Vector2 NormalizedTexC; // [0, 1]

        [VertexElement("TEXCOORD1")]
        public readonly Vector2 ScaledTexC;     // [a, b]


        public WaterVertex(Vector3 position, Vector2 normalizedTexC, Vector2 scaledTexC)
        {
            Position = position;
            NormalizedTexC = normalizedTexC;
            ScaledTexC = scaledTexC;
        }

        public bool Equals(WaterVertex other)
        {
            return Position.Equals(other.Position) && NormalizedTexC.Equals(other.NormalizedTexC) && ScaledTexC.Equals(other.ScaledTexC);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is WaterVertex && Equals((WaterVertex)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ NormalizedTexC.GetHashCode();
                hashCode = (hashCode * 397) ^ ScaledTexC.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(WaterVertex left, WaterVertex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WaterVertex left, WaterVertex right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("Position: {0}, NormalizedTexC: {1}, ScaledTexC: {2}", Position, NormalizedTexC, ScaledTexC);
        }
    }
}
