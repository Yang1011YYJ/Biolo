
using UnityEngine; // 引入 Unity 基本功能。

// 定義物品的使用方式。
public enum ItemUseType
{
    OpenPanel, // 開啟物品對應的介面，物品保留。
    Consume    // 消耗物品，使用後從背包移除。
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("基本資料")]

    public int itemID; // 物品的唯一編號。

    public string itemName; // 物品名稱。

    [TextArea(2, 5)]
    public string itemDescription; // 物品詳細說明。


    [Header("物品圖片")]

    public Sprite itemImage; // 背包格子顯示的圖片。


    [Header("使用設定")]

    public bool isUsable = true; // 是否允許使用。

    public ItemUseType useType; // 物品的使用類型。

    public string useButtonLabel = "使用"; // 使用按鈕顯示的文字。

    [Tooltip("只有 OpenPanel 類型的物品需要指定。")]
    public GameObject openPanelPrefab; // 使用物品時要開啟的介面 Prefab。


    [Header("丟棄設定")]

    public bool isDeletable = false; // 是否允許丟棄。
}
