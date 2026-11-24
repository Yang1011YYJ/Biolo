using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParentSelectionPanel : MonoBehaviour
{
    [Header("介面")]

    [Tooltip("親代選擇面板根物件。")]
    public GameObject panelRoot;

    [Tooltip("自動產生親代選項的容器，建議加 Vertical Layout Group。")]
    public Transform optionsRoot;

    [Tooltip("親代選項範本。需包含 TMP 文字，且不要指定 Toggle Group。")]
    public Toggle optionTemplate;

    [Tooltip("顯示已選取的親代組合。")]
    public TMP_Text selectionText;

    [Tooltip("交配方一、交配方二都選好後才能按下，允許選擇同一株。")]
    public Button confirmButton;

    [Tooltip("清空兩個交配方，讓玩家重新選擇。")]
    public Button resetButton;

    [Tooltip("取消選擇並恢復探索。")]
    public Button cancelButton;

    // 本次選取的親代，保留選取順序。
    private readonly List<PeaPlant> selectedParents =
        new List<PeaPlant>();

    // 本次建立的選項，關閉後清除。
    private readonly List<Toggle> generatedOptions =
        new List<Toggle>();

    private Action<PeaPlant, PeaPlant> onConfirm;
    private Action onCancel;

    private void Awake()
    {
        optionTemplate.gameObject.SetActive(false);

        confirmButton.onClick.AddListener(Confirm);
        // 綁定重新選擇按鈕。
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetSelection);
        }
        cancelButton.onClick.AddListener(Cancel);

        panelRoot.SetActive(false);
    }

    public void Open(
        List<PeaPlant> plants,
        Action<PeaPlant, PeaPlant> confirmCallback,
        Action cancelCallback)
    {
        ClearOptions();

        onConfirm = confirmCallback;
        onCancel = cancelCallback;

        // 避免清單中不小心指定到同一株兩次。
        HashSet<PeaPlant> addedPlants = new HashSet<PeaPlant>();

        foreach (PeaPlant plant in plants)
        {
            if (plant == null ||
                !plant.canBreed ||
                !plant.gameObject.activeInHierarchy ||
                !addedPlants.Add(plant))
            {
                continue;
            }

            Toggle option = Instantiate(optionTemplate, optionsRoot);

            option.group = null;
            option.SetIsOnWithoutNotify(false);

            TMP_Text label =
                option.GetComponentInChildren<TMP_Text>(true);
            Debug.Log(
                $"親代選項｜植物：{plant.name}｜" +
                $"名稱：{plant.displayName}｜" +
                $"文字元件：{(label != null ? label.name : "找不到")}",
                plant
            );

            if (label != null)
            {
                label.text =
                    $"{plant.displayName}"+":"+$"{plant.description}";
            }

            // 每個選項保存自己對應的植物。
            PeaPlant optionPlant = plant;

            option.onValueChanged.AddListener(
                isOn => ChangeSelection(optionPlant, option, isOn)
            );

            generatedOptions.Add(option);
            option.gameObject.SetActive(true);
        }

        RefreshSelection();
        panelRoot.SetActive(true);
    }

    private void ChangeSelection(
    PeaPlant plant,
    Toggle option,
    bool isOn)
    {
        // 只處理玩家勾選的那一刻。
        if (!isOn)
        {
            return;
        }

        // 立即清除勾選，讓同一個選項可以再次被點選。
        // 不觸發事件，避免重複執行這個函式。
        option.SetIsOnWithoutNotify(false);

        // 選滿兩個交配方後，不再新增。
        if (selectedParents.Count >= 2)
        {
            return;
        }

        // 確認這株植物目前仍可使用。
        if (plant == null ||
            !plant.canBreed ||
            !plant.gameObject.activeInHierarchy)
        {
            return;
        }

        // 依序記錄，不排除重複植物，因此同一株可以選兩次。
        selectedParents.Add(plant);

        // 更新交配方文字與按鈕狀態。
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        // 選滿兩個交配方才允許確認。
        bool selectionComplete = selectedParents.Count == 2;

        // 更新確認按鈕。
        confirmButton.interactable = selectionComplete;

        // 選滿後暫停選項，重新選擇時再開放。
        foreach (Toggle option in generatedOptions)
        {
            // 略過已不存在的選項。
            if (option == null)
            {
                continue;
            }

            // Toggle 只負責點選，畫面上保持未勾選。
            option.SetIsOnWithoutNotify(false);

            // 還沒選滿時才允許點選。
            option.interactable = !selectionComplete;
        }

        // 至少選過一次，才需要開放重新選擇。
        if (resetButton != null)
        {
            resetButton.interactable = selectedParents.Count > 0;
        }

        // 取得第一個交配方的顯示名稱。
        string firstName = selectedParents.Count >= 1
            ? selectedParents[0].displayName
            : "尚未選擇";

        // 取得第二個交配方的顯示名稱。
        string secondName = selectedParents.Count >= 2
            ? selectedParents[1].displayName
            : "尚未選擇";

        // 固定顯示兩個欄位，讓玩家知道還缺哪一方。
        selectionText.text =
            $"{firstName} X {secondName}";
    }

    private void ResetSelection()
    {
        // 清空兩次選取結果，保留已建立的選項。
        selectedParents.Clear();

        // 恢復選項並更新文字。
        RefreshSelection();
    }

    private void Confirm()
    {
        if (selectedParents.Count != 2)
        {
            return;
        }

        PeaPlant first = selectedParents[0];
        PeaPlant second = selectedParents[1];

        if (first == null || second == null ||
            !first.canBreed || !second.canBreed)
        {
            return;
        }

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
        ClearOptions();

        onConfirm = null;
        onCancel = null;
    }

    private void ClearOptions()
    {
        foreach (Toggle option in generatedOptions)
        {
            if (option != null)
            {
                // Destroy 會延後，先停用避免新舊選項重疊。
                option.gameObject.SetActive(false);
                Destroy(option.gameObject);
            }
        }

        generatedOptions.Clear();
        selectedParents.Clear();
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
        // 移除重新選擇按鈕的事件。
        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(ResetSelection);
        }
    }
}