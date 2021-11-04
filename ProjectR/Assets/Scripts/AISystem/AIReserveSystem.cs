using BT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AIReserveSystem : IObjectManagerListener
{
    private SmartDictionary<Node, List<AIReserver>> reserverDict = new SmartDictionary<Node, List<AIReserver>>();

    public void RegistReserver(AIReserver reserver)
    {
        if (reserver.Node == null)
            return;

        if (reserverDict.TryGetValue(reserver.Node, out var list) == false)
        {
            list = new List<AIReserver>();
            reserverDict.Add(reserver.Node, list);
        }

        list.Add(reserver);
    }

    public void BreakReserver(AIReserver reserver)
    {
        reserverDict[reserver.Node].Remove(reserver);
    }

    public List<TAIReserver> GetAllReserverFromAI<TAIReserver>(Node node) where TAIReserver : AIReserver
    {
        List<TAIReserver> retVal = new List<TAIReserver>();
        if (reserverDict.TryGetValue(node, out List<AIReserver> reserverList) == true)
        {
            foreach (var reserver in reserverList)
            {
                if (reserver is TAIReserver)
                    retVal.Add(reserver as TAIReserver);
            }
        }

        return retVal;
    }

    public List<TAIReserver> GetAllReserverFromTarget<TAIReserver>(RObject rObj) where TAIReserver : AIReserver
    {
        List<TAIReserver> retVal = new List<TAIReserver>();
        foreach (var reserverList in reserverDict.Values)
        {
            foreach (var reserver in reserverList)
            {
                if (reserver is TAIReserver && reserver.Target == rObj)
                    retVal.Add(reserver as TAIReserver);
            }
        }

        return retVal;
    }

    public void BreakAllReserverFromNode(Node node)
    {
        foreach (var reserver in GetAllReserverFromAI<AIReserver>(node))
        {
            reserver.Break();
        }
    }

    public void OnCreateObject(RObject rObject)
    {
    }

    public void OnDestroyObject(RObject rObject)
    {
        HashSet<Node> cancelAIList = new HashSet<Node>();
        foreach (var reserverList in reserverDict.Values)
        {
            foreach (var reserver in reserverList)
            {
                if (reserver.Target == rObject)
                    cancelAIList.Add(reserver.Node);
            }
        }

        foreach (var cancelNode in cancelAIList)
        {
            cancelNode.Cancel();
        }
    }
}

public class AIReserver
{
    public Node Node { get; private set; }
    public RObject Target { get; private set; }

    public void Break()
    {
        GameManager.Instance.AIReserveSystem.BreakReserver(this);
    }

    public AIReserver(Node node, RObject target)
    {
        Node = node;
        Target = target;
    }
}

public class ItemReserver : AIReserver
{
    public enum Type { haul, pickup };

    public ItemContainer ItemContainer { get; private set; }

    public Type ActionType { get; private set; }

    public List<Item> ItemList { get; private set; }

    public ItemReserver(Node node, ItemContainer itemContainer, Type type, List<Item> itemList) : base(node, itemContainer.ParentObject)
    {
        ActionType = type;
        ItemContainer = itemContainer;
        ItemList = itemList;
    }
}
