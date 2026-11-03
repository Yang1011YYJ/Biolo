using TMPro; // 引入 TextMeshPro。
using UnityEngine; // 引入 Unity 基本功能。

public class ExperimentClipboardPanelController : MonoBehaviour
{
    // ==========================================
    // 紀錄板介面
    // ==========================================

    [Header("紀錄板介面")]

    public TMP_Text hypothesisText;
    // 顯示玩家目前正式記錄的實驗假設。


    // ==========================================
    // 研究資料
    // ==========================================

    private ResearchRecordController researchRecordController;
    // 儲存玩家研究資料的控制器。
    // 因為 ClipboardPanel 是 Prefab 動態生成的，
    // 所以這個引用會由 InventoryManager 在生成時傳進來。


    // ==========================================
    // 初始化紀錄板
    // ==========================================

    public void Initialize(
        ResearchRecordController recordController)
    {
        // 儲存 InventoryManager 傳入的研究資料控制器。
        researchRecordController = recordController;


        // 初始化完成後，
        // 立即更新紀錄板上的內容。
        RefreshPanel();
    }


    // ==========================================
    // 更新紀錄板內容
    // ==========================================

    public void RefreshPanel()
    {
        // 如果沒有取得 ResearchRecordController。
        if (researchRecordController == null)
        {
            // 顯示錯誤訊息。
            Debug.LogError(
                "ExperimentClipboardPanelController 沒有取得 ResearchRecordController。"
            );

            return;
        }


        // 如果玩家目前還沒有提出任何假設。
        if (!researchRecordController.HasHypothesis())
        {
            // 顯示尚未提出假設。
            hypothesisText.text =
                "尚未提出假設";

            return;
        }


        // 顯示玩家真正提出的假設。
        hypothesisText.text =
            researchRecordController.GetHypothesisDescription();
    }
}