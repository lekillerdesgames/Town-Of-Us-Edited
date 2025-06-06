using System;
using System.Collections.Generic;
using HarmonyLib;
using Reactor.Utilities;
using TownOfUsEdited.Extensions;
using TownOfUsEdited.Roles;
using TownOfUsEdited.Roles.Modifiers;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;

namespace TownOfUsEdited.CrewmateRoles.MayorMod
{
    [HarmonyPatch(typeof(MeetingHud))]
    public class RegisterExtraVotes
    {
        public static Dictionary<byte, int> CalculateAllVotes(MeetingHud __instance)
        {
            var dictionary = new Dictionary<byte, int>();

            for (var i = 0; i < __instance.playerStates.Length; i++)
            {
                var playerVoteArea = __instance.playerStates[i];
                if (!playerVoteArea.DidVote
                    || playerVoteArea.AmDead
                    || playerVoteArea.VotedFor == PlayerVoteArea.MissedVote
                    || playerVoteArea.VotedFor == PlayerVoteArea.DeadVote) continue;

                var player = Utils.PlayerById(playerVoteArea.TargetPlayerId);
                if (player.Is(RoleEnum.Mayor))
                {
                    var mayor = Role.GetRole<Mayor>(player);
                    if (mayor.Revealed)
                    {
                        if (dictionary.TryGetValue(playerVoteArea.VotedFor, out var num2))
                            dictionary[playerVoteArea.VotedFor] = num2 + 2;
                        else
                            dictionary[playerVoteArea.VotedFor] = 2;
                    }
                }

                if (dictionary.TryGetValue(playerVoteArea.VotedFor, out var num))
                    dictionary[playerVoteArea.VotedFor] = num + 1;
                else
                    dictionary[playerVoteArea.VotedFor] = 1;
            }

            dictionary.MaxPair(out var tie);

            if (tie)
                foreach (var player in __instance.playerStates)
                {
                    if (!player.DidVote
                        || player.AmDead
                        || player.VotedFor == PlayerVoteArea.MissedVote
                        || player.VotedFor == PlayerVoteArea.DeadVote) continue;

                    var modifiers = Modifier.GetModifiers(player);
                    if (modifiers == null || modifiers.Length == 0) continue;
                    if (modifiers.Any(x => x.ModifierType == ModifierEnum.Tiebreaker))
                    {
                        if (dictionary.TryGetValue(player.VotedFor, out var num))
                            dictionary[player.VotedFor] = num + 1;
                        else
                            dictionary[player.VotedFor] = 1;
                    }
                }

            return dictionary;
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
        public static class VotingComplete
        {
            public static void Postfix(MeetingHud __instance,
                [HarmonyArgument(0)] Il2CppStructArray<MeetingHud.VoterState> states,
                [HarmonyArgument(1)] NetworkedPlayerInfo exiled,
                [HarmonyArgument(2)] bool tie)
            {
                // __instance.exiledPlayer = __instance.wasTie ? null : __instance.exiledPlayer;
                var exiledString = exiled == null ? "null" : exiled.PlayerName;
                PluginSingleton<TownOfUsEdited>.Instance.Log.LogMessage($"Exiled PlayerName = {exiledString}");
                PluginSingleton<TownOfUsEdited>.Instance.Log.LogMessage($"Was a tie = {tie}");
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
        public static class PopulateResults
        {
            public static bool Prefix(MeetingHud __instance,
                [HarmonyArgument(0)] Il2CppStructArray<MeetingHud.VoterState> states)
            {
                var allNums = new Dictionary<int, int>();

                __instance.TitleText.text = Object.FindObjectOfType<TranslationController>()
                    .GetString(StringNames.MeetingVotingResults, Array.Empty<Il2CppSystem.Object>());
                var amountOfSkippedVoters = 0;

                for (var i = 0; i < __instance.playerStates.Length; i++)
                {
                    var playerVoteArea = __instance.playerStates[i];
                    playerVoteArea.ClearForResults();
                    allNums.Add(i, 0);

                    for (var stateIdx = 0; stateIdx < states.Length; stateIdx++)
                    {
                        var voteState = states[stateIdx];
                        var playerInfo = GameData.Instance.GetPlayerById(voteState.VoterId);

                        if (playerInfo == null)
                        {
                            Debug.LogError(string.Format("Couldn't find player info for voter: {0}",
                                voteState.VoterId));
                        }
                        else if (i == 0 && voteState.SkippedVote)
                        {
                            __instance.BloopAVoteIcon(playerInfo, amountOfSkippedVoters, __instance.SkippedVoting.transform);
                            amountOfSkippedVoters++;
                        }
                        else if (voteState.VotedForId == playerVoteArea.TargetPlayerId)
                        {
                            __instance.BloopAVoteIcon(playerInfo, allNums[i], playerVoteArea.transform);
                            allNums[i]++;
                        }
                        foreach (var mayor in Role.GetRoles(RoleEnum.Mayor))
                        {
                            var mayorRole = (Mayor)mayor;
                            if (mayorRole.Revealed)
                            {
                                if (voteState.VoterId == mayorRole.Player.PlayerId)
                                {
                                    if (playerInfo == null)
                                    {
                                        Debug.LogError(string.Format("Couldn't find player info for voter: {0}",
                                            voteState.VoterId));
                                    }
                                    else if (i == 0 && voteState.SkippedVote)
                                    {
                                        __instance.BloopAVoteIcon(playerInfo, amountOfSkippedVoters, __instance.SkippedVoting.transform);
                                        __instance.BloopAVoteIcon(playerInfo, amountOfSkippedVoters, __instance.SkippedVoting.transform);
                                        amountOfSkippedVoters++;
                                        amountOfSkippedVoters++;
                                    }
                                    else if (voteState.VotedForId == playerVoteArea.TargetPlayerId)
                                    {
                                        __instance.BloopAVoteIcon(playerInfo, allNums[i], playerVoteArea.transform);
                                        __instance.BloopAVoteIcon(playerInfo, allNums[i], playerVoteArea.transform);
                                        allNums[i]++;
                                        allNums[i]++;
                                    }
                                }
                            }
                        }
                    }
                }
                return false;
            }
        }
    }
}