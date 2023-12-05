using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NCMS;
using NCMS.Utils;
using NeoModLoader.api;
using NeoModLoader.services;

namespace NeoModLoader.ncms_compatible_layer
{
    internal static class NCMSCompatibleLayer
    {
        public const string modGlobalObject = @"
                                        using System;
                                        using System.IO;
                                        using System.Reflection;
                                        using UnityEngine;
                                        using UnityEngine.Events;
                                        using UnityEngine.UI;

                                        internal class Mod
                                        {
                                            public static ModDeclaration.Info Info;
                                            public static GameObject GameObject;
                                            public static Action OnDebug;

                                            private static int debugClicked = 0;
                                            
                                            public static void Initialize(Button button)
                                            {
                                                OnDebug += new Action(() => {
                                                    Debug.Log($""Debug toggled for mod {Info.Name}"");
                                                });

                                                button.onClick.AddListener(new UnityAction(() =>
                                                {
                                                    if(debugClicked < 10)
                                                    {
                                                        debugClicked++;
                                                        return;
                                                    }
                                                    
                                                    OnDebug();
                                                }));
                                            }
                                            
                                            public class EmbededResources
                                            {
                                                public static Sprite LoadSprite(string name, float pivotX = 0, float pivotY = 0, float pixelsPerUnit = 1f)
                                                {
                                                    //Assembly myAssembly = Assembly.GetExecutingAssembly();
                                                    //Stream myStream = myAssembly.GetManifestResourceStream(name);

                                                    byte[] data = GetBytes(name);
                                                    Texture2D texture2D = new Texture2D(1, 1);
                                                    texture2D.anisoLevel = 0;
                                                    texture2D.LoadImage(data);
                                                    texture2D.filterMode = FilterMode.Point;
                                                    return Sprite.Create(texture2D, new Rect(0.0f, 0.0f, (float)texture2D.width, (float)texture2D.height), new Vector2(pivotX, pivotX), pixelsPerUnit);
                                                }

                                                public static byte[] GetBytes(string name)
                                                {
                                                    Assembly myAssembly = Assembly.GetExecutingAssembly();
                                                    Stream myStream = myAssembly.GetManifestResourceStream(name);

                                                    return ReadFully(myStream);
                                                }

                                                internal static byte[] ReadFully(Stream input)
                                                {
                                                    byte[] buffer = new byte[16 * 1024];
                                                    using (MemoryStream ms = new MemoryStream())
                                                    {
                                                        int read;
                                                        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                                                        {
                                                            ms.Write(buffer, 0, read);
                                                        }
                                                        return ms.ToArray();
                                                    }
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
}