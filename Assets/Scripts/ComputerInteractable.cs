
using TMPro;
using UnityEngine; // 引入 Unity 基本功能。

public class ComputerInteractable : MonoBehaviour
{
    [Header("互動設定")]

    public float interactionDistance = 2.5f; // 可以操作電腦的最大距離。


    [Header("中央內容區")]

    [Tooltip("實習任務按鈕所對應的內容")]
    public GameObject missionContent; // 顯示實習任務的區域。
    [Tooltip("故事按鈕所對應的內容")]
    public GameObject storyContent; // 顯示故事的區域。

    [Tooltip("實驗報告按鈕所對應的內容")]
    public GameObject reportContent; // 顯示實驗報告的區域。

    [Header("教材內容")]

    [Tooltip("整體流程控制器，用來讀取目前使用的教材版本。")]
    public GameManager gameManager;

    [Tooltip("電腦任務頁的標題文字，不是任務接受狀態文字。")]
    public TMP_Text missionTitleText;

    [Tooltip("電腦任務頁的委託或實驗說明文字。")]
    public TMP_Text missionDescriptionText;

    

    [Header("腳本")]
    public ExperimentReportController experimentReportController;
    // 控制實驗報告內容。


    // ==========================================
    // 玩家距離判斷
    // ==========================================

    public bool IsPlayerInRange(Transform player)
    {
        InteractionDebug.LogDistance(
            "使用電腦",
            player,
            transform,
            interactionDistance
        );

        // 計算玩家和電腦的距離。
        float distance = Vector3.Distance(
            player.position,
            transform.position
        );

        // 判斷距離是否符合互動條件。
        return distance <= interactionDistance;
    }


    // ==========================================
    // 開啟電腦時，更新並顯示任務頁
    // ==========================================

    public void ResetContent()
    {
        // 更新任務文字，並顯示任務頁。
        ShowMissionContent();
    }


    // ==========================================
    // 顯示任務頁
    // ==========================================

    public void ShowMissionContent()
    {
        // 每次打開都依目前版本重新整理內容。
        RefreshMissionContent();

        // 沿用原本的頁面切換功能。
        ShowContent(missionContent);
    }


    // ==========================================
    // 顯示研究故事
    // ==========================================

    public void ShowStoryContent()
    {
        // 顯示研究故事內容。
        ShowContent(storyContent);
    }


    // ==========================================
    // 顯示實驗報告
    // ==========================================

    // ==========================================
    // 顯示實驗報告頁面
    // ==========================================

    public void ShowReportContent()
    {
        // 先關閉其他電腦內容。
        HideAllContent();

        // 開啟實驗報告內容。
        reportContent.SetActive(true);

        // 如果有指定實驗報告控制器。
        if (experimentReportController != null)
        {
            // 每次玩家打開實驗報告，
            // 都強制重新讀取目前的研究資料。
            experimentReportController.RefreshReport();

            // 測試目前是否真的有執行刷新。
            Debug.Log("已重新整理實驗報告。");
        }
        else
        {
            // 如果沒有指定，就提醒我們 Inspector 漏接。
            Debug.LogError(
                "ComputerInteractable 尚未指定 ExperimentReportController。"
            );
        }
    }


    // ==========================================
    // 通用顯示內容
    // ==========================================

    private void ShowContent(GameObject targetContent)
    {
        // 先關閉所有中央內容。
        HideAllContent();

        // 確認目標內容存在。
        if (targetContent != null)
        {
            // 顯示指定的內容。
            targetContent.SetActive(true);
        }
        else
        {
            // 如果沒有指定內容，顯示警告。
            Debug.LogWarning("電腦中央內容尚未指定。");
        }
    }


    // ==========================================
    // 關閉所有中央內容
    // ==========================================

    private void HideAllContent()
    {
        // 關閉實習任務內容。
        if (missionContent != null)
        {
            missionContent.SetActive(false);
        }

        // 關閉研究故事內容。
        if (storyContent != null)
        {
            storyContent.SetActive(false);
        }

        // 關閉實驗報告內容。
        if (reportContent != null)
        {
            reportContent.SetActive(false);
        }
    }

    // ==========================================
    // 更新任務頁內容
    // ==========================================

    private void RefreshMissionContent()
    {
        // 引用不完整時，保留原本頁面切換，
        // 並提醒開發者補齊 Inspector。
        if (gameManager == null ||
            missionTitleText == null ||
            missionDescriptionText == null)
        {
            Debug.LogWarning(
                "電腦任務頁尚未指定 GameManager、標題或說明文字。",
                this
            );

            return;
        }

        if (gameManager.IsNarrativeVersion)
        {
            missionTitleText.text = "研究委託：豌豆性狀調查";

            missionDescriptionText.text =
                "【研究中心的問題】\n" +
                "中心收到純品系高莖與矮莖豌豆，" +
                "需要釐清兩種性狀在子代中的表現，並判斷顯隱性。\n\n" +

                "【你負責的工作】\n" +
                "觀察親代，留下初步假設；" +
                "透過交配與培育，追蹤第一、第二子代的結果。\n\n" +

                "【委託完成條件】\n" +
                "整理觀察證據，提出最終假設，" +
                "向研究中心提交有依據的判斷報告。\n\n" +

                "【研究參考】\n" +
                "可先查看「故事」頁中的孟德爾研究資料，" +
                "了解前人如何研究豌豆的遺傳現象。";
        }
        else
        {
            // 先準備任務頁的說明版內容。
            // 其他階段的版本分流會在後續完成。
            missionTitleText.text = "實驗主題：豌豆性狀遺傳";

            missionDescriptionText.text =
                "【實驗問題】\n" +
                "純品系高莖與矮莖豌豆交配後，" +
                "兩種性狀如何出現在子代中？如何判斷顯隱性？\n\n" +

                "【實驗內容】\n" +
                "觀察親代並提出初步假設；" +
                "進行交配與培育，比較第一、第二子代的結果。\n\n" +

                "【完成條件】\n" +
                "整理觀察證據，提出最終假設，" +
                "提交判斷及其依據。\n\n" +

                "【參考資料】\n" +
                "查看孟德爾研究資料，" +
                "了解研究問題、實驗方法與觀察結果。";
        }
    }
}
