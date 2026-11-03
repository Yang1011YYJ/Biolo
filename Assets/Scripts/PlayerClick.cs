using UnityEngine; // 引入 Unity 基本功能。
using UnityEngine.InputSystem; // 引入新版 Input System。
using UnityEngine.EventSystems; // 用來判斷滑鼠是不是點在 UI 上。

public class PlayerClick : MonoBehaviour
{
    // ==========================================
    // 主程式
    // ==========================================

    [Header("主程式")]

    public GameManager gameManager;
    // 整個軟體流程的主程式。


    // ==========================================
    // 攝影機
    // ==========================================

    [Header("攝影機")]

    public Camera mainCamera;
    // 用來發射 Raycast 的攝影機。


    // ==========================================
    // Layer
    // ==========================================

    [Header("Layer")]

    public LayerMask groundLayer;
    // 可以移動的地板。

    public LayerMask interactableLayer;
    // 可以互動的場景物件。


    // ==========================================
    // 腳本
    // ==========================================

    [Header("腳本")]

    public GreenhouseController greenhouseController;
    // 負責處理溫室牌子等互動。


    // ==========================================
    // 初始化
    // ==========================================

    private void Start()
    {
        // 如果 Inspector 沒有指定 Camera。
        if (mainCamera == null)
        {
            // 自動尋找 MainCamera。
            mainCamera = Camera.main;
        }
    }


    // ==========================================
    // 每幀偵測玩家輸入
    // ==========================================

    private void Update()
    {
        // 詢問 GameManager：
        // 現在是不是允許玩家點擊場景。
        if (!gameManager.CanUseWorldInput())
        {
            return;
        }


        // 如果目前沒有滑鼠。
        if (Mouse.current == null)
        {
            return;
        }


        // 如果這一幀沒有按下滑鼠左鍵。
        if (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }


        // 如果目前的滑鼠位置是在 UI 上。
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            // 不執行場景點擊。
            return;
        }


        // 執行場景點擊判斷。
        CheckWorldClick();
    }


    // ==========================================
    // 判斷玩家點擊到哪一種場景物件
    // ==========================================

    private void CheckWorldClick()
    {
        // 取得滑鼠目前在螢幕上的位置。
        Vector2 mousePosition =
            Mouse.current.position.ReadValue();


        // 從 Camera 穿過滑鼠位置發射射線。
        Ray ray =
            mainCamera.ScreenPointToRay(mousePosition);


        // 儲存 Raycast 擊中的物件資料。
        RaycastHit hit;


        // ==========================================
        // 第一優先：互動物件
        // ==========================================

        if (Physics.Raycast(
            ray,
            out hit,
            Mathf.Infinity,
            interactableLayer))
        {
            // 顯示這次 Raycast 真正點到哪個物件。
            Debug.Log(
                "目前點到的互動物件：" + hit.collider.gameObject.name
            );

            // ======================================
            // 實驗紀錄板互動
            // ======================================

            // 從玩家點擊到的 Collider 開始，
            // 往父物件尋找 ExperimentClipboardInteractable。
            ExperimentClipboardInteractable clipboard =
                hit.collider.GetComponentInParent<
                    ExperimentClipboardInteractable
                >();


            // 如果這次點擊的是實驗紀錄板。
            if (clipboard != null)
            {
                // PlayerClick 本身就掛在 Player 身上，
                // 因此 transform 就是玩家自己的 Transform。
                clipboard.TryPickup(
                    transform
                );


                // 這次點擊已經交給紀錄板處理，
                // 不再繼續判斷電腦或其他互動。
                return;
            }


            // ======================================
            // 電腦互動
            // ======================================

            // 從被點擊的 Collider 開始，
            // 往父物件尋找 ComputerInteractable。
            ComputerInteractable computer =
                hit.collider.GetComponentInParent<
                    ComputerInteractable
                >();


            // 如果真的點到電腦。
            if (computer != null)
            {
                // 告訴 GameManager：
                // 玩家想要操作這台電腦。
                gameManager.TryOpenComputer(
                    computer
                );


                // 電腦互動完成，
                // 不再判斷其他物件。
                return;
            }


            // ======================================
            // 溫室互動
            // ======================================

            // 如果有指定 GreenhouseController。
            if (greenhouseController != null)
            {
                // 將這次點擊到的場景物件
                // 交給 GameManager 與 GreenhouseController 判斷。
                gameManager.TryInteractGreenhouse(
                    greenhouseController,
                    hit.collider.gameObject
                );


                // 溫室互動判斷完成，
                // 不再繼續判斷地面。
                return;
            }
        }


        // ==========================================
        // 第二優先：地面
        // ==========================================

        if (Physics.Raycast(
            ray,
            out hit,
            Mathf.Infinity,
            groundLayer))
        {
            // 告訴 GameManager：
            // 玩家想移動到 Raycast 擊中的地面位置。
            gameManager.MovePlayerTo(
                hit.point
            );
        }
    }
}