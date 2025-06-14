using Il2CppInterop.Runtime;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace TownOfUsEdited.Patches
{
    public class AssetLoader
    {
        public static string[] AssetBundles = { "trappershader", "soundvision", "touebundle" };

        public AssetLoader() { Initialize(); }

        public void Initialize()
        {
            Array.ForEach(AssetBundles, x =>
            {
                var b = loadBundle(x);
                bundles.Add(b.name, b);
                b.GetAllAssetNames().ToList().ForEach(y =>
                {
                    objectname_to_bundle.Add(ConvertToBaseName(y), x);
                });
            });
        }

        private static AssetBundle loadBundle(string bundlename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream($"TownOfUsEdited.Resources.{bundlename}");
            var assets = stream.ReadFully();
            return AssetBundle.LoadFromMemory(assets);
        }

        private string ConvertToBaseName(string name)
        {
            return name.Split('/').Last().Split('.').First();
        }

        private T? LoadAsset<T>(AssetBundle assetBundle, string name) where T : UnityObject
        {
            var asset = assetBundle.LoadAsset(name, Il2CppType.Of<T>())?.Cast<T>().DontUnload();
            loadedObjects.Add(name, asset);
            return asset;
        }

        public static Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();
        private Dictionary<string, string> objectname_to_bundle = new Dictionary<string, string>();
        private Dictionary<string, UnityObject> loadedObjects = new Dictionary<string, UnityObject>();

        public static RuntimeAnimatorController LoadController(AssetBundle bundle, string name)
        {
            return bundle.LoadAsset<RuntimeAnimatorController>(name);
        }

        public static AnimationClip LoadAnimationClip(AssetBundle bundle, string name)
        {
            return bundle.LoadAsset<AnimationClip>(name);
        }

        public static TMPro.TMP_SpriteAsset LoadSpriteAsset(AssetBundle bundle, string name)
        {
            return bundle.LoadAsset<TMPro.TMP_SpriteAsset>(name);
        }

        public T Get<T>(string name) where T : UnityObject
        {
            if (loadedObjects.TryGetValue(name, out var obj)) return (T)obj;
            if (objectname_to_bundle.TryGetValue(name.ToLower(), out var obj2))
            {
                return LoadAsset<T>(bundles[obj2], name);
            }
            return null;
        }
    }
}