using UnityEngine;

public class PeaInfoSign : MonoBehaviour
{
    [Tooltip("這塊牌子對應的種子來源資料，由播種程式自動指定。")]
    public SeedData seed;

    [Tooltip("牌子資訊面板顯示的標題。")]
    public string title = "第一子代（F1）";

    [Tooltip("目前可以觀察到的植株性狀。")]
    public string observedTrait = "高莖";

    [Tooltip("玩家距離牌子多近時可以查看，只比較 X、Z 距離。")]
    [Min(0.1f)]
    public float interactionDistance = 2.5f;

    public bool IsPlayerInRange(Transform player)
    {
        if (player == null)
        {
            return false;
        }

        InteractionDebug.LogDistance(
            "查看子代牌子",
            player,
            transform,
            interactionDistance
        );

        Vector3 difference = player.position - transform.position;
        difference.y = 0f;

        return difference.sqrMagnitude <=
               interactionDistance * interactionDistance;
    }
}