using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCMS;
using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.services;
using NeoModLoader.utils;

namespace NeoModLoader.ncms_compatible_layer
{
    internal static class NCMSCompatibleLayer
    {
        public static void Init()
        {
            NCMS.ModLoader.Mods = new();
            foreach (IMod mod in WorldBoxMod.LoadedMods)
            {
                ModDeclare declare = mod.GetDeclaration();
                NCMS.ModLoader.Mods.Add(new NCMod()
                {
                    author = declare.Author,
                    description = declare.Description,
                    iconPath = declare.IconPath,
                    name = declare.Name,
                    path = declare.FolderPath,
                    version = declare.Version,
                    targetGameBuild = declare.TargetGameBuild
                });
            }

            NCMS.Utils.ResourcesPatch.modsResources = ResourcesPatch.GetAllPatchedResources();
            NCMS.Utils.Windows.init();
            
            LogService.LogInfo($"NCMS Compatible Layer has been initialized.");
        }
    }
}
