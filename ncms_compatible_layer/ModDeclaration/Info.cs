using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCMS;
using NeoModLoader.constants;
using UnityEngine;

namespace ModDeclaration
{
    [Obsolete("Compatible Layer will not be maintained and be removed in the future")]
    public class Info
    {
        public static readonly string DataPath = Application.dataPath;

        public static readonly string ModsPath = Info.DataPath + "/StreamingAssets/Mods";

        public static readonly string NCMSPath = Info.ModsPath + "/NCMS";

        public static readonly string NCMSModsPath = Paths.ModsPath;

        public readonly string Name;

        public readonly string Author;

        public readonly string Version;

        public readonly string Description;

        public readonly string IconPath;

        public readonly string Path;
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
