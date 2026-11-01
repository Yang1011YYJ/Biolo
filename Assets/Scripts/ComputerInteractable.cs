using UnityEngine; // 引入 Unity 基本功能。

public class ComputerInteractable : MonoBehaviour
{
    [Header("互動設定")]

    public float interactionDistance = 2.5f; // 可以操作電腦的最大距離。

    [Header("中央內容區")]

    [Tooltip("實習任務按鈕所對應的內容")]public GameObject missionContent; // 顯示實習任務的區域。
    [Tooltip("故事按鈕所對應的內容")]public GameObject storyContent; // 顯示故事的區域。

    public void Start()
    {

    }

    // GameManager 會呼叫這個函式，
    // 詢問玩家現在能不能操作這台電腦。
    public bool IsPlayerInRange(Transform player)
    {
        // 計算玩家和電腦的距離。
        float distance = Vector3.Distance(
            player.position,
            transform.position
        );


        // 如果距離小於等於允許距離，就回傳 true。
        return distance <= interactionDistance;
    }

    // 每次開啟電腦介面時，
    // 由 GameManager 呼叫這個函式重置中央內容。
    public void ResetContent()
    {
        // 關閉所有中央內容。
        HideAllContent();
    }


    // 點擊「實習任務」按鈕時呼叫。
    public void ShowMissionContent()
    {
        // 先把所有內容關閉。
        HideAllContent();

        // 顯示實習任務內容。
        missionContent.SetActive(true);
    }


    // 點擊「研究資料」按鈕時呼叫。
    public void ShowStoryContent()
    {
        // 先把所有內容關閉。
        HideAllContent();

        // 顯示研究資料內容。
        storyContent.SetActive(true);
    }


    // 關閉所有中央內容。
    private void HideAllContent()
    {
        // 如果實習任務內容存在，就把它關閉。
        if (missionContent != null)
        {
            missionContent.SetActive(false);
        }

        // 如果研究資料內容存在，就把它關閉。
        if (storyContent != null)
        {
            storyContent.SetActive(false);
        }
    }
}