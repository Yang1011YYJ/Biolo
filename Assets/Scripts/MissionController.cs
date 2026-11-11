using TMPro; // 引入 TextMeshPro，用來修改任務接受狀態文字。
using UnityEngine;
using UnityEngine.UI; // 引入 Unity 基本功能。

public class MissionController : MonoBehaviour
{
    // ==========================================
    // 任務目前進度
    // ==========================================

    public enum MissionStep
    {
        ReadMission,         // 查看電腦中的實習任務。
        GoToGreenhouse,      // 前往溫室。
        ObserveParents,      // 觀察兩株親代。
        MakeHypothesis,      // 提出假設。
        RecordHypothesis,    // 回電腦紀錄假設。
        GetClipboard,        // 取得實驗紀錄板。
        ReadyForExperiment,     // 準備開始第一次交配。
        SelfCrossF1,   // 第一子代與矮莖親代乙交配。
        ObserveFirstOffspring, // 查看第一子代的資訊牌。
        SowF2Seed,            // 播種 F1 自交取得的種子。
        ObserveSecondOffspring, // 查看第二子代的資訊牌。
        ReviewOffspringData, // 開啟實驗紀錄板，比較多株子代的結果。
        ReviewHypothesis,//老師引導玩家比對原本的假設
        ReportSubmitted
    }


    [Header("目前任務進度")]

    public MissionStep currentMissionStep = MissionStep.ReadMission;
    // 遊戲開始時，第一個任務階段是查看電腦。

    // 防止第一次觀察 F1 的引導對話重複播放。
    private bool firstOffspringDialogueStarted = false;

    [Header("UI")]
    public Button acceptMissionButton;
    // 電腦中用來接受實習任務的按鈕。

    // ==========================================
    // 任務接受狀態
    // ==========================================

    [Header("任務接受狀態")]

    [Tooltip("玩家是否已經查看並確認第一個實習任務")]
    public bool missionAccepted = false;
    // 紀錄玩家是否已經接受目前的實習任務。
    public TextMeshProUGUI AcceptMissionState;
    // 電腦中顯示「未接受／已接受」的文字。

    // ==========================================
    // 任務目標指示
    // ==========================================

    [Header("任務目標指示")]
    public Transform computerTarget;
    // 研究室電腦的位置。
    public Transform greenhouseTarget;
    // 溫室入口或溫室中心的位置。
    public Transform clipboardTarget;
    // 場景中的實驗紀錄板位置。
    public Transform experimentTarget;
    // 正式進行交配實驗的位置。


    // ==========================================
    // 對話系統
    // ==========================================

    [Header("對話系統")]
    public GameObject teacherVisual;
    // 指導老師的角色圖片或角色物件。

    [Header("腳本")]
    public MissionTargetIndicator missionTargetIndicator;
    // 控制目前任務目標上方的指示圖示。
    public HypothesisController hypothesisController;
    // 控制假設選擇介面。
    public ResearchRecordController researchRecordController;
    // 儲存玩家的研究紀錄。
    public MissionHintController missionHintController;
    // 負責顯示目前的任務提示。
    public DialogueControll dialogueControll;
    // 共用的對話系統。
    public GameManager gameManager;
    // 整個教材的全域流程控制程式。

    // ==========================================
    // 初始化
    // ==========================================

    private void Start()
    {
        // 遊戲開始時任務預設尚未接受。
        missionAccepted = false;


        // 顯示「未接受」。
        AcceptMissionState.text = "未接受";

        // 遊戲一開始任務尚未接受，
        // 所以接受任務按鈕可以使用。
        acceptMissionButton.interactable = true;


        // 將未接受狀態設定成灰色。
        AcceptMissionState.color =
            new Color(
                0.4792453f,
                0.4792453f,
                0.4792453f,
                1f
            );
    }


    // ==========================================
    // 接受實習任務
    // ==========================================

