using TMPro; // 引入 TextMeshPro。
using UnityEngine; // 引入 Unity 基本功能。
using UnityEngine.UI; // 引入 Button。

public class ExperimentReportController : MonoBehaviour
{
    // ==========================================
    // 假設紀錄介面
    // ==========================================

    [Header("假設紀錄")]

    public TMP_Text hypothesisText;
    // 顯示玩家先前提出的假設。


    public TMP_Text hypothesisRecordStateText;
    // 顯示假設是否已經正式記錄。


    public Button recordHypothesisButton;
    // 正式記錄假設的按鈕。


    // ==========================================
    // 相關腳本
    // ==========================================

    [Header("相關腳本")]

    public ResearchRecordController researchRecordController;
    // 保存玩家目前的研究資料。


    public MissionController missionController;
    // 控制目前的任務進度。


    // ==========================================
    // 每次打開報告頁面
    // ==========================================

    private void OnEnable()
    {
        // 每次開啟實驗報告頁面時，
        // 都重新讀取目前最新的假設資料。
        RefreshReport();
    }


    // ==========================================
    // 更新實驗報告
    // ==========================================

    public void RefreshReport()
    {
        // ======================================
        // 尚未提出假設
        // ======================================

        if (!researchRecordController.HasHypothesis())
        {
            // 顯示尚未提出假設。
            hypothesisText.text =
                "尚未提出假設";


            // 顯示尚未正式記錄。
            hypothesisRecordStateText.text =
                "尚未記錄";


            // 沒有假設時不能按正式記錄。
            recordHypothesisButton.interactable =
                false;


            // 不需要再繼續執行後面的判斷。
            return;
        }


        // ======================================
        // 已經提出假設
        // ======================================

        // 顯示玩家真正提出的假設內容。
        hypothesisText.text =
            researchRecordController.GetHypothesisDescription();


        // ======================================
        // 假設已經正式記錄
        // ======================================

        if (researchRecordController.IsHypothesisRecorded())
        {
            // 顯示已記錄狀態。
            hypothesisRecordStateText.text =
                "已記錄";


            // 不允許玩家重複記錄。
            recordHypothesisButton.interactable =
                false;


            return;
        }


        // ======================================
        // 假設尚未正式記錄
        // ======================================

        // 顯示尚未記錄。
        hypothesisRecordStateText.text =
            "尚未記錄";


        // 只有目前任務真的處於 RecordHypothesis，
        // 玩家才可以按下正式記錄。
        recordHypothesisButton.interactable =
            missionController.CanRecordHypothesis();
    }


    // ==========================================
    // 正式記錄假設
    // ==========================================

    public void RecordHypothesis()
    {
        // 如果目前任務階段不允許記錄假設。
        if (!missionController.CanRecordHypothesis())
        {
            return;
        }


        // 如果玩家根本還沒有提出假設。
        if (!researchRecordController.HasHypothesis())
        {
            return;
        }


        // 如果假設已經正式記錄過。
        if (researchRecordController.IsHypothesisRecorded())
        {
            return;
        }


        // 將目前假設標記為正式記錄。
        researchRecordController.RecordHypothesis();


        // 通知 MissionController：
        // 假設已經正式記錄完成。
        //
        // MissionController 會把任務從
        // RecordHypothesis
        // 推進到
        // GetClipboard。
        missionController.OnHypothesisRecorded();


        // 重新整理實驗報告畫面。
        RefreshReport();
    }
}