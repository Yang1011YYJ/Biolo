using TMPro; // 引入 TextMeshPro。
using UnityEngine; // 引入 Unity 基本功能。
using UnityEngine.UI;

public class ExperimentClipboardPanelController : MonoBehaviour
{
    [Header("紀錄板介面")]

    [Tooltip("顯示玩家原本提出的假設，保留原始選擇供後續比較。")]
    public TMP_Text hypothesisText;

    [Tooltip("顯示 F1 與 F2 多株培育的教學示例資料。")]
    public TMP_Text offspringDataText;

    [Header("子代資料判讀")]

    [Tooltip("整個答題區，完成 F2 觀察後才顯示。")]
    [SerializeField] private GameObject offspringQuizRoot;

    [Tooltip("第一題選項，依序為請選擇、高莖、矮莖。")]
    [SerializeField] private TMP_Dropdown traitAnswerDropdown;

    [Tooltip("第二題選項，依序為請選擇、2 倍、3 倍、4 倍。")]
    [SerializeField] private TMP_Dropdown ratioAnswerDropdown;

    [Tooltip("第一題的確認按鈕，提交性狀答案。")]
    [SerializeField] private Button Q1ConfirmButton;

    [Tooltip("第二題的確認按鈕，提交倍數答案。")]
    [SerializeField] private Button Q2ConfirmButton;

    [Tooltip("顯示第一題「哪種性狀再次出現」的答題回饋。")]
    [SerializeField] private TMP_Text Q1FeedbackText;

    [Tooltip("顯示第二題「高莖株數是矮莖的幾倍」的答題回饋。")]
    [SerializeField] private TMP_Text Q2FeedbackText;

    // 由 InventoryManager 在建立面板時傳入場景中的研究紀錄。
    private ResearchRecordController researchRecordController;

    private void Awake()
    {
        // 面板生成時，讓兩顆按鈕各自呼叫對應的判讀方法。
        if (Q1ConfirmButton != null)
            Q1ConfirmButton.onClick.AddListener(CheckTraitAnswer);

        if (Q2ConfirmButton != null)
            Q2ConfirmButton.onClick.AddListener(CheckRatioAnswer);
    }

    private void OnDestroy()
    {
        // 面板銷毀時，解除這支腳本加入的事件。
        if (Q1ConfirmButton != null)
            Q1ConfirmButton.onClick.RemoveListener(CheckTraitAnswer);

        if (Q2ConfirmButton != null)
            Q2ConfirmButton.onClick.RemoveListener(CheckRatioAnswer);
    }

    public void Initialize(
        ResearchRecordController recordController)
    {
        // 保存研究資料引用，沿用原本的初始化方式。
        researchRecordController = recordController;

        // 立即更新面板內容。
        RefreshPanel();
    }

    public void RefreshPanel()
    {
        // 確認已取得研究資料。
        if (researchRecordController == null)
        {
            // 缺少資料時停止更新，避免讀取失敗。
            Debug.LogError(
                "ExperimentClipboardPanelController 沒有取得 ResearchRecordController。"
            );
            return;
        }

        // 多株資料開放後，才顯示下方的答題區。
        if (offspringQuizRoot != null)
        {
            offspringQuizRoot.SetActive(
                researchRecordController.OffspringDataUnlocked
            );
        }

        // 更新原本的假設欄位。
        if (hypothesisText != null)
        {
            // 尚未提出假設時，顯示提示文字。
            hypothesisText.text =
                researchRecordController.HasHypothesis()
                    ? researchRecordController.GetHypothesisDescription()
                    : "尚未提出假設";
        }

        // 確認新的資料文字已在 Prefab 中指定。
        if (offspringDataText == null)
        {
            // 顯示設定問題，原本假設欄位仍可正常使用。
            Debug.LogWarning("紀錄板尚未指定 Offspring Data Text。");
            return;
        }

        // 完成 F2 觀察以前，先不顯示後續結果。
        if (!researchRecordController.OffspringDataUnlocked)
        {
            // 提醒玩家繼續進行實驗。
            offspringDataText.text =
                "子代觀察資料\n\n完成第二子代的觀察後開放。";
            return;
        }

        // 使用固定的教學示例，與玩家自行種植的結果分開呈現。
        // 這些數字不會計入或取代玩家實際種出的植株。
        offspringDataText.text =
            "多株培育資料｜教學示例\n" +
            "以下為教材提供的比較資料，非玩家種植統計。\n\n" +
            "第一子代（F1）\n" +
            "純品系高莖 × 純品系矮莖\n" +
            "高莖：80 株　矮莖：0 株\n\n" +
            "第二子代（F2）\n" +
            "第一子代自花授粉\n" +
            "高莖：60 株　矮莖：20 株\n\n";

        // 開啟紀錄板時，恢復這一局已完成的題目。
        RefreshQuizProgress();
    }

    public void CheckTraitAnswer()
    {
        // 確認資料已開放，且第一題的元件已指定。
        if (researchRecordController == null ||
            !researchRecordController.OffspringDataUnlocked ||
            traitAnswerDropdown == null ||
            Q1FeedbackText == null)
        {
            return;
        }

        // 選項 0 是「請選擇」。
        if (traitAnswerDropdown.value == 0)
        {
            Q1FeedbackText.text = "請先選擇一種性狀。";
            return;
        }

        // 第一題的索引 2 是「矮莖」。
        if (traitAnswerDropdown.value != 2)
        {
            Q1FeedbackText.text =
                "再比對 F1 和 F2，哪一種性狀從 0 株變成有觀察到？";
            return;
        }

        // 第一題提交正確，保存進度並更新介面。
        researchRecordController.CompleteTraitAnswer();
        RefreshQuizProgress();
    }

    public void CheckRatioAnswer()
    {
        // 確認資料已開放，且第二題的元件已指定。
        if (researchRecordController == null ||
            !researchRecordController.OffspringDataUnlocked ||
            ratioAnswerDropdown == null ||
            Q2FeedbackText == null)
        {
            return;
        }

        // 選項 0 是「請選擇」。
        if (ratioAnswerDropdown.value == 0)
        {
            Q2FeedbackText.text = "請先選擇一個倍數。";
            return;
        }

        // 第二題的索引 2 是「3 倍」。
        if (ratioAnswerDropdown.value != 2)
        {
            Q2FeedbackText.text =
                "再看一次 F2 的資料，用高莖株數除以矮莖株數。";
            return;
        }

        // 第二題提交正確，保存進度並更新介面。
        researchRecordController.CompleteRatioAnswer();
        RefreshQuizProgress();
    }

    private void RefreshQuizProgress()
    {
        // 第一題完成後，恢復答案與回饋，再停用該題操作。
        if (researchRecordController.TraitAnswerCompleted)
        {
            traitAnswerDropdown.SetValueWithoutNotify(2);
            traitAnswerDropdown.interactable = false;
            Q1ConfirmButton.interactable = false;
            Q1FeedbackText.text =
                "答對了！矮莖在 F1 是 0 株，在 F2 增加到 20 株。";
        }

        // 第二題完成後，恢復答案與回饋，再停用該題操作。
        if (researchRecordController.RatioAnswerCompleted)
        {
            ratioAnswerDropdown.SetValueWithoutNotify(2);
            ratioAnswerDropdown.interactable = false;
            Q2ConfirmButton.interactable = false;
            Q2FeedbackText.text =
                "答對了！60 ÷ 20 = 3，高莖株數是矮莖的 3 倍。";
        }
    }
}