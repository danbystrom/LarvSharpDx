using System;
using System.Collections.Generic;
using System.Linq;
using factor10.VisionThing;
using factor10.VisionThing.CameraStuff;
using factor10.VisionThing.FloatingText;
using factor10.VisionThing.Util;
using Larv.Field;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using SharpDX.Toolkit.Input;

namespace Larv.Serpent
{
    public class Serpents : VDrawable
    {
        public enum Result
        {
            GameOn,
            PlayerDied,
            LevelComplete
        }

        public readonly LarvContent LContent;
        public readonly FloatingTexts FloatingTexts;
        public readonly ParallelToDo ParallelToDos = new ParallelToDo();

        public readonly CaveModel PlayerCave;
        public readonly CaveModel EnemyCave;
        public readonly Windmill Windmill;

        public PlayingField PlayingField { get; private set; }

        public PlayerSerpent PlayerSerpent { get; private set; }

        public Egg PlayerEgg;
        public readonly List<EnemySerpent> Enemies = new List<EnemySerpent>();
        public readonly List<Egg> EnemyEggs = new List<Egg>();
        public readonly List<Frog> Frogs = new List<Frog>();

        public readonly Random Rnd = new Random();
        private double _onceASecond;

        public readonly Camera Camera;

        public int Scene { get; private set; }
        public int Score { get; private set; }
        public int LivesLeft;
        private int _pendingScore;

        public Serpents(
            LarvContent lcontent,
            Camera camera,
            int scene)
            : base(lcontent.BumpEffect)
        {
            LContent = lcontent;

            FloatingTexts = new FloatingTexts(LContent, LContent.SpriteBatch, LContent.Font);

            PlayerCave = new CaveModel(lcontent);
            EnemyCave = new CaveModel(lcontent);
            Windmill = new Windmill(lcontent, new Vector3(-2, 0.4f, 1));

            Camera = camera;

            Scene = -1;
            Restart(scene);
        }

        public void Restart(int scene)
        {
            if (PlayingField != null)
                PlayingField.Dispose();
            PlayingField = new PlayingField(
                LContent,
                LContent.Content.Load<Texture2D>(@"Textures\woodfloor"),
                scene);

            PlayerCave.SetPosition(PlayingField.PlayerWhereaboutsStart, PlayingField);
            EnemyCave.SetPosition(PlayingField.EnemyWhereaboutsStart, PlayingField);

            if (Scene != scene)
                LContent.Ground.GeneratePlayingField(PlayingField);
            Scene = scene;

            PlayerSerpent = new PlayerSerpent(
                LContent,
                PlayingField);

            Enemies.Clear();
            EnemyEggs.Clear();
            Frogs.Clear();
            PlayerEgg = null;

            var enemies = (7 + scene)/2;
            for (var i = 0; i < enemies; i++)
                Enemies.Add(new EnemySerpent(
                    LContent,
                    PlayingField,
                    i*1.5f,
                    2));

            for (var i = 0; i < 3; i++)
                Frogs.Add(new Frog(
                    LContent,
                    LContent.LoadEffect(@"Effects\SimpleTextureEffect"),
                    this));
        }

        public void ResetScoreAndLives()
        {
            Score = 0;
            _pendingScore = 0;
            LivesLeft = 2;
        }

        public void UpdateScore()
        {
            Score += _pendingScore;
            _pendingScore = 0;
        }

        public Result GameStatus()
        {
            if (PlayerSerpent.SerpentStatus == SerpentStatus.Alive && Enemies.All(e => e.SerpentStatus != SerpentStatus.Alive) && !EnemyEggs.Any())
                return Result.LevelComplete;
            return PlayerSerpent.SerpentStatus != SerpentStatus.Finished ? Result.GameOn : Result.PlayerDied;
        }

        private bool _paused, _pausedEnemy;

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);

            FloatingTexts.Update(camera, gameTime);
            Windmill.Update(camera, gameTime);
            PlayerCave.Update(camera, gameTime);
            EnemyCave.Update(camera,gameTime);

            ParallelToDos.Do(gameTime);

            _paused ^= Camera.KeyboardState.IsKeyPressed(Keys.P);
            _pausedEnemy ^= Camera.KeyboardState.IsKeyPressed(Keys.E);
            if (_paused)
                return;

            if (Camera.KeyboardState.IsKeyPressed(Keys.Z))
                foreach (var enemy in Enemies)
                    enemy.SerpentStatus = SerpentStatus.Ghost;

            _onceASecond += gameTime.ElapsedGameTime.TotalSeconds;
            if (_onceASecond >= 1)
            {
                _onceASecond = 0;

                if (PlayerEgg == null && (Rnd.NextDouble() < 0.03 || Camera.KeyboardState.IsKeyPressed(Keys.D3)))
                    PlayerSerpent.IsPregnant = true;

                if (Rnd.NextDouble() < 0.02 && !Enemies.Any(_ => _.IsPregnant) && Enemies.Any())
                    Enemies[Rnd.Next(Enemies.Count)].IsPregnant = true;
            }

            PlayerSerpent.Update(Camera, gameTime);
            if (PlayerEgg == null)
                PlayerEgg = PlayerSerpent.TimeToLayEgg();

