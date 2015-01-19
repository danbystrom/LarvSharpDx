using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Util;
using Larv.Serpent;
using SharpDX.Toolkit;

namespace Larv.GameStates
{
    class DieState : IGameState
    {
        private readonly Serpents _serpents;
        private readonly SequentialToDo _todo  = new SequentialToDo();

        public DieState(Serpents serpents)
        {
            _serpents = serpents;

            HomingDevice.Attach(_serpents, false);
            _serpents.PlayerCave.OpenDoor = true;
            _serpents.EnemyCave.OpenDoor = true;

            // zoom out while ghost goes up and enemies goes home
            var forward = _serpents.PlayerSerpent.LookAtPosition - _serpents.Camera.Position;
            forward.Normalize();
            _todo.AddDurable(new MoveCamera(
                _serpents.Camera,
                4f.Time(), // time to look at death scene
                _serpents.Camera.Position - forward*8,
                () => _serpents.PlayerSerpent.LookAtPosition));
        }

        public void Update(Camera camera, GameTime gameTime, ref IGameState gameState)
        {
            _serpents.Update(camera, gameTime);
            if (_todo.Do(gameTime))
                return;

            if (_serpents.LivesLeft >= 1)
            {
                _serpents.LivesLeft--;
                gameState = new StartSerpentState(_serpents);
            }
            else
                gameState = new GameOverState(_serpents);
        }

        public void Draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            _serpents.Draw(camera, drawingReason, shadowMap);
        }

    }

}
