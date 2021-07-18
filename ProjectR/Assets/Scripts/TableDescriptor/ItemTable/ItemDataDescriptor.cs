using System.Collections;
using System.Collections.Generic;
using Table;
using UnityEngine;

public class ItemDataDescriptor : Descriptor
{
    [GoogleColumn]
    public string Image { get; set; }

    [GoogleColumn]
    public int StackAmount { get; set; }
}