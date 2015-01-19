using System.Linq;

namespace factor10.VisionThing.Util
{
    public class ParallelToDo : ToDoBase
    {
        protected override bool Execute(float delta)
        {
            for (var i = 0; i < Actions.Count;)
            {
                Actions[i].Time += delta;
                if (Actions[i].Action(Actions[i].Time))
                    i++;
                else
                    Actions.RemoveAt(i);
            }
            return Actions.Any();
        }

    }

}
