using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Util;
using Larv.Serpent;
using Larv.Util;
using SharpDX;
using SharpDX.Toolkit;
using System.Linq;

namespace Larv.GameStates
{
    class LevelCompleteState : IGameState, ITakeDirection
    {
        private readonly Serpents _serpents;
        private readonly HomingDevice _homingDevice;
        private MoveCamera _moveCamera;

        private readonly SequentialToDo _todo = new SequentialToDo();

        private bool _serpentIsHome;
        private bool _haltSerpents;
        private bool _homeIsNearCaveEntrance;
        private int _bonusLives;

        public LevelCompleteState(Serpents serpents)
        {
            _serpents = serpents;

            _homingDevice = new HomingDevice(_serpents);
            _serpents.PlayerSerpent.DirectionTaker = this;

            Vector3 toPosition, toLookAt;
            _serpents.PlayingField.GetCameraPositionForLookingAtPlayerCave(out toPosition, out toLookAt);

            _moveCamera = new MoveCamera(
                _serpents.Camera,
                4.5f.UnitsPerSecond(),
                toPosition,
                () => _serpents.PlayerSerpent.LookAtPosition);

            _serpents.PlayerCave.OpenDoor = true;

            // wait until serpent is in cave, then give length bonus
            _todo.AddOneShot(() => _homeIsNearCaveEntrance = true);
            _todo.AddWhile(time => !_serpentIsHome);
            _todo.AddOneShot(() =>
            {
                _serpents.PlayerSerpent.IsPregnant = false;
                _haltSerpents = true;
            });
            for (var i = 0; i < _serpents.PlayerSerpent.Length; i++)
                _todo.AddOneShot(1, () =>
                {
                    var tailSegement = _serpents.PlayerSerpent.RemoveTailWhenLevelComplete();
                    if (tailSegement != null)
                        _serpents.AddAndShowScore(500, tailSegement.Position);
                });

            // wait until all bonus texts gone
            _todo.AddWhile(time => _serpents.FloatingTexts.Items.Any());
            _todo.AddOneShot(() =>
            {
                _serpentIsHome = false;
                _haltSerpents = false;
                _homeIsNearCaveEntrance = false;
            });
            //_todo.AddWhile(time => !_serpentIsHome);

            _todo.AddOneShot(() =>
            {
                if (_serpents.PlayerEgg == null)
                {
                    _haltSerpents = false;
                    return;
                }
                _bonusLives = 1;
                _serpents.PlayerSerpent.DirectionTaker = null;
                var playerEggPosition = _serpents.PlayerEgg.Position;
                _moveCamera = new MoveCamera(_serpents.Camera, 2f.Time(), toPosition, () => playerEggPosition);
                // wait two sec (for camera) and then drive the baby home
                _todo.InsertNext(
                    time => time < 2,
                    time =>
                    {
                        _serpents.PlayerSerpent.Restart(_serpents.PlayingField, 0, _serpents.PlayerEgg.Whereabouts);
                        _serpents.PlayerEgg = null;
                        _serpents.PlayerSerpent.DirectionTaker = this;
                        _haltSerpents = false;
                        _serpentIsHome = false;
                        return false;
                    });
            });

            // make sure the camera aims at a serpent (the original or the new born baby)
            _todo.AddOneShot(() =>
            {
                _moveCamera = new MoveCamera(_serpents.Camera, 1f.Time(), toPosition, () => _serpents.PlayerSerpent.LookAtPosition);
            });

            _todo.AddWhile(time => (!_serpentIsHome || _serpents.FloatingTexts.Items.Any()) && time < 10);
        }

        RelativeDirection ITakeDirection.TakeDelayedDirection(BaseSerpent serpent)
        {
            return RelativeDirection.Forward;
        }

        RelativeDirection ITakeDirection.TakeDirection(BaseSerpent serpent)
        {
            _serpentIsHome = _homingDevice.PlayerPathFinder.GetDistance(_serpents.PlayerSerpent.Whereabouts) < (_homeIsNearCaveEntrance ? 6 : 3);
            _haltSerpents |= _serpentIsHome;
            return _homingDevice.TakeDirection(serpent);
        }

        bool ITakeDirection.CanOverrideRestrictedDirections(BaseSerpent serpent)
        {
            return true;
        }

        public void Update(Camera camera, GameTime gameTime, ref IGameState gameState)
        {
            _serpents.UpdateScore();
            if (_haltSerpents)
            {
                _serpents.FloatingTexts.Update(camera, gameTime);
                _serpents.PlayerCave.Update(camera, gameTime);
            }
            else
                _serpents.Update(camera, gameTime);
            _moveCamera.Move(gameTime);
            if (_todo.Do(gameTime))
                return;

            _serpents.LivesLeft += _bonusLives;
            gameState = new GotoBoardState(_serpents, _serpents.Scene + 1);
        }

        public void Draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            _serpents.Draw(camera,drawingReason,shadowMap);
        }

    }

}
