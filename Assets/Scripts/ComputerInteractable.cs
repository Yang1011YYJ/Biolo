
using TMPro;
using UnityEngine;
using UnityEngine.UI; // 引入 Unity 基本功能。

public class ComputerInteractable : MonoBehaviour
{
    public enum ComputerSection
    {
        Story,
        Mission,
        Report,
        Book
    }

    [Header("電腦分頁")]
    [Tooltip("故事頁面，依閱讀順序指定。")] public GameObject[] storyPages;
    [Tooltip("任務頁面，依閱讀順序指定。")] public GameObject[] missionPages;
    [Tooltip("報告頁面，依閱讀順序指定。")] public GameObject[] reportPages;
    [Tooltip("遺傳手冊頁面，依閱讀順序指定。")] public GameObject[] BookPages;
    [Tooltip("所有側欄共用的上一頁按鈕。")] public Button previousPageButton;
    [Tooltip("所有側欄共用的下一頁按鈕。")] public Button nextPageButton;
    [Tooltip("所有側欄共用的頁碼文字。")] public TMP_Text pageNumberText;
    [Tooltip("影片控制器，只負責播放與觀看紀錄。")] public StoryVideoController storyVideoController;
    [Tooltip("故事分類中，影片頁的索引。第一頁填 0。")][Min(0)] public int storyVideoPageIndex = 0;
    [Tooltip("目前選取的側欄分類，由程式更新。")][SerializeField] private ComputerSection currentSection = ComputerSection.Mission;
    [Tooltip("目前分類的頁面索引，從 0 開始，由程式更新。")][SerializeField] private int currentPageIndex;

    [Header("互動設定")]
    public float interactionDistance = 2.5f; // 可以操作電腦的最大距離。

    [Header("中央內容區")]
    [Tooltip("實習任務按鈕所對應的內容")] public GameObject missionContent; // 顯示實習任務的區域。
    [Tooltip("故事按鈕所對應的內容")] public GameObject storyContent; // 顯示故事的區域。
    [Tooltip("實驗報告按鈕所對應的內容")] public GameObject reportContent; // 顯示實驗報告的區域。
    [Tooltip("遺傳手冊按鈕所對應的內容")] public GameObject BookContent; // 顯示遺傳手冊的區域。

    [Header("任務分頁")]
    [Tooltip("任務第二頁的研究問題與方法文字。")] public TMP_Text researchInfoText;
    [Tooltip("任務控制器，用來讀取目前任務階段。")] public MissionController missionController;

    [Header("報告第三頁影片")]
    [Tooltip("實驗解說影片在報告分類中的頁面索引。第三頁填 2。")][Min(0)] public int reportVideoPageIndex = 2;

    [Header("教材內容")]
    [Tooltip("電腦任務頁的標題文字，不是任務接受狀態文字。")] public TMP_Text missionTitleText;
    [Tooltip("電腦任務頁的委託或實驗說明文字。")] public TMP_Text missionDescriptionText;

    [Header("研究背景頁")]
    [Tooltip("研究背景頁的標題文字，須放在 storyContent 底下。")] public TMP_Text storyTitleText;
    [Tooltip("研究背景頁的內文，建議放在 Scroll View 的 Content 底下。")] public TMP_Text storyDescriptionText;

    [Header("遺傳手冊頁面")]
    [Tooltip("標題")] public TMP_Text BookTitle;
    [Tooltip("Scroll View 內的 TMP 文字")] public TMP_Text ScrollView_TMP;
    [Tooltip("查閱完成按鈕")] public Button FinishButton;


    [Header("腳本")]
    public ExperimentReportController experimentReportController;
    [Tooltip("報告實驗解說影片專用的控制器，與故事影片分開指定。")] public StoryVideoController reportVideoController;
    [Tooltip("整體流程控制器，用來讀取目前使用的教材版本。")] public GameManager gameManager;


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
    // 開啟電腦：預設顯示任務第一頁
    // ==========================================

    public void ResetContent()
    {
        ShowMissionContent();
    }


    // ==========================================
    // 側欄
    // ==========================================

    public void ShowMissionContent()
    {
        if (gameManager != null && gameManager.IsStageB)
        {
            StageBMissionController stageB = gameManager.stageBMissionController;
            stageB.GetMissionContent(out string title, out string description,
                out string researchInfo);
            if (missionTitleText != null) missionTitleText.text = title;
            if (missionDescriptionText != null) missionDescriptionText.text = description;
            if (researchInfoText != null) researchInfoText.text = researchInfo;
            OpenSection(ComputerSection.Mission);
            RefreshMissionAcceptanceUI();
            return;
        }

        RefreshMissionContent();

        if (missionController != null && researchInfoText != null)
        {
            RefreshResearchInfo();
        }

        OpenSection(ComputerSection.Mission);
        RefreshMissionAcceptanceUI();
    }



