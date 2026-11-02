using TMPro; // 引入 TextMeshPro，讓程式可以修改 UI 文字。
using UnityEngine; // 引入 Unity 基本功能。

public class MissionHintController : MonoBehaviour
{
    // ==========================================
    // 任務提示介面
    // ==========================================

    [Header("任務提示介面")]

    public GameObject missionHintPanel;
    // 顯示目前任務提示的整個 Panel。


    public TMP_Text missionHintText;
    // 顯示目前任務內容的文字。


    // ==========================================
    // 初始化
    // ==========================================

    private void Awake()
    {
        // 遊戲一開始先隱藏任務提示。
        missionHintPanel.SetActive(false);
    }


    // ==========================================
    // 顯示／更新任務提示
    // ==========================================

    public void ShowHint(string message)
    {
        // 將傳進來的任務文字顯示在畫面上。
        missionHintText.text = message;


        // 顯示任務提示 Panel。
        missionHintPanel.SetActive(true);
    }


    // ==========================================
    // 隱藏任務提示
    // ==========================================

    public void HideHint()
    {
        // 關閉任務提示 Panel。
        missionHintPanel.SetActive(false);
    }
}