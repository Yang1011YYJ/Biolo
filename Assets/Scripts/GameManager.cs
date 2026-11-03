using TMPro;
using UnityEngine;
using UnityEngine.UI; // 引入 Unity 基本功能。

public class GameManager : MonoBehaviour
{
    // ==============================
    // 遊戲目前所在的流程狀態
    // ==============================

    public enum GameState
    {
        PreStart,        // 遊戲尚未正式開始。
        Intro,           // 開場劇情。
        Exploring,       // 玩家自由探索。
        Computer,        // 玩家正在操作電腦。
        MissionDialogue, // 任務相關對話。
        SignInfo,         // 玩家正在查看場景中的牌子資訊。
        Inventory        // 玩家正在操作背包。
    }


    [Header("目前狀態")]

    public GameState currentState; // 紀錄現在處於哪一個流程狀態。


    [Header("玩家")]

    public Move playerMove; // 玩家身上的移動程式。
    public Transform player; // 玩家本身的 Transform。
    [Tooltip("玩家顯示物件")] public GameObject playerVisual; // 要隱藏／顯示的玩家模型或圖片物件
    [Tooltip("玩家開始遊戲時要出現的位置")] public Vector3 startPosition = new Vector3(0f, 31.0599995f, 71.4000015f);
    public int FadeIn, FadeOut;


    [Header("介面")]
    public GameObject computerPanel; // 電腦介面。
    public GameObject BlackPanel;
    public Button startButton;
    [Tooltip("背包介面UI")] public GameObject BackpackPanel;

    [Header("場景中的設備")]
    [Tooltip("場景中的電腦設備")]public GameObject computerObject;

    [Header("教材進度")]

    [Header("腳本")]
    public FadeController fadeController;
    public MissionController missionController; // 控制任務流程。
    public DialogueControll dialogueControll;// 控制共用對話框的顯示與句子推進。


    private void Start()
    {
        // 強制開啟 Unity Logger。
        Debug.unityLogger.logEnabled = true;

        // 測試三種訊息。
        //Debug.Log("GameManager Log");
        //Debug.LogWarning("GameManager Warning");
        //Debug.LogError("GameManager Error");

        // 遊戲一開始先隱藏玩家。
        playerVisual.SetActive(false);

        // 遊戲一開始先隱藏場景中的電腦設備。
        computerObject.SetActive(false);

        // 關閉黑幕。
        BlackPanel.SetActive(false);

        // 關閉電腦介面。
        computerPanel.SetActive(false);

        //關閉背包介面。
        BackpackPanel.SetActive(false);

        // 設定初始狀態。
        SetState(GameState.PreStart);
    }

    private void StartGame()
    {
        // 測試 StartGame 是否真的有被執行。
        //Debug.LogError("★★★ StartGame 有執行 ★★★");

        // 將玩家移動到遊戲開始位置。
        player.position = startPosition;

        // 顯示玩家。
        playerVisual.SetActive(true);

        // 顯示場景中的電腦。
        computerObject.SetActive(true);

        //關閉開始按鈕
        startButton.gameObject.SetActive(false);

        //// 顯示目前 GameManager 指到的是哪一個物件。
        //Debug.LogError("電腦物件名稱：" + computerObject.name);

        //// 檢查電腦自己是否被啟用。
        //Debug.LogError("activeSelf：" + computerObject.activeSelf);

        //// 檢查電腦在整個 Hierarchy 中是否真的有效。
        //Debug.LogError("activeInHierarchy：" + computerObject.activeInHierarchy);
    }

    public void StartButton()
    {
        //Debug.LogError("★★★ 現在執行的是 GameManager.StartButton ★★★");

        if (currentState == GameState.PreStart)
        {
            Debug.Log("2. 狀態是 PreStart");

            BlackPanel.SetActive(true);

            fadeController.Fade(BlackPanel, 0, 1, FadeIn, () =>
            {
                Debug.Log("3. 淡入完成");

                fadeController.Wait(1.5f, () =>
                {
                    Debug.Log("4. Wait 完成");

                    StartGame();

                    fadeController.Fade(BlackPanel, 1, 0, FadeOut, () =>
                    {
                        Debug.Log("5. 淡出完成");

                        BlackPanel.SetActive(false);

                        startButton.gameObject.SetActive(false);

                        StartIntro();
                    });
                });
            });
        }
    }

    // ==========================================
    // 開場劇情
    // ==========================================

