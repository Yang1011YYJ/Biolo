using UnityEngine; // 引入 Unity 基本功能。

public class ResearchRecordController : MonoBehaviour
{
    // ==========================================
    // 假設類型
    // ==========================================

    public enum HypothesisType
    {
        None,               // 尚未提出假設。
        TallStemDominant,   // 假設高莖為顯性。
        ShortStemDominant   // 假設矮莖為顯性。
    }


    [Header("玩家研究紀錄")]

    [SerializeField]
    private HypothesisType currentHypothesis = HypothesisType.None;
    // 儲存玩家目前提出的假設。
    [Tooltip("假設紀錄狀態")][SerializeField]private bool hypothesisRecorded = false;
    // 紀錄玩家是否已經回到電腦，
    // 將先前提出的假設正式寫入實驗報告。

    [Header("子代觀察進度")]

    [Tooltip("玩家是否已查看完第二子代牌子，開放紀錄板中的多株教學資料。")]
    [SerializeField]
    private bool offspringDataUnlocked = false;

    // 提供紀錄板查詢資料是否已開放。
    public bool OffspringDataUnlocked
    {
        get { return offspringDataUnlocked; }
    }

    // 玩家完成 F2 觀察後，開放多株資料。
    public void UnlockOffspringData()
    {
        offspringDataUnlocked = true;
    }

    [Header("子代資料判讀")]

    [Tooltip("玩家是否已提交正確的性狀答案。")]
    [SerializeField] private bool traitAnswerCompleted;

    [Tooltip("玩家是否已提交正確的倍數答案。")]
    [SerializeField] private bool ratioAnswerCompleted;

    // 提供紀錄板讀取兩題各自的完成狀態。
    public bool TraitAnswerCompleted => traitAnswerCompleted;
    public bool RatioAnswerCompleted => ratioAnswerCompleted;

    // 兩題都提交正確才算完成判讀。
    public bool OffspringQuizCompleted =>
        traitAnswerCompleted && ratioAnswerCompleted;

    [Tooltip("玩家最近一次提交給研究中心的判斷，與原始假設分開保存。")]
    [SerializeField]
    private HypothesisType submittedHypothesis = HypothesisType.None;

    // 提供研究中心回覆流程讀取本次提交的判斷。
    public HypothesisType SubmittedHypothesis => submittedHypothesis;

    public void SaveSubmittedHypothesis(HypothesisType judgment)
    {
        // 保存本次提交的判斷。
        submittedHypothesis = judgment;
    }

    // ==========================================
    // 測試目前儲存的假設
    // ==========================================

    [ContextMenu("測試：查看目前假設")]
    private void TestCurrentHypothesis()
    {
        // 顯示目前儲存的假設。
        Debug.Log(
            "目前儲存的假設：" + currentHypothesis
        );

        // 顯示目前是否已經正式記錄假設。
        Debug.Log(
            "是否已正式記錄：" + hypothesisRecorded
        );
    }
    // ==========================================
    // 取得目前假設
    // ==========================================

    public HypothesisType CurrentHypothesis
    {
        get { return currentHypothesis; }
    }


    // ==========================================
    // 確認是否已有假設
    // ==========================================

    public bool HasHypothesis()
    {
        return currentHypothesis != HypothesisType.None;
    }


    // ==========================================
    // 儲存玩家假設
    // ==========================================

    public void SaveHypothesis(HypothesisType hypothesis)
    {
        // 沒有選擇假設就不儲存。
        if (hypothesis == HypothesisType.None)
        {
            return;
        }

        // 儲存玩家選擇的假設。
        currentHypothesis = hypothesis;

        // 顯示測試訊息。
        Debug.Log("玩家提出假設：" + GetHypothesisDescription());
    }


    // ==========================================
    // 取得假設文字
    // ==========================================

    public string GetHypothesisDescription()
    {
        // 根據玩家的選擇回傳對應文字。
        switch (currentHypothesis)
        {
            case HypothesisType.TallStemDominant:
                return "高莖可能為顯性";

            case HypothesisType.ShortStemDominant:
                return "矮莖可能為顯性";

            default:
                return "尚未提出假設";
        }
    }

    // ==========================================
    // 確認假設是否已正式記錄
    // ==========================================

    public bool IsHypothesisRecorded()
    {
        // 回傳目前是否已經完成正式紀錄。
        return hypothesisRecorded;
    }


    // ==========================================
    // 正式記錄假設
    // ==========================================

    public void RecordHypothesis()
    {
        // 如果玩家根本還沒有提出假設。
        if (!HasHypothesis())
        {
            // 就不能進行正式紀錄。
            return;
        }


        // 標記這份假設已經正式寫入研究紀錄。
        hypothesisRecorded = true;


        // 測試用訊息。
        Debug.Log(
            "假設已正式記錄：" +
            GetHypothesisDescription()
        );
    }

    public void CompleteTraitAnswer()
    {
        // 保存第一題的完成狀態。
        traitAnswerCompleted = true;
    }

    public void CompleteRatioAnswer()
    {
        // 保存第二題的完成狀態。
        ratioAnswerCompleted = true;
    }
}