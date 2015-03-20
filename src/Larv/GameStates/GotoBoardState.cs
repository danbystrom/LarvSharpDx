using System.Linq;
using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Effects;
using factor10.VisionThing.Util;
using Larv.Serpent;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;

namespace Larv.GameStates
{
    internal class GotoBoardState : IGameState
    {
        public readonly Vector4 SignTextColor = new Vector4(0.5f, 0.4f, 0.3f, 1);

        private readonly Serpents _serpents;
        private readonly int _scene;
        private readonly IVEffect _signEffect;
        private readonly Vector3 _signPosition;

        private readonly SequentialToDo _actions = new SequentialToDo();

        public GotoBoardState(Serpents serpents, int scene)
        {
            _serpents = serpents;
            _scene = scene;
            _signEffect = _serpents.LContent.LoadEffect("effects/signtexteffect");

            HomingDevice.Attach(serpents).CanTurnAround = true;
            _serpents.PlayerCave.OpenDoor = true;

            _signPosition = _serpents.LContent.Ground.SignPosition + Vector3.Up*1.5f;
            var direction = Vector3.Left;
            var toCameraPosition = _signPosition + direction*2.4f;

            const float goToBoardSpeed = 2.5f;
            const float lookAtBoardTime = 1f;
            const float turnToCaveTime = 2f;

            // move to the board in an arc
            _actions.AddDurable(() => new MoveCameraArc(_serpents.Camera, goToBoardSpeed.UnitsPerSecond(), toCameraPosition, Vector3.Right, 5));

            // look at the board while resetting the playing field
            _actions.AddOneShot(lookAtBoardTime, () =>
            {
                _serpents.Restart(_scene);
                HomingDevice.Attach(_serpents);
            });

            Vector3 toPosition, toLookAt;
            _serpents.PlayingField.GetCameraPositionForLookingAtPlayerCave(out toPosition, out toLookAt);

            // turn around the camera to look at the cave 
            _actions.AddDurable(
                () => new MoveCameraYaw(_serpents.Camera, turnToCaveTime.Time(), toPosition, _serpents.PlayingField.PlayerWhereaboutsStart.GetPosition()));
        }

        public void Update(Camera camera, GameTime gameTime, ref IGameState gameState)
        {
            _serpents.Update(camera, gameTime);

            if (_actions.Do(gameTime))
                return;

            gameState = new StartSerpentState(_serpents);
        }

        public void Draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            _serpents.Draw(camera, drawingReason, shadowMap);

            camera.UpdateEffect(_signEffect);

            var sb = _serpents.LContent.SpriteBatch;
            var font = _serpents.LContent.Font;
            var text = string.Format("Entering scene {0}", 1 + _scene);
            _signEffect.World = Matrix.BillboardRH(_signPosition + Vector3.Left * 0.1f, _signPosition + Vector3.Left, -camera.Up, Vector3.Right);
            _signEffect.DiffuseColor = SignTextColor;
            sb.Begin(SpriteSortMode.Deferred, _signEffect.GraphicsDevice.BlendStates.NonPremultiplied, null, _signEffect.GraphicsDevice.DepthStencilStates.DepthRead, null, _signEffect.Effect);
            sb.DrawString(font, text, Vector2.Zero, Color.Black, 0, font.MeasureString(text) / 2, 0.010f, 0, 0);
            sb.End();
        }

    }

}
