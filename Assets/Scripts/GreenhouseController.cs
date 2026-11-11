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

    // 目前查看的子代牌子。
    // 查看原本甲、乙牌子時為 null。
    private PeaInfoSign currentOffspringSign;


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
        InteractionDebug.LogDistance(
            "查看牌子",
            player,
            clickedObject.transform,
            interactionDistance
        );

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

            gameManager.ShowSystemHint("距離牌子太遠，請再靠近一點");

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
        currentOffspringSign = null;

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
        currentOffspringSign = null;

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
        signPanel.SetActive(false);

        // ======================================
        // 剛剛查看的是子代牌子
        // ======================================

        if (currentOffspringSign != null)
        {
            // 先保存資料，再清除目前查看紀錄。
            SeedData seed = currentOffspringSign.seed;
            currentOffspringSign = null;

            // 有來源資料時，依交配類型通知對應的觀察任務。
            if (seed != null)
            {
                // 預設沒有老師對話接手。
                bool dialogueStarted = false;

                // 原本親代交配產生的植株，走 F1 觀察流程。
                if (seed.crossType == SeedData.CrossType.OriginalParents)
                {
                    dialogueStarted =
                        missionController.OnFirstOffspringViewed();
                }
                else if (seed.crossType == SeedData.CrossType.F1SelfCross)
                {
                    // F1 自交產生的植株，走 F2 觀察流程。
                    dialogueStarted =
                        missionController.OnSecondOffspringViewed();
                }

                // 有老師對話時，保持操作鎖定，交給對話結束回呼恢復。
                if (dialogueStarted)
                {
                    return;
                }
            }

            // 已播放過引導，或不是第一子代，
            // 正常回到探索。
            gameManager.ReturnToExploring();
            return;
        }

        // ======================================
        // 原本親代甲、乙的觀察流程
        // ======================================

        if (tallPeaViewed && shortPeaViewed)
        {
            bool missionDialogueStarted =
                missionController.OnParentsObserved();

            if (missionDialogueStarted)
            {
                return;
            }
        }

        gameManager.ReturnToExploring();
    }

    // ==========================================
    // 查看種植區中的子代
    // ==========================================

    public bool OpenOffspringSign(PeaInfoSign sign)
    {
        // 沒有牌子或來源種子，就不開啟資訊。
        if (sign == null || sign.seed == null)
        {
            return false;
        }
        // 保存目前查看的牌子，供關閉時判斷任務進度。
        currentOffspringSign = sign;
        // 顯示子代名稱，不加入種子編號。
        signTitleText.text = sign.title;

        // 顯示植株的性狀，以及產生這顆種子的兩個親代。
        signContentText.text =
            $"觀察性狀：{sign.observedTrait}\n\n" +
            $"親代來源：{sign.seed.firstParentName} × " +
            $"{sign.seed.secondParentName}";

        signPanel.SetActive(true);

        return true;
    }
}