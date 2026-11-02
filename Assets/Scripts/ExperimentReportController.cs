using TMPro; // 引入 TextMeshPro，控制報告文字。
using UnityEngine; // 引入 Unity 基本功能。
using UnityEngine.UI; // 引入 Button。

public class ExperimentReportController : MonoBehaviour
{
    // ==========================================
    // 報告介面
    // ==========================================

    [Header("假設紀錄介面")]

    public TMP_Text hypothesisText;
    // 顯示玩家先前提出的假設。


    public TMP_Text hypothesisRecordStateText;
    // 顯示「尚未記錄」或「已記錄」。


    public Button recordHypothesisButton;
    // 玩家按下後，
    // 將假設正式記錄進實驗報告。


    // ==========================================
    // 相關腳本
    // ==========================================

    [Header("相關腳本")]

    public ResearchRecordController researchRecordController;
    // 保存玩家實驗資料。


    public MissionController missionController;
    // 控制目前任務進度。


    // ==========================================
    // 每次打開報告頁面
    // ==========================================

    private void OnEnable()
    {
        // 每次 ReportContent 被開啟時，
        // 都重新讀取目前的研究紀錄。
        RefreshReport();
    }


    // ==========================================
    // 更新報告內容
    // ==========================================

    public void RefreshReport()
    {
        // 如果玩家目前還沒有提出任何假設。
        if (!researchRecordController.HasHypothesis())
        {
            // 顯示尚未提出假設。
            hypothesisText.text = "尚未提出假設";


            // 顯示尚未記錄。
            hypothesisRecordStateText.text = "尚未記錄";


            // 沒有假設，因此不能按記錄。
            recordHypothesisButton.interactable = false;


            // 不繼續往下執行。
            return;
        }


        // 顯示玩家先前真正選擇的假設。
        hypothesisText.text =
            researchRecordController.GetHypothesisDescription();


        // 如果這份假設已經正式記錄過。
        if (researchRecordController.IsHypothesisRecorded())
        {
            // 顯示已記錄。
            hypothesisRecordStateText.text = "已記錄";


            // 已經記錄完成，
            // 不允許重複按下。
            recordHypothesisButton.interactable = false;


            // 不繼續執行。
            return;
        }


        // 如果目前還沒正式記錄。
        hypothesisRecordStateText.text = "尚未記錄";


        // 只有目前任務真的走到「記錄假設」，
        // 才允許玩家按下記錄按鈕。
        recordHypothesisButton.interactable =
            missionController.CanRecordHypothesis();
    }


    // ==========================================
    // 正式記錄假設
    // ==========================================

    public void RecordHypothesis()
    {
        // 如果目前任務不是記錄假設階段。
        if (!missionController.CanRecordHypothesis())
        {
            // 不執行。
            return;
        }


        // 如果玩家沒有任何假設。
        if (!researchRecordController.HasHypothesis())
        {
            // 不執行。
            return;
        }


        // 如果已經記錄過。
        if (researchRecordController.IsHypothesisRecorded())
        {
            // 不允許重複記錄。
            return;
        }


        // 將玩家之前提出的假設正式存入研究紀錄。
        researchRecordController.RecordHypothesis();


        // 通知 MissionController：
        // 「回電腦紀錄假設」這個任務已完成。
        missionController.OnHypothesisRecorded();


        // 更新報告畫面。
        RefreshReport();
    }
}