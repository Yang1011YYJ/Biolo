using UnityEngine;

public class PeaPlant : MonoBehaviour
{
    [Tooltip("親代選擇面板中顯示的名稱，例如親代甲。")]
    public string displayName;

    [Tooltip("目前可觀察到的資料，例如純品系、高莖。不提前顯示基因型。")]
    [TextArea]
    public string description;

    [Tooltip("交配時玩家的站立定位點。開場親代手動指定，生成子代由程式取得所屬種植區的 Interaction Point。")]
    public Transform interactionPoint;

    [Tooltip("這株植物目前是否可作為交配親代。")]
    public bool canBreed = true;

    [Tooltip("這株植物由哪顆種子長成。開場親代甲、乙留空，由播種程式設定子代資料。")]
    public SeedData sourceSeed;
}