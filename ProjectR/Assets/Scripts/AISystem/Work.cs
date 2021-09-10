//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class WorkSystem
//{
//    private List<WorkPlace> workPlaces = new List<WorkPlace>();
//}

//public class WorkPlace
//{
//    public enum Direction
//    { 
//        Left = 1 << 0,
//        Top = 1 << 1,
//        Right = 1 << 2,
//        Bottom = 1 << 3,
//    }

//    private List<IWork> workList = new List<IWork>();
//}

//public interface IWork
//{ 

//}

//public class BuildWork : IWork
//{
//    public Inventory Inventory { get; private set; }

//    public float RemainWorkload { get; private set; }

//    public bool IsFilledMaterials
//    {
//        get
//        {
//            foreach (var reqItem in planningDesc.ReqItemList)
//            {
//                if (Inventory.GetItemAmount(reqItem.ItemDesc) < reqItem.Amount)
//                    return false;
//            }

//            return true;
//        }
//    }

//    public void Work(float work)
//    {
//        RemainWorkload -= work;

//        if (RemainWorkload <= 0)
//        {
//            Complete();
//        }
//    }

//    public List<RequireItem> GetRemainReqItemList()
//    {
//        List<RequireItem> reqItemList = new List<RequireItem>();
//        foreach (var reqItem in planningDesc.ReqItemList)
//        {
//            int remainAmount = reqItem.Amount - Inventory.GetItemAmount(reqItem.ItemDesc);
//            if (remainAmount > 0)
//                reqItemList.Add(new RequireItem() { ItemDesc = reqItem.ItemDesc, Amount = remainAmount });
//        }

//        return reqItemList;
//    }

//    public void Complete()
//    {
//        GameManager.Instance.DestroyRObject(this);

//        if (PlanningDesc.Structure != null)
//            GameManager.Instance.WorldMap.SetTile(MapTilePosition, PlanningDesc.Structure);

//        //if (planObject.PlanningDesc.InstallObject != null)
//        //GameManager.Instance.CreateRObject(new InstallObject());
//    }

//    public bool AA()
//    {
        
//    }
//}