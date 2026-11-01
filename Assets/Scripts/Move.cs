using UnityEngine; // 引入 Unity 基本功能。

public class Move : MonoBehaviour
{
    [Header("移動設定")]

    public float moveSpeed = 3f; // 玩家每秒移動速度。

    public float stopDistance = 0.05f; // 距離目標多近時停止。


    private Vector3 targetPosition; // 玩家目前的移動目標。

    private bool hasTarget = false; // 玩家現在是否有目標。

    private bool canMove = true; // 玩家目前是否允許移動。


    private void Update()
    {
        // 如果目前禁止移動，就不執行後面的移動。
        if (!canMove)
        {
            return;
        }


        // 如果目前有移動目標。
        if (hasTarget)
        {
            // 執行移動。
            MovePlayer();
        }
    }


    // GameManager 會呼叫這個函式，
    // 告訴玩家要走去哪裡。
    public void SetTargetPosition(Vector3 position)
    {
        // 儲存新的目標位置。
        targetPosition = position;

        // 目前地板是平面的，所以保留玩家原本的 Y。
        targetPosition.y = transform.position.y;

        // 表示現在有一個目標。
        hasTarget = true;
    }


    private void MovePlayer()
    {
        // 計算玩家和目標位置之間的距離。
        float distance = Vector3.Distance(
            transform.position,
            targetPosition
        );


        // 如果已經到達目標附近。
        if (distance <= stopDistance)
        {
            // 將玩家放到目標位置。
            transform.position = targetPosition;

            // 清除目前目標。
            hasTarget = false;

            // 結束這次執行。
            return;
        }


        // 玩家往目標位置移動。
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }


    // GameManager 使用這個函式控制玩家是否可以移動。
    public void SetMovementEnabled(bool enabled)
    {
        // 更新移動狀態。
        canMove = enabled;


        // 如果現在禁止移動。
        if (!enabled)
        {
            // 取消目前的移動目標。
            StopMoving();
        }
    }


    // 強制玩家停止。
    public void StopMoving()
    {
        // 清除目前目標。
        hasTarget = false;
    }
}