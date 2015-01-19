using System;
using SharpDX.Toolkit;

namespace factor10.VisionThing
{
    public struct DoubleSin
    {
        private double _angle;

        private readonly float _frequency1;
        private readonly float _frequency2;
        private readonly float _amplitude1;
        private readonly float _amplitude2;
        private readonly float _phase1;
        private readonly float _phase2;

        public DoubleSin(
            float amplitude1,
            float amplitude2,
            float frequency1,
            float frequency2,
            float phase1,
            float phase2)
        {
            _angle = 0;
            _amplitude1 = amplitude1;
            _amplitude2 = amplitude2;
            _frequency1 = frequency1;
            _frequency2 = frequency2;
            _phase1 = phase1;
            _phase2 = phase2;
        }

        public void Update(GameTime gameTime)
        {
            _angle += gameTime.ElapsedGameTime.TotalSeconds;
            //if (_angle > MathHelper.TwoPi*2)
            //    _angle -= MathHelper.TwoPi;
        }

        public double Value
        {
            get { return Math.Sin(_angle*_frequency1 + _phase1)*_amplitude1 + Math.Sin(_angle*_frequency2 + _phase2)*_amplitude2; }
        }
    }

}
