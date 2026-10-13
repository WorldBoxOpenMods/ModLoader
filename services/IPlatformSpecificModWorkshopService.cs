using NeoModLoader.api;
using RSG;

namespace NeoModLoader.services;

internal interface IPlatformSpecificModWorkshopService
{
    void UploadModLoader(string changelog);

    Promise UploadMod(string name, string description, string previewImagePath, string workshopPath, string changelog,
        bool verified, string aiTag = null, List<string> extraTags = null);

    Promise EditMod(ulong fileID, string previewImagePath, string workshopPath, string changelog,
        string aiTag = null, List<string> extraTags = null);
    void FindSubscribedMods();
    ModDeclare GetNextModFromWorkshopItem();
}