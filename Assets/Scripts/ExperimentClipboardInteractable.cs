using UnityEngine; // 引入 Unity 基本功能。

public class ExperimentClipboardInteractable : MonoBehaviour
{
    // ==========================================
    // 互動距離
    // ==========================================

    [Header("互動設定")]

    public float interactionDistance = 2.5f;
    // 玩家必須在這個距離內，
    // 才可以取得實驗紀錄板。


    // ==========================================
    // 實驗紀錄板資料
    // ==========================================

    [Header("背包設定")]

    public int clipboardItemID = 1;
    // 實驗紀錄板在 ItemDatabase 裡的 ID。
    // 我們目前設定的 ExperimentClipboard ID 是 1。


    public InventoryManager inventoryManager;
    // 新版 Grid 背包系統。


    // ==========================================
    // 任務系統
    // ==========================================

    [Header("任務系統")]

    public MissionController missionController;
    // 用來確認目前是不是 GetClipboard 任務。


    // ==========================================
    // 玩家嘗試取得紀錄板
    // ==========================================

    public bool TryPickup(Transform player)
    {
        // 如果沒有取得玩家 Transform，
        // 就不能進行距離判斷。
        if (player == null)
        {
            return false;
        }


        // --------------------------------------
        // 1. 判斷目前任務能不能拿紀錄板
        // --------------------------------------

        // 如果 MissionController 沒有設定。
        if (missionController == null)
        {
            Debug.LogError(
                "ExperimentClipboardInteractable 尚未指定 MissionController。"
            );

            return false;
        }


        // 如果目前任務還沒有進入 GetClipboard。
        if (!missionController.CanGetClipboard())
        {
            // 按照我們目前的設計，
            // 在任務尚未觸發以前點擊紀錄板不會有任何反應。
            return false;
        }


        // --------------------------------------
        // 2. 判斷玩家距離
        // --------------------------------------

        InteractionDebug.LogDistance(
            "拾取實驗紀錄板",
            player,
            transform,
            interactionDistance
        );

        // 計算玩家與紀錄板之間的距離。
        float distance =
            Vector3.Distance(
                player.position,
                transform.position
            );


        // 如果玩家距離太遠。
        if (distance > interactionDistance)
        {
            return false;
        }


        // --------------------------------------
        // 3. 確認背包系統存在
        // --------------------------------------

        // 如果沒有指定 InventoryManager。
        if (inventoryManager == null)
        {
            Debug.LogError(
                "ExperimentClipboardInteractable 尚未指定 InventoryManager。"
            );

            return false;
        }


        // --------------------------------------
        // 4. 避免重複取得
        // --------------------------------------

        // 如果背包裡本來就已經有實驗紀錄板。
        if (inventoryManager.HasItemByID(clipboardItemID))
        {
            return false;
        }


        // --------------------------------------
        // 5. 將紀錄板加入背包
        // --------------------------------------

        // 嘗試透過 Item ID 將紀錄板加入背包。
        bool added =
            inventoryManager.AddItemByID(
                clipboardItemID
            );


        // 如果加入失敗。
        if (!added)
        {
            return false;
        }


        // --------------------------------------
        // 6. 推進任務
        // --------------------------------------

        // 通知 MissionController：
        // 玩家已經成功取得實驗紀錄板。
        missionController.OnClipboardObtained();


        // --------------------------------------
        // 7. 隱藏場景中的紀錄板
        // --------------------------------------

        // 不使用 Destroy，
        // 方便我們開發時重新開啟物件進行測試。
        gameObject.SetActive(false);


        // 顯示測試訊息。
        Debug.Log(
            "玩家成功取得場景中的實驗紀錄板。"
        );


        // 告訴 PlayerClick：
        // 這次互動成功。
        return true;
    }
}