using System; // 引入 Action，讓對話結束後可以通知其他程式。
using TMPro; // 引入 TextMeshPro，讓程式可以修改對話文字。
using UnityEngine; // 引入 Unity 基本功能。
using UnityEngine.UI; // 引入 Button 元件。

public class DialogueControll : MonoBehaviour
{
    // ==========================================
    // 對話介面
    // ==========================================

    [Header("對話介面")]

    public Button textPanelButton;
    // 整個對話框 Panel 本身的 Button。
    // 玩家在平板上點擊這個區域，就代表要繼續對話。

    public TMP_Text speakerNameText;
    // 顯示目前說話者名稱。

    public TMP_Text dialogueText;
    // 顯示目前這一句對話內容。


    // ==========================================
    // 對話資料
    // ==========================================

    private string[] textList;
    // 儲存目前這一整段對話的所有句子。

    private int index = 0;
    // 紀錄目前顯示到第幾句。


    // ==========================================
    // 對話狀態
    // ==========================================

    private bool inTalk = false;
    // 紀錄現在是不是正在進行對話。

    private bool clickPanel = false;
    // 紀錄玩家這一幀有沒有點擊對話框。


    // ==========================================
    // 角色顯示
    // ==========================================

    private GameObject currentSpeakerVisual;
    // 紀錄目前正在顯示的角色圖片或模型。


    // ==========================================
    // 對話結束事件
    // ==========================================

    private Action onDialogueFinished;
    // 儲存這段對話全部結束後要執行的事情。


    private void Awake()
    {
        // 初始化對話框本身的 Button。
        InitialTextButton();

        // 遊戲開始時先把對話框關閉。
        gameObject.SetActive(false);
    }


    private void Update()
    {
        // 如果現在沒有正在對話。
        if (!inTalk)
        {
            // 就不處理任何對話操作。
            return;
        }


        // 如果玩家沒有點擊對話框。
        if (!clickPanel)
        {
            // 就繼續等待玩家操作。
            return;
        }


        // 處理下一句對話。
        ShowNextText();


        // 處理完這次點擊後，
        // 把點擊狀態重新設回 false。
        clickPanel = false;
    }


    // ==========================================
    // 初始化對話框按鈕
    // ==========================================

    private void InitialTextButton()
    {
        // 如果沒有指定 Button。
        if (textPanelButton == null)
        {
            // 不執行後續設定。
            return;
        }


        // 清除 Inspector 或程式中可能殘留的舊事件。
        textPanelButton.onClick.RemoveListener(ONCLICKset);

        // 當玩家點擊整個對話框時，
        // 執行 ONCLICKset。
        textPanelButton.onClick.AddListener(ONCLICKset);
    }


    // ==========================================
    // 玩家點擊對話框
    // ==========================================

    public void ONCLICKset()
    {
        // 紀錄玩家已經點擊對話框。
        clickPanel = true;
    }


    // ==========================================
    // 開始一段新的對話
    // ==========================================

    public void OpenDialogue(
        string speakerName,
        GameObject speakerVisual,
        string[] dialogueLines,
        Action finishedAction = null)
    {
        // 儲存這一次的所有對話句子。
        textList = dialogueLines;


        // 從第一句開始。
        index = 0;


        // 儲存這段對話結束後要執行的函式。
        onDialogueFinished = finishedAction;


        // 設定說話者名稱。
        speakerNameText.text = speakerName;


        // 儲存目前角色物件。
        currentSpeakerVisual = speakerVisual;


        // 如果有指定角色圖片或模型。
        if (currentSpeakerVisual != null)
        {
            // 顯示角色。
            currentSpeakerVisual.SetActive(true);
        }


        // 正式進入對話狀態。
        inTalk = true;


        // 清除之前可能殘留的點擊。
        clickPanel = false;


        // 顯示對話框。
        gameObject.SetActive(true);


        // 顯示第一句。
        ShowCurrentText();
    }


    // ==========================================
    // 顯示目前這一句
    // ==========================================

    private void ShowCurrentText()
    {
        // 如果沒有任何對話資料。
        if (textList == null || textList.Length == 0)
        {
            // 直接結束對話。
            FinishDialogue();

            return;
        }


        // 顯示目前索引所對應的句子。
        dialogueText.text = textList[index];
    }


    // ==========================================
    // 前往下一句
    // ==========================================

    private void ShowNextText()
    {
        // 如果目前還有下一句。
        if (index < textList.Length - 1)
        {
            // 將索引移到下一句。
            index++;

            // 顯示新的句子。
            ShowCurrentText();

            return;
        }


        // 如果目前已經是最後一句，
        // 玩家再點一次就結束整段對話。
        FinishDialogue();
    }


    // ==========================================
    // 結束對話
    // ==========================================

    private void FinishDialogue()
    {
        // 離開對話狀態。
        inTalk = false;


        // 如果目前有角色圖片或模型。
        if (currentSpeakerVisual != null)
        {
            // 將角色隱藏。
            currentSpeakerVisual.SetActive(false);
        }


        // 暫存對話結束後要執行的事件。
        Action finishedAction = onDialogueFinished;


        // 清除這次的完成事件。
        onDialogueFinished = null;


        // 清除目前對話資料。
        textList = null;


        // 重設索引。
        index = 0;


        // 關閉對話框。
        gameObject.SetActive(false);


        // 如果有設定對話結束後要執行的事情。
        if (finishedAction != null)
        {
            // 執行對話結束事件。
            finishedAction.Invoke();
        }
    }
}