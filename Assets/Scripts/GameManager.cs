using TMPro;
using UnityEngine;
using UnityEngine.UI; // 引入 Unity 基本功能。
using System.Collections;
using UnityEngine.AI;

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
        Inventory,       // 玩家正在操作背包。
        PlantingActions,  // 玩家正在選擇播種或交配。
        ParentSelection, // 正在選擇交配親代。
        BreedingSequence // 系統控制玩家進行交配演出。
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

    [Header("交配演出")]

    [Tooltip("抵達每株親代後停留的秒數。")]
    [Min(0f)]
    public float parentVisitWait = 1f;

    [Tooltip("每一段系統移動允許的最長秒數。超時會中止並恢復操作。")]
    [Min(1f)]
    public float systemMoveTimeout = 30f;

    // 防止交配演出重複開始。
    private bool isBreedingSequenceRunning;

    // 記錄單段系統移動是否成功抵達。
    private bool systemMoveSucceeded;

    // 按下開始交配時的玩家位置。
    private Vector3 breedingReturnPosition;

    [Header("種子")]

    [Tooltip("目前持有的第一子代種子數量。純品系高莖與矮莖交配成功後增加。")]
    [Min(0)]
    public int firstGenerationSeedCount = 0;

    [Header("親代選擇")]

    [Header("腳本")]
    public FadeController fadeController;
    public MissionController missionController; // 控制任務流程。
    public DialogueControll dialogueControll;// 控制共用對話框的顯示與句子推進。
    [Tooltip("負責顯示播種、交配與關閉按鈕的選單控制器。")]public PlantingActionPanel plantingActionPanel;
    // 暫存玩家選定的種植區，後續交配生成子代時使用。
    private PlantingSpot selectedPlantingSpot;
    [Tooltip("交配時使用的親代選擇面板控制器。")]public ParentSelectionPanel parentSelectionPanel;
    // 保存這次確認的兩株親代，後續演出依此順序走位。
    private PeaPlant selectedFirstParent;
    private PeaPlant selectedSecondParent;


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
            "指導老師", // 如果開場沒有特定說話者，名稱可以先留空。

            missionController.teacherVisual, // 如果開場沒有角色圖片，可以先不指定。

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
        currentState = newState;

        // 探索時由玩家移動，交配演出時由系統移動。
        bool movementEnabled =
            currentState == GameState.Exploring ||
            currentState == GameState.BreedingSequence;

        playerMove.SetMovementEnabled(movementEnabled);
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
        Debug.Log(
    $"牌子互動｜" +
    $"控制器：{greenhouse.name}｜" +
    $"點擊物件：{clickedObject.name}｜" +
    $"物件座標：{clickedObject.transform.position}｜" +
    $"玩家座標：{player.position}｜" +
    $"直線距離：{Vector3.Distance(player.position, clickedObject.transform.position):F2}",
    clickedObject
);

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

    // ==========================================
    // 嘗試開啟種植區操作選單
    // ==========================================

    public void TryOpenPlantingSpot(PlantingSpot spot)
    {
        if (currentState != GameState.Exploring)
        {
            return;
        }

        if (spot == null || plantingActionPanel == null)
        {
            Debug.LogWarning("種植區或操作選單尚未指定。");
            return;
        }

        if (!spot.CanUse())
        {
            Debug.Log("這個種植區目前無法使用。");
            return;
        }

        // 目前先沿用與電腦相同的操作方式：
        // 玩家走近後，再點擊開啟。
        if (!spot.IsPlayerInRange(player))
        {
            Debug.Log("距離種植區太遠，請先走近。");
            return;
        }

        selectedPlantingSpot = spot;

        // 切換狀態時，既有 SetState 會停止玩家移動，
        // PlayerClick 也會停止接受場景輸入。
        SetState(GameState.PlantingActions);

        plantingActionPanel.Open(
            spot,
            OnCrossbreedSelected,
            ClosePlantingActions,
            firstGenerationSeedCount
        );
    }

    // ==========================================
    // 關閉種植操作選單
    // ==========================================

    private void ClosePlantingActions()
    {
        selectedPlantingSpot = null;

        ReturnToExploring();
    }

    // ==========================================
    // 選擇交配
    // ==========================================

    private void OnCrossbreedSelected(PlantingSpot spot)
    {
        if (spot == null || !spot.CanUse())
        {
            ClosePlantingActions();
            return;
        }

        // 只有完成前置任務，才能進入首次交配。
        if (missionController.currentMissionStep !=
            MissionController.MissionStep.ReadyForExperiment)
        {
            Debug.Log("請先完成任務準備並取得實驗紀錄板。");
            ClosePlantingActions();
            return;
        }

        if (parentSelectionPanel == null)
        {
            Debug.LogWarning("尚未指定親代選擇面板。");
            ClosePlantingActions();
            return;
        }

        selectedPlantingSpot = spot;

        // 保持場景輸入與移動鎖定，直接切到親代面板。
        SetState(GameState.ParentSelection);

        parentSelectionPanel.Open(
            OnParentsConfirmed,
            CancelParentSelection
        );
    }

    private void CancelParentSelection()
    {
        selectedPlantingSpot = null;
        selectedFirstParent = null;
        selectedSecondParent = null;

        ReturnToExploring();
    }

    private void OnParentsConfirmed(
    PeaPlant first,
    PeaPlant second)
    {
        if (isBreedingSequenceRunning)
        {
            return;
        }

        if (selectedPlantingSpot == null ||
            !selectedPlantingSpot.CanUse() ||
            first == null ||
            second == null ||
            first == second ||
            !first.canBreed ||
            !second.canBreed)
        {
            Debug.LogWarning("交配資料已失效，請重新選擇。");
            CancelParentSelection();
            return;
        }

        if (first.interactionPoint == null ||
            second.interactionPoint == null)
        {
            Debug.LogWarning("請指定兩株親代的 Interaction Point。");
            CancelParentSelection();
            return;
        }

        if (playerMove == null ||
            playerMove.agent == null ||
            !playerMove.agent.enabled ||
            !playerMove.agent.isOnNavMesh)
        {
            Debug.LogWarning("玩家目前無法使用 NavMesh 移動。");
            CancelParentSelection();
            return;
        }

        selectedFirstParent = first;
        selectedSecondParent = second;

        // 使用實際掛 Agent、正在移動的物件位置。
        breedingReturnPosition =
            playerMove.agent.transform.position;

        selectedPlantingSpot.isReserved = true;
        isBreedingSequenceRunning = true;

        // 先清除原本的探索移動，再交給系統。
        playerMove.StopMoving();
        SetState(GameState.BreedingSequence);

        StartCoroutine(PlayBreedingSequence());
    }

    // ==========================================
    // 交配演出流程
    // ==========================================

    private IEnumerator PlayBreedingSequence()
    {
        // 第一株親代。
        yield return MoveForBreeding(
            selectedFirstParent.interactionPoint.position,
            selectedFirstParent.displayName
        );

        if (!systemMoveSucceeded)
        {
            FinishBreedingMovement(false);
            yield break;
        }

        yield return new WaitForSeconds(parentVisitWait);

        // 第二株親代。
        yield return MoveForBreeding(
            selectedSecondParent.interactionPoint.position,
            selectedSecondParent.displayName
        );

        if (!systemMoveSucceeded)
        {
            FinishBreedingMovement(false);
            yield break;
        }

        yield return new WaitForSeconds(parentVisitWait);

        // 返回開始交配前的位置。
        yield return MoveForBreeding(
            breedingReturnPosition,
            "原本位置"
        );

        FinishBreedingMovement(systemMoveSucceeded);
    }

    // ==========================================
    // 單段系統移動與抵達檢查
    // ==========================================

    private IEnumerator MoveForBreeding(
        Vector3 destination,
        string targetName)
    {
        systemMoveSucceeded = false;

        NavMeshAgent agent = playerMove.agent;

        // 忽略定位點的 Y，沿用玩家目前高度。
        destination.y = agent.transform.position.y;

        float arrivalDistance =
            agent.stoppingDistance +
            playerMove.arrivalTolerance;

        // 已經站在目標附近，不必重新尋路。
        if (PlanarDistance(agent.transform.position, destination)
            <= arrivalDistance)
        {
            playerMove.StopMoving();
            systemMoveSucceeded = true;
            yield break;
        }

        if (!playerMove.TrySetSystemTargetPosition(destination))
        {
            Debug.LogWarning($"系統無法完整抵達：{targetName}");
            yield break;
        }

        Debug.Log($"系統移動開始：{targetName}");

        float startedAt = Time.time;

        // 至少等待一幀，讓移動狀態更新。
        yield return null;

        while (Time.time - startedAt < systemMoveTimeout)
        {
            if (!agent.enabled ||
                !agent.isOnNavMesh ||
                currentState != GameState.BreedingSequence)
            {
                yield break;
            }

            if (!agent.pathPending)
            {
                bool nearTarget =
                    PlanarDistance(
                        agent.transform.position,
                        destination
                    ) <= arrivalDistance;

                // Move.Update() 可能已在抵達時清除路徑，
                // 因此同時接受「靠近目標且路徑已清除」。
                bool routeFinished =
                    !agent.hasPath ||
                    (
                        !float.IsInfinity(agent.remainingDistance) &&
                        agent.remainingDistance <= arrivalDistance
                    );

                if (nearTarget && routeFinished)
                {
                    playerMove.StopMoving();

                    systemMoveSucceeded = true;
                    Debug.Log($"系統已抵達：{targetName}");

                    yield break;
                }

                if (!agent.hasPath ||
                    agent.pathStatus != NavMeshPathStatus.PathComplete)
                {
                    Debug.LogWarning($"系統移動路徑中斷：{targetName}");
                    yield break;
                }
            }

            yield return null;
        }

        Debug.LogWarning($"系統移動超時：{targetName}");
    }

    // ==========================================
    // 結束本階段演出
    // ==========================================

    private void FinishBreedingMovement(bool succeeded)
    {
        playerMove.StopMoving();

        if (selectedPlantingSpot != null)
        {
            // 交配演出結束，解除種植區預留。
            selectedPlantingSpot.isReserved = false;
        }

        isBreedingSequenceRunning = false;

        // 演出中止時，不發放種子。
        if (!succeeded)
        {
            Debug.LogWarning(
                "交配走位中止：未取得種子，已恢復玩家操作。"
            );

            ReturnToExploring();
            return;
        }

        // 成功返回後，取得一顆種子。
        firstGenerationSeedCount++;

        Debug.Log(
            $"交配成功｜獲得種子 ×1｜" +
            $"目前持有：{firstGenerationSeedCount}"
        );

        if (selectedPlantingSpot == null ||
            plantingActionPanel == null)
        {
            Debug.LogWarning(
                "已取得種子，但種植區或操作選單不存在，已恢復探索。"
            );

            ReturnToExploring();
            return;
        }

        // 切換至操作選單狀態，保持玩家停止移動。
        SetState(GameState.PlantingActions);

        // 重新開啟剛剛選定種植區的操作選單。
        plantingActionPanel.Open(
            selectedPlantingSpot,
            OnCrossbreedSelected,
            ClosePlantingActions,
            firstGenerationSeedCount,
            $"交配成功！獲得種子 ×1\n" +
            $"目前持有種子：{firstGenerationSeedCount} 顆\n" +
            "可以選擇播種。"
        );
    }

    // ==========================================
    // 只計算 X、Z 距離
    // ==========================================

    private float PlanarDistance(Vector3 first, Vector3 second)
    {
        Vector3 difference = first - second;
        difference.y = 0f;

        return difference.magnitude;
    }
}