using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using Larv.Hof;
using Larv.Serpent;
using SharpDX;
using SharpDX.Toolkit;
using Keys = SharpDX.Toolkit.Input.Keys;

namespace Larv.GameStates
{
    class HallOfameState : IGameState
    {
        private readonly Serpents _serpents;
        private readonly HofPainter _hofPainter;
        private readonly HallOfFame.Entry _entry;
        private readonly int _entryIndex;

        private float _cursorFlash;

        public HallOfameState(Serpents serpents)
        {
            _serpents = serpents;
            _hofPainter = new HofPainter(serpents.LContent);
            _entry = new HallOfFame.Entry("", _serpents.Score);
            _entryIndex = _serpents.LContent.HallOfFame.Insert(_entry);
        }

        public void Update(Camera camera, GameTime gameTime, ref IGameState gameState)
        {
            _serpents.Update(camera, gameTime);

            _cursorFlash += (float) gameTime.ElapsedGameTime.TotalSeconds;
            _cursorFlash -= (int) _cursorFlash;

            var kbd = camera.KeyboardState;
            if (kbd.IsKeyPressed(Keys.Enter))
            {
                HofStorage.Save(_serpents.LContent.HallOfFame);
                _serpents.Restart(_serpents.Scene);
                gameState = new AttractState(_serpents);
                return;
            }

            if (kbd.IsKeyPressed(Keys.Back) && _entry.Name.Length >= 1)
                _entry.Name = _entry.Name.Substring(0, _entry.Name.Length - 1);

            var chr = camera.KeyboardState.PressedCharacter();
            if (chr >= ' ' && _entry.Name.Length < 32)
                _entry.Name += chr;
        }

        public void Draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            _serpents.Draw(camera, drawingReason, shadowMap);
            using (_serpents.LContent.UsingSpriteBatch())
                _hofPainter.Paint(Color.LightYellow, _entryIndex, _cursorFlash < 0.5f);
        }

    }

}
