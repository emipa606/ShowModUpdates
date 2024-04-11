using Verse;

namespace ShowModUpdates;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class ShowModUpdatesSettings : ModSettings
{
    public bool CheckAll;

    /// <summary>
    ///     Saving and loading the values
    /// </summary>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref CheckAll, "CheckAll", true);
    }
}