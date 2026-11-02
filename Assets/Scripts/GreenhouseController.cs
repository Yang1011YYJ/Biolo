using TMPro; // 引入 TextMeshPro，讓程式可以修改 UI 文字。
using UnityEngine; // 引入 Unity 基本功能。

public class GreenhouseController : MonoBehaviour
{
    [Header("主程式")]

    public GameManager gameManager; // 整個教材的主流程控制程式。


    [Header("豌豆牌子")]

    [Tooltip("高莖親代旁邊的3D牌子")]
    public GameObject tallPeaSign; // 高莖豌豆所對應的3D牌子。

    [Tooltip("矮莖親代旁邊的3D牌子")]
    public GameObject shortPeaSign; // 矮莖豌豆所對應的3D牌子。


    [Header("牌子互動設定")]

    public float interactionDistance = 2.5f; // 玩家距離牌子多近時才可以查看。


    [Header("牌子資訊介面")]

    public GameObject signPanel; // 共用的牌子資訊Panel。

    public TMP_Text signTitleText; // 顯示牌子標題的文字。

    public TMP_Text signContentText; // 顯示牌子詳細資料的文字。


    private void Start()
    {
        // 遊戲開始時先把牌子資訊Panel關掉。
        signPanel.SetActive(false);
    }


    // PlayerClick 點到溫室中的物件後，
    // GameManager會呼叫這個函式詢問：
    // 「剛才點到的東西是不是可以查看的牌子？」
    public bool TryOpenSign(GameObject clickedObject, Transform player)
    {
        // 計算玩家與剛才點擊物件之間的距離。
        float distance = Vector3.Distance(
            player.position, // 玩家目前的位置。
            clickedObject.transform.position // 被點擊物件的位置。
        );


        // 如果玩家距離太遠。
        if (distance > interactionDistance)
        {
            // 顯示測試訊息。
            Debug.Log("距離牌子太遠。");

            // 回傳false，表示這次沒有成功開啟牌子。
            return false;
        }


        // 如果玩家點到的是高莖豌豆的牌子。
        if (clickedObject == tallPeaSign)
        {
            // 顯示高莖親代資料。
            ShowTallPeaSign();

            // 回傳true，表示成功開啟牌子。
            return true;
        }


        // 如果玩家點到的是矮莖豌豆的牌子。
        if (clickedObject == shortPeaSign)
        {
            // 顯示矮莖親代資料。
            ShowShortPeaSign();

            // 回傳true，表示成功開啟牌子。
            return true;
        }


        // 如果點到的是溫室中的其他物件，
        // 就表示這次沒有牌子可以開啟。
        return false;
    }


    // 顯示高莖豌豆牌子內容。
    private void ShowTallPeaSign()
    {
        // 設定牌子標題。
        signTitleText.text = "親代甲";

        // 設定牌子的詳細資料。
        signContentText.text =
            "品系：純品系\n" +
            "性狀：高莖";

        // 顯示共用牌子Panel。
        signPanel.SetActive(true);
    }


    // 顯示矮莖豌豆牌子內容。
    private void ShowShortPeaSign()
    {
        // 設定牌子標題。
        signTitleText.text = "親代乙";

        // 設定牌子的詳細資料。
        signContentText.text =
            "品系：純品系\n" +
            "性狀：矮莖";

        // 顯示共用牌子Panel。
        signPanel.SetActive(true);
    }


    // 玩家按下牌子資訊的關閉按鈕時呼叫。
    public void CloseSign()
    {
        // 關閉牌子資訊Panel。
        signPanel.SetActive(false);

        // 通知GameManager回到自由探索狀態。
        gameManager.ReturnToExploring();
    }
}