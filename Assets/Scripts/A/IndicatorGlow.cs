using UnityEngine; // 引入 Unity 基本功能。
using UnityEngine.UI; // 引入 UI Image 功能。

public class IndicatorGlow : MonoBehaviour
{
    // ==========================================
    // 內層光暈
    // ==========================================

    [Header("內層光暈")]

    public RectTransform glowInner;
    // 內層光暈的 RectTransform。
    // 用來控制縮放大小。


    public Image glowInnerImage;
    // 內層光暈的 Image。
    // 用來控制透明度。


    // ==========================================
    // 外層光暈
    // ==========================================

    [Header("外層光暈")]

    public RectTransform glowOuter;
    // 外層光暈的 RectTransform。
    // 用來控制縮放大小。


    public Image glowOuterImage;
    // 外層光暈的 Image。
    // 用來控制透明度。


    // ==========================================
    // 呼吸速度
    // ==========================================

    [Header("呼吸速度")]

    public float pulseSpeed = 2f;
    // 光暈呼吸的速度。
    // 數值越大，變化越快。


    // ==========================================
    // 內層光暈設定
    // ==========================================

    [Header("內層光暈設定")]

    public float innerMinScale = 1.15f;
    // 內層光暈最小倍率。


    public float innerMaxScale = 1.35f;
    // 內層光暈最大倍率。


    [Range(0f, 1f)]
    public float innerMinAlpha = 0.15f;
    // 內層光暈最淡時的透明度。


    [Range(0f, 1f)]
    public float innerMaxAlpha = 0.4f;
    // 內層光暈最亮時的透明度。


    // ==========================================
    // 外層光暈設定
    // ==========================================

    [Header("外層光暈設定")]

    public float outerMinScale = 1.35f;
    // 外層光暈最小倍率。


    public float outerMaxScale = 1.65f;
    // 外層光暈最大倍率。


    [Range(0f, 1f)]
    public float outerMinAlpha = 0.05f;
    // 外層光暈最淡時的透明度。


    [Range(0f, 1f)]
    public float outerMaxAlpha = 0.2f;
    // 外層光暈最亮時的透明度。


    // ==========================================
    // 原始比例
    // ==========================================

    private Vector3 innerBaseScale;
    // 保存內層光暈原本的 Scale。


    private Vector3 outerBaseScale;
    // 保存外層光暈原本的 Scale。


    // ==========================================
    // 初始化
    // ==========================================

    private void Awake()
    {
        // 如果有指定內層光暈。
        if (glowInner != null)
        {
            // 保存內層光暈目前的原始比例。
            innerBaseScale = glowInner.localScale;
        }


        // 如果有指定外層光暈。
        if (glowOuter != null)
        {
            // 保存外層光暈目前的原始比例。
            outerBaseScale = glowOuter.localScale;
        }
    }


    // ==========================================
    // 每幀更新光暈
    // ==========================================

    private void Update()
    {
        // 使用 Sin 產生 0 到 1 之間循環變化的數值。
        float pulse =
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;


        // 更新內層光暈。
        UpdateInnerGlow(pulse);


        // 更新外層光暈。
        UpdateOuterGlow(pulse);
    }


    // ==========================================
    // 更新內層光暈
    // ==========================================

    private void UpdateInnerGlow(float pulse)
    {
        // 如果沒有指定內層光暈 Transform。
        if (glowInner == null)
        {
            return;
        }


        // 計算目前內層光暈的縮放倍率。
        float scaleMultiplier =
            Mathf.Lerp(
                innerMinScale,
                innerMaxScale,
                pulse
            );


        // 根據原始大小改變光暈尺寸。
        glowInner.localScale =
            innerBaseScale *
            scaleMultiplier;


        // 如果沒有指定 Image。
        if (glowInnerImage == null)
        {
            return;
        }


        // 取得目前光暈顏色。
        Color color =
            glowInnerImage.color;


        // 根據呼吸進度調整透明度。
        color.a =
            Mathf.Lerp(
                innerMinAlpha,
                innerMaxAlpha,
                pulse
            );


        // 將新顏色設定回 Image。
        glowInnerImage.color =
            color;
    }


    // ==========================================
    // 更新外層光暈
    // ==========================================

    private void UpdateOuterGlow(float pulse)
    {
        // 如果沒有指定外層光暈 Transform。
        if (glowOuter == null)
        {
            return;
        }


        // 計算目前外層光暈的縮放倍率。
        float scaleMultiplier =
            Mathf.Lerp(
                outerMinScale,
                outerMaxScale,
                pulse
            );


        // 根據原始大小改變外層光暈尺寸。
        glowOuter.localScale =
            outerBaseScale *
            scaleMultiplier;


        // 如果沒有指定外層 Image。
        if (glowOuterImage == null)
        {
            return;
        }


        // 取得目前外層光暈顏色。
        Color color =
            glowOuterImage.color;


        // 外層光暈使用相反的呼吸節奏，
        // 讓兩層光暈不會完全一起亮暗。
        color.a =
            Mathf.Lerp(
                outerMinAlpha,
                outerMaxAlpha,
                1f - pulse
            );


        // 將新顏色設定回 Image。
        glowOuterImage.color =
            color;
    }
}