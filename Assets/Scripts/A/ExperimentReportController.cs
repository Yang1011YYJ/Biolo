using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExperimentReportController : MonoBehaviour
{
    [Header("第一頁：初步假設")]

    [Tooltip("顯示玩家最初提出的假設，不會被最終判斷覆寫。")]
    public TMP_Text hypothesisText;

    [Tooltip("顯示初步假設是否已正式記錄。")]
    public TMP_Text hypothesisRecordStateText;

    [Tooltip("正式記錄初步假設的按鈕。")]
    public Button recordHypothesisButton;


    [Header("第二頁：最終報告")]

    [Tooltip("顯示第一、第二子代的教學觀察結果。")]
    public TMP_Text evidenceText;

    [Tooltip("最終判斷選單。依序為請選擇、高莖顯性、矮莖顯性。")]
    public TMP_Dropdown judgmentDropdown;

    [Tooltip("提交最終判斷的按鈕。")]
    public Button submitReportButton;

    [Tooltip("顯示報告是否開放、是否已提交，以及接下來的操作。")]
    public TMP_Text reportStatusText;

    [Tooltip("與紀錄板一致的多株教學資料摘要。不是單株隨機生成結果。")]
    [TextArea(3, 6)]
    public string offspringEvidenceSummary =
        "第一子代（F1）：全為高莖。\n" +
        "第二子代（F2）：同時出現高莖與矮莖。\n" +
        "第二子代高莖：矮莖＝3：1。";

    [Header("提交後提醒")]
    [Tooltip("提交成功後，若玩家仍未關閉電腦，隔幾秒再次提醒。")][Min(0f)]public float closeComputerReminderDelay = 5f;
    // 保存等待提醒的協程，避免重複啟動。
    private Coroutine closeComputerReminder;

    [Header("研究中心回覆")]
    [Tooltip("回覆區標題，敘事版顯示研究中心回覆，說明版顯示判斷檢核。")]public TMP_Text replyTitleText;
    [Tooltip("顯示尚未提交、等待回覆、通過或退回。")]public TMP_Text replyResultText;
    [Tooltip("顯示本次判斷的檢核理由。")]public TMP_Text replyContentText;
    // 記住上次顯示的任務階段，階段改變時才刷新。
    private MissionController.MissionStep lastDisplayedStep;
    private bool hasDisplayedStep;

    [Header("相關腳本")]
    [Tooltip("保存原始假設、資料判讀進度及本次提交判斷。")]public ResearchRecordController researchRecordController;
    [Tooltip("判斷目前是否可以記錄假設或提交報告。")]public MissionController missionController;
    [Tooltip("整體流程控制器，用來顯示下方提示與判斷教材版本。")] public GameManager gameManager;


    private void OnEnable()
    {
        RefreshReport();
    }

    private void Update()
    {
        if (missionController == null ||
            researchRecordController == null)
        {
            return;
        }

        if (!hasDisplayedStep ||
            lastDisplayedStep != missionController.currentMissionStep)
        {
            RefreshReport();
        }
    }

    // 每次打開報告頁，更新兩頁內容。
    public void RefreshReport()
    {
        if (researchRecordController == null ||
            missionController == null)
        {
            Debug.LogError(
                "報告介面尚未指定研究紀錄或任務控制器。",
                this
            );

            if (recordHypothesisButton != null)
                recordHypothesisButton.interactable = false;

            if (submitReportButton != null)
                submitReportButton.interactable = false;

            if (judgmentDropdown != null)
                judgmentDropdown.interactable = false;

            return;
        }

        RefreshOriginalHypothesis();
        RefreshFinalReport();
        RefreshReportReply();

        // 保存這次已顯示的階段。
        lastDisplayedStep = missionController.currentMissionStep;
        hasDisplayedStep = true;
    }


    private void RefreshOriginalHypothesis()
    {
        bool hasHypothesis =
            researchRecordController.HasHypothesis();

        bool recorded =
            researchRecordController.IsHypothesisRecorded();

        if (hypothesisText != null)
        {
            hypothesisText.text =
                researchRecordController.GetHypothesisDescription();
        }

        if (hypothesisRecordStateText != null)
        {
            hypothesisRecordStateText.text =
                recorded ? "已記錄" : "尚未記錄";
        }

        if (recordHypothesisButton != null)
        {
            recordHypothesisButton.interactable =
                hasHypothesis &&
                !recorded &&
                missionController.CanRecordHypothesis();
        }
    }

    private bool CanSubmitReport()
    {
        return missionController != null &&
               missionController.CanSubmitHypothesisReport();
    }


    private void RefreshFinalReport()
    {
        // 尚未解鎖前，不提前顯示子代結果。
        if (evidenceText != null)
        {
            evidenceText.text =
                researchRecordController.OffspringDataUnlocked
                    ? offspringEvidenceSummary
                    : "完成第二子代觀察後，開放實驗資料。";
        }

        bool canSubmit = CanSubmitReport();

        if (judgmentDropdown != null)
        {
            judgmentDropdown.interactable = canSubmit;

            // 提交後重新開啟，顯示實際提交的選項。
            if (!canSubmit &&
                researchRecordController.SubmittedHypothesis !=
                    ResearchRecordController.HypothesisType.None)
            {
                int submittedIndex =
                    researchRecordController.SubmittedHypothesis ==
                    ResearchRecordController.HypothesisType.TallStemDominant
                        ? 1
                        : 2;

                judgmentDropdown.SetValueWithoutNotify(submittedIndex);
            }
        }

        RefreshSubmitButton();

        if (reportStatusText == null)
            return;

        switch (missionController.currentMissionStep)
        {
            case MissionController.MissionStep.ReviewHypothesis:

                reportStatusText.text = canSubmit
                    ? "尚未提交報告"
                    : "請先完成紀錄板的兩道資料判讀題。";

                break;

            case MissionController.MissionStep.ReportSubmitted:

                reportStatusText.text =
                    "提交成功，請關閉電腦等待回覆。";

                break;

            case MissionController.MissionStep.WaitingReportReply:

                reportStatusText.text =
                    "已提交，正在等待回覆。";

                break;

            case MissionController.MissionStep.ReportReplyReady:

                reportStatusText.text =
                    missionController.HasPassedReport
                        ? "報告通過，請到第 3 頁觀看實驗解說。"
                        : "請修正判斷，再次提交報告。";

                break;


            case MissionController.MissionStep.ReturnToUpdatedSigns:

                reportStatusText.text =
                    "請回溫室查看更新後的牌子。";

                break;


            case MissionController.MissionStep.StageACompleted:

                reportStatusText.text =
                    "本次研究已完成。";

                break;

            default:

                reportStatusText.text =
                    "完成子代觀察與資料判讀後，開放提交。";

                break;
        }
    }


    private void RefreshSubmitButton()
    {
        if (submitReportButton == null)
            return;

        submitReportButton.interactable =
            CanSubmitReport() &&
            judgmentDropdown != null &&
            (judgmentDropdown.value == 1 ||
             judgmentDropdown.value == 2);
    }


    // 綁定 Dropdown 的 On Value Changed。
    public void OnJudgmentChanged(int value)
    {
        RefreshSubmitButton();
    }


    // 綁定提交報告按鈕。
    public void SubmitReport()
    {
        if (!CanSubmitReport() || judgmentDropdown == null)
            return;

        ResearchRecordController.HypothesisType judgment;

        switch (judgmentDropdown.value)
        {
            case 1:
                judgment =
                    ResearchRecordController.HypothesisType.TallStemDominant;
                break;

            case 2:
                judgment =
                    ResearchRecordController.HypothesisType.ShortStemDominant;
                break;

            default:
                if (reportStatusText != null)
                    reportStatusText.text = "請先選擇最後的判斷。";

                return;
        }

        bool submitted =
            missionController.TrySubmitHypothesisReport(judgment);

        if (!submitted)
        {
            if (reportStatusText != null)
                reportStatusText.text = "目前無法提交，請確認任務進度。";

            return;
        }

        RefreshReport();

        if (gameManager != null)
        {
            gameManager.ShowSystemHint(
                gameManager.IsNarrativeVersion
                    ? "已提交，請關閉電腦等待回覆"
                    : "已提交，請關閉電腦等待回覆"
            );
        }

        if (closeComputerReminder != null)
        {
            StopCoroutine(closeComputerReminder);
        }

        closeComputerReminder =
            StartCoroutine(RemindToCloseComputer());
    }


    // 綁定第一頁原本的記錄假設按鈕。
    public void RecordHypothesis()
    {
        if (researchRecordController == null ||
            missionController == null ||
            !missionController.CanRecordHypothesis() ||
            !researchRecordController.HasHypothesis() ||
            researchRecordController.IsHypothesisRecorded())
        {
            return;
        }

        researchRecordController.RecordHypothesis();
        missionController.OnHypothesisRecorded();

        RefreshReport();
    }

    private void RefreshReportReply()
    {
        bool narrative =
            gameManager == null || gameManager.IsNarrativeVersion;

        if (replyTitleText != null)
        {
            replyTitleText.text =
                narrative ? "研究中心回覆" : "判斷檢核";
        }

        string result;
        string content;

        switch (missionController.currentMissionStep)
        {
            case MissionController.MissionStep.ReportSubmitted:

                result = "已提交";

                content = "請關閉電腦，等待回覆通知。";

                break;

            case MissionController.MissionStep.WaitingReportReply:

                result = "等待回覆";

                content = narrative
                    ? "研究中心已收到報告，正在檢查你的判斷。"
                    : "系統正在檢核本次提交的判斷。";

                break;

            case MissionController.MissionStep.ReportReplyReady:
            case MissionController.MissionStep.ReturnToUpdatedSigns:
            case MissionController.MissionStep.StageACompleted:

                ResearchRecordController.HypothesisType judgment =
                    researchRecordController.SubmittedHypothesis;

                if (judgment ==
                    ResearchRecordController.HypothesisType.None)
                {
                    result = "回覆資料異常";
                    content = "找不到本次提交的判斷，請檢查提交紀錄。";
                    break;
                }

                bool passed =
                    judgment ==
                    ResearchRecordController.HypothesisType.TallStemDominant;

                if (passed)
                {
                    result = narrative ? "報告通過" : "判斷正確";

                    content = "你的判斷與模擬實驗一致。\n\n";

                    switch (missionController.currentMissionStep)
                    {
                        case MissionController.MissionStep.ReturnToUpdatedSigns:
                            content += "溫室資料已更新，請返回查看牌子。";
                            break;

                        case MissionController.MissionStep.StageACompleted:
                            content += narrative
                                ? "研究中心已採用你的報告，本次委託已完成。"
                                : "本次實驗已完成。";
                            break;

                        default:
                            content += "請到第 3 頁觀看實驗解說。";
                            break;
                    }
                }
                else
                {
                    result = narrative ? "報告退回" : "需要重新判斷";

                    content =
                        "若矮莖為顯性，第一子代應呈現矮莖，" +
                        "觀察資料與預期不符。\n\n" +
                        "請重新比較子代資料，修正判斷後再次提交。";
                }

                break;

            default:

                result = "尚未提交";
                content = "提交最終判斷後，這裡會顯示回覆結果。";

                break;
        }

        if (replyResultText != null)
            replyResultText.text = result;

        if (replyContentText != null)
            replyContentText.text = content;
    }

    private IEnumerator RemindToCloseComputer()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(
                Mathf.Max(0.1f, closeComputerReminderDelay)
            );

            // 玩家已關閉電腦，或任務已離開提交階段，
            // 就結束提醒。
            if (gameManager == null ||
                missionController == null ||
                missionController.currentMissionStep !=
                    MissionController.MissionStep.ReportSubmitted ||
                gameManager.currentState !=
                    GameManager.GameState.Computer)
            {
                closeComputerReminder = null;
                yield break;
            }

            gameManager.ShowSystemHint(
                gameManager.IsNarrativeVersion
                    ? "請先關閉電腦，研究中心回覆將於不久後送達"
                    : "請先關閉電腦，檢核結果將於不久後提供"
            );
        }
    }
}