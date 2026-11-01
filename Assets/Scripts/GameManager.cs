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
        PreStart,   // 還在開始畫面，遊戲尚未正式開始。
        Intro,      // 開場劇情。
        Exploring,  // 玩家可以自由移動。
        Computer    // 玩家正在操作電腦。
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

    public GameObject introDialoguePanel; // 開場劇情對話框。
    public GameObject computerPanel; // 電腦介面。
    public GameObject BlackPanel;
    public Button startButton;

    [Header("場景中的設備")]
    [Tooltip("場景中的電腦設備")]public GameObject computerObject;

    [Header("教材進度")]
    [Tooltip("玩家是否已經查看並確認第一個實習任務")]
    public bool missionAccepted = false; // 紀錄玩家是否已經接下任務。
    public TextMeshProUGUI AcceptMissionState;

    [Header("腳本")]
    public FadeController fadeController;


    private void Start()
    {
        // 強制開啟 Unity Logger。
        Debug.unityLogger.logEnabled = true;

        // 測試三種訊息。
        Debug.Log("GameManager Log");
        Debug.LogWarning("GameManager Warning");
        Debug.LogError("GameManager Error");

        // 遊戲一開始先隱藏玩家。
        playerVisual.SetActive(false);

        // 遊戲一開始先隱藏場景中的電腦設備。
        computerObject.SetActive(false);

        //未接受任何任務
        AcceptMissionState.text = "未接受";
        AcceptMissionState.color = new Color(0.4792453f, 0.4792453f, 0.4792453f,1);

        // 關閉黑幕。
        BlackPanel.SetActive(false);

        // 關閉開場對話框。
        introDialoguePanel.SetActive(false);

        // 關閉電腦介面。
        computerPanel.SetActive(false);

        // 設定初始狀態。
        SetState(GameState.PreStart);
    }

    private void StartGame()
    {
        // 測試 StartGame 是否真的有被執行。
        Debug.LogError("★★★ StartGame 有執行 ★★★");

        // 將玩家移動到遊戲開始位置。
        player.position = startPosition;

        // 顯示玩家。
        playerVisual.SetActive(true);

        // 顯示場景中的電腦。
        computerObject.SetActive(true);

        // 顯示目前 GameManager 指到的是哪一個物件。
        Debug.LogError("電腦物件名稱：" + computerObject.name);

        // 檢查電腦自己是否被啟用。
        Debug.LogError("activeSelf：" + computerObject.activeSelf);

        // 檢查電腦在整個 Hierarchy 中是否真的有效。
        Debug.LogError("activeInHierarchy：" + computerObject.activeInHierarchy);
    }

    public void StartButton()
    {
        Debug.LogError("★★★ 現在執行的是 GameManager.StartButton ★★★");

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
        // 顯示開場劇情對話框。
        introDialoguePanel.SetActive(true);

        // 正式進入開場劇情狀態。
        SetState(GameState.Intro);
    }


    // 這個函式給開場對話框的 Button 呼叫。
    public void CloseIntro()
    {
        // 關閉開場對話框。
        introDialoguePanel.SetActive(false);

        // 進入自由探索狀態。
        SetState(GameState.Exploring);
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
        // 關閉電腦 Panel。
        computerPanel.SetActive(false);

        // 回到自由探索狀態。
        SetState(GameState.Exploring);
    }

    // ==========================================
    // 確認實習任務
    // ==========================================

    public void AcceptMission()
    {
        //接受任務
        AcceptMissionState.text = "已接受";
        AcceptMissionState.color = new Color(0, 0.8415094f, 5.015787e-08f, 1);

        // 標記玩家已經查看並確認任務。
        missionAccepted = true;

        // 測試用訊息，之後可以刪除。
        Debug.Log("玩家已確認實習任務。");
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
}