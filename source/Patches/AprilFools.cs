using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUsEdited.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUsEdited.Patches
{
    [HarmonyPatch]

    public static class AprilFoolsPatches
    {
        public static bool IsAprilFools()
        {
            try
            {
                DateTime utcNow = DateTime.UtcNow;
                DateTime t = new DateTime(utcNow.Year, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                DateTime t2 = new DateTime(utcNow.Year, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc);
                if (utcNow >= t && utcNow <= t2)
                {
                    return true;
                }
            }
            catch
            {
                // Whatever
            }
            return false;
        }
        public static int CurrentMode;
        public static Dictionary<byte, GameObject> Hehs = new Dictionary<byte, GameObject>();
        public static float OriginalFlipX = 0f;

        private static Dictionary<int, string> Modes = new()
        {
            {0, "Off"},
            {1, "Horse"},
            {2, "Long"},
            {3, "Long Horse"}
        };

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static void Postfix()
        {
            if (!IsAprilFools() || !PlayerControl.LocalPlayer || PlayerControl.AllPlayerControls.Count <= 0)
            {
                while (Hehs.Keys.Any())
                {
                    var heh1 = Hehs.FirstOrDefault();
                    if (heh1.Value != null) Object.Destroy(heh1.Value.gameObject);
                    Hehs.Remove(heh1.Key);
                }
                return;
            }
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.Disconnected)
                {
                    if (Hehs.TryGetValue(player.PlayerId, out GameObject hehObject2))
                    {
                        if (hehObject2 != null) Object.Destroy(hehObject2);
                        Hehs.Remove(player.PlayerId);
                    }
                    continue;
                }
                if (player == null || player.Data == null) continue;
                if (!Hehs.ContainsKey(player.PlayerId))
                {
                    var heh = new GameObject($"HEH{player.PlayerId}");
                    SpriteRenderer render = heh.AddComponent<SpriteRenderer>();
                    render.sprite = TownOfUsEdited.heh;
                    Hehs.Add(player.PlayerId, heh);
                    if (OriginalFlipX == 0f) OriginalFlipX = heh.transform.localScale.x;
                }
                Hehs.TryGetValue(player.PlayerId, out GameObject hehObject);
                var position = player.transform.localPosition;
                hehObject.transform.localPosition = new Vector3(position.x, position.y, -1f);
                var scale = hehObject.transform.localScale;
                bool flipped = player.cosmetics.currentBodySprite.BodySprite.flipX;
                if (flipped) hehObject.transform.localScale = new Vector3(OriginalFlipX * -1, scale.y, scale.z);
                else hehObject.transform.localScale = new Vector3(OriginalFlipX * 1, scale.y, scale.z);

                hehObject.SetActive(!player.Data.Disconnected && (!player.Data.IsDead || PlayerControl.LocalPlayer.Data.IsDead) &&
                player.nameText().color != Color.clear && !player.inVent);
            }
            foreach (var key in Hehs.Keys)
            {
                if (key > PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Data.Disconnected).ToList().Count - 1)
                {
                    Hehs.TryGetValue(key, out GameObject hehObject);
                    if (hehObject != null) Object.Destroy(hehObject);
                    Hehs.Remove(key);
                }
            }
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        [HarmonyPrefix]

        public static void Prefix(MainMenuManager __instance)
        {
            if (__instance.newsButton != null)
            {
                var aprilfoolstoggle = UnityEngine.Object.Instantiate(__instance.newsButton, null);
                aprilfoolstoggle.name = "aprilfoolstoggle";

                aprilfoolstoggle.transform.localScale = new Vector3(0.44f, 0.84f, 1f);

                PassiveButton passive = aprilfoolstoggle.GetComponent<PassiveButton>();
                passive.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();

                aprilfoolstoggle.gameObject.transform.SetParent(GameObject.Find("RightPanel").transform);
                var pos = aprilfoolstoggle.gameObject.AddComponent<AspectPosition>();
                pos.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
                pos.DistanceFromEdge = new Vector3(2.1f, 2f, 8f);

                passive.OnClick.AddListener((Action)(() =>
                {
                    int num = CurrentMode + 1;
                    CurrentMode = num > 3 ? 0 : num;
                    var text = aprilfoolstoggle.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>();
                    text.text = $"April fools mode: {Modes[CurrentMode]}";
                }));

                var text = aprilfoolstoggle.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>();
                __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
                {
                    text.text = $"April fools mode: {Modes[CurrentMode]}";
                    pos.AdjustPosition();
                })));

                aprilfoolstoggle.transform.GetChild(0).transform.localScale = new Vector3(aprilfoolstoggle.transform.localScale.x + 1, 1f, 1f);
                aprilfoolstoggle.transform.GetChild(0).transform.localPosition -= new Vector3(1.5f, 0f, 0f);
                aprilfoolstoggle.transform.GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().sprite = null;
                aprilfoolstoggle.transform.GetChild(2).GetChild(0).GetComponent<SpriteRenderer>().sprite = null;
                aprilfoolstoggle.GetComponent<NewsCountButton>().DestroyImmediate();
                aprilfoolstoggle.transform.GetChild(3).gameObject.DestroyImmediate();
            }
        }

        [HarmonyPatch(typeof(AprilFoolsMode), nameof(AprilFoolsMode.ShouldLongAround))]
        [HarmonyPrefix]

        public static bool Prefix(ref bool __result)
        {
            __result = CurrentMode == 2;
            return true;
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.SetBodyType))]
        [HarmonyPrefix]

        public static void Prefix(ref PlayerBodyTypes bodyType)
        {
            switch (CurrentMode)
            {
                case 1:
                    bodyType = PlayerBodyTypes.Horse;
                    break;
                case 2:
                    bodyType = PlayerBodyTypes.Long;
                    break;
                case 3:
                    bodyType = PlayerBodyTypes.LongSeeker;
                    break;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.BodyType), MethodType.Getter)]
        [HarmonyPrefix]

        public static bool Prefix2(ref PlayerBodyTypes __result)
        {
            switch (CurrentMode)
            {
                case 1:
                    __result = PlayerBodyTypes.Horse;
                    return false;
                case 2:
                    __result = PlayerBodyTypes.Long;
                    return false;
                case 3:
                    __result = PlayerBodyTypes.LongSeeker;
                    return false;
                default:
                    return true;
            }
        }
    }
}

