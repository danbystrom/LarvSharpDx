using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Util;
using Larv.Field;
using Larv.Serpent;
using Larv.Util;
using SharpDX;
using SharpDX.Toolkit;

namespace Larv.GameStates
{
    internal class StartSerpentState : IGameState
    {
        private readonly Serpents _serpents;
        private readonly SequentialToDo _actions = new SequentialToDo();
        private SerpentCamera _serpentCamera;

        public StartSerpentState(Serpents serpents)
        {
            _serpents = serpents;
            HomingDevice.Attach(_serpents).CanTurnAround = true;

            Vector3 toPosition, toLookAt;
            _serpents.PlayingField.GetCameraPositionForLookingAtPlayerCave(out toPosition, out toLookAt);

            _actions.AddDurable(new MoveCameraYaw(
                serpents.Camera,
                5f.UnitsPerSecond(),
                toPosition,
                GetPlayerInitialLookAt(_serpents.PlayingField)));
            _actions.AddOneShot(() => _serpents.PlayerSerpent.Restart(_serpents.PlayingField, 1));
            _actions.AddWhile(time => _serpents.PlayingField.FieldValue(_serpents.PlayerSerpent.Whereabouts).Restricted != Direction.None);
            _actions.AddOneShot(() =>
            {
                _serpentCamera = new SerpentCamera(_serpents.Camera, _serpents.PlayerSerpent, 0, 1, 8);
                while (_serpents.Enemies.Count > 3 + _serpents.Scene) // remove spawned serpents
                    _serpents.Enemies.RemoveAt(0);
                foreach (var enemy in _serpents.Enemies)
                    enemy.DirectionTaker = null;
            });
        }

        public void Update(Camera camera, GameTime gameTime, ref IGameState gameState)
        {
            _serpents.Update(camera, gameTime);
            if (!_actions.Do(gameTime))
                gameState = new PlayingState(_serpents, _serpentCamera);
        }

        public void Draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            _serpents.Draw(camera, drawingReason, shadowMap);
        }

        public static Vector3 GetPlayerInitialLookAt(PlayingField pf)
        {
            return pf.PlayerWhereaboutsStart.GetPosition(pf);
            ;
        }

    }

}
