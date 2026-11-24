using System;
using UnityEngine;

[Serializable]
public class SeedData
{
    // 區分種子的交配來源。
    public enum CrossType
    {
        OriginalParents, // 原本的親代甲 × 親代乙。
        F1WithShortParent, // F1 與矮莖親代交配，保留供後續使用。
        F1SelfCross       // 同一株 F1 自交，產生 F2。
    }

    [Tooltip("這顆種子由哪一種交配組合產生，供播種時決定子代資料。")]
    public CrossType crossType = CrossType.OriginalParents;
    [Tooltip("這顆種子長成後是否為高莖。交配取得時決定，播種時讀取。")]
    public bool isTall = true;
    [Tooltip("每顆種子的獨立編號，不會因播種或清單排序改變。")]
    public string seedId;

    [Tooltip("顯示給玩家的種子名稱。")]
    public string displayName;

    [Tooltip("產生這顆種子的第一株親代名稱。")]
    public string firstParentName;

    [Tooltip("產生這顆種子的第二株親代名稱。")]
    public string secondParentName;

    [Tooltip("子代世代。第一子代為 1，第二子代為 2。")]
    public int generation;

    public string GetLabel()
    {
        return $"{displayName}｜{firstParentName} × {secondParentName}";
    }
}