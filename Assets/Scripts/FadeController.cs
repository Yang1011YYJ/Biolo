using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    

    [Header("主程式")]
    public GameManager gameManager; // 整體遊戲流程控制程式。

    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    

    // 通用淡入淡出函式
    public void Fade(GameObject target, float start, float end, float duration, Action onComplete = null)
    {
        // 啟動淡入淡出協程
        StartCoroutine(FadeRoutine(target, start, end, duration, onComplete));
    }

    // 執行淡入淡出動畫
    private IEnumerator FadeRoutine(GameObject target, float start, float end, float duration, Action onComplete)
    {

        // 檢查目標物件是否存在
        if (target == null) yield break;

        // 取得目標物件的 CanvasGroup 元件
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();

        // 如果沒有 CanvasGroup，就結束協程
        if (canvasGroup == null) yield break;

        // 設定初始透明度
        canvasGroup.alpha = start;

        // 初始化計時器
        float time = 0f;

        // 持續執行直到達到指定時間
        while (time < duration)
        {
            // 累加每一幀經過的時間
            time += Time.deltaTime;

            // 計算目前動畫進度
            float progress = duration > 0f ? Mathf.Clamp01(time / duration) : 1f;

            // 根據進度計算透明度
            canvasGroup.alpha = Mathf.Lerp(start, end, progress);

            // 等待下一幀
            yield return null;
        }

        // 確保最後透明度準確
        canvasGroup.alpha = end;

        // 如果有指定完成事件，就執行
        onComplete?.Invoke();
    }

    // 通用等待函式，seconds 是等待秒數，onComplete 是等待結束後的動作
    public void Wait(float seconds, Action onComplete = null)
    {
        // 啟動等待協程
        StartCoroutine(WaitRoutine(seconds, onComplete));
    }

    // 實際執行等待的協程
    private IEnumerator WaitRoutine(float seconds, Action onComplete)
    {
        // 等待指定秒數
        yield return new WaitForSeconds(seconds);

        // 等待結束後，執行指定動作
        onComplete?.Invoke();
    }
}
