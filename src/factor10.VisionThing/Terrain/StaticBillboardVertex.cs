using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Terrain
{
    [StructLayout(LayoutKind.Sequential)]
    public struct StaticBillboardVertex : IEquatable<StaticBillboardVertex>
    {
        public static readonly int Size = sizeof (float)*(3 + 3 + 2 + 2);

        [VertexElement("SV_Position")]
        public readonly Vector3 Position;

        [VertexElement("NORMAL")]
        public readonly Vector3 Normal;

        [VertexElement("TEXCOORD0")]
        public readonly Vector2 TexCoord;

        [VertexElement("TEXCOORD1")]
        public readonly Vector3 Front;

        public StaticBillboardVertex(Vector3 position, Vector3 normal, Vector3 front, Vector2 texCoord)
        {
            normal.Normalize();
            Position = position;
            Normal = normal;
            TexCoord = texCoord;
            Front = front;
        }

        public bool Equals(StaticBillboardVertex other)
        {
            return Position.Equals(other.Position) && Normal.Equals(other.Normal) && TexCoord.Equals(other.TexCoord) &&
                   Front.Equals(other.Front);
            ;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is CxBillboardVertex && Equals((CxBillboardVertex) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode*397) ^ Normal.GetHashCode();
                hashCode = (hashCode*397) ^ TexCoord.GetHashCode();
                hashCode = (hashCode*397) ^ Front.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(StaticBillboardVertex left, StaticBillboardVertex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StaticBillboardVertex left, StaticBillboardVertex right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("Position: {0}, Normal: {1}, TexCoord: {2}, Front {3}", Position, Normal,
                TexCoord, Front);
        }

    }

}
