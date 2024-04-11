using System;
using System.Text.RegularExpressions;
using Steamworks;
using Verse;

namespace ShowModUpdates;

public class ModWithUpdateInfo(ModMetaData modMetaData)
{
    public readonly ModMetaData ModMetaData = modMetaData;
    public Uri DiscordUri;
    public Uri OtherUrl;
    public Uri SteamChangelog;
    public Uri SteamUri;

    public void PopulateLinks()
    {
        var steamId = ModMetaData.GetPublishedFileId();
        if (steamId != PublishedFileId_t.Invalid)
        {
            SteamChangelog = new Uri(ShowModUpdates.SteamBaseChangelogUri, steamId.ToString());
            SteamUri = new Uri(ShowModUpdates.SteamBaseUri, steamId.ToString());
        }

        if (!string.IsNullOrEmpty(ModMetaData.Url))
        {
            OtherUrl = new Uri(ModMetaData.Url);
        }

        if (string.IsNullOrEmpty(ModMetaData.Description))
        {
            return;
        }

        var urlsInDescription = Regex.Matches(ModMetaData.Description,
            "(http|ftp|https):\\/\\/([\\w_-]+(?:(?:\\.[\\w_-]+)+))([\\w.,@?^=%&:\\/~+#-]*[\\w@?^=%&\\/~+#-])");

        if (urlsInDescription.Count == 0)
        {
            return;
        }

        foreach (Match url in urlsInDescription)
        {
            if (ShowModUpdates.DiscordDomains?.Any(domain => url.Value.Contains(domain)) == false)
            {
                continue;
            }

            DiscordUri = new Uri(url.Value);
            break;
        }
    }
}