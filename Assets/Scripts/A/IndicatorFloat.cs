using UnityEngine; // 引入 Unity 基本功能。

public class IndicatorFloat : MonoBehaviour
{
    // ==========================================
    // 浮動設定
    // ==========================================

    [Header("上下浮動設定")]

    public float floatDistance = 0.15f;
    // 箭頭上下浮動的距離。
    // 數值越大，移動幅度越明顯。


    public float floatSpeed = 2f;
    // 箭頭上下浮動的速度。


    // ==========================================
    // 初始位置
    // ==========================================

    private Vector3 startLocalPosition;
    // 紀錄箭頭原本的 Local Position。


    // ==========================================
    // 初始化
    // ==========================================

    private void Awake()
    {
        // 記住箭頭原本的位置。
        startLocalPosition = transform.localPosition;
    }


    // ==========================================
    // 每幀更新
    // ==========================================

    private void Update()
    {
        // 使用 Sin 產生 -1 到 1 之間
        // 平順循環的數值。
        float offsetY =
            Mathf.Sin(Time.time * floatSpeed)
            * floatDistance;


        // 從原本的位置開始，
        // 只修改 Y 軸的位置。
        transform.localPosition =
            startLocalPosition +
            new Vector3(
                0f,
                offsetY,
                0f
            );
    }
}