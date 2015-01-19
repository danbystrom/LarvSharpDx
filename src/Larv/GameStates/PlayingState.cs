using System.Linq;
using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using Larv.Serpent;
using Larv.Util;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Input;

namespace Larv.GameStates
{
    class PlayingState : IGameState, ITakeDirection
    {
        private readonly Serpents _serpents;
        private float _delayAfterLevelComplete;

        public SerpentCamera SerpentCamera;

        public PlayingState(Serpents serpents, SerpentCamera serpentCamera)
        {
            SerpentCamera = serpentCamera;
            _serpents = serpents;
            _serpents.PlayerSerpent.DirectionTaker = this;
            _serpents.ParallelToDos.AddOneShot(0.9f, () => _serpents.PlayerCave.OpenDoor = false);
            _serpents.EnemyCave.OpenDoor = false;
        }

        public void Update(Camera camera, GameTime gameTime, ref IGameState gameState)
        {
            _turnAround ^= _serpents.Camera.KeyboardState.IsKeyPressed(Keys.Down);

            SerpentCamera.Move(gameTime);
            _serpents.Update(camera, gameTime);
            _serpents.UpdateScore();
            //camera.UpdateFreeFlyingCamera(gameTime);

            switch (_serpents.GameStatus())
            {
                case Serpents.Result.LevelComplete:
                    _delayAfterLevelComplete += (float) gameTime.ElapsedGameTime.TotalSeconds;
                    if (_delayAfterLevelComplete > 3)
                    {
                        _serpents.PlayerSerpent.DirectionTaker = null;
                        gameState = new LevelCompleteState(_serpents);
                    }
                    break;

                case Serpents.Result.PlayerDied:
                    _serpents.PlayerSerpent.DirectionTaker = null;
                    gameState = new DieState(_serpents);
                    break;
            }
        }

        private bool _isHoldingBothPointers;
        private bool _turnAround;

        RelativeDirection ITakeDirection.TakeDelayedDirection(BaseSerpent serpent)
        {
            return takeDirection(serpent, true);
        }

        RelativeDirection ITakeDirection.TakeDirection(BaseSerpent serpent)
        {
            return takeDirection(serpent, false);
        }

        RelativeDirection takeDirection(BaseSerpent serpent, bool delayed)
        {
            var kbd = _serpents.Camera.KeyboardState;
            var keyLeft = delayed ? kbd.IsKeyPressed(Keys.Left) : kbd.IsKeyDown(Keys.Left);
            var keyRight = delayed ? kbd.IsKeyPressed(Keys.Right) : kbd.IsKeyDown(Keys.Right);

            var pointerPoints = _serpents.Camera.PointerState.Points.Where(_ =>
                _.DeviceType == PointerDeviceType.Touch && _.EventType == PointerEventType.Moved).ToArray();
            var pointerLeft = pointerPoints.Any(_ => _.Position.X < 0.15f);
            var pointerRight = pointerPoints.Any(_ => _.Position.X > 0.5f);
            if (pointerLeft && pointerRight)
            {
                pointerLeft = false;
                pointerRight = false;
                _turnAround = !_isHoldingBothPointers;
                _isHoldingBothPointers = true;
            }
            else
                _isHoldingBothPointers = false;

            var nextDirection = RelativeDirection.Forward;
            if (!delayed && _turnAround)
            {
                nextDirection = RelativeDirection.Backward;
                _turnAround = false;
            }

            if (keyLeft || pointerLeft)
                nextDirection = RelativeDirection.Left;
            else if (keyRight || pointerRight)
                nextDirection = RelativeDirection.Right;
            return nextDirection;
        }

        bool ITakeDirection.CanOverrideRestrictedDirections(BaseSerpent serpent)
        {
            return false;
        }

        public void Draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            _serpents.Draw(camera, drawingReason, shadowMap);
        }

    }

}
