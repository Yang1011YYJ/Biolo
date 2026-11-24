using UnityEngine;

public class PlantingSpot : MonoBehaviour
{
    [Header("種植區資料")]

    [Tooltip("操作選單中顯示的種植區名稱。")]
    public string spotName = "空種植區";

    [Header("定位點")]

    [Tooltip("子代生成的位置，包含生成時的高度。")]
    public Transform offspringSpawnPoint;

    [Header("互動設定")]

    [Tooltip("玩家距離操作定位點多近時，可以開啟選單。只比較 X、Z 距離。")]
    [Min(0.1f)]
    public float interactionDistance = 2f;

    [Header("使用狀態")]

    [Tooltip("此種植區是否已經有植物。有植物時不能再次使用。")]
    public bool isOccupied = false;

    [Tooltip("是否已預留給正在進行的交配實驗，避免重複使用。")]
    public bool isReserved = false;

    [Header("已種植資料")]

    [Tooltip("此種植區播下的種子資料，保留其編號與親代來源。")]
    public SeedData plantedSeed;

    [Tooltip("此種植區目前生成的植物物件。")]
    public GameObject plantedPlant;

    [Header("子代牌子")]

    [Tooltip("播種成功後生成資訊牌子的位置與旋轉。")]
    public Transform signSpawnPoint;

    [Tooltip("此種植區生成的子代資訊牌子，由程式自動保存。")]
    public PeaInfoSign plantedSign;

    public bool CanUse()
    {
        return !isOccupied && !isReserved;
    }

    public bool IsPlayerInRange(Transform player)
    {
        if (player == null)
        {
            return false;
        }

        InteractionDebug.LogDistance(
            "操作種植區",
            player,
            transform,
            interactionDistance
        );

        // 直接以種植區自身的位置判斷水平距離。
        Vector3 difference = player.position - transform.position;
        difference.y = 0f;

        return difference.sqrMagnitude <=
               interactionDistance * interactionDistance;
    }
}