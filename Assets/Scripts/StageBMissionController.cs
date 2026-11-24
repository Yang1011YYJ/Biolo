using UnityEngine;

// 只管理 B 任務；移動、對話、分頁與轉場沿用共用系統。
public class StageBMissionController : MonoBehaviour
{
    public enum MissionStep
    {
        NotStarted,
        ReadMission,
        ReadManual,
        ObserveParents,
        ListCombinations
    }

    [Header("B 任務進度")]
    [Tooltip("本次 B 任務的第一塊親代牌子，指定 Hierarchy 中的牌子物件。")]
    public GameObject firstParentSign;

    [Tooltip("本次 B 任務的第二塊親代牌子，指定 Hierarchy 中的牌子物件。")]
    public GameObject secondParentSign;
    [Tooltip("B 專用進度，不會覆寫 A 的任務階段。")]
    [SerializeField] private MissionStep currentStep = MissionStep.NotStarted;
    [Tooltip("B 委託是否已接受，與 A 的接受紀錄分開保存。")]
    [SerializeField] private bool missionAccepted;
    [Tooltip("本次遊戲是否已開啟 B 任務第二頁。")]
    [SerializeField] private bool missionDetailsViewed;

    [Header("遺傳手冊")]
    [Tooltip("玩家是否已在接受 B 委託後開啟遺傳手冊頁。")][SerializeField]private bool manualPageViewed;
    [Tooltip("玩家是否已按下查閱完成。只保留在本次遊戲。")][SerializeField]private bool manualCompleted;

    [Header("B 親代觀察")]
    [Tooltip("是否已查看並關閉第一塊 B 親代牌子。")][SerializeField]private bool firstParentViewed;
    [Tooltip("是否已查看並關閉第二塊 B 親代牌子。")][SerializeField]private bool secondParentViewed;
    [Tooltip("後續整理配對使用的實驗桌目標定位點。尚未建立時可以先留空。")]public Transform experimentTableTarget;
    // 完成手冊後可查看；進入下一步後仍可重看。
    public bool CanObserveParents =>
        manualCompleted &&
        (
            currentStep == MissionStep.ObserveParents ||
            currentStep == MissionStep.ListCombinations
        );

    [Header("共用系統")]
    [Tooltip("整體流程與玩家操作權。")]
    public GameManager gameManager;
    [Tooltip("沿用場景中的共用對話控制器。")]
    public DialogueControll dialogueControll;
    [Tooltip("B 開場使用的老師圖片，沿用現有老師。")]
    public GameObject teacherVisual;
    [Tooltip("沿用場景中的任務提示。")]
    public MissionHintController missionHintController;
    [Tooltip("沿用場景中的任務箭頭。")]
    public MissionTargetIndicator missionTargetIndicator;
    [Tooltip("B 開始後指向的電腦目標定位點。")]
    public Transform computerTarget;

    public MissionStep CurrentStep => currentStep;
    public bool MissionAccepted => missionAccepted;
    public bool CanAcceptMission => currentStep == MissionStep.ReadMission &&
        !missionAccepted && missionDetailsViewed;

    public bool IsConfigured => gameManager != null &&
        missionHintController != null && missionTargetIndicator != null &&
        computerTarget != null &&
        (!gameManager.IsNarrativeVersion || dialogueControll != null);

    public void BeginStage()
    {
        if (currentStep != MissionStep.NotStarted) return;
        currentStep = MissionStep.ReadMission;
        missionAccepted = false;
        missionDetailsViewed = false;
        manualPageViewed = false;
        manualCompleted = false;
        firstParentViewed = false;
        secondParentViewed = false;

        if (gameManager.IsNarrativeVersion)
        {
            gameManager.EnterMissionDialogue();
            dialogueControll.OpenDialogue("指導老師", teacherVisual,
                new string[] {
                    "歡迎回到研究中心。這次，你要以新進研究員的身分，接下第一份育種預測委託。",
                    "中心準備培育一批豌豆。正式栽培需要時間和資源，我們得先了解這兩株親代可能產生哪些子代。",
                    "這次交給你的工作，就是提出培育前的預測，讓中心能根據你的報告安排後續工作。",
                    "委託資料已經放在電腦裡，先去看看吧。"
                }, FinishIntroduction);
        }
        else FinishIntroduction();
    }

