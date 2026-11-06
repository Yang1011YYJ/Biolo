using UnityEngine; // 引入 Unity 基本功能。

public class MissionTargetIndicator : MonoBehaviour
{
    // ==========================================
    // 玩家
    // ==========================================
     
    [Header("玩家")]

    public Transform player;
    // 玩家 Transform。
    // 用來計算玩家與任務目標之間的距離。


    // ==========================================
    // 攝影機
    // ==========================================

    [Header("攝影機")]

    public Camera mainCamera;
    // 主攝影機。
    // 用來讓指示器始終朝向玩家畫面。


    // ==========================================
    // 指示器顯示物件
    // ==========================================

    [Header("指示器")]

    public GameObject indicatorVisual;
    // 真正顯示的 World Space Canvas。


    public Vector3 worldOffset =
        new Vector3(
            0f,
            2.5f,
            0f
        );
    // 指示器與目標之間的高度差。
    // 預設顯示在目標上方 2.5 公尺。


    public float hideDistance = 3f;
    // 玩家靠近目標多少距離後，
    // 自動隱藏指示器。


    // ==========================================
    // 目前任務目標
    // ==========================================

    private Transform currentTarget;
    // 紀錄目前要指向哪個任務目標。


    // ==========================================
    // 初始化
    // ==========================================

    private void Start()
    {
        // 如果 Inspector 沒有指定 Camera。
        if (mainCamera == null)
        {
            // 自動尋找 Main Camera。
            mainCamera = Camera.main;
        }


        // 遊戲開始時先隱藏指示器。
        HideIndicator();
    }


    // ==========================================
    // 更新指示器位置
    // ==========================================

    private void LateUpdate()
    {
        // 如果目前沒有任務目標。
        if (currentTarget == null)
        {
            // 確保指示器隱藏。
            HideIndicator();

            return;
        }


        // 如果沒有指定玩家。
        if (player == null)
        {
            // 無法計算距離。
            return;
        }


        // 如果目前任務目標本身已經被關閉。
        if (!currentTarget.gameObject.activeInHierarchy)
        {
            // 暫時隱藏指示器。
            HideIndicator();

            return;
        }


        // ======================================
        // 更新位置
        // ======================================

        // 將指示器放到目標上方。
        transform.position =
            currentTarget.position +
            worldOffset;


        // ======================================
        // 計算玩家距離
        // ======================================

        // 計算玩家與任務目標之間的距離。
        float distance =
            Vector3.Distance(
                player.position,
                currentTarget.position
            );


        // 如果玩家已經靠近目標。
        if (distance <= hideDistance)
        {
            // 隱藏指示器。
            HideIndicator();

            return;
        }


        // ======================================
        // 顯示指示器
        // ======================================

        // 玩家還沒靠近，
        // 顯示任務指示器。
        ShowIndicator();


        // ======================================
        // 讓指示器朝向 Camera
        // ======================================

        // 如果有主攝影機。
        if (mainCamera != null)
        {
            // 讓 World Space UI 保持與攝影機相同的觀看方向。
            transform.LookAt(
                transform.position +
                mainCamera.transform.rotation *
                Vector3.forward,

                mainCamera.transform.rotation *
                Vector3.up
            );
        }
    }


    // ==========================================
    // 設定新的任務目標
    // ==========================================

    public void SetTarget(Transform newTarget)
    {
        // 儲存新的任務目標。
        currentTarget = newTarget;


        // 如果傳入的是空值。
        if (currentTarget == null)
        {
            // 直接關閉指示器。
            HideIndicator();

            return;
        }


        // 先將指示器移到正確位置。
        transform.position =
            currentTarget.position +
            worldOffset;
    }


    // ==========================================
    // 清除任務目標
    // ==========================================

    public void ClearTarget()
    {
        // 清除目前目標。
        currentTarget = null;


        // 隱藏指示器。
        HideIndicator();
    }


    // ==========================================
    // 顯示指示器
    // ==========================================

    private void ShowIndicator()
    {
        // 確認顯示物件存在。
        if (indicatorVisual == null)
        {
            return;
        }


        // 如果目前沒有顯示。
        if (!indicatorVisual.activeSelf)
        {
            // 開啟顯示。
            indicatorVisual.SetActive(true);
        }
    }


    // ==========================================
    // 隱藏指示器
    // ==========================================

    private void HideIndicator()
    {
        // 確認顯示物件存在。
        if (indicatorVisual == null)
        {
            return;
        }


        // 如果目前有顯示。
        if (indicatorVisual.activeSelf)
        {
            // 關閉顯示。
            indicatorVisual.SetActive(false);
        }
    }
}