using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlantingActionPanel : MonoBehaviour
{
    [Header("介面")]

    [Tooltip("播種／交配操作選單的根物件，由此程式控制顯示與隱藏。")]
    public GameObject panelRoot;

    [Tooltip("顯示目前選取種植區名稱的文字。")]
    public TextMeshProUGUI titleText;

    [Tooltip("顯示無種子等操作提示的文字。")]
    public TextMeshProUGUI hintText;

    [Tooltip("播種按鈕。目前沒有種子，維持不可操作。")]
    public Button sowButton;

    [Tooltip("交配按鈕。按下後將選定種植區交給 GameManager。")]
    public Button MatButton;

    [Tooltip("關閉選單並恢復自由探索的按鈕。")]
    public Button closeButton;

    private PlantingSpot currentSpot;

    private Action<PlantingSpot> onMat;
    private Action onClose;
    // 播種按鈕按下後，交由 GameManager 處理。
    private Action<PlantingSpot> onSow;

    private void Awake()
    {
        sowButton.interactable = false;
        sowButton.onClick.AddListener(OnSowClicked);
        MatButton.onClick.AddListener(
            OnMatClicked
        );

        closeButton.onClick.AddListener(
            OnCloseClicked
        );

        panelRoot.SetActive(false);
    }

    // 由 GameManager 開啟，並傳入後續處理。
    public void Open(
    PlantingSpot spot,
    Action<PlantingSpot> crossbreedCallback,
    Action closeCallback,
    int seedCount = 0,
    string message = null,
    Action<PlantingSpot> sowCallback = null)
    {
        currentSpot = spot;

        onMat = crossbreedCallback;
        onClose = closeCallback;
        onSow = sowCallback;
        titleText.text = spot.spotName;

        hintText.text = string.IsNullOrEmpty(message)
            ? (
                seedCount > 0
                    ? $"目前持有種子：{seedCount} 顆"
                    : "尚無可播種的種子"
            )
            : message;

        sowButton.interactable =
            seedCount > 0 &&
            spot.CanUse() &&
            onSow != null;

        MatButton.interactable = spot.CanUse();

        panelRoot.SetActive(true);
    }

    private void OnMatClicked()
    {
        if (currentSpot == null || !currentSpot.CanUse())
        {
            return;
        }

        // 先保存資料，再清除目前選單。
        PlantingSpot selectedSpot = currentSpot;
        Action<PlantingSpot> callback = onMat;

        Hide();

        // 交給 GameManager 接續開啟親代面板。
        // 此時不恢復自由探索。
        callback?.Invoke(selectedSpot);
    }

    private void OnCloseClicked()
    {
        Action callback = onClose;

        Hide();

        // 交給 GameManager 恢復探索。
        callback?.Invoke();
    }

    private void Hide()
    {
        panelRoot.SetActive(false);

        currentSpot = null;
        onMat = null;
        onClose = null;
        onSow = null;
    }

    private void OnDestroy()
    {
        if (MatButton != null)
        {
            MatButton.onClick.RemoveListener(
                OnMatClicked
            );
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(
                OnCloseClicked
            );
        }

        if (sowButton != null)
        {
            sowButton.onClick.RemoveListener(OnSowClicked);
        }
    }

    private void OnSowClicked()
    {
        if (currentSpot == null ||
            !currentSpot.CanUse() ||
            onSow == null)
        {
            return;
        }

        PlantingSpot spot = currentSpot;
        Action<PlantingSpot> callback = onSow;

        Hide();
        callback.Invoke(spot);
    }
}