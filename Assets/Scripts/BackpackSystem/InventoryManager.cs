
using System.Collections.Generic; // 引入 List 集合。
using UnityEngine; // 引入 Unity 基本功能。
using TMPro; // 引入 TextMeshPro。
using UnityEngine.UI; // 引入 Button 元件。

public class InventoryManager : MonoBehaviour
{
    // ==========================================
    // 物品資料庫
    // ==========================================

    [Header("物品資料庫")]

    public ItemDatabase itemDatabase; // 管理所有物品資料的資料庫。


    // ==========================================
    // 背包 UI
    // ==========================================

    [Header("背包 UI")]

    public Transform gridParent; // 背包 Grid 的 Transform。

    public GameObject itemSlotPrefab; // 用來生成物品格子的 Prefab。

    // ==========================================
    // 物品詳細資訊 UI
    // ==========================================

    [Header("物品詳細資訊")]

    public TMP_Text descriptionText; // 顯示物品描述。

    public Button useButton; // 使用物品的按鈕。

    public Button deleteButton; // 丟棄物品的按鈕。
    public TMP_Text useButtonText; // 顯示使用按鈕的文字。


    private ItemData selectedItem; // 儲存目前選取的物品。

    private InventorySlotUI selectedSlot; // 儲存目前選取的物品格子。



    // ==========================================
    // 背包物品資料
    // ==========================================

    [Header("背包物品")]

    [SerializeField]
    private List<ItemData> inventoryItems = new List<ItemData>();
    // 儲存玩家目前持有的所有物品資料。


    // ==========================================
    // 物品介面
    // ==========================================

    [Header("物品介面")]

    public Transform itemPanelParent; // 物品介面生成時的父物件。

    private GameObject currentOpenedPanel; // 紀錄目前開啟的物品介面。


    [Header("腳本")]
    public ResearchRecordController researchRecordController;
    // 儲存玩家目前的研究紀錄。
    // 用來將資料傳給需要讀取研究內容的物品介面。

    private void Start()
    {
        // 遊戲開始時先清除選取狀態。
        ClearSelection();
    }


    // ==========================================
    // 透過 ID 加入物品
    // ==========================================

    public bool AddItemByID(int itemID)
    {
        // 從資料庫尋找指定 ID 的物品。
        ItemData item = itemDatabase.GetItemByID(itemID);

        // 如果找不到物品資料。
        if (item == null)
        {
            // 停止加入。
            return false;
        }

        // 將找到的物品加入背包。
        return AddItem(item);
    }


    // ==========================================
    // 加入物品
    // ==========================================

    public bool AddItem(ItemData item)
    {
        // 確認物品資料存在。
        if (item == null)
        {
            Debug.LogWarning("無法加入背包：物品資料不存在。");
            return false;
        }

        // 確認目前沒有重複加入同一個物品。
        if (inventoryItems.Contains(item))
        {
            Debug.LogWarning("背包已經有這個物品：" + item.itemName);
            return false;
        }

        // 確認資料庫存在。
        if (itemDatabase == null)
        {
            Debug.LogError("InventoryManager 尚未指定 ItemDatabase。");
            return false;
        }

        // 確認 Grid 存在。
        if (gridParent == null)
        {
            Debug.LogError("InventoryManager 尚未指定 Grid。");
            return false;
        }

        // 確認物品格子 Prefab 存在。
        if (itemSlotPrefab == null)
        {
            Debug.LogError("InventoryManager 尚未指定 Item Slot Prefab。");
            return false;
        }

        // 確認 Prefab 上有 InventorySlotUI。
        if (itemSlotPrefab.GetComponent<InventorySlotUI>() == null)
        {
            Debug.LogError("Item Slot Prefab 缺少 InventorySlotUI。");
            return false;
        }

        // 將物品資料加入背包 List。
        inventoryItems.Add(item);

        // 生成物品格子的 UI。
        CreateItemSlot(item);

        // 顯示加入成功訊息。
        Debug.Log("成功加入背包：" + item.itemName);

        // 回傳加入成功。
        return true;
    }


    // ==========================================
    // 生成物品格子
    // ==========================================


