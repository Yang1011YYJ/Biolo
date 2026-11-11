using System.Collections;
using TMPro;
using UnityEngine;

public class SystemHintController : MonoBehaviour
{
    [Header("提示介面")]

    [Tooltip("畫面下方提示框的根物件，由此程式控制顯示與隱藏。")]
    public GameObject panelRoot;

    [Tooltip("顯示操作提示、獲得道具與失敗原因的 TMP 文字。")]
    public TMP_Text hintText;

    [Tooltip("提示框上的 CanvasGroup，讓提示不阻擋滑鼠操作。")]
    public CanvasGroup canvasGroup;

    [Header("顯示設定")]

    [Tooltip("每則提示預設顯示的秒數。新提示出現時會重新計時。")]
    [Min(0.1f)]
    public float defaultDuration = 3f;

    // 保存目前的自動隱藏程序，供新提示取消舊計時。
    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        panelRoot.SetActive(false);
    }

    public void Show(string message, float duration = -1f)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        hintText.text = message;
        panelRoot.SetActive(true);

        // 確保提示顯示在同一父物件下的其他 UI 上方。
        panelRoot.transform.SetAsLastSibling();

        float displayDuration = duration > 0f
            ? duration
            : defaultDuration;

        hideCoroutine = StartCoroutine(
            HideAfterDelay(displayDuration)
        );
    }

    private IEnumerator HideAfterDelay(float duration)
    {
        // 不受 Time.timeScale 影響。
        yield return new WaitForSecondsRealtime(duration);

        panelRoot.SetActive(false);
        hideCoroutine = null;
    }

    public void Hide()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        panelRoot.SetActive(false);
    }
}