//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Plant : RObject
//{
//    public Plant(DataContainer properties) : base(properties)
//    {
//        if (Properties["plantId"].Equals("grass"))
//            VisualImage = Resources.Load<Sprite>("Plants/Grass");
//        else if (Properties["plantId"].Equals("tree"))
//            VisualImage = Resources.Load<Sprite>("Plants/Tree");
//    }

//    public override void Update(float dt)
//    {
//        Properties.TryGetValue("age", out float age);
//        Properties.TryGetValue("breedableAge", out float breedableAge);
//        age += dt;

//        if (age >= breedableAge)
//        {
//            Properties.TryGetValue("breeding", out float breeding);
//            breeding += dt;

//            Properties.TryGetValue("breedPeriod", out float breedPeriod);
//            if (breeding >= breedPeriod)
//            {
//                breeding -= breedPeriod;
//                Properties.TryGetValue("breedAmount", out int breedAmount);

//                List<Vector2Int> breedCandidate = new List<Vector2Int>();
//                if (GameManager.Instance.World.IsPlantable(MapTilePosition + new Vector2Int(-1, -1)) == true)
//                    breedCandidate.Add(MapTilePosition + new Vector2Int(-1, -1));
//                if (GameManager.Instance.World.IsPlantable(MapTilePosition + new Vector2Int(-1, 0)) == true)
//                    breedCandidate.Add(MapTilePosition + new Vector2Int(-1, 0));
//                if (GameManager.Instance.World.IsPlantable(MapTilePosition + new Vector2Int(-1, 1)) == true)
//                    breedCandidate.Add(MapTilePosition + new Vector2Int(-1, 1));
//                if (GameManager.Instance.World.IsPlantable(MapTilePosition + new Vector2Int(0, -1)) == true)
//                    breedCandidate.Add(MapTilePosition + new Vector2Int(0, -1));
//                if (GameManager.Instance.World.IsPlantable(MapTilePosition + new Vector2Int(0, 1)) == true)
//                    breedCandidate.Add(MapTilePosition + new Vector2Int(0, 1));
//                if (GameManager.Instance.World.IsPlantable(MapTilePosition + new Vector2Int(1, -1)) == true)
//                    breedCandidate.Add(MapTilePosition + new Vector2Int(1, -1));
//                if (GameManager.Instance.World.IsPlantable(MapTilePosition + new Vector2Int(1, 0)) == true)
//                    breedCandidate.Add(MapTilePosition + new Vector2Int(1, 0));
//                if (GameManager.Instance.World.IsPlantable(MapTilePosition + new Vector2Int(1, 1)) == true)
//                    breedCandidate.Add(MapTilePosition + new Vector2Int(1, 1));

//                breedAmount = Mathf.Min(breedAmount, breedCandidate.Count);
//                for (int i = 0; i < breedAmount; ++i)
//                {
//                    int posIdx = Random.Range(0, breedCandidate.Count);
//                    Vector2Int breedPos = breedCandidate[posIdx];
//                    breedCandidate.RemoveAt(posIdx);

//                    Plant plant = new Plant(Properties.Clone("plantId", "breedableAge", "breedPeriod", "breedAmount"));
//                    plant.MapTilePosition = breedPos;
//                    GameManager.Instance.World.AddRObject(plant);
//                }
//            }
//            Properties["breeding"] = breeding;
//        }

//        Properties["age"] = age;
//    }

//    public override void VisualUpdate(float dt)
//    {
//    }
//}