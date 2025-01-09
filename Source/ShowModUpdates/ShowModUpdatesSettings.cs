using Verse;

namespace ShowModUpdates;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class ShowModUpdatesSettings : ModSettings
{
    public bool CheckAll;
    public bool CheckOnline;
    public bool OrderByDate;
    public bool PreferOverlay;

    /// <summary>
    ///     Saving and loading the values
    /// </summary>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref CheckAll, "CheckAll", true);
        Scribe_Values.Look(ref CheckOnline, "CheckOnline");
        Scribe_Values.Look(ref OrderByDate, "OrderByDate");
        Scribe_Values.Look(ref PreferOverlay, "PreferOverlay");
    }
}