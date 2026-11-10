using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParentSelectionPanel : MonoBehaviour
{
    [Serializable]
    public class ParentOption
    {
        [Tooltip("這個選項對應的場景植物。")]
        public PeaPlant plant;

        [Tooltip("選擇／取消選擇這株親代的 Toggle。")]
        public Toggle toggle;

        [Tooltip("顯示親代名稱與可觀察資料的文字。")]
        public Text label;
    }

    [Header("介面")]

    [Tooltip("親代選擇面板的根物件。")]
    public GameObject panelRoot;

    [Tooltip("可選親代與其對應的介面元件。目前設定甲、乙兩項。")]
    public ParentOption[] options;

    [Tooltip("顯示目前選取的親代組合與選取提示。")]
    public TMP_Text selectionText;

    [Tooltip("選滿兩株不同親代後才能操作的開始交配按鈕。")]
    public Button confirmButton;

    [Tooltip("取消選擇並返回探索的按鈕。")]
    public Button cancelButton;

    // 保存這次選取順序，供後續系統走位使用。
    private readonly List<PeaPlant> selectedParents =
        new List<PeaPlant>();

    private Action<PeaPlant, PeaPlant> onConfirm;
    private Action onCancel;

    private void Awake()
    {
        foreach (ParentOption option in options)
        {
            option.toggle.onValueChanged.AddListener(
                OnSelectionChanged
            );
        }

        confirmButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Cancel);

        panelRoot.SetActive(false);
    }

    public void Open(
        Action<PeaPlant, PeaPlant> confirmCallback,
        Action cancelCallback)
    {
        onConfirm = confirmCallback;
        onCancel = cancelCallback;

        selectedParents.Clear();

        foreach (ParentOption option in options)
        {
            option.toggle.SetIsOnWithoutNotify(false);

            bool available =
                option.plant != null &&
                option.plant.canBreed;

            option.toggle.interactable = available;

            option.label.text = option.plant != null
                ? $"{option.plant.displayName}\n{option.plant.description}"
                : "未指定植物";
        }

        RefreshSelection();
        panelRoot.SetActive(true);
    }

    private void OnSelectionChanged(bool unused)
    {
        // 移除已取消或已不可用的親代。
        selectedParents.RemoveAll(
            plant => !IsSelectedAndAvailable(plant)
        );

        foreach (ParentOption option in options)
        {
            if (!option.toggle.isOn ||
                option.plant == null ||
                !option.plant.canBreed)
            {
                continue;
            }

            // 避免同一株植物被重複加入。
            if (selectedParents.Contains(option.plant))
            {
                continue;
            }

            if (selectedParents.Count >= 2)
            {
                option.toggle.SetIsOnWithoutNotify(false);
                continue;
            }

            selectedParents.Add(option.plant);
        }

        RefreshSelection();
    }

    private bool IsSelectedAndAvailable(PeaPlant plant)
    {
        if (plant == null || !plant.canBreed)
        {
            return false;
        }

        foreach (ParentOption option in options)
        {
            if (option.plant == plant && option.toggle.isOn)
            {
                return true;
            }
        }

        return false;
    }

    private void RefreshSelection()
    {
        confirmButton.interactable = selectedParents.Count == 2;

        if (selectedParents.Count == 0)
        {
            selectionText.text = "請選擇兩株親代";
        }
        else if (selectedParents.Count == 1)
        {
            selectionText.text =
                $"{selectedParents[0].displayName} × 尚未選擇";
        }
        else
        {
            selectionText.text =
                $"{selectedParents[0].displayName} × " +
                $"{selectedParents[1].displayName}";
        }
    }

    private void Confirm()
    {
        // 確認前再次更新，避免植物狀態改變。
        OnSelectionChanged(false);

        if (selectedParents.Count != 2)
        {
            return;
        }

        PeaPlant first = selectedParents[0];
        PeaPlant second = selectedParents[1];

        Action<PeaPlant, PeaPlant> callback = onConfirm;

        Hide();
        callback?.Invoke(first, second);
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

        selectedParents.Clear();
        onConfirm = null;
        onCancel = null;
    }

    private void OnDestroy()
    {
        foreach (ParentOption option in options)
        {
            if (option.toggle != null)
            {
                option.toggle.onValueChanged.RemoveListener(
                    OnSelectionChanged
                );
            }
        }

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