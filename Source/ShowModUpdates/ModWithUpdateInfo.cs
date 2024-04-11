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
            Uri.TryCreate(ShowModUpdates.SteamBaseChangelogUri, steamId.ToString(), out SteamChangelog);
            Uri.TryCreate(ShowModUpdates.SteamBaseUri, steamId.ToString(), out SteamUri);
        }

        if (!string.IsNullOrEmpty(ModMetaData.Url))
        {
            Uri.TryCreate(ModMetaData.Url, UriKind.Absolute, out OtherUrl);
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

            Uri.TryCreate(url.Value, UriKind.Absolute, out DiscordUri);
            break;
        }
    }
}