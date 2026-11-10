using UnityEngine; // 引入 Unity 基本功能。
using UnityEngine.InputSystem; // 引入新版 Input System。
using UnityEngine.EventSystems; // 用來判斷滑鼠是否位於 UI 上。

public class PlayerClick : MonoBehaviour
{
    // ==========================================
    // 主程式
    // ==========================================

    [Header("主程式")]

    public GameManager gameManager;
    // 整個教材流程的 GameManager。


    // ==========================================
    // 攝影機
    // ==========================================

    [Header("攝影機")]

    public Camera mainCamera;
    // 從螢幕位置發射 Raycast 的攝影機。


    // ==========================================
    // Layer
    // ==========================================

    [Header("Layer")]

    public LayerMask groundLayer;
    // 玩家可以點擊移動的地面 Layer。


    public LayerMask interactableLayer;
    // 電腦、牌子、紀錄板等可互動物件 Layer。


    // ==========================================
    // 其他控制腳本
    // ==========================================

    [Header("腳本")]

    public GreenhouseController greenhouseController;
    // 溫室牌子等互動功能。


    // ==========================================
    // 長按移動
    // ==========================================

    [Header("長按移動")]

    public float holdMoveDelay = 0.25f;
    // 按住多久之後才開始判定為長按。
    //
    // 單純快速點一下不會進入持續更新模式。


    public float holdMoveUpdateInterval = 0.05f;
    // 長按期間多久更新一次目的地。
    //
    // 不需要每一幀都呼叫 SetDestination，
    // 可以降低不必要的 NavMesh 路徑更新。


    // ==========================================
    // 長按狀態
    // ==========================================

    private bool isPressing = false;
    // 玩家目前是否正在按住滑鼠左鍵。


    private bool canContinueHoldMove = false;
    // 這一次按下是否是從地面開始。
    //
    // 如果一開始按的是電腦、牌子或紀錄板，
    // 就禁止長按移動。


    private float pressStartTime = 0f;
    // 紀錄這一次開始按下的時間。


    private float nextHoldMoveTime = 0f;
    // 下一次允許更新長按移動目的地的時間。


    // ==========================================
    // 初始化
    // ==========================================

    private void Start()
    {
        // 如果 Inspector 沒有指定 Camera。
        if (mainCamera == null)
        {
            // 自動尋找 Main Camera。
            mainCamera = Camera.main;
        }
    }


    // ==========================================
    // 每幀偵測玩家輸入
    // ==========================================

    private void Update()
    {
        // ======================================
        // GameManager 不允許操作場景
        // ======================================

        if (!gameManager.CanUseWorldInput())
        {
            // 清除目前長按狀態。
            ResetPressState();

            return;
        }


        // ======================================
        // 確認目前有滑鼠
        // ======================================

        if (Mouse.current == null)
        {
            return;
        }


        // ======================================
        // 玩家剛按下左鍵
        // ======================================

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // 如果滑鼠目前位於 UI 上。
            if (IsPointerOverUI())
            {
                // 不處理場景輸入。
                ResetPressState();

                return;
            }


            // 標記目前開始按住。
            isPressing = true;


            // 紀錄按下時間。
            pressStartTime = Time.time;


            // 設定第一次長按更新的時間。
            nextHoldMoveTime =
                Time.time +
                holdMoveDelay;


            // 執行一次普通單點。
            //
            // 如果這次點到的是地面，
            // CheckWorldClick() 會回傳 true，
            // 代表之後可以進入長按移動。
            //
            // 如果點到互動物件，
            // 則回傳 false。
            canContinueHoldMove =
                CheckWorldClick();
        }


        // ======================================
        // 玩家正在持續按住左鍵
        // ======================================

        if (
            isPressing &&
            canContinueHoldMove &&
            Mouse.current.leftButton.isPressed
        )
        {
            // 計算目前已經按住多久。
            float holdDuration =
                Time.time -
                pressStartTime;


            // 只有超過長按判定時間後，
            // 才開始持續更新位置。
            if (
                holdDuration >= holdMoveDelay &&
                Time.time >= nextHoldMoveTime
            )
            {
                // 如果目前沒有按在 UI 上。
                if (!IsPointerOverUI())
                {
                    // 只更新「地面移動」。
                    //
                    // 長按期間不重新觸發：
                    // 電腦、
                    // 紀錄板、
                    // 溫室牌子。
                    UpdateHoldMovement();
                }


                // 安排下一次更新時間。
                nextHoldMoveTime =
                    Time.time +
                    holdMoveUpdateInterval;
            }
        }


