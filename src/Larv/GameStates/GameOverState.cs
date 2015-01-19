using System;
using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.Util;
using Larv.Serpent;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;

namespace Larv.GameStates
{
    class GameOverState : IGameState
    {
        private readonly Serpents _serpents;
        private readonly SequentialToDo _todo  = new SequentialToDo();

        public GameOverState(Serpents serpents)
        {
            _serpents = serpents;
            _todo.AddDurable(AttractState.GetOverviewMoveCamera(_serpents.Camera));
            _todo.AddWait(2);
        }

        public void Update(Camera camera, GameTime gameTime, ref IGameState gameState)
        {
            _serpents.Update(camera, gameTime);
            if (_todo.Do(gameTime))
                return;
            gameState = _serpents.LContent.HallOfFame.HasMadeIt(_serpents.Score)
                ? (IGameState) new HallOfameState(_serpents)
                : new AttractState(_serpents);
        }

        public void Draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            _serpents.Draw(camera, drawingReason, shadowMap);

            var gd = _serpents.LContent.GraphicsDevice;
            var sb = _serpents.LContent.SpriteBatch;
            var font = _serpents.LContent.Font;

            const string text = "GAME OVER";
            var fsize = _serpents.LContent.FontScaleRatio*6;
            var ssize = new Vector2(gd.BackBuffer.Width, gd.BackBuffer.Height);

            sb.Begin(SpriteSortMode.Deferred, gd.BlendStates.NonPremultiplied);
            sb.DrawString(font, text, (ssize - fsize * font.MeasureString(text)) / 2, Color.LightYellow, 0, Vector2.Zero, fsize, SpriteEffects.None, 0);
            sb.End();

            gd.SetDepthStencilState(gd.DepthStencilStates.Default);
            gd.SetBlendState(gd.BlendStates.Opaque);

        }

    }

}
