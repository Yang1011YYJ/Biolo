using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlantingActionPanel : MonoBehaviour
{
    [Header("介面")]
    public GameObject panelRoot;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI hintText;

    public Button sowButton;
    public Button MatButton;
    public Button closeButton;

    private PlantingSpot currentSpot;

    private Action<PlantingSpot> onMat;
    private Action onClose;

    private void Awake()
    {
        sowButton.interactable = false;

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
        Action<PlantingSpot> MatCallback,
        Action closeCallback)
    {
        currentSpot = spot;

        onMat = MatCallback;
        onClose = closeCallback;

        titleText.text = spot.spotName;
        hintText.text = "尚無可播種的種子";

        sowButton.interactable = false;
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
    }
}