using TMPro; // 引入 TextMeshPro，用來修改任務接受狀態文字。
using UnityEngine; // 引入 Unity 基本功能。

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
        ReadyForExperiment   // 準備開始實驗。
    }


    [Header("目前任務進度")]

    public MissionStep currentMissionStep = MissionStep.ReadMission;
    // 遊戲開始時，第一個任務階段是查看電腦。


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
    // 主程式
    // ==========================================

    [Header("主程式")]

    public GameManager gameManager;
    // 整個教材的全域流程控制程式。


    // ==========================================
    // 對話系統
    // ==========================================

    [Header("對話系統")]

    public DialogueControll dialogueControll;
    // 共用的對話系統。


    public GameObject teacherVisual;
    // 指導老師的角色圖片或角色物件。

    [Header("腳本")]
    public HypothesisController hypothesisController;
    // 控制假設選擇介面。
    public ResearchRecordController researchRecordController;
    // 儲存玩家的研究紀錄。


    // ==========================================
    // 初始化
    // ==========================================

    private void Start()
    {
        // 遊戲開始時任務預設尚未接受。
        missionAccepted = false;


        // 顯示「未接受」。
        AcceptMissionState.text = "未接受";


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
            currentMissionStep = MissionStep.GoToGreenhouse;


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
            currentMissionStep = MissionStep.ObserveParents;


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
        currentMissionStep = MissionStep.MakeHypothesis;

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
        currentMissionStep = MissionStep.RecordHypothesis;

        // 對話結束，恢復玩家移動。
        gameManager.ReturnToExploring();

        // 顯示測試訊息。
        Debug.Log("任務進度：回研究室紀錄假設。");
    }

    // ==========================================
    // 親代觀察完成對話結束
    // ==========================================

    
}