using UnityEngine;

public enum WasteCategory
{
    Organik,
    Anorganik
}

public class WasteItem : MonoBehaviour
{
    [Header("Kategori Sampah")]
    public WasteCategory category;
}