using UnityEngine;

public class PeaPlant : MonoBehaviour
{
    [Tooltip("親代選擇面板中顯示的名稱，例如親代甲。")]
    public string displayName;

    [Tooltip("目前可觀察到的資料，例如純品系、高莖。不提前顯示基因型。")]
    [TextArea]
    public string description;

    [Tooltip("系統控制玩家採集／授粉時的站立定位點。移動只取 X、Z。")]
    public Transform interactionPoint;

    [Tooltip("這株植物目前是否可作為交配親代。")]
    public bool canBreed = true;
}