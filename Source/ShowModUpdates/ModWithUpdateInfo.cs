using System;
using System.Text.RegularExpressions;
using Steamworks;
using Verse;

namespace ShowModUpdates;

public class ModWithUpdateInfo(ModMetaData modMetaData)
{
    public readonly ModMetaData ModMetaData = modMetaData;
    public readonly PublishedFileId_t PublishedFileId = modMetaData.GetPublishedFileId();
    public string Description;
    public Uri DiscordUri;
    public Uri OtherUrl;
    private bool Populated;
    public Uri SteamChangelog;
    public Uri SteamUri;
    public bool Synced;
    public DateTime Updated;

    public void PopulateLinks()
    {
        if (Populated)
        {
            return;
        }

        Populated = true;
        if (PublishedFileId != PublishedFileId_t.Invalid)
        {
            Uri.TryCreate(ShowModUpdates.SteamBaseChangelogUri, PublishedFileId.ToString(), out SteamChangelog);
            Uri.TryCreate(ShowModUpdates.SteamBaseUri, PublishedFileId.ToString(), out SteamUri);
        }

        if (!string.IsNullOrEmpty(ModMetaData.Url))
        {
            Uri.TryCreate(ModMetaData.Url, UriKind.Absolute, out OtherUrl);
        }

        if (string.IsNullOrEmpty(Description))
        {
            Description = ModMetaData.Description;
        }

        if (string.IsNullOrEmpty(Description))
        {
            return;
        }

        var urlsInDescription = Regex.Matches(Description,
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