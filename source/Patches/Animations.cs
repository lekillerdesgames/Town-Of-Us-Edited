using System;
using System.Collections;
using System.Linq;
using HarmonyLib;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUsEdited.Extensions;
using TownOfUsEdited.Roles;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using AmongUs.GameOptions;
using System.Collections.Generic;
using AmongUs.Data;
using TownOfUsEdited.Patches.ImpostorRoles;
using TownOfUsEdited.Patches.NeutralRoles.VampireMod;
using TownOfUsEdited.Patches.NeutralRoles.SerialKillerMod;
using TownOfUsEdited.Patches.CovenRoles;
using TownOfUsEdited.Patches.Modifiers.LoversMod;

namespace TownOfUsEdited.Patches
{
    [HarmonyPatch]
    public class Animations
    {
        [HarmonyPatch(typeof(ShapeshifterPanel), nameof(ShapeshifterPanel.SetPlayer))]
        public static class FixNameplate
        {
            public static bool Prefix(ShapeshifterPanel __instance, [HarmonyArgument(0)] int index, [HarmonyArgument(1)] NetworkedPlayerInfo playerInfo,
            [HarmonyArgument(2)] Il2CppSystem.Action onShift)
            {
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return true;
                __instance.shapeshift = onShift;
                __instance.PlayerIcon.SetFlipX(false);
                __instance.PlayerIcon.ToggleName(false);
                SpriteRenderer[] componentsInChildren = __instance.GetComponentsInChildren<SpriteRenderer>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].material.SetInt(PlayerMaterial.MaskLayer, index + 2);
                }
                __instance.PlayerIcon.SetMaskLayer(index + 2);
                __instance.PlayerIcon.UpdateFromEitherPlayerDataOrCache(playerInfo, PlayerOutfitType.Default, PlayerMaterial.MaskType.ComplexUI, false, null);
                __instance.LevelNumberText.text = ProgressionManager.FormatVisualLevel(playerInfo.PlayerLevel);
                __instance.NameText.text = playerInfo.PlayerName;
                DataManager.Settings.Accessibility.OnColorBlindModeChanged += (Action)__instance.SetColorblindText;
                __instance.SetColorblindText();
                return false;
            }
        }
        public class AnimationMenu
        {
            public ShapeshifterMinigame Menu;
            public Select Click;
            public static AnimationMenu singleton;
            public delegate void Select(string anim);

            public AnimationMenu(Select click)
            {
                Click = click;
                if (singleton != null)
                {
                    singleton.Menu.DestroyImmediate();
                    singleton = null;
                }
                singleton = this;
            }

            public IEnumerator Open(float delay)
            {
                yield return new WaitForSecondsRealtime(delay);
                while (ExileController.Instance != null) { yield return 0; }
                if (Menu == null)
                {
                    if (Camera.main == null)
                        yield break;

                    Menu = GameObject.Instantiate(GetShapeshifterMenu(), Camera.main.transform, false);
                }

                Menu.transform.SetParent(Camera.main.transform, false);
                Menu.transform.localPosition = new(0f, 0f, -50f);
                Menu.Begin(null);
            }

            private static ShapeshifterMinigame GetShapeshifterMenu()
            {
                var rolePrefab = RoleManager.Instance.AllRoles.First(r => r.Role == RoleTypes.Shapeshifter);
                return GameObject.Instantiate(rolePrefab?.Cast<ShapeshifterRole>(), GameData.Instance.transform).ShapeshifterMenu;
            }

            public void Clicked(string anim)
            {
                Click(anim);
                Menu.Close();
            }

            [HarmonyPatch(typeof(ShapeshifterMinigame), nameof(ShapeshifterMinigame.OnDisable))]
            public static class ClosePatch
            {
                public static void Postfix()
                {
                    if (singleton == null) return;
                    singleton.Menu.DestroyImmediate();
                    singleton = null;
                }
            }

            [HarmonyPatch(typeof(ShapeshifterMinigame), nameof(ShapeshifterMinigame.Begin))]
            public static class MenuPatch
            {
                public static bool Prefix(ShapeshifterMinigame __instance)
                {
                    var menu = AnimationMenu.singleton;

                    if (menu == null)
                        return true;

                    var panels = Object.FindObjectsOfType<ShapeshifterPanel>();

                    if (panels.Count > 0)
                    {
                        foreach (var panel in panels)
                        {
                            if (panel != null)
                            {
                                panel.gameObject.DestroyImmediate();
                            }
                        }
                    }

                    __instance.potentialVictims = new();
                    var list2 = new Il2CppSystem.Collections.Generic.List<UiElement>();
                    List<string> Animations = new List<string> { "Bean Dance", "Wave", "Roll", "Sleepy" };

                    for (var i = 0; i < Animations.Count; i++)
                    {
                        var anim = Animations[i];
                        string name = PlayerControl.LocalPlayer.Data.PlayerName;
                        PlayerControl.LocalPlayer.Data.PlayerName = anim;
                        var num = i % 3;
                        var num2 = i / 3;
                        var panel = GameObject.Instantiate(__instance.PanelPrefab, __instance.transform);
                        panel.transform.localPosition = new(__instance.XStart + (num * __instance.XOffset), __instance.YStart + (num2 * __instance.YOffset), -1f);
                        panel.SetPlayer(i, PlayerControl.LocalPlayer.Data, (Action)(() => menu.Clicked(anim)));
                        __instance.potentialVictims.Add(panel);
                        list2.Add(panel.Button);
                        PlayerControl.LocalPlayer.Data.PlayerName = name;
                    }

                    ControllerManager.Instance.OpenOverlayMenu(__instance.name, __instance.BackButton, __instance.DefaultButtonSelected, list2);
                    return false;
                }
            }
            [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
            public static class StartMeeting
            {
                public static void Prefix(PlayerControl __instance)
                {
                    if (__instance == null) return;
                    try
                    {
                        AnimationMenu.singleton.Menu.Close();
                    }
                    catch { }
                }
            }
        }
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public class OnPlayerJoined
        {
            public static void Postfix()
            {
                if (!PlayerControl.LocalPlayer) return;
                var animObject = GameObject.Find($"Animation Object {PlayerControl.LocalPlayer.PlayerId}");
                if (animObject == null) return;
                var controller = animObject.GetComponent<Animator>().runtimeAnimatorController;
                if (controller == null) return;
                string anim = "None";
                bool hidePlayer = true;
                if (controller == boingController) anim = "Bean Dance";
                else if (controller == rollController) anim = "Roll";
                else if (controller == sleepyController) anim = "Sleepy";
                else if (controller == waveController)
                {
                    anim = "Wave";
                    hidePlayer = false;
                }
                Utils.Rpc(CustomRPC.Animate, anim, PlayerControl.LocalPlayer.PlayerId, hidePlayer);
            }
        }
        public static RuntimeAnimatorController boingController;
        public static RuntimeAnimatorController waveController;
        public static RuntimeAnimatorController rollController;
        public static RuntimeAnimatorController sleepyController;
        public static IEnumerator AnimatePlayer(RuntimeAnimatorController controller, PlayerControl player, bool hidePlayer = true)
        {
            if (boingController == null) boingController = AssetLoader.LoadController(AssetLoader.bundles.FirstOrDefault(x => x.Key == "touebundle").Value, "Boing_0.controller");
            if (sleepyController == null) sleepyController = AssetLoader.LoadController(AssetLoader.bundles.FirstOrDefault(x => x.Key == "touebundle").Value, "sleepy_0.controller");
            if (waveController == null) waveController = AssetLoader.LoadController(AssetLoader.bundles.FirstOrDefault(x => x.Key == "touebundle").Value, "wave_0.controller");
            if (rollController == null) rollController = AssetLoader.LoadController(AssetLoader.bundles.FirstOrDefault(x => x.Key == "touebundle").Value, "roll_0.controller");
            if (GameObject.Find($"Animation Object {player.PlayerId}")) StopAllAnimations(player); // Already animating
            GameObject _object = new GameObject($"Animation Object {player.PlayerId}");
            if (hidePlayer)
            {
                player.cosmetics.currentBodySprite.Visible = false;
                player.cosmetics.SetBodyCosmeticsVisible(false);
            }
            player.NetTransform.Halt();
            Vector2 scale = new Vector2(0.6f, 0.6f);
            if (controller == waveController) scale = new Vector2(0.7f, 0.7f);
            else if (controller == sleepyController) scale = new Vector2(0.4f, 0.4f);
            _object.transform.localScale *= scale;
            _object.gameObject.AddComponent<SpriteRenderer>().material = new Material(Shader.Find("Unlit/PlayerShader"));
            PlayerMaterial.SetColors(player.Data.DefaultOutfit.ColorId, _object.gameObject.GetComponent<SpriteRenderer>());
            _object.gameObject.GetComponent<SpriteRenderer>().flipX = player.myRend().flipX;
            _object.gameObject.AddComponent<Animator>().runtimeAnimatorController = controller;
            _object.gameObject.SetActive(true);
            while (true)
            {
                FollowerCamera cam = Camera.main.GetComponent<FollowerCamera>();
                if (_object == null) yield break;
                if (!PlayerControl.LocalPlayer || player == null || player.Data == null || player.Data.IsDead || MeetingHud.Instance || player.Data.Disconnected
                || player.MyPhysics.body.velocity != Vector2.zero || AmongUsClient.Instance.IsGameOver) // Didn't work when using player.isKilling for some reason
                {
                    if (player && !player.Data.Disconnected && hidePlayer)
                    {
                        player.cosmetics.currentBodySprite.Visible = true;
                        player.cosmetics.SetBodyCosmeticsVisible(true);
                    }
                    _object.gameObject.DestroyImmediate();
                    yield break;
                }
                Vector3 pos = player.transform.localPosition;
                float addX = 0f;
                float addY = 0f;
                if (controller == waveController)
                {
                    addY = 0.2f;
                    if (!player.myRend().flipX) addX += 0.2f;
                    else addX -= 0.2f;
                }
                _object.gameObject.transform.localPosition = new Vector3(pos.x + addX, pos.y + addY, pos.z);
                yield return null;
            }
        }

        public static void StopAllAnimations(PlayerControl player)
        {
            if (GameObject.Find($"Animation Object {player.PlayerId}")) // Already animating
            {
                var usedController = GameObject.Find($"Animation Object {player.PlayerId}").GetComponent<Animator>().runtimeAnimatorController;
                bool playerHidden = usedController == boingController || usedController == rollController || sleepyController;
                if (playerHidden)
                {
                    player.cosmetics.currentBodySprite.Visible = true;
                    player.cosmetics.SetBodyCosmeticsVisible(true);
                }
                GameObject.Find($"Animation Object {player.PlayerId}").gameObject.DestroyImmediate();
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class HudUpdate
        {
            private static GameObject AnimationButton;
            public static void Postfix(HudManager __instance)
            {
                if (boingController == null) boingController = AssetLoader.LoadController(AssetLoader.bundles.FirstOrDefault(x => x.Key == "touebundle").Value, "Boing_0.controller");
                if (sleepyController == null) sleepyController = AssetLoader.LoadController(AssetLoader.bundles.FirstOrDefault(x => x.Key == "touebundle").Value, "sleepy_0.controller");
                if (waveController == null) waveController = AssetLoader.LoadController(AssetLoader.bundles.FirstOrDefault(x => x.Key == "touebundle").Value, "wave_0.controller");
                if (rollController == null) rollController = AssetLoader.LoadController(AssetLoader.bundles.FirstOrDefault(x => x.Key == "touebundle").Value, "roll_0.controller");
                if (!PlayerControl.LocalPlayer)
                {
                    if (AnimationButton != null) AnimationButton.gameObject.DestroyImmediate();
                }
                if (!AnimationButton)
                {
                    AnimationButton = Object.Instantiate(__instance.MapButton.gameObject, __instance.MapButton.transform.parent);
                    AnimationButton.GetComponent<PassiveButton>().OnClick = new();
                    AnimationButton.GetComponent<PassiveButton>().OnClick.AddListener(new Action(AnimationMenu)); // Soon
                    AnimationButton.name = "Animation Button";
                }
                bool hasCustomChat;
                bool hasLoverChat;
                if (Role.GetRole(PlayerControl.LocalPlayer) == null)
                {
                    hasCustomChat = false;
                    hasLoverChat = false;
                }
                else
                {
                    hasCustomChat = ((PlayerControl.LocalPlayer.Is(RoleEnum.Vampire) && VampireChat.VampireChatButton != null && VampireChat.VampireChatButton.isActiveAndEnabled) ||
                    (PlayerControl.LocalPlayer.Is(RoleEnum.SerialKiller) && SerialKillerChat.SerialKillerChatButton != null && SerialKillerChat.SerialKillerChatButton.isActiveAndEnabled) ||
                    (PlayerControl.LocalPlayer.Is(Faction.Coven) && CovenChat.CovenChatButton != null && CovenChat.CovenChatButton.isActiveAndEnabled) ||
                    (PlayerControl.LocalPlayer.Is(Faction.Impostors) && ImpostorChat.ImpostorChatButton != null && ImpostorChat.ImpostorChatButton.isActiveAndEnabled))
                    && AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay;
                    hasLoverChat = PlayerControl.LocalPlayer.IsLover() && LoversChat.LoversChatButton != null && LoversChat.LoversChatButton.isActiveAndEnabled
                    && AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay;
                }
                Vector3 Pos;
                var chatPos = HudManager.Instance.Chat.chatButton.transform.localPosition;
                if ((hasCustomChat && !hasLoverChat) || (hasLoverChat && !hasCustomChat) || HudManager.Instance.Chat.isActiveAndEnabled) Pos = new Vector3(chatPos.x - 0.9f, chatPos.y + 0.18f, chatPos.z);
                else if ((hasCustomChat || HudManager.Instance.Chat.isActiveAndEnabled) && hasLoverChat) Pos = new Vector3(chatPos.x - 1.75f, chatPos.y + 0.18f, chatPos.z);
                else Pos = new Vector3(chatPos.x - 0.31f, chatPos.y + 0.18f, chatPos.z);
                AnimationButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite = TownOfUsEdited.AnimationButton;
                AnimationButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite = TownOfUsEdited.AnimationButtonActive;
                AnimationButton.transform.Find("Active").GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Unlit/PlayerShader"));
                PlayerMaterial.SetColors(PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId, AnimationButton.transform.Find("Active").GetComponent<SpriteRenderer>());
                AnimationButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Unlit/PlayerShader"));
                PlayerMaterial.SetColors(PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId, AnimationButton.transform.Find("Inactive").GetComponent<SpriteRenderer>());
                AnimationButton.SetActive(PlayerControl.LocalPlayer && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                && PlayerControl.LocalPlayer.CanMove && !HudManager.Instance.IsIntroDisplayed);
                AnimationButton.transform.Find("Background").localPosition = Vector3.zero;
                AnimationButton.transform.localPosition = Pos;
            }

            public static void AnimationMenu()
            {
                var menu = new AnimationMenu((x) =>
                {
                    RuntimeAnimatorController Controller = null;
                    bool hidePlayer = true;
                    if (x == "Bean Dance") Controller = boingController;
                    else if (x == "Roll") Controller = rollController;
                    else if (x == "Sleepy") Controller = sleepyController;
                    else if (x == "Wave")
                    {
                        Controller = waveController;
                        hidePlayer = false;
                    }
                    Coroutines.Start(AnimatePlayer(Controller, PlayerControl.LocalPlayer, hidePlayer));
                    Utils.Rpc(CustomRPC.Animate, x, PlayerControl.LocalPlayer.PlayerId, hidePlayer);
                });
                Coroutines.Start(menu.Open(0f));
            }
        }
    }
}