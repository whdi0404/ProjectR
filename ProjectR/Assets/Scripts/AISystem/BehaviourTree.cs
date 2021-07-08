using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public enum State 
    { 
        Failed,
        Running,
        Complete,
    };
    
    public abstract class Node
    {
        public abstract IEnumerable<State> Run(Pawn pawn);
    }
    public abstract class ActionTask : Node
    {
    }

    public abstract class CompositeTask : Node
    {
        protected List<Node> childList = new List<Node>();
        protected List<Node> GetChild() { return childList; }
        public void AddChild(Node node)
        {
            childList.Add(node);
        }
    }

    public class Selector : CompositeTask
    {
        public override IEnumerable<State> Run(Pawn pawn)
        {
            State childState = State.Complete;
            foreach (Node child in childList)
            {
                foreach (State state in child.Run(pawn))
                {
                    if (state == State.Running)
                        yield return State.Running;
                    else
                    {
                        childState = state;
                        break;
                    }
                }

                if (childState == State.Complete)
                {
                    break;
                }
            }

            yield return childState;
        }
    }

    public class Sequence : CompositeTask
    {
        public override IEnumerable<State> Run(Pawn pawn)
        {
            State childState = State.Complete;
            foreach (Node child in childList)
            {
                foreach (State state in child.Run(pawn))
                {
                    if (state == State.Running)
                        yield return State.Running;
                    else
                    {
                        childState = state;
                        break;
                    }
                }

                if (childState == State.Failed)
                {
                    break;
                }
            }

            yield return childState;
        }
    }
}
