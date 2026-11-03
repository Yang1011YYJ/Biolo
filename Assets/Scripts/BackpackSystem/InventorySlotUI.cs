
using UnityEngine; // 引入 Unity 基本功能。
using UnityEngine.UI; // 引入 Image 元件。
using UnityEngine.EventSystems; // 引入 UI 點擊事件介面。

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("物品圖片")]

    public Image itemImage; // 顯示物品圖示的 Image 元件。

    private ItemData currentItem; // 儲存這個格子目前的物品資料。

    private InventoryManager inventoryManager; // 儲存背包管理器。


    // ==========================================
    // 初始化
    // ==========================================

    private void Awake()
    {
        // 如果 Inspector 沒有指定圖片元件。
        if (itemImage == null)
        {
            // 自動取得目前物件上的 Image。
            itemImage = GetComponent<Image>();
        }
    }


    // ==========================================
    // 設定格子物品
    // ==========================================

    public void SetItem(ItemData item, InventoryManager manager)
    {
        // 確認物品資料存在。
        if (item == null)
        {
            Debug.LogWarning("無法設定物品格子：ItemData 為空。");
            return;
        }

        // 確認圖片元件存在。
        if (itemImage == null)
        {
            Debug.LogError("物品格子找不到 Image 元件。");
            return;
        }

        // 儲存目前的物品資料。
        currentItem = item;

        // 儲存背包管理器。
        inventoryManager = manager;

        // 將物品資料中的圖片指定給格子。
        itemImage.sprite = currentItem.itemImage;

        // 確認物品是否有圖片。
        if (currentItem.itemImage == null)
        {
            Debug.LogWarning("物品尚未指定圖片：" + currentItem.itemName);
        }
    }


    // ==========================================
    // 點擊物品格子
    // ==========================================

    public void OnPointerClick(PointerEventData eventData)
    {
        // 如果目前沒有物品或背包管理器。
        if (currentItem == null || inventoryManager == null)
        {
            return;
        }

        // 通知 InventoryManager：
        // 玩家選取了這個物品格子。
        inventoryManager.SelectItem(this);
    }


    // ==========================================
    // 取得目前格子的物品
    // ==========================================

    public ItemData GetItem()
    {
        // 回傳目前格子的物品資料。
        return currentItem;
    }
}