    public void AcceptMission()
    {
        // 如果任務已經接受過。
        if (missionAccepted)
        {
            // 不需要再重複執行。
            return;
        }


        // 標記任務已接受。
        missionAccepted = true;


        // 將狀態文字改成「已接受」。
        AcceptMissionState.text = "已接受";


        // 將狀態文字改成綠色。
        AcceptMissionState.color =
            new Color(
                0f,
                0.8415094f,
                5.015787e-08f,
                1f
            );

        
        // 任務一旦接受，
        // 這個按鈕就不能再按。
        acceptMissionButton.interactable = false;

        // 測試目前狀態。
        Debug.Log("玩家已確認實習任務。");


        // 這裡不播放老師對話。
        // 玩家可以繼續使用電腦。
    }


    // ==========================================
    // 玩家關閉電腦
    // ==========================================

    public bool OnComputerClosed()
    {
        // 如果玩家根本還沒接受任務。
        if (!missionAccepted)
        {
            // 不播放任何任務劇情。
            return false;
        }


        // 如果現在已經不是查看任務階段。
        if (currentMissionStep != MissionStep.ReadMission)
        {
            // 表示「前往溫室」這段劇情已經播放過。
            return false;
        }


        // 先通知 GameManager：
        // 現在進入任務對話狀態。
        gameManager.EnterMissionDialogue();


        // 使用共用對話系統播放指導老師的內容。
        dialogueControll.OpenDialogue(
            "指導老師", // 說話者名稱。

            teacherVisual, // 顯示指導老師角色。

            new string[]
            {
                // 第一段對話。
                "任務內容都確認好了嗎？",

                // 第二段對話。
                "接下來先去溫室看看這次的實驗材料吧，確認兩株親代的特徵。"
            },

            OnMissionDialogueFinished
        // 整段對話完成後，
        // 自動執行這個函式。
        );


        // 告訴 GameManager：
        // 現在確實有任務劇情正在播放。
        return true;
    }


    // ==========================================
    // 任務對話完整播放結束
    // ==========================================

    private void OnMissionDialogueFinished()
    {
        // 如果目前仍然處於查看任務階段。
        if (currentMissionStep == MissionStep.ReadMission)
        {
            // 將任務推進到「前往溫室」。
            SetMissionStep(MissionStep.GoToGreenhouse);


            // 測試目前任務進度。
            Debug.Log("任務進度：前往溫室。");
        }


        // 整段對話結束後，
        // 回到自由探索狀態。
        gameManager.ReturnToExploring();
    }

    // ==========================================
    // 玩家開始查看親代
    // ==========================================

    public void OnParentSignViewed()
    {
        // 如果目前任務是「前往溫室」。
        if (currentMissionStep == MissionStep.GoToGreenhouse)
        {
            // 代表玩家已經成功抵達，
            // 並開始查看親代資料。
            SetMissionStep(MissionStep.ObserveParents);


            // 測試目前任務進度。
            Debug.Log("任務進度：觀察親代。");
        }
    }

    // ==========================================
    // 兩株親代觀察完成
    // ==========================================

    // ==========================================
    // 兩株親代觀察完成
    // ==========================================

    public bool OnParentsObserved()
    {
        // 只有在觀察親代階段才能進入假設。
        if (currentMissionStep != MissionStep.ObserveParents)
        {
            return false;
        }

        // 更新任務進度。
        SetMissionStep(MissionStep.MakeHypothesis);

        // 進入任務對話狀態。
        gameManager.EnterMissionDialogue();

        // 播放指導老師的提問。
        dialogueControll.OpenDialogue(
            "指導老師",

            teacherVisual,

            new string[]
            {
            "根據你剛才觀察到的兩株親代，你認為高莖和矮莖，哪一種性狀可能是顯性的？"
            },

            OnHypothesisPromptFinished
        );

        return true;
    }


    // ==========================================
    // 老師提問結束
    // ==========================================

    private void OnHypothesisPromptFinished()
    {
        // 開啟假設選擇介面。
        hypothesisController.OpenPanel();
    }


    // ==========================================
    // 玩家確認假設
    // ==========================================

