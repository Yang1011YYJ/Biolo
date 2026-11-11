using TMPro;
using UnityEngine;
using UnityEngine.UI; // 引入 Unity 基本功能。
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;

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
        BreedingSequence, // 系統控制玩家進行交配演出。
        SeedSelection // 玩家正在選擇要播種的種子。
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
    public GameObject StartPanel;
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

    //[Tooltip("目前持有的第一子代種子數量。純品系高莖與矮莖交配成功後增加。")]
    //[Min(0)]
    //public int firstGenerationSeedCount = 0;
    [Header("種子與播種")]

    [Tooltip("目前持有的種子。每次交配新增一筆，播種時移除選中的那筆。")]
    public List<SeedData> ownedSeeds = new List<SeedData>();

    [Tooltip("顯示種子清單與確認播種按鈕的面板控制器。")]
    public SeedSelectionPanel seedSelectionPanel;

    [Tooltip("目前甲、乙交配所得 F1 的高莖植株 Prefab。")]
    public GameObject firstGenerationPlantPrefab;
    [Tooltip("第二次交配產生矮莖子代時使用的植株 Prefab，根物件須掛上 PeaPlant。")]
    public GameObject shortOffspringPlantPrefab;

    // 保留原本讀取數量的名稱，既有選單呼叫不用全部修改。
    public int firstGenerationSeedCount => ownedSeeds.Count;
    [Tooltip("下一顆種子的顯示編號。播種消耗種子後不會倒退，避免名稱重複。")]
    [SerializeField, Min(1)]
    private int nextSeedNumber = 1;

    [Header("可交配植株")]

    [Tooltip("目前可供選擇的植株。開場指定甲、乙，播種後由程式加入子代。")]
    public List<PeaPlant> availableParents = new List<PeaPlant>();

    [Tooltip("開場的矮莖親代乙，用來驗證第二次交配組合。")]
    public PeaPlant shortOriginalParent;
    [Header("植株生成高度")]

    [Tooltip("高莖植株生成時使用的世界座標 Y，不受生成定位點的 Y 影響。")]
    public float tallPlantWorldY= 31.87150001525879f;

    [Tooltip("矮莖植株生成時使用的世界座標 Y，不受生成定位點的 Y 影響。")]
    public float shortPlantWorldY= 30.60569953918457f;

    [Header("展示與快速測試")]

    [Tooltip("開始畫面的階段選單：0 正常開始、1 直接交配。未指定時走正常流程。")]
    public TMP_Dropdown startupStageDropdown;

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
    [Tooltip("畫面下方共用提示框，顯示操作結果、距離不足及獲得道具訊息。")]public SystemHintController systemHintController;
    [Tooltip("共用的溫室資訊控制器，用來顯示已種下子代的資料。")]public GreenhouseController greenhouseController;
    [Tooltip("播種成功後生成的資訊牌子 Prefab，需掛有 PeaInfoSign。")]
    public PeaInfoSign offspringSignPrefab;
    [Tooltip("場景中可拾取的實驗紀錄板。快速開始時會將它加入背包並隱藏。")]public ExperimentClipboardInteractable experimentClipboard;


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
        StartPanel.SetActive(false);

        //// 顯示目前 GameManager 指到的是哪一個物件。
        //Debug.LogError("電腦物件名稱：" + computerObject.name);

        //// 檢查電腦自己是否被啟用。
        //Debug.LogError("activeSelf：" + computerObject.activeSelf);

        //// 檢查電腦在整個 Hierarchy 中是否真的有效。
        //Debug.LogError("activeInHierarchy：" + computerObject.activeInHierarchy);
    }

    public void StartButton()
    {
        // 避免開始後重複啟動。
        if (currentState != GameState.PreStart)
        {
            return;
        }

        int selectedStage = startupStageDropdown != null
            ? startupStageDropdown.value
            : 0;

        // 快速測試：不等待黑幕與開場對話。
        if (selectedStage == 1)
        {
            StartFromTestStage();
            return;
        }

        // 正常開始時隱藏選單。
        if (startupStageDropdown != null)
        {
            startupStageDropdown.gameObject.SetActive(false);
        }
        //Debug.LogError("★★★ 現在執行的是 GameManager.StartButton ★★★");

        if (currentState == GameState.PreStart)
        {
            // 淡入開始就離開 PreStart，避免重複點擊啟動。
            SetState(GameState.Intro);

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
    // 快速開始：交配測試
    // ==========================================

    private void StartFromTestStage()
    {
        // ======================================
        // 先確認快速開始需要的元件
        // ======================================

        if (missionController == null ||
            missionController.researchRecordController == null ||
            greenhouseController == null ||
            experimentClipboard == null)
        {
            Debug.LogError(
                "快速開始設定不完整：請確認任務、研究紀錄、" +
                "溫室控制器與場景紀錄板。"
            );

            return;
        }

        // 確認能取得紀錄板，再開始切換流程。
        if (!experimentClipboard.GrantForQuickStart())
        {
            return;
        }

        // ======================================
        // 補齊假設與正式紀錄
        // ======================================

        ResearchRecordController record =
            missionController.researchRecordController;

        record.SaveHypothesis(
            ResearchRecordController.HypothesisType.ShortStemDominant
        );

        record.RecordHypothesis();

        // ======================================
        // 補齊任務與觀察狀態
        // ======================================

        missionController.missionAccepted = true;

        greenhouseController.tallPeaViewed = true;
        greenhouseController.shortPeaViewed = true;

        missionController.currentMissionStep =
            MissionController.MissionStep.ReadyForExperiment;

        // 同步任務接受介面。
        if (missionController.AcceptMissionState != null)
        {
            missionController.AcceptMissionState.text = "已接受";

            missionController.AcceptMissionState.color =
                new Color(0f, 0.84f, 0f, 1f);
        }

        if (missionController.acceptMissionButton != null)
        {
            missionController.acceptMissionButton.interactable = false;
        }

        // ======================================
        // 顯示場景、關閉開始介面
        // ======================================

        StartGame();

        BlackPanel.SetActive(false);
        computerPanel.SetActive(false);
        BackpackPanel.SetActive(false);

        if (startupStageDropdown != null)
        {
            startupStageDropdown.gameObject.SetActive(false);
        }

        // ======================================
        // 進入交配前的自由探索
        // ======================================

        ReturnToExploring();

        missionController.ShowCurrentMissionHint();

        Debug.Log(
            "快速開始完成｜任務已接受｜親代皆已觀察｜" +
            $"假設：{record.GetHypothesisDescription()}｜" +
            $"已記錄：{record.IsHypothesisRecorded()}｜" +
            "紀錄板已加入背包｜ReadyForExperiment"
        );
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

            ShowSystemHint("距離電腦太遠，請再靠近一點");

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
        if (currentState != GameState.Exploring ||
            spot == null)
        {
            return;
        }

        if (!spot.IsPlayerInRange(player))
        {
            ShowSystemHint("距離種植區太遠，請再靠近一點");
            return;
        }

        // ======================================
        // 有植物：嘗試查看子代資料
        // ======================================

        if (spot.isOccupied)
        {
            ShowSystemHint("這裡已經有植物，請查看旁邊的資訊牌");
            return;
        }

        // ======================================
        // 正在使用中
        // ======================================

        if (spot.isReserved)
        {
            ShowSystemHint("這個種植區正在使用中");
            return;
        }

        // ======================================
        // 空種植區：開啟操作選單
        // ======================================

        if (plantingActionPanel == null)
        {
            Debug.LogWarning("尚未指定種植操作選單。");
            return;
        }

        selectedPlantingSpot = spot;

        SetState(GameState.PlantingActions);

        plantingActionPanel.Open(
            spot,
            OnCrossbreedSelected,
            ClosePlantingActions,
            firstGenerationSeedCount,
            sowCallback: OnSowSelected
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

        bool canStartCross =
    missionController.currentMissionStep ==
        MissionController.MissionStep.ReadyForExperiment ||
    missionController.currentMissionStep ==
        MissionController.MissionStep.SelfCrossF1;

        if (!canStartCross)
        {
            ShowSystemHint("請先完成目前的任務");
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
            availableParents,
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
            !first.canBreed ||
            !second.canBreed)
        {
            Debug.LogWarning("交配資料已失效，請重新選擇。");
            CancelParentSelection();
            return;
        }
        // 暫時沿用既有任務列舉名稱，這個階段現在改為要求 F1 自交。
        // 後續整理任務文字時，再一起調整名稱。
        if (missionController.currentMissionStep ==
            MissionController.MissionStep.SelfCrossF1)
        {
            // 同一株植物必須同時擔任兩個交配方。
            // 來源也必須是原本親代交配產生的第一子代。
            bool validSelfCross =
                first == second &&
                first.sourceSeed != null &&
                first.sourceSeed.generation == 1 &&
                first.sourceSeed.crossType ==
                    SeedData.CrossType.OriginalParents;

            // 選錯組合時，讓玩家重新選擇。
            if (!validSelfCross)
            {
                // 說明自交在這個面板中的操作方式。
                ShowSystemHint("這次請連續選擇同一株第一子代，讓它自花授粉");

                // 重新開啟面板，也會清空剛才的選擇。
                parentSelectionPanel.Open(
                    availableParents,
                    OnParentsConfirmed,
                    CancelParentSelection
                );

                // 不進入自動走位。
                return;
            }

            // 組合符合，繼續執行下方原本的走位流程。
        }
        else if (first == second)
        {
            // 其他階段暫時維持選擇兩株不同植物的規則。
            ShowSystemHint("這個階段請選擇兩株不同的親代");

            // 重新開啟選擇面板。
            parentSelectionPanel.Open(
                availableParents,
                OnParentsConfirmed,
                CancelParentSelection
            );

            // 不執行後續交配。
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

        //FacePeaPlant(selectedFirstParent.interactionPoint);

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

        //FacePeaPlant(selectedSecondParent.interactionPoint);

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
        string seedId = System.Guid.NewGuid().ToString("N");

        // 判斷這次是否為第一子代與矮莖親代乙交配。
        // 使用實際選取的植物資料判斷，保留原本的選取順序。
        // 判斷是否為同一株第一子代自交。
        bool isF1SelfCross =
            selectedFirstParent == selectedSecondParent &&
            selectedFirstParent.sourceSeed != null &&
            selectedFirstParent.sourceSeed.generation == 1 &&
            selectedFirstParent.sourceSeed.crossType ==
                SeedData.CrossType.OriginalParents;

        // 判斷是否為第一子代與原本矮莖親代交配。
        // 先保留這種分類，供之後的延伸實驗使用。
        bool isBackcross =
            (
                selectedFirstParent == shortOriginalParent &&
                selectedSecondParent.sourceSeed != null &&
                selectedSecondParent.sourceSeed.generation == 1
            )
            ||
            (
                selectedSecondParent == shortOriginalParent &&
                selectedFirstParent.sourceSeed != null &&
                selectedFirstParent.sourceSeed.generation == 1
            );

        // 原本親代交配為預設來源。
        SeedData.CrossType crossType =
            SeedData.CrossType.OriginalParents;

        // 優先辨識 F1 自交。
        if (isF1SelfCross)
        {
            crossType = SeedData.CrossType.F1SelfCross;
        }
        else if (isBackcross)
        {
            crossType = SeedData.CrossType.F1WithShortParent;
        }

        // 建立種子，保留原本的編號和親代來源。
        SeedData newSeed = new SeedData
        {
            seedId = seedId, // 保存唯一識別碼。
            displayName = $"豌豆種子 {nextSeedNumber:D2}", // 保留原本種子命名。
            firstParentName = selectedFirstParent.displayName, // 第一個交配方。
            secondParentName = selectedSecondParent.displayName, // 第二個交配方。
            generation = crossType == SeedData.CrossType.OriginalParents ? 1 : 2,
            crossType = crossType // 保存交配類型。
        };

        // 在取得種子時決定性狀，播種時不會重新抽取。
        switch (crossType)
        {
            case SeedData.CrossType.F1SelfCross:
                // F1 自交：每顆種子的高莖機率為 75%。
                newSeed.isTall = UnityEngine.Random.value < 0.75f;
                break;

            case SeedData.CrossType.F1WithShortParent:
                // 保留原本回交的高、矮莖機率設定。
                newSeed.isTall = UnityEngine.Random.value < 0.5f;
                break;

            default:
                // 原本兩株純品系親代交配，F1 固定為高莖。
                newSeed.isTall = true;
                break;
        }

        // 確認這次建立的種子分類。
        Debug.Log(
            $"種子建立｜交配類型：{newSeed.crossType}｜" +
            $"世代：{newSeed.generation}｜" +
            $"來源：{newSeed.firstParentName} × {newSeed.secondParentName}"
        );

        ownedSeeds.Add(newSeed);

        // 確認種子已加入持有清單，才通知任務系統自交完成。
        if (newSeed.crossType == SeedData.CrossType.F1SelfCross)
        {
            missionController.OnF1SelfCrossCompleted();
        }

        // 新增成功後，準備下一個編號。
        nextSeedNumber++;
        ShowSystemHint("交配成功！獲得種子 ×1");

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
            sowCallback: OnSowSelected
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

    public void ShowSystemHint(string message, float duration = -1f)
    {
        if (systemHintController == null)
        {
            Debug.LogWarning($"尚未指定系統提示框｜{message}");
            return;
        }

        systemHintController.Show(message, duration);
    }

    private void OnSowSelected(PlantingSpot spot)
    {
        if (spot == null || !spot.CanUse())
        {
            ShowSystemHint("這個種植區目前無法使用");
            ClosePlantingActions();
            return;
        }

        if (ownedSeeds.Count == 0)
        {
            ShowSystemHint("尚無可播種的種子");
            ClosePlantingActions();
            return;
        }

        if (seedSelectionPanel == null)
        {
            Debug.LogWarning("尚未指定種子選擇面板。");
            ClosePlantingActions();
            return;
        }

        selectedPlantingSpot = spot;

        SetState(GameState.SeedSelection);

        seedSelectionPanel.Open(
            ownedSeeds,
            spot.spotName,
            OnSeedConfirmed,
            ClosePlantingActions
        );
    }

    private void OnSeedConfirmed(SeedData seed)
    {
        PlantingSpot spot = selectedPlantingSpot;

        // 再次驗證，避免重複播種或使用已不存在的種子。
        if (currentState != GameState.SeedSelection ||
            spot == null ||
            !spot.CanUse() ||
            seed == null ||
            !ownedSeeds.Contains(seed))
        {
            ShowSystemHint("無法播種，請重新選擇種子與位置");
            ClosePlantingActions();
            return;
        }

        // 確認種子屬於目前支援的交配來源。
        // 接受原本親代交配、回交，以及這次新增的 F1 自交種子。
        if (seed.crossType != SeedData.CrossType.OriginalParents &&
            seed.crossType != SeedData.CrossType.F1WithShortParent &&
            seed.crossType != SeedData.CrossType.F1SelfCross)
        {
            // 未支援的來源不消耗種子。
            ShowSystemHint("目前尚未開放這種種子的播種");
            ClosePlantingActions();
            return;
        }

        // 判斷是否為原本親代交配產生的第一子代。
        bool isFirstOffspring =
            seed.crossType == SeedData.CrossType.OriginalParents;

        // 第一子代固定使用高莖；第二次交配讀取種子保存的結果。
        bool isTall = isFirstOffspring || seed.isTall;

        // 取得這次要顯示的性狀文字。
        string traitText = isTall ? "高莖" : "矮莖";

        // 取得這次植株與牌子使用的名稱。
        // 依種子的交配來源決定植株與牌子的標題。
        string offspringTitle;

        // 原本親代交配產生第一子代。
        if (isFirstOffspring)
        {
            offspringTitle = "第一子代（F1）";
        }
        else if (seed.crossType == SeedData.CrossType.F1SelfCross)
        {
            // 第一子代自交產生第二子代。
            offspringTitle = "第二子代（F2）";
        }
        else
        {
            // 保留回交子代的獨立名稱。
            offspringTitle = "回交子代";
        }

        // 依性狀選擇要生成的模型。
        GameObject plantPrefab = isTall
            ? firstGenerationPlantPrefab
            : shortOffspringPlantPrefab;

        // 生成前確認模型和植株元件都已設定。
        if (plantPrefab == null ||
            plantPrefab.GetComponent<PeaPlant>() == null)
        {
            // 顯示缺少設定的性狀，方便回到 Inspector 檢查。
            Debug.LogWarning(
                $"{traitText}植株 Prefab 未指定，或根物件尚未掛上 PeaPlant。"
            );

            // 保留種子，讓玩家可以在修正設定後重新播種。
            ShowSystemHint("目前無法播種，種子已保留");
            ClosePlantingActions();
            return;
        }

        //Transform spawnPoint = spot.offspringSpawnPoint;
        // 從這次選定種植區的直接子物件取得交配走位點。
        Transform offspringInteractionPoint =
            spot.transform.Find("Interaction Point");

        if (offspringInteractionPoint == null)
        {
            Debug.LogWarning(
                $"種植區「{spot.name}」底下找不到 Interaction Point。",
                spot
            );

            ShowSystemHint("目前無法播種，種子已保留");
            ClosePlantingActions();
            return;
        }

        // 牌子也先確認設定，避免植物生成後才因缺少牌子設定中斷。
        if (spot.signSpawnPoint == null ||
            offspringSignPrefab == null)
        {
            Debug.LogWarning("尚未指定牌子生成點或牌子 Prefab。");

            ShowSystemHint("目前無法播種，種子已保留");
            ClosePlantingActions();
            return;
        }
        // X、Z 使用所選種植區的位置；Y 使用各性狀設定的世界高度。
        Vector3 plantPosition = new Vector3(
            spot.transform.position.x,
            isTall ? tallPlantWorldY : shortPlantWorldY,
            spot.transform.position.z
        );

        // 先在世界生成，使用對應 Prefab 的原始旋轉與大小。
        GameObject plant = Instantiate(
            plantPrefab,
            plantPosition,
            plantPrefab.transform.rotation
        );

        // 再歸到種植區底下，保留世界位置、旋轉與大小。
        plant.transform.SetParent(spot.transform, true);
        PeaPlant offspring = plant.GetComponent<PeaPlant>();
        // 使用所屬種植區的走位點，不使用 Prefab 預先指定的位置。
        offspring.interactionPoint = offspringInteractionPoint;

        // 保存這株植物的種子來源。
        offspring.sourceSeed = seed;

        // 名稱包含子代分類和種子編號，方便交配選單辨識。
        offspring.displayName = $"{offspringTitle}";

        // 顯示這株植物實際長出的性狀。
        offspring.description = $"性狀：{traitText}";

        // 保留原本允許選作交配對象的設定。
        offspring.canBreed = true;

        if (!availableParents.Contains(offspring))
        {
            availableParents.Add(offspring);
        }

        // Hierarchy 名稱包含子代分類、性狀。
        plant.name = $"{offspringTitle}_{traitText}";

        // 保存此種植區使用的種子來源。
        spot.plantedSeed = seed;
        spot.plantedPlant = plant;
        // 牌子使用獨立定位點，不受植物的位置與高度影響。
        PeaInfoSign sign = Instantiate(
            offspringSignPrefab,
            spot.signSpawnPoint.position,
            spot.signSpawnPoint.rotation
        );

        // 保留世界位置、旋轉與大小，再歸到種植區底下。
        sign.transform.SetParent(spot.transform, true);

        // 設定牌子資料。
        sign.seed = seed;
        sign.title = offspringTitle;
        sign.observedTrait = traitText;

        spot.plantedSign = sign;
        spot.isOccupied = true;

        // 成功生成後，才消耗指定的那顆種子。
        ownedSeeds.Remove(seed);

        Debug.Log(
            $"播種完成｜種植區：{spot.spotName}｜" +
            $"種子：{seed.seedId}｜" +
            $"來源：{seed.firstParentName} × {seed.secondParentName}"
        );

        // 播種完成後，提示這次長出的子代分類。
        ShowSystemHint($"播種完成！{offspringTitle}已長成");
        // 依交配來源通知任務，不讓不同來源的植株推進錯誤階段。
        if (seed.crossType == SeedData.CrossType.OriginalParents)
        {
            // 原本親代交配的種子已長成 F1。
            missionController.OnFirstOffspringPlanted();
        }
        else if (seed.crossType == SeedData.CrossType.F1SelfCross)
        {
            // F1 自交的種子已長成 F2。
            missionController.OnSecondOffspringPlanted();
        }

        ClosePlantingActions();
    }

    public void TryOpenOffspringSign(PeaInfoSign sign)
    {
        if (currentState != GameState.Exploring ||
            sign == null)
        {
            return;
        }

        if (!sign.IsPlayerInRange(player))
        {
            ShowSystemHint("距離牌子太遠，請再靠近一點");
            return;
        }

        if (greenhouseController == null)
        {
            Debug.LogWarning("尚未指定溫室資訊控制器。");
            return;
        }

        if (greenhouseController.OpenOffspringSign(sign))
        {
            SetState(GameState.SignInfo);
        }
    }

    public void OnBackpackClosed()
    {
        // 先隱藏背包面板，讓接下來的對話能正常顯示。
        BackpackPanel.SetActive(false);

        // 兩題都答對且正處於判讀階段時，由老師對話接手。
        if (missionController != null &&
            missionController.TryStartOffspringReviewDialogue())
        {
            return;
        }

        // 沒有對話要播放時，恢復玩家操作。
        ReturnToExploring();
    }

    // ==========================================
    // 依定位點的 +X 方向，設定玩家面向
    // ==========================================

    private void FacePeaPlant(Transform interactionPoint)
    {
        Transform movingPlayer = playerMove.agent.transform;

        // 保留玩家原本的 X、Z 旋轉，
        // 只將水平面向對齊定位空物件。
        Vector3 rotation = movingPlayer.eulerAngles;
        rotation.y = interactionPoint.eulerAngles.y;

        movingPlayer.rotation = Quaternion.Euler(rotation);
    }
}