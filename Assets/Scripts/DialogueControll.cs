using UnityEngine;

public class DialogueControll : MonoBehaviour
{
    [Header("玩家")]

    public Move playerMove; // 玩家身上的 ClickMove。


    private void Start() // 遊戲開始時執行。
    {
        // 顯示目前這個對話 Panel。
        gameObject.SetActive(true);

        // 鎖住玩家操作。
        playerMove.SetMovementEnabled(false);
    }


    public void CloseDialogue() // 點擊對話框時執行。
    {
        // 隱藏開場對話框。
        gameObject.SetActive(false);

        // 解鎖玩家操作。
        playerMove.SetMovementEnabled(true);
    }
}
