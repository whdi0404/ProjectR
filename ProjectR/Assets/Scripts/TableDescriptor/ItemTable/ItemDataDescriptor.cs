using System.Collections;
using System.Collections.Generic;
using Table;
using UnityEngine;

public class ItemDataDescriptor : Descriptor
{
    [GoogleColumn]
    public string Image { get; private set; }

    [GoogleColumn]
    public int StackAmount { get; private set; }
}
