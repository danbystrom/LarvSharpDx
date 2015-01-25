using System.Runtime.InteropServices;
using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Primitives;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Objects
{
    public class DrawableBox : VDrawable
    {
        public Matrix World;

        private readonly CubePrimitive<VertexPositionNormalTangentTexture> _cube;
        private readonly Texture2D _texture;
        private readonly Texture2D _bumpMap;

        public DrawableBox(VisionContent vContent, Matrix world, Vector3 size, float texScale = 1)
            : base(vContent.LoadEffect("effects/SimpleBumpEffect"))
        {
            _texture = vContent.Load<Texture2D>("textures/brick_texture");
            _bumpMap = vContent.Load<Texture2D>("textures/brick_normal");
            _cube = new CubePrimitive<VertexPositionNormalTangentTexture>(
                Effect.GraphicsDevice,
                _ => createVertex(_, size, texScale),
                size);
            World = world;
        }

        private VertexPositionNormalTangentTexture createVertex(PositionNormalTangentTexture pntt, Vector3 size, float texScale)
        {
            var textureCoordinate = pntt.TextureCoordinate;
            //if (pntt.Normal.X != 0)
            //    textureCoordinate *= new Vector2(size.Z, size.Y);
            //else if (pntt.Normal.Y != 0)
            //    textureCoordinate *= new Vector2(size.X, size.Z);
            //else if (pntt.Normal.Z != 0)
            //    textureCoordinate *= new Vector2(size.Y, size.X);
            return new VertexPositionNormalTangentTexture(
                pntt.Position,
                pntt.Normal,
                pntt.Tangent,
                textureCoordinate);
        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            camera.UpdateEffect(Effect);
            Effect.World = World;
            if (drawingReason != DrawingReason.ShadowDepthMap)
            {
                Effect.Texture = _texture;
                Effect.Parameters["BumpMap"].SetResource(_bumpMap);
            }
            _cube.Draw(Effect);
            return true;
        }

    }

}