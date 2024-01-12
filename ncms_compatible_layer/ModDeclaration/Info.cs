using NCMS;
using NeoModLoader.constants;
using UnityEngine;

#pragma warning disable CS1591 // No comment for NCMS compatible layer
namespace ModDeclaration
{
    /// <remarks>
    ///     From [NCMS](https://denq04.github.io/ncms/)
    /// </remarks>
    [Obsolete("Compatible Layer will not be maintained and be removed in the future")]
    public class Info
    {
        public static readonly string DataPath = Application.dataPath;

        public static readonly string ModsPath = DataPath + "/StreamingAssets/Mods";

        public static readonly string NCMSPath = ModsPath + "/NCMS";

        public static readonly string NCMSModsPath = Paths.ModsPath;

        public readonly string Author;

        public readonly string Description;

        public readonly string IconPath;

        public readonly string Name;

        public readonly string Path;

        public readonly string Version;

        internal Info(NCMod mod)
        {
            this.Name = mod.name;
            this.Author = mod.author;
            this.Version = mod.version;
            this.Description = mod.description;
            this.IconPath = mod.iconPath;
            this.Path = mod.path;
        }
    }
}