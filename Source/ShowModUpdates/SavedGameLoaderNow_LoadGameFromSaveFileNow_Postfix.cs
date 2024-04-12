using HarmonyLib;
using Verse;

namespace ShowModUpdates;

[HarmonyPatch(typeof(SavedGameLoaderNow), nameof(SavedGameLoaderNow.LoadGameFromSaveFileNow))]
public static class SavedGameLoaderNow_LoadGameFromSaveFileNow_Postfix
{
    public static void Postfix(string fileName)
    {
        if (Current.Game == null)
        {
            ShowModUpdates.CurrentSavePath = null;
            ShowModUpdates.CurrentSaveName = null;
            return;
        }

        ShowModUpdates.CurrentSavePath = GenFilePaths.FilePathForSavedGame(fileName);
        ShowModUpdates.CurrentSaveName = $"{fileName}.rws";
        ShowModUpdates.FinishedLoading = false;
        ShowModUpdates.ReadyToRead();
    }
}