    public void OnHypothesisConfirmed(
        ResearchRecordController.HypothesisType hypothesis)
    {
        // 確認目前任務確實處於假設階段。
        if (currentMissionStep != MissionStep.MakeHypothesis)
        {
            return;
        }

        // 將玩家的假設交給研究紀錄系統儲存。
        researchRecordController.SaveHypothesis(hypothesis);

        // 播放老師接下來的指示。
        dialogueControll.OpenDialogue(
            "指導老師",

            teacherVisual,

            new string[]
            {
            "好，先把這個想法記下來。請回研究室的電腦，將你的假設正式記錄下來，並取得實驗紀錄板。完成後，我們就可以開始交配實驗了。"
            },

            OnHypothesisDialogueFinished
        );
    }


    // ==========================================
    // 假設對話結束
    // ==========================================

    private void OnHypothesisDialogueFinished()
    {
        // 更新任務進度。
        SetMissionStep(MissionStep.RecordHypothesis);

        // 對話結束，恢復玩家移動。
        gameManager.ReturnToExploring();

        // 顯示測試訊息。
        Debug.Log("任務進度：回研究室紀錄假設。");
    }

    // ==========================================
    // 親代觀察完成對話結束
    // ==========================================

    // ==========================================
    // 更新目前任務階段
    // ==========================================

    private void SetMissionStep(MissionStep newStep)
    {
        // 更新目前任務階段。
        currentMissionStep = newStep;

        // 更新畫面上的任務文字。
        UpdateMissionHint();

        // 更新目前世界中的任務目標指示。
        UpdateMissionTarget();
    }

    // ==========================================
    // 更新任務提示文字
    // ==========================================

    private void UpdateMissionHint()
    {
        // 根據目前任務階段，
        // 決定玩家畫面上應該看到什麼提示。
        switch (currentMissionStep)
        {
            // ======================================
            // 查看實習任務
            // ======================================

            case MissionStep.ReadMission:

                // 告訴玩家先去操作研究室電腦。
                missionHintController.ShowHint(
                    "前往研究室電腦查看任務"
                );

                break;


            // ======================================
            // 前往溫室
            // ======================================

            case MissionStep.GoToGreenhouse:

                // 提醒玩家前往溫室。
                missionHintController.ShowHint(
                    "前往溫室查看兩株豌豆"
                );

                break;


            // ======================================
            // 觀察親代
            // ======================================

            case MissionStep.ObserveParents:

                // 提醒玩家兩株親代都需要查看。
                missionHintController.ShowHint(
                    "查看高莖與矮莖親代的牌子"
                );

                break;


            // ======================================
            // 提出假設
            // ======================================

            case MissionStep.MakeHypothesis:

                // 提醒玩家根據剛才的資訊提出假設。
                missionHintController.ShowHint(
                    "提出假設"
                );

                break;


            // ======================================
            // 紀錄假設
            // ======================================

            case MissionStep.RecordHypothesis:

                // 提醒玩家回到研究室電腦。
                missionHintController.ShowHint(
                    "前往研究室電腦記錄假設"
                );

                break;


            // ======================================
            // 取得紀錄板
            // ======================================

            case MissionStep.GetClipboard:

                // 提醒玩家取得等等實驗會使用的紀錄板。
                missionHintController.ShowHint(
                    "取得實驗紀錄板"
                );

                break;


            // ======================================
            // 準備開始實驗
            // ======================================

            case MissionStep.ReadyForExperiment:

                // 告訴玩家已經可以正式開始實驗。
                missionHintController.ShowHint(
                    "前往溫室進行實驗"
                );

                break;

            // ======================================
            // 第一子代資料牌子查看
            // ======================================
            case MissionStep.ObserveFirstOffspring:

                missionHintController.ShowHint(
                    "查看新長出的第一子代（F1）旁的資訊牌"
                );

                break;

            case MissionStep.SelfCrossF1:
                // 提醒兩個交配方必須選取同一株 F1。
                missionHintController.ShowHint(
                    "選擇空種植區，連續選同一株第一子代兩次，進行自花授粉"
                );
                break;

            case MissionStep.SowF2Seed:
                // 自交完成後，引導播種取得的種子。
                missionHintController.ShowHint(
                    "選擇空種植區，播種第一子代自交取得的種子"
                );
                break;

            case MissionStep.ObserveSecondOffspring:
                // 植株長成後，引導查看資料。
                missionHintController.ShowHint(
                    "查看新長出的第二子代（F2）旁的資訊牌"
                );
                break;

            case MissionStep.ReviewOffspringData:
                // 引導玩家使用背包裡已取得的紀錄板。
                missionHintController.ShowHint(
                    "從背包開啟實驗紀錄板，查看多株子代的觀察資料"
                );
                break;

            case MissionStep.ReviewHypothesis:
                // 引導玩家回研究室，準備檢視原始假設。
                missionHintController.ShowHint("前往研究室電腦，檢視原本的假設");
                break;

            case MissionStep.ReportSubmitted:
                // 提醒玩家關閉電腦，接續等待流程。
                missionHintController.ShowHint("報告已提交，請關閉電腦等待研究中心回覆");
                break;
        }
    }

