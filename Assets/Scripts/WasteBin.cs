
using UnityEngine;

public class WasteBin : MonoBehaviour
{
    public WasteCategory acceptedCategory;

    public bool Accepts(WasteItem item)
    {
        return item.category == acceptedCategory;
    }
}
