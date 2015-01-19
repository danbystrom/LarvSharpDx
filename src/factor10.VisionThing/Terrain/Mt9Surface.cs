using System;
using System.Linq;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Terrain
{

    public unsafe class Mt9Surface : Sculptable<Mt9Surface.Mt9>
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct Mt9
        {
            [FieldOffset(0)] public float A;
            [FieldOffset(4)] public float B;
            [FieldOffset(8)] public float C;
            [FieldOffset(12)] public float D;
            [FieldOffset(16)] public float E;
            [FieldOffset(20)] public float F;
            [FieldOffset(24)] public float G;
            [FieldOffset(28)] public float H;
            [FieldOffset(32)] public float I;
            [FieldOffset(0)] public fixed float X [9];

            public float this[int i]
            {
                get
                {
                    switch (i)
                    {
                        case 0:
                            return A;
                        case 1:
                            return B;
                        case 2:
                            return C;
                        case 3:
                            return D;
                        case 4:
                            return E;
                        case 5:
                            return F;
                        case 6:
                            return G;
                        case 7:
                            return H;
                        case 8:
                            return I;
                    }
                    throw new ArgumentOutOfRangeException();
                }
                set
                {
                    switch (i)
                    {
                        case 0:
                            A = value;
                            return;
                        case 1:
                            B = value;
                            return;
                        case 2:
                            C = value;
                            return;
                        case 3:
                            D = value;
                            return;
                        case 4:
                            E = value;
                            return;
                        case 5:
                            F = value;
                            return;
                        case 6:
                            G = value;
                            return;
                        case 7:
                            H = value;
                            return;
                        case 8:
                            I = value;
                            return;
                    }
                    throw new ArgumentOutOfRangeException();
                }
            }

            public Color ToArgb()
            {
                var x = new[]
                {
                    A,
                    B > F ? B : 0,
                    C > G ? C : 0,
                    D > H ? D : 0,
                    E > I ? E : 0,
                    B > F ? 0 : F,
                    C > G ? 0 : G,
                    D > H ? 0 : H,
                    E > I ? 0 : I
                };

                var f = 0.5f/(x.Sum() + 0.000001f);
                var c = new Color(
                    combine(x[3], x[7], f),
                    combine(x[2], x[6], f),
                    combine(x[1], x[5], f),
                    combine(x[4], x[8], f));
                return c;
            }

            private static float combine(float x, float y, float f)
            {
                //note that x*f or y*f will always be in [0,0.5]
                return x > y
                    ? x*f + 0.5f
                    : 0.5f - y*f;
            }
        }

        public Mt9Surface(int width, int height)
            : base(width, height)
        {
        }

        public Mt9Surface(int width, int height, Mt9[] surface)
            : base(width, height, surface)
        {
        }

        public override Texture2D CreateTexture2D(GraphicsDevice graphicsDevice)
        {
            return Texture2D.New(graphicsDevice, Width, Height, PixelFormat.B8G8R8A8.UNorm,
                Values.Select(_ => _.ToArgb()).ToArray());
        }

        public void Soften(int rounds = 1)
        {
            var end = Values.Length - Width - 1;
            while (rounds-- > 0)
            {
                var old = (Mt9[]) Values.Clone();
                for (var i = Width + 1; i < end; i++)
                    for (var j = 0; j < 9; j++)
                        Values[i][j] =
                            (old[i - Width - 1][j] + old[i - Width][j] + old[i - Width + 1][j] +
                             old[i - 1][j] + old[i][j] + old[i + 1][j] +
                             old[i + Width - 1][j] + old[i + Width][j] + old[i + Width + 1][j])/9;
            }
        }

    }

}
