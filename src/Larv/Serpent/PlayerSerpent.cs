using factor10.VisionThing.CameraStuff;
using Larv.Field;
using Larv.Util;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;

namespace Larv.Serpent
{
    public class PlayerSerpent : BaseSerpent
    {
        public const float SpeedDecreasePerSecond = 0.0025f;
        public const float InitialSpeed = 1.4f;
        public float Speed = InitialSpeed;

        public PlayerSerpent(
            LarvContent lcontent,
            PlayingField playingField)
            : base(
                lcontent,
                playingField,
                lcontent.Load<Texture2D>(@"Textures\snakeskin"),
                lcontent.Load<Texture2D>(@"Textures\snakeskinhead"),
                lcontent.Load<Texture2D>(@"Textures\snakeskinmap"), 
                lcontent.Load<Texture2D>(@"Textures\eggshell"))
        {
            Restart(playingField, 1);
        }

        public void Restart(PlayingField playingField, int length, Whereabouts? whereabouts = null)
        {
            Speed = InitialSpeed;
            Restart(playingField, whereabouts.GetValueOrDefault(PlayingField.PlayerWhereaboutsStart));
            DirectionTaker = null;
            SerpentStatus = SerpentStatus.Alive;
            while (length-- > 0)
                AddTail();
        }

        protected override float ModifySpeed()
        {
            return base.ModifySpeed()*Speed;
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            Speed -= SpeedDecreasePerSecond*(float) gameTime.ElapsedGameTime.TotalSeconds;
            base.Update(camera, gameTime);
        }

        protected override void takeDirection(bool delayed)
        {
            if (delayed && DirectionTaker != null)
            {
                // this is a mess
                var rdir = DirectionTaker.TakeDelayedDirection(this);
                if (rdir == RelativeDirection.Forward)
                    return;
                var dir = HeadDirection.Turn(rdir);
                var possibleLocationTo = _whereabouts.Location.Add(dir);
                if (!PlayingField.CanMoveHere(ref _whereabouts.Floor, _whereabouts.Location, possibleLocationTo))
                    return;
                _tail.RevokePathToWalk(_whereabouts.Location);
                _whereabouts.Direction = dir;
                _whereabouts.Fraction = 0; //-_whereabouts.Fraction; // ???
                _tail.AddPathToWalk(_whereabouts);
                return;
            }

            if (TakeDirection())
                return;

            if (!TryMove(HeadDirection))
                if (!TryMove(_whereabouts.Direction))
                    _whereabouts.Direction = Direction.None;
        }

        protected override Vector4 TintColor()
        {
            if (SerpentStatus == SerpentStatus.Ghost)
                return new Vector4(1.2f, 1.2f, 0.5f, AlphaValue());
            return Vector4.One;
        }

    }

}
