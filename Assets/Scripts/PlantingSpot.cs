using UnityEngine;

public class PlantingSpot : MonoBehaviour
{
    [Header("種植區資料")]

    [Tooltip("操作選單中顯示的種植區名稱。")]
    public string spotName = "空種植區";

    [Header("定位點")]

    [Tooltip("子代生成的位置，包含生成時的高度。")]
    public Transform offspringSpawnPoint;

    [Tooltip("玩家操作此種植區時的站立定位點。系統移動只使用其 X、Z。")]
    public Transform interactionPoint;

    [Header("互動設定")]

    [Tooltip("玩家距離操作定位點多近時，可以開啟選單。只比較 X、Z 距離。")]
    [Min(0.1f)]
    public float interactionDistance = 2f;

    [Header("使用狀態")]

    [Tooltip("此種植區是否已經有植物。有植物時不能再次使用。")]
    public bool isOccupied = false;

    [Tooltip("是否已預留給正在進行的交配實驗，避免重複使用。")]
    public bool isReserved = false;

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

        Transform distanceTarget = interactionPoint != null
        ? interactionPoint
        : transform;

        InteractionDebug.LogDistance(
            "操作種植區",
            player,
            distanceTarget,
            interactionDistance
        );

        Vector3 targetPosition = distanceTarget.position;

        Vector3 difference = player.position - targetPosition;

        // 高度不影響互動距離。
        difference.y = 0f;

        return difference.sqrMagnitude <=
               interactionDistance * interactionDistance;
    }
}