    // ==========================================
    // 更新任務目標指示
    // ==========================================

    private void UpdateMissionTarget()
    {
        // 如果沒有指定任務指示器。
        if (missionTargetIndicator == null)
        {
            return;
        }


        // 根據目前任務階段，
        // 決定世界中的指示器要指向哪裡。
        switch (currentMissionStep)
        {
            // ======================================
            // 查看電腦任務
            // ======================================

            case MissionStep.ReadMission:

                // 指向研究室電腦。
                missionTargetIndicator.SetTarget(
                    computerTarget
                );

                break;


            // ======================================
            // 前往溫室
            // ======================================

            case MissionStep.GoToGreenhouse:

                // 指向溫室。
                missionTargetIndicator.SetTarget(
                    greenhouseTarget
                );

                break;


            // ======================================
            // 觀察親代
            // ======================================

            case MissionStep.ObserveParents:

                // 玩家此時通常已經在溫室附近。
                // 仍然暫時維持溫室作為目標。
                // 因為距離夠近，所以指示器會自動隱藏。
                missionTargetIndicator.SetTarget(
                    greenhouseTarget
                );

                break;


            // ======================================
            // 提出假設
            // ======================================

            case MissionStep.MakeHypothesis:

                // 此階段是 UI 操作，
                // 不需要世界中的導航提示。
                missionTargetIndicator.ClearTarget();

                break;


            // ======================================
            // 回電腦記錄假設
            // ======================================

            case MissionStep.RecordHypothesis:

                // 指向研究室電腦。
                missionTargetIndicator.SetTarget(
                    computerTarget
                );

                break;


            // ======================================
            // 取得實驗紀錄板
            // ======================================

            case MissionStep.GetClipboard:

                // 指向場景中的實驗紀錄板。
                missionTargetIndicator.SetTarget(
                    clipboardTarget
                );

                break;


            // ======================================
            // 前往實驗
            // ======================================

            case MissionStep.ReadyForExperiment:

                // 指向正式進行實驗的位置。
                missionTargetIndicator.SetTarget(
                    experimentTarget
                );

                break;

            case MissionStep.SelfCrossF1:
            case MissionStep.SowF2Seed:
            case MissionStep.ObserveSecondOffspring:
                // 這幾個階段先清除上一個目標，避免箭頭仍指向舊設備。
                // 動態植株的精確指向之後再接。
                missionTargetIndicator.ClearTarget();
                break;

            case MissionStep.ReviewOffspringData:
                // 紀錄板從背包開啟，不需要場景中的目標箭頭。
                missionTargetIndicator.ClearTarget();
                break;

            case MissionStep.ReviewHypothesis:
                // 將任務箭頭指向研究室電腦。
                missionTargetIndicator.SetTarget(computerTarget);
                break;
        }
    }

    // ==========================================
    // 第一次顯示目前任務
    // ==========================================

    public void ShowCurrentMissionHint()
    {
        // 顯示目前任務文字。
        UpdateMissionHint();


        // 同時顯示目前任務的世界目標。
        UpdateMissionTarget();
    }

