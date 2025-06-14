using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TownOfUsEdited.CrewmateRoles.MayorMod;
using TownOfUsEdited.Extensions;
using TownOfUsEdited.Roles;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.UI;

namespace TownOfUsEdited.CrewmateRoles.SwapperMod
{
    public class ShowHideButtonsSwapper
    {
        public static Dictionary<byte, int> CalculateVotes(MeetingHud __instance)
        {
            var self = CalculateVotesSwap(__instance);

            var maxIdx = self.MaxPair(out var tie);

            var exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(v => !tie && v.PlayerId == maxIdx.Key);

            foreach (var oracle in Role.GetRoles(RoleEnum.Oracle))
            {
                var oracleRole = (Oracle)oracle;
                if (oracleRole.Player.Data.IsDead || oracleRole.Player.Data.Disconnected || exiled == null || oracleRole.Blessed == null) continue;
                if (oracleRole.Blessed.PlayerId == exiled.PlayerId)
                {
                    oracleRole.SavedBlessed = true;
                    Utils.Rpc(CustomRPC.Bless, oracleRole.Player.PlayerId, (byte)0);
                    var dictionary = new Dictionary<byte, int>();
                    return dictionary;
                }
            }
            foreach (var terrorist in Role.GetRoles(RoleEnum.Terrorist))
            {
                var terroristRole = (Terrorist)terrorist;
                if (terroristRole.Player.Data.IsDead || terroristRole.Player.Data.Disconnected || exiled == null) continue;
                if (terroristRole.Player.PlayerId == exiled.PlayerId)
                {
                    terroristRole.SavedVote = true;
                    Utils.Rpc(CustomRPC.TerroristSaveVote, terroristRole.Player.PlayerId);
                    var dictionary = new Dictionary<byte, int>();
                    return dictionary;
                }
            }

            return self;
        }
        public static Dictionary<byte, int> CalculateVotesSwap(MeetingHud __instance)
        {
            var self = RegisterExtraVotes.CalculateAllVotes(__instance);
            if (SwapVotes.Swap1 == null || SwapVotes.Swap2 == null) return self;
            foreach (var swapper in Role.AllRoles.Where(x => x.RoleType == RoleEnum.Swapper))
            {
                if (swapper.Player.Data.IsDead || swapper.Player.Data.Disconnected) return self;
            }
            PlayerControl swapPlayer1 = null;
            PlayerControl swapPlayer2 = null;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == SwapVotes.Swap1.TargetPlayerId) swapPlayer1 = player;
                if (player.PlayerId == SwapVotes.Swap2.TargetPlayerId) swapPlayer2 = player;
            }
            if (swapPlayer1.Data.IsDead || swapPlayer1.Data.Disconnected ||
                swapPlayer2.Data.IsDead || swapPlayer2.Data.Disconnected) return self;

            var swap1 = 0;
            if (self.TryGetValue(SwapVotes.Swap1.TargetPlayerId, out var value)) swap1 = value;

            var swap2 = 0;
            if (self.TryGetValue(SwapVotes.Swap2.TargetPlayerId, out var value2)) swap2 = value2;

            self[SwapVotes.Swap2.TargetPlayerId] = swap1;
            self[SwapVotes.Swap1.TargetPlayerId] = swap2;

            return self;
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Confirm))]
        public static class Confirm
        {
            public static bool Prefix(MeetingHud __instance)
            {
                if (!PlayerControl.LocalPlayer.Is(RoleEnum.Swapper)) return true;
                var swapper = Role.GetRole<Swapper>(PlayerControl.LocalPlayer);
                foreach (var button in swapper.Buttons.Where(button => button != null))
                {
                    if (button.GetComponent<SpriteRenderer>().sprite == AddButton.DisabledSprite)
                        button.SetActive(false);

                    button.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
                }

                if (swapper.ListOfActives.Count(x => x.Item2) == 2)
                {
                    var toSet1 = true;
                    for (var i = 0; i < swapper.ListOfActives.Count; i++)
                    {
                        if (!swapper.ListOfActives[i].Item2) continue;

                        if (toSet1)
                        {
                            SwapVotes.Swap1 = __instance.playerStates[i];
                            toSet1 = false;
                        }
                        else
                        {
                            SwapVotes.Swap2 = __instance.playerStates[i];
                        }
                    }
                }

                if (SwapVotes.Swap1 == null || SwapVotes.Swap2 == null) return true;

                Utils.Rpc(CustomRPC.SetSwaps, SwapVotes.Swap1.TargetPlayerId, SwapVotes.Swap2.TargetPlayerId);
                return true;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
        public static class CheckForEndVoting
        {
            private static bool CheckVoted(PlayerVoteArea playerVoteArea)
            {
                if (playerVoteArea.AmDead || playerVoteArea.DidVote)
                    return true;

                var playerInfo = GameData.Instance.GetPlayerById(playerVoteArea.TargetPlayerId);
                if (playerInfo == null)
                    return true;

                var playerControl = playerInfo.Object;

                if (playerControl.Is(AbilityEnum.Assassin) && playerInfo.IsDead)
                {
                    playerVoteArea.VotedFor = PlayerVoteArea.DeadVote;
                    playerVoteArea.SetDead(false, true);
                    return true;
                }

                return true;
            }
            public static bool Prefix(MeetingHud __instance)
            {
                if (__instance.playerStates.All(ps => ps.AmDead || ps.DidVote && CheckVoted(ps)))
                {
                    var self = CalculateVotes(__instance);

                    var array = new Il2CppStructArray<MeetingHud.VoterState>(__instance.playerStates.Length);

                    var maxIdx = self.MaxPair(out var tie);

                    var exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(v => !tie && v.PlayerId == maxIdx.Key);
                    for (var i = 0; i < __instance.playerStates.Length; i++)
                    {
                        var playerVoteArea = __instance.playerStates[i];
                        array[i] = new MeetingHud.VoterState
                        {
                            VoterId = playerVoteArea.TargetPlayerId,
                            VotedForId = playerVoteArea.VotedFor
                        };
                    }

                    __instance.RpcVotingComplete(array, exiled, tie);
                }

                return false;
            }
        }
    }
}