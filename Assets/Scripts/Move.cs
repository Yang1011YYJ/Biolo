using UnityEngine; // 引入 Unity 基本功能。
using UnityEngine.AI; // 引入 NavMeshAgent 導航功能。

public class Move : MonoBehaviour
{
    // ==========================================
    // NavMesh Agent
    // ==========================================

    [Header("NavMesh")]

    public NavMeshAgent agent;
    // 玩家身上的 NavMeshAgent。
    // 負責實際尋路與移動。


    // ==========================================
    // 移動狀態
    // ==========================================

    private bool canMove = true;
    // 紀錄目前是否允許玩家移動。


    private void Awake()
    {
        // 如果 Inspector 沒有手動指定 NavMeshAgent。
        if (agent == null)
        {
            // 自動取得同一個 GameObject 上的 NavMeshAgent。
            agent = GetComponent<NavMeshAgent>();
        }
    }


    // ==========================================
    // 設定玩家移動目的地
    // ==========================================

    public void SetTargetPosition(Vector3 position)
    {
        // 如果現在不允許玩家移動。
        if (!canMove)
        {
            // 就不設定新的目的地。
            return;
        }


        // 如果沒有找到 NavMeshAgent。
        if (agent == null)
        {
            // 顯示錯誤訊息。
            Debug.LogError("Move 找不到 NavMeshAgent。");

            // 不繼續執行。
            return;
        }


        // 如果 Agent 目前沒有位於 NavMesh 上。
        if (!agent.isOnNavMesh)
        {
            // 顯示錯誤訊息。
            Debug.LogWarning("玩家目前不在 NavMesh 上。");

            // 不設定目的地。
            return;
        }


        // 告訴 NavMeshAgent：
        // 玩家要移動到這個位置。
        agent.SetDestination(position);
    }


    // ==========================================
    // 開啟／關閉玩家移動
    // ==========================================

    public void SetMovementEnabled(bool enabled)
    {
        // 更新目前是否允許移動。
        canMove = enabled;


        // 如果沒有 NavMeshAgent。
        if (agent == null)
        {
            // 不執行後續內容。
            return;
        }


        // 如果 Agent 目前不在 NavMesh 上。
        if (!agent.isOnNavMesh)
        {
            // 不操作 Agent。
            return;
        }


        // 如果現在禁止移動。
        if (!enabled)
        {
            // 立即停止目前的移動路徑。
            StopMoving();
        }


        // 如果允許移動，
        // 不需要特別啟動 Agent。
        // 下一次 SetTargetPosition() 時就會重新尋路。
    }


    // ==========================================
    // 停止目前移動
    // ==========================================

    public void StopMoving()
    {
        // 如果沒有 NavMeshAgent。
        if (agent == null)
        {
            // 不執行。
            return;
        }


        // 如果 Agent 目前不在 NavMesh 上。
        if (!agent.isOnNavMesh)
        {
            // 不執行。
            return;
        }


        // 清除目前正在走的路徑。
        agent.ResetPath();
    }
}