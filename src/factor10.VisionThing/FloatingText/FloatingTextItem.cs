using System;
using SharpDX;

namespace factor10.VisionThing.FloatingText
{
    public class FloatingTextItem
    {
        public IPosition Target;
        public string Text;
        public float Age;
        public float TimeToLive;
        public Func<FloatingTextItem, Vector4> GetColor = _ => Vector4.One;
        public Func<FloatingTextItem, Vector3> GetOffset = _ => Vector3.Zero;
        public Func<FloatingTextItem, float> GetSize = _ => 0.015f;

        public FloatingTextItem(IPosition target, string text, float timeToLive)
        {
            Target = target;
            Text = text;
            TimeToLive = timeToLive;
        }

        public FloatingTextItem SetAlphaAnimation(Color color, float fallInFactor = 0.3f, float fallOutFactor = 0.7f)
        {
            GetColor = _ => new Vector4(color.ToVector3(), alphaAnimate(_.Age/_.TimeToLive,fallInFactor,fallOutFactor));
            return this;
        }

        public FloatingTextItem SetOffsetMovement(Vector3 start, Vector3 end)
        {
            GetOffset = _ => Vector3.Lerp(start, end, _.Age / _.TimeToLive);
            return this;
        }

        private float alphaAnimate(float factor, float fallInFactor, float fallOutFactor)
        {
            if (factor < fallInFactor)
                return factor/fallInFactor;
            if (factor > fallOutFactor)
                return 1 - (factor - fallOutFactor) / (1 - fallOutFactor);
            return 1;
        }

    }

}
