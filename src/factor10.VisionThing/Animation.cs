using System;
using System.Collections.Generic;
using SharpDX;

namespace factor10.VisionThing
{
    public class ObjectAnimation
    {
        Vector3 startPosition, endPosition, startRotation, endRotation;
        TimeSpan duration;
        bool loop;

        TimeSpan elapsedTime = TimeSpan.FromSeconds(0);

        public Vector3 Position { get; private set; }
        public Vector3 Rotation { get; private set; }

        public ObjectAnimation(Vector3 StartPosition, Vector3 EndPosition,
            Vector3 StartRotation, Vector3 EndRotation, TimeSpan Duration, bool Loop)
        {
            this.startPosition = StartPosition;
            this.endPosition = EndPosition;
            this.startRotation = StartRotation;
            this.endRotation = EndRotation;
            this.duration = Duration;
            this.loop = Loop;
            Position = startPosition;
            Rotation = startRotation;
        }

        public void Update(TimeSpan Elapsed)
        {
            // Update the time
            this.elapsedTime += Elapsed;

            // Determine how far along the duration value we are (0 to 1)
            float amt = (float)elapsedTime.TotalSeconds / (float)duration.TotalSeconds;

            if (loop)
                while (amt > 1) // Wrap the time if we are looping
                    amt -= 1;
            else // Clamp to the end value if we are not
                amt = MathUtil.Clamp(amt, 0, 1); 

            // Update the current position and rotation
            Position = Vector3.Lerp(startPosition, endPosition, amt);
            Rotation = Vector3.Lerp(startRotation, endRotation, amt);
        }
    }

    public class ObjectAnimationFrame
    {
        public Vector3 Position { get; private set; }
        public Vector3 Rotation { get; private set; }
        public TimeSpan Time { get; private set; }

        public ObjectAnimationFrame(Vector3 Position, Vector3 Rotation, TimeSpan Time)
        {
            this.Position = Position;
            this.Rotation = Rotation;
            this.Time = Time;
        }
    }

    public class KeyframedObjectAnimation
    {
        List<ObjectAnimationFrame> frames = new List<ObjectAnimationFrame>();
        bool loop;
        TimeSpan elapsedTime = TimeSpan.FromSeconds(0);

        public Vector3 Position { get; private set; }
        public Vector3 Rotation { get; private set; }

        public KeyframedObjectAnimation(List<ObjectAnimationFrame> Frames, bool Loop)
        {
            this.frames = Frames;
            this.loop = Loop;
            Position = Frames[0].Position;
            Rotation = Frames[0].Rotation;
        }

        public void Update(TimeSpan Elapsed)
        {
            // Update the time
            this.elapsedTime += Elapsed;

            TimeSpan totalTime = elapsedTime;
            TimeSpan end = frames[frames.Count - 1].Time;

            if (loop) // Loop around the total time if necessary
                while (totalTime > end)
                    totalTime -= end;
            else // Otherwise, clamp to the end values
            {
                Position = frames[frames.Count - 1].Position;
                Rotation = frames[frames.Count - 1].Rotation;
                return;
            }

            int i = 0;

            // Find the index of the current frame
            while(frames[i + 1].Time < totalTime)
                i++;

            // Find the time since the beginning of this frame
            totalTime -= frames[i].Time;

            // Find how far we are between the current and next frame (0 to 1)
            float amt = (float)((totalTime.TotalSeconds) / 
                (frames[i + 1].Time - frames[i].Time).TotalSeconds);

            // Interpolate position and rotation values between frames
            //Position = catmullRom3D(
            //    frames[wrap(i - 1, frames.Count - 1)].Position,
            //    frames[wrap(i, frames.Count - 1)].Position,
            //    frames[wrap(i + 1, frames.Count - 1)].Position,
            //    frames[wrap(i + 2, frames.Count - 1)].Position,
            //    amt);

            Rotation = Vector3.Lerp(frames[i].Rotation, frames[i + 1].Rotation, amt);
        }

        //Vector3 catmullRom3D(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float amt)
        //{
        //    return new Vector3(
        //        MathUtil.CatmullRom(v1.X, v2.X, v3.X, v4.X, amt),
        //        MathUtil.CatmullRom(v1.Y, v2.Y, v3.Y, v4.Y, amt),
        //        MathUtil.CatmullRom(v1.Z, v2.Z, v3.Z, v4.Z, amt));
        //}

        // Wraps the "value" argument around [0, max]
        int wrap(int value, int max)
        {
            while (value > max)
                value -= max;

            while (value < 0)
                value += max;

            return value;
        }
    }
}
