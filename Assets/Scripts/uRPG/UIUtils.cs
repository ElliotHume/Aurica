using UnityEngine;
using UnityEngine.UI;

public class UIUtils
{
    // instantiate/remove enough prefabs to match amount
    public static void BalancePrefabs(GameObject prefab, int amount, Transform parent)
    {
        // instantiate until amount
        for (int i = parent.childCount; i < amount; ++i)
        {
            GameObject go = GameObject.Instantiate(prefab);
            go.transform.SetParent(parent, false);
        }

        // delete everything that's too much
        // (backwards loop because Destroy changes childCount)
        for (int i = parent.childCount-1; i >= amount; --i)
            GameObject.Destroy(parent.GetChild(i).gameObject);
    }

    // find out if any input is currently active by using Selectable.all
    // (FindObjectsOfType<InputField>() is far too slow for huge scenes)
    public static bool AnyInputActive()
    {
        // avoid Linq.Any because it is HEAVY(!) on GC and performance
        foreach (Selectable sel in Selectable.allSelectablesArray)
            if (sel is InputField && ((InputField)sel).isFocused)
                return true;
        return false;
    }
}
