
using System.Collections.Generic; // 引入 List 集合。
using UnityEngine; // 引入 Unity 基本功能。

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")] // 允許在 Project 建立物品資料庫。
public class ItemDatabase : ScriptableObject // 建立 ScriptableObject 資料庫。
{
    [Header("物品資料")] // 物品資料設定區域。

    public List<ItemData> itemList = new List<ItemData>(); // 儲存所有物品的資料。


    // ==========================================
    // 根據 ID 尋找物品
    // ==========================================
     
    public ItemData GetItemByID(int id) // 傳入物品 ID，尋找對應資料。
    {
        // 逐一檢查資料庫中的物品。
        foreach (ItemData item in itemList)
        {
            // 確認物品存在，而且 ID 符合。
            if (item != null && item.itemID == id)
            {
                // 找到物品後回傳資料。
                return item;
            }
        }

        // 如果沒有找到符合的物品，輸出警告。
        Debug.LogWarning("找不到 ID 為 " + id + " 的物品。");

        // 找不到時回傳 null。
        return null;
    }
}
