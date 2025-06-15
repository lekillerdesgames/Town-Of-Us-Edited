using AmongUs.Data;
using Assets.CoreScripts;
using HarmonyLib;
using System;
using TownOfUsEdited.Extensions;
using TownOfUsEdited.Roles;
using UnityEngine;

namespace TownOfUsEdited.NeutraleRoles.DoppelgangerMod
{
    public class MeetingPatch
    {
        public static bool doppelchat = false;
        public static bool doppelVoted = false;
        public static bool doppelCosmeticsUpdated = false;
        public static bool doppelNameUpdated = false;
        public static bool doppelStartedMeeting = false;
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public class DoppelgangerUpdate
        {
            public static void UpdateDoppelMeeting(Doppelganger role, MeetingHud __instance)
            {
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    foreach (var state in __instance.playerStates)
                    {
                        if (player.PlayerId == state.TargetPlayerId && role.Player != null
                        && !role.Player.Data.Disconnected)
                        {
                            if (player == role.Player && role.TransformedPlayer != null && !player.Data.IsDead)
                            {
                                state.NameText.text = role.TransformedPlayer.GetDefaultOutfit().PlayerName;
                                state.SetCosmetics(role.TransformedPlayer.Data);
                                if (state.DidVote)
                                {
                                    doppelVoted = true;
                                }
                            }
                            else if (player == role.TransformedPlayer && !role.Player.Data.IsDead)
                            {
                                state.NameText.text = role.Player.GetDefaultOutfit().PlayerName;
                                state.SetCosmetics(role.Player.Data);
                            }
                        }
                    }
                }
            }
            public static void Postfix(HudManager __instance)
            {
                if (doppelCosmeticsUpdated && doppelNameUpdated)
                {
                    doppelchat = false;
                }
                if (!MeetingHud.Instance) return;
                foreach (var doppel in Role.GetRoles(RoleEnum.Doppelganger))
                {
                    var role = Role.GetRole<Doppelganger>(doppel.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || role.TransformedPlayer == null) return;
                    UpdateDoppelMeeting(role, MeetingHud.Instance);
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        public class VoteUpdate
        {
            public static void Postfix()
            {
                if (doppelVoted)
                {
                    doppelVoted = false;
                }
            }
        }

        [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
        public class ChatBubbleNameFix
        {
            public static bool Prefix(ChatBubble __instance)
            {
                if (LobbyBehaviour.Instance) return true;
                foreach (var doppel in Role.GetRoles(RoleEnum.Doppelganger))
                {
                    var role = Role.GetRole<Doppelganger>(doppel.Player);
                    if (doppelchat)
                    {
                        __instance.NameText.text = role.TransformedPlayer.GetDefaultOutfit().PlayerName;
                        if (__instance.Player.transform.localScale != ChatBubble.PlayerNotificationScale) __instance.NameText.color = Color.white; // Sometimes its green for some reason
                        __instance.NameText.ForceMeshUpdate(true, true);
                        __instance.Xmark.enabled = false;
			            __instance.Background.color = Palette.White;
                        if (MeetingHud.Instance)
                        {
                            if (doppelVoted)
                            {
                                __instance.votedMark.enabled = true;
                            }
                            else
                            {
                                __instance.votedMark.enabled = false;
                            }
                        }
                        doppelNameUpdated = true;
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetCosmetics))]
        public class ChatBubbleCosmeticFix
        {
            public static bool Prefix(ChatBubble __instance)
            {
                if (LobbyBehaviour.Instance) return true;
                foreach (var doppel in Role.GetRoles(RoleEnum.Doppelganger))
                {
                    var role = Role.GetRole<Doppelganger>(doppel.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || role.TransformedPlayer == null) return true;
                    foreach (var player in PlayerControl.AllPlayerControls)
                    {
                        if (player == role.Player && doppelchat)
                        {
		                    __instance.playerInfo = role.TransformedPlayer.Data;
                            __instance.Player.UpdateFromPlayerData(role.TransformedPlayer.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.ScrollingUI, false, null, true);
		                    __instance.Player.ToggleName(false);
		                    __instance.maskLayer = 51 + __instance.PoolIndex;
		                    __instance.SetMaskLayer();
		                    __instance.SetColorblindText();
                            doppelCosmeticsUpdated = true;
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.OnEnable))]
        public class ChatBubbleCosmeticFix2
        {
            public static bool Prefix(ChatBubble __instance)
            {
                if (LobbyBehaviour.Instance) return true;
                foreach (var doppel in Role.GetRoles(RoleEnum.Doppelganger))
                {
                    var role = Role.GetRole<Doppelganger>(doppel.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || role.TransformedPlayer == null) return true;
                    if (__instance.playerInfo != null && __instance.playerInfo == role.TransformedPlayer.Data)
		            {
                        __instance.Player.UpdateFromPlayerData(role.TransformedPlayer.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.ScrollingUI, false, null, true);
			            __instance.SetMaskLayer();
			            __instance.SetColorblindText();
		            }
		            __instance.ColorBlindName.enabled = DataManager.Settings.Accessibility.ColorBlindMode;
		            __instance.ColorBlindName.text = __instance.Player.ColorBlindName;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
        public class AddChatDoppel
        {
            public static bool Prefix(ChatController __instance, [HarmonyArgument(0)] PlayerControl sourcePlayer)
            {
                if (__instance != HudManager.Instance.Chat)
                    return true;

                if (LobbyBehaviour.Instance)
                {
                    doppelchat = false;
                    return true;
                }

                foreach (var doppel in Role.GetRoles(RoleEnum.Doppelganger))
                {
                    var role = Role.GetRole<Doppelganger>(doppel.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || role.TransformedPlayer == null) return true;
                }
                
                if (sourcePlayer.Is(RoleEnum.Doppelganger))
                {
                    doppelchat = true;
                    doppelCosmeticsUpdated = false;
                    doppelNameUpdated = false;
                }
                else doppelchat = false;
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
        public class CosmeticsDeadFix
        {
            public static bool Prefix(PlayerVoteArea __instance, [HarmonyArgument(0)] NetworkedPlayerInfo playerInfo)
            {
                foreach (var doppel in Role.GetRoles(RoleEnum.Doppelganger))
                {
                    var role = Role.GetRole<Doppelganger>(doppel.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || role.TransformedPlayer == null) return true;
                    if (playerInfo == role.TransformedPlayer.Data)
                    {
                        __instance.Background.sprite = ShipStatus.Instance.CosmeticsCache.GetNameplate(playerInfo.DefaultOutfit.NamePlateId).Image;
		                __instance.PlayerIcon.UpdateFromPlayerData(role.TransformedPlayer.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.ComplexUI, false, null, true);
		                __instance.PlayerIcon.ToggleName(false);
		                __instance.NameText.text = playerInfo.PlayerName;
		                __instance.LevelNumberText.text = ProgressionManager.FormatVisualLevel(playerInfo.PlayerLevel);
		                PlayerMaterial.SetColors((int)DataManager.Player.Customization.Color, __instance.ThumbsDown);
		                __instance.ColorBlindName.enabled = DataManager.Settings.Accessibility.ColorBlindMode;
		                __instance.ColorBlindName.text = __instance.PlayerIcon.ColorBlindName;
		                __instance.SetColorblindText();
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
        public class StartMeetingPatch
        {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
            {
                foreach (var doppel in Role.GetRoles(RoleEnum.Doppelganger))
                {
                    var role = Role.GetRole<Doppelganger>(doppel.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || role.TransformedPlayer == null) return true;
                    if (__instance == doppel.Player)
                    {
                        bool flag = target == null;
                        var targetPlayer = Utils.PlayerByData(target);
                        if (target != null && targetPlayer == role.TransformedPlayer) target = role.OldTransformed.Data;
		                DestroyableSingleton<UnityTelemetry>.Instance.WriteMeetingStarted(flag);
		                DestroyableSingleton<DebugAnalytics>.Instance.Analytics.MeetingStarted(role.TransformedPlayer.Data, target != null);
		                ShipStatus.Instance.StartMeeting(role.TransformedPlayer, target);
                        doppelStartedMeeting = true;
		                if (__instance.AmOwner)
		                {
			                if (flag)
			                {
				                __instance.RemainingEmergencies--;
				                DataManager.Player.Stats.IncrementStat(StatID.EmergenciesCalled);
				                return false;
			                }
			                DataManager.Player.Stats.IncrementStat(StatID.BodiesReported);
		                }
                        return false;
                    }
                    else return true;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(MeetingCalledAnimation), nameof(MeetingCalledAnimation.Initialize))]
        public class MeetingAnimationColorPatch
        {
            public static bool Prefix(MeetingCalledAnimation __instance, [HarmonyArgument(0)] NetworkedPlayerInfo reportInfo)
            {
                foreach (var doppel in Role.GetRoles(RoleEnum.Doppelganger))
                {
                    var role = Role.GetRole<Doppelganger>(doppel.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || role.TransformedPlayer == null) return true;
                    if (reportInfo == role.TransformedPlayer.Data)
                    {
                        __instance.playerParts.UpdateFromPlayerData(role.TransformedPlayer.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, false, null, true);
                        doppelStartedMeeting = false;
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PoolablePlayer), nameof(PoolablePlayer.UpdateFromEitherPlayerDataOrCache))]
        public class FixDataAppearence
        {
            public static bool Prefix(PoolablePlayer __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pData)
            {
                foreach (var doppel in Role.GetRoles(RoleEnum.Doppelganger))
                {
                    var role = Role.GetRole<Doppelganger>(doppel.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || role.TransformedPlayer == null) return true;
                    if (pData == role.TransformedPlayer.Data)
                    {
                        __instance.UpdateFromPlayerData(role.TransformedPlayer.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.ComplexUI, false, null, true);
                        return false;
                    }
                    return true;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChatNote))]
        public class FixVotingPlayer
        {
            [HarmonyPriority(Priority.Last)]
            public static void Prefix(ref NetworkedPlayerInfo srcPlayer)
            {
                foreach (var doppel in Role.GetRoles(RoleEnum.Doppelganger))
                {
                    var role = Role.GetRole<Doppelganger>(doppel.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || role.TransformedPlayer == null) return;
                    if (srcPlayer == role.Player.Data)
                    {
                        srcPlayer = role.TransformedPlayer.Data;
                    }
                    return;
                }
                return;
            }
        }
    }
}