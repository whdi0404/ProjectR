using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObject : RObject
{
    public ItemDataDescriptor Desc { get; private set; }

    public int Amount { get; set; }

    public ItemObject(ItemDataDescriptor desc, int amount)
    {
        Desc = desc;
        Amount = amount;
        VisualImage = Resources.Load<Sprite>(desc.Image);
    }

    public override void VisualUpdate(float dt)
    {

    }
}
