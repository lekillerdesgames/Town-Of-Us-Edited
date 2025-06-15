using AmongUs.Data;
using Assets.CoreScripts;
using HarmonyLib;
using TownOfUsEdited.Extensions;
using TownOfUsEdited.Roles;
using UnityEngine;

namespace TownOfUsEdited.ImpostorRoles.ReviverMod
{
    public class MeetingPatch
    {
        public static bool reviverchat = false;
        public static bool voted = false;
        public static bool cosmeticsUpdated = false;
        public static bool nameUpdated = false;
        public static bool StartedMeeting = false;
        
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public class ReviverUpdate
        {
            public static void UpdateMeeting(Reviver role, MeetingHud __instance)
            {
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    foreach (var state in __instance.playerStates)
                    {
                        if (player.PlayerId == state.TargetPlayerId && role.Player != null
                        && !role.Player.Data.Disconnected)
                        {
                            if (player == role.Player && role.UsedRevive && !player.Data.IsDead)
                            {
                                state.NameText.text = role.RevivedPlayer.GetDefaultOutfit().PlayerName;
                                state.SetCosmetics(role.RevivedPlayer.Data);
                                if (state.DidVote)
                                {
                                    voted = true;
                                }
                            }
                            else if (player == role.RevivedPlayer && role.UsedRevive && !role.Player.Data.IsDead)
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
                if (cosmeticsUpdated && nameUpdated)
                {
                    reviverchat = false;
                }
                if (!MeetingHud.Instance) return;
                foreach (var reviver in Role.GetRoles(RoleEnum.Reviver))
                {
                    var role = Role.GetRole<Reviver>(reviver.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || role.UsedRevive == false) return;
                    UpdateMeeting(role, MeetingHud.Instance);
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        public class VoteUpdate
        {
            public static void Postfix()
            {
                if (voted)
                {
                    voted = false;
                }
            }
        }

        [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
        public class ChatBubbleNameFix
        {
            public static bool Prefix(ChatBubble __instance)
            {
                if (LobbyBehaviour.Instance) return true;
                foreach (var reviver in Role.GetRoles(RoleEnum.Reviver))
                {
                    var role = Role.GetRole<Reviver>(reviver.Player);
                    if (reviverchat)
                    {
                        __instance.NameText.text = role.RevivedPlayer.GetDefaultOutfit().PlayerName;
                        if (__instance.Player.transform.localScale != ChatBubble.PlayerNotificationScale)
                        {
                            if (PlayerControl.LocalPlayer.Is(Faction.Impostors) || PlayerControl.LocalPlayer.Is(Faction.Madmates)) __instance.NameText.color = Palette.ImpostorRed;
                            else __instance.NameText.color = Color.white; // Sometimes its green for some reason
                        }
                        __instance.NameText.ForceMeshUpdate(true, true);
                        __instance.Xmark.enabled = false;
			            __instance.Background.color = Palette.White;
                        if (MeetingHud.Instance)
                        {
                            if (voted)
                            {
                                __instance.votedMark.enabled = true;
                            }
                            else
                            {
                                __instance.votedMark.enabled = false;
                            }
                        }
                        if (PlayerControl.LocalPlayer.Data.IsImpostor())
                        {
                            __instance.NameText.text = "<color=#FF0000>" + role.RevivedPlayer.GetDefaultOutfit().PlayerName + "</color>";
                        }
                        nameUpdated = true;
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
                foreach (var reviver in Role.GetRoles(RoleEnum.Reviver))
                {
                    var role = Role.GetRole<Reviver>(reviver.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || role.UsedRevive == false) return true;
                    foreach (var player in PlayerControl.AllPlayerControls)
                    {
                        if (player == role.Player && reviverchat)
                        {
		                    __instance.playerInfo = role.RevivedPlayer.Data;
                            __instance.Player.UpdateFromPlayerData(role.RevivedPlayer.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.ScrollingUI, false, null, true);
		                    __instance.Player.ToggleName(false);
		                    __instance.maskLayer = 51 + __instance.PoolIndex;
		                    __instance.SetMaskLayer();
		                    __instance.SetColorblindText();
                            cosmeticsUpdated = true;
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
                foreach (var reviver in Role.GetRoles(RoleEnum.Reviver))
                {
                    var role = Role.GetRole<Reviver>(reviver.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || role.UsedRevive == false) return true;
                    if (__instance.playerInfo != null && __instance.playerInfo == role.RevivedPlayer.Data)
		            {
                        __instance.Player.UpdateFromPlayerData(role.RevivedPlayer.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.ScrollingUI, false, null, true);
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
        public class AddChat
        {
            public static bool Prefix(ChatController __instance, [HarmonyArgument(0)] PlayerControl sourcePlayer)
            {
                if (__instance != HudManager.Instance.Chat)
                    return true;

                if (LobbyBehaviour.Instance)
                {
                    reviverchat = false;
                    return true;
                }

                foreach (var reviver in Role.GetRoles(RoleEnum.Reviver))
                {
                    var role = Role.GetRole<Reviver>(reviver.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || role.UsedRevive == false) return true;
                }
                
                if (sourcePlayer.Is(RoleEnum.Reviver))
                {
                    reviverchat = true;
                    cosmeticsUpdated = false;
                    nameUpdated = false;
                }
                else reviverchat = false;
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
        public class CosmeticsDeadFix
        {
            public static bool Prefix(PlayerVoteArea __instance, [HarmonyArgument(0)] NetworkedPlayerInfo playerInfo)
            {
                foreach (var reviver in Role.GetRoles(RoleEnum.Reviver))
                {
                    var role = Role.GetRole<Reviver>(reviver.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || role.UsedRevive == false) return true;
                    if (playerInfo == role.RevivedPlayer.Data)
                    {
                        __instance.Background.sprite = ShipStatus.Instance.CosmeticsCache.GetNameplate(playerInfo.DefaultOutfit.NamePlateId).Image;
		                __instance.PlayerIcon.UpdateFromPlayerData(role.RevivedPlayer.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.ComplexUI, false, null, true);
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
                foreach (var reviver in Role.GetRoles(RoleEnum.Reviver))
                {
                    var role = Role.GetRole<Reviver>(reviver.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || role.UsedRevive == false) return true;
                    if (__instance == reviver.Player)
                    {
                        bool flag = target == null;
		                DestroyableSingleton<UnityTelemetry>.Instance.WriteMeetingStarted(flag);
		                DestroyableSingleton<DebugAnalytics>.Instance.Analytics.MeetingStarted(role.RevivedPlayer.Data, target != null);
		                ShipStatus.Instance.StartMeeting(role.RevivedPlayer, target);
                        StartedMeeting = true;
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
                foreach (var reviver in Role.GetRoles(RoleEnum.Reviver))
                {
                    var role = Role.GetRole<Reviver>(reviver.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || role.UsedRevive == false) return true;
                    if (reportInfo == role.RevivedPlayer.Data)
                    {
                        __instance.playerParts.UpdateFromPlayerData(role.RevivedPlayer.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, false, null, true);
                        StartedMeeting = false;
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
                foreach (var reviver in Role.GetRoles(RoleEnum.Reviver))
                {
                    var role = Role.GetRole<Reviver>(reviver.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || role.UsedRevive == false) return true;
                    if (pData == role.RevivedPlayer.Data)
                    {
                        __instance.UpdateFromPlayerData(role.RevivedPlayer.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.ComplexUI, false, null, true);
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
                foreach (var reviver in Role.GetRoles(RoleEnum.Reviver))
                {
                    var role = Role.GetRole<Reviver>(reviver.Player);
                    if (role == null || role.Player == null || role.Player.Data.Disconnected ||
                    role.Player.Data.IsDead || !role.UsedRevive) return;
                    if (srcPlayer == role.Player.Data)
                    {
                        srcPlayer = role.RevivedPlayer.Data;
                    }
                    return;
                }
                return;
            }
        }
    }
}