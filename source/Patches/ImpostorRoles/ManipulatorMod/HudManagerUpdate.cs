using HarmonyLib;
using TownOfUsEdited.Roles;
using UnityEngine;
using TownOfUsEdited.Extensions;
using System.Linq;
using TMPro;
using TownOfUsEdited.Roles.Modifiers;

namespace TownOfUsEdited.Patches.ImpostorRoles.ManipulatorMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class HudManagerUpdate
    {
        public static Sprite ManipulateSprite => TownOfUsEdited.ManipulateSprite;
        public static void Postfix(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Manipulator)) return;

            var role = Role.GetRole<Manipulator>(PlayerControl.LocalPlayer);

            if (role.ManipulateButton == null)
            {
                role.ManipulateButton = Object.Instantiate(__instance.KillButton, __instance.KillButton.transform.parent);
                role.ManipulateButton.graphic.enabled = true;
                role.ManipulateButton.gameObject.SetActive(false);
                role.ManipulateButton.graphic.sprite = ManipulateSprite;
                role.ManipulateText = Object.Instantiate(__instance.KillButton.buttonLabelText, role.ManipulateButton.transform);
                role.ManipulateText.gameObject.SetActive(false);
                role.ButtonLabels.Add(role.ManipulateText);
            }

            // Check if the game state allows the KillButton to be active
            bool isKillButtonActive = __instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled;
            isKillButtonActive = isKillButtonActive && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead;
            isKillButtonActive = isKillButtonActive && (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started ||
            AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay);

            role.ManipulateButton.graphic.sprite = ManipulateSprite;

            role.ManipulateButton.gameObject.SetActive(isKillButtonActive);
            role.ManipulateButton.transform.localPosition = new Vector3(-2f, 1f, 0f);

            role.ManipulateText.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started ||
                    AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay));

            role.ManipulateText.SetOutlineColor(Palette.ImpostorRed);

            var notimp = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Is(Faction.Impostors)).ToList();

            var renderer = role.ManipulateButton.graphic;
            var labelrender = role.ManipulateText;
            if (role.ManipulatedPlayer == null)
            {
                role.ManipulateText.text = "Manipulate";
                var killButton = role.ManipulateButton;
                if ((CamouflageUnCamouflage.IsCamoed && CustomGameOptions.CamoCommsKillAnyone) || PlayerControl.LocalPlayer.IsHypnotised()) Utils.SetTarget(ref role.ClosestPlayer, killButton, float.NaN, PlayerControl.AllPlayerControls.ToArray().Where(x => (!x.isDummy || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) && !x.IsManipulated()).ToList());
                else if (PlayerControl.LocalPlayer.IsLover() && CustomGameOptions.ImpLoverKillTeammate) Utils.SetTarget(ref role.ClosestPlayer, killButton, float.NaN, PlayerControl.AllPlayerControls.ToArray().Where(x => !x.IsLover() && (!x.isDummy || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) && !x.IsManipulated()).ToList());
                else if (PlayerControl.LocalPlayer.IsLover() && !CustomGameOptions.MadmateKillEachOther) Utils.SetTarget(ref role.ClosestPlayer, killButton, float.NaN, PlayerControl.AllPlayerControls.ToArray().Where(x => !x.IsLover() && !x.Is(Faction.Impostors) && !x.Is(Faction.Madmates) && (!x.isDummy || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) && !x.IsManipulated()).ToList());
                else if (PlayerControl.LocalPlayer.IsLover()) Utils.SetTarget(ref role.ClosestPlayer, killButton, float.NaN, PlayerControl.AllPlayerControls.ToArray().Where(x => !x.IsLover() && !x.Is(Faction.Impostors) && (!x.isDummy || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) && !x.IsManipulated()).ToList());
                else if (!CustomGameOptions.MadmateKillEachOther) Utils.SetTarget(ref role.ClosestPlayer, killButton, float.NaN, PlayerControl.AllPlayerControls.ToArray().Where(x => !x.IsLover() && !x.Is(Faction.Impostors) && !x.Is(Faction.Madmates) && (!x.isDummy || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) && !x.IsManipulated()).ToList());
                else Utils.SetTarget(ref role.ClosestPlayer, killButton, float.NaN, PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Is(Faction.Impostors) && (!x.isDummy || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) && !x.IsManipulated()).ToList());

                if (role.ClosestPlayer != null)
                {
                    labelrender.color = Palette.EnabledColor;
                    labelrender.material.SetFloat("_Desat", 0f);
                }
                else
                {
                    labelrender.color = Palette.DisabledClear;
                    labelrender.material.SetFloat("_Desat", 1f);
                }
            }
            else if (!role.ManipulateButton.isCoolingDown || role.UsingManipulation)
            {
                role.ManipulateText.text = "Control";
                renderer.color = Palette.EnabledColor;
                renderer.material.SetFloat("_Desat", 0f);
                labelrender.color = Palette.EnabledColor;
                labelrender.material.SetFloat("_Desat", 0f);
            }
            else
            {
                role.ManipulateText.text = "Control";
                renderer.color = Palette.DisabledClear;
                renderer.material.SetFloat("_Desat", 1f);
                labelrender.color = Palette.DisabledClear;
                labelrender.material.SetFloat("_Desat", 1f);
            }

            if (role.ClosestPlayer != null)
            {
                role.ClosestPlayer.myRend().material.SetColor("_OutlineColor", Palette.ImpostorRed);
            }

            if (role.UsingManipulation) role.ManipulateButton.SetCoolDown(role.TimeRemaining, CustomGameOptions.ManipulationDuration);
            else role.ManipulateButton.SetCoolDown(role.ManipulateTimer(), CustomGameOptions.ManipulateCD);

            role.ManipulateButton.graphic.SetCooldownNormalizedUvs();
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class PatchManipulate
    {
        public static void Postfix(HudManager __instance)
        {
            var manipulators = Role.AllRoles.Where(x => x.RoleType == RoleEnum.Manipulator && x.Player != null).Cast<Manipulator>();
            foreach (var role in manipulators)
            {
                if (!MeetingHud.Instance && role.ManipulatedPlayer != null && !role.ManipulatedPlayer.Data.IsDead
                && !role.ManipulatedPlayer.Data.Disconnected && role.IsManipulating == true)
                {
                    if (role.Player == PlayerControl.LocalPlayer)
                    {
                        PlayerControl.LocalPlayer.moveable = false;
                        Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(role.ManipulatedPlayer);
                        var light = PlayerControl.LocalPlayer.lightSource;
                        light.transform.SetParent(role.ManipulatedPlayer.transform);
                        light.transform.localPosition = role.ManipulatedPlayer.Collider.offset;
                    }
                    if (role.ManipulatedPlayer == PlayerControl.LocalPlayer)
                    {
                        PlayerControl.LocalPlayer.moveable = false;
                        HudManager.Instance.TaskCompleteOverlay.gameObject.GetComponentInChildren<TextMeshPro>().text = "<color=#FF0000>You are being Controlled!</color>";
                        HudManager.Instance.TaskCompleteOverlay.gameObject.transform.localPosition = new Vector3(0f, 0f, 1f);
                        HudManager.Instance.TaskCompleteOverlay.gameObject.SetActive(true);
                        try
                        {
                            Minigame.Instance.Close();
                            Minigame.Instance.Close();
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }
    }

    // Code from Stellar Roles, link: https://github.com/Mr-Fluuff/StellarRolesAU/blob/main/StellarRoles/Roles/Impostor/Parasite/ParasiteAbilities.cs
    public static class ManipulatorMovementPatches
    {
        public static Vector2 manipVector = Vector2.zero;

        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
        public class ManipulatedPlayerKeyboardUpdate
        {
            public static bool Prefix(KeyboardJoystick __instance)
            {
                if (!PlayerControl.LocalPlayer.Is(RoleEnum.Manipulator)) return true;
                var role = Role.GetRole<Manipulator>(PlayerControl.LocalPlayer);
                if (!role.UsingManipulation) return true;
                if (Utils.Rewinding()) return true;
                manipVector.x = 0f;
                manipVector.y = 0f;
                if (manipVector == Vector2.zero)
                {
                    if (KeyboardJoystick.player.GetButton(40))
                    {
                        manipVector.x = manipVector.x + 1;
                    }
                    if (KeyboardJoystick.player.GetButton(39))
                    {
                        manipVector.x = manipVector.x - 1;
                    }
                    if (KeyboardJoystick.player.GetButton(44))
                    {
                        manipVector.y = manipVector.y + 1;
                    }
                    if (KeyboardJoystick.player.GetButton(42))
                    {
                        manipVector.y = manipVector.y - 1;
                    }
                }
                manipVector.Normalize();
                if (Input.GetKeyDown(KeyCode.Escape))
		        {
			        if (Minigame.Instance)
                    {
                        Minigame.Instance.Close();
                    }
                    else if (DestroyableSingleton<HudManager>.InstanceExists && MapBehaviour.Instance && MapBehaviour.Instance.IsOpen)
                    {
                        MapBehaviour.Instance.Close();
                    }
                    else if (PlayerCustomizationMenu.Instance)
                    {
                        PlayerCustomizationMenu.Instance.Close(true);
                    }
                    else if (GameSettingMenu.Instance)
                    {
                        GameSettingMenu.Instance.Close();
                    }
                }

                if (role.ManipulatedPlayer != null && role.UsingManipulation && !role.ManipulatedPlayer.Data.Disconnected)
                {
                    var modifiers = Modifier.GetModifiers(role.ManipulatedPlayer);
                    var speedFactor = 1.0f;
                    if (modifiers != null && modifiers.Any())
                    {
                        var modifier = modifiers.FirstOrDefault(x => x is IVisualAlteration);
                        if (modifier is IVisualAlteration alteration)
                        {
                            alteration.TryGetModifiedAppearance(out VisualAppearance appearance);
                            speedFactor = appearance.SpeedFactor;
                        }
                    }
                    var vel = manipVector * role.ManipulatedPlayer.MyPhysics.TrueSpeed * speedFactor;
                    role.ManipulatedPlayer.MyPhysics.body.velocity = vel;
                    Utils.Rpc(CustomRPC.SyncManipMovement, role.ManipulatedPlayer.PlayerId, vel.x, vel.y);
                }
                
                return false;
            }
        }

        [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.FixedUpdate))]
        public class ServerPatch
        {
            public static bool Prefix(CustomNetworkTransform __instance)
            {
                if (!PlayerControl.LocalPlayer.Is(RoleEnum.Manipulator)) return true;
                var role = Role.GetRole<Manipulator>(PlayerControl.LocalPlayer);
                if (Utils.Rewinding())
                {
                    if (role.UsingManipulation && __instance.myPlayer == role.ManipulatedPlayer) __instance.SetPaused(false);
                    return true;
                }
                if (role.ManipulatedPlayer != null && role.ManipulatedPlayer == __instance.myPlayer
                && role.UsingManipulation)
                {
                    __instance.SetPaused(true);
                    return false;
                }
                __instance.SetPaused(false);
                return true;
            }
        }
    }
}