using System.Linq;

namespace factor10.VisionThing.Util
{
    public class SequentialToDo : ToDoBase
    {
        private TimedAction _current;

        protected override bool Execute(float delta)
        {
            if(_current==null)
                if (Actions.Any())
                {
                    _current = Actions[0];
                    Actions.RemoveAt(0);
                }
                else
                    return false;

            _current.Time += delta;
            if (_current.Action(_current.Time))
                return true;
            _current = null;
            return Actions.Any();
        }

    }

}
