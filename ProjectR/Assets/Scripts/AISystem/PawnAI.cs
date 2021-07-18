using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT;

//밥먹기

//잠자기

//건설하기

//만들기

public class DirectControl : CompositeTask
{
    public int ChildCount { get => childList.Count; }

    public override IEnumerable<State> Run(Pawn pawn)
    {
        while(childList.Count > 0)
            foreach (State state in childList[0].Run(pawn))
            {
                if (state == State.Running)
                    yield return State.Running;
                else
                    childList.RemoveAt(0);
            }

        yield return State.Complete;
    }
}

public class MovePosition : ActionTask
{
    private FlowField flowField;
    private Vector2Int goalPosition;
    public MovePosition(Vector2Int goalPosition)
    {
        flowField = new FlowField(GameManager.Instance.WorldMap);
        this.goalPosition = goalPosition;
    }

    public override IEnumerable<State> Run(Pawn pawn)
    {
        if (flowField.Calculate(pawn.MapTilePosition, goalPosition) == false)
            yield return State.Failed;

        //시작정렬(타일중점으로 이동)

        while (pawn.MapTilePosition != goalPosition)
        {
            Vector2 movingDir = flowField.GetMovingDriection(pawn.MapTilePosition);
            pawn.MapPosition += movingDir * pawn.Speed * Time.deltaTime;

            if(pawn.MapTilePosition != goalPosition)
                yield return State.Running;
        }

        //끝정렬(타일중점으로 이동)
        
        yield return State.Complete;
    }
}

public class PawnAI
{
    private Selector aiSelector;
    private DirectControl directControl;
    private IEnumerator<State> runningState;

    private bool isDirectControl;

    public bool IsDirectControl 
    { 
        get => isDirectControl;
        private set 
        {
            if (isDirectControl == value)
                return;

            runningState = null;
            isDirectControl = value;
        }
    }

    public PawnAI()
    {
        aiSelector = new Selector();
        directControl = new DirectControl();
    }

    public void UpdateTick(Pawn pawn)
    {
        if (runningState?.Current == State.Running)
            runningState.MoveNext();
        else
        {
            if (directControl.ChildCount > 0)
                runningState = directControl.Run(pawn).GetEnumerator();
            else if (isDirectControl == false)
                runningState = aiSelector.Run(pawn).GetEnumerator();
            
            runningState?.MoveNext();
        }
    }

    public void AddDirectOrder(Pawn pawn, Node node)
    {
        directControl.AddChild(node);
    }
}
