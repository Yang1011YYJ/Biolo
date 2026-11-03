
using UnityEngine; // 引入 Unity 基本功能。

public class ItemPanelController : MonoBehaviour
{
    [Header("背包管理器")]

    private InventoryManager inventoryManager; // 儲存背包管理器。


    // ==========================================
    // 初始化
    // ==========================================

    public void Initialize(InventoryManager manager)
    {
        // 接收 InventoryManager 傳入的管理器。
        inventoryManager = manager;
    }


    // ==========================================
    // 關閉物品介面
    // ==========================================

    public void ClosePanel()
    {
        // 如果背包管理器存在。
        if (inventoryManager != null)
        {
            // 通知背包管理器關閉目前的物品介面。
            inventoryManager.CloseOpenedPanel(gameObject);
        }
        else
        {
            // 如果沒有管理器，就先關閉自己。
            gameObject.SetActive(false);
        }
    }
}
