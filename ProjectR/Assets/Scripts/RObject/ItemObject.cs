using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObject : RObject
{
    public SingleItemContainer ItemContainer { get; private set; }

    public ItemObject(ItemDataDescriptor desc)
    {
        ItemContainer = GameManager.Instance.ObjectManager.ItemSystem.CreateSingleItemContainer(this, desc);
        VisualImage = Resources.Load<Sprite>(desc.Image);
        IndexId = $"Item/{desc.Id}";
    }

    public override void VisualUpdate(float dt)
    {

    }
}