    private void FinishIntroduction()
    {
        gameManager.ReturnToExploring();
        RefreshMissionHint();
        gameManager.ShowSystemHint(gameManager.IsNarrativeVersion
            ? "收到育種預測委託，請前往電腦查看"
            : "已開啟子代遺傳預測，請前往電腦查看");
    }

    public void NotifyMissionPageViewed(int pageIndex)
    {
        // 這裡只記錄任務第二頁。
        if (currentStep == MissionStep.ReadMission && pageIndex == 1)
        {
            missionDetailsViewed = true;
        }
    }

    public void AcceptMission()
    {
        if (!CanAcceptMission) return;
        missionAccepted = true;
        RefreshMissionHint();
        gameManager.ShowSystemHint(gameManager.IsNarrativeVersion
            ? "已接受育種預測委託" : "已確認子代遺傳預測實驗");
    }

    public void OnComputerClosed()
    {
        if (currentStep == MissionStep.ReadMission && missionAccepted)
            currentStep = MissionStep.ReadManual;
        RefreshMissionHint();
    }

    public void RefreshMissionHint()
    {
        switch (currentStep)
        {
            case MissionStep.ReadMission:

                missionHintController.ShowHint(
                    missionAccepted
                        ? "前往電腦，開啟「遺傳手冊」查閱規則"
                        : gameManager.IsNarrativeVersion
                            ? "前往電腦，查看並接受育種預測委託"
                            : "前往電腦，查看並確認子代遺傳預測實驗"
                );

                missionTargetIndicator.SetTarget(computerTarget);
                break;


            case MissionStep.ReadManual:

                missionHintController.ShowHint(
                    "前往電腦，開啟「遺傳手冊」查閱規則"
                );

                missionTargetIndicator.SetTarget(computerTarget);
                break;


            case MissionStep.ObserveParents:

                missionHintController.ShowHint(
                    "前往溫室，查看本次實驗的兩株親代資料"
                );

                GameObject nextSign =
                    !firstParentViewed
                        ? firstParentSign
                        : secondParentSign;

                if (nextSign != null)
                    missionTargetIndicator.SetTarget(nextSign.transform);
                else
                    missionTargetIndicator.ClearTarget();

                break;


            case MissionStep.ListCombinations:

                missionHintController.ShowHint(
                    "前往實驗桌，整理雙方可提供的遺傳因子與可能組合"
                );

                if (experimentTableTarget != null)
                    missionTargetIndicator.SetTarget(experimentTableTarget);
                else
                    missionTargetIndicator.ClearTarget();

                break;
        }
    }

    public void GetMissionContent(out string title, out string description,
        out string researchInfo)
    {
        bool narrative = gameManager.IsNarrativeVersion;
        title = narrative ? "研究委託：豌豆育種預測" : "實驗主題：子代遺傳預測";
        description = narrative
            ? "【研究中心的需求】\n正式栽培需要時間與資源。中心需要先了解指定親代交配後，可能產生哪些子代。\n\n【你負責的工作】\n查閱遺傳手冊與親代資料，整理可能組合，再以模擬培育核對預測。\n\n【委託完成條件】\n提出沒有漏列或重複計算的預測報告，供中心安排培育工作參考。"
            : "【實驗問題】\n指定親代交配後，子代可能有哪些基因型、表現型及比例？\n\n【實驗內容】\n查閱遺傳規則與親代資料，整理可能組合，再以模擬培育核對預測。\n\n【完成條件】\n完整列出可能組合，檢查漏列與重複，提交預測與紀錄。";
        researchInfo = "【本次親代條件】\n兩株親代的基因型皆為 Tt。\n\n【研究問題】\nTt × Tt 可能產生哪些子代？各種結果的比例為何？\n\n【進行方式】\n先查閱遺傳手冊，了解規則與 T、t 的意義；再查看親代資料、整理可能組合，並以模擬培育核對。\n\n【記錄重點】\n親代可提供的遺傳因子、子代基因型、表現型與比例。";
    }

