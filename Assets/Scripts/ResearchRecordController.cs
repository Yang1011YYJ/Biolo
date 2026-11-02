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
}