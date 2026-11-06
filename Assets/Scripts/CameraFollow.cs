using UnityEngine; // 引入 Unity 基本功能。

public class CameraFollow : MonoBehaviour
{
    // ==========================================
    // 跟隨目標
    // ==========================================

    [Header("跟隨目標")]

    public Transform target;
    // 要跟隨的目標。
    // 這裡指定 Player。


    // ==========================================
    // 相機位置
    // ==========================================

    [Header("相機位置")]

    public Vector3 offset;
    // 相機與玩家之間固定保持的世界座標距離。


    public float followSpeed = 5f;
    // 相機跟隨玩家的平滑速度。


    // ==========================================
    // 初始化
    // ==========================================

    private void Start()
    {
        // 如果已經指定玩家。
        if (target != null)
        {
            // 根據遊戲開始時的相機位置與玩家位置，
            // 自動計算兩者之間的距離。
            offset =
                transform.position -
                target.position;
        }
    }


    // ==========================================
    // 相機跟隨
    // ==========================================

    private void LateUpdate()
    {
        // 如果沒有指定玩家。
        if (target == null)
        {
            return;
        }


        // 計算相機應該移動到的位置。
        Vector3 targetPosition =
            target.position +
            offset;


        // 平滑地讓相機靠近目標位置。
        transform.position =
            Vector3.Lerp(
                transform.position,
                targetPosition,
                followSpeed * Time.deltaTime
            );


        // 注意：
        // 這裡完全沒有修改 transform.rotation。
        //
        // 所以不管 Player 怎麼轉，
        // Camera 都會保持自己原本的旋轉角度。
    }
}