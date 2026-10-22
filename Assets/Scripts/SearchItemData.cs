using UnityEngine;

[CreateAssetMenu(fileName = "SearchItemData", menuName = "Gameplay/Search Item Data")]
public class SearchItemData : ScriptableObject
{
    [Header("Item Details")]
    public string itemName;

    [TextArea(2, 5)]
    public string description;

    [Header("Visuals")]
    public Sprite itemIcon;
    public Texture2D itemTexture;
    public Material itemMaterial;
}