    private void CreateItemSlot(ItemData item)
    {
        // 確認目前指定的 Grid。
        Debug.Log("[背包測試] Grid 名稱：" + gridParent.name);

        // 確認 Grid 所在場景。
        Debug.Log("[背包測試] Grid 所在場景：" + gridParent.gameObject.scene.name);

        // 紀錄生成前的子物件數量。
        Debug.Log("[背包測試] 生成前 Child Count：" + gridParent.childCount);

        // 在指定 Grid 底下生成物品格子。
        GameObject newSlot = Instantiate(
            itemSlotPrefab,
            gridParent,
            false
        );

        // 確認生成的物件名稱。
        Debug.Log("[背包測試] 生成物件：" + newSlot.name);

        // 確認生成物件的父物件。
        Debug.Log("[背包測試] Parent：" + newSlot.transform.parent.name);

        // 確認生成物件是否啟用。
        Debug.Log("[背包測試] Active：" + newSlot.activeSelf);

        // 取得物品格子控制腳本。
        InventorySlotUI slotUI =
            newSlot.GetComponent<InventorySlotUI>();

        // 設定物品資料。
        slotUI.SetItem(item, this);

        // 確認生成後的子物件數量。
        Debug.Log("[背包測試] 生成後 Child Count：" + gridParent.childCount);
    }



    // ==========================================
    // 確認是否持有指定物品
    // ==========================================

    public bool HasItemByID(int itemID)
    {
        // 檢查背包 List 是否有符合 ID 的物品。
        return inventoryItems.Exists(
            item => item != null && item.itemID == itemID
        );
    }


    // ==========================================
    // 測試用：加入實驗紀錄板
    // ==========================================

    [ContextMenu("測試加入實驗紀錄板")]
    private void TestAddExperimentClipboard()
    {
        // 使用 ID 1 測試加入實驗紀錄板。
        AddItemByID(1);
    }

    [ContextMenu("測試加入消耗品")]
    private void TestAddConsumable()
    {
        AddItemByID(2);
    }

    // ==========================================
    // 選取物品
    // ==========================================

    public void SelectItem(InventorySlotUI slot)
    {
        // 確認物品格子存在。
        if (slot == null)
        {
            return;
        }

        // 取得這個格子裡的物品資料。
        ItemData item = slot.GetItem();

        // 確認物品資料存在。
        if (item == null)
        {
            return;
        }

        // 儲存目前選取的物品。
        selectedItem = item;

        // 儲存目前選取的格子。
        selectedSlot = slot;

        // 更新物品描述。
        descriptionText.text = item.itemDescription;

        // 根據物品設定決定能否使用。
        useButton.interactable = item.isUsable;
        // 根據物品資料更新使用按鈕文字。
        useButtonText.text = item.useButtonLabel;

        // 根據物品設定決定能否丟棄。
        deleteButton.interactable = item.isDeletable;

        // 顯示測試訊息。
        Debug.Log("目前選取物品：" + item.itemName);
    }


    // ==========================================
    // 清除選取狀態
    // ==========================================

    public void ClearSelection()
    {
        // 清除目前選取的物品。
        selectedItem = null;

        // 清除目前選取的格子。
        selectedSlot = null;

        // 沒有選取物品時，恢復預設按鈕文字。
        if (useButtonText != null)
        {
            useButtonText.text = "使用";
        }

        // 顯示預設提示文字。
        if (descriptionText != null)
        {
            descriptionText.text = "請選擇物品";
        }

        // 初始狀態禁止使用物品。
        if (useButton != null)
        {
            useButton.interactable = false;
        }

        // 初始狀態禁止丟棄物品。
        if (deleteButton != null)
        {
            deleteButton.interactable = false;
        }
    }


    // ==========================================
    // 使用目前選取的物品
    // ==========================================

    public void UseSelectedItem()
    {
        // 如果目前沒有選取物品。
        if (selectedItem == null)
        {
            return;
        }

        // 如果物品不允許使用。
        if (!selectedItem.isUsable)
        {
            return;
        }

        // 根據物品的使用類型執行不同功能。
        switch (selectedItem.useType)
        {
            // ==================================
            // 開啟物品介面
            // ==================================

            case ItemUseType.OpenPanel:

                // 開啟物品對應的介面。
                OpenSelectedItemPanel();

                break;


            // ==================================
            // 消耗物品
            // ==================================

            case ItemUseType.Consume:

                // 消耗目前選取的物品。
                ConsumeSelectedItem();

                break;
        }
    }



    // ==========================================
    // 開啟物品對應介面
    // ==========================================