    // ==========================================
    // 是否可以正式記錄假設
    // ==========================================

    public bool CanRecordHypothesis()
    {
        // 只有目前任務是 RecordHypothesis，
        // 才允許玩家正式紀錄。
        return currentMissionStep == MissionStep.RecordHypothesis;
    }


    // ==========================================
    // 假設正式記錄完成
    // ==========================================

    public void OnHypothesisRecorded()
    {
        // 確認目前真的處於記錄假設階段。
        if (currentMissionStep != MissionStep.RecordHypothesis)
        {
            // 如果不是，就不推進任務。
            return;
        }


        // 假設已完成正式記錄，
        // 下一步改成取得實驗紀錄板。
        SetMissionStep(MissionStep.GetClipboard);


        // 測試用訊息。
        Debug.Log("任務進度：取得實驗紀錄板。");
    }

    // ==========================================
    // 判斷目前是否可以取得實驗紀錄板
    // ==========================================

    public bool CanGetClipboard()
    {
        // 只有任務進度來到 GetClipboard 時，
        // 才允許玩家取得紀錄板。
        return currentMissionStep == MissionStep.GetClipboard;
    }


    // ==========================================
    // 玩家成功取得實驗紀錄板
    // ==========================================

    public void OnClipboardObtained()
    {
        // 如果目前不是取得紀錄板的任務階段，
        // 就不執行。
        if (!CanGetClipboard())
        {
            return;
        }

        // 將任務推進到「準備進行實驗」。
        SetMissionStep(MissionStep.ReadyForExperiment);

        // 顯示測試訊息。
        Debug.Log(
            "玩家已取得實驗紀錄板，任務進入 ReadyForExperiment。"
        );
    }

    // ==========================================
    // 玩家查看完第一子代資料
    // ==========================================

    public bool OnFirstOffspringViewed()
    {
        // 只有第一次交配準備階段，才觸發這段後續引導。
        if (currentMissionStep != MissionStep.ObserveFirstOffspring ||
            firstOffspringDialogueStarted)
        {
            return false;
        }

        // 播放前就標記，避免重複觸發。
        firstOffspringDialogueStarted = true;

        gameManager.EnterMissionDialogue();

        dialogueControll.OpenDialogue(
            "指導老師",
            teacherVisual,
            new string[]
            {
                // 只描述玩家目前實際觀察到的植株。
                "這株第一子代是高莖，和高莖親代的外觀相同。我們先把這個觀察結果記下來。",

                // 引導學生思考沒有表現出來的性狀。
                "不過，它也有一株矮莖親代。矮莖的遺傳訊息，還留在這株第一子代裡嗎？",

                // 說明下一次實驗的操作與目的。
                "接下來，讓這株第一子代自花授粉，也就是用同一株植物的花粉完成授粉，再觀察它產生的第二子代。",

                // 配合目前兩次點選的介面。
                "請選擇一個空種植區，開啟交配選單，連續選擇同一株第一子代兩次，再確認交配。"
            },
            OnFirstOffspringDialogueFinished
        );

        return true;
    }

    // ==========================================
    // 第一子代引導對話結束
    // ==========================================

    private void OnFirstOffspringDialogueFinished()
    {
        // 對話結束後，要求玩家進行 F1 自交。
        SetMissionStep(MissionStep.SelfCrossF1);

        // 恢復玩家操作。
        gameManager.ReturnToExploring();

        // 顯示目前進度，方便測試。
        Debug.Log("任務進度：讓同一株第一子代自花授粉。");
    }

    // ==========================================
    // 第一子代已種植完成
    // ==========================================

    public void OnFirstOffspringPlanted()
    {
        // 只在首次實驗階段推進，避免後續播種倒退任務。
        if (currentMissionStep != MissionStep.ReadyForExperiment)
        {
            return;
        }

        SetMissionStep(MissionStep.ObserveFirstOffspring);

        Debug.Log("任務進度：查看第一子代資訊牌。");
    }

