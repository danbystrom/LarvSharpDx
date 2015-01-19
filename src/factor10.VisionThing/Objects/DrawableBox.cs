using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Primitives;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Objects
{
    public class DrawableBox : VDrawable
    {
        public Matrix World;

        private readonly CubePrimitive<VertexPositionNormalTexture> _cube;
        private readonly Texture2D _texture;
        private readonly Texture2D _bumpMap;

        public DrawableBox(VisionContent vContent, Matrix world, Vector3 size, float texScale = 1)
            : base(vContent.LoadEffect("effects/SimpleTextureEffect"))
        {
            _texture = vContent.Load<Texture2D>("textures/brick_texture_map");
            _bumpMap = vContent.Load<Texture2D>("textures/brick_normal_map");
            _cube = new CubePrimitive<VertexPositionNormalTexture>(
                Effect.GraphicsDevice,
                (p,n,t) => createVertex(p,n,t,size,texScale),
                1);
            World = world;
        }

        private VertexPositionNormalTexture createVertex(Vector3 position, Vector3 normal, Vector2 textureCoordinate, Vector3 size, float texScale)
        {
            if (normal.X != 0)
                textureCoordinate *= new Vector2(size.Z, size.Y);
            else if (normal.Y != 0)
                textureCoordinate *= new Vector2(size.X, size.Z);
            else if (normal.Z != 0)
                textureCoordinate *= new Vector2(size.Y, size.X);
            return new VertexPositionNormalTexture(
                position*size,
                normal,
                textureCoordinate*texScale);
        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            camera.UpdateEffect(Effect);
            Effect.World = World;
            if (drawingReason != DrawingReason.ShadowDepthMap)
            {
                Effect.Texture = _texture;
//TODO                Effect.Parameters["BumpMap"].SetResource(_bumpMap);
            }
            _cube.Draw(Effect);
            return true;
        }

    }

}