    private void OpenSelectedItemPanel()
    {
        // 確認物品有指定介面 Prefab。
        if (selectedItem.openPanelPrefab == null)
        {
            Debug.LogWarning(
                "物品沒有指定介面：" + selectedItem.itemName
            );

            return;
        }

        // 確認介面父物件存在。
        if (itemPanelParent == null)
        {
            Debug.LogError("尚未指定 Item Panel Parent。");

            return;
        }

        // 如果已經有介面開啟，就不重複生成。
        if (currentOpenedPanel != null)
        {
            currentOpenedPanel.SetActive(true);

            // 將目前介面移到最上層。
            currentOpenedPanel.transform.SetAsLastSibling();

            return;
        }

        // 生成物品對應的介面 Prefab。
        currentOpenedPanel = Instantiate(
            selectedItem.openPanelPrefab,
            itemPanelParent,
            false
        );

        // 取得生成介面上的控制腳本。
        ItemPanelController panelController =
            currentOpenedPanel.GetComponent<ItemPanelController>();

        // 確認介面有掛上控制腳本。
        if (panelController == null)
        {
            Debug.LogError("物品介面缺少 ItemPanelController。");

            // 刪除錯誤生成的介面。
            Destroy(currentOpenedPanel);

            // 清除目前介面的紀錄。
            currentOpenedPanel = null;

            return;
        }

        // 將目前的背包管理器傳給介面。
        panelController.Initialize(this);

        // ==========================================
        // 實驗紀錄板資料初始化
        // ==========================================

        // 嘗試確認這個物品介面是不是實驗紀錄板。
        ExperimentClipboardPanelController clipboardPanelController =
            currentOpenedPanel.GetComponent<
                ExperimentClipboardPanelController
            >();


        // 如果這個介面具有實驗紀錄板控制器。
        if (clipboardPanelController != null)
        {
            // 將玩家目前的研究資料傳給紀錄板。
            clipboardPanelController.Initialize(
                researchRecordController
            );
        }
        // 初始化完成後，同一次點擊就顯示新生成的面板。
        currentOpenedPanel.SetActive(true);

        // 讓面板顯示在同一容器的其他 UI 上方。
        currentOpenedPanel.transform.SetAsLastSibling();

        // 顯示開啟成功訊息。
        Debug.Log("開啟物品介面：" + selectedItem.itemName);
    }


    // ==========================================
    // 關閉物品介面
    // ==========================================

    public void CloseOpenedPanel(GameObject panel)
    {
        // 確認傳入的介面就是目前開啟的介面。
        if (panel == null || panel != currentOpenedPanel)
        {
            return;
        }

        // 刪除目前開啟的介面。
        Destroy(currentOpenedPanel);

        // 清除目前介面的紀錄。
        currentOpenedPanel = null;

        // 顯示關閉成功訊息。
        Debug.Log("物品介面已關閉。");
    }


    // ==========================================
    // 消耗目前選取的物品
    // ==========================================

    private void ConsumeSelectedItem()
    {
        // 確認目前有選取物品。
        if (selectedItem == null || selectedSlot == null)
        {
            return;
        }

        // 暫存物品名稱。
        string consumedItemName = selectedItem.itemName;

        // 從背包 List 移除物品資料。
        bool removed = inventoryItems.Remove(selectedItem);

        // 如果資料沒有成功移除。
        if (!removed)
        {
            Debug.LogWarning("無法從背包移除物品。");

            return;
        }

        // 刪除 Grid 裡對應的物品格子。
        Destroy(selectedSlot.gameObject);

        // 清除目前選取狀態。
        ClearSelection();

        // 顯示消耗成功訊息。
        Debug.Log("已消耗物品：" + consumedItemName);
    }

    // ==========================================
    // 丟棄目前選取的物品
    // ==========================================

    public void DeleteSelectedItem()
    {
        // 如果目前沒有選取物品。
        if (selectedItem == null)
        {
            Debug.LogWarning("目前沒有選取可以丟棄的物品。");

            return;
        }

        // 如果目前沒有對應的物品格子。
        if (selectedSlot == null)
        {
            Debug.LogWarning("找不到目前選取物品的格子。");

            return;
        }

        // 如果這個物品不允許丟棄。
        if (!selectedItem.isDeletable)
        {
            Debug.LogWarning(
                "此物品不可丟棄：" + selectedItem.itemName
            );

            return;
        }

        // 暫存即將丟棄的物品名稱。
        // 因為稍後 ClearSelection() 會把 selectedItem 清空。
        string deletedItemName = selectedItem.itemName;

        // 從背包的資料 List 中移除目前選取的物品。
        bool removed =
            inventoryItems.Remove(selectedItem);

        // 如果 List 中沒有成功找到並移除物品。
        if (!removed)
        {
            Debug.LogWarning(
                "無法從背包資料中移除物品：" + deletedItemName
            );

            return;
        }

        // 刪除 Grid 中目前選取的物品格子。
        Destroy(selectedSlot.gameObject);

        // 清除目前的選取狀態。
        ClearSelection();

        // 在 Console 顯示測試訊息。
        Debug.Log(
            "已丟棄物品：" + deletedItemName
        );
    }
}
