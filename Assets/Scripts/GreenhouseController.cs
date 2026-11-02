using TMPro; // 引入 TextMeshPro，讓程式可以修改牌子文字。
using UnityEngine; // 引入 Unity 基本功能。

public class GreenhouseController : MonoBehaviour
{
    // ==========================================
    // 主程式
    // ==========================================

    [Header("主程式")]

    public GameManager gameManager;
    // 整個教材的全域流程控制程式。


    public MissionController missionController;
    // 負責任務進度與任務劇情。


    // ==========================================
    // 豌豆牌子
    // ==========================================

    [Header("豌豆牌子")]

    [Tooltip("高莖親代旁邊的牌子")]
    public GameObject tallPeaSign;
    // 高莖親代的牌子。


    [Tooltip("矮莖親代旁邊的牌子")]
    public GameObject shortPeaSign;
    // 矮莖親代的牌子。


    // ==========================================
    // 牌子互動設定
    // ==========================================

    [Header("牌子互動設定")]

    public float interactionDistance = 2.5f;
    // 玩家距離牌子多近時才可以查看。


    // ==========================================
    // 牌子資訊介面
    // ==========================================

    [Header("牌子資訊介面")]

    public GameObject signPanel;
    // 共用的牌子資訊 Panel。


    public TMP_Text signTitleText;
    // 顯示牌子的標題。


    public TMP_Text signContentText;
    // 顯示牌子的內容。


    // ==========================================
    // 親代觀察紀錄
    // ==========================================

    [Header("親代觀察紀錄")]

    public bool tallPeaViewed = false;
    // 紀錄高莖親代是否已經查看過。


    public bool shortPeaViewed = false;
    // 紀錄矮莖親代是否已經查看過。


    // ==========================================
    // 初始化
    // ==========================================

    private void Start()
    {
        // 遊戲開始時先關閉牌子資訊 Panel。
        signPanel.SetActive(false);


        // 一開始兩株親代都還沒有看過。
        tallPeaViewed = false;
        shortPeaViewed = false;
    }


    // ==========================================
    // 嘗試查看牌子
    // ==========================================

    public bool TryOpenSign(
        GameObject clickedObject,
        Transform player)
    {
        // 計算玩家和被點擊物件之間的距離。
        float distance = Vector3.Distance(
            player.position,
            clickedObject.transform.position
        );


        // 如果距離超過允許互動的範圍。
        if (distance > interactionDistance)
        {
            // 顯示測試訊息。
            Debug.Log("距離牌子太遠。");

            // 表示這次沒有成功開啟牌子。
            return false;
        }


        // ======================================
        // 高莖親代
        // ======================================

        if (clickedObject == tallPeaSign)
        {
            // 紀錄高莖親代已經被查看。
            tallPeaViewed = true;


            // 通知 MissionController：
            // 玩家已經開始觀察親代。
            missionController.OnParentSignViewed();


            // 顯示高莖親代資料。
            ShowTallPeaSign();


            // 表示成功開啟牌子。
            return true;
        }


        // ======================================
        // 矮莖親代
        // ======================================

        if (clickedObject == shortPeaSign)
        {
            // 紀錄矮莖親代已經被查看。
            shortPeaViewed = true;


            // 通知 MissionController：
            // 玩家已經開始觀察親代。
            missionController.OnParentSignViewed();


            // 顯示矮莖親代資料。
            ShowShortPeaSign();


            // 表示成功開啟牌子。
            return true;
        }


        // 點到的不是目前可以查看的牌子。
        return false;
    }


    // ==========================================
    // 顯示高莖牌子
    // ==========================================

    private void ShowTallPeaSign()
    {
        // 設定標題。
        signTitleText.text = "親代甲";


        // 設定內容。
        signContentText.text =
            "品系：純品系\n" +
            "性狀：高莖";


        // 顯示牌子 Panel。
        signPanel.SetActive(true);
    }


    // ==========================================
    // 顯示矮莖牌子
    // ==========================================

    private void ShowShortPeaSign()
    {
        // 設定標題。
        signTitleText.text = "親代乙";


        // 設定內容。
        signContentText.text =
            "品系：純品系\n" +
            "性狀：矮莖";


        // 顯示牌子 Panel。
        signPanel.SetActive(true);
    }


    // ==========================================
    // 關閉牌子
    // ==========================================

    public void CloseSign()
    {
        // 先把牌子資訊 Panel 關掉。
        signPanel.SetActive(false);


        // 檢查兩株親代是不是都已經查看過。
        if (tallPeaViewed && shortPeaViewed)
        {
            // 通知 MissionController：
            // 玩家已經完成兩株親代的觀察。
            bool missionDialogueStarted =
                missionController.OnParentsObserved();


            // 如果 MissionController 已經開始播放老師對話。
            if (missionDialogueStarted)
            {
                // 就不要回到 Exploring。
                // 因為現在已經進入 MissionDialogue。
                return;
            }
        }


        // 如果還沒看完兩株，
        // 或這段任務對話之前已經播放過，
        // 就正常回到自由探索。
        gameManager.ReturnToExploring();
    }
}