    // ==========================================
    // 開啟兩版共用的故事頁
    // ==========================================

    public void ShowStoryContent()
    {
        OpenSection(ComputerSection.Story);
    }

    // ==========================================
    // 開啟遺傳手冊頁面
    // ==========================================
    public void ShowBookContent()
    {
        if (gameManager == null)
        {
            return;
        }

        if (!gameManager.IsStageB ||
            gameManager.stageBMissionController == null ||
            !gameManager.stageBMissionController.MissionAccepted)
        {
            gameManager.ShowSystemHint("請先接受本次遺傳預測任務");
            return;
        }

        OpenSection(ComputerSection.Book);
    }

    // ==========================================
    // 顯示實驗報告頁面
    // ==========================================

    public void ShowReportContent()
    {
        OpenSection(ComputerSection.Report);

        // 保留原本的報告資料更新。
        if (experimentReportController != null)
        {
            experimentReportController.RefreshReport();
        }
    }

    // ==========================================
    // 切換分類
    // ==========================================

    private void OpenSection(ComputerSection section)
    {
        // 切換分類時暫停影片，保留進度。
        if (storyVideoController != null)
            storyVideoController.SetPageVisible(false);

        if (reportVideoController != null)
            reportVideoController.SetPageVisible(false);

        HideAllContent();

        currentSection = section;
        currentPageIndex = 0;

        GameObject targetContent = null;

        switch (section)
        {
            case ComputerSection.Story:
                targetContent = storyContent;
                break;

            case ComputerSection.Mission:
                targetContent = missionContent;
                break;

            case ComputerSection.Report:
                targetContent = reportContent;
                break;

            case ComputerSection.Book:
                targetContent = BookContent;
                break;
        }

        if (targetContent == null)
        {
            Debug.LogError(
                $"電腦分類「{section}」尚未指定內容根物件。",
                this
            );

            RefreshNavigation();
            return;
        }

        targetContent.SetActive(true);
        ApplyCurrentPage();
    }

    // ==========================================
    // 取得目前分類的頁面清單
    // ==========================================

    private GameObject[] GetCurrentPages()
    {
        switch (currentSection)
        {
            case ComputerSection.Story:
                return storyPages;

            case ComputerSection.Mission:
                return missionPages;

            case ComputerSection.Report:
                return reportPages;

            case ComputerSection.Book:
                return BookPages;

            default:
                return null;
        }
    }

    // ==========================================
    // 共用上一頁／下一頁
    // ==========================================

    public void PreviousPage()
    {
        if (currentPageIndex <= 0)
        {
            return;
        }

        currentPageIndex--;
        ApplyCurrentPage();
    }


    public void NextPage()
    {
        GameObject[] pages = GetCurrentPages();

        if (pages == null ||
            currentPageIndex >= pages.Length - 1 ||
            IsNextPageLocked())
        {
            return;
        }

        currentPageIndex++;
        ApplyCurrentPage();
    }


    // ==========================================
    // 顯示目前頁面
    // ==========================================

