using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX.Toolkit;

namespace factor10.VisionThing.Util
{
    public abstract class ToDoBase : IDurable
    {
        public class TimedAction
        {
            public float Time;
            public readonly Func<float, bool> Action;
            public readonly ToDoBase ToDoBase;

            public TimedAction(ToDoBase toDoBase, Func<float, bool> action)
            {
                ToDoBase = toDoBase;
                Action = action;
            }

            public virtual bool Do()
            {
                return Action(Time);
            }

        }

        protected readonly List<TimedAction> Actions = new List<TimedAction>();
        protected float ElapsedTime;

        protected abstract bool Execute(float delta);

        public bool Do(GameTime gameTime)
        {
            var delta = (float) gameTime.ElapsedGameTime.TotalSeconds;
            ElapsedTime += delta;
            return Execute(delta);
        }

        public bool Do(float elapsedTime)
        {
            var delta = elapsedTime - ElapsedTime;
            ElapsedTime += elapsedTime;
            return Execute(delta);
        }

        public void InsertNext(params Func<float, bool>[] actions)
        {
            Actions.InsertRange(0, actions.Select(_ => new TimedAction(this, _)));
        }

        public void AddWhile(TimedAction timedAction)
        {
            Actions.Add(timedAction);
        }

        public void AddWhile(Func<float, bool> action)
        {
            AddWhile(new TimedAction(this, action));
        }

        public void AddWait(float timeToWait)
        {
            AddWhile(time => time <= timeToWait);
        }

        public void AddOneShot(Action action)
        {
            AddOneShot(0, action);
        }

        public void AddOneShot(float timeToWait, Action action)
        {
            AddWhile(time =>
            {
                if (time < timeToWait)
                    return true;
                action();
                return false;
            });
        }

        public void AddDurable(IDurable durable)
        {
            AddWhile(durable.Do);
        }

        public void AddDurable(Func<IDurable> getDurable)
        {
            AddOneShot(() =>
            {
                var durable = getDurable();
                InsertNext(durable.Do);
            });
        }

        public void Clear()
        {
            Actions.Clear();
        }

    }

}