        // ======================================
        // 玩家放開左鍵
        // ======================================

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // 清除長按狀態。
            //
            // 注意：
            // 這裡「不會直接 StopMoving」。
            //
            // 玩家仍然會走完最後一次指定的目的地，
            // 只是放開之後不再更新新的目的地。
            ResetPressState();
        }
    }


    // ==========================================
    // 處理一般單點
    // ==========================================

    private bool CheckWorldClick()
    {
        // 取得滑鼠目前在螢幕上的位置。
        Vector2 mousePosition =
            Mouse.current.position.ReadValue();


        // 從 Camera 穿過滑鼠位置發射射線。
        Ray ray =
            mainCamera.ScreenPointToRay(
                mousePosition
            );


        // 儲存 Raycast 擊中的資料。
        RaycastHit hit;


        // ======================================
        // 第一優先：場景互動物件
        // ======================================

        if (
            Physics.Raycast(
                ray,
                out hit,
                Mathf.Infinity,
                interactableLayer
            )
        )
        {
            // ==================================
            // 實驗紀錄板
            // ==================================

            ExperimentClipboardInteractable clipboard =
                hit.collider.GetComponentInParent<
                    ExperimentClipboardInteractable
                >();


            // 如果點到實驗紀錄板。
            if (clipboard != null)
            {
                // 嘗試拾取紀錄板。
                //
                // PlayerClick 掛在 Player 本身，
                // 所以 transform 就是玩家位置。
                clipboard.TryPickup(
                    transform
                );


                // 互動物件不啟用長按移動。
                return false;
            }


            // ==================================
            // 電腦
            // ==================================

            ComputerInteractable computer =
                hit.collider.GetComponentInParent<
                    ComputerInteractable
                >();


            // 如果點到電腦。
            if (computer != null)
            {
                // 通知 GameManager 打開電腦。
                gameManager.TryOpenComputer(
                    computer
                );


                // 電腦互動不能進入長按移動。
                return false;
            }
            // ==================================
            // 空種植區
            // ==================================

            PlantingSpot plantingSpot =
                hit.collider.GetComponentInParent<PlantingSpot>();

            if (plantingSpot != null)
            {
                gameManager.TryOpenPlantingSpot(plantingSpot);

                // 點擊種植區不啟用長按移動。
                return false;
            }

            // ==================================
            // 溫室互動
            // ==================================

            if (greenhouseController != null)
            {
                // 將點到的物件交給溫室控制器判斷。
                gameManager.TryInteractGreenhouse(
                    greenhouseController,
                    hit.collider.gameObject
                );


                // 場景互動物件不啟用長按移動。
                return false;
            }
        }


        // ======================================
        // 第二優先：地面
        // ======================================

        if (
            Physics.Raycast(
                ray,
                out hit,
                Mathf.Infinity,
                groundLayer
            )
        )
        {
            // 將這次點擊位置交給 GameManager。
            gameManager.MovePlayerTo(
                hit.point
            );


            // 回傳 true，
            // 代表這次是從地面開始按，
            // 因此可以繼續進入長按移動。
            return true;
        }


        // 沒有點到任何有效物件。
        return false;
    }


    // ==========================================
    // 長按期間更新地面目的地
    // ==========================================

    private void UpdateHoldMovement()
    {
        // 取得滑鼠目前位置。
        Vector2 mousePosition =
            Mouse.current.position.ReadValue();


        // 從 Camera 發射 Raycast。
        Ray ray =
            mainCamera.ScreenPointToRay(
                mousePosition
            );


        // 儲存地面碰撞資料。
        RaycastHit hit;


        // ======================================
        // 長按時只偵測地面
        // ======================================

        if (
            Physics.Raycast(
                ray,
                out hit,
                Mathf.Infinity,
                groundLayer
            )
        )
        {
            // 持續把目前滑鼠指向的位置
            // 更新成玩家的新目的地。
            gameManager.MovePlayerTo(
                hit.point
            );
        }
    }


    // ==========================================
    // 判斷滑鼠是否位於 UI
    // ==========================================

    private bool IsPointerOverUI()
    {
        // 如果目前根本沒有 EventSystem。
        if (EventSystem.current == null)
        {
            // 就不可能正在點 UI。
            return false;
        }


        // 回傳 EventSystem 的判斷結果。
        return EventSystem.current
            .IsPointerOverGameObject();
    }


    // ==========================================
    // 清除長按狀態
    // ==========================================

    private void ResetPressState()
    {
        // 玩家目前不再處於按住狀態。
        isPressing = false;


        // 禁止繼續長按更新。
        canContinueHoldMove = false;


        // 清除按下時間。
        pressStartTime = 0f;


        // 清除下一次更新時間。
        nextHoldMoveTime = 0f;
    }
}