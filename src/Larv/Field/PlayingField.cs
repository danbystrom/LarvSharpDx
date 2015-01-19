using System;
using System.Collections.Generic;
using System.Linq;
using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using Larv.Serpent;
using Larv.Util;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace Larv.Field
{
    public class PlayingField : VDrawable
    {
        public readonly Buffer<VertexPositionNormalTexture> VertexBuffer;
        public readonly Buffer<VertexPositionNormalTexture> VertexBufferShadow;
        public readonly VertexInputLayout VertexInputLayout;

        private readonly Texture2D _texture;

        public readonly int Floors, Width, Height;
        public readonly PlayingFieldSquare[, ,] TheField;
        public readonly Whereabouts PlayerWhereaboutsStart;
        public readonly Whereabouts EnemyWhereaboutsStart;

        public readonly float MiddleX;
        public readonly float MiddleY;

        public PlayingField(LarvContent lContent, Texture2D texture, int level)
            : base(lContent.TextureEffect)
        {
            _texture = texture;

            var pfInfo = lContent.PlayingFieldInfos[level];
            TheField = pfInfo.PlayingField;
            Floors = pfInfo.Floors;
            Height = pfInfo.Height;
            Width = pfInfo.WIdth;
            PlayerWhereaboutsStart = pfInfo.PlayerSerpentStart;
            EnemyWhereaboutsStart = pfInfo.EnemySerpentStart;

            MiddleX = Width/2f;
            MiddleY = Height/2f;

            var verts = new List<VertexPositionNormalTexture>
            {
                new VertexPositionNormalTexture(new Vector3(-1, 0, -1), Vector3.Up, new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(Width + 1, 0, -1), Vector3.Up, new Vector2(Width*0.25f, 0)),
                new VertexPositionNormalTexture(new Vector3(-1, 0, Height + 1), Vector3.Up, new Vector2(0, Height*0.25f)),
                new VertexPositionNormalTexture(new Vector3(Width + 1, 0, -1), Vector3.Up, new Vector2(Width*0.25f, 0)),
                new VertexPositionNormalTexture(new Vector3(Width + 1, 0, Height + 1), Vector3.Up, new Vector2(Width*0.25f, Height*0.25f)),
                new VertexPositionNormalTexture(new Vector3(-1, 0, Height + 1), Vector3.Up, new Vector2(0, Height*0.25f))
            };
            var vertsShadow = new List<VertexPositionNormalTexture>();
            for (var floor = 0; floor < Floors; floor++)
                for (var y = 0; y < Height; y++ )
                    for (var x = 0; x < Width; x++)
                    {
                        var sq = TheField[floor, y, x];
                        if ((!sq.IsNone && floor != 0) || sq.IsSlope)
                            contructSquare(verts, vertsShadow, floor, new Point(x,y), sq.Corners);
                    }


            VertexBuffer = Buffer.Vertex.New(lContent.GraphicsDevice, verts.ToArray());
            if(vertsShadow.Any())
                VertexBufferShadow = Buffer.Vertex.New(lContent.GraphicsDevice, vertsShadow.ToArray());
            VertexInputLayout = VertexInputLayout.FromBuffer(0, VertexBuffer);
        }

        private void contructSquare(
            IList<VertexPositionNormalTexture> verts,
            IList<VertexPositionNormalTexture> vertsShadow,
            int floor,
            Point p,
            int[] cornerElevations)
        {
            var z = floor*4;
            var i = verts.Count;
            addVertex(verts, z, cornerElevations[1], p, 0, 0); //NW 0
            addVertex(verts, z, cornerElevations[3], p, 1, 0); //NE 1
            addVertex(verts, z, cornerElevations[0], p, 0, 1); //SW 2
            addVertex(verts, z, cornerElevations[0], p, 0, 1); //SW 3
            addVertex(verts, z, cornerElevations[3], p, 1, 0); //NE 4
            addVertex(verts, z, cornerElevations[2], p, 1, 1); //SE 5

            if (!CanMoveHere(floor, p, p.Add(0, -1)))
                addSide(vertsShadow, verts[i + 1].Position, verts[i].Position, Vector3.ForwardRH);
            if (!CanMoveHere(floor, p, p.Add(0, 1)))
                addSide(vertsShadow, verts[i + 2].Position, verts[i + 5].Position, Vector3.BackwardRH);
            if (!CanMoveHere(floor, p, p.Add(1, 0)))
                addSide(vertsShadow, verts[i + 1].Position, verts[i + 5].Position, Vector3.Left);
            if (!CanMoveHere(floor, p, p.Add(-1, 0)))
                addSide(vertsShadow, verts[i].Position, verts[i + 2].Position, Vector3.Right);
        }

        private void addVertex(
            IList<VertexPositionNormalTexture> verts,
            int floor,
            int cornerElevation,
            Point p,
            int w,
            int h)
        {
            verts.Add(new VertexPositionNormalTexture(
                new Vector3((p.X + w), (floor + cornerElevation)/3f, (p.Y + h)),
                Vector3.Up,
                new Vector2(w*0.25f, h*0.25f)));
        }

        private void addSide(
            IList<VertexPositionNormalTexture> verts,
            Vector3 p1,
            Vector3 p2,
            Vector3 normal)
        {
            const float q = 0.25f;
            const float h = 0.25f;
            var d = Vector3.Down*h;
            verts.Add(new VertexPositionNormalTexture(p1, normal, Vector2.Zero));
            verts.Add(new VertexPositionNormalTexture(p2, normal, new Vector2(q, 0)));
            verts.Add(new VertexPositionNormalTexture(p1 + d, normal, new Vector2(0, q*h)));
            verts.Add(new VertexPositionNormalTexture(p2, normal, new Vector2(q, 0)));
            verts.Add(new VertexPositionNormalTexture(p2 + d, normal, new Vector2(q, q*h)));
            verts.Add(new VertexPositionNormalTexture(p1 + d, normal, new Vector2(0, q * h)));
        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            camera.UpdateEffect(Effect);

            Effect.World = Matrix.Translation(-0.5f, 0, -0.5f);
            Effect.Texture = _texture;

            Effect.GraphicsDevice.SetVertexInputLayout(VertexInputLayout);

            Effect.DiffuseColor = Vector4.One;
            Effect.GraphicsDevice.SetVertexBuffer(VertexBuffer);
            Effect.ForEachPass(() =>
                Effect.GraphicsDevice.Draw(
                    PrimitiveType.TriangleList,
                    VertexBuffer.ElementCount));

            if (VertexBufferShadow != null)
            {
                Effect.DiffuseColor = new Vector4(0.4f, 0.4f, 0.4f, 1);
                Effect.GraphicsDevice.SetVertexBuffer(VertexBufferShadow);
                Effect.ForEachPass(() =>
                    Effect.GraphicsDevice.Draw(
                        PrimitiveType.TriangleList,
                        VertexBufferShadow.ElementCount));
            }

            return true;
        }

        public PlayingFieldSquare FieldValue(Whereabouts whereabouts)
        {
            return FieldValue(whereabouts.Floor, whereabouts.Location);
        }

        public PlayingFieldSquare FieldValue( int floor, Point p)
        {
            if (floor < 0 || floor >= Floors)
                return new PlayingFieldSquare();
            if (p.Y < 0 || p.Y >= Height)
                return new PlayingFieldSquare();
            if (p.X < 0 || p.X >= Width)
                return new PlayingFieldSquare();
            return TheField[floor, p.Y, p.X];
        }

        public bool CanMoveHere(int floor, Point currentLocation, Point newLocation)
        {
            return CanMoveHere(ref floor, currentLocation, newLocation);
        }

        public bool CanMoveHere(ref int floor, Point currentLocation, Point newLocation, bool ignoreRestriction = false)
        {
            if (!FieldValue(floor, newLocation).IsNone)
            {
                if (ignoreRestriction)
                    return true;
                var restricted = FieldValue(floor, newLocation).Restricted;
                return restricted == Direction.None || restricted == Direction.FromPoints(currentLocation, newLocation);
            }
            if (FieldValue(floor, currentLocation).IsSlope && FieldValue(floor + 1, newLocation).IsPortal)
            {
                floor++;
                return true;
            }
            if (FieldValue(floor, currentLocation).IsPortal && !FieldValue(floor - 1, newLocation).IsNone)
            {
                floor--;
                return true;
            }
            return false;
        }

        private PlayingFieldSquare fieldValue(ref int floor, Point p)
        {
            var sq = FieldValue(floor, p);
            if (!sq.IsNone)
                return sq;
            floor++;
            sq = FieldValue(floor, p);
            if (!sq.IsNone)
                return sq;
            floor -= 2;
            sq = FieldValue(floor, p);
            if (sq.IsNone)
                floor++;
            return sq;
        }

        private float getElevation(
            Whereabouts whereabouts)
        {
            var p1 = whereabouts.Location;
            var p2 = whereabouts.NextLocation;
            var floor1 = whereabouts.Floor;
            var floor2 = floor1;
            var square1 = fieldValue(ref floor1, p1);
            var square2 = fieldValue(ref floor2, p2);
            return square1.IsNone
                ? 0
                : MathUtil.Lerp(floor1*4 + square1.Elevation, floor2*4 + square2.Elevation, whereabouts.Fraction)/3;
        }

        public float GetElevation(
            Whereabouts whereabouts)
        {
            whereabouts.Fraction -= 0.5f;
            whereabouts.Realign();
            var x = getElevation(whereabouts);
            whereabouts.Location = whereabouts.NextLocation;
            return Math.Max(x, getElevation(whereabouts));
        }

        public void GetSurroundingElevation(int x, int y, out float min, out float max)
        {
            min = float.MaxValue;
            max = 0;

            for (var i = -1; i < 2; i++)
                for (var j = -1; j < 2; j++)
                {
                    var floor = 0;
                    var sq = fieldValue(ref floor, new Point(x + i, y + j));
                    var e = (floor*4 + sq.Elevation)/3f;
                    min = Math.Min(min, e);
                    max = Math.Max(max, e);
                }
        }

        public void GetCameraPositionForLookingAtPlayerCave(out Vector3 toPosition, out Vector3 toLookAt)
        {
            var lookAtDirection = PlayerWhereaboutsStart.Direction.DirectionAsVector3();
            toLookAt = PlayerWhereaboutsStart.GetPosition(this) + lookAtDirection * 4;

            var finalNormal = Vector3.TransformNormal(
                lookAtDirection * SerpentCamera.CameraDistanceToHeadXz * 1.2f,
                Matrix.RotationY(-MathUtil.Pi * 0.2f));
            toPosition = toLookAt + finalNormal;
            toPosition.Y += SerpentCamera.CameraDistanceToHeadY;
        }

        public override void Dispose()
        {
            VertexBuffer.Dispose();
            if(VertexBufferShadow!=null)
                VertexBufferShadow.Dispose();
        }

    }

}
