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

    [System.Serializable]
    public class SignPage
    {
        [Tooltip("這份資料對應的場景牌子。Collider 可以放在它的子物件。")]
        public GameObject signObject;

        [Tooltip("牌子面板的標題。")]
        public string title;

        [Tooltip("牌子面板的內文。")]
        [TextArea(4, 12)]
        public string content;
    }

    [Header("共用牌子資料")]

    [Tooltip("集中設定各塊牌子的標題與內文，A、B 都可使用。")]
    public SignPage[] signPages;

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
    [Tooltip("目前開啟的牌子是否顯示了更新後的研究結論，由程式設定。")][SerializeField]private bool currentSignShowsResearchResult;

    // 共用資料牌關閉後的通知。
    // 回傳 true 代表對話等其他流程已接手玩家操作。
    private System.Func<bool> referenceSignClosed;

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
        if (player == null ||
            gameManager == null ||
            !TryResolveSign(clickedObject, out GameObject signObject))
        {
            return false;
        }

        if (signPanel == null ||
            signTitleText == null ||
            signContentText == null)
        {
            Debug.LogError("共用牌子介面尚未設定完整。", this);
            return false;
        }

        InteractionDebug.LogDistance(
            "查看牌子",
            player,
            signObject.transform,
            interactionDistance
        );

        // 沿用 A 原本的三維距離判斷。
        float distance = Vector3.Distance(
            player.position,
            signObject.transform.position
        );

        if (distance > interactionDistance)
        {
            gameManager.ShowSystemHint("距離牌子太遠，請再靠近一點");
            return false;
        }

        // A 原本的兩塊牌子：保留原觀察流程。
        if (signObject == tallPeaSign)
        {
            referenceSignClosed = null;
            tallPeaViewed = true;

            missionController.OnParentSignViewed();
            ShowTallPeaSign();

            return true;
        }

        if (signObject == shortPeaSign)
        {
            referenceSignClosed = null;
            shortPeaViewed = true;

            missionController.OnParentSignViewed();
            ShowShortPeaSign();

            return true;
        }

        // 其餘登記的牌子：共用顯示機制，
        // 開放條件與關閉後行為交給流程控制器。
        SignPage page = FindSignPage(signObject);

        if (page == null ||
            !gameManager.CanOpenReferenceSign(signObject))
        {
            return false;
        }

        return OpenReferenceSign(
            page.title,
            page.content,
            () => gameManager.OnReferenceSignClosed(signObject)
        );
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

        ApplyConfiguredContent(tallPeaSign);
        AppendResearchResult();

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

        ApplyConfiguredContent(shortPeaSign);
        AppendResearchResult();

        // 顯示牌子 Panel。
        signPanel.SetActive(true);
    }


    // ==========================================
    // 關閉牌子
    // ==========================================

    public void CloseSign()
    {
        // 避免關閉事件被重複呼叫。
        if (!signPanel.activeSelf)
        {
            return;
        }

        signPanel.SetActive(false);

        // 優先結束共用資料牌，不進入 A 的觀察判斷。
        if (referenceSignClosed != null)
        {
            System.Func<bool> callback = referenceSignClosed;
            referenceSignClosed = null;

            bool flowTakenOver = callback.Invoke();

            if (!flowTakenOver)
            {
                gameManager.ReturnToExploring();
            }

            return;
        }

        bool viewedUpdatedResult = currentSignShowsResearchResult;
        currentSignShowsResearchResult = false;

        if (viewedUpdatedResult &&
            missionController.TryCompleteStageAFromSign())
        {
            currentOffspringSign = null;
            return;
        }

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

        AppendResearchResult();

        signPanel.SetActive(true);

        return true;
    }

    private void AppendResearchResult()
    {
        currentSignShowsResearchResult =
            missionController != null &&
            missionController.AreSignResultsUnlocked;

        if (!currentSignShowsResearchResult)
        {
            return;
        }

        signContentText.text +=
            "\n\n【本次研究結論】\n" +
            "高莖為顯性，矮莖為隱性。";
    }

    public bool OpenReferenceSign(
    string title,
    string content,
    System.Func<bool> closedCallback)
    {
        if (signPanel == null ||
            signTitleText == null ||
            signContentText == null)
        {
            Debug.LogError("共用牌子介面尚未設定完整。", this);
            return false;
        }

        // 本次不是 A 的親代或子代觀察。
        currentOffspringSign = null;
        currentSignShowsResearchResult = false;

        referenceSignClosed = closedCallback;

        signTitleText.text = title;
        signContentText.text = content;

        signPanel.SetActive(true);
        return true;
    }

    private SignPage FindSignPage(GameObject signObject)
    {
        if (signObject == null || signPages == null)
        {
            return null;
        }

        foreach (SignPage page in signPages)
        {
            if (page != null && page.signObject == signObject)
            {
                return page;
            }
        }

        return null;
    }


    // 將 Collider 所在的子物件，轉成真正登記的牌子物件。
    public bool TryResolveSign(
        GameObject clickedObject,
        out GameObject signObject)
    {
        signObject = null;

        if (clickedObject == null)
        {
            return false;
        }

        Transform current = clickedObject.transform;

        while (current != null)
        {
            GameObject candidate = current.gameObject;

            if (candidate == tallPeaSign ||
                candidate == shortPeaSign ||
                FindSignPage(candidate) != null)
            {
                signObject = candidate;
                return true;
            }

            current = current.parent;
        }

        return false;
    }


    // A 原有牌子也能改用 Inspector 裡的內容。
    private void ApplyConfiguredContent(GameObject signObject)
    {
        SignPage page = FindSignPage(signObject);

        if (page == null)
        {
            return;
        }

        signTitleText.text = page.title;
        signContentText.text = page.content;
    }
}