    public void OnF1SelfCrossCompleted()
    {
        // 只在要求 F1 自交的階段推進，避免任務倒退。
        if (currentMissionStep != MissionStep.SelfCrossF1)
        {
            return;
        }

        // 自交成功取得種子後，下一步是播種。
        SetMissionStep(MissionStep.SowF2Seed);
    }

    public void OnSecondOffspringPlanted()
    {
        // 只在要求播種 F2 的階段推進。
        if (currentMissionStep != MissionStep.SowF2Seed)
        {
            return;
        }

        // 播種成功後，要求玩家查看 F2 牌子。
        SetMissionStep(MissionStep.ObserveSecondOffspring);
    }

    public bool OnSecondOffspringViewed()
    {
        // 只有正在等待觀察 F2 時，才播放這段引導。
        if (currentMissionStep != MissionStep.ObserveSecondOffspring)
        {
            return false;
        }

        // 玩家已查看完 F2，開放紀錄板的多株觀察資料。
        researchRecordController.UnlockOffspringData();

        // 接續原本的任務切換。
        // 先切換任務，避免重複關閉牌子時再次觸發。
        SetMissionStep(MissionStep.ReviewOffspringData);

        // 對話期間暫停玩家操作。
        gameManager.EnterMissionDialogue();

        // 引導學生從單株觀察，進一步查看多株資料。
        dialogueControll.OpenDialogue(
            "指導老師",
            teacherVisual,
            new string[]
            {
            // 不預設玩家這次種出的性狀。
            "我們已經觀察了這株第二子代。不過，一株的結果，還不能告訴我們整批子代會有哪些性狀、各有多少。",

            // 說明接下來比較資料的目的。
            "接下來，把你的觀察和研究中心提供的多株培育資料放在一起，比較第一子代與第二子代的高、矮莖結果。",

            // 告知下一個操作。
            "請從背包開啟實驗紀錄板，查看子代的觀察資料。"
            },
            () =>
            {
                // 對話結束後，恢復自由操作。
                gameManager.ReturnToExploring();
            }
        );

        // 通知牌子系統：老師對話已接手操作狀態。
        return true;
    }

    public bool TryStartOffspringReviewDialogue()
    {
        // 只有正在判讀子代資料，而且兩題都答對，才接續對話。
        if (currentMissionStep != MissionStep.ReviewOffspringData ||
            researchRecordController == null ||
            !researchRecordController.OffspringQuizCompleted)
        {
            return false;
        }

        // 推進任務，避免下次關閉背包時重播。
        SetMissionStep(MissionStep.ReviewHypothesis);

        // 讓老師對話接手玩家操作。
        gameManager.EnterMissionDialogue();

        // 引導玩家用兩代的資料檢視原本的假設。
        dialogueControll.OpenDialogue(
            "指導老師",
            teacherVisual,
            new string[]
            {
            "你找到了兩個線索。這份資料裡，第一子代都是高莖，到了第二子代，矮莖又出現了。",
            "回想一下你最初的假設。如果矮莖是顯性，純品系高莖和純品系矮莖交配後，第一子代應該長成什麼樣子？",
            "請回到研究室的電腦，比一比你原本的預測和觀察結果，想想你的假設是否需要修正。"
            },
            // 老師說完後，再讓玩家自由移動。
            gameManager.ReturnToExploring
        );

        // 告知呼叫端，這次由對話接手。
        return true;
    }

    public bool TrySubmitHypothesisReport(
    ResearchRecordController.HypothesisType judgment)
    {
        // 確認目前正在檢視假設，並且玩家已選擇一個判斷。
        if (currentMissionStep != MissionStep.ReviewHypothesis ||
            researchRecordController == null ||
            judgment == ResearchRecordController.HypothesisType.None)
        {
            return false;
        }

        // 保存本次報告，供研究中心產生回覆。
        researchRecordController.SaveSubmittedHypothesis(judgment);

        // 標記報告已提交，等待玩家關閉電腦。
        SetMissionStep(MissionStep.ReportSubmitted);
        return true;
    }
}