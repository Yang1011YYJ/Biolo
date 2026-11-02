
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


    // ==========================================
    // 玩家距離判斷
    // ==========================================

    public bool IsPlayerInRange(Transform player)
    {
        // 計算玩家和電腦的距離。
        float distance = Vector3.Distance(
            player.position,
            transform.position
        );

        // 判斷距離是否符合互動條件。
        return distance <= interactionDistance;
    }


    // ==========================================
    // 重置中央內容
    // ==========================================

    public void ResetContent()
    {
        // 關閉所有中央內容。
        HideAllContent();
    }


    // ==========================================
    // 顯示實習任務
    // ==========================================

    public void ShowMissionContent()
    {
        // 顯示實習任務內容。
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

    public void ShowReportContent()
    {
        // 顯示實驗報告內容。
        ShowContent(reportContent);
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
}
