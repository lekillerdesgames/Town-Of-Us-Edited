using System.Linq;
using HarmonyLib;
using TownOfUsEdited.Extensions;
using TownOfUsEdited.Roles;
using UnityEngine;

namespace TownOfUsEdited.CovenRoles.CovenMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class CovenUpdate
    {
        public static KillButton SabotageButton;
        public static void Postfix(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            // Check if there is only one player or if local player is null or dead
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(Faction.Coven)) return;

            var role = Role.GetRole(PlayerControl.LocalPlayer);

            var alivecoven = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(Faction.Coven) && !x.Data.IsDead).ToList();

            foreach (var player2 in PlayerControl.AllPlayerControls)
            {
                // Add purple color to coven names
                if (player2.Is(Faction.Coven))
                {
                    player2.nameText().color = Patches.Colors.Coven;
                }
            }

            if (HudManager.Instance?.Chat != null)
            {
                // Add purple color to chat bubble
                foreach (var player3 in PlayerControl.AllPlayerControls)
                {
                    foreach (var bubble in HudManager.Instance.Chat.chatBubblePool.activeChildren)
                    {
                        if (bubble.Cast<ChatBubble>().NameText != null &&
                            player3.Data.PlayerName == bubble.Cast<ChatBubble>().NameText.text &&
                            player3.Is(Faction.Coven))
                        {
                            bubble.Cast<ChatBubble>().NameText.color = Patches.Colors.Coven;
                        }
                    }
                }
            }

            if (MeetingHud.Instance) UpdateMeeting(MeetingHud.Instance);

            if (SabotageButton == null)
            {
                SabotageButton = Object.Instantiate(__instance.KillButton, __instance.KillButton.transform.parent);
                SabotageButton.graphic.enabled = true;
                SabotageButton.gameObject.SetActive(false);
            }

            SabotageButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);
                    
            SabotageButton.graphic.sprite = TownOfUsEdited.SabotageCoven;

            SabotageButton.SetCoolDown(0f, 1f);

            SabotageButton.graphic.color = Palette.EnabledColor;
            SabotageButton.graphic.material.SetFloat("_Desat", 0f);

            if (PlayerControl.LocalPlayer.Is(RoleEnum.HexMaster)) return;

            if (!PlayerControl.LocalPlayer.Data.IsDead)
            {
                SabotageButton.transform.localPosition = new Vector3(-1f, 1f, 0f);
            }
            else
            {
                var position = __instance.KillButton.transform.localPosition;
                SabotageButton.transform.localPosition = new Vector3(position.x,
                position.y, position.z);
            }

            // Check if the game state allows the KillButton to be active
            bool isKillButtonActive = __instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled;
            isKillButtonActive = isKillButtonActive && !MeetingHud.Instance && !player.Data.IsDead;
            isKillButtonActive = isKillButtonActive && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started;

            // Set KillButton's visibility
            __instance.KillButton.gameObject.SetActive(isKillButtonActive);

            __instance.KillButton.gameObject.transform.localPosition = new Vector3(0f, 1f, 0f);

            // Set KillButton's cooldown
            __instance.KillButton.SetCoolDown(KillTimer(), CustomGameOptions.CovenKCD);

            PerformKill.SetTarget(__instance.KillButton);
        }

        public static void UpdateMeeting(MeetingHud __instance)
        {
            // Add purple color to coven names in meeting
            var covens = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(Faction.Coven)).ToList();
            foreach (PlayerVoteArea pva in __instance.playerStates)
            {
                if (covens.Any(x => x.PlayerId == pva.TargetPlayerId))
                {
                    pva.NameText.text = "<color=#bf5fff>" + pva.NameText.text + "</color>";
                }
            }
        }

        public static float KillTimer()
        {
            var role = Role.GetRole(PlayerControl.LocalPlayer);
            if (role == null) return 0f;

            if (!PlayerControl.LocalPlayer.coolingDown()) return 0f;
            else if (!PlayerControl.LocalPlayer.inVent)
            {
                role.KillCooldown -= Time.deltaTime;
                return role.KillCooldown;
            }
            else return role.KillCooldown;
        }
    }

    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKill
    {
        public static void SetTarget(KillButton killButton)
        {
            PlayerControl target = null;

            if (!PlayerControl.LocalPlayer.moveable) target = null;
            else if ((CamouflageUnCamouflage.IsCamoed && CustomGameOptions.CamoCommsKillAnyone) || PlayerControl.LocalPlayer.IsHypnotised()) Utils.SetTarget(ref target,killButton);
            else if (PlayerControl.LocalPlayer.IsLover() && CustomGameOptions.ImpLoverKillTeammate) Utils.SetTarget(ref target, killButton, float.NaN, PlayerControl.AllPlayerControls.ToArray().Where(x => !x.IsLover()).ToList());
            else if (PlayerControl.LocalPlayer.IsLover()) Utils.SetTarget(ref target, killButton, float.NaN, PlayerControl.AllPlayerControls.ToArray().Where(x => !x.IsLover() && !x.Is(Faction.Coven)).ToList());
            else Utils.SetTarget(ref target, killButton, float.NaN, PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Is(Faction.Coven)).ToList());
            killButton.SetTarget(target);
        }
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(KillButton __instance)
        {
            if (!PlayerControl.LocalPlayer.Is(Faction.Coven)) return true;

            var role = Role.GetRole(PlayerControl.LocalPlayer);
            var killbutton = DestroyableSingleton<HudManager>.Instance.KillButton;
                
            if (__instance == CovenUpdate.SabotageButton)
            {
                DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions
                {
                    Mode = MapOptions.Modes.Sabotage
                });
                return false;
            }

            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            
            if (__instance == killbutton)
            {
                if (PlayerControl.LocalPlayer.Is(RoleEnum.HexMaster)) return true;

                SetTarget(__instance);
                var target = __instance.currentTarget;
                if (target == null) return false;

                if (PlayerControl.LocalPlayer.IsJailed()) return false;

                if (PlayerControl.LocalPlayer.coolingDown()) return false;

                if (target.IsGuarded2())
                {
                    role.KillCooldown = CustomGameOptions.GuardKCReset;
                    return false; 
                }

                Utils.Interact(PlayerControl.LocalPlayer, target, true);

                // Set the last kill time
                role.KillCooldown = CustomGameOptions.CovenKCD;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(NormalGameManager), nameof(NormalGameManager.GetMapOptions))]
    public class MapPatch
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(ref MapOptions __result)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(Faction.Coven)) return;
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            if (MeetingHud.Instance) return;
            __result = new MapOptions
		    {
			    Mode = MapOptions.Modes.Sabotage
		    };
        }
    }

    //Code by 50, made me win more time ty
    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageReactor))]
    public class PreventReactor
    {
        public static bool Prefix()
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return true;
            if (PlayerControl.LocalPlayer == null) return true;
            if (PlayerControl.LocalPlayer.Data == null) return true;
            if (!PlayerControl.LocalPlayer.Is(Faction.Coven)) return true;
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return true;
            return false;
        }
    }
    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageOxygen))]
    public class PreventOxygen
    {
        public static bool Prefix()
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return true;
            if (PlayerControl.LocalPlayer == null) return true;
            if (PlayerControl.LocalPlayer.Data == null) return true;
            if (!PlayerControl.LocalPlayer.Is(Faction.Coven)) return true;
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return true;
            return false;
        }
    }
    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageSeismic))]
    public class PreventSeismic
    {
        public static bool Prefix()
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return true;
            if (PlayerControl.LocalPlayer == null) return true;
            if (PlayerControl.LocalPlayer.Data == null) return true;
            if (!PlayerControl.LocalPlayer.Is(Faction.Coven)) return true;
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return true;
            return false;
        }
    }
    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageHeli))]
    public class PreventHeli
    {
        public static bool Prefix()
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return true;
            if (PlayerControl.LocalPlayer == null) return true;
            if (PlayerControl.LocalPlayer.Data == null) return true;
            if (!PlayerControl.LocalPlayer.Is(Faction.Coven)) return true;
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return true;
            return false;
        }
    }
}