    public void StartIntro()
    {
        // 將整體流程切換成開場劇情狀態。
        SetState(GameState.Intro);


        // 使用共用對話系統播放開場內容。
        dialogueControll.OpenDialogue(
            "", // 如果開場沒有特定說話者，名稱可以先留空。

            null, // 如果開場沒有角色圖片，可以先不指定。

            new string[]
            {
            // 第一段開場內容。
            "歡迎來到研究中心。",

            // 第二段開場內容。
            "今天開始，你將以實習生的身分參與研究工作。",

            // 第三段開場內容。
            "先到研究室的電腦看看今天的實習任務吧。"
            },

            OnIntroDialogueFinished
        // 整段開場對話播放完成後，
        // 自動執行這個函式。
        );
    }


    private void OnIntroDialogueFinished()
    {
        // 開場劇情全部播放完成後，
        // 正式讓玩家進入自由探索。
        ReturnToExploring();


        // 顯示第一個正式任務提示。
        missionController.ShowCurrentMissionHint();
    }


    // ==========================================
    // 玩家移動
    // ==========================================

    public void MovePlayerTo(Vector3 position)
    {
        // 只有在自由探索狀態才能移動。
        if (currentState != GameState.Exploring)
        {
            return;
        }

        // 呼叫玩家移動程式。
        playerMove.SetTargetPosition(position);
    }


    // ==========================================
    // 電腦互動
    // ==========================================

    public void TryOpenComputer(ComputerInteractable computer)
    {
        // 如果目前不是自由探索狀態，就不能使用電腦。
        if (currentState != GameState.Exploring)
        {
            return;
        }


        // 詢問電腦：玩家目前距離夠不夠。
        if (!computer.IsPlayerInRange(player))
        {
            // 目前先用 Console 提示。
            Debug.Log("距離電腦太遠。");

            // 不執行後續操作。
            return;
        }
        // 先讓電腦自己重置內部顯示內容。
        computer.ResetContent();

        // 玩家距離足夠，開啟電腦介面。
        computerPanel.SetActive(true);

        // 將流程切換成使用電腦。
        SetState(GameState.Computer);
    }


    // 這個函式給電腦 Panel 的關閉按鈕呼叫。
    public void CloseComputer()
    {
        // 關閉電腦介面。
        computerPanel.SetActive(false);


        // 通知 MissionController：
        // 玩家剛剛把電腦關掉了。
        bool missionDialogueStarted =
            missionController.OnComputerClosed();


        // 如果 MissionController 已經開啟任務對話，
        // 就不要把玩家切回自由探索。
        if (missionDialogueStarted)
        {
            // 任務對話會自己維持 MissionDialogue 狀態。
            return;
        }


        // 如果這次沒有任何任務劇情，
        // 就正常回到自由探索。
        ReturnToExploring();
    }


    // ==========================================
    // 流程狀態控制
    // ==========================================

    private void SetState(GameState newState)
    {
        // 更新目前狀態。
        currentState = newState;


        // 如果目前是自由探索狀態。
        if (currentState == GameState.Exploring)
        {
            // 允許玩家移動。
            playerMove.SetMovementEnabled(true);
        }
        else
        {
            // 其他狀態全部禁止玩家移動。
            playerMove.SetMovementEnabled(false);
        }
    }


    // 提供 PlayerClick 查詢：
    // 現在是不是可以接受場景點擊。
    public bool CanUseWorldInput()
    {
        // 只有自由探索時可以點場景。
        return currentState == GameState.Exploring;
    }


    // ==========================================
    // 任務對話狀態
    // ==========================================

    public void EnterMissionDialogue()
    {
        // 進入任務對話狀態。
        SetState(GameState.MissionDialogue);
    }


    // ==========================================
    // 回到自由探索
    // ==========================================

    public void ReturnToExploring()
    {
        // 回到自由探索狀態。
        SetState(GameState.Exploring);
    }

    // ==========================================
    // 背包狀態
    // ==========================================

    public void EnterInventory()
    {
        // 進入背包操作狀態。
        SetState(GameState.Inventory);
    }


    // ==========================================
    // 溫室互動
    // ==========================================

    public void TryInteractGreenhouse(
        GreenhouseController greenhouse,
        GameObject clickedObject)
    {
        // 只有玩家正在自由探索時，
        // 才允許和溫室中的物件互動。
        if (currentState != GameState.Exploring)
        {
            // 如果現在不是自由探索，就直接停止。
            return;
        }


        // 讓GreenhouseController判斷：
        // 點到的物件是不是牌子，而且玩家距離是否足夠。
        bool signOpened = greenhouse.TryOpenSign(
            clickedObject, // 玩家實際點擊到的物件。
            player // 玩家本身的Transform。
        );


        // 如果真的成功開啟牌子資訊。
        if (signOpened)
        {
            // 把整體操作狀態切換成牌子資訊。
            SetState(GameState.SignInfo);
        }
    }
}