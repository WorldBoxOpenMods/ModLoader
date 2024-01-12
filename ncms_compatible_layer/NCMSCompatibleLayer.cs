using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NCMS;
using NCMS.Utils;
using NeoModLoader.api;
using NeoModLoader.services;

namespace NeoModLoader.ncms_compatible_layer
{
#pragma warning disable CS0618
    internal static class NCMSCompatibleLayer
    {
        /// <summary>
        ///     An improved variant of mod global object in [NCMS](https://denq04.github.io/ncms/)
        /// </summary>
        public const string modGlobalObject = @"
    using System;
    using System.IO;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;
    using NeoModLoader.services;
    using System.Collections.Generic;


    internal class Mod
    {
        public static ModDeclaration.Info Info;
        public static GameObject GameObject;
        public static Action OnDebug;

        private static int debugClicked = 0;

        public static void Initialize(Button button)
        {
            OnDebug += new Action(() => { LogService.LogInfo($""Debug toggled for mod {Info.Name}""); });

            button.onClick.AddListener(new UnityAction(() =>
            {
                if (debugClicked < 10)
                {
                    debugClicked++;
                    return;
                }

                OnDebug();
            }));
        }

        public class EmbededResources
        {
            private static Assembly this_assembly = Assembly.GetExecutingAssembly();

            public static Sprite LoadSprite(string name, float pivotX = 0, float pivotY = 0, float pixelsPerUnit = 1f)
            {
                string hash = $""{name}-{pivotX}-{pivotY}-{pixelsPerUnit}"";
                if (sprite_cache.TryGetValue(hash, out var sprite))
                    return sprite;
                Texture2D texture2D = new Texture2D(0, 0);
                texture2D.LoadImage(GetBytes(name));
                texture2D.anisoLevel = 0;
                texture2D.filterMode = FilterMode.Point;
                sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, (float)texture2D.width, (float)texture2D.height),
                    new Vector2(pivotX, pivotY), pixelsPerUnit);
                sprite_cache.Add(hash, sprite);
                return sprite;
            }

            private static Dictionary<string, Sprite> sprite_cache = new();

            public static byte[] GetBytes(string name)
            {
                return ReadFully(this_assembly.GetManifestResourceStream(name));
            }

            internal static byte[] ReadFully(Stream input)
            {
                using var ms = new MemoryStream();
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }";

        public static void PreInit()
        {
            Windows.init();
            ResourcesPatch.modsResources ??= utils.ResourcesPatch.GetAllPatchedResources();
        }

        public static void Init()
        {
            NCMS.ModLoader.Mods ??= new();
            foreach (IMod mod in WorldBoxMod.LoadedMods)
            {
                ModDeclare declare = mod.GetDeclaration();
                NCMS.ModLoader.Mods.Add(GenerateNCMSMod(declare));
            }

            LogService.LogInfo($"NCMS Compatible Layer has been initialized.");
        }

        public static NCMod GenerateNCMSMod(ModDeclare modDeclare)
        {
            return new NCMod()
            {
                author = modDeclare.Author,
                description = modDeclare.Description,
                iconPath = modDeclare.IconPath,
                name = modDeclare.Name,
                path = modDeclare.FolderPath,
                version = modDeclare.Version,
                targetGameBuild = modDeclare.TargetGameBuild
            };
        }

        public static bool IsNCMSMod(SyntaxTree syntaxTree)
        {
            var root = syntaxTree.GetCompilationUnitRoot();
            foreach (var classdecl in root.DescendantNodes())
            {
                if (classdecl is not ClassDeclarationSyntax classDeclarationSyntax) continue;
                if (classDeclarationSyntax.AttributeLists.Any(a =>
                        a.Attributes.Any(a => a.Name.ToString().Contains("ModEntry"))))
                    return true;
            }

            return false;
        }
    }
#pragma warning restore
}