            if(!_pausedEnemy)
                foreach (var enemy in Enemies)
                {
                    int eatenSegments;

                    enemy.Update(Camera, gameTime);
                    if (enemy.EatAt(PlayerSerpent, out eatenSegments))
                        PlayerSerpent.SerpentStatus = SerpentStatus.Ghost;
                    if (enemy.SerpentStatus == SerpentStatus.Alive)
                    {
                        if (PlayerSerpent.EatAt(enemy, out eatenSegments))
                        {
                            enemy.SerpentStatus = SerpentStatus.Ghost;
                            eatenSegments += 10;
                        }
                        if (eatenSegments != 0)
                            AddAndShowScore(eatenSegments*50, PlayerSerpent.Position);
                    }

                    if (enemy.EatFrogOrEgg(PlayerEgg))
                        PlayerEgg = null;

                    var egg = enemy.TimeToLayEgg();
                    if (egg != null)
                        EnemyEggs.Add(egg);
                }
            Enemies.RemoveAll(e => e.SerpentStatus == SerpentStatus.Finished);

            for (var i = EnemyEggs.Count - 1; i >= 0; i--)
            {
                EnemyEggs[i].Update(gameTime);

                if (PlayerSerpent.EatFrogOrEgg(EnemyEggs[i]))
                {
                    AddAndShowScore(100, EnemyEggs[i].Position);
                    EnemyEggs.RemoveAt(i);
                    continue;
                }

                if (!EnemyEggs[i].TimeToHatch())
                    continue;
                var newSerpent = new EnemySerpent(LContent, PlayingField, 0, 0);
                newSerpent.Restart(PlayingField, EnemyEggs[i].Whereabouts);
                EnemyEggs.RemoveAt(i);
                Enemies.Add(newSerpent);
            }

            foreach (var frog in Frogs)
            {
                frog.Update(Camera, gameTime);

                if (PlayerEgg != null && frog.DistanceSquared(PlayerEgg) < 0.4f)
                    PlayerEgg = null;
                EnemyEggs.RemoveAll(_ => frog.DistanceSquared(_) < 0.4f);

                if (PlayerSerpent.EatFrogOrEgg(frog))
                {
                    AddAndShowScore(100, frog.Position);
                    frog.Restart();
                }
                else if (Enemies.Any(enemy => enemy.EatFrogOrEgg(frog)))
                    frog.Restart();
            }
        }

        protected override bool draw(Camera camera, DrawingReason drawingReason, ShadowMap shadowMap)
        {
            PlayerCave.Draw(camera, drawingReason, shadowMap);
            EnemyCave.Draw(camera, drawingReason, shadowMap);
            Windmill.Draw(camera, drawingReason, shadowMap);
            LContent.Ground.Draw(camera, drawingReason, shadowMap);
            PlayingField.Draw(camera, drawingReason, shadowMap);
            LContent.Sky.Draw(camera, drawingReason, shadowMap);

            if (PlayerEgg != null)
                PlayerEgg.Draw(camera, drawingReason, shadowMap);
            foreach (var egg in EnemyEggs)
                egg.Draw(camera, drawingReason, shadowMap);
            foreach (var frog in Frogs)
                frog.Draw(camera, drawingReason, shadowMap);

            var allSerpents = new List<BaseSerpent> {PlayerSerpent};
            allSerpents.AddRange(Enemies);

            foreach (var serpent in allSerpents.Where(_ => _.SerpentStatus == SerpentStatus.Alive))
                serpent.Draw(camera, drawingReason, shadowMap);

            if (drawingReason != DrawingReason.Normal)
                return true;

            if (allSerpents.Any(_ => _.SerpentStatus == SerpentStatus.Ghost))
            {
                LContent.GraphicsDevice.SetBlendState(LContent.GraphicsDevice.BlendStates.AlphaBlend);
                foreach (var serpent in allSerpents.Where(_ => _.SerpentStatus == SerpentStatus.Ghost))
                    serpent.Draw(camera, drawingReason, shadowMap);
                LContent.GraphicsDevice.SetBlendState(LContent.GraphicsDevice.BlendStates.Default);
            }

            var w = LContent.GraphicsDevice.BackBuffer.Width;
            const float fsize = 1.2f;
            LContent.SpriteBatch.Begin();
            LContent.DrawString("Score: {0:000 000}".Fmt(Score), new Vector2(10, 5), fsize);
            LContent.DrawString("Scene: {0}".Fmt(Scene + 1), new Vector2(w/2f, 5), fsize, 0.5f);
            LContent.DrawString("Lives left: {0}".Fmt(LivesLeft), new Vector2(w - 10, 5), fsize, 1);
            LContent.SpriteBatch.End();

            FloatingTexts.Draw(camera, drawingReason, shadowMap);

            Effect.GraphicsDevice.SetDepthStencilState(Effect.GraphicsDevice.DepthStencilStates.Default);
            Effect.GraphicsDevice.SetBlendState(Effect.GraphicsDevice.BlendStates.Opaque);

            return true;
        }

        public void AddAndShowScore(int score, Vector3 position)
        {
            _pendingScore += score;
            FloatingTexts.Items.Add(new FloatingTextItem(
                new PositionHolder(position),
                "+" + score,
                3).SetAlphaAnimation(Color.WhiteSmoke, 0.1f, 0.2f).SetOffsetMovement(Vector3.Up, Vector3.Up * 3));
        }

        public override void Dispose()
        {
            if (PlayingField != null)
                PlayingField.Dispose();
            base.Dispose();
        }

    }

}