    private void ApplyCurrentPage()
    {
        GameObject[] pages = GetCurrentPages();

        if (pages == null || pages.Length == 0)
        {
            if (storyVideoController != null)
            {
                storyVideoController.SetPageVisible(false);
            }

            RefreshNavigation();
            return;
        }

        currentPageIndex = Mathf.Clamp(
            currentPageIndex,
            0,
            pages.Length - 1
        );

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == currentPageIndex);
            }
        }

        // 明確告訴影片控制器：影片頁現在是否可見。
        if (storyVideoController != null)
        {
            bool videoVisible =
                currentSection == ComputerSection.Story &&
                currentPageIndex == storyVideoPageIndex &&
                pages[currentPageIndex] != null &&
                pages[currentPageIndex].activeInHierarchy;

            storyVideoController.SetPageVisible(videoVisible);
        }


        if (currentSection == ComputerSection.Mission &&
            gameManager != null && gameManager.IsStageB &&
            pages[currentPageIndex] != null && pages[currentPageIndex].activeInHierarchy)
        {
            gameManager.stageBMissionController.NotifyMissionPageViewed(currentPageIndex);
        }

        // 確認手冊最後一頁真的顯示在畫面上。
        if (currentSection == ComputerSection.Book &&
            gameManager != null &&
            gameManager.IsStageB &&
            gameManager.stageBMissionController != null &&
            currentPageIndex == pages.Length - 1 &&
            pages[currentPageIndex] != null &&
            pages[currentPageIndex].activeInHierarchy)
        {
            gameManager.stageBMissionController.NotifyManualPageViewed();
        }

        RefreshMissionAcceptanceUI();
        RefreshReportVideoVisibility();
        RefreshNavigation();
    }


    // ==========================================
    // 首次影片限制
    // ==========================================

    private bool IsNextPageLocked()
    {
        // 故事影片：沿用首次完整觀看限制。
        if (currentSection == ComputerSection.Story &&
            currentPageIndex == storyVideoPageIndex)
        {
            return storyVideoController == null ||
                   !storyVideoController.HasCompletedFirstViewing;
        }

        // 報告第二頁往第三頁：必須先收到通過回覆。
        if (currentSection == ComputerSection.Report &&
            currentPageIndex == reportVideoPageIndex - 1)
        {
            return missionController == null ||
                   !missionController.HasPassedReport;
        }

        return false;
    }


    // ==========================================
    // 更新頁碼與按鈕
    // ==========================================

    private void RefreshNavigation()
    {
        GameObject[] pages = GetCurrentPages();
        int count = pages != null ? pages.Length : 0;

        if (previousPageButton != null)
        {
            previousPageButton.interactable =
                count > 0 && currentPageIndex > 0;
        }

        if (nextPageButton != null)
        {
            nextPageButton.interactable =
                count > 0 &&
                currentPageIndex < count - 1 &&
                !IsNextPageLocked();
        }

        if (pageNumberText != null)
        {
            pageNumberText.text = count > 0
                ? $"{currentPageIndex + 1} / {count}"
                : "0 / 0";
        }
        RefreshBookUI();
    }


    // ==========================================
    // 影片看完後，即時更新下一頁狀態
    // ==========================================

    private void Update()
    {
        if (gameManager != null &&
            gameManager.currentState == GameManager.GameState.Computer)
        {
            RefreshNavigation();
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

    // ==========================================
    // 更新任務第二頁的研究資訊
    // ==========================================

    private void RefreshResearchInfo()
    {
        switch (missionController.currentMissionStep)
        {
            case MissionController.MissionStep.ReadMission:
            case MissionController.MissionStep.GoToGreenhouse:
            case MissionController.MissionStep.ObserveParents:
            case MissionController.MissionStep.MakeHypothesis:
            case MissionController.MissionStep.RecordHypothesis:
            case MissionController.MissionStep.GetClipboard:

                researchInfoText.text =
                    "【研究問題】\n" +
                    "純品系高莖與矮莖豌豆交配後，" +
                    "子代會呈現哪些性狀？\n\n" +

                    "【實驗方法】\n" +
                    "先觀察親代甲、乙的品系與莖高，" +
                    "提出並記錄初步假設，" +
                    "再透過交配與培育觀察子代。\n\n" +

                    "【需要記錄的資料】\n" +
                    "親代的品系、莖高，以及實驗前的假設。\n\n" +

                    "目前的假設需要後續子代的觀察結果來檢驗。";

                break;

            case MissionController.MissionStep.ReadyForExperiment:
            case MissionController.MissionStep.ObserveFirstOffspring:

                researchInfoText.text =
                    "【研究問題】\n" +
                    "純品系高莖與矮莖親代交配後，" +
                    "第一子代會呈現什麼莖高？\n\n" +

                    "【實驗方法】\n" +
                    "讓親代甲與親代乙交配，" +
                    "播種取得的種子，查看第一子代資訊牌。\n\n" +

                    "【需要記錄的資料】\n" +
                    "親代來源與第一子代的莖高。\n\n" +

                    "【比較重點】\n" +
                    "比較觀察結果與原先假設，" +
                    "整理已有的證據與尚待解答的問題。";

                break;

            case MissionController.MissionStep.SelfCrossF1:
            case MissionController.MissionStep.SowF2Seed:
            case MissionController.MissionStep.ObserveSecondOffspring:

                researchInfoText.text = gameManager.IsNarrativeVersion
                    ? "【委託中的待解問題】\n" +
                      "你培育的第一子代呈現高莖，但它的親代之一是矮莖。" +
                      "矮莖沒有出現在這株第一子代，是否代表相關遺傳訊息已經消失？\n\n" +

                      "【接續調查】\n" +
                      "讓同一株第一子代自花授粉，培育第二子代，" +
                      "追查矮莖是否會再次出現。\n\n" +

                      "【報告需要的證據】\n" +
                      "記錄第二子代的性狀與親代來源，" +
                      "再與第一子代及最初假設比較。"

                    : "【研究問題】\n" +
                      "第一子代呈現高莖時，是否仍可能保有與矮莖相關的遺傳訊息？" +
                      "第二子代是否會出現矮莖？\n\n" +

                      "【實驗方法】\n" +
                      "讓同一株第一子代自花授粉，取得種子並培育第二子代。\n\n" +

                      "【觀察與比較】\n" +
                      "記錄第二子代的性狀與親代來源，" +
                      "再與第一子代及最初假設比較。";

                break;

            case MissionController.MissionStep.ReviewOffspringData:

                researchInfoText.text = gameManager.IsNarrativeVersion
                    ? "【目前的調查進度】\n" +
                      "你已完成第一、第二子代的培育與單株觀察。" +
                      "接下來需要比較多株資料，回答矮莖是否再次出現。\n\n" +

                      "【待整理的證據】\n" +
                      "比較兩代出現的性狀，以及高莖、矮莖的株數關係。\n\n" +

                      "【接續工作】\n" +
                      "從背包開啟實驗紀錄板，完成兩道資料判讀題。"

                    : "【資料比較】\n" +
                      "完成單株觀察後，使用多株培育資料比較第一、第二子代。\n\n" +

                      "【判讀項目】\n" +
                      "比較兩代出現的性狀，以及高莖、矮莖的株數關係。\n\n" +

                      "【完成條件】\n" +
                      "從背包開啟實驗紀錄板，完成兩道資料判讀題。";

                break;

            default:
                // 後續階段的內容隨教材製作補上。
                researchInfoText.text =
                    "本階段研究資訊尚在製作中，" +
                    "請依目前任務提示進行。";

                break;
        }
    }

    private void RefreshReportVideoVisibility()
    {
        if (reportVideoController == null)
        {
            return;
        }

        GameObject[] pages = GetCurrentPages();

        bool visible =
            currentSection == ComputerSection.Report &&
            currentPageIndex == reportVideoPageIndex &&
            missionController != null &&
            missionController.HasPassedReport &&
            pages != null &&
            currentPageIndex >= 0 &&
            currentPageIndex < pages.Length &&
            pages[currentPageIndex] != null &&
            pages[currentPageIndex].activeInHierarchy;

        reportVideoController.SetPageVisible(visible);
    }

    [Header("共用任務接受介面")]
    [Tooltip("任務第二頁既有的接受按鈕，A/B 共用。On Click 改接 AcceptCurrentMission。")]
    public Button missionAcceptButton;
    [Tooltip("任務第二頁既有的接受狀態文字，A/B 共用。")]
    public TMP_Text missionAcceptanceText;

    public void AcceptCurrentMission()
    {
        if (currentSection != ComputerSection.Mission || currentPageIndex != 1)
            return;
        if (gameManager != null && gameManager.IsStageB)
            gameManager.stageBMissionController.AcceptMission();
        else if (missionController != null)
            missionController.AcceptMission();
        RefreshMissionAcceptanceUI();
    }

    private void RefreshMissionAcceptanceUI()
    {
        bool isB = gameManager != null && gameManager.IsStageB;
        bool accepted = isB
            ? gameManager.stageBMissionController.MissionAccepted
            : missionController != null && missionController.missionAccepted;
        bool canAccept = isB
            ? gameManager.stageBMissionController.CanAcceptMission
            : missionController != null && !accepted;

        if (missionAcceptButton != null)
            missionAcceptButton.interactable = canAccept &&
                currentSection == ComputerSection.Mission && currentPageIndex == 1;
        if (missionAcceptanceText != null)
        {
            missionAcceptanceText.text = accepted ? "已接受" : "未接受";
            missionAcceptanceText.color = accepted
                ? new Color(0f, 0.84f, 0f, 1f) : Color.gray;
        }
    }

    private void RefreshBookUI()
    {
        if (FinishButton == null)
        {
            return;
        }

        int pageCount = BookPages != null ? BookPages.Length : 0;

        FinishButton.interactable =
            gameManager != null &&
            gameManager.IsStageB &&
            gameManager.stageBMissionController != null &&
            gameManager.currentState == GameManager.GameState.Computer &&
            currentSection == ComputerSection.Book &&
            pageCount > 0 &&
            currentPageIndex == pageCount - 1 &&
            gameManager.stageBMissionController.CanCompleteManualReading;
    }

    private void HideAllContent()
    {
        if (storyContent != null)
            storyContent.SetActive(false);

        if (missionContent != null)
            missionContent.SetActive(false);

        if (reportContent != null)
            reportContent.SetActive(false);

        if (BookContent != null)
            BookContent.SetActive(false);
    }
}