    public void CompleteManualReading()
    {
        if (!missionAccepted ||
            currentStep != MissionStep.ReadManual ||
            !manualPageViewed ||
            manualCompleted)
        {
            return;
        }

        manualCompleted = true;
        currentStep = MissionStep.ObserveParents;

        RefreshMissionHint();

        gameManager.ShowSystemHint(
            gameManager.IsNarrativeVersion
                ? "已查閱遺傳手冊，請查看本次委託的兩株親代資料"
                : "已完成規則查閱，請查看本次實驗的兩株親代資料"
        );
    }

    // 電腦顯示到手冊最後一頁時呼叫。
    public void NotifyManualPageViewed()
    {
        if (!missionAccepted ||
            manualCompleted ||
            (
                currentStep != MissionStep.ReadMission &&
                currentStep != MissionStep.ReadManual
            ))
        {
            return;
        }

        manualPageViewed = true;
        currentStep = MissionStep.ReadManual;

        RefreshMissionHint();
    }

    // 提供共用介面判斷按鈕是否能互動。
    public bool CanCompleteManualReading =>
        missionAccepted &&
        currentStep == MissionStep.ReadManual &&
        manualPageViewed &&
        !manualCompleted;

    // 判斷這個物件是否為本次 B 任務的親代牌子。
    public bool IsCurrentParentSign(GameObject sign)
    {
        return sign != null &&
               (sign == firstParentSign ||
                sign == secondParentSign);
    }


    // 回傳 true 代表老師對話已接手操作權。
    public bool OnParentSignClosed(GameObject sign)
    {
        if (currentStep != MissionStep.ObserveParents ||
            !IsCurrentParentSign(sign))
        {
            return false;
        }

        // 設定錯誤時，不讓同一塊牌子算成兩株。
        if (firstParentSign == null ||
            secondParentSign == null ||
            firstParentSign == secondParentSign)
        {
            Debug.LogError("B 任務需要指定兩塊不同的親代牌子。", this);
            return false;
        }

        bool finishesObservation =
            (firstParentViewed || sign == firstParentSign) &&
            (secondParentViewed || sign == secondParentSign);

        if (finishesObservation &&
            gameManager.IsNarrativeVersion &&
            dialogueControll == null)
        {
            Debug.LogError("B 親代觀察引導尚未指定對話控制器。", this);
            return false;
        }

        if (sign == firstParentSign)
            firstParentViewed = true;

        if (sign == secondParentSign)
            secondParentViewed = true;

        if (!firstParentViewed || !secondParentViewed)
        {
            RefreshMissionHint();
            return false;
        }

        // 先推進，避免重複查看時再次播放。
        currentStep = MissionStep.ListCombinations;
        RefreshMissionHint();

        if (!gameManager.IsNarrativeVersion)
        {
            gameManager.ShowSystemHint(
                "親代資料已確認，請前往實驗桌整理可能組合"
            );

            return false;
        }

        gameManager.EnterMissionDialogue();

        dialogueControll.OpenDialogue(
            "指導老師",
            teacherVisual,
            new string[]
            {
            "這次交給你的兩株親代，基因型都是 Tt，外觀也都是高莖。",

            "不過，中心需要的是子代的預測，光看親代外觀還不夠。",

            "先到實驗桌，分別整理兩株親代可能提供的遺傳因子，再試著配對。這會成為你預測報告的第一份草稿。"
            },
            gameManager.ReturnToExploring
        );

        return true;
    }
}
