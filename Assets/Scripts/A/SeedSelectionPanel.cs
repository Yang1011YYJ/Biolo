using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeedSelectionPanel : MonoBehaviour
{
    [Tooltip("種子選擇面板根物件。")]
    public GameObject panelRoot;

    [Tooltip("列出每顆種子的編號與親代來源。")]
    public TMP_Dropdown seedDropdown;

    [Tooltip("顯示本次準備播種的種植區名稱。")]
    public TMP_Text locationText;

    [Tooltip("確認使用目前選中的種子進行播種。")]
    public Button confirmButton;

    [Tooltip("取消選擇，不消耗種子。")]
    public Button cancelButton;

    // 保存本次顯示的種子對照，避免用索引直接操作背包。
    private readonly List<SeedData> displayedSeeds =
        new List<SeedData>();

    private Action<SeedData> onConfirm;
    private Action onCancel;

    private void Awake()
    {
        confirmButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Cancel);

        panelRoot.SetActive(false);
    }

    public void Open(
        List<SeedData> seeds,
        string spotName,
        Action<SeedData> confirmCallback,
        Action cancelCallback)
    {
        onConfirm = confirmCallback;
        onCancel = cancelCallback;

        displayedSeeds.Clear();
        seedDropdown.ClearOptions();

        List<string> labels = new List<string>();

        foreach (SeedData seed in seeds)
        {
            if (seed == null)
            {
                continue;
            }

            displayedSeeds.Add(seed);
            labels.Add(seed.GetLabel());
        }

        seedDropdown.AddOptions(labels);
        seedDropdown.SetValueWithoutNotify(0);
        seedDropdown.RefreshShownValue();

        locationText.text = $"播種位置：{spotName}";

        bool hasSeeds = displayedSeeds.Count > 0;

        seedDropdown.interactable = hasSeeds;
        confirmButton.interactable = hasSeeds;

        panelRoot.SetActive(true);
    }

    private void Confirm()
    {
        int index = seedDropdown.value;

        if (index < 0 || index >= displayedSeeds.Count)
        {
            return;
        }

        SeedData selectedSeed = displayedSeeds[index];
        Action<SeedData> callback = onConfirm;

        Hide();
        callback?.Invoke(selectedSeed);
    }

    private void Cancel()
    {
        Action callback = onCancel;

        Hide();
        callback?.Invoke();
    }

    private void Hide()
    {
        panelRoot.SetActive(false);

        displayedSeeds.Clear();
        onConfirm = null;
        onCancel = null;
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(Confirm);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(Cancel);
        }
    }
}