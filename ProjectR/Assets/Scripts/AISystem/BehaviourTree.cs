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

    public abstract class Decorator : Node
    {
        protected Node taskNode;

        public Decorator(Node taskNode)
        {
            this.taskNode = taskNode;
        }
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
            foreach (Node child in childList)
            {
                foreach (State state in child.Run(pawn))
                {
                    if (state == State.Running)
                        yield return State.Running;
                    else if (state == State.Failed)
                        break;
                    else if (state == State.Complete)
                    {
                        yield return State.Complete;
                        yield break;
                    }
                }
            }

            yield return State.Failed;
        }
    }

    public class Sequence : CompositeTask
    {
        public override IEnumerable<State> Run(Pawn pawn)
        {
            foreach (Node child in childList)
            {
                foreach (State state in child.Run(pawn))
                {
                    if (state == State.Running)
                        yield return State.Running;
                    else if (state == State.Complete)
                        break;
                    else if (state == State.Failed)
                    {
                        yield return State.Failed;
                        yield break;
                    }
                }
            }

            yield return State.Complete;
        }
    }

    public abstract class If : Decorator
    {
        public If(Node taskNode) : base(taskNode)
        { 
        }

        protected abstract bool CheckCondition(Pawn pawn);

        public override IEnumerable<State> Run(Pawn pawn)
        {
            if (CheckCondition(pawn) == false)
                yield return State.Failed;

            foreach (State state in taskNode.Run(pawn))
            {
                if (state == State.Running)
                    yield return State.Running;
                else if (state == State.Complete)
                    break;
                else if(state == State.Failed)
                {
                    yield return State.Failed;
                    yield break;
                }
            }

            yield return State.Complete;
        }

    }


    public abstract class While : Decorator
    {
        public While(Node taskNode) : base(taskNode)
        {
        }

        protected abstract bool CheckCondition(Pawn pawn);

        public override IEnumerable<State> Run(Pawn pawn)
        {
            while (CheckCondition(pawn) == true)
            {
                foreach (State state in taskNode.Run(pawn))
                {
                    if (state == State.Running)
                        yield return State.Running;
                    else if (state == State.Complete)
                        break;
                    else if (state == State.Failed)
                    {
                        yield return State.Failed;
                        yield break;
                    }
                }
            }
        }

    }
}
