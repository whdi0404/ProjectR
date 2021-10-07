using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObject : RObject
{
    public ItemDataDescriptor Desc { get; private set; }

    public int Amount { get; set; }

    //Item/ItemId
    //WorkBench/AI
    //Pawn/FoF

    public ItemObject(ItemDataDescriptor desc, int amount)
    {
        Desc = desc;
        Amount = amount;
        VisualImage = Resources.Load<Sprite>(desc.Image);
        IndexId = $"Item/{desc.Id}";
    }

    public override void VisualUpdate(float dt)
    {

    }
}