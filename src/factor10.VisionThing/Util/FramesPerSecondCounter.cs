using SharpDX.Toolkit;

namespace factor10.VisionThing.Util
{
    public class FramesPerSecondCounter
    {
        private readonly int[] _fpsHalf = new int[2];

        private double _time;
        private int _frames;
        private int _half;

        public void Update(GameTime gameTime)
        {
            _time += gameTime.ElapsedGameTime.TotalSeconds;
            _frames++;
            if (_time > 0.5)
            {
                _fpsHalf[_half] = _frames;
                _frames = 0;
                _time -= 0.5;
                _half = 1 - _half;
            }
        }

        public int FrameRate
        {
            get { return _fpsHalf[0] + _fpsHalf[1]; }
        }

    }

}
