using HarmonyLib;
using UnityEngine;
using Verse;

namespace ShowModUpdates;

[HarmonyPatch(typeof(Widgets), nameof(Widgets.ButtonText), typeof(Rect), typeof(string), typeof(bool), typeof(bool),
    typeof(bool), typeof(TextAnchor))]
public static class Widgets_ButtonText_Postfix
{
    public static void Postfix(Rect rect, string label)
    {
        if (ShowModUpdates.NoExistingSave || label != LanguageDatabase.activeLanguage.FriendlyNameNative ||
            !ShowModUpdates.FinishedLoading)
        {
            return;
        }

        if (!ShowModUpdates.ModUpdates.Any() && !ShowModUpdates.GameUpdated)
        {
            return;
        }

        if (Find.WindowStack.AnyWindowAbsorbingAllInput)
        {
            return;
        }

        var newRect = rect;
        newRect.y += rect.height + 5f;
        if (Widgets.ButtonText(newRect, ShowModUpdates.GetUpdatesString()))
        {
            Find.WindowStack.Add(new Dialog_ModUpdates());
        }
    }
}