using UnityEngine; // 引入 Unity 基本功能。
using UnityEngine.InputSystem; // 引入新版 Input System。
using UnityEngine.EventSystems; // 用來判斷滑鼠是不是點在 UI 上。

public class PlayerClick : MonoBehaviour
{
    [Header("主程式")]

    public GameManager gameManager; // 整個軟體流程的主程式。


    [Header("攝影機")]

    public Camera mainCamera; // 用來發射 Raycast 的攝影機。


    [Header("Layer")]

    public LayerMask groundLayer; // 可以移動的地板。

    public LayerMask interactableLayer; // 可以互動的場景物件。

    [Header("腳本")]
    public GreenhouseController greenhouseController;


    private void Start()
    {
        // 如果 Inspector 沒有指定 Camera。
        if (mainCamera == null)
        {
            // 自動尋找 MainCamera。
            mainCamera = Camera.main;
        }
    }


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


        // 如果這一幀沒有按下左鍵。
        if (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }


        // 如果滑鼠目前點在 UI 上。
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }


        // 執行場景點擊判斷。
        CheckWorldClick();
    }


    private void CheckWorldClick()
    {
        // 取得滑鼠目前在螢幕上的位置。
        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        // 從 Camera 穿過滑鼠位置發射射線。
        Ray ray =
            mainCamera.ScreenPointToRay(mousePosition);

        // 儲存射線碰撞資料。
        RaycastHit hit;


        // =====================================
        // 第一優先：互動物件
        // =====================================

        if (Physics.Raycast(
            ray,
            out hit,
            Mathf.Infinity,
            interactableLayer))
        {
            // 嘗試尋找這個物件或它父物件上的 ComputerInteractable。
            ComputerInteractable computer =
                hit.collider.GetComponentInParent<ComputerInteractable>();


            // 如果真的點到電腦。
            if (computer != null)
            {
                // 告訴 GameManager 玩家點到了電腦。
                gameManager.TryOpenComputer(computer);

                // 互動完成，不再判斷地板。
                return;
            }

            // 嘗試從被點擊物件的父物件中，
            // 找到GreenhouseController。
            GreenhouseController greenhouse =
                hit.collider.GetComponentInParent<GreenhouseController>();


            // 確認溫室控制腳本是否有指定。
            if (greenhouseController != null)
            {
                // 將點擊的物件交給溫室控制腳本判斷。
                gameManager.TryInteractGreenhouse(
                    greenhouseController,
                    hit.collider.gameObject
                );

                return;
            }
        }


        // =====================================
        // 第二優先：地面
        // =====================================

        if (Physics.Raycast(
            ray,
            out hit,
            Mathf.Infinity,
            groundLayer))
        {
            // 告訴 GameManager 玩家想移動到這個地方。
            gameManager.MovePlayerTo(hit.point);
        }
    }
}