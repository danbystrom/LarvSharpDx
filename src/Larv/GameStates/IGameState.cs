using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using SharpDX.Toolkit;

namespace Larv.GameStates
{
    public interface IGameState
    {
        void Update(Camera camera, GameTime gameTime, ref IGameState gameState);
        void Draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap);
    }
}
