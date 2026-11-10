using UnityEngine;

public class PlantingSpot : MonoBehaviour
{
    [Header("種植區資料")]
    public string spotName = "空種植區";

    [Header("定位點")]
    public Transform offspringSpawnPoint;
    // 子代生成位置。

    public Transform interactionPoint;
    // 玩家走近這個種植區時的定位點。
    // 後續移動只取它的 X、Z。

    [Header("使用狀態")]
    public bool isOccupied = false;
    // 是否已經有植物。

    public bool isReserved = false;
    // 是否正在進行交配，已預留給子代。

    public bool CanUse()
    {
        return !isOccupied && !isReserved;
    }
}