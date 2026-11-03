using UnityEngine; // 引入 Unity 基本功能。

public class InventoryController : MonoBehaviour
{
    // ==========================================
    // 主程式
    // ==========================================

    [Header("主程式")]

    public GameManager gameManager;
    // 控制全域操作狀態。


    // ==========================================
    // 背包介面
    // ==========================================

    [Header("背包介面")]

    public GameObject backpackPanel;
    // 整個背包 Panel。


    public GameObject clipboardItem;
    // 背包中顯示「實驗紀錄板」的物件或按鈕。


    // ==========================================
    // 道具狀態
    // ==========================================

    [Header("目前持有物品")]

    [SerializeField]
    private bool hasExperimentClipboard = false;
    // 紀錄玩家目前是否已經取得實驗紀錄板。


    // ==========================================
    // 初始化
    // ==========================================

    private void Start()
    {
        // 遊戲開始時先關閉背包。
        backpackPanel.SetActive(false);


        // 尚未取得紀錄板，
        // 所以背包裡先不要顯示它。
        clipboardItem.SetActive(false);
    }


    // ==========================================
    // 加入實驗紀錄板
    // ==========================================

    public void AddExperimentClipboard()
    {
        // 如果已經有紀錄板。
        if (hasExperimentClipboard)
        {
            // 不重複加入。
            return;
        }


        // 標記玩家已經取得紀錄板。
        hasExperimentClipboard = true;


        // 在背包裡顯示紀錄板。
        clipboardItem.SetActive(true);


        // 測試訊息。
        Debug.Log("實驗紀錄板已加入背包。");
    }


    // ==========================================
    // 是否持有實驗紀錄板
    // ==========================================

    public bool HasExperimentClipboard()
    {
        // 回傳目前是否持有紀錄板。
        return hasExperimentClipboard;
    }


    // ==========================================
    // 開啟背包
    // ==========================================

    public void OpenBackpack()
    {
        // 顯示背包 Panel。
        backpackPanel.SetActive(true);


        // 告訴 GameManager：
        // 玩家目前正在操作背包。
        gameManager.EnterInventory();
    }


    // ==========================================
    // 關閉背包
    // ==========================================

    public void CloseBackpack()
    {
        // 關閉背包 Panel。
        backpackPanel.SetActive(false);


        // 回到自由探索。
        gameManager.ReturnToExploring();
    }
}