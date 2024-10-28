using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace ShowModUpdates;

[HarmonyPatch(typeof(OptionListingUtility), nameof(OptionListingUtility.DrawOptionListing), typeof(Rect),
    typeof(List<ListableOption>))]
public static class OptionListingUtility_DrawOptionListing_Postfix
{
    public static void Postfix(Rect rect, List<ListableOption> optList, float __result)
    {
        if (!ShowModUpdates.ReadyToRead())
        {
            return;
        }

        if (Current.ProgramState == ProgramState.Entry)
        {
            return;
        }

        if (!ShowModUpdates.ModUpdates.Any() && !ShowModUpdates.GameUpdated)
        {
            return;
        }

        if (!optList.Any(option =>
                option is ListableOption_WebLink weblink && weblink.label == "BuySoundtrack".Translate()))
        {
            return;
        }

        var newRect = new Rect(170f + 24f + 3f, __result + 10f, rect.width, 35f);

        if (Widgets.ButtonText(newRect, ShowModUpdates.GetUpdatesString()))
        {
            Find.WindowStack.Add(new Dialog_ModUpdates());
        }
    }
}