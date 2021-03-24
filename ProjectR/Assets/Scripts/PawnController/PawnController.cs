using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnController
{
    private List<Pawn> selectedPawn = new List<Pawn>();

    public void SelectPawn(Pawn pawn)
    {
        if (selectedPawn.Contains(pawn) == false)
            selectedPawn.Add(pawn);
    }

    public void DeselectPawn(Pawn pawn)
    {
        selectedPawn.Remove(pawn);
    }
}

public abstract class PawnCommandBase
{
    public abstract void InitCommand();

    public abstract void Command(Pawn pawn);
}

public class MovePawnCommand : PawnCommandBase
{
    public override void InitCommand()
    {
        //flow field ����
    }

    public override void Command(Pawn pawn)
    {
        //flow field ����
        throw new System.NotImplementedException();
    }
}