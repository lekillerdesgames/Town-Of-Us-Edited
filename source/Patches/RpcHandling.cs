using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using Reactor.Networking.Extensions;
using TownOfUsEdited.CrewmateRoles.TimeLordMod;
using TownOfUsEdited.CrewmateRoles.MedicMod;
using TownOfUsEdited.CrewmateRoles.SwapperMod;
using TownOfUsEdited.CrewmateRoles.VigilanteMod;
using TownOfUsEdited.NeutralRoles.DoomsayerMod;
using TownOfUsEdited.ImpostorRoles.ConverterMod;
using TownOfUsEdited.CustomOption;
using TownOfUsEdited.Modifiers.AssassinMod;
using Assassin = TownOfUsEdited.Roles.Modifiers.Assassin;
using TownOfUsEdited.NeutralRoles.ExecutionerMod;
using TownOfUsEdited.NeutralRoles.GuardianAngelMod;
using TownOfUsEdited.ImpostorRoles.MinerMod;
using TownOfUsEdited.CrewmateRoles.HaunterMod;
using TownOfUsEdited.CrewmateRoles.HelperMod;
using TownOfUsEdited.NeutralRoles.PhantomMod;
using TownOfUsEdited.ImpostorRoles.TraitorMod;
using TownOfUsEdited.CrewmateRoles.ImitatorMod;
using TownOfUsEdited.ImpostorRoles.WitchMod;
using TownOfUsEdited.Roles;
using TownOfUsEdited.Roles.Modifiers;
using UnityEngine;
using Object = UnityEngine.Object;
using PerformKillButton = TownOfUsEdited.NeutralRoles.AmnesiacMod.PerformKillButton;
using PerformKill = TownOfUsEdited.Patches.NeutralRoles.ShifterMod.PerformKill;
using PerformStake = TownOfUsEdited.CrewmateRoles.VampireHunterMod.PerformKill;
using Random = UnityEngine.Random;
using TownOfUsEdited.Patches;
using AmongUs.GameOptions;
using TownOfUsEdited.NeutralRoles.VampireMod;
using TownOfUsEdited.CrewmateRoles.MayorMod;
using System.Reflection;
using TownOfUsEdited.Patches.NeutralRoles;
using TownOfUsEdited.CrewmateRoles.GuardianMod;
using TownOfUsEdited.Patches.Modifiers.MadmateMod;
using TownOfUsEdited.ImpostorRoles.SpiritMod;
using TownOfUsEdited.ImpostorRoles.BlinderMod;
using TownOfUsEdited.ImpostorRoles.FreezerMod;
using TownOfUsEdited.CovenRoles.RitualistMod;
using TownOfUsEdited.ChooseCrewGhostRoles;
using TownOfUsEdited.ChooseImpGhostRole;
using TownOfUsEdited.CrewmateRoles.DoctorMod;
using KillButtonTarget = TownOfUsEdited.CrewmateRoles.AltruistMod.KillButtonTarget;
using TownOfUsEdited.CrewmateRoles.DeputyMod;
using TownOfUsEdited.Patches.ImpostorRoles;
using TownOfUsEdited.Patches.CovenRoles;
using TownOfUsEdited.Patches.NeutralRoles.SerialKillerMod;
using TownOfUsEdited.Patches.NeutralRoles.VampireMod;
using TownOfUsEdited.Patches.Modifiers.LoversMod;
using TownOfUsEdited.Extensions;
using TownOfUsEdited.ImpostorRoles.BlackmailerMod;
using TownOfUsEdited.CrewmateRoles.ClericMod;
using TownOfUsEdited.CrewmateRoles.JailorMod;
using AllowExtraVotes = TownOfUsEdited.CrewmateRoles.ProsecutorMod.AllowExtraVotes;
using TownOfUsEdited.Patches.CrewmateRoles.JailorMod;
using TownOfUsEdited.WerewolfRoles.TalkativeWolfMod.ChatPatch;
using System.Collections;
using Sentry.Unity.NativeUtils;
using TownOfUsEdited.ImpostorRoles.MorphlingMod;
using TownOfUsEdited.CrewmateRoles.ChameleonMod;
using TownOfUsEdited.Patches.CovenRoles.PotionMasterMod;
using TownOfUsEdited.ImpostorRoles.SwooperMod;

namespace TownOfUsEdited
{
    public static class RpcHandling
    {
        private static readonly List<(Type, int, bool)> CrewmateRoles = new();
        private static readonly List<(Type, int, bool)> CrewmateKillingRoles = new();
        private static readonly List<(Type, int, bool)> CrewmatePowerRoles = new();
        private static readonly List<(Type, int, bool)> CrewmateSupportRoles = new();
        private static readonly List<(Type, int, bool)> CrewmateInvestigativeRoles = new();
        private static readonly List<(Type, int, bool)> CrewmateProtectiveRoles = new();
        private static readonly List<(Type, int, bool)> NeutralBenignRoles = new();
        private static readonly List<(Type, int, bool)> NeutralEvilRoles = new();
        private static readonly List<(Type, int, bool)> NeutralKillingRoles = new();
        private static readonly List<(Type, int, bool)> CovenRoles = new();
        private static readonly List<(Type, int, bool)> CovenKillingRoles = new();
        private static readonly List<(Type, int, bool)> CovenSupportRoles = new();
        private static readonly List<(Type, int, bool)> ImpostorRoles = new();
        private static readonly List<(Type, int, bool)> ImpostorConcealingRoles = new();
        private static readonly List<(Type, int, bool)> ImpostorKillingRoles = new();
        private static readonly List<(Type, int, bool)> ImpostorSupportRoles = new();
        private static readonly List<(Type, int)> CrewmateModifiers = new();
        private static readonly List<(Type, int)> GlobalModifiers = new();
        private static readonly List<(Type, int)> ImpostorModifiers = new();
        private static readonly List<(Type, int)> AssassinModifiers = new();
        private static readonly List<(Type, CustomRPC, int)> AssassinAbility = new();
        private static bool PhantomOn;
        private static bool TraitorOn;

        public static Dictionary<PlayerControl, string> Upped = new Dictionary<PlayerControl, string>();

        internal static bool Check(int probability)
        {
            if (probability == 0) return false;
            if (probability == 100) return true;
            var num = Random.RandomRangeInt(1, 101);
            return num <= probability;
        }
        internal static bool CheckJugg()
        {
            var num = Random.RandomRangeInt(1, 101);
            return num <= 10 * CustomGameOptions.MaxNeutralKillingRoles;
        }
        private static int PickRoleCount(int min, int max)
        {
            if (min > max) min = max;
            return Random.RandomRangeInt(min, max + 1);
        }

        private static (Type, int, bool) SelectRole(List<(Type, int, bool)> roles)
        {
            var chosenRoles = roles.Where(x => x.Item2 == 100).ToList();
            if (chosenRoles.Count > 0)
            {
                chosenRoles.Shuffle();
                return chosenRoles[0];
            }

            chosenRoles = roles.Where(x => x.Item2 < 100).ToList();
            int total = chosenRoles.Sum(x => x.Item2);
            int random = Random.RandomRangeInt(1, total + 1);

            int cumulative = 0;
            (Type, int, bool) selectedRole = default;

            foreach (var role in chosenRoles)
            {
                cumulative += role.Item2;
                if (random <= cumulative)
                {
                    selectedRole = role;
                    break;
                }
            }
            return selectedRole;
        }

        private static void SortRoles(this List<(Type, int, bool)> roles, int max)
        {
            if (max <= 0)
            {
                roles.Clear();
                return;
            }

            var chosenRoles = roles.Where(x => x.Item2 == 100).ToList();
            // Shuffle to ensure that the same 100% roles do not appear in
            // every game if there are more than the maximum.
            chosenRoles.Shuffle();
            // Truncate the list if there are more 100% roles than the max.
            chosenRoles = chosenRoles.GetRange(0, Math.Min(max, chosenRoles.Count));

            if (chosenRoles.Count < max)
            {
                // These roles MAY appear in this game, but they may not.
                var potentialRoles = roles.Where(x => x.Item2 < 100).ToList();
                // Determine which roles appear in this game.
                var optionalRoles = potentialRoles.Where(x => Check(x.Item2)).ToList();
                potentialRoles = potentialRoles.Where(x => !optionalRoles.Contains(x)).ToList();

                optionalRoles.Shuffle();
                chosenRoles.AddRange(optionalRoles.GetRange(0, Math.Min(max - chosenRoles.Count, optionalRoles.Count)));

                // If there are not enough roles after that, randomly add
                // ones which were previously eliminated, up to the max.
                if (chosenRoles.Count < max)
                {
                    potentialRoles.Shuffle();
                    chosenRoles.AddRange(potentialRoles.GetRange(0, Math.Min(max - chosenRoles.Count, potentialRoles.Count)));
                }
            }

            // This list will be shuffled later in GenEachRole.
            roles.Clear();
            roles.AddRange(chosenRoles);
        }

        private static void GenEachRole(List<NetworkedPlayerInfo> infected, int impsCount)
        {
            var crewmates = Utils.GetPlayers(infected);
            var impostors = new List<PlayerControl>();
            var coven = new List<PlayerControl>();
            // I do not shuffle impostors/crewmates because roles should be shuffled before they are assigned to them anyway.
            // Assigning shuffled roles across a shuffled list may mess with the statistics? I dunno, I didn't major in math.
            // One Fisher-Yates shuffle should have statistically equal permutation probability on its own, anyway.

            var crewRoles = new List<(Type, int, bool)>();
            var neutRoles = new List<(Type, int, bool)>();
            var impRoles = new List<(Type, int, bool)>();
            var covenRoles = new List<(Type, int, bool)>();

            // Generate Spectator role for host
            if (CustomGameOptions.SpectateHost)
            {
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data == GameData.Instance.GetHost())
                    {
                        Role.GenRole<Role>(typeof(Spectator), player);
                        crewmates.Remove(player);
                    }
                }
            }

            while (impsCount > 0 && crewmates.Count > 0)
            {   
                var rand = UnityEngine.Random.RandomRangeInt(0, crewmates.Count);
                var pc = crewmates[rand];
                impostors.Add(pc);
                crewmates.Remove(pc);
                impsCount -= 1;
            }

            if (CustomGameOptions.GameMode == GameMode.Classic)
            {
                var benign = PickRoleCount(CustomGameOptions.MinNeutralBenignRoles, Math.Min(CustomGameOptions.MaxNeutralBenignRoles, NeutralBenignRoles.Count));
                var evil = PickRoleCount(CustomGameOptions.MinNeutralEvilRoles, Math.Min(CustomGameOptions.MaxNeutralEvilRoles, NeutralEvilRoles.Count));
                var killing = PickRoleCount(CustomGameOptions.MinNeutralKillingRoles, Math.Min(CustomGameOptions.MaxNeutralKillingRoles, NeutralKillingRoles.Count));
                var covenCount = PickRoleCount(CustomGameOptions.MinCoven, CustomGameOptions.MaxCoven);

                var canSubtract = (int faction, int minFaction) => { return faction > minFaction; };
                var factions = new List<string>() { "Benign", "Evil", "Killing" };

                // Crew must always start out outnumbering neutrals, so subtract roles until that can be guaranteed.
                while (Math.Ceiling((double)crewmates.Count/2) <= benign + evil + killing)
                {
                    bool canSubtractBenign = canSubtract(benign, CustomGameOptions.MinNeutralBenignRoles);
                    bool canSubtractEvil = canSubtract(evil, CustomGameOptions.MinNeutralEvilRoles);
                    bool canSubtractKilling = canSubtract(killing, CustomGameOptions.MinNeutralKillingRoles);
                    bool canSubtractNone = !canSubtractBenign && !canSubtractEvil && !canSubtractKilling;

                    factions.Shuffle();
                    switch(factions.First())
                    {
                        case "Benign":
                            if (benign > 0 && (canSubtractBenign || canSubtractNone))
                            {
                                benign -= 1;
                                break;
                            }
                            goto case "Evil";
                        case "Evil":
                            if (evil > 0 && (canSubtractEvil || canSubtractNone))
                            {
                                evil -= 1;
                                break;
                            }
                            goto case "Killing";
                        case "Killing":
                            if (killing > 0 && (canSubtractKilling || canSubtractNone))
                            {
                                killing -= 1;
                                break;
                            }
                            goto default;
                        default:
                            if (benign > 0)
                            {
                                benign -= 1;
                            }
                            else if (evil > 0)
                            {
                                evil -= 1;
                            }
                            else if (killing > 0)
                            {
                                killing -= 1;
                            }
                            break;
                    }

                    if (benign + evil + killing == 0)
                        break;
                }

                // Crew must always start out outnumbering coven, so subtract roles until that can be guaranteed.
                while (Math.Ceiling((double)crewmates.Count/2) <= covenCount)
                {
                    bool canSubtractCoven = canSubtract(covenCount, CustomGameOptions.MinCoven);
                    bool canSubtractNone = !canSubtractCoven;

                    if (covenCount > 0 && (canSubtractCoven || canSubtractNone))
                    {
                        covenCount -= 1;
                        break;
                    }

                    if (covenCount == 0)
                        break;
                }

                if (CustomGameOptions.SpectateHost)
                {
                    if (Upped.Keys.Any(x => x.Data == GameData.Instance.GetHost()))
                    {
                        Upped.Remove(Upped.Keys.FirstOrDefault(x => x.Data == GameData.Instance.GetHost()));
                    }
                }

                var uppedPlayers = Upped;

                while (uppedPlayers.Count > 0 && CustomGameOptions.GameMode == GameMode.Classic && CustomGameOptions.AllowUp)
                {
                    var players = Upped.Keys.ToArray();
                    foreach (var pc in players)
                    {
                        Upped.TryGetValue(pc, out var roleName);
                        var newRoleCrew = CrewmateRoles.Any(x => x.Item1.Name == roleName);
                        if (newRoleCrew)
                        {
                            Role.GenRole<Role>(CrewmateRoles.FirstOrDefault(x => x.Item1.Name == roleName).Item1, pc);
                            uppedPlayers.Remove(pc);
                            CrewmateRoles.Remove(CrewmateRoles.FirstOrDefault(x => x.Item1.Name == roleName));
                            crewmates.Remove(pc);
                        }
                        else if (roleName == "Crewmate")
                        {
                            Role.GenRole<Role>(typeof(Crewmate), pc);
                            uppedPlayers.Remove(pc);
                            crewmates.Remove(pc);
                        }
                        var newRoleImpo = ImpostorRoles.Any(x => x.Item1.Name == roleName);
                        if (newRoleImpo)
                        {
                            Role.GenRole<Role>(ImpostorRoles.FirstOrDefault(x => x.Item1.Name == roleName).Item1, pc);
                            uppedPlayers.Remove(pc);
                            ImpostorRoles.Remove(ImpostorRoles.FirstOrDefault(x => x.Item1.Name == roleName));
                            crewmates.Remove(pc);
                            impsCount--;
                        }
                        else if (roleName == "Impostor")
                        {
                            Role.GenRole<Role>(typeof(Impostor), pc);
                            uppedPlayers.Remove(pc);
                            crewmates.Remove(pc);
                            impsCount--;
                        }
                        var newRoleCoven = CovenRoles.Any(x => x.Item1.Name == roleName);
                        if (newRoleCoven)
                        {
                            Role.GenRole<Role>(CovenRoles.FirstOrDefault(x => x.Item1.Name == roleName).Item1, pc);
                            uppedPlayers.Remove(pc);
                            CovenRoles.Remove(CovenRoles.FirstOrDefault(x => x.Item1.Name == roleName));
                            crewmates.Remove(pc);
                            covenCount--;
                        }
                        else if (roleName == "Coven")
                        {
                            Role.GenRole<Role>(typeof(Coven), pc);
                            uppedPlayers.Remove(pc);
                            crewmates.Remove(pc);
                            covenCount--;
                        }
                        var newRoleBenign = NeutralBenignRoles.Any(x => x.Item1.Name == roleName);
                        if (newRoleBenign)
                        {
                            Role.GenRole<Role>(NeutralBenignRoles.FirstOrDefault(x => x.Item1.Name == roleName).Item1, pc);
                            uppedPlayers.Remove(pc);
                            NeutralBenignRoles.Remove(NeutralBenignRoles.FirstOrDefault(x => x.Item1.Name == roleName));
                            crewmates.Remove(pc);
                            benign--;
                        }
                        var newRoleEvil = NeutralEvilRoles.Any(x => x.Item1.Name == roleName);
                        if (newRoleEvil)
                        {
                            Role.GenRole<Role>(NeutralEvilRoles.FirstOrDefault(x => x.Item1.Name == roleName).Item1, pc);
                            uppedPlayers.Remove(pc);
                            NeutralEvilRoles.Remove(NeutralEvilRoles.FirstOrDefault(x => x.Item1.Name == roleName));
                            crewmates.Remove(pc);
                            evil--;
                        }
                        var newRoleKilling = NeutralKillingRoles.Any(x => x.Item1.Name == roleName);
                        if (newRoleKilling)
                        {
                            Role.GenRole<Role>(NeutralKillingRoles.FirstOrDefault(x => x.Item1.Name == roleName).Item1, pc);
                            uppedPlayers.Remove(pc);
                            NeutralKillingRoles.Remove(NeutralKillingRoles.FirstOrDefault(x => x.Item1.Name == roleName));
                            crewmates.Remove(pc);
                            killing--;
                        }
                        if (uppedPlayers.ContainsKey(pc))
                        {
                            uppedPlayers.Remove(pc);
                        }
                    }
                }

                NeutralBenignRoles.SortRoles(benign);
                NeutralEvilRoles.SortRoles(evil);
                NeutralKillingRoles.SortRoles(killing);
                CovenRoles.SortRoles(covenCount);

                var toRemove = covenCount;

                while (toRemove > 0)
                {
                    var rand = UnityEngine.Random.RandomRangeInt(0, crewmates.Count);
                    var pc = crewmates[rand];
                    coven.Add(pc);
                    crewmates.Remove(pc);
                    toRemove -= 1;
                }

                if (NeutralKillingRoles.Contains((typeof(Vampire), CustomGameOptions.VampireOn, true)) && CustomGameOptions.VampireHunterOn > 0)
                    CrewmateRoles.Add((typeof(VampireHunter), CustomGameOptions.VampireHunterOn, true));

                CrewmateRoles.SortRoles(crewmates.Count - NeutralBenignRoles.Count - NeutralEvilRoles.Count - NeutralKillingRoles.Count);
                ImpostorRoles.SortRoles(impostors.Count);

                crewRoles.AddRange(CrewmateRoles);
                impRoles.AddRange(ImpostorRoles);

                neutRoles.AddRange(NeutralBenignRoles);
                neutRoles.AddRange(NeutralEvilRoles);
                neutRoles.AddRange(NeutralKillingRoles);
                covenRoles.AddRange(CovenRoles);
                // Roles are not, at this point, shuffled yet.
            }
            else if (CustomGameOptions.GameMode == GameMode.RoleList)
            {
                var covensCount = PickRoleCount(CustomGameOptions.MinCoven, CustomGameOptions.MaxCoven);

                var toRemove = covensCount;

                while (toRemove > 0)
                {
                    var rand = UnityEngine.Random.RandomRangeInt(0, crewmates.Count);
                    var pc = crewmates[rand];
                    coven.Add(pc);
                    crewmates.Remove(pc);
                    toRemove -= 1;
                }

                // sort out bad lists
                var players = impostors.Count + crewmates.Count;
                List<RoleOptions> crewNKBuckets = [RoleOptions.CrewInvest, RoleOptions.CrewKilling, RoleOptions.CrewPower, RoleOptions.CrewProtective,
                RoleOptions.CrewSupport, RoleOptions.CrewCommon, RoleOptions.CrewSpecial, RoleOptions.CrewRandom, RoleOptions.NeutKilling];
                List<RoleOptions> impBuckets = [RoleOptions.ImpConceal, RoleOptions.ImpKilling, RoleOptions.ImpSupport, RoleOptions.ImpCommon, RoleOptions.ImpRandom];
                List<RoleOptions> covenBuckets = [RoleOptions.CovenKilling, RoleOptions.CovenSupport, RoleOptions.CovenCommon, RoleOptions.CovenRandom];
                List<RoleOptions> buckets = [CustomGameOptions.Slot1, CustomGameOptions.Slot2, CustomGameOptions.Slot3, CustomGameOptions.Slot4];
                var impCount = 0;
                var covenCount = 0;
                var anySlots = 0;

                if (players > 4) buckets.Add(CustomGameOptions.Slot5);
                if (players > 5) buckets.Add(CustomGameOptions.Slot6);
                if (players > 6) buckets.Add(CustomGameOptions.Slot7);
                if (players > 7) buckets.Add(CustomGameOptions.Slot8);
                if (players > 8) buckets.Add(CustomGameOptions.Slot9);
                if (players > 9) buckets.Add(CustomGameOptions.Slot10);
                if (players > 10) buckets.Add(CustomGameOptions.Slot11);
                if (players > 11) buckets.Add(CustomGameOptions.Slot12);
                if (players > 12) buckets.Add(CustomGameOptions.Slot13);
                if (players > 13) buckets.Add(CustomGameOptions.Slot14);
                if (players > 14) buckets.Add(CustomGameOptions.Slot15);
                if (players > 15)
                {
                    for (int i = 0; i < players - 15; i++)
                    {
                        int random = Random.RandomRangeInt(0, 4);
                        if (random == 0) buckets.Add(RoleOptions.CrewRandom);
                        else buckets.Add(RoleOptions.NonImpCoven);
                    }
                }

                // imp / coven issues
                foreach (var roleOption in buckets)
                {
                    if (impBuckets.Contains(roleOption)) impCount += 1;
                    else if (covenBuckets.Contains(roleOption)) covenCount += 1;
                    else if (roleOption == RoleOptions.Any) anySlots += 1;
                }
                while (impCount > impostors.Count)
                {
                    buckets.Shuffle();
                    buckets.Remove(buckets.FindLast(x => impBuckets.Contains(x)));
                    buckets.Add(RoleOptions.NonImpCoven);
                    impCount -= 1;
                }
                while (impCount + anySlots < impostors.Count)
                {
                    buckets.Shuffle();
                    buckets.RemoveAt(0);
                    buckets.Add(RoleOptions.ImpRandom);
                    impCount += 1;
                }
                while (covenCount > coven.Count)
                {
                    buckets.Shuffle();
                    buckets.Remove(buckets.FindLast(x => covenBuckets.Contains(x)));
                    buckets.Add(RoleOptions.NonImpCoven);
                    covenCount -= 1;
                }
                while (covenCount + anySlots < coven.Count)
                {
                    buckets.Shuffle();
                    buckets.RemoveAt(0);
                    buckets.Add(RoleOptions.CovenRandom);
                    covenCount += 1;
                }
                while (buckets.Contains(RoleOptions.Any))
                {
                    buckets.Shuffle();
                    buckets.Remove(buckets.FindLast(x => x == RoleOptions.Any));
                    if (impCount < impostors.Count)
                    {
                        buckets.Add(RoleOptions.ImpRandom);
                        impCount += 1;
                    }
                    else if (covenCount < coven.Count)
                    {
                        buckets.Add(RoleOptions.CovenRandom);
                        covenCount += 1;
                    }
                    else buckets.Add(RoleOptions.NonImpCoven);
                }

                // crew and neut issues
                var noChange = false;
                var nonImpCoven = false;
                var randNeut = false;
                foreach (var roleOption in buckets)
                {
                    if (crewNKBuckets.Contains(roleOption))
                    {
                        noChange = true;
                        break;
                    }
                    else if (roleOption == RoleOptions.NeutRandom)
                    {
                        randNeut = true;
                        break;
                    }
                    else if (roleOption == RoleOptions.NonImpCoven)
                    {
                        nonImpCoven = true;
                        break;
                    }
                }
                if (!noChange)
                {
                    List<RoleOptions> add = [RoleOptions.CrewRandom, RoleOptions.NeutKilling];
                    add.Shuffle();
                    if (randNeut)
                    {
                        buckets.Remove(RoleOptions.NeutRandom);
                        buckets.Add(RoleOptions.NeutKilling);
                    }
                    else if (nonImpCoven)
                    {
                        buckets.Remove(RoleOptions.NonImpCoven);
                        buckets.Add(add[0]);
                    }
                    else
                    {
                        buckets.Remove(buckets.FindLast(x => !impBuckets.Contains(x)));
                        buckets.Add(add[0]);
                    }
                }

                // imp buckets
                while (buckets.Contains(RoleOptions.ImpConceal))
                {
                    if (ImpostorConcealingRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.ImpConceal))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.ImpConceal));
                            buckets.Add(RoleOptions.ImpCommon);
                        }
                        break;
                    }
                    var addedRole = SelectRole(ImpostorConcealingRoles);
                    impRoles.Add(addedRole);
                    ImpostorConcealingRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) ImpostorConcealingRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.ImpConceal);
                }
                var commonImpRoles = ImpostorConcealingRoles;
                while (buckets.Contains(RoleOptions.ImpSupport))
                {
                    if (ImpostorSupportRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.ImpSupport))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.ImpSupport));
                            buckets.Add(RoleOptions.ImpCommon);
                        }
                        break;
                    }
                    var addedRole = SelectRole(ImpostorSupportRoles);
                    impRoles.Add(addedRole);
                    ImpostorSupportRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) ImpostorSupportRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.ImpSupport);
                }
                commonImpRoles.AddRange(ImpostorSupportRoles);
                while (buckets.Contains(RoleOptions.ImpKilling))
                {
                    if (ImpostorKillingRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.ImpKilling))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.ImpKilling));
                            buckets.Add(RoleOptions.ImpRandom);
                        }
                        break;
                    }
                    var addedRole = SelectRole(ImpostorKillingRoles);
                    impRoles.Add(addedRole);
                    ImpostorKillingRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) ImpostorKillingRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.ImpKilling);
                }
                var randomImpRoles = ImpostorKillingRoles;
                while (buckets.Contains(RoleOptions.ImpCommon))
                {
                    if (commonImpRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.ImpCommon))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.ImpCommon));
                            buckets.Add(RoleOptions.ImpRandom);
                        }
                        break;
                    }
                    var addedRole = SelectRole(commonImpRoles);
                    impRoles.Add(addedRole);
                    commonImpRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) commonImpRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.ImpCommon);
                }
                randomImpRoles.AddRange(commonImpRoles);
                while (buckets.Contains(RoleOptions.ImpRandom))
                {
                    if (randomImpRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.ImpRandom))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.ImpRandom));
                        }
                        break;
                    }
                    var addedRole = SelectRole(randomImpRoles);
                    impRoles.Add(addedRole);
                    randomImpRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) randomImpRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.ImpRandom);
                }

                // coven buckets
                while (buckets.Contains(RoleOptions.CovenSupport))
                {
                    if (CovenSupportRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.CovenSupport))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.CovenSupport));
                            buckets.Add(RoleOptions.CovenCommon);
                        }
                        break;
                    }
                    var addedRole = SelectRole(CovenSupportRoles);
                    covenRoles.Add(addedRole);
                    CovenSupportRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) CovenSupportRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.CovenSupport);
                }
                var commonCovenRoles = CovenSupportRoles;
                while (buckets.Contains(RoleOptions.CovenKilling))
                {
                    if (CovenKillingRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.CovenKilling))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.CovenKilling));
                            buckets.Add(RoleOptions.CovenRandom);
                        }
                        break;
                    }
                    var addedRole = SelectRole(CovenKillingRoles);
                    covenRoles.Add(addedRole);
                    CovenKillingRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) CovenKillingRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.CovenKilling);
                }
                var randomCovenRoles = CovenKillingRoles;
                while (buckets.Contains(RoleOptions.CovenCommon))
                {
                    if (commonCovenRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.CovenCommon))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.CovenCommon));
                            buckets.Add(RoleOptions.CovenRandom);
                        }
                        break;
                    }
                    var addedRole = SelectRole(commonCovenRoles);
                    covenRoles.Add(addedRole);
                    commonCovenRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) commonCovenRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.CovenCommon);
                }
                randomCovenRoles.AddRange(commonCovenRoles);
                while (buckets.Contains(RoleOptions.CovenRandom))
                {
                    if (randomCovenRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.CovenRandom))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.CovenRandom));
                        }
                        break;
                    }
                    var addedRole = SelectRole(randomCovenRoles);
                    covenRoles.Add(addedRole);
                    randomCovenRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) randomCovenRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.CovenRandom);
                }

                // crew buckets
                while (buckets.Contains(RoleOptions.CrewInvest))
                {
                    if (CrewmateInvestigativeRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.CrewInvest))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.CrewInvest));
                            buckets.Add(RoleOptions.CrewCommon);
                        }
                        break;
                    }
                    var addedRole = SelectRole(CrewmateInvestigativeRoles);
                    crewRoles.Add(addedRole);
                    CrewmateInvestigativeRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) CrewmateInvestigativeRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.CrewInvest);
                }
                var commonCrewRoles = CrewmateInvestigativeRoles;
                while (buckets.Contains(RoleOptions.CrewProtective))
                {
                    if (CrewmateProtectiveRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.CrewProtective))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.CrewProtective));
                            buckets.Add(RoleOptions.CrewCommon);
                        }
                        break;
                    }
                    var addedRole = SelectRole(CrewmateProtectiveRoles);
                    crewRoles.Add(addedRole);
                    CrewmateProtectiveRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) CrewmateProtectiveRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.CrewProtective);
                }
                commonCrewRoles.AddRange(CrewmateProtectiveRoles);
                while (buckets.Contains(RoleOptions.CrewSupport))
                {
                    if (CrewmateSupportRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.CrewSupport))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.CrewSupport));
                            buckets.Add(RoleOptions.CrewCommon);
                        }
                        break;
                    }
                    var addedRole = SelectRole(CrewmateSupportRoles);
                    crewRoles.Add(addedRole);
                    CrewmateSupportRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) CrewmateSupportRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.CrewSupport);
                }
                commonCrewRoles.AddRange(CrewmateSupportRoles);
                while (buckets.Contains(RoleOptions.CrewKilling))
                {
                    if (CrewmateKillingRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.CrewKilling))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.CrewKilling));
                            buckets.Add(RoleOptions.CrewSpecial);
                        }
                        break;
                    }
                    var addedRole = SelectRole(CrewmateKillingRoles);
                    crewRoles.Add(addedRole);
                    CrewmateKillingRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) CrewmateKillingRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.CrewKilling);
                }
                var specialCrewRoles = CrewmateKillingRoles;
                while (buckets.Contains(RoleOptions.CrewPower))
                {
                    if (CrewmatePowerRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.CrewPower))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.CrewPower));
                            buckets.Add(RoleOptions.CrewSpecial);
                        }
                        break;
                    }
                    var addedRole = SelectRole(CrewmatePowerRoles);
                    crewRoles.Add(addedRole);
                    CrewmatePowerRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) CrewmatePowerRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.CrewPower);
                }
                specialCrewRoles.AddRange(CrewmatePowerRoles);
                while (buckets.Contains(RoleOptions.CrewCommon))
                {
                    if (commonCrewRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.CrewCommon))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.CrewCommon));
                            buckets.Add(RoleOptions.CrewRandom);
                        }
                        break;
                    }
                    var addedRole = SelectRole(commonCrewRoles);
                    crewRoles.Add(addedRole);
                    commonCrewRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) commonCrewRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.CrewCommon);
                }
                var randomCrewRoles = commonCrewRoles;
                while (buckets.Contains(RoleOptions.CrewSpecial))
                {
                    if (specialCrewRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.CrewSpecial))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.CrewSpecial));
                            buckets.Add(RoleOptions.CrewRandom);
                        }
                        break;
                    }
                    var addedRole = SelectRole(specialCrewRoles);
                    crewRoles.Add(addedRole);
                    specialCrewRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) specialCrewRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.CrewSpecial);
                }
                randomCrewRoles.AddRange(specialCrewRoles);
                while (buckets.Contains(RoleOptions.CrewRandom))
                {
                    if (randomCrewRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.CrewRandom))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.CrewRandom));
                        }
                        break;
                    }
                    var addedRole = SelectRole(randomCrewRoles);
                    crewRoles.Add(addedRole);
                    randomCrewRoles.Remove(addedRole);
                     if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) randomCrewRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.CrewRandom);
                }
                var randomNonImpRoles = randomCrewRoles;

                // neutral buckets
                while (buckets.Contains(RoleOptions.NeutBenign))
                {
                    if (NeutralBenignRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.NeutBenign))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.NeutBenign));
                            buckets.Add(RoleOptions.NeutCommon);
                        }
                        break;
                    }
                    var addedRole = SelectRole(NeutralBenignRoles);
                    crewRoles.Add(addedRole);
                    NeutralBenignRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) NeutralBenignRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.NeutBenign);
                }
                var commonNeutRoles = NeutralBenignRoles;
                while (buckets.Contains(RoleOptions.NeutEvil))
                {
                    if (NeutralEvilRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.NeutEvil))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.NeutEvil));
                            buckets.Add(RoleOptions.NeutCommon);
                        }
                        break;
                    }
                    var addedRole = SelectRole(NeutralEvilRoles);
                    crewRoles.Add(addedRole);
                    NeutralEvilRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) NeutralEvilRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.NeutEvil);
                }
                commonNeutRoles.AddRange(NeutralEvilRoles);
                while (buckets.Contains(RoleOptions.NeutKilling))
                {
                    if (NeutralKillingRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.NeutKilling))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.NeutKilling));
                            buckets.Add(RoleOptions.NeutRandom);
                        }
                        break;
                    }
                    var addedRole = SelectRole(NeutralKillingRoles);
                    crewRoles.Add(addedRole);
                    NeutralKillingRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) NeutralKillingRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.NeutKilling);
                }
                var randomNeutRoles = NeutralKillingRoles;
                while (buckets.Contains(RoleOptions.NeutCommon))
                {
                    if (commonNeutRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.NeutCommon))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.NeutCommon));
                            buckets.Add(RoleOptions.NeutRandom);
                        }
                        break;
                    }
                    var addedRole = SelectRole(commonNeutRoles);
                    crewRoles.Add(addedRole);
                    commonNeutRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) commonNeutRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.NeutCommon);
                }
                randomNeutRoles.AddRange(commonNeutRoles);
                while (buckets.Contains(RoleOptions.NeutRandom))
                {
                    if (randomNeutRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.NeutRandom))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.NeutRandom));
                            buckets.Add(RoleOptions.NonImpCoven);
                        }
                        break;
                    }
                    var addedRole = SelectRole(randomNeutRoles);
                    crewRoles.Add(addedRole);
                    randomNeutRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) randomNeutRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.NeutRandom);
                }
                randomNonImpRoles.AddRange(randomNeutRoles);
                while (buckets.Contains(RoleOptions.NonImpCoven))
                {
                    if (randomNonImpRoles.Count == 0)
                    {
                        while (buckets.Contains(RoleOptions.NonImpCoven))
                        {
                            buckets.Remove(buckets.FindLast(x => x == RoleOptions.NonImpCoven));
                        }
                        break;
                    }
                    var addedRole = SelectRole(randomNonImpRoles);
                    crewRoles.Add(addedRole);
                    randomNonImpRoles.Remove(addedRole);
                    if (addedRole.Item2 >= 100) addedRole.Item2 -= 5;
                    if (addedRole.Item2 > 0 && !addedRole.Item3) randomNonImpRoles.Add(addedRole);
                    buckets.Remove(RoleOptions.NonImpCoven);
                }
            }

            if (CustomGameOptions.GameMode == GameMode.Cultist)
            {
                impRoles.Clear();
                ImpostorRoles.Clear();
                ImpostorRoles.Add((typeof(Converter), 100, true));
                ImpostorRoles.SortRoles(1);
                impRoles.AddRange(ImpostorRoles);
                coven.Clear();
                CrewmateRoles.SortRoles(crewmates.Count);
                crewRoles.AddRange(CrewmateRoles);
            }
            else if (CustomGameOptions.GameMode == GameMode.Classic)
            {
                // Roles have already been sorted for Classic mode.
                // So just add in the neutral roles.
                crewRoles.AddRange(neutRoles);
            }

            // Shuffle roles before handing them out.
            // This should ensure a statistically equal chance of all permutations of roles.
            crewRoles.Shuffle();
            impRoles.Shuffle();
            covenRoles.Shuffle();

            crewmates.RemoveAll(x => x.Data.Disconnected);
            impostors.RemoveAll(x => x.Data.Disconnected);
            coven.RemoveAll(x => x.Data.Disconnected);

            if (impsCount > 0 && impostors.Count <= 0 && crewmates.Count > 0)
            {
                var rand = UnityEngine.Random.RandomRangeInt(0, crewmates.Count);
                var pc = crewmates[rand];
                impostors.Add(pc);
                crewmates.Remove(pc);
            }

            // Hand out appropriate roles to crewmates, impostors & coven.
            foreach (var (type, _, unique) in crewRoles)
            {
                Role.GenRole<Role>(type, crewmates);
            }
            foreach (var (type, _, unique) in impRoles)
            {
                Role.GenRole<Role>(type, impostors);
            }
            foreach (var (type, _, unique) in covenRoles)
            {
                Role.GenRole<Role>(type, coven);
            }

            // Assign vanilla roles to anyone who did not receive a role.
            foreach (var crewmate in crewmates)
                Role.GenRole<Role>(typeof(Crewmate), crewmate);

            foreach (var impostor in impostors)
                Role.GenRole<Role>(typeof(Impostor), impostor);

            foreach (var player in coven)
                Role.GenRole<Role>(typeof(Coven), player);

            // Hand out assassin ability to killers according to the settings.
            var canHaveAbility = PlayerControl.AllPlayerControls.ToArray().Where(player => player.Is(Faction.Impostors)).ToList();
            canHaveAbility.Shuffle();
            var canHaveAbility2 = PlayerControl.AllPlayerControls.ToArray().Where(player => player.Is(Faction.NeutralKilling)).ToList();
            canHaveAbility2.Shuffle();

            var assassinConfig = new (List<PlayerControl>, int)[]
            {
                (canHaveAbility, CustomGameOptions.NumberOfImpostorAssassins),
                (canHaveAbility2, CustomGameOptions.NumberOfNeutralAssassins)
            };
            foreach ((var abilityList, int maxNumber) in assassinConfig)
            {
                int assassinNumber = maxNumber;
                if (CustomGameOptions.AssassinImpostorRole) assassinNumber = 0;
                while (abilityList.Count > 0 && assassinNumber > 0)
                {
                    var (type, rpc, _) = AssassinAbility.Ability();
                    Role.Gen<Ability>(type, abilityList.TakeFirst(), rpc);
                    assassinNumber -= 1;
                }
            }

            // Hand out assassin modifiers, if enabled, to impostor assassins.
            var canHaveAssassinModifier = PlayerControl.AllPlayerControls.ToArray().Where(player => player.Is(Faction.Impostors) && player.Is(AbilityEnum.Assassin)).ToList();
            canHaveAssassinModifier.Shuffle();
            AssassinModifiers.Shuffle();

            foreach (var (type, _) in AssassinModifiers)
            {
                if (canHaveAssassinModifier.Count == 0) break;
                Role.GenModifier<Modifier>(type, canHaveAssassinModifier);
            }

            // Hand out impostor modifiers.
            var canHaveImpModifier = PlayerControl.AllPlayerControls.ToArray().Where(player => player.Is(Faction.Impostors) && !player.Is(ModifierEnum.DoubleShot)).ToList();
            canHaveImpModifier.Shuffle();
            ImpostorModifiers.Shuffle();

            foreach (var (type, _) in ImpostorModifiers)
            {
                if (canHaveImpModifier.Count == 0) break;
                Role.GenModifier<Modifier>(type, canHaveImpModifier);
            }

            // Now hand out Crewmate Modifiers to all remaining eligible players.
            var impscount = PlayerControl.AllPlayerControls.ToArray().Where(player => player.Is(Faction.Impostors)).ToList();
            if (impscount.Count == 0 && CrewmateModifiers.Contains((typeof(Madmate), CustomGameOptions.MadmateOn)))
            {
                PluginSingleton<TownOfUsEdited>.Instance.Log.LogWarning("Madmate won't spawn because there are no Impostors");
                CrewmateModifiers.RemoveAll(x => x == (typeof(Madmate), CustomGameOptions.MadmateOn));
            }
            var canHaveCrewModifier = PlayerControl.AllPlayerControls.ToArray().Where(player => player.Is(Faction.Crewmates)).ToList();
            canHaveCrewModifier.Shuffle();
            CrewmateModifiers.Shuffle();

            foreach (var (type, _) in CrewmateModifiers)
            {
                if (canHaveCrewModifier.Count == 0) break;
                Role.GenModifier<Modifier>(type, canHaveCrewModifier);
            }

            // Hand out global modifiers.
            var canHaveModifier = PlayerControl.AllPlayerControls.ToArray().ToList();
            canHaveModifier.Shuffle();
            GlobalModifiers.Shuffle();

            foreach (var (type, id) in GlobalModifiers)
            {
                if (type.FullName.Contains("Lover")) Lover.Gen(PlayerControl.AllPlayerControls.ToArray().ToList());
                else if (canHaveModifier.Count > 0) Role.GenModifier<Modifier>(type, canHaveModifier);
            }

            // Set the Traitor, if there is one enabled.
            var toChooseFromCrew = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(Faction.Crewmates) && !x.Is(RoleEnum.Politician) && !x.Is(ModifierEnum.Lover)).ToList();
            if (TraitorOn && toChooseFromCrew.Count != 0)
            {
                var rand = Random.RandomRangeInt(0, toChooseFromCrew.Count);
                var pc = toChooseFromCrew[rand];

                SetTraitor.WillBeTraitor = pc;

                Utils.Rpc(CustomRPC.SetTraitor, pc.PlayerId);
            }
            else
            {
                Utils.Rpc(CustomRPC.SetTraitor, byte.MaxValue);
            }

            var toChooseFromNeut = PlayerControl.AllPlayerControls.ToArray().Where(x => (x.Is(Faction.NeutralBenign) || x.Is(Faction.NeutralEvil) || x.Is(Faction.NeutralKilling)) && !x.Is(ModifierEnum.Lover)).ToList();
            if (PhantomOn && toChooseFromNeut.Count != 0)
            {
                var rand = Random.RandomRangeInt(0, toChooseFromNeut.Count);
                var pc = toChooseFromNeut[rand];

                SetPhantom.WillBePhantom = pc;

                Utils.Rpc(CustomRPC.SetPhantom, pc.PlayerId);
            }
            else
            {
                Utils.Rpc(CustomRPC.SetPhantom, byte.MaxValue);
            }

            var exeTargets = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(Faction.Crewmates) && !x.Is(ModifierEnum.Lover) && !x.Is(RoleEnum.Politician) && !x.Is(RoleEnum.Prosecutor) && !x.Is(RoleEnum.Swapper) && !x.Is(RoleEnum.Vigilante) && x != SetTraitor.WillBeTraitor).ToList();
            foreach (var role in Role.GetRoles(RoleEnum.Executioner))
            {
                var exe = (Executioner)role;
                if (exeTargets.Count > 0)
                {
                    exe.target = exeTargets[Random.RandomRangeInt(0, exeTargets.Count)];
                    exeTargets.Remove(exe.target);

                    Utils.Rpc(CustomRPC.SetTarget, role.Player.PlayerId, exe.target.PlayerId);
                }
            }

            var goodGATargets = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(Faction.Crewmates) && !x.Is(ModifierEnum.Lover)).ToList();
            var evilGATargets = PlayerControl.AllPlayerControls.ToArray().Where(x => (x.Is(Faction.Impostors) || x.Is(Faction.NeutralKilling) || x.Is(Faction.Coven) || x.Is(Faction.Madmates)) && !x.Is(ModifierEnum.Lover)).ToList();
            foreach (var role in Role.GetRoles(RoleEnum.GuardianAngel))
            {
                var ga = (GuardianAngel)role;
                if (!(goodGATargets.Count == 0 && CustomGameOptions.EvilTargetPercent == 0) ||
                    (evilGATargets.Count == 0 && CustomGameOptions.EvilTargetPercent == 100) ||
                    goodGATargets.Count == 0 && evilGATargets.Count == 0)
                {
                    if (goodGATargets.Count == 0)
                    {
                        ga.target = evilGATargets[Random.RandomRangeInt(0, evilGATargets.Count)];
                        evilGATargets.Remove(ga.target);
                    }
                    else if (evilGATargets.Count == 0 || !Check(CustomGameOptions.EvilTargetPercent))
                    {
                        ga.target = goodGATargets[Random.RandomRangeInt(0, goodGATargets.Count)];
                        goodGATargets.Remove(ga.target);
                    }
                    else
                    {
                        ga.target = evilGATargets[Random.RandomRangeInt(0, evilGATargets.Count)];
                        evilGATargets.Remove(ga.target);
                    }

                    Utils.Rpc(CustomRPC.SetGATarget, role.Player.PlayerId, ga.target.PlayerId);
                }
            }
        }

        private static void GenEachRoleWerewolf(List<NetworkedPlayerInfo> infected, int impsCount)
        {
            var crewmates = Utils.GetPlayers(infected);
            var impostors = new List<PlayerControl>();
            // I do not shuffle impostors/crewmates because roles should be shuffled before they are assigned to them anyway.
            // Assigning shuffled roles across a shuffled list may mess with the statistics? I dunno, I didn't major in math.
            // One Fisher-Yates shuffle should have statistically equal permutation probability on its own, anyway.

            var crewRoles = new List<(Type, int, bool)>();
            var impRoles = new List<(Type, int, bool)>();

            //Crew Roles
            if (CustomGameOptions.VillagerOn > 0) crewRoles.Add((typeof(Villager), CustomGameOptions.VillagerOn, true));
            if (CustomGameOptions.SorcererOn > 0) crewRoles.Add((typeof(Sorcerer), CustomGameOptions.SorcererOn, true));
            if (CustomGameOptions.WerewolfSeerOn > 0) crewRoles.Add((typeof(Seer), CustomGameOptions.WerewolfSeerOn, true));
            if (CustomGameOptions.WerewolfProsecutorOn > 0) crewRoles.Add((typeof(Prosecutor), CustomGameOptions.WerewolfProsecutorOn, true));
            if (CustomGameOptions.WerewolfMayorOn > 0) crewRoles.Add((typeof(Mayor), CustomGameOptions.WerewolfMayorOn, true));
            if (CustomGameOptions.SoulCatcherOn > 0) crewRoles.Add((typeof(SoulCatcher), CustomGameOptions.SoulCatcherOn, true));
            if (CustomGameOptions.WerewolfChameleonOn > 0) crewRoles.Add((typeof(Chameleon), CustomGameOptions.ChameleonOn, true));
            if (CustomGameOptions.GuardOn > 0) crewRoles.Add((typeof(Guard), CustomGameOptions.GuardOn, true));
            if (CustomGameOptions.WerewolfSheriffOn > 0) crewRoles.Add((typeof(Sheriff), CustomGameOptions.WerewolfSheriffOn, true));
            if (CustomGameOptions.WerewolfParanoïacOn > 0) crewRoles.Add((typeof(Paranoïac), CustomGameOptions.WerewolfParanoïacOn, true));

            //Neutral Roles (Added to Crew list since there's only one)
            if (CustomGameOptions.WhiteWolfOn > 0) crewRoles.Add((typeof(WhiteWolf), CustomGameOptions.WhiteWolfOn, true));

            //Impo Roles
            if (CustomGameOptions.BlackWolfOn > 0) impRoles.Add((typeof(BlackWolf), CustomGameOptions.BlackWolfOn, true));
            if (CustomGameOptions.BasicWerewolfOn > 0) impRoles.Add((typeof(Werewolf), CustomGameOptions.BasicWerewolfOn, true));
            if (CustomGameOptions.TalkativeWolfOn > 0) impRoles.Add((typeof(TalkativeWolf), CustomGameOptions.TalkativeWolfOn, true));

            // Generate Spectator role for host
            if (CustomGameOptions.SpectateHost)
            {
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data == GameData.Instance.GetHost())
                    {
                        Role.GenRole<Role>(typeof(Spectator), player);
                        crewmates.Remove(player);
                    }
                }
            }

            if (CustomGameOptions.SpectateHost)
            {
                if (Upped.Keys.Any(x => x.Data == GameData.Instance.GetHost()))
                {
                    Upped.Remove(Upped.Keys.FirstOrDefault(x => x.Data == GameData.Instance.GetHost()));
                }
            }

            var uppedPlayers = Upped;

            while (uppedPlayers.Count > 0 && CustomGameOptions.GameMode == GameMode.Werewolf && CustomGameOptions.AllowUp)
            {
                var players = Upped.Keys.ToArray();
                foreach (var pc in players)
                {
                    Upped.TryGetValue(pc, out var roleName);
                    var newRoleCrew = CrewmateRoles.Any(x => x.Item1.Name == roleName);
                    if (newRoleCrew)
                    {
                        Role.GenRole<Role>(CrewmateRoles.FirstOrDefault(x => x.Item1.Name == roleName).Item1, pc);
                        uppedPlayers.Remove(pc);
                        CrewmateRoles.Remove(CrewmateRoles.FirstOrDefault(x => x.Item1.Name == roleName));
                        crewmates.Remove(pc);
                    }
                    else if (roleName == "Villager")
                    {
                        Role.GenRole<Role>(typeof(Villager), pc);
                        uppedPlayers.Remove(pc);
                        crewmates.Remove(pc);
                    }
                    var newRoleImpo = ImpostorRoles.Any(x => x.Item1.Name == roleName);
                    if (newRoleImpo)
                    {
                        Role.GenRole<Role>(ImpostorRoles.FirstOrDefault(x => x.Item1.Name == roleName).Item1, pc);
                        uppedPlayers.Remove(pc);
                        ImpostorRoles.Remove(ImpostorRoles.FirstOrDefault(x => x.Item1.Name == roleName));
                        crewmates.Remove(pc);
                        impsCount--;
                    }
                    else if (roleName == "Werewolf")
                    {
                        Role.GenRole<Role>(typeof(Werewolf), pc);
                        uppedPlayers.Remove(pc);
                        crewmates.Remove(pc);
                        impsCount--;
                    }
                    if (uppedPlayers.ContainsKey(pc))
                    {
                        uppedPlayers.Remove(pc);
                    }
                }
            }
            while (impsCount > 0 && crewmates.Count > 0)
            {
                var rand = UnityEngine.Random.RandomRangeInt(0, crewmates.Count);
                var pc = crewmates[rand];
                impostors.Add(pc);
                crewmates.Remove(pc);
                impsCount -= 1;
            }

            crewRoles.SortRoles(crewmates.Count);
            impRoles.SortRoles(impostors.Count);

            // Shuffle roles before handing them out.
            // This should ensure a statistically equal chance of all permutations of roles.
            crewRoles.Shuffle();
            impRoles.Shuffle();

            // Hand out appropriate roles to crewmates and impostors.
            foreach (var (type, _, unique) in crewRoles)
            {
                Role.GenRole<Role>(type, crewmates);
            }
            foreach (var (type, _, unique) in impRoles)
            {
                Role.GenRole<Role>(type, impostors);
            }

            // Assign villager / werewolf roles to anyone who did not receive a role.
            foreach (var crewmate in crewmates)
                Role.GenRole<Role>(typeof(Villager), crewmate);

            foreach (var impostor in impostors)
                Role.GenRole<Role>(typeof(Werewolf), impostor);
        }

        private static void GenEachRoleChaos(List<NetworkedPlayerInfo> infected)
        {
            var players = Utils.GetPlayers(infected);
            var impostors = new List<PlayerControl>();
            int impsCount = 1;

            // Generate Spectator role for host
            if (CustomGameOptions.SpectateHost)
            {
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data == GameData.Instance.GetHost())
                    {
                        Role.GenRole<Role>(typeof(Spectator), player);
                        players.Remove(player);
                    }
                }
            }

            while (impsCount > 0 && players.Count > 0)
            {
                var rand = UnityEngine.Random.RandomRangeInt(0, players.Count);
                var pc = players[rand];
                impostors.Add(pc);
                players.Remove(pc);
                impsCount -= 1;
            }
            var crewmates = Utils.GetCrewmates(impostors);
            // I do not shuffle impostors/crewmates because roles should be shuffled before they are assigned to them anyway.
            // Assigning shuffled roles across a shuffled list may mess with the statistics? I dunno, I didn't major in math.
            // One Fisher-Yates shuffle should have statistically equal permutation probability on its own, anyway.

            // Assign Doctor / Undertaker roles to everyone
            foreach (var crewmate in crewmates)
                Role.GenRole<Role>(typeof(Doctor), crewmate);

            foreach (var impostor in impostors)
                Role.GenRole<Role>(typeof(Undertaker), impostor);
        }

        private static void GenEachRoleBattleRoyale(List<NetworkedPlayerInfo> infected)
        {
            var players = Utils.GetPlayers(infected);

            // Assign Player Role to everyone.
            foreach (var player in players)
                Role.GenRole<Role>(typeof(Player), player);
        }


        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        public static class HandleRpc
        {
            public static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                Assembly asm = typeof(Role).Assembly;

                byte readByte, readByte1, readByte2;
                sbyte readSByte, readSByte2;
                switch (callId)
                {
                    case 254:
                        var newRpc = (CustomRPC)reader.ReadInt32();
                        switch (newRpc)
                        {
                            case CustomRPC.Test:
                                System.Console.WriteLine("Rpc received!");
                                break;

                            case CustomRPC.SetRole:
                                var player = Utils.PlayerById(reader.ReadByte());
                                var rstring = reader.ReadString();
                                Activator.CreateInstance(asm.GetType(rstring), new object[] { player });
                                break;
                            case CustomRPC.SetModifier:
                                var player2 = Utils.PlayerById(reader.ReadByte());
                                var mstring = reader.ReadString();
                                Activator.CreateInstance(asm.GetType(mstring), new object[] { player2 });
                                break;

                            case CustomRPC.LoveWin:
                                var winnerlover = Utils.PlayerById(reader.ReadByte());
                                Modifier.GetModifier<Lover>(winnerlover).Win();
                                break;

                            case CustomRPC.NobodyWins:
                                Role.NobodyWinsFunc();
                                break;

                            case CustomRPC.SurvivorOnlyWin:
                                Role.SurvOnlyWin();
                                break;

                            case CustomRPC.VampireWin:
                                Role.VampWin();
                                break;

                            case CustomRPC.SKwin:
                                Role.SKWin();
                                break;

                            case CustomRPC.SetCouple:
                                var id = reader.ReadByte();
                                var id2 = reader.ReadByte();
                                var lover1 = Utils.PlayerById(id);
                                var lover2 = Utils.PlayerById(id2);

                                var modifierLover1 = new Lover(lover1);
                                var modifierLover2 = new Lover(lover2);

                                modifierLover1.OtherLover = modifierLover2;
                                modifierLover2.OtherLover = modifierLover1;

                                break;

                            case CustomRPC.Start:
                                readByte = reader.ReadByte();
                                Utils.ShowDeadBodies = false;
                                ShowShield.FirstRoundShielded = readByte == byte.MaxValue ? null : Utils.PlayerById(readByte);
                                ShowShield.DiedFirst = "";
                                Murder.KilledPlayers.Clear();
                                PerformRewind.Revived.Clear();
                                Role.NobodyWins = false;
                                Role.SurvOnlyWins = false;
                                Role.VampireWins = false;
                                Role.SKWins = false;
                                Role.CovenWins = false;
                                Role.ImpostorWins = false;
                                Role.CrewmateWins = false;
                                Role.ForceGameEnd = false;
                                SetSpirit.WillBeSpirit = null;
                                SetBlinder.WillBeBlinder = null;
                                SetFreezer.WillBeFreezer = null;
                                SetGuardian.WillBeGuardian = null;
                                SetHelper.WillBeHelper = null;
                                PickImpRole.GhostRoles.Clear();
                                PickCrewRole.GhostRoles.Clear();
                                ExileControllerPatch.lastExiled = null;
                                StartImitate.ImitatingPlayers.Clear();
                                KillButtonTarget.DontRevive = byte.MaxValue;
                                ConverterHudManagerUpdate.DontRevive = byte.MaxValue;
                                AddHauntPatch.AssassinatedPlayers.Clear();
                                HudUpdate.Zooming = false;
                                HudUpdate.ZoomStart();
                                break;

                            case CustomRPC.JanitorClean:
                                readByte1 = reader.ReadByte();
                                var janitorPlayer = Utils.PlayerById(readByte1);
                                var janitorRole = Role.GetRole<Janitor>(janitorPlayer);
                                readByte = reader.ReadByte();
                                var deadBodies = Object.FindObjectsOfType<DeadBody>();
                                foreach (var body in deadBodies)
                                    if (body.ParentId == readByte)
                                        Coroutines.Start(global::TownOfUsEdited.ImpostorRoles.JanitorMod.Coroutine.CleanCoroutine(body,
                                        janitorRole));

                                break;
                            case CustomRPC.VultureEat:
                                var vulturePlayer = Utils.PlayerById(reader.ReadByte());
                                var vultureRole = Role.GetRole<Vulture>(vulturePlayer);
                                var ThedeadBodies = Object.FindObjectsOfType<DeadBody>();
                                foreach (var body in ThedeadBodies)
                                    if (body.ParentId == reader.ReadByte())
                                        Coroutines.Start(global::TownOfUsEdited.NeutralRoles.VultureMod.Coroutine.EatCoroutine(body,
                                        vultureRole));

                                break;
                            case CustomRPC.Curse:
                                var witchPlayer = Utils.PlayerById(reader.ReadByte());
                                var witchRole = Role.GetRole<Witch>(witchPlayer);
                                var cursedPlayer = Utils.PlayerById(reader.ReadByte());
                                witchRole.CursedList.Add(cursedPlayer.PlayerId);
                                break;
                            case CustomRPC.Hex:
                                var hexmasterPlayer = Utils.PlayerById(reader.ReadByte());
                                var hexmasterRole = Role.GetRole<HexMaster>(hexmasterPlayer);
                                var hexedPlayer = Utils.PlayerById(reader.ReadByte());
                                hexmasterRole.Hexed.Add(hexedPlayer.PlayerId);
                                break;
                            case CustomRPC.ClearHex:
                                var hexmasterPlayer2 = Utils.PlayerById(reader.ReadByte());
                                var hexmasterRole2 = Role.GetRole<HexMaster>(hexmasterPlayer2);
                                hexmasterRole2.Hexed.Clear();
                                break;
                            case CustomRPC.SpiritualistControl:
                                var spiritualistPlayer = Utils.PlayerById(reader.ReadByte());
                                var spiritualistRole = Role.GetRole<Spiritualist>(spiritualistPlayer);
                                var controlledPlayer = Utils.PlayerById(reader.ReadByte());
                                spiritualistRole.ControlledPlayer = controlledPlayer;
                                break;
                            case CustomRPC.SetVoodooPlayer:
                                var vmPlayer = Utils.PlayerById(reader.ReadByte());
                                var vmRole = Role.GetRole<VoodooMaster>(vmPlayer);
                                var voodooPlayer = Utils.PlayerById(reader.ReadByte());
                                vmRole.VoodooPlayer = voodooPlayer;
                                break;
                            case CustomRPC.ForceEndGame:
                                if (Role.ForceGameEnd != true) Role.ForceGameEnd = true;
                                break;
                            case CustomRPC.EngineerFix:
                                if (ShipStatus.Instance.Systems.ContainsKey(SystemTypes.MushroomMixupSabotage))
                                {
                                    var mushroom = ShipStatus.Instance.Systems[SystemTypes.MushroomMixupSabotage].Cast<MushroomMixupSabotageSystem>();
                                    if (mushroom.IsActive) mushroom.currentSecondsUntilHeal = 0.1f;
                                }
                                break;

                            case CustomRPC.FixLights:
                                var lights = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                                lights.ActualSwitches = lights.ExpectedSwitches;
                                break;

                            case CustomRPC.Reveal:
                                var mayor = Utils.PlayerById(reader.ReadByte());
                                var mayorRole = Role.GetRole<Mayor>(mayor);
                                mayorRole.Revealed = true;
                                AddRevealButton.RemoveAssassin(mayorRole);
                                break;

                            case CustomRPC.Elect:
                                bool isMadmate = false;
                                var politician = Utils.PlayerById(reader.ReadByte());
                                if (politician.Is(Faction.Madmates)) isMadmate = true;
                                Role.RoleDictionary.Remove(politician.PlayerId);
                                var mayorRole2 = new Mayor(politician);
                                mayorRole2.Revealed = true;
                                AddRevealButton.RemoveAssassin(mayorRole2);
                                if (isMadmate) Utils.TurnMadmate(politician, false);
                                break;

                            case CustomRPC.Prosecute:
                                var prosecutor = Utils.PlayerById(reader.ReadByte());
                                var prosRole = Role.GetRole<Prosecutor>(prosecutor);
                                prosRole.ProsecuteThisMeeting = true;
                                var prosecutedPlayer = Utils.PlayerById(reader.ReadByte());
                                AllowExtraVotes.VoteForMe.Prosecute(prosecutor, prosecutedPlayer);
                                break;

                            case CustomRPC.DoppelMorph:
                                var doppel = Utils.PlayerById(reader.ReadByte());
                                var doppelRole2 = Role.GetRole<Doppelganger>(doppel);
                                var doppelTarget = Utils.PlayerById(reader.ReadByte());
                                var isReviver = reader.ReadBoolean();
                                doppelRole2.Transform(doppel, doppelTarget);
                                break;

                            case CustomRPC.Bite:
                                var newVamp = Utils.PlayerById(reader.ReadByte());
                                Bite.Convert(newVamp);
                                break;

                            case CustomRPC.WerewolfConvert:
                                var blackwolf = Utils.PlayerById(reader.ReadByte());
                                var blackwolfRole = Role.GetRole<BlackWolf>(blackwolf);
                                var newwolf = Utils.PlayerById(reader.ReadByte());
                                blackwolfRole.Convert(newwolf);
                                break;

                            case CustomRPC.SKConvert:
                                var newsk = Utils.PlayerById(reader.ReadByte());
                                SerialKiller.Convert(newsk);
                                break;

                            case CustomRPC.CovenConvert:
                                var newcoven = Utils.PlayerById(reader.ReadByte());
                                CovenLeader.Convert(newcoven);
                                break;

                            case CustomRPC.ImpConvert:
                                var newmad = Utils.PlayerById(reader.ReadByte());
                                Utils.TurnMadmate(newmad, false);
                                break;

                            case CustomRPC.UpdateCrusade:
                                var crusader = Utils.PlayerById(reader.ReadByte());
                                var crusRole = Role.GetRole<Crusader>(crusader);
                                crusRole.CrusadedPlayer = null;
                                break;

                            case CustomRPC.UpdateGuard:
                                var guard = Utils.PlayerById(reader.ReadByte());
                                var guardRole = Role.GetRole<Guard>(guard);
                                guardRole.ProtectedPlayer = null;
                                break;

                            case CustomRPC.SetGuard:
                                var guardPlayer = Utils.PlayerById(reader.ReadByte());
                                var guardedPlayer = Utils.PlayerById(reader.ReadByte());
                                var guardRole1 = Role.GetRole<Guard>(guardPlayer);
                                guardRole1.ProtectedPlayer = guardedPlayer;
                                break;

                            case CustomRPC.DoctorPopUp:
                                var revivedPlayer = Utils.PlayerById(reader.ReadByte());
                                if (PlayerControl.LocalPlayer == revivedPlayer)
                                {
                                    HudManager.Instance.ShowPopUp("You have been resurected, you can no longer talk.");
                                }
                                break;

                            case CustomRPC.SetJail:
                                var jailorPlayer = Utils.PlayerById(reader.ReadByte());
                                var jailedPlayer = Utils.PlayerById(reader.ReadByte());
                                var jailorRole1 = Role.GetRole<Jailor>(jailorPlayer);
                                jailorRole1.JailedPlayer = jailedPlayer;
                                if (PlayerControl.LocalPlayer == jailedPlayer)
                                {
                                    Coroutines.Start(UpdateJailButton.Jail(jailedPlayer));
                                    JailorChat.UpdateJailorChat();
                                    HudManager.Instance.ShowPopUp("You have been jailed, you can't use any special ability and can't guess next meeting unless the Jailor releases you.");
                                }
                                break;

                            case CustomRPC.ReleaseJail:
                                var jailorPlayer2 = Utils.PlayerById(reader.ReadByte());
                                var jailorRole3 = Role.GetRole<Jailor>(jailorPlayer2);
                                var jailedPlayer2 = jailorRole3.JailedPlayer;
                                if (PlayerControl.LocalPlayer == jailedPlayer2)
                                {
                                    jailorRole3.JailedPlayer = null;
                                    JailorChat.UpdateJailorChat();
                                    HudManager.Instance.ShowPopUp("The Jailor has decided to release you... for now...");
                                }
                                break;

                            case CustomRPC.SpiritKill:
                                var spiritPlayer1 = Utils.PlayerById(reader.ReadByte());
                                var toDie6 = Utils.PlayerById(reader.ReadByte());
                                Utils.MurderPlayer(spiritPlayer1, toDie6, false);
                                break;

                            case CustomRPC.SetTalkativeWord:
                                var word = reader.ReadString();
                                ChatPatch.Word = word;
                                var impos = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Data.Disconnected && x.Is(Faction.Impostors)).ToArray();
                                foreach (var impPlayer in impos)
                                {
                                    var playerResults = $"Today's word is {word}, if the Talkative Wolf doesn't say it before the day ends, he will die!";
                                    if (!string.IsNullOrWhiteSpace(playerResults) && impPlayer == PlayerControl.LocalPlayer) DestroyableSingleton<HudManager>.Instance.Chat.AddChat(impPlayer, playerResults);
                                }
                                break;

                            case CustomRPC.SetCrusade:
                                var crusaderPlayer = Utils.PlayerById(reader.ReadByte());
                                var crusadedPlayer = Utils.PlayerById(reader.ReadByte());
                                var crusaderRole1 = Role.GetRole<Crusader>(crusaderPlayer);
                                crusaderRole1.CrusadedPlayer = crusadedPlayer;
                                break;

                            case CustomRPC.Transform:
                                readByte = reader.ReadByte();
                                var seekerPlayer = Utils.PlayerById(readByte);
                                var mutant = Role.GetRole<Mutant>(seekerPlayer);
                                seekerPlayer.MyPhysics.SetBodyType(PlayerBodyTypes.Seeker);
                                Coroutines.Start(Utils.FlashCoroutine(Colors.Mutant, 0.5f));
                                System.Console.WriteLine("Mutant FLASH");
                                mutant.IsTransformed = true;
                                break;

                            case CustomRPC.UnTransform:
                                readByte = reader.ReadByte();
                                var SeekerPlayer = Utils.PlayerById(readByte);
                                var mutant2 = Role.GetRole<Mutant>(SeekerPlayer);
                                SeekerPlayer.MyPhysics.SetBodyType(PlayerBodyTypes.Normal);
                                mutant2.IsTransformed = false;
                                break;

                            case CustomRPC.Animate:
                                if (Animations.waveController == null) Animations.waveController = AssetLoader.LoadController(AssetLoader.bundles.FirstOrDefault(x => x.Key == "touebundle").Value, "wave_0.controller");
                                if (Animations.boingController == null) Animations.boingController = AssetLoader.LoadController(AssetLoader.bundles.FirstOrDefault(x => x.Key == "touebundle").Value, "Boing_0.controller");
                                if (Animations.sleepyController == null) Animations.sleepyController = AssetLoader.LoadController(AssetLoader.bundles.FirstOrDefault(x => x.Key == "touebundle").Value, "sleepy_0.controller");
                                if (Animations.rollController == null) Animations.rollController = AssetLoader.LoadController(AssetLoader.bundles.FirstOrDefault(x => x.Key == "touebundle").Value, "roll_0.controller");
                                var animation = reader.ReadString();
                                var playerToAnimate = Utils.PlayerById(reader.ReadByte());
                                var hidePlayer = reader.ReadBoolean();
                                RuntimeAnimatorController Controller = null;
                                if (animation == "Bean Dance") Controller = Animations.boingController;
                                else if (animation == "Wave") Controller = Animations.waveController;
                                else if (animation == "Sleepy") Controller = Animations.sleepyController;
                                else if (animation == "Roll") Controller = Animations.rollController;
                                if (PlayerControl.LocalPlayer) Coroutines.Start(Animations.AnimatePlayer(Controller, playerToAnimate, hidePlayer));
                                else Coroutines.Start(WaitForLocalAnim(Controller, playerToAnimate, hidePlayer));
                                break;

                            case CustomRPC.Rewind:
                                var TimeLordPlayer = Utils.PlayerById(reader.ReadByte());
                                var TimeLordRole = Role.GetRole<TimeLord>(TimeLordPlayer);
                                PerformRewind.Rewind(TimeLordRole);
                                break;

                            case CustomRPC.RewindRevive:
                                var revivedRewind = Utils.PlayerById(reader.ReadByte());
                                PerformRewind.Revive(revivedRewind);
                                break;

                            case CustomRPC.RecreateTasks:
                                var taskPlayer = GameData.Instance.GetPlayerById(reader.ReadByte());
                                Utils.RecreateTasks(taskPlayer);
                                break;

                            case CustomRPC.AddTraitorRole:
                                var traitor = Utils.PlayerById(reader.ReadByte());
                                var traitorRole = Role.GetRole<Traitor>(traitor);
                                var selectedRole = (RoleEnum)reader.ReadByte();
                                SetTraitor.ChangeRole(traitorRole, selectedRole);
                                break;

                            case CustomRPC.Barrier:
                                var cleric = Utils.PlayerById(reader.ReadByte());
                                var clericRole = Role.GetRole<Cleric>(cleric);
                                switch (reader.ReadByte())
                                {
                                    default:
                                    case 0: //barrier
                                        var barriered = Utils.PlayerById(reader.ReadByte());
                                        clericRole.Barriered = barriered;
                                        clericRole.TimeRemaining = CustomGameOptions.BarrierCD;
                                        break;
                                    case 2: //attack
                                        if (PlayerControl.LocalPlayer == cleric) Coroutines.Start(Utils.FlashCoroutine(Color.blue));
                                        break;
                                    case 1: //cleanse
                                        var cleansed = Utils.PlayerById(reader.ReadByte());
                                        var count = reader.ReadByte();
                                        var effects = new List<EffectType>();
                                        for (int i = 0; i < count; i++)
                                        {
                                            effects.Add((EffectType)reader.ReadByte());
                                        }
                                        if (clericRole.CleansedPlayers.ContainsKey(cleansed.PlayerId))
                                        {
                                            foreach (var effect in effects)
                                            {
                                                if (!clericRole.CleansedPlayers[cleansed.PlayerId].Contains(effect))
                                                {
                                                    clericRole.CleansedPlayers[cleansed.PlayerId].Add(effect);
                                                }
                                            }
                                        }
                                        else clericRole.CleansedPlayers[cleansed.PlayerId] = effects;

                                        if (PlayerControl.LocalPlayer == cleansed)
                                        {
                                            if (effects.Contains(EffectType.Blind))
                                            {
                                                foreach (var b in Role.GetRoles(RoleEnum.Blinder))
                                                {
                                                    var blindRole = (Blinder)b;
                                                    blindRole.TimeRemaining = 0f;
                                                }
                                            }
                                            if (effects.Contains(EffectType.Flash))
                                            {
                                                foreach (var gr in Role.GetRoles(RoleEnum.Grenadier))
                                                {
                                                    var grRole = (Grenadier)gr;
                                                    grRole.TimeRemaining = 0f;
                                                }
                                            }
                                            if (effects.Contains(EffectType.Hack))
                                            {
                                                foreach (var g in Role.GetRoles(RoleEnum.Glitch))
                                                {
                                                    var gRole = (Glitch)g;
                                                    if (gRole.Hacked == PlayerControl.LocalPlayer) gRole.Hacked = null;
                                                }
                                            }
                                        }
                                        if (MeetingHud.Instance)
                                        {
                                            if (effects.Contains(EffectType.Blackmail) && cleansed.IsBlackmailed())
                                            {
                                                try
                                                {
                                                    foreach (var role in Role.GetRoles(RoleEnum.Blackmailer))
                                                    {
                                                        foreach (var voteArea in MeetingHud.Instance.playerStates)
                                                        {
                                                            var bmer = (Blackmailer)role;
                                                            if (voteArea.TargetPlayerId == bmer.Blackmailed.PlayerId)
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
                                                }
                                                catch { }
                                            }
                                            AddMeetingButtons.CleansePlayer(cleansed.PlayerId, effects);
                                        }
                                        break;
                                }
                                break;

                            case CustomRPC.WerewolfRampage:
                                readByte = reader.ReadByte();
                                var seekerPlayer1 = Utils.PlayerById(readByte);
                                var werewolf = Role.GetRole<Werewolf>(seekerPlayer1);
                                seekerPlayer1.MyPhysics.SetBodyType(PlayerBodyTypes.Seeker);
                                if (werewolf.Rampaged == true && MeetingHud.Instance)
                                {
                                    seekerPlayer1.MyPhysics.SetBodyType(PlayerBodyTypes.Normal);
                                    werewolf.Rampaged = false;
                                }
                                break;

                            case CustomRPC.WerewolfUnRampage:
                                readByte = reader.ReadByte();
                                var SeekerPlayer1 = Utils.PlayerById(readByte);
                                SeekerPlayer1.MyPhysics.SetBodyType(PlayerBodyTypes.Normal);
                                break;

                            case CustomRPC.SetSwaps:
                                readSByte = reader.ReadSByte();
                                SwapVotes.Swap1 =
                                    MeetingHud.Instance.playerStates.FirstOrDefault(x => x.TargetPlayerId == readSByte);
                                readSByte2 = reader.ReadSByte();
                                SwapVotes.Swap2 =
                                    MeetingHud.Instance.playerStates.FirstOrDefault(x => x.TargetPlayerId == readSByte2);
                                PluginSingleton<TownOfUsEdited>.Instance.Log.LogMessage("Bytes received - " + readSByte + " - " +
                                                                                  readSByte2);
                                break;

                            case CustomRPC.Imitate:
                                var imitator = Utils.PlayerById(reader.ReadByte());
                                var imitatorRole = Role.GetRole<Imitator>(imitator);
                                var imitateTarget = Utils.PlayerById(reader.ReadByte());
                                imitatorRole.ImitatePlayer = imitateTarget;
                                break;
                            case CustomRPC.StartImitate:
                                var imitator2 = Utils.PlayerById(reader.ReadByte());
                                if (imitator2.Is(RoleEnum.Traitor)) break;
                                var imitatorRole2 = Role.GetRole<Imitator>(imitator2);
                                StartImitate.Imitate(imitatorRole2);
                                break;
                            case CustomRPC.Remember:
                                readByte1 = reader.ReadByte();
                                readByte2 = reader.ReadByte();
                                var amnesiac = Utils.PlayerById(readByte1);
                                var other = Utils.PlayerById(readByte2);
                                PerformKillButton.Remember(Role.GetRole<Amnesiac>(amnesiac), other);
                                break;
                            case CustomRPC.Cure:
                                var oldVamp = Utils.PlayerById(reader.ReadByte());
                                PerformStake.Cure(oldVamp);
                                break;
                            case CustomRPC.Shift:
                                readByte1 = reader.ReadByte();
                                readByte2 = reader.ReadByte();
                                var shifter = Utils.PlayerById(readByte1);
                                var otherplayer = Utils.PlayerById(readByte2);
                                Coroutines.Start(PerformKill.Shift(Role.GetRole<Shifter>(shifter), otherplayer));
                                break;
                            case CustomRPC.Protect:
                                readByte1 = reader.ReadByte();
                                readByte2 = reader.ReadByte();

                                var medic = Utils.PlayerById(readByte1);
                                var shield = Utils.PlayerById(readByte2);
                                Role.GetRole<Medic>(medic).ShieldedPlayer = shield;
                                break;
                            case CustomRPC.Hypnotise:
                                var hypnotist = Utils.PlayerById(reader.ReadByte());
                                var hypnotistRole = Role.GetRole<Hypnotist>(hypnotist);
                                switch (reader.ReadByte())
                                {
                                    default:
                                    case 0: //set hypnosis
                                        var hypnotised = Utils.PlayerById(reader.ReadByte());
                                        hypnotistRole.HypnotisedPlayers.Add(hypnotised.PlayerId);
                                        break;
                                    case 1: //trigger hysteria
                                        hypnotistRole.HysteriaActive = true;
                                        hypnotistRole.Hysteria();
                                        break;
                                }
                                break;
                            case CustomRPC.AttemptSound:
                                var medicId = reader.ReadByte();
                                readByte = reader.ReadByte();
                                StopKill.BreakShield(medicId, readByte, CustomGameOptions.ShieldBreaks);
                                break;
                            case CustomRPC.BypassKill:
                                var killer = Utils.PlayerById(reader.ReadByte());
                                var target = Utils.PlayerById(reader.ReadByte());

                                Utils.MurderPlayer(killer, target, true);
                                break;
                            case CustomRPC.BypassMultiKill:
                                var killer2 = Utils.PlayerById(reader.ReadByte());
                                var target2 = Utils.PlayerById(reader.ReadByte());

                                Utils.MurderPlayer(killer2, target2, false);
                                break;
                            case CustomRPC.AssassinKill:
                                var toDie = Utils.PlayerById(reader.ReadByte());
                                var assassin = Utils.PlayerById(reader.ReadByte());
                                AssassinKill.MurderPlayer(toDie, assassin);
                                AssassinKill.AssassinKillCount(toDie, assassin);
                                break;
                            case CustomRPC.RitualistKill:
                                var toDie4 = Utils.PlayerById(reader.ReadByte());
                                var ritualist = Utils.PlayerById(reader.ReadByte());
                                RitualistKill.MurderPlayer(toDie4, ritualist);
                                RitualistKill.RitualistKillCount(toDie4, ritualist);
                                break;
                            case CustomRPC.VigilanteKill:
                                var toDie2 = Utils.PlayerById(reader.ReadByte());
                                var vigi = Utils.PlayerById(reader.ReadByte());
                                VigilanteKill.MurderPlayer(toDie2, vigi);
                                VigilanteKill.VigiKillCount(toDie2, vigi);
                                break;
                            case CustomRPC.WitchMurder:
                                var toDie5 = Utils.PlayerById(reader.ReadByte());
                                var witch = Utils.PlayerById(reader.ReadByte());
                                ApplyCurse.MurderPlayer(toDie5, witch, true);
                                break;
                            case CustomRPC.DoomsayerKill:
                                var toDie3 = Utils.PlayerById(reader.ReadByte());
                                var doom = Utils.PlayerById(reader.ReadByte());
                                DoomsayerKill.DoomKillCount(toDie3, doom);
                                if (!toDie3.IsBlessed()) DoomsayerKill.MurderPlayer(toDie3, doom);
                                else if (PlayerControl.LocalPlayer.Is(RoleEnum.Oracle) && toDie3.IsBlessed() && toDie3.GetOracle().Contains(Role.GetRole<Oracle>(PlayerControl.LocalPlayer))) Coroutines.Start(Utils.FlashCoroutine(Colors.Oracle));
                                break;
                            case CustomRPC.SetMimic:
                                var glitchPlayer = Utils.PlayerById(reader.ReadByte());
                                var mimicPlayer = Utils.PlayerById(reader.ReadByte());
                                var glitchRole = Role.GetRole<Glitch>(glitchPlayer);
                                glitchRole.MimicTarget = mimicPlayer;
                                glitchRole.IsUsingMimic = true;
                                glitchRole.Enabled = true;
                                Utils.Morph(glitchPlayer, mimicPlayer, playAnim:true);
                                break;
                            case CustomRPC.RpcResetAnim:
                                var animPlayer = Utils.PlayerById(reader.ReadByte());
                                var theGlitchRole = Role.GetRole<Glitch>(animPlayer);
                                theGlitchRole.MimicTarget = null;
                                theGlitchRole.IsUsingMimic = false;
                                Utils.Unmorph(theGlitchRole.Player);
                                break;
                            case CustomRPC.GlitchWin:
                                var theGlitch = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.Glitch);
                                ((Glitch)theGlitch)?.Wins();
                                break;
                            case CustomRPC.PlayerWin:
                                var targetplayer = Utils.PlayerById(reader.ReadByte());
                                var playerRole = Role.GetRole<Player>(targetplayer);
                                ((Player)playerRole)?.Wins();
                                break;
                            case CustomRPC.ImpostorWin:
                                Role.ImpostorWins = true;
                                break;
                            case CustomRPC.CrewmateWin:
                                Role.CrewmateWins = true;
                                break;
                            case CustomRPC.CovenWin:
                                Role.CovenWins = true;
                                break;
                            case CustomRPC.WhiteWolfWin:
                                var whitewolf = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.WhiteWolf);
                                ((WhiteWolf)whitewolf)?.Wins();
                                break;
                            case CustomRPC.MutantWin:
                                var Mutant = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.Mutant);
                                ((Mutant)Mutant)?.Wins();
                                break;
                            case CustomRPC.InfectiousWin:
                                var Infectious = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.Infectious);
                                ((Infectious)Infectious)?.Wins();
                                break;
                            case CustomRPC.VultureWin:
                                var Vulture = Utils.PlayerById(reader.ReadByte());
                                var VultureRole = Role.GetRole<Vulture>(Vulture);
                                VultureRole.Wins();
                                break;
                            case CustomRPC.DoomsayerWin:
                                var Doomsayer = Utils.PlayerById(reader.ReadByte());
                                var DoomRole = Role.GetRole<Doomsayer>(Doomsayer);
                                DoomRole.Wins();
                                break;
                            case CustomRPC.JuggernautWin:
                                var juggernaut = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.Juggernaut);
                                ((Juggernaut)juggernaut)?.Wins();
                                break;
                            case CustomRPC.SetHacked:
                                var hackPlayer = Utils.PlayerById(reader.ReadByte());
                                if (hackPlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                                {
                                    var glitch = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.Glitch);
                                    ((Glitch)glitch)?.SetHacked(hackPlayer);
                                }

                                break;
                            case CustomRPC.Morph:
                                var morphling = Utils.PlayerById(reader.ReadByte());
                                var morphTarget = Utils.PlayerById(reader.ReadByte());
                                var morphRole = Role.GetRole<Morphling>(morphling);
                                morphRole.MorphedPlayer = morphTarget;
                                Coroutines.Start(PerformKillMorphling.Morph(morphRole));
                                break;
                            case CustomRPC.UnMorph:
                                var morphling1 = Utils.PlayerById(reader.ReadByte());
                                var morphRole1 = Role.GetRole<Morphling>(morphling1);
                                morphRole1.Unmorph();
                                break;
                            case CustomRPC.UnMorphReviver:
                                var reviver2 = Utils.PlayerById(reader.ReadByte());
                                Utils.Unmorph(reviver2);
                                break;
                            case CustomRPC.Poison:
                                var poisoner = Utils.PlayerById(reader.ReadByte());
                                var poisoned = Utils.PlayerById(reader.ReadByte());
                                var poisonerRole = Role.GetRole<Poisoner>(poisoner);
                                poisonerRole.PoisonedPlayer = poisoned;
                                break;
                            case CustomRPC.PoisonKill:
                                var poisoner1 = Utils.PlayerById(reader.ReadByte());
                                var poisonerRole1 = Role.GetRole<Poisoner>(poisoner1);
                                var success = reader.ReadBoolean();
                                var poisonedPlayer2 = poisonerRole1.PoisonedPlayer;
                                poisonerRole1.PoisonedPlayer = null;
                                if (success)
                                {
                                    Utils.MurderPlayer(poisonerRole1.Player, poisonedPlayer2, false);
                                }
                                break;
                            case CustomRPC.SetTarget:
                                var exe = Utils.PlayerById(reader.ReadByte());
                                var exeTarget = Utils.PlayerById(reader.ReadByte());
                                var exeRole = Role.GetRole<Executioner>(exe);
                                exeRole.target = exeTarget;
                                break;
                            case CustomRPC.SetGATarget:
                                var ga = Utils.PlayerById(reader.ReadByte());
                                var gaTarget = Utils.PlayerById(reader.ReadByte());
                                var gaRole = Role.GetRole<GuardianAngel>(ga);
                                gaRole.target = gaTarget;
                                break;
                            case CustomRPC.Blackmail:
                                var blackmailer = Role.GetRole<Blackmailer>(Utils.PlayerById(reader.ReadByte()));
                                var blackmailed = Utils.PlayerById(reader.ReadByte());
                                switch (reader.ReadByte())
                                {
                                    default:
                                    case 0:
                                        if (AmongUsClient.Instance.AmHost && !MeetingHud.Instance)
                                        {
                                            blackmailer.Blackmailed = blackmailed;
                                            Utils.Rpc(CustomRPC.Blackmail, blackmailer.Player.PlayerId, blackmailer.Blackmailed.PlayerId, (byte)1);
                                        }
                                        break;
                                    case 1:
                                        if (PlayerControl.LocalPlayer == blackmailer.Player && blackmailer.Blackmailed != null)
                                        {
                                            blackmailer.Blackmailed?.myRend().material.SetFloat("_Outline", 0f);
                                            if (blackmailer.Blackmailed.Data.IsImpostor())
                                            {
                                                if (blackmailer.Blackmailed.GetCustomOutfitType() != CustomPlayerOutfitType.Camouflage &&
                                                    blackmailer.Blackmailed.GetCustomOutfitType() != CustomPlayerOutfitType.Swooper)
                                                    blackmailer.Blackmailed.nameText().color = Patches.Colors.Impostor;
                                                else blackmailer.Blackmailed.nameText().color = Color.clear;
                                            }
                                        }
                                        blackmailer.Blackmailed = blackmailed;
                                        break;
                                }
                                break;
                            case CustomRPC.Confess:
                                var oracle = Role.GetRole<Oracle>(Utils.PlayerById(reader.ReadByte()));
                                oracle.Confessor = Utils.PlayerById(reader.ReadByte());
                                var faction = reader.ReadInt32();
                                if (faction == 0) oracle.RevealedFaction = Faction.Crewmates;
                                else if (faction == 1) oracle.RevealedFaction = Faction.NeutralEvil;
                                else if (faction == 3) oracle.RevealedFaction = Faction.Coven;
                                else oracle.RevealedFaction = Faction.Impostors;
                                break;
                            case CustomRPC.CheckMurder:
                                var murderKiller = Utils.PlayerById(reader.ReadByte());
                                var murderTarget = Utils.PlayerById(reader.ReadByte());
                                murderKiller.CheckMurder(murderTarget);
                                break;
                            case CustomRPC.Bless:
                                var oracle2 = Role.GetRole<Oracle>(Utils.PlayerById(reader.ReadByte()));
                                readByte = reader.ReadByte();
                                switch (readByte)
                                {
                                    case 0:
                                        oracle2.SavedBlessed = true;
                                        break;
                                    case 1:
                                        var blessed = Utils.PlayerById(reader.ReadByte());
                                        oracle2.Blessed = blessed;
                                        break;
                                    case 2:
                                        if (PlayerControl.LocalPlayer == oracle2.Player) Coroutines.Start(Utils.FlashCoroutine(Colors.Oracle));
                                        break;
                                }
                                break;
                            case CustomRPC.Flush:
                                readByte = reader.ReadByte();
                                switch (readByte)
                                {
                                    case 0:
                                        if (PlayerControl.LocalPlayer.inVent)
                                        {
                                            PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                                            PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();
                                            Coroutines.Start(Utils.FlashCoroutine(Patches.Colors.Plumber));
                                        }
                                        break;
                                    case 1:
                                        var plumber = Utils.PlayerById(reader.ReadByte());
                                        var plumberRole = Role.GetRole<Plumber>(plumber);
                                        readByte = reader.ReadByte();
                                        if (!plumberRole.FutureBlocks.Contains(readByte)) plumberRole.FutureBlocks.Add(readByte);
                                        break;
                                }
                                break;
                            case CustomRPC.TerroristSaveVote:
                                var terrorist = Role.GetRole<Terrorist>(Utils.PlayerById(reader.ReadByte()));
                                terrorist.SavedVote = true;
                                break;
                            case CustomRPC.ExecutionerToJester:
                                TargetColor.ExeToJes(Utils.PlayerById(reader.ReadByte()));
                                break;
                            case CustomRPC.GAToSurv:
                                GATargetColor.GAToSurv(Utils.PlayerById(reader.ReadByte()));
                                break;
                            case CustomRPC.Mine:
                                var ventId = reader.ReadInt32();
                                var miner = Utils.PlayerById(reader.ReadByte());
                                var minerRole = Role.GetRole<Miner>(miner);
                                var pos = reader.ReadVector2();
                                var zAxis = reader.ReadSingle();
                                PlaceVent.SpawnVent(ventId, minerRole, pos, zAxis);
                                break;
                            case CustomRPC.Swoop:
                                var swooper = Utils.PlayerById(reader.ReadByte());
                                var swooperRole = Role.GetRole<Swooper>(swooper);
                                Coroutines.Start(PerformKillSwooper.Swoop(swooperRole));
                                break;
                            case CustomRPC.ChameleonSwoop:
                                var chameleon = Utils.PlayerById(reader.ReadByte());
                                var chameleonRole = Role.GetRole<Chameleon>(chameleon);
                                Coroutines.Start(PerformKillChameleon.Swoop(chameleonRole));
                                break;
                            case CustomRPC.UnSwoop:
                                var swooper1 = Utils.PlayerById(reader.ReadByte());
                                var swooperRole1 = Role.GetRole<Swooper>(swooper1);
                                swooperRole1.UnSwoop();
                                break;
                            case CustomRPC.ChameleonUnSwoop:
                                var chameleon1 = Utils.PlayerById(reader.ReadByte());
                                var chameleonRole1 = Role.GetRole<Chameleon>(chameleon1);
                                chameleonRole1.UnSwoop();
                                break;
                            case CustomRPC.Camouflage:
                                var venerer = Utils.PlayerById(reader.ReadByte());
                                var venererRole = Role.GetRole<Venerer>(venerer);
                                venererRole.TimeRemaining = CustomGameOptions.AbilityDuration;
                                venererRole.Ability();
                                break;
                            case CustomRPC.SyncManipMovement:
                                var controlled = Utils.PlayerById(reader.ReadByte());
                                Vector2 newVel = new Vector2(reader.ReadSingle(), reader.ReadSingle());

                                if (controlled != null && controlled.AmOwner)
                                {
                                    controlled.MyPhysics.body.velocity = newVel;
                                }
                                break;
                            case CustomRPC.Fortify:
                                switch (reader.ReadByte())
                                {
                                    default:
                                    case 0: //set fortify
                                        var warden = Utils.PlayerById(reader.ReadByte());
                                        var fortified = Utils.PlayerById(reader.ReadByte());
                                        var wardenRole = Role.GetRole<Warden>(warden);
                                        wardenRole.Fortified = fortified;
                                        break;
                                    case 1: //fortify alert
                                        var wardenPlayer = Utils.PlayerById(reader.ReadByte());
                                        if (PlayerControl.LocalPlayer == wardenPlayer) Coroutines.Start(Utils.FlashCoroutine(Colors.Warden));
                                        break;
                                }
                                break;
                            case CustomRPC.Alert:
                                var veteran = Utils.PlayerById(reader.ReadByte());
                                var veteranRole = Role.GetRole<Veteran>(veteran);
                                veteranRole.TimeRemaining = CustomGameOptions.AlertDuration;
                                veteranRole.Alert();
                                break;
                            case CustomRPC.HelperAlert:
                                var helper = Utils.PlayerById(reader.ReadByte());
                                var alertedPlayer = Utils.PlayerById(reader.ReadByte());
                                var helperRole = Role.GetRole<Helper>(helper);
                                helperRole.TimeRemaining = CustomGameOptions.HelperDuration;
                                helperRole.AlertedPlayer = alertedPlayer;
                                if (PlayerControl.LocalPlayer == alertedPlayer)
                                {
                                    HudManager.Instance.ShowPopUp("Someone has alerted you of a nearby danger.");
                                }
                                helperRole.Alert();
                                break;
                            case CustomRPC.Guard:
                                var guard2 = Utils.PlayerById(reader.ReadByte());
                                var protectedPlayer = Utils.PlayerById(reader.ReadByte());
                                var guardRole2 = Role.GetRole<Guardian>(guard2);
                                guardRole2.TimeRemaining = CustomGameOptions.GuardDuration;
                                guardRole2.ProtectedPlayer = protectedPlayer;
                                guardRole2.Guard();
                                break;
                            case CustomRPC.Blind:
                                var blinder = Utils.PlayerById(reader.ReadByte());
                                var blindedPlayer = Utils.PlayerById(reader.ReadByte());
                                var blinderRole = Role.GetRole<Blinder>(blinder);
                                blinderRole.TimeRemaining = CustomGameOptions.BlindDuration;
                                blinderRole.BlindedPlayer = blindedPlayer;
                                blinderRole.Blind();
                                break;
                            case CustomRPC.Freeze:
                                var freezer = Utils.PlayerById(reader.ReadByte());
                                var frozenPlayer = Utils.PlayerById(reader.ReadByte());
                                var freezerRole = Role.GetRole<Freezer>(freezer);
                                freezerRole.TimeRemaining = CustomGameOptions.FreezeDuration;
                                freezerRole.FrozenPlayer = frozenPlayer;
                                if (PlayerControl.LocalPlayer == frozenPlayer)
                                {
                                    PlayerControl.LocalPlayer.NetTransform.Halt();
                                }
                                freezerRole.Freeze();
                                break;
                            case CustomRPC.TurnGhost:
                                var astral = Utils.PlayerById(reader.ReadByte());
                                var astralRole = Role.GetRole<Astral>(astral);
                                astralRole.TimeRemaining = CustomGameOptions.GhostDuration;
                                astralRole.TurnGhost(astral);
                                break;
                            case CustomRPC.Vest:
                                var surv = Utils.PlayerById(reader.ReadByte());
                                var survRole = Role.GetRole<Survivor>(surv);
                                survRole.TimeRemaining = CustomGameOptions.VestDuration;
                                survRole.Vest();
                                break;
                            case CustomRPC.GAProtect:
                                var ga2 = Utils.PlayerById(reader.ReadByte());
                                var ga2Role = Role.GetRole<GuardianAngel>(ga2);
                                ga2Role.TimeRemaining = CustomGameOptions.ProtectDuration;
                                ga2Role.Protect();
                                break;
                            case CustomRPC.Transport:
                                Coroutines.Start(Transporter.TransportPlayers(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean()));
                                break;
                            case CustomRPC.SetUntransportable:
                                if (PlayerControl.LocalPlayer.Is(RoleEnum.Transporter))
                                {
                                    Role.GetRole<Transporter>(PlayerControl.LocalPlayer).UntransportablePlayers.Add(reader.ReadByte(), DateTime.UtcNow);
                                }
                                break;
                            case CustomRPC.Mediate:
                                var mediatedPlayer = Utils.PlayerById(reader.ReadByte());
                                var medium = Role.GetRole<Medium>(Utils.PlayerById(reader.ReadByte()));
                                if (PlayerControl.LocalPlayer.PlayerId != mediatedPlayer.PlayerId) break;
                                medium.AddMediatePlayer(mediatedPlayer.PlayerId);
                                break;
                            case CustomRPC.FlashGrenade:
                                var grenadier = Utils.PlayerById(reader.ReadByte());
                                var grenadierRole = Role.GetRole<Grenadier>(grenadier);
                                grenadierRole.TimeRemaining = CustomGameOptions.GrenadeDuration;
                                byte playersFlashed = reader.ReadByte();
                                Il2CppSystem.Collections.Generic.List<PlayerControl> playerControlList = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                                for (int i = 0; i < playersFlashed; i++)
                                {
                                    playerControlList.Add(Utils.PlayerById(reader.ReadByte()));
                                }
                                grenadierRole.flashedPlayers = playerControlList;
                                grenadierRole.Flash();
                                break;
                            case CustomRPC.Bribe:
                                var mercenary = Utils.PlayerById(reader.ReadByte());
                                var mercenaryRole = Role.GetRole<Mercenary>(mercenary);
                                mercenaryRole.Bribed.Add(reader.ReadByte());
                                break;
                            case CustomRPC.ArsonistWin:
                                var theArsonistTheRole = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.Arsonist);
                                ((Arsonist)theArsonistTheRole)?.Wins();
                                break;
                            case CustomRPC.DoppelgangerWin:
                                var DoppelgangerRole = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.Doppelganger);
                                ((Doppelganger)DoppelgangerRole)?.Wins();
                                break;
                            case CustomRPC.SoulCollectorWin:
                                var soulCollector = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.SoulCollector);
                                ((SoulCollector)soulCollector)?.Wins();
                                break;
                            case CustomRPC.WerewolfWin:
                                var theWerewolfTheRole = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.Maul);
                                ((Maul)theWerewolfTheRole)?.Wins();
                                break;
                            case CustomRPC.PlaguebearerWin:
                                var thePlaguebearerTheRole = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.Plaguebearer);
                                ((Plaguebearer)thePlaguebearerTheRole)?.Wins();
                                break;
                            case CustomRPC.AttackerWin:
                                var theAttackerTheRole = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.Attacker);
                                ((Attacker)theAttackerTheRole)?.Wins();
                                break;
                            case CustomRPC.TerroristWin:
                                var theTerroristTheRole = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.Terrorist);
                                ((Terrorist)theTerroristTheRole)?.Wins();
                                break;
                            case CustomRPC.Infect:
                                var pb = Role.GetRole<Plaguebearer>(Utils.PlayerById(reader.ReadByte()));
                                pb.SpreadInfection(Utils.PlayerById(reader.ReadByte()), Utils.PlayerById(reader.ReadByte()));
                                break;
                            case CustomRPC.TurnPestilence:
                                Role.GetRole<Plaguebearer>(Utils.PlayerById(reader.ReadByte())).TurnPestilence();
                                break;
                            case CustomRPC.TurnTerrorist:
                                Role.GetRole<Attacker>(Utils.PlayerById(reader.ReadByte())).TurnTerrorist();
                                break;
                            case CustomRPC.PestilenceWin:
                                var thePestilenceTheRole = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.Pestilence);
                                ((Pestilence)thePestilenceTheRole)?.Wins();
                                break;
                            case CustomRPC.SyncCustomSettings:
                                Rpc.ReceiveRpc(reader, reader.BytesRemaining > 8);
                                break;
                            case CustomRPC.AltruistRevive:
                                readByte1 = reader.ReadByte();
                                var altruistPlayer = Utils.PlayerById(readByte1);
                                var altruistRole = Role.GetRole<Altruist>(altruistPlayer);
                                readByte = reader.ReadByte();
                                var theDeadBodies = Object.FindObjectsOfType<DeadBody>();
                                foreach (var body in theDeadBodies)
                                    if (body.ParentId == readByte)
                                    {
                                        if (body.ParentId == PlayerControl.LocalPlayer.PlayerId)
                                            Coroutines.Start(Utils.FlashCoroutine(Colors.Altruist,
                                                CustomGameOptions.ReviveDuration, 0.5f));

                                        Coroutines.Start(
                                            global::TownOfUsEdited.CrewmateRoles.AltruistMod.Coroutine.AltruistRevive(body,
                                                altruistRole));
                                    }

                                break;
                            case CustomRPC.SorcererRevive:
                                var sorcererPlayer = Utils.PlayerById(reader.ReadByte());
                                var sorcererRole = Role.GetRole<Sorcerer>(sorcererPlayer);
                                var targetbody1 = reader.ReadByte();
                                var theDeadBodies3 = Object.FindObjectsOfType<DeadBody>();
                                foreach (var body in theDeadBodies3)
                                    if (body.ParentId == targetbody1)
                                    {
                                        if (body.ParentId == PlayerControl.LocalPlayer.PlayerId)
                                            Coroutines.Start(Utils.FlashCoroutine(sorcererRole.Color,
                                                1f, 0.5f));

                                        Coroutines.Start(
                                            global::TownOfUsEdited.WerewolfRoles.SorcererMod.Coroutine.SorcererRevive(body,
                                                sorcererRole));
                                    }

                                break;
                            case CustomRPC.DoctorRevive:
                                var doctorPlayer = Utils.PlayerById(reader.ReadByte());
                                var targetbody = reader.ReadByte();
                                var doctorRole = Role.GetRole<Doctor>(doctorPlayer);
                                var DoctorDeadBodies = GameObject.FindObjectsOfType<DeadBody>();
                                foreach (var body in DoctorDeadBodies)
                                {
                                    if (body.ParentId == targetbody)
                                    {
                                        DocRevive.DoctorRevive(body, doctorRole);
                                    }
                                }
                                break;
                            case CustomRPC.StartWatch:
                                var lookout = Utils.PlayerById(reader.ReadByte());
                                var watched = Utils.PlayerById(reader.ReadByte());
                                var lookoutRole = Role.GetRole<Lookout>(lookout);
                                lookoutRole.WatchedPlayer = watched;
                                lookoutRole.TimeRemaining = CustomGameOptions.WatchDuration;
                                lookoutRole.StartWatching();
                                lookoutRole.IsWatching = true;
                                break;
                            case CustomRPC.StopWatch:
                                var lookout2 = Utils.PlayerById(reader.ReadByte());
                                var lookoutRole2 = Role.GetRole<Lookout>(lookout2);
                                lookoutRole2.StopWatching();
                                break;
                            case CustomRPC.SetManipulate:
                                var manipulatorPlayer = Utils.PlayerById(reader.ReadByte());
                                var manipulatedPlayer = Utils.PlayerById(reader.ReadByte());
                                var manipulatorRole = Role.GetRole<Manipulator>(manipulatorPlayer);
                                manipulatorRole.ManipulatedPlayer = manipulatedPlayer;
                                break;
                            case CustomRPC.StartManipulate:
                                var manipulatorPlayer2 = Utils.PlayerById(reader.ReadByte());
                                var manipulatorRole2 = Role.GetRole<Manipulator>(manipulatorPlayer2);
                                manipulatorRole2.IsManipulating = true;
                                manipulatorRole2.TimeRemaining = CustomGameOptions.ManipulationDuration;
                                manipulatorRole2.StartManipulation();
                                if (PlayerControl.LocalPlayer == manipulatorRole2.ManipulatedPlayer)
                                {
                                    if (PlayerControl.LocalPlayer.inVent)
                                    {
                                        PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                                        PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();
                                    }
                                    if (PlayerControl.LocalPlayer.moveable) PlayerControl.LocalPlayer.NetTransform.Halt();
                                }
                                break;
                            case CustomRPC.SetManipulateOff:
                                var manipulatorPlayer1 = Utils.PlayerById(reader.ReadByte());
                                var manipulatorRole1 = Role.GetRole<Manipulator>(manipulatorPlayer1);
                                manipulatorRole1.StopManipulation();
                                break;
                            case CustomRPC.BarryButton:
                                var buttonBarry = Utils.PlayerById(reader.ReadByte());

                                if (AmongUsClient.Instance.AmHost)
                                {
                                    MeetingRoomManager.Instance.reporter = buttonBarry;
                                    MeetingRoomManager.Instance.target = null;
                                    AmongUsClient.Instance.DisconnectHandlers.AddUnique(MeetingRoomManager.Instance
                                        .Cast<IDisconnectHandler>());
                                    if (GameManager.Instance.CheckTaskCompletion()) return;

                                    DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(buttonBarry);
                                    buttonBarry.RpcStartMeeting(null);
                                }
                                break;
                            case CustomRPC.Disperse:
                                byte teleports = reader.ReadByte();
                                Dictionary<byte, Vector2> coordinates = new Dictionary<byte, Vector2>();
                                for (int i = 0; i < teleports; i++)
                                {
                                    byte playerId = reader.ReadByte();
                                    Vector2 location = reader.ReadVector2();
                                    coordinates.Add(playerId, location);
                                }
                                Disperser.DispersePlayersToCoordinates(coordinates);
                                break;
                            case CustomRPC.BaitReport:
                                var baitKiller = Utils.PlayerById(reader.ReadByte());
                                var bait = Utils.PlayerById(reader.ReadByte());
                                baitKiller.ReportDeadBody(bait.Data);
                                break;
                            case CustomRPC.Drag:
                                readByte1 = reader.ReadByte();
                                var dienerPlayer = Utils.PlayerById(readByte1);
                                var dienerRole = Role.GetRole<Undertaker>(dienerPlayer);
                                readByte = reader.ReadByte();
                                var dienerBodies = Object.FindObjectsOfType<DeadBody>();
                                foreach (var body in dienerBodies)
                                    if (body.ParentId == readByte)
                                        dienerRole.CurrentlyDragging = body;

                                break;
                            case CustomRPC.Drop:
                                readByte1 = reader.ReadByte();
                                var v2 = reader.ReadVector2();
                                var v2z = reader.ReadSingle();
                                var dienerPlayer2 = Utils.PlayerById(readByte1);
                                var dienerRole2 = Role.GetRole<Undertaker>(dienerPlayer2);
                                var body2 = dienerRole2.CurrentlyDragging;
                                dienerRole2.CurrentlyDragging = null;

                                body2.transform.position = new Vector3(v2.x, v2.y, v2z);

                                break;
                            case CustomRPC.DoctorDrag:
                                readByte1 = reader.ReadByte();
                                var doctorPlayer1 = Utils.PlayerById(readByte1);
                                var doctorRole1 = Role.GetRole<Doctor>(doctorPlayer1);
                                readByte = reader.ReadByte();
                                var doctorBodies = Object.FindObjectsOfType<DeadBody>();
                                foreach (var body in doctorBodies)
                                    if (body.ParentId == readByte)
                                        doctorRole1.CurrentlyDragging = body;

                                break;
                            case CustomRPC.DoctorDrop:
                                readByte1 = reader.ReadByte();
                                var V2 = reader.ReadVector2();
                                var V2z = reader.ReadSingle();
                                var doctorPlayer2 = Utils.PlayerById(readByte1);
                                var doctorRole2 = Role.GetRole<Doctor>(doctorPlayer2);
                                var body3 = doctorRole2.CurrentlyDragging;
                                doctorRole2.CurrentlyDragging = null;

                                body3.transform.position = new Vector3(V2.x, V2.y, V2z);

                                break;
                            case CustomRPC.SetAssassin:
                                new Assassin(Utils.PlayerById(reader.ReadByte()));
                                break;
                            case CustomRPC.SetPhantom:
                                readByte = reader.ReadByte();
                                SetPhantom.WillBePhantom = readByte == byte.MaxValue ? null : Utils.PlayerById(readByte);
                                break;
                            case CustomRPC.CatchPhantom:
                                var phantomPlayer = Utils.PlayerById(reader.ReadByte());
                                Role.GetRole<Phantom>(phantomPlayer).Caught = true;
                                if (PlayerControl.LocalPlayer == phantomPlayer) HudManager.Instance.AbilityButton.gameObject.SetActive(true);
                                phantomPlayer.Exiled();
                                break;
                            case CustomRPC.PhantomWin:
                                var phantomWinner = Role.GetRole<Phantom>(Utils.PlayerById(reader.ReadByte()));
                                phantomWinner.CompletedTasks = true;
                                if (!CustomGameOptions.NeutralEvilWinEndsGame)
                                {
                                    phantomWinner.Caught = true;
                                    if (!PlayerControl.LocalPlayer.Is(RoleEnum.Phantom) || !CustomGameOptions.PhantomSpook || MeetingHud.Instance) return;
                                    byte[] toKill = MeetingHud.Instance.playerStates.Where(x => !Utils.PlayerById(x.TargetPlayerId).Is(RoleEnum.Pestilence)).Select(x => x.TargetPlayerId).ToArray();
                                    Role.GetRole(PlayerControl.LocalPlayer).PauseEndCrit = true;
                                    var pk = new PlayerMenu((x) =>
                                    {
                                        Utils.RpcMultiMurderPlayer(PlayerControl.LocalPlayer, x);
                                        Role.GetRole(PlayerControl.LocalPlayer).PauseEndCrit = false;
                                    }, (y) =>
                                    {
                                        return toKill.Contains(y.PlayerId);
                                    });
                                    Coroutines.Start(pk.Open(1f));
                                }
                                else
                                {
                                    if (AmongUsClient.Instance.AmHost) Coroutines.Start(Role.WaitForEnd());
                                }
                                break;
                            case CustomRPC.SetHaunter:
                                readByte = reader.ReadByte();
                                SetHaunter.WillBeHaunter = Utils.PlayerById(readByte);
                                break;
                            case CustomRPC.SetHelper:
                                readByte = reader.ReadByte();
                                SetHelper.WillBeHelper = Utils.PlayerById(readByte);
                                break;
                            case CustomRPC.SetGuardian:
                                readByte = reader.ReadByte();
                                SetGuardian.WillBeGuardian = Utils.PlayerById(readByte);
                                break;
                            case CustomRPC.SetSpirit:
                                readByte = reader.ReadByte();
                                SetSpirit.WillBeSpirit = Utils.PlayerById(readByte);
                                break;
                            case CustomRPC.SetBlinder:
                                readByte = reader.ReadByte();
                                SetBlinder.WillBeBlinder = Utils.PlayerById(readByte);
                                break;
                            case CustomRPC.SetFreezer:
                                readByte = reader.ReadByte();
                                SetFreezer.WillBeFreezer = Utils.PlayerById(readByte);
                                break;
                            case CustomRPC.CatchHaunter:
                                var haunterPlayer = Utils.PlayerById(reader.ReadByte());
                                Role.GetRole<Haunter>(haunterPlayer).Caught = true;
                                if (PlayerControl.LocalPlayer == haunterPlayer) HudManager.Instance.AbilityButton.gameObject.SetActive(true);
                                haunterPlayer.Exiled();
                                break;
                            case CustomRPC.CatchSpirit:
                                var spiritPlayer = Utils.PlayerById(reader.ReadByte());
                                Role.GetRole<Spirit>(spiritPlayer).Caught = true;
                                if (PlayerControl.LocalPlayer == spiritPlayer) HudManager.Instance.AbilityButton.gameObject.SetActive(true);
                                spiritPlayer.Exiled();
                                break;
                            case CustomRPC.SetTraitor:
                                readByte = reader.ReadByte();
                                SetTraitor.WillBeTraitor = readByte == byte.MaxValue ? null : Utils.PlayerById(readByte);
                                break;
                            case CustomRPC.TraitorSpawn:
                                var traitor2 = SetTraitor.WillBeTraitor;
                                var oldRole = Role.GetRole(traitor2);
                                var killsList = (oldRole.CorrectKills, oldRole.IncorrectKills, oldRole.CorrectAssassinKills, oldRole.IncorrectAssassinKills);
                                Role.RoleDictionary.Remove(traitor2.PlayerId);
                                var traitorRole2 = new Traitor(traitor2);
                                if (StartImitate.ImitatingPlayers.Contains(traitor2.PlayerId))
                                {
                                    traitorRole2.formerRole = RoleEnum.Imitator;
                                    StartImitate.ImitatingPlayers.Remove(traitor2.PlayerId);
                                }
                                else traitorRole2.formerRole = oldRole.RoleType;
                                traitorRole2.formerRole = oldRole.RoleType;
                                traitorRole2.CorrectKills = killsList.CorrectKills;
                                traitorRole2.IncorrectKills = killsList.IncorrectKills;
                                traitorRole2.CorrectAssassinKills = killsList.CorrectAssassinKills;
                                traitorRole2.IncorrectAssassinKills = killsList.IncorrectAssassinKills;
                                traitorRole2.RegenTask();
                                SetTraitor.TurnImp(traitor2);
                                break;
                            case CustomRPC.TurnImpostor:
                                var impostor = Utils.PlayerById(reader.ReadByte());
                                var oldRole2 = Role.GetRole(impostor);
                                var killsList2 = (oldRole2.CorrectKills, oldRole2.IncorrectKills, oldRole2.CorrectAssassinKills, oldRole2.IncorrectAssassinKills);
                                Role.RoleDictionary.Remove(impostor.PlayerId);
                                var impostorRole = new Impostor(impostor);
                                if (StartImitate.ImitatingPlayers.Contains(impostor.PlayerId))
                                {
                                    impostorRole.formerRole = RoleEnum.Imitator;
                                    StartImitate.ImitatingPlayers.Remove(impostor.PlayerId);
                                }
                                else impostorRole.formerRole = oldRole2.RoleType;
                                impostorRole.CorrectKills = killsList2.CorrectKills;
                                impostorRole.IncorrectKills = killsList2.IncorrectKills;
                                impostorRole.CorrectAssassinKills = killsList2.CorrectAssassinKills;
                                impostorRole.IncorrectAssassinKills = killsList2.IncorrectAssassinKills;
                                impostorRole.RegenTask();
                                ImpostorDeathUpdate.TurnImp(impostor);
                                break;
                            case CustomRPC.Escape:
                                var escapist = Utils.PlayerById(reader.ReadByte());
                                var escapistRole = Role.GetRole<Escapist>(escapist);
                                var escapePos = reader.ReadVector2();
                                escapistRole.EscapePoint = escapePos;
                                Escapist.Escape(escapist);
                                break;
                            case CustomRPC.ConverterRevive:
                                var converter = Utils.PlayerById(reader.ReadByte());
                                var converterRole = Role.GetRole<Converter>(converter);
                                var deadPlayer = reader.ReadByte();
                                var theDeadBodies4 = Object.FindObjectsOfType<DeadBody>();
                                foreach (var body in theDeadBodies4)
                                    if (body.ParentId == deadPlayer)
                                    {
                                        Converter.Revive(body);
                                    }
                                break;
                            case CustomRPC.ReviverRevive:
                                var reviver = Utils.PlayerById(reader.ReadByte());
                                var reviverRole = Role.GetRole<Reviver>(reviver);
                                var deadPlayer2 = reader.ReadByte();
                                var theDeadBodies5 = Object.FindObjectsOfType<DeadBody>();
                                foreach (var body in theDeadBodies5)
                                    if (body.ParentId == deadPlayer2)
                                    {
                                        Reviver.Revive(body, reviver);
                                    }
                                break;
                            case CustomRPC.Troll:
                                var troll = Utils.PlayerById(reader.ReadByte());
                                var trollRole = Role.GetRole<Troll>(troll);
                                var trolledPlayer = Utils.PlayerById(reader.ReadByte());
                                trollRole.TrollAbility(trolledPlayer, troll);
                                break;
                            case CustomRPC.TurnMadmate:
                                var madPlayer = Utils.PlayerById(reader.ReadByte());
                                Utils.TurnMadmate(madPlayer, false);
                                break;
                            case CustomRPC.TurnCrewmateTeam:
                                var crewPlayer = Utils.PlayerById(reader.ReadByte());
                                Utils.TurnCrewmateTeam(crewPlayer);
                                break;
                            /*case CustomRPC.Convert:
                                var convertedPlayer = Utils.PlayerById(reader.ReadByte());
                                Utils.Convert(convertedPlayer);
                                break;*/
                            case CustomRPC.RemoveAllBodies:
                                var buggedBodies = Object.FindObjectsOfType<DeadBody>();
                                foreach (var body in buggedBodies)
                                    body.gameObject.Destroy();
                                break;
                            case CustomRPC.SubmergedFixOxygen:
                                Patches.SubmergedCompatibility.RepairOxygen();
                                break;
                            case CustomRPC.SetPos:
                                var setplayer = Utils.PlayerById(reader.ReadByte());
                                setplayer.transform.position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                                break;
                            case CustomRPC.UnFreeze:
                                var frozenPlayer2 = Utils.PlayerById(reader.ReadByte());
                                if (!Utils.Rewinding() && PlayerControl.LocalPlayer == frozenPlayer2)
                                {
                                    PlayerControl.LocalPlayer.moveable = true;
                                }
                                break;
                            case CustomRPC.SetSettings:
                                readByte = reader.ReadByte();
                                GameOptionsManager.Instance.currentNormalGameOptions.MapId = readByte == byte.MaxValue ? (byte)0 : readByte;
                                GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
                                GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions.SetRoleRate(RoleTypes.Engineer, 0, 0);
                                GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, 0, 0);
                                GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions.SetRoleRate(RoleTypes.Tracker, 0, 0);
                                GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions.SetRoleRate(RoleTypes.Noisemaker, 0, 0);
                                GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
                                GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions.SetRoleRate(RoleTypes.Phantom, 0, 0);
                                RandomMap.AdjustSettings(readByte);
                                break;

                            case CustomRPC.HunterStalk:
                                var stalker = Utils.PlayerById(reader.ReadByte());
                                var stalked = Utils.PlayerById(reader.ReadByte());
                                Hunter hunterRole = Role.GetRole<Hunter>(stalker);
                                hunterRole.StalkDuration = CustomGameOptions.HunterStalkDuration;
                                hunterRole.StalkedPlayer = stalked;
                                hunterRole.Stalk();
                                break;
                            case CustomRPC.Retribution:
                                var lastVoted = Utils.PlayerById(reader.ReadByte());
                                var hunterPlayer = Utils.PlayerById(reader.ReadByte());
                                AssassinKill.MurderPlayer(lastVoted, hunterPlayer);
                                break;
                            case CustomRPC.UsePotion:
                                var pm = Utils.PlayerById(reader.ReadByte());
                                var pmRole = Role.GetRole<PotionMaster>(pm);
                                var potion = reader.ReadString();
                                pmRole.Potion = potion;
                                pmRole.UsePotion();
                                if (potion != "Invisibility") pmRole.TimeRemaining = CustomGameOptions.PotionDuration;
                                else Coroutines.Start(PerformKillPotionMaster.Swoop(pmRole));
                                break;
                            case CustomRPC.Execution:
                                var executed = Utils.PlayerById(reader.ReadByte());
                                var killer3 = Utils.PlayerById(reader.ReadByte());
                                var jailorRole2 = Role.GetRole<Jailor>(killer3);
                                if (executed != killer3 && !killer3.Is(Faction.Madmates))
                                {
                                    bool flag = jailorRole2.JailedPlayer.Is(Faction.Crewmates) || (jailorRole2.JailedPlayer.Is(Faction.NeutralBenign) && !CustomGameOptions.CanJailNB)
                                    || (jailorRole2.JailedPlayer.Is(Faction.NeutralKilling) && !CustomGameOptions.CanJailNK)
                                    || (jailorRole2.JailedPlayer.Is(Faction.Coven) && !CustomGameOptions.CanJailCoven)
                                    || (jailorRole2.JailedPlayer.Is(Faction.NeutralEvil) && !CustomGameOptions.CanJailNE)
                                    || (jailorRole2.JailedPlayer.Is(Faction.Madmates) && !CustomGameOptions.CanJailMad);
                                    if (flag)
                                    {
                                        jailorRole2.IncorrectKills++;
                                        jailorRole2.CanJail = false;
                                    }
                                    else jailorRole2.CorrectKills++;
                                }
                                else if (killer3.Is(Faction.Madmates))
                                {
                                    jailorRole2.Kills++;
                                }
                                jailorRole2.JailedPlayer = null;
                                jailorRole2.JailCell.Destroy();
                                AssassinKill.MurderPlayer(executed, killer3);
                                break;
                            case CustomRPC.TalkWolfDie:
                                var talkwolf = Utils.PlayerById(reader.ReadByte());
                                AssassinKill.MurderPlayer(talkwolf, talkwolf);
                                break;
                            case CustomRPC.SendCustomChat:
                                var sender = Utils.PlayerById(reader.ReadByte());
                                var text = reader.ReadString();
                                var chatType = reader.ReadString();
                                if (chatType == "ImpChat")
                                {
                                    if (!PlayerControl.LocalPlayer.Data.IsDead && ImpostorChat.ImpostorChatButton != null) ImpostorChat.ImpostorChatButton.AddChat(sender, text, false);
                                    else if (PlayerControl.LocalPlayer.Data.IsDead && Utils.ShowDeadBodies)
                                    {
                                        var text2 = "[Impostor Chat]\n" + text;
                                        HudManager.Instance.Chat.AddChat(sender, text2, false);
                                    }
                                }
                                else if (chatType == "CovenChat")
                                {
                                    if (!PlayerControl.LocalPlayer.Data.IsDead && CovenChat.CovenChatButton != null) CovenChat.CovenChatButton.AddChat(sender, text, false);
                                    else if (PlayerControl.LocalPlayer.Data.IsDead && Utils.ShowDeadBodies)
                                    {
                                        var text2 = "[Coven Chat]\n" + text;
                                        HudManager.Instance.Chat.AddChat(sender, text2, false);
                                    }
                                }
                                else if (chatType == "VampChat")
                                {
                                    if (!PlayerControl.LocalPlayer.Data.IsDead && VampireChat.VampireChatButton != null) VampireChat.VampireChatButton.AddChat(sender, text, false);
                                    else if (PlayerControl.LocalPlayer.Data.IsDead && Utils.ShowDeadBodies)
                                    {
                                        var text2 = "[Vampire Chat]\n" + text;
                                        HudManager.Instance.Chat.AddChat(sender, text2, false);
                                    }
                                }
                                else if (chatType == "SKChat")
                                {
                                    if (!PlayerControl.LocalPlayer.Data.IsDead && SerialKillerChat.SerialKillerChatButton != null) SerialKillerChat.SerialKillerChatButton.AddChat(sender, text, false);
                                    else if (PlayerControl.LocalPlayer.Data.IsDead && Utils.ShowDeadBodies)
                                    {
                                        var text2 = "[Serial Killer Chat]\n" + text;
                                        HudManager.Instance.Chat.AddChat(sender, text2, false);
                                    }
                                }
                                else if (chatType == "LoversChat")
                                {
                                    if (!PlayerControl.LocalPlayer.Data.IsDead && LoversChat.LoversChatButton != null) LoversChat.LoversChatButton.AddChat(sender, text, false);
                                    else if (PlayerControl.LocalPlayer.Data.IsDead && Utils.ShowDeadBodies)
                                    {
                                        var text2 = "[Lovers Chat]\n" + text;
                                        HudManager.Instance.Chat.AddChat(sender, text2, false);
                                    }
                                }
                                else if (chatType == "JailorChat")
                                {
                                    if (!PlayerControl.LocalPlayer.Data.IsDead && JailorChat.JailorChatButton != null) JailorChat.JailorChatButton.AddChat(sender, text, false);
                                    else if (PlayerControl.LocalPlayer.Data.IsDead && Utils.ShowDeadBodies)
                                    {
                                        var text2 = "[Jailor Chat]\n" + text;
                                        HudManager.Instance.Chat.AddChat(sender, text2, false);
                                    }
                                }
                                break;
                            case CustomRPC.AbilityTrigger:
                                var abilityUser = Utils.PlayerById(reader.ReadByte());
                                var abilitytargetId = reader.ReadByte();
                                var abilitytarget = abilitytargetId == byte.MaxValue ? null : Utils.PlayerById(abilitytargetId);
                                if (PlayerControl.LocalPlayer.Is(ModifierEnum.SixthSense) && !PlayerControl.LocalPlayer.Data.IsDead && abilitytarget == PlayerControl.LocalPlayer)
                                {
                                    Coroutines.Start(Utils.FlashCoroutine(Colors.SixthSense));
                                }
                                foreach (Role hunterRole2 in Role.GetRoles(RoleEnum.Hunter))
                                {
                                    Hunter hunter = (Hunter)hunterRole2;
                                    if (hunter.StalkedPlayer == abilityUser) hunter.RpcCatchPlayer(abilityUser);
                                }
                                if (PlayerControl.LocalPlayer.Is(RoleEnum.Aurial) && !PlayerControl.LocalPlayer.Data.IsDead)
                                {
                                    var aurial = Role.GetRole<Aurial>(PlayerControl.LocalPlayer);
                                    Coroutines.Start(aurial.Sense(abilityUser));
                                }
                                else if (PlayerControl.LocalPlayer.Is(RoleEnum.Mercenary) && abilitytarget != null)
                                {
                                    var merc = Role.GetRole<Mercenary>(PlayerControl.LocalPlayer);
                                    if (merc.Guarded.Contains(abilitytargetId))
                                    {
                                        merc.Gold += 1;
                                    }
                                }
                                break;
                            case CustomRPC.SetDeathReason:
                                var deadPlayer3 = Utils.PlayerById(reader.ReadByte());
                                var deadPlayerRole = Role.GetRole(deadPlayer3);
                                var deathreason = (DeathReasons)reader.ReadByte();
                                switch (deathreason)
                                {
                                    case DeathReasons.Burned:
                                        deadPlayerRole.DeathReason = DeathReasons.Burned;
                                        break;
                                    case DeathReasons.Hexed:
                                        deadPlayerRole.DeathReason = DeathReasons.Hexed;
                                        break;
                                    case DeathReasons.Infected:
                                        deadPlayerRole.DeathReason = DeathReasons.Infected;
                                        break;
                                    case DeathReasons.Misfired:
                                        deadPlayerRole.DeathReason = DeathReasons.Misfired;
                                        break;
                                    case DeathReasons.Exiled:
                                        deadPlayerRole.DeathReason = DeathReasons.Exiled;
                                        break;
                                    case DeathReasons.Cursed:
                                        deadPlayerRole.DeathReason = DeathReasons.Cursed;
                                        break;
                                    case DeathReasons.Guessed:
                                        deadPlayerRole.DeathReason = DeathReasons.Guessed;
                                        break;
                                    case DeathReasons.Executed:
                                        deadPlayerRole.DeathReason = DeathReasons.Executed;
                                        break;
                                    case DeathReasons.Suicided:
                                        deadPlayerRole.DeathReason = DeathReasons.Suicided;
                                        break;
                                    case DeathReasons.Poisoned:
                                        deadPlayerRole.DeathReason = DeathReasons.Poisoned;
                                        break;
                                    case DeathReasons.Exploded:
                                        deadPlayerRole.DeathReason = DeathReasons.Exploded;
                                        break;
                                    case DeathReasons.Won:
                                        deadPlayerRole.DeathReason = DeathReasons.Won;
                                        break;
                                }
                                break;
                            case CustomRPC.InfectiousInfect:
                                var infectedPlayer = Utils.PlayerById(reader.ReadByte());
                                var infectious = Utils.PlayerById(reader.ReadByte());
                                var infectiousRole = Role.GetRole<Infectious>(infectious);
                                infectiousRole.Infected.Add(infectedPlayer.PlayerId);
                                break;
                            case CustomRPC.Camp:
                                var deputy = Utils.PlayerById(reader.ReadByte());
                                var deputyRole = Role.GetRole<Deputy>(deputy);
                                switch (reader.ReadByte())
                                {
                                    default: // the reason why I do both is in case of desync
                                    case 0: //camp
                                        var camp = Utils.PlayerById(reader.ReadByte());
                                        deputyRole.Camping = camp;
                                        break;
                                    case 1: //camp trigger
                                        var killerTarget = Utils.PlayerById(reader.ReadByte());
                                        deputyRole.Killer = killerTarget;
                                        deputyRole.Camping = null;
                                        break;
                                    case 2: //shoot
                                        var shot = Utils.PlayerById(reader.ReadByte());
                                        if (shot == deputyRole.Killer && !shot.Is(RoleEnum.Pestilence) && !shot.IsBlessed())
                                        {
                                            AddButtonDeputy.Shoot(deputyRole, shot);
                                            if (shot.Is(Faction.Crewmates)) deputyRole.IncorrectKills += 1;
                                            else deputyRole.CorrectKills += 1;
                                        }
                                        deputyRole.Killer = null;
                                        break;
                                }
                                break;

                            case CustomRPC.CheckForStatus:
                                var playerIdRequester = reader.ReadByte();
                                Coroutines.Start(ReceiveStatusCheck(playerIdRequester));
                                break;

                            case CustomRPC.SyncCustomStatus:
                                var playerIdSender = reader.ReadByte();
                                var friendCode2 = reader.ReadString();
                                var hidden2 = reader.ReadBoolean();
                                var playerToAdd4 = Utils.PlayerById(playerIdSender);
                                if (hidden2 == true) friendCode2 = "[HIDDEN]" + friendCode2;
                                if (playerToAdd4.FriendCode != friendCode2) playerToAdd4.FriendCode = friendCode2;
                                break;

                            case CustomRPC.ReceiveStatusCheck:
                                var playerToAdd3 = Utils.PlayerById(reader.ReadByte());
                                var playerIdRequester2 = reader.ReadByte();
                                if (PlayerControl.LocalPlayer.PlayerId != playerIdRequester2) break;
                                var friendCode = reader.ReadString();
                                var hidden = reader.ReadBoolean();
                                if (hidden == true) friendCode = "[HIDDEN]" + friendCode;
                                if (playerToAdd3.FriendCode != friendCode) playerToAdd3.FriendCode = friendCode;
                                break;

                            case CustomRPC.AddUp:
                                var playerToAdd2 = Utils.PlayerById(reader.ReadByte());
                                var roleToAdd = reader.ReadString();
                                System.Console.WriteLine("Received AddUp Rpc");
                                if (roleToAdd != "Cancel")
                                {
                                    if (!Upped.ContainsKey(playerToAdd2))
                                    {
                                        Upped.Add(playerToAdd2, roleToAdd);
                                    }
                                    else
                                    {
                                        Upped.Remove(playerToAdd2);
                                        Upped.Add(playerToAdd2, roleToAdd);
                                    }
                                }
                                else
                                {
                                    if (Upped.ContainsKey(playerToAdd2))
                                    {
                                        Upped.Remove(playerToAdd2);
                                    }
                                }
                                break;
                            case CustomRPC.CheckStatus:
                                var author = Utils.PlayerById(reader.ReadByte());
                                var targetCheck = Utils.PlayerById(reader.ReadByte());
                                if (PlayerControl.LocalPlayer == targetCheck)
                                {
                                    string status = "None";
                                    if (DevFeatures.Data.MainDev == EOSManager.Instance.FriendCode) status = "Main Dev";
                                    else if (PlayerControl.LocalPlayer.IsDev()) status = "Dev";
                                    else if (PlayerControl.LocalPlayer.IsTester()) status = "Tester";
                                    else if (PlayerControl.LocalPlayer.IsArtist()) status = "Artist";
                                    else if (PlayerControl.LocalPlayer.IsVip()) status = "VIP";
                                    string returnMessage = $"{targetCheck.Data.PlayerName}'s infos:\nLocal Status: {status}";
                                    Utils.Rpc(CustomRPC.ReceiveStatus, author.PlayerId, targetCheck.PlayerId, returnMessage);
                                }
                                break;
                            case CustomRPC.ReceiveStatus:
                                var receiver = Utils.PlayerById(reader.ReadByte());
                                var targetInfos = Utils.PlayerById(reader.ReadByte());
                                var returnedMessage = reader.ReadString();
                                if (receiver == PlayerControl.LocalPlayer)
                                {
                                    string status = "None";
                                    if (DevFeatures.Data.MainDev == targetInfos.FriendCode) status = "Main Dev";
                                    else if (targetInfos.IsDev()) status = "Dev";
                                    else if (targetInfos.IsTester()) status = "Tester";
                                    else if (targetInfos.IsArtist()) status = "Artist";
                                    else if (targetInfos.IsVip()) status = "VIP";
                                    returnedMessage += $"\nGlobal Status: {status}";
                                    DevFeatures.system = true;
                                    DestroyableSingleton<ChatController>.Instance.AddChat(receiver, returnedMessage, false);
                                }
                                break;
                            case CustomRPC.GetCode:
                                var author2 = Utils.PlayerById(reader.ReadByte());
                                var targetCheck2 = Utils.PlayerById(reader.ReadByte());
                                if (PlayerControl.LocalPlayer == targetCheck2)
                                {
                                    string returnMessage = $"{targetCheck2.Data.PlayerName}'s Friend Code Infos:\nFriend Code: {EOSManager.Instance.FriendCode}";
                                    Utils.Rpc(CustomRPC.ReceiveCode, author2.PlayerId, returnMessage);
                                }
                                break;
                            case CustomRPC.ReceiveCode:
                                var receiver2 = Utils.PlayerById(reader.ReadByte());
                                var returnedMessage2 = reader.ReadString();
                                if (receiver2 == PlayerControl.LocalPlayer)
                                {
                                    DevFeatures.system = true;
                                    DestroyableSingleton<ChatController>.Instance.AddChat(receiver2, returnedMessage2, false);
                                }
                                break;
                        }
                        break;
                }
            }

            public static IEnumerator ReceiveStatusCheck(byte playerIdRequester)
            {
                while (!PlayerControl.LocalPlayer) yield return null;
                Utils.Rpc(CustomRPC.ReceiveStatusCheck, PlayerControl.LocalPlayer.PlayerId, playerIdRequester, EOSManager.Instance.FriendCode, DevStatus.hidden);
                yield break;
            }
            
            public static IEnumerator WaitForLocalAnim(RuntimeAnimatorController controller, PlayerControl animatingPlayer, bool hidePlayer)
            {
                while (!PlayerControl.LocalPlayer) yield return null;
                yield return new WaitForSeconds(0.5f);
                Coroutines.Start(Animations.AnimatePlayer(controller, animatingPlayer, hidePlayer));
                yield break;
            }
        }

        [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
        public static class RpcSetRole
        {
            public static bool Prefix()
            {
                if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
                {
                    AmongUsClient.Instance.DisconnectWithReason("You were kicked for cheating, please stop.");
                    return false;
                }
                PluginSingleton<TownOfUsEdited>.Instance.Log.LogMessage("RPC SET ROLE");
                var infected = GameData.Instance.AllPlayers.ToArray();
                foreach (var data in infected)
                {
                    var player = Utils.PlayerByData(data);
                    player.RpcSetRole(RoleTypes.Crewmate, false);
                }

                // Get Impostors Count
                    int __result = 0;
                if (CustomGameOptions.GameMode == GameMode.Cultist || CustomGameOptions.GameMode == GameMode.Chaos)
                {
                    __result = 1;
                }
                else if ((CustomGameOptions.CovenReplaceImps && CustomGameOptions.GameMode != GameMode.Cultist && CustomGameOptions.GameMode != GameMode.Chaos && CustomGameOptions.GameMode != GameMode.Werewolf) || CustomGameOptions.GameMode == GameMode.BattleRoyale)
                {
                    __result = 0;
                }
                else if (CustomGameOptions.GameMode == GameMode.RoleList)
                {
                    List<RoleOptions> buckets = [CustomGameOptions.Slot1, CustomGameOptions.Slot2, CustomGameOptions.Slot3, CustomGameOptions.Slot4];
                    var anySlots = 0;
                    var players = GameData.Instance.PlayerCount;

                    if (players > 4) buckets.Add(CustomGameOptions.Slot5);
                    if (players > 5) buckets.Add(CustomGameOptions.Slot6);
                    if (players > 6) buckets.Add(CustomGameOptions.Slot7);
                    if (players > 7) buckets.Add(CustomGameOptions.Slot8);
                    if (players > 8) buckets.Add(CustomGameOptions.Slot9);
                    if (players > 9) buckets.Add(CustomGameOptions.Slot10);
                    if (players > 10) buckets.Add(CustomGameOptions.Slot11);
                    if (players > 11) buckets.Add(CustomGameOptions.Slot12);
                    if (players > 12) buckets.Add(CustomGameOptions.Slot13);
                    if (players > 13) buckets.Add(CustomGameOptions.Slot14);
                    if (players > 14) buckets.Add(CustomGameOptions.Slot15);

                    foreach (var roleOption in buckets)
                    {
                        if (roleOption == RoleOptions.Any) anySlots += 1;
                    }

                    int impProbability = (int)Math.Floor((double)players / anySlots * 5 / 3);
                    for (int i = 0; i < anySlots; i++)
                    {
                        var random = Random.RandomRangeInt(0, 100);
                        if (random < impProbability) __result += 1;
                        impProbability += 3;
                    }

                    if (__result == 0) __result = 1;
                }
                else __result = GameOptionsManager.Instance.currentNormalGameOptions.NumImpostors;

                Utils.ShowDeadBodies = false;
                if (ShowShield.DiedFirst != null && CustomGameOptions.FirstDeathShield)
                {
                    var shielded = false;
                    foreach (var player in PlayerControl.AllPlayerControls)
                    {
                        if (player.name == ShowShield.DiedFirst)
                        {
                            ShowShield.FirstRoundShielded = player;
                            shielded = true;
                        }
                    }
                    if (!shielded) ShowShield.FirstRoundShielded = null;
                }
                else ShowShield.FirstRoundShielded = null;
                ShowShield.DiedFirst = "";
                if (CustomGameOptions.GameMode == GameMode.BattleRoyale || CustomGameOptions.GameMode == GameMode.Chaos)
                {
                    ShowShield.FirstRoundShielded = null;
                }
                Role.NobodyWins = false;
                Role.SurvOnlyWins = false;
                Role.VampireWins = false;
                Role.SKWins = false;
                Role.CovenWins = false;
                Role.ImpostorWins = false;
                Role.CrewmateWins = false;
                Role.ForceGameEnd = false;
                SetSpirit.WillBeSpirit = null;
                SetFreezer.WillBeFreezer = null;
                SetBlinder.WillBeBlinder = null;
                SetGuardian.WillBeGuardian = null;
                SetHelper.WillBeHelper = null;
                PickImpRole.GhostRoles.Clear();
                PickCrewRole.GhostRoles.Clear();
                ExileControllerPatch.lastExiled = null;
                StartImitate.ImitatingPlayers.Clear();
                AddHauntPatch.AssassinatedPlayers.Clear();
                CrewmateInvestigativeRoles.Clear();
                CrewmateKillingRoles.Clear();
                CrewmatePowerRoles.Clear();
                CrewmateProtectiveRoles.Clear();
                CrewmateSupportRoles.Clear();
                ImpostorConcealingRoles.Clear();
                ImpostorKillingRoles.Clear();
                ImpostorSupportRoles.Clear();
                CovenSupportRoles.Clear();
                CovenKillingRoles.Clear();
                CrewmateRoles.Clear();
                CovenRoles.Clear();
                NeutralBenignRoles.Clear();
                NeutralEvilRoles.Clear();
                NeutralKillingRoles.Clear();
                ImpostorRoles.Clear();
                CrewmateModifiers.Clear();
                GlobalModifiers.Clear();
                ImpostorModifiers.Clear();
                AssassinModifiers.Clear();
                AssassinAbility.Clear();

                Murder.KilledPlayers.Clear();
                PerformRewind.Revived.Clear();
                KillButtonTarget.DontRevive = byte.MaxValue;
                ConverterHudManagerUpdate.DontRevive = byte.MaxValue;
                HudUpdate.Zooming = false;
                HudUpdate.ZoomStart();

                if (ShowShield.FirstRoundShielded != null)
                {
                    Utils.Rpc(CustomRPC.Start, ShowShield.FirstRoundShielded.PlayerId);
                }
                else
                {
                    Utils.Rpc(CustomRPC.Start, byte.MaxValue);
                }

                if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek) return true;

                if (CustomGameOptions.GameMode == GameMode.Classic)
                {
                    PhantomOn = Check(CustomGameOptions.PhantomOn);
                    TraitorOn = Check(CustomGameOptions.TraitorOn);
                }
                else
                {
                    PhantomOn = false;
                    TraitorOn = false;
                }

                if (CustomGameOptions.GameMode == GameMode.Classic || CustomGameOptions.GameMode == GameMode.Cultist
                || CustomGameOptions.GameMode == GameMode.RoleList)
                {
                    #region Crewmate Roles
                    if (CustomGameOptions.CrewmateOn > 0)
                        CrewmateRoles.Add((typeof(Crewmate), CustomGameOptions.CrewmateOn, true));

                    if (CustomGameOptions.PoliticianOn > 0)
                        CrewmateRoles.Add((typeof(Politician), CustomGameOptions.PoliticianOn, true));

                    if (CustomGameOptions.WardenOn > 0)
                        CrewmateRoles.Add((typeof(Warden), CustomGameOptions.WardenOn, true));

                    if (CustomGameOptions.SheriffOn > 0)
                        CrewmateRoles.Add((typeof(Sheriff), CustomGameOptions.SheriffOn, false));

                    if (CustomGameOptions.FighterOn > 0)
                        CrewmateRoles.Add((typeof(Fighter), CustomGameOptions.FighterOn, false));

                    if (CustomGameOptions.JailorOn > 0)
                        CrewmateRoles.Add((typeof(Jailor), CustomGameOptions.JailorOn, false));

                    if (CustomGameOptions.DeputyOn > 0)
                        CrewmateRoles.Add((typeof(Deputy), CustomGameOptions.DeputyOn, false));

                    if (CustomGameOptions.KnightOn > 0)
                        CrewmateRoles.Add((typeof(Knight), CustomGameOptions.KnightOn, false));

                    if (CustomGameOptions.EngineerOn > 0)
                        CrewmateRoles.Add((typeof(Engineer), CustomGameOptions.EngineerOn, false));

                    if (CustomGameOptions.InformantOn > 0)
                        CrewmateRoles.Add((typeof(Informant), CustomGameOptions.InformantOn, false));

                    if (CustomGameOptions.SwapperOn > 0)
                        CrewmateRoles.Add((typeof(Swapper), CustomGameOptions.SwapperOn, true));

                    if (CustomGameOptions.AvengerOn > 0)
                        CrewmateRoles.Add((typeof(Avenger), CustomGameOptions.AvengerOn, true));

                    if (CustomGameOptions.InvestigatorOn > 0)
                        CrewmateRoles.Add((typeof(Investigator), CustomGameOptions.InvestigatorOn, false));

                    if (CustomGameOptions.MedicOn > 0)
                        CrewmateRoles.Add((typeof(Medic), CustomGameOptions.MedicOn, true));

                    if (CustomGameOptions.AstralOn > 0)
                        CrewmateRoles.Add((typeof(Astral), CustomGameOptions.AstralOn, true));

                    if (CustomGameOptions.LookoutOn > 0)
                        CrewmateRoles.Add((typeof(Lookout), CustomGameOptions.LookoutOn, true));

                    if (CustomGameOptions.SeerOn > 0)
                        CrewmateRoles.Add((typeof(Seer), CustomGameOptions.SeerOn, false));

                    if (CustomGameOptions.SpyOn > 0 && GameOptionsManager.Instance.currentNormalGameOptions.MapId != 5)
                        CrewmateRoles.Add((typeof(Spy), CustomGameOptions.SpyOn, false));

                    if (CustomGameOptions.SnitchOn > 0)
                        CrewmateRoles.Add((typeof(Snitch), CustomGameOptions.SnitchOn, true));

                    if (CustomGameOptions.AltruistOn > 0)
                        CrewmateRoles.Add((typeof(Altruist), CustomGameOptions.AltruistOn, true));

                    if (CustomGameOptions.VigilanteOn > 0)
                        CrewmateRoles.Add((typeof(Vigilante), CustomGameOptions.VigilanteOn, false));

                    if (CustomGameOptions.VeteranOn > 0)
                        CrewmateRoles.Add((typeof(Veteran), CustomGameOptions.VeteranOn, false));

                    if (CustomGameOptions.HunterOn > 0)
                        CrewmateRoles.Add((typeof(Hunter), CustomGameOptions.HunterOn, false));

                    if (CustomGameOptions.TrackerOn > 0)
                        CrewmateRoles.Add((typeof(Tracker), CustomGameOptions.TrackerOn, false));

                    if (CustomGameOptions.TransporterOn > 0)
                        CrewmateRoles.Add((typeof(Transporter), CustomGameOptions.TransporterOn, false));

                    if (CustomGameOptions.TimeLordOn > 0)
                        CrewmateRoles.Add((typeof(TimeLord), CustomGameOptions.TimeLordOn, false));

                    if (CustomGameOptions.MediumOn > 0)
                        CrewmateRoles.Add((typeof(Medium), CustomGameOptions.MediumOn, false));

                    if (CustomGameOptions.MysticOn > 0)
                        CrewmateRoles.Add((typeof(Mystic), CustomGameOptions.MysticOn, false));

                    if (CustomGameOptions.TrapperOn > 0)
                        CrewmateRoles.Add((typeof(Trapper), CustomGameOptions.TrapperOn, false));

                    if (CustomGameOptions.CaptainOn > 0)
                        CrewmateRoles.Add((typeof(Captain), CustomGameOptions.CaptainOn, false));

                    if (CustomGameOptions.ParanoïacOn > 0)
                        CrewmateRoles.Add((typeof(Paranoïac), CustomGameOptions.ParanoïacOn, false));

                    if (CustomGameOptions.ChameleonOn > 0)
                        CrewmateRoles.Add((typeof(Chameleon), CustomGameOptions.ChameleonOn, false));

                    if (CustomGameOptions.DetectiveOn > 0)
                        CrewmateRoles.Add((typeof(Detective), CustomGameOptions.DetectiveOn, false));

                    if (CustomGameOptions.DoctorOn > 0)
                        CrewmateRoles.Add((typeof(Doctor), CustomGameOptions.DoctorOn, false));

                    if (CustomGameOptions.BodyguardOn > 0)
                        CrewmateRoles.Add((typeof(Bodyguard), CustomGameOptions.BodyguardOn, false));

                    if (CustomGameOptions.CrusaderOn > 0)
                        CrewmateRoles.Add((typeof(Crusader), CustomGameOptions.CrusaderOn, false));

                    if (CustomGameOptions.ImitatorOn > 0)
                        CrewmateRoles.Add((typeof(Imitator), CustomGameOptions.ImitatorOn, true));

                    if (CustomGameOptions.ProsecutorOn > 0)
                        CrewmateRoles.Add((typeof(Prosecutor), CustomGameOptions.ProsecutorOn, true));

                    if (CustomGameOptions.OracleOn > 0)
                        CrewmateRoles.Add((typeof(Oracle), CustomGameOptions.OracleOn, true));

                    if (CustomGameOptions.AurialOn > 0)
                        CrewmateRoles.Add((typeof(Aurial), CustomGameOptions.AurialOn, false));

                    if (CustomGameOptions.PlumberOn > 0)
                        CrewmateRoles.Add((typeof(Plumber), CustomGameOptions.PlumberOn, false || CustomGameOptions.UniqueRoles));

                    if (CustomGameOptions.ClericOn > 0)
                        CrewmateRoles.Add((typeof(Cleric), CustomGameOptions.ClericOn, false || CustomGameOptions.UniqueRoles));
                    #endregion
                    #region Neutral Roles
                    if (CustomGameOptions.JesterOn > 0)
                        NeutralEvilRoles.Add((typeof(Jester), CustomGameOptions.JesterOn, false || CustomGameOptions.UniqueRoles));

                    if (CustomGameOptions.VultureOn > 0)
                        NeutralEvilRoles.Add((typeof(Vulture), CustomGameOptions.VultureOn, false || CustomGameOptions.UniqueRoles));

                    if (CustomGameOptions.TrollOn > 0)
                        NeutralEvilRoles.Add((typeof(Troll), CustomGameOptions.TrollOn, false || CustomGameOptions.UniqueRoles));

                    if (CustomGameOptions.AmnesiacOn > 0)
                        NeutralBenignRoles.Add((typeof(Amnesiac), CustomGameOptions.AmnesiacOn, false || CustomGameOptions.UniqueRoles));

                    if (CustomGameOptions.ShifterOn > 0)
                        NeutralBenignRoles.Add((typeof(Shifter), CustomGameOptions.ShifterOn, false || CustomGameOptions.UniqueRoles));

                    if (CustomGameOptions.SoulCollectorOn > 0)
                        NeutralKillingRoles.Add((typeof(SoulCollector), CustomGameOptions.SoulCollectorOn, false || CustomGameOptions.UniqueRoles));

                    if (CustomGameOptions.ExecutionerOn > 0)
                        NeutralEvilRoles.Add((typeof(Executioner), CustomGameOptions.ExecutionerOn, false || CustomGameOptions.UniqueRoles));

                    if (CustomGameOptions.DoomsayerOn > 0)
                        NeutralEvilRoles.Add((typeof(Doomsayer), CustomGameOptions.DoomsayerOn, false || CustomGameOptions.UniqueRoles));

                    if (CustomGameOptions.SurvivorOn > 0)
                        NeutralBenignRoles.Add((typeof(Survivor), CustomGameOptions.SurvivorOn, false || CustomGameOptions.UniqueRoles));

                    if (CustomGameOptions.GuardianAngelOn > 0)
                        NeutralBenignRoles.Add((typeof(GuardianAngel), CustomGameOptions.GuardianAngelOn, false || CustomGameOptions.UniqueRoles));

                    if (CustomGameOptions.MercenaryOn > 0)
                        NeutralBenignRoles.Add((typeof(Mercenary), CustomGameOptions.MercenaryOn, false || CustomGameOptions.UniqueRoles));

                    if (CustomGameOptions.GlitchOn > 0)
                        NeutralKillingRoles.Add((typeof(Glitch), CustomGameOptions.GlitchOn, true));

                    if (CustomGameOptions.SerialKillerOn > 0)
                        NeutralKillingRoles.Add((typeof(SerialKiller), CustomGameOptions.SerialKillerOn, true));

                    if (CustomGameOptions.DoppelgangerOn > 0)
                        NeutralKillingRoles.Add((typeof(Doppelganger), CustomGameOptions.DoppelgangerOn, true));

                    if (CustomGameOptions.MutantOn > 0)
                        NeutralKillingRoles.Add((typeof(Mutant), CustomGameOptions.MutantOn, true));

                    if (CustomGameOptions.InfectiousOn > 0)
                        NeutralKillingRoles.Add((typeof(Infectious), CustomGameOptions.InfectiousOn, true));

                    if (CustomGameOptions.ArsonistOn > 0)
                        NeutralKillingRoles.Add((typeof(Arsonist), CustomGameOptions.ArsonistOn, true));

                    if (CustomGameOptions.PlaguebearerOn > 0)
                        NeutralKillingRoles.Add((typeof(Plaguebearer), CustomGameOptions.PlaguebearerOn, true));

                    if (CustomGameOptions.AttackerOn > 0)
                        NeutralKillingRoles.Add((typeof(Attacker), CustomGameOptions.AttackerOn, true));

                    if (CustomGameOptions.WerewolfOn > 0)
                        NeutralKillingRoles.Add((typeof(Maul), CustomGameOptions.WerewolfOn, true));

                    if (CustomGameOptions.GameMode == GameMode.Classic && CustomGameOptions.VampireOn > 0)
                        NeutralKillingRoles.Add((typeof(Vampire), CustomGameOptions.VampireOn, true));

                    if (CustomGameOptions.JuggernautOn > 0)
                        NeutralKillingRoles.Add((typeof(Juggernaut), CustomGameOptions.JuggernautOn, true));
                    #endregion
                    #region Impostor Roles
                    if (CustomGameOptions.ImpostorOn > 0)
                        ImpostorRoles.Add((typeof(Impostor), CustomGameOptions.ImpostorOn, false));

                    if (CustomGameOptions.PoisonerOn > 0)
                        ImpostorRoles.Add((typeof(Poisoner), CustomGameOptions.PoisonerOn, false));

                    if (CustomGameOptions.HypnotistOn > 0)
                        ImpostorRoles.Add((typeof(Hypnotist), CustomGameOptions.HypnotistOn, true));

                    if (CustomGameOptions.ShooterOn > 0)
                        ImpostorRoles.Add((typeof(Shooter), CustomGameOptions.ShooterOn, true));

                    if (CustomGameOptions.UndertakerOn > 0)
                        ImpostorRoles.Add((typeof(Undertaker), CustomGameOptions.UndertakerOn, true));

                    if (CustomGameOptions.MorphlingOn > 0)
                        ImpostorRoles.Add((typeof(Morphling), CustomGameOptions.MorphlingOn, false));

                    if (CustomGameOptions.AssassinOn > 0 && CustomGameOptions.AssassinImpostorRole)
                        ImpostorRoles.Add((typeof(Roles.Assassin), CustomGameOptions.AssassinOn, false));

                    if (CustomGameOptions.BlackmailerOn > 0)
                        ImpostorRoles.Add((typeof(Blackmailer), CustomGameOptions.BlackmailerOn, true));

                    if (CustomGameOptions.ConverterOn > 0)
                        ImpostorRoles.Add((typeof(Converter), CustomGameOptions.ConverterOn, true));

                    if (CustomGameOptions.MinerOn > 0)
                        ImpostorRoles.Add((typeof(Miner), CustomGameOptions.MinerOn, true));

                    if (CustomGameOptions.SwooperOn > 0)
                        ImpostorRoles.Add((typeof(Swooper), CustomGameOptions.SwooperOn, false));

                    if (CustomGameOptions.JanitorOn > 0)
                        ImpostorRoles.Add((typeof(Janitor), CustomGameOptions.JanitorOn, false));

                    if (CustomGameOptions.WitchOn > 0)
                        ImpostorRoles.Add((typeof(Witch), CustomGameOptions.WitchOn, false));

                    if (CustomGameOptions.GrenadierOn > 0)
                        ImpostorRoles.Add((typeof(Grenadier), CustomGameOptions.GrenadierOn, true));

                    if (CustomGameOptions.EscapistOn > 0)
                        ImpostorRoles.Add((typeof(Escapist), CustomGameOptions.EscapistOn, false));

                    if (CustomGameOptions.BomberOn > 0)
                        ImpostorRoles.Add((typeof(Bomber), CustomGameOptions.BomberOn, true));

                    if (CustomGameOptions.ConjurerOn > 0)
                        ImpostorRoles.Add((typeof(Conjurer), CustomGameOptions.ConjurerOn, true));

                    if (CustomGameOptions.BountyHunterOn > 0)
                        ImpostorRoles.Add((typeof(BountyHunter), CustomGameOptions.BountyHunterOn, true));

                    if (CustomGameOptions.WarlockOn > 0)
                        ImpostorRoles.Add((typeof(Warlock), CustomGameOptions.WarlockOn, false));

                    if (CustomGameOptions.MafiosoOn > 0)
                        ImpostorRoles.Add((typeof(Mafioso), CustomGameOptions.MafiosoOn, false));

                    if (CustomGameOptions.ReviverOn > 0)
                        ImpostorRoles.Add((typeof(Reviver), CustomGameOptions.ReviverOn, false));

                    if (CustomGameOptions.ManipulatorOn > 0)
                        ImpostorRoles.Add((typeof(Manipulator), CustomGameOptions.ManipulatorOn, false));

                    if (CustomGameOptions.VenererOn > 0)
                        ImpostorRoles.Add((typeof(Venerer), CustomGameOptions.VenererOn, true));
                    #endregion
                    #region Coven Roles
                    if (CustomGameOptions.CovenOn > 0)
                        CovenRoles.Add((typeof(Coven), CustomGameOptions.CovenOn, true));
                    if (CustomGameOptions.RitualistOn > 0)
                        CovenRoles.Add((typeof(Ritualist), CustomGameOptions.RitualistOn, true));
                    if (CustomGameOptions.HexMasterOn > 0)
                        CovenRoles.Add((typeof(HexMaster), CustomGameOptions.HexMasterOn, true));
                    if (CustomGameOptions.CovenLeaderOn > 0)
                        CovenRoles.Add((typeof(CovenLeader), CustomGameOptions.CovenLeaderOn, true));
                    if (CustomGameOptions.SpiritualistOn > 0)
                        CovenRoles.Add((typeof(Spiritualist), CustomGameOptions.SpiritualistOn, true));
                    if (CustomGameOptions.VoodooMasterOn > 0)
                        CovenRoles.Add((typeof(VoodooMaster), CustomGameOptions.VoodooMasterOn, true));
                    if (CustomGameOptions.PotionMasterOn > 0)
                        CovenRoles.Add((typeof(PotionMaster), CustomGameOptions.PotionMasterOn, true));
                    #endregion
                    #region Crewmate Modifiers
                    if (Check(CustomGameOptions.TorchOn) && GameOptionsManager.Instance.currentNormalGameOptions.MapId != 5)
                        CrewmateModifiers.Add((typeof(Torch), CustomGameOptions.TorchOn));

                    if (Check(CustomGameOptions.VengefulOn))
                        CrewmateModifiers.Add((typeof(Vengeful), CustomGameOptions.VengefulOn));

                    if (Check(CustomGameOptions.MadmateOn))
                        CrewmateModifiers.Add((typeof(Madmate), CustomGameOptions.MadmateOn));

                    if (Check(CustomGameOptions.DiseasedOn))
                        CrewmateModifiers.Add((typeof(Diseased), CustomGameOptions.DiseasedOn));

                    if (Check(CustomGameOptions.BaitOn))
                        CrewmateModifiers.Add((typeof(Bait), CustomGameOptions.BaitOn));

                    if (Check(CustomGameOptions.AftermathOn))
                        CrewmateModifiers.Add((typeof(Aftermath), CustomGameOptions.AftermathOn));

                    if (Check(CustomGameOptions.MultitaskerOn))
                        CrewmateModifiers.Add((typeof(Multitasker), CustomGameOptions.MultitaskerOn));

                    if (Check(CustomGameOptions.FrostyOn))
                        CrewmateModifiers.Add((typeof(Frosty), CustomGameOptions.FrostyOn));

                    if (Check(CustomGameOptions.TaskmasterOn) && !SubmergedCompatibility.isSubmerged())
                        CrewmateModifiers.Add((typeof(Taskmaster), CustomGameOptions.TaskmasterOn));
                    #endregion
                    #region Global Modifiers
                    if (Check(CustomGameOptions.TiebreakerOn))
                        GlobalModifiers.Add((typeof(Tiebreaker), CustomGameOptions.TiebreakerOn));

                    if (Check(CustomGameOptions.FlashOn))
                        GlobalModifiers.Add((typeof(Flash), CustomGameOptions.FlashOn));

                    if (Check(CustomGameOptions.GiantOn))
                        GlobalModifiers.Add((typeof(Giant), CustomGameOptions.GiantOn));

                    if (Check(CustomGameOptions.MiniOn))
                        GlobalModifiers.Add((typeof(Mini), CustomGameOptions.MiniOn));

                    if (Check(CustomGameOptions.SatelliteOn))
                        GlobalModifiers.Add((typeof(Satellite), CustomGameOptions.SatelliteOn));

                    if (Check(CustomGameOptions.ShyOn))
                        GlobalModifiers.Add((typeof(Shy), CustomGameOptions.ShyOn));

                    if (Check(CustomGameOptions.SixthSenseOn))
                        GlobalModifiers.Add((typeof(SixthSense), CustomGameOptions.SixthSenseOn));

                    if (Check(CustomGameOptions.SpotterOn) && GameOptionsManager.Instance.currentNormalGameOptions.AnonymousVotes == true)
                        GlobalModifiers.Add((typeof(Spotter), CustomGameOptions.SpotterOn));

                    if (Check(CustomGameOptions.MotionlessOn))
                        GlobalModifiers.Add((typeof(Motionless), CustomGameOptions.MotionlessOn));

                    if (Check(CustomGameOptions.ButtonBarryOn) && GameOptionsManager.Instance.currentNormalGameOptions.NumEmergencyMeetings > 0)
                        GlobalModifiers.Add((typeof(ButtonBarry), CustomGameOptions.ButtonBarryOn));

                    if (Check(CustomGameOptions.LoversOn) && CustomGameOptions.GameMode != GameMode.Cultist)
                        GlobalModifiers.Add((typeof(Lover), CustomGameOptions.LoversOn));

                    if (Check(CustomGameOptions.SleuthOn))
                        GlobalModifiers.Add((typeof(Sleuth), CustomGameOptions.SleuthOn));

                    if (Check(CustomGameOptions.ScientistOn))
                        GlobalModifiers.Add((typeof(Scientist), CustomGameOptions.ScientistOn));

                    if (Check(CustomGameOptions.RadarOn))
                        GlobalModifiers.Add((typeof(Radar), CustomGameOptions.RadarOn));

                    if (Check(CustomGameOptions.SuperstarOn))
                        GlobalModifiers.Add((typeof(Superstar), CustomGameOptions.SuperstarOn));
                    #endregion
                    #region Impostor Modifiers
                    if (Check(CustomGameOptions.DisperserOn))
                        ImpostorModifiers.Add((typeof(Disperser), CustomGameOptions.DisperserOn));

                    if (Check(CustomGameOptions.BloodlustOn))
                        ImpostorModifiers.Add((typeof(Bloodlust), CustomGameOptions.BloodlustOn));

                    if (Check(CustomGameOptions.SaboteurOn))
                        ImpostorModifiers.Add((typeof(Saboteur), CustomGameOptions.SaboteurOn));

                    if (Check(CustomGameOptions.DoubleShotOn))
                        AssassinModifiers.Add((typeof(DoubleShot), CustomGameOptions.DoubleShotOn));

                    if (Check(CustomGameOptions.UnderdogOn))
                        ImpostorModifiers.Add((typeof(Underdog), CustomGameOptions.UnderdogOn));

                    if (Check(CustomGameOptions.LuckyOn))
                        ImpostorModifiers.Add((typeof(Lucky), CustomGameOptions.LuckyOn));
                    #endregion
                    #region Assassin Ability
                    AssassinAbility.Add((typeof(Assassin), CustomRPC.SetAssassin, 100));
                    #endregion

                    if (CustomGameOptions.GameMode == GameMode.RoleList)
                    {
                        // Crewmates
                        if (CustomGameOptions.CrewmateOn > 0)
                            CrewmateSupportRoles.Add((typeof(Crewmate), CustomGameOptions.CrewmateOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.PoliticianOn > 0)
                            CrewmatePowerRoles.Add((typeof(Politician), CustomGameOptions.PoliticianOn, true));

                        if (CustomGameOptions.WardenOn > 0)
                            CrewmateSupportRoles.Add((typeof(Warden), CustomGameOptions.WardenOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.SheriffOn > 0)
                            CrewmateKillingRoles.Add((typeof(Sheriff), CustomGameOptions.SheriffOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.FighterOn > 0)
                            CrewmateKillingRoles.Add((typeof(Fighter), CustomGameOptions.FighterOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.JailorOn > 0)
                            CrewmatePowerRoles.Add((typeof(Jailor), CustomGameOptions.JailorOn, true));

                        if (CustomGameOptions.DeputyOn > 0)
                            CrewmateKillingRoles.Add((typeof(Deputy), CustomGameOptions.DeputyOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.PlumberOn > 0)
                            CrewmateSupportRoles.Add((typeof(Plumber), CustomGameOptions.PlumberOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.ClericOn > 0)
                            CrewmateProtectiveRoles.Add((typeof(Cleric), CustomGameOptions.ClericOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.KnightOn > 0)
                            CrewmateKillingRoles.Add((typeof(Knight), CustomGameOptions.KnightOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.EngineerOn > 0)
                            CrewmateSupportRoles.Add((typeof(Engineer), CustomGameOptions.EngineerOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.InformantOn > 0)
                            CrewmateInvestigativeRoles.Add((typeof(Informant), CustomGameOptions.InformantOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.SwapperOn > 0)
                            CrewmatePowerRoles.Add((typeof(Swapper), CustomGameOptions.SwapperOn, true));

                        if (CustomGameOptions.AvengerOn > 0)
                            CrewmateKillingRoles.Add((typeof(Avenger), CustomGameOptions.AvengerOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.InvestigatorOn > 0)
                            CrewmateInvestigativeRoles.Add((typeof(Investigator), CustomGameOptions.InvestigatorOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.MedicOn > 0)
                            CrewmateProtectiveRoles.Add((typeof(Medic), CustomGameOptions.MedicOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.AstralOn > 0)
                            CrewmateInvestigativeRoles.Add((typeof(Astral), CustomGameOptions.AstralOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.LookoutOn > 0)
                            CrewmateInvestigativeRoles.Add((typeof(Lookout), CustomGameOptions.LookoutOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.SeerOn > 0)
                            CrewmateInvestigativeRoles.Add((typeof(Seer), CustomGameOptions.SeerOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.SpyOn > 0 && GameOptionsManager.Instance.currentNormalGameOptions.MapId != 5)
                            CrewmateInvestigativeRoles.Add((typeof(Spy), CustomGameOptions.SpyOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.SnitchOn > 0)
                            CrewmateInvestigativeRoles.Add((typeof(Snitch), CustomGameOptions.SnitchOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.AltruistOn > 0)
                            CrewmateProtectiveRoles.Add((typeof(Altruist), CustomGameOptions.AltruistOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.VigilanteOn > 0)
                            CrewmateKillingRoles.Add((typeof(Vigilante), CustomGameOptions.VigilanteOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.VeteranOn > 0)
                            CrewmateKillingRoles.Add((typeof(Veteran), CustomGameOptions.VeteranOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.HunterOn > 0)
                            CrewmateKillingRoles.Add((typeof(Hunter), CustomGameOptions.HunterOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.TrackerOn > 0)
                            CrewmateInvestigativeRoles.Add((typeof(Tracker), CustomGameOptions.TrackerOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.TransporterOn > 0)
                            CrewmateSupportRoles.Add((typeof(Transporter), CustomGameOptions.TransporterOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.TimeLordOn > 0)
                            CrewmatePowerRoles.Add((typeof(TimeLord), CustomGameOptions.TimeLordOn, true));

                        if (CustomGameOptions.MediumOn > 0)
                            CrewmateInvestigativeRoles.Add((typeof(Medium), CustomGameOptions.MediumOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.MysticOn > 0)
                            CrewmateInvestigativeRoles.Add((typeof(Mystic), CustomGameOptions.MysticOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.TrapperOn > 0)
                            CrewmateInvestigativeRoles.Add((typeof(Trapper), CustomGameOptions.TrapperOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.CaptainOn > 0)
                            CrewmateInvestigativeRoles.Add((typeof(Captain), CustomGameOptions.CaptainOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.ParanoïacOn > 0)
                            CrewmateSupportRoles.Add((typeof(Paranoïac), CustomGameOptions.ParanoïacOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.ChameleonOn > 0)
                            CrewmateInvestigativeRoles.Add((typeof(Chameleon), CustomGameOptions.ChameleonOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.DetectiveOn > 0)
                            CrewmateInvestigativeRoles.Add((typeof(Detective), CustomGameOptions.DetectiveOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.DoctorOn > 0)
                            CrewmateProtectiveRoles.Add((typeof(Doctor), CustomGameOptions.DoctorOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.BodyguardOn > 0)
                            CrewmateProtectiveRoles.Add((typeof(Bodyguard), CustomGameOptions.BodyguardOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.CrusaderOn > 0)
                            CrewmateProtectiveRoles.Add((typeof(Crusader), CustomGameOptions.CrusaderOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.ImitatorOn > 0)
                            CrewmateSupportRoles.Add((typeof(Imitator), CustomGameOptions.ImitatorOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.ProsecutorOn > 0)
                            CrewmatePowerRoles.Add((typeof(Prosecutor), CustomGameOptions.ProsecutorOn, true));

                        if (CustomGameOptions.OracleOn > 0)
                            CrewmateProtectiveRoles.Add((typeof(Oracle), CustomGameOptions.OracleOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.AurialOn > 0)
                            CrewmateInvestigativeRoles.Add((typeof(Aurial), CustomGameOptions.AurialOn, false || CustomGameOptions.UniqueRoles));

                        // Impostors
                        if (CustomGameOptions.ImpostorOn > 0)
                            ImpostorSupportRoles.Add((typeof(Impostor), CustomGameOptions.ImpostorOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.PoisonerOn > 0)
                            ImpostorKillingRoles.Add((typeof(Poisoner), CustomGameOptions.PoisonerOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.HypnotistOn > 0)
                            ImpostorSupportRoles.Add((typeof(Hypnotist), CustomGameOptions.HypnotistOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.ShooterOn > 0)
                            ImpostorKillingRoles.Add((typeof(Shooter), CustomGameOptions.ShooterOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.UndertakerOn > 0)
                            ImpostorSupportRoles.Add((typeof(Undertaker), CustomGameOptions.UndertakerOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.MorphlingOn > 0)
                            ImpostorConcealingRoles.Add((typeof(Morphling), CustomGameOptions.MorphlingOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.AssassinOn > 0 && CustomGameOptions.AssassinImpostorRole)
                            ImpostorKillingRoles.Add((typeof(Roles.Assassin), CustomGameOptions.AssassinOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.BlackmailerOn > 0)
                            ImpostorSupportRoles.Add((typeof(Blackmailer), CustomGameOptions.BlackmailerOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.ConverterOn > 0)
                            ImpostorSupportRoles.Add((typeof(Converter), CustomGameOptions.ConverterOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.MinerOn > 0)
                            ImpostorSupportRoles.Add((typeof(Miner), CustomGameOptions.MinerOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.SwooperOn > 0)
                            ImpostorConcealingRoles.Add((typeof(Swooper), CustomGameOptions.SwooperOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.JanitorOn > 0)
                            ImpostorSupportRoles.Add((typeof(Janitor), CustomGameOptions.JanitorOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.WitchOn > 0)
                            ImpostorKillingRoles.Add((typeof(Witch), CustomGameOptions.WitchOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.GrenadierOn > 0)
                            ImpostorConcealingRoles.Add((typeof(Grenadier), CustomGameOptions.GrenadierOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.EscapistOn > 0)
                            ImpostorConcealingRoles.Add((typeof(Escapist), CustomGameOptions.EscapistOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.BomberOn > 0)
                            ImpostorKillingRoles.Add((typeof(Bomber), CustomGameOptions.BomberOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.ConjurerOn > 0)
                            ImpostorKillingRoles.Add((typeof(Conjurer), CustomGameOptions.ConjurerOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.BountyHunterOn > 0)
                            ImpostorKillingRoles.Add((typeof(BountyHunter), CustomGameOptions.BountyHunterOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.WarlockOn > 0)
                            ImpostorKillingRoles.Add((typeof(Warlock), CustomGameOptions.WarlockOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.MafiosoOn > 0)
                            ImpostorSupportRoles.Add((typeof(Mafioso), CustomGameOptions.MafiosoOn, true));

                        if (CustomGameOptions.ReviverOn > 0)
                            ImpostorSupportRoles.Add((typeof(Reviver), CustomGameOptions.ReviverOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.ManipulatorOn > 0)
                            ImpostorKillingRoles.Add((typeof(Manipulator), CustomGameOptions.ManipulatorOn, false || CustomGameOptions.UniqueRoles));

                        if (CustomGameOptions.VenererOn > 0)
                            ImpostorConcealingRoles.Add((typeof(Venerer), CustomGameOptions.VenererOn, false || CustomGameOptions.UniqueRoles));

                        // Coven
                        if (CustomGameOptions.CovenOn > 0)
                            CovenSupportRoles.Add((typeof(Coven), CustomGameOptions.CovenOn, false || CustomGameOptions.UniqueRoles));
                        if (CustomGameOptions.RitualistOn > 0)
                            CovenKillingRoles.Add((typeof(Ritualist), CustomGameOptions.RitualistOn, false || CustomGameOptions.UniqueRoles));
                        if (CustomGameOptions.HexMasterOn > 0)
                            CovenKillingRoles.Add((typeof(HexMaster), CustomGameOptions.HexMasterOn, false || CustomGameOptions.UniqueRoles));
                        if (CustomGameOptions.CovenLeaderOn > 0)
                            CovenSupportRoles.Add((typeof(CovenLeader), CustomGameOptions.CovenLeaderOn, true));
                        if (CustomGameOptions.SpiritualistOn > 0)
                            CovenSupportRoles.Add((typeof(Spiritualist), CustomGameOptions.SpiritualistOn, false || CustomGameOptions.UniqueRoles));
                        if (CustomGameOptions.VoodooMasterOn > 0)
                            CovenSupportRoles.Add((typeof(VoodooMaster), CustomGameOptions.VoodooMasterOn, false || CustomGameOptions.UniqueRoles));
                        if (CustomGameOptions.PotionMasterOn > 0)
                            CovenSupportRoles.Add((typeof(PotionMaster), CustomGameOptions.PotionMasterOn, false || CustomGameOptions.UniqueRoles));
                    }
                }

                if (CustomGameOptions.GameMode == GameMode.Werewolf) GenEachRoleWerewolf(infected.ToList(), __result);
                else if (CustomGameOptions.GameMode == GameMode.BattleRoyale) GenEachRoleBattleRoyale(infected.ToList());
                else if (CustomGameOptions.GameMode == GameMode.Chaos) GenEachRoleChaos(infected.ToList());
                else GenEachRole(infected.ToList(), __result);

                return false;
            }
        }
    }
}
