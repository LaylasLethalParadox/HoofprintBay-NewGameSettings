using System;
using UnityEngine;
using UnityEngine.UI;

namespace NewGameSettings;

// Other mods may add a heading to the per-save column whose rows Mod Manager then moves to its Mods menu
// (ImprovedStaff's "Mods"), which leaves the heading alone on the paper. It is checked every frame because
// ImprovedStaff replaces its heading a frame after the column opens.
public class EmptyHeadings : MonoBehaviour
{
    public EmptyHeadings(IntPtr ptr) : base(ptr) { }

    internal Transform Content;

    private void Update()
    {
        if (Content == null)
            return;
        for (var i = 0; i < Content.childCount; i++)
        {
            var row = Content.GetChild(i);
            // The game's own titles are named Text...; only other mods' text rows are touched.
            if (!row.gameObject.activeSelf || row.name.StartsWith("Text") || !IsText(row))
                continue;
            var next = NextActive(i);
            if (next == null || IsText(next))
                row.gameObject.SetActive(false);
        }
    }

    private Transform NextActive(int index)
    {
        for (var i = index + 1; i < Content.childCount; i++)
            if (Content.GetChild(i).gameObject.activeSelf)
                return Content.GetChild(i);
        return null;
    }

    private static bool IsText(Transform row) =>
        row.GetComponentInChildren<Selectable>(true) == null && row.GetComponentInChildren<TMPro.TMP_Text>(true) != null;
}
