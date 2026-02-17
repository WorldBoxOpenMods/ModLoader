using UnityEngine;
using MelonLoader;
using MelonLoader.Utils;
using NeoModLoader.constants;

// Required assembly attributes
[assembly: MelonInfo(typeof(NeoModLoader.NeoMelonLoader), "Neo Mod Loader", "1.0.0", "inmny and others")]
[assembly: MelonGame(null, null)]

namespace NeoModLoader
{
    public class NeoMelonLoader : MelonMod
    {
        private bool initlized = false;
        public override void OnUpdate()
        {
            if (initlized)
            {
                return;
            }
            initlized = true;
            GameObject NeoModLoader = new GameObject("NeoModLoader");
            NeoModLoader.AddComponent<WorldBoxMod>();
            Paths.MelonPath = MelonEnvironment.GameRootDirectory;
        }
    }
}

