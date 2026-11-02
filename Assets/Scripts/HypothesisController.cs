using TMPro; // 引入 TextMeshPro。
using UnityEngine; // 引入 Unity 基本功能。
using UnityEngine.UI; // 引入 Button。

public class HypothesisController : MonoBehaviour
{
    // ==========================================
    // 假設介面
    // ==========================================

    [Header("假設介面")]

    public GameObject hypothesisPanel;
    // 假設選擇介面。

    public Button tallStemButton;
    // 高莖為顯性的選項。

    public Button shortStemButton;
    // 矮莖為顯性的選項。

    public Button confirmButton;
    // 確認假設的按鈕。

    public TMP_Text selectedHypothesisText;
    // 顯示目前選擇的假設。


    // ==========================================
    // 相關腳本
    // ==========================================

    [Header("相關腳本")]

    public MissionController missionController;
    // 任務流程控制。


    // ==========================================
    // 玩家目前選擇
    // ==========================================

    private ResearchRecordController.HypothesisType selectedHypothesis;
    // 暫存玩家目前選擇的假設。


    // ==========================================
    // 初始化
    // ==========================================

    private void Awake()
    {
        // 遊戲開始時先關閉假設介面。
        hypothesisPanel.SetActive(false);

        // 將高莖按鈕綁定選擇函式。
        tallStemButton.onClick.AddListener(SelectTallStem);

        // 將矮莖按鈕綁定選擇函式。
        shortStemButton.onClick.AddListener(SelectShortStem);

        // 將確認按鈕綁定確認函式。
        confirmButton.onClick.AddListener(ConfirmSelection);
    }


    // ==========================================
    // 開啟假設介面
    // ==========================================

    public void OpenPanel()
    {
        // 清除之前的選擇。
        selectedHypothesis = ResearchRecordController.HypothesisType.None;

        // 顯示尚未選擇的提示。
        selectedHypothesisText.text = "尚未選擇假設";

        // 尚未選擇時不能確認。
        confirmButton.interactable = false;

        // 顯示假設介面。
        hypothesisPanel.SetActive(true);
    }


    // ==========================================
    // 選擇高莖
    // ==========================================

    private void SelectTallStem()
    {
        // 紀錄玩家選擇高莖為顯性。
        selectedHypothesis = ResearchRecordController.HypothesisType.TallStemDominant;

        // 更新目前選擇的文字。
        selectedHypothesisText.text = "目前假設：高莖可能為顯性";

        // 開放確認按鈕。
        confirmButton.interactable = true;
    }


    // ==========================================
    // 選擇矮莖
    // ==========================================

    private void SelectShortStem()
    {
        // 紀錄玩家選擇矮莖為顯性。
        selectedHypothesis = ResearchRecordController.HypothesisType.ShortStemDominant;

        // 更新目前選擇的文字。
        selectedHypothesisText.text = "目前假設：矮莖可能為顯性";

        // 開放確認按鈕。
        confirmButton.interactable = true;
    }


    // ==========================================
    // 確認假設
    // ==========================================

    private void ConfirmSelection()
    {
        // 如果玩家沒有選擇任何假設，就不執行。
        if (selectedHypothesis == ResearchRecordController.HypothesisType.None)
        {
            return;
        }

        // 關閉假設介面。
        hypothesisPanel.SetActive(false);

        // 將玩家選擇的假設交給 MissionController。
        missionController.OnHypothesisConfirmed(selectedHypothesis);
    }
}