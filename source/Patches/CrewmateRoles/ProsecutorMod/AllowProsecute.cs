using System.Collections;
using HarmonyLib;
using Reactor.Utilities;
using TownOfUsEdited.CovenRoles.RitualistMod;
using TownOfUsEdited.CrewmateRoles.AltruistMod;
using TownOfUsEdited.CrewmateRoles.SwapperMod;
using TownOfUsEdited.Modifiers.AssassinMod;
using TownOfUsEdited.NeutralRoles.DoomsayerMod;
using TownOfUsEdited.Roles;
using TownOfUsEdited.Roles.Modifiers;
using UnityEngine;
using Assassin = TownOfUsEdited.Roles.Modifiers.Assassin;
using UnityEngine.UI;
using TownOfUsEdited.CrewmateRoles.ImitatorMod;
using TownOfUsEdited.CrewmateRoles.VigilanteMod;
using Reactor.Utilities.Extensions;
using TownOfUsEdited.CrewmateRoles.DeputyMod;
using System.Linq;
using System.Collections.Generic;
using TownOfUsEdited.ImpostorRoles.BlackmailerMod;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace TownOfUsEdited.CrewmateRoles.ProsecutorMod
{
    public class AllowExtraVotes
    {
        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.VoteForMe))]
        public static class VoteForMe
        {
            public static bool Prefix(PlayerVoteArea __instance)
            {
                if (!PlayerControl.LocalPlayer.Is(RoleEnum.Prosecutor)) return true;
                var role = Role.GetRole<Prosecutor>(PlayerControl.LocalPlayer);
                if (__instance.Parent.state == MeetingHud.VoteStates.Proceeding ||
                    __instance.Parent.state == MeetingHud.VoteStates.Results)
                    return false;

                if (__instance != role.Prosecute)
                {
                    if (role.StartProsecute)
                    {
                        if (MeetingHud.Instance.lastSecond < 10)
                        {
                            role.StartProsecute = false;
                            MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(true);
                            return false;
                        }
                        else
                        {
                            role.ProsecuteThisMeeting = true;
                            role.StartProsecute = false;
                            var VotedPlayer = Utils.PlayerById(__instance.TargetPlayerId);
                            MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(true);
                            role.Prosecute.gameObject.SetActive(false);
                            __instance.Buttons.gameObject.SetActive(false);
                            Prosecute(role.Player, VotedPlayer);
                            Utils.Rpc(CustomRPC.Prosecute, role.Player.PlayerId, VotedPlayer.PlayerId);
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    role.StartProsecute = true;
                    MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(false);
                    AddProsecute.UpdateButton(role, MeetingHud.Instance);
                    return false;
                }
            }

            public static void Prosecute(PlayerControl prosecutor, PlayerControl VotedPlayer)
            {
                var role = Role.GetRole<Prosecutor>(prosecutor);
                Coroutines.Start(ProsecuteCoroutine(MeetingHud.Instance, VotedPlayer));
            }

            public static IEnumerator ProsecuteCoroutine(MeetingHud __instance, PlayerControl VotedPlayer)
            {
                foreach (var role in Role.GetRoles(RoleEnum.Prosecutor))
                {
                    var prosecutor = (Prosecutor)role;
                    if (prosecutor.ProsecuteThisMeeting)
                    {
                        ConsoleJoystick.SetMode_Task();
                        yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black, 1f, false);
                        ExileController exileController = Object.Instantiate<ExileController>(ShipStatus.Instance.ExileCutscenePrefab);
                        exileController.transform.SetParent(DestroyableSingleton<HudManager>.Instance.transform, false);
                        exileController.transform.localPosition = new Vector3(0f, 0f, -60f);
                        MeetingHud.Instance.gameObject.SetActive(false);
                        exileController.BeginForGameplay(VotedPlayer.Data, false);
                        while (ExileController.Instance) yield return null;
                        var votedPlayerRole = Role.GetRole(VotedPlayer);
                        votedPlayerRole.DeathReason = DeathReasons.Exiled;
                        Utils.Rpc(CustomRPC.SetDeathReason, VotedPlayer.PlayerId, (byte)DeathReasons.Exiled);
                        __instance.DespawnOnDestroy = false;
                        if (MapBehaviour.Instance)
                        {
                            MapBehaviour.Instance.Close();
                        }
                        if (PlayerControl.LocalPlayer == VotedPlayer)
                        {
                            Utils.ShowDeadBodies = true;
                        }
                        UpdateProsecuted(VotedPlayer);
                        MeetingHud.Instance.gameObject.SetActive(true);
                        prosecutor.Prosecuted = true;
                        if ((VotedPlayer.Is(Faction.Crewmates) && CustomGameOptions.ProsDiesOnIncorrectPros && !role.Player.Is(Faction.Madmates)) || (CustomGameOptions.GameMode == GameMode.Werewolf && VotedPlayer.Is(Faction.Crewmates)))
                        {
                            KillButtonTarget.DontRevive = prosecutor.Player.PlayerId;
                            prosecutor.Player.Exiled();
                            UpdateProsecuted(prosecutor.Player);
                            prosecutor.DeathReason = DeathReasons.Exiled;
                            Utils.Rpc(CustomRPC.SetDeathReason, prosecutor.Player.PlayerId, (byte)DeathReasons.Exiled);
                            if (PlayerControl.LocalPlayer == prosecutor.Player)
                            {
                                Utils.ShowDeadBodies = true;
                            }
                            if (prosecutor.Player.IsLover() && CustomGameOptions.BothLoversDie)
                            {
                                var otherlover = Modifier.GetModifier<Lover>(prosecutor.Player).OtherLover;
                                otherlover.Player.Exiled();
                                UpdateProsecuted(otherlover.Player);
                                var otherloverRole = Role.GetRole(otherlover.Player);
                                otherloverRole.DeathReason = DeathReasons.Suicided;
                                Utils.Rpc(CustomRPC.SetDeathReason, otherlover.Player.PlayerId, (byte)DeathReasons.Suicided);
                                if (PlayerControl.LocalPlayer == otherlover.Player)
                                {
                                    Utils.ShowDeadBodies = true;
                                }
                            }
                        }
                        if (VotedPlayer.IsLover() && CustomGameOptions.BothLoversDie)
                        {
                            var otherlover = Modifier.GetModifier<Lover>(VotedPlayer).OtherLover;
                            otherlover.Player.Exiled();
                            UpdateProsecuted(otherlover.Player);
                            var otherloverRole = Role.GetRole(otherlover.Player);
                            otherloverRole.DeathReason = DeathReasons.Suicided;
                            Utils.Rpc(CustomRPC.SetDeathReason, otherlover.Player.PlayerId, (byte)DeathReasons.Suicided);
                            if (PlayerControl.LocalPlayer == otherlover.Player)
                            {
                                Utils.ShowDeadBodies = true;
                            }
                        }
                        yield return new WaitForSeconds(2);
                        prosecutor.ProsecuteThisMeeting = false;
                        yield break;
                    }
                }
            }

            public static void UpdateProsecuted(PlayerControl player)
            {
                var hudManager = DestroyableSingleton<HudManager>.Instance;
                var amOwner = player.AmOwner;
                if (amOwner)
                {
                    Utils.ShowDeadBodies = true;
                    hudManager.ShadowQuad.gameObject.SetActive(false);

                    if (player.Is(RoleEnum.Swapper))
                    {
                        var swapper = Role.GetRole<Swapper>(PlayerControl.LocalPlayer);
                        swapper.ListOfActives.Clear();
                        swapper.Buttons.Clear();
                        SwapVotes.Swap1 = null;
                        SwapVotes.Swap2 = null;
                        Utils.Rpc(CustomRPC.SetSwaps, sbyte.MaxValue, sbyte.MaxValue);
                        var buttons = Role.GetRole<Swapper>(player).Buttons;
                        foreach (var button in buttons)
                        {
                            button.SetActive(false);
                            button.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
                        }
                    }

                    if (player.Is(RoleEnum.Imitator))
                    {
                        var imitator = Role.GetRole<Imitator>(PlayerControl.LocalPlayer);
                        imitator.ListOfActives.Clear();
                        imitator.Buttons.Clear();
                        SetImitate.Imitate = null;
                        var buttons = Role.GetRole<Imitator>(player).Buttons;
                        foreach (var button in buttons)
                        {
                            button.SetActive(false);
                            button.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
                        }
                    }

                    if (player.Is(RoleEnum.Vigilante))
                    {
                        var retributionist = Role.GetRole<Vigilante>(PlayerControl.LocalPlayer);
                        ShowHideButtonsVigi.HideButtonsVigi(retributionist);
                    }

                    if (player.Is(AbilityEnum.Assassin))
                    {
                        var assassin = Ability.GetAbility<Assassin>(PlayerControl.LocalPlayer);
                        ShowHideButtons.HideButtons(assassin);
                    }

                    if (player.Is(RoleEnum.Doomsayer))
                    {
                        var doomsayer = Role.GetRole<Doomsayer>(PlayerControl.LocalPlayer);
                        ShowHideButtonsDoom.HideButtonsDoom(doomsayer);
                    }

                    if (player.Is(RoleEnum.Ritualist))
                    {
                        var ritualist = Role.GetRole<Ritualist>(PlayerControl.LocalPlayer);
                        ShowHideButtonsRitualist.HideButtons(ritualist);
                    }

                    if (player.Is(RoleEnum.Mayor))
                    {
                        var mayor = Role.GetRole<Mayor>(PlayerControl.LocalPlayer);
                        mayor.RevealButton.Destroy();
                    }

                    if (player.Is(RoleEnum.Deputy))
                    {
                        var dep = Role.GetRole<Deputy>(PlayerControl.LocalPlayer);
                        RemoveButtons.HideButtons(dep);
                    }
                }
                player.Die(DeathReason.Kill, false);
                var voteArea = MeetingHud.Instance.playerStates.FirstOrDefault(x => x.TargetPlayerId == player.PlayerId);
                if (voteArea.DidVote) voteArea.UnsetVote();
                voteArea.AmDead = true;
                voteArea.Overlay.gameObject.SetActive(true);
                voteArea.Overlay.color = Color.white;
                voteArea.XMark.gameObject.SetActive(true);
                voteArea.XMark.transform.localScale = Vector3.one;

                var blackmailers = Role.AllRoles.Where(x => x.RoleType == RoleEnum.Blackmailer && x.Player != null).Cast<Blackmailer>();
                var blackmailed = new List<PlayerControl>();
                foreach (var role in blackmailers)
                {
                    if (role.Blackmailed != null && !blackmailed.Contains(role.Blackmailed))
                    {
                        blackmailed.Add(role.Blackmailed);
                        if (voteArea.TargetPlayerId == role.Blackmailed.PlayerId)
                        {
                            if (BlackmailMeetingUpdate.PrevXMark != null && BlackmailMeetingUpdate.PrevOverlay != null)
                            {
                                voteArea.XMark.sprite = BlackmailMeetingUpdate.PrevXMark;
                                voteArea.Overlay.sprite = BlackmailMeetingUpdate.PrevOverlay;
                                voteArea.XMark.transform.localPosition = new Vector3(
                                    voteArea.XMark.transform.localPosition.x - BlackmailMeetingUpdate.LetterXOffset,
                                    voteArea.XMark.transform.localPosition.y - BlackmailMeetingUpdate.LetterYOffset,
                                    voteArea.XMark.transform.localPosition.z);
                            }
                        }
                    }
                }

                var jailors = Role.AllRoles.Where(x => x.RoleType == RoleEnum.Jailor && x.Player != null).Cast<Jailor>();
                foreach (var role in jailors)
                {
                    if (role.JailedPlayer == player || role.Player == player)
                    {
                        role.JailCell.Destroy();
                    }
                }

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Vigilante) && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    var vigi = Role.GetRole<Vigilante>(PlayerControl.LocalPlayer);
                    ShowHideButtonsVigi.HideTarget(vigi, voteArea.TargetPlayerId);
                }

                if (PlayerControl.LocalPlayer.Is(AbilityEnum.Assassin) && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    var assassin = Ability.GetAbility<Assassin>(PlayerControl.LocalPlayer);
                    ShowHideButtons.HideTarget(assassin, voteArea.TargetPlayerId);
                }

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Ritualist) && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    var ritualist = Role.GetRole<Ritualist>(PlayerControl.LocalPlayer);
                    ShowHideButtonsRitualist.HideTarget(ritualist, voteArea.TargetPlayerId);
                }

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Doomsayer) && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    var doom = Role.GetRole<Doomsayer>(PlayerControl.LocalPlayer);
                    ShowHideButtonsDoom.HideTarget(doom, voteArea.TargetPlayerId);
                }

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Deputy) && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    var dep = Role.GetRole<Deputy>(PlayerControl.LocalPlayer);
                    if (dep.Buttons.Count > 0 && dep.Buttons[voteArea.TargetPlayerId] != null)
                    {
                        dep.Buttons[voteArea.TargetPlayerId].SetActive(false);
                        dep.Buttons[voteArea.TargetPlayerId].GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
                    }
                }

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Swapper) && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    var swapper = Role.GetRole<Swapper>(PlayerControl.LocalPlayer);
                    var index = int.MaxValue;
                    for (var i = 0; i < swapper.ListOfActives.Count; i++)
                    {
                        if (swapper.ListOfActives[i].Item1 == voteArea.TargetPlayerId)
                        {
                            index = i;
                            break;
                        }
                    }
                    if (index != int.MaxValue)
                    {
                        var button = swapper.Buttons[index];
                        if (button != null)
                        {
                            if (button.GetComponent<SpriteRenderer>().sprite == TownOfUsEdited.SwapperSwitch)
                            {
                                swapper.ListOfActives[index] = (swapper.ListOfActives[index].Item1, false);
                                if (SwapVotes.Swap1 == voteArea) SwapVotes.Swap1 = null;
                                if (SwapVotes.Swap2 == voteArea) SwapVotes.Swap2 = null;
                                Utils.Rpc(CustomRPC.SetSwaps, sbyte.MaxValue, sbyte.MaxValue);
                            }
                            button.SetActive(false);
                            button.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
                            swapper.Buttons[index] = null;
                        }
                    }
                }

                foreach (var playerVoteArea in MeetingHud.Instance.playerStates)
                {
                    if (playerVoteArea.VotedFor != player.PlayerId) continue;
                    playerVoteArea.UnsetVote();
                    var voteAreaPlayer = Utils.PlayerById(playerVoteArea.TargetPlayerId);
                    if (voteAreaPlayer.Is(RoleEnum.Prosecutor))
                    {
                        var pros = Role.GetRole<Prosecutor>(voteAreaPlayer);
                        pros.ProsecuteThisMeeting = false;
                    }
                    if (!voteAreaPlayer.AmOwner) continue;
                    MeetingHud.Instance.ClearVote();
                }

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Imitator) && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    var imitatorRole = Role.GetRole<Imitator>(PlayerControl.LocalPlayer);
                    if (!MeetingHud.Instance.playerStates[PlayerControl.LocalPlayer.PlayerId].DidVote)
                        if (MeetingHud.Instance.state != MeetingHud.VoteStates.Results && MeetingHud.Instance.state != MeetingHud.VoteStates.Proceeding)
                        {
                            AddButtonImitator.GenButton(imitatorRole, voteArea, true);
                        }
                }

                if (AmongUsClient.Instance.AmHost) MeetingHud.Instance.CheckForEndVoting();
            }
        }

        [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
        public class NoSpawnMiniGame
        {
            public static bool Prefix(SpawnInMinigame __instance)
            {
                foreach (var role in Role.GetRoles(RoleEnum.Prosecutor))
                {
                    var prosecutor = (Prosecutor)role;
                    if (prosecutor.ProsecuteThisMeeting)
                    {
                        __instance.Close();
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
        public class StopGameplayerEnable
        {
            public static bool Prefix()
            {
                foreach (var role in Role.GetRoles(RoleEnum.Prosecutor))
                {
                    var prosecutor = (Prosecutor)role;
                    if (prosecutor.ProsecuteThisMeeting)
                    {
                        DestroyableSingleton<HudManager>.Instance.StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f, false));
                        return false;
                    }
                }

                return true;
            }
        }
    }
}