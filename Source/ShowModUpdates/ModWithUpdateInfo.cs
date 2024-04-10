using System;
using Steamworks;
using Verse;

namespace ShowModUpdates;

public class ModWithUpdateInfo(ModMetaData modMetaData)
{
    public readonly ModMetaData ModMetaData = modMetaData;
    public Uri OtherUrl;
    public Uri SteamChangelog;
    public Uri SteamUri;

    public void PopulateLinks()
    {
        if (ModMetaData.OnSteamWorkshop)
        {
            var steamId = ModMetaData.GetPublishedFileId();
            if (steamId != PublishedFileId_t.Invalid)
            {
                SteamChangelog = new Uri(ShowModUpdates.SteamBaseChangelogUri, steamId.ToString());
                SteamUri = new Uri(ShowModUpdates.SteamBaseUri, steamId.ToString());
            }
        }

        if (!string.IsNullOrEmpty(ModMetaData.Url))
        {
            OtherUrl = new Uri(ModMetaData.Url);
        }
    }
}