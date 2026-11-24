using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HypothesisReportUI : MonoBehaviour
{

    [Header("UI")]
    [Tooltip("ReportContent 底下新增的提交區。")]
    [SerializeField] private GameObject reportSubmissionArea;

    [Tooltip("顯示紀錄板中的 F1、F2 教學示例。")]
    [SerializeField] private TMP_Text evidenceText;

    [Tooltip("依序放入請選擇、高莖為顯性、矮莖為顯性。")]
    [SerializeField] private TMP_Dropdown judgmentDropdown;

    [Tooltip("提交本次顯、隱性判斷的按鈕。")]
    [SerializeField] private Button submitReportButton;

    [Tooltip("顯示選擇提示及報告提交狀態。")]
    [SerializeField] private TMP_Text reportStatusText;

    [Header("腳本")]
    [Tooltip("場景中的任務控制器，負責驗證並提交報告。")]
    [SerializeField] private MissionController missionController;

    [Tooltip("場景中的研究紀錄，用於讀取原始假設和已提交的判斷。")]
    [SerializeField] private ResearchRecordController researchRecordController;

    private void OnEnable()
    {
        // 每次切到報告頁時，依目前任務更新提交區。
        RefreshReport();
    }

    public void RefreshReport()
    {
        var step = missionController.currentMissionStep;

        // 只有到了檢視假設階段，才開放提交區。
        bool canSubmit = step == MissionController.MissionStep.ReviewHypothesis;
        bool submitted = step == MissionController.MissionStep.ReportSubmitted;
        reportSubmissionArea.SetActive(canSubmit || submitted);

        if (!canSubmit && !submitted)
        {
            return;
        }

        // 使用與紀錄板相同的教學示例。
        evidenceText.text =
            "多株培育資料｜教學示例\n" +
            "F1：高莖 80 株、矮莖 0 株\n" +
            "F2：高莖 60 株、矮莖 20 株";

        // 初次檢視時預填原始假設，提交後顯示本次提交的判斷。
        var judgment = submitted
            ? researchRecordController.SubmittedHypothesis
            : researchRecordController.CurrentHypothesis;

        switch (judgment)
        {
            case ResearchRecordController.HypothesisType.TallStemDominant:
                judgmentDropdown.SetValueWithoutNotify(1);
                break;
            case ResearchRecordController.HypothesisType.ShortStemDominant:
                judgmentDropdown.SetValueWithoutNotify(2);
                break;
            default:
                judgmentDropdown.SetValueWithoutNotify(0);
                break;
        }

        // 已提交時鎖定選項和按鈕，等玩家關閉電腦。
        judgmentDropdown.interactable = canSubmit;
        submitReportButton.interactable = canSubmit;
        reportStatusText.text = submitted
            ? "報告已提交，請關閉電腦等待研究中心回覆。"
            : "請根據兩代的觀察結果，保留或修正你的判斷。";
    }

    public void SubmitReport()
    {
        // 把下拉選項轉成研究紀錄使用的判斷。
        ResearchRecordController.HypothesisType judgment;
        switch (judgmentDropdown.value)
        {
            case 1:
                judgment = ResearchRecordController.HypothesisType.TallStemDominant;
                break;
            case 2:
                judgment = ResearchRecordController.HypothesisType.ShortStemDominant;
                break;
            default:
                reportStatusText.text = "請先選擇本次報告的判斷。";
                return;
        }

        // 提交成功後，更新報告頁的顯示與操作狀態。
        if (missionController.TrySubmitHypothesisReport(judgment))
        {
            RefreshReport();
        }
    }
}