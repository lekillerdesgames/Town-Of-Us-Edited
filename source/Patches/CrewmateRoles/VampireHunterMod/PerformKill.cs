using System;
using HarmonyLib;
using TownOfUsEdited.Roles;
using UnityEngine;
using AmongUs.GameOptions;
using TownOfUsEdited.CrewmateRoles.SnitchMod;
using TownOfUsEdited.CrewmateRoles.ImitatorMod;
using TownOfUsEdited.CrewmateRoles.TrapperMod;
using Reactor.Utilities;
using TownOfUsEdited.Roles.Modifiers;
using TownOfUsEdited.Patches;

namespace TownOfUsEdited.CrewmateRoles.VampireHunterMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKill
    {
        public static bool Prefix(KillButton __instance)
        {
            if (__instance != DestroyableSingleton<HudManager>.Instance.KillButton) return true;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.VampireHunter)) return true;
            var role = Role.GetRole<VampireHunter>(PlayerControl.LocalPlayer);
            if (!PlayerControl.LocalPlayer.CanMove || role.ClosestPlayer == null) return false;
            if (role.Cooldown > 0) return false;
            if (!__instance.enabled) return false;
            var maxDistance = LegacyGameOptions.KillDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance];
            if (Vector2.Distance(role.ClosestPlayer.GetTruePosition(),
                PlayerControl.LocalPlayer.GetTruePosition()) > maxDistance) return false;
            if (role.ClosestPlayer == null) return false;
            if (!role.ButtonUsable) return false;

            if (!role.ClosestPlayer.Is(RoleEnum.Vampire))
            {
                var interact = Utils.Interact(PlayerControl.LocalPlayer, role.ClosestPlayer);
                if (interact[0] == true)
                {
                    role.Cooldown = CustomGameOptions.StakeCd;
                    role.UsesLeft--;
                    if (role.UsesLeft == 0 && role.CorrectKills == 0 && CustomGameOptions.SelfKillAfterFinalStake)
                    {
                        Utils.RpcMurderPlayer(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer);
                        role.DeathReason = DeathReasons.Suicided;
                        Utils.Rpc(CustomRPC.SetDeathReason, role.Player, (byte)DeathReasons.Suicided);
                    }
                    return false;
                }
                else if (interact[1] == true)
                {
                    role.Cooldown = CustomGameOptions.TempSaveCdReset;
                    return false;
                }
                else if (interact[3] == true) return false;
                return false;
            }
            else
            {
                var VampRole = Role.GetRole<Vampire>(role.ClosestPlayer);
                if (VampRole.SpawnedAs == true)
                {
                    role.Cooldown = CustomGameOptions.StakeCd;
                    Utils.Interact(PlayerControl.LocalPlayer, role.ClosestPlayer, true);
                    role.UsesLeft--;
                    return false;
                }
                var interact = Utils.Interact(PlayerControl.LocalPlayer, role.ClosestPlayer);
                if (interact[4] == true)
                {
                    Cure(role.ClosestPlayer);
                    Utils.Rpc(CustomRPC.Cure, role.ClosestPlayer.PlayerId);
                    role.Cooldown = CustomGameOptions.StakeCd;
                    Coroutines.Start(Utils.FlashCoroutine(Color.green));
                    return false;
                }
                else if (interact[1] == true)
                {
                    role.Cooldown = CustomGameOptions.TempSaveCdReset;
                    return false;
                }
                else if (interact[3] == true) return false;
                return false;
            }
        }
        public static void Cure(PlayerControl oldVamp)
        {
            var oldRole = Role.GetRole(oldVamp);
            var killsList = (oldRole.Kills, oldRole.CorrectKills, oldRole.IncorrectKills, oldRole.CorrectAssassinKills, oldRole.IncorrectAssassinKills);
            var VampRole = Role.GetRole<Vampire>(oldVamp);

            var roleEnum = VampRole.OldRole.RoleType;
            var role = VampRole.OldRole;

            Role.RoleDictionary.Remove(oldVamp.PlayerId);
            Role.RoleDictionary.Add(oldVamp.PlayerId, role);

            role.RegenTask();
            role.AddToRoleHistory(role.RoleType);

            if (roleEnum == RoleEnum.Snitch)
            {
                var snitchRole = Role.GetRole<Snitch>(oldVamp);
                snitchRole.ImpArrows.DestroyAll();
                snitchRole.SnitchArrows.Values.DestroyAll();
                snitchRole.SnitchArrows.Clear();
                CompleteTask.Postfix(oldVamp);
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (roleEnum == RoleEnum.Sheriff)
            {
                var sheriffRole = Role.GetRole<Sheriff>(oldVamp);
                sheriffRole.Cooldown = CustomGameOptions.SheriffKillCd;
            }

            else if (roleEnum == RoleEnum.Shifter)
            {
                var shifterRole = Role.GetRole<Shifter>(oldVamp);
                shifterRole.Cooldown = CustomGameOptions.ShiftCD;
            }

            else if (roleEnum == RoleEnum.Knight)
            {
                var knightRole = Role.GetRole<Knight>(oldVamp);
                knightRole.Cooldown = CustomGameOptions.KnightKCD;
                knightRole.UsesLeft = 1;
            }

            else if (roleEnum == RoleEnum.Fighter)
            {
                var fighterRole = Role.GetRole<Fighter>(oldVamp);
                fighterRole.Cooldown = CustomGameOptions.FighterKCD;
            }

            else if (roleEnum == RoleEnum.Engineer)
            {
                var engiRole = Role.GetRole<Engineer>(oldVamp);
                engiRole.UsesLeft = CustomGameOptions.MaxFixes;
            }

            else if (roleEnum == RoleEnum.Medic)
            {
                var medicRole = Role.GetRole<Medic>(oldVamp);
                medicRole.ShieldedPlayer = null;
            }

            else if (roleEnum == RoleEnum.Mayor)
            {
                var mayorRole = Role.GetRole<Mayor>(oldVamp);
                mayorRole.Revealed = false;
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (roleEnum == RoleEnum.Prosecutor)
            {
                var prosRole = Role.GetRole<Prosecutor>(oldVamp);
                prosRole.Prosecuted = false;
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (roleEnum == RoleEnum.Politician)
            {
                var pnRole = Role.GetRole<Politician>(oldVamp);
                pnRole.CampaignedPlayers.RemoveRange(0, pnRole.CampaignedPlayers.Count);
                pnRole.Cooldown = CustomGameOptions.CampaignCd;
            }

            else if (roleEnum == RoleEnum.SoulCollector)
            {
                var scRole = Role.GetRole<SoulCollector>(oldVamp);
                scRole.Cooldown = CustomGameOptions.ReapCd;
            }

            else if (roleEnum == RoleEnum.Cleric)
            {
                var clericRole = Role.GetRole<Cleric>(oldVamp);
                clericRole.Cooldown = CustomGameOptions.BarrierCD;
            }

            else if (roleEnum == RoleEnum.Plumber)
            {
                var plumberRole = Role.GetRole<Plumber>(oldVamp);
                plumberRole.UsesLeft = CustomGameOptions.MaxBarricades;
                plumberRole.FutureBlocks.Clear();
                plumberRole.Cooldown = CustomGameOptions.FlushCd;
            }

            else if (roleEnum == RoleEnum.Vigilante)
            {
                var vigiRole = Role.GetRole<Vigilante>(oldVamp);
                vigiRole.RemainingKills = CustomGameOptions.VigilanteKills;
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (roleEnum == RoleEnum.Veteran)
            {
                var vetRole = Role.GetRole<Veteran>(oldVamp);
                vetRole.UsesLeft = CustomGameOptions.MaxAlerts;
                vetRole.Cooldown = CustomGameOptions.AlertCd;
            }

            else if (roleEnum == RoleEnum.Astral)
            {
                var astralRole = Role.GetRole<Astral>(oldVamp);
                astralRole.Cooldown = CustomGameOptions.GhostCD;
            }

            else if (roleEnum == RoleEnum.Lookout)
            {
                var lookoutRole = Role.GetRole<Lookout>(oldVamp);
                lookoutRole.Cooldown = CustomGameOptions.WatchCD;
            }

            else if (roleEnum == RoleEnum.Hunter)
            {
                var hunterRole = Role.GetRole<Hunter>(oldVamp);
                hunterRole.UsesLeft = CustomGameOptions.HunterStalkUses;
                hunterRole.StalkCooldown = CustomGameOptions.HunterStalkCd;
                hunterRole.Cooldown = CustomGameOptions.HunterKillCd;
            }

            else if (roleEnum == RoleEnum.Doctor)
            {
                var docRole = Role.GetRole<Doctor>(oldVamp);
                docRole.Cooldown = CustomGameOptions.DocReviveCooldown;
            }

            else if (roleEnum == RoleEnum.TimeLord)
            {
                var tlRole = Role.GetRole<TimeLord>(oldVamp);
                tlRole.Cooldown = CustomGameOptions.RewindCooldown;
            }

            else if (roleEnum == RoleEnum.Crusader)
            {
                var crusRole = Role.GetRole<Crusader>(oldVamp);
                crusRole.Cooldown = CustomGameOptions.CrusadeCD;
            }

            else if (roleEnum == RoleEnum.Tracker)
            {
                var trackerRole = Role.GetRole<Tracker>(oldVamp);
                trackerRole.TrackerArrows.Values.DestroyAll();
                trackerRole.TrackerArrows.Clear();
                trackerRole.UsesLeft = CustomGameOptions.MaxTracks;
                trackerRole.Cooldown = CustomGameOptions.TrackCd;
            }

            else if (roleEnum == RoleEnum.VampireHunter)
            {
                var vhRole = Role.GetRole<VampireHunter>(oldVamp);
                if (vhRole.AddedStakes) vhRole.UsesLeft = CustomGameOptions.MaxFailedStakesPerGame;
                else vhRole.UsesLeft = 0;
                vhRole.Cooldown = CustomGameOptions.StakeCd;
            }

            else if (roleEnum == RoleEnum.Captain)
            {
                var capRole = Role.GetRole<Captain>(oldVamp);
                capRole.Cooldown = CustomGameOptions.ZoomCooldown;
            }

            else if (roleEnum == RoleEnum.Jailor)
            {
                var jailRole = Role.GetRole<Jailor>(oldVamp);
                jailRole.Cooldown = CustomGameOptions.JailCD;
                jailRole.JailedPlayer = null;
            }

            else if (roleEnum == RoleEnum.Chameleon)
            {
                var chamRole = Role.GetRole<Chameleon>(oldVamp);
                chamRole.Enabled = false;
                chamRole.Cooldown = CustomGameOptions.ChamSwoopCooldown;
            }

            else if (roleEnum == RoleEnum.Detective)
            {
                var detectiveRole = Role.GetRole<Detective>(oldVamp);
                detectiveRole.Cooldown = CustomGameOptions.ExamineCd;
                detectiveRole.CurrentTarget = null;
            }

            else if (roleEnum == RoleEnum.Mystic)
            {
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (roleEnum == RoleEnum.Transporter)
            {
                var tpRole = Role.GetRole<Transporter>(oldVamp);
                tpRole.TransportPlayer1 = null;
                tpRole.TransportPlayer2 = null;
                tpRole.Cooldown = CustomGameOptions.TransportCooldown;
                tpRole.UsesLeft = CustomGameOptions.TransportMaxUses;
            }

            else if (roleEnum == RoleEnum.Medium)
            {
                var medRole = Role.GetRole<Medium>(oldVamp);
                medRole.MediatedPlayers.Values.DestroyAll();
                medRole.MediatedPlayers.Clear();
                medRole.Cooldown = CustomGameOptions.MediateCooldown;
            }

            else if (roleEnum == RoleEnum.Seer)
            {
                var seerRole = Role.GetRole<Seer>(oldVamp);
                seerRole.Investigated.RemoveRange(0, seerRole.Investigated.Count);
                seerRole.Cooldown = CustomGameOptions.SeerCd;
            }

            else if (roleEnum == RoleEnum.Oracle)
            {
                var oracleRole = Role.GetRole<Oracle>(oldVamp);
                oracleRole.Confessor = null;
                oracleRole.Cooldown = CustomGameOptions.ConfessCd;
            }

            else if (oldVamp.Is(RoleEnum.Aurial))
            {
                var aurialRole = Role.GetRole<Aurial>(oldVamp);
                aurialRole.SenseArrows.Values.DestroyAll();
                aurialRole.SenseArrows.Clear();
                if (PlayerControl.LocalPlayer == aurialRole.Player) DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (roleEnum == RoleEnum.Survivor)
            {
                var survRole = Role.GetRole<Survivor>(oldVamp);
                survRole.LastVested = DateTime.UtcNow;
                survRole.UsesLeft = CustomGameOptions.MaxVests;
            }

            else if (roleEnum == RoleEnum.Mercenary)
            {
                var mercRole = Role.GetRole<Mercenary>(oldVamp);
                mercRole.Cooldown = CustomGameOptions.MercenaryCD;
                mercRole.Guarded.Clear();
                mercRole.Bribed.Clear();
                mercRole.Alert = false;
            }

            else if (roleEnum == RoleEnum.GuardianAngel)
            {
                var gaRole = Role.GetRole<GuardianAngel>(oldVamp);
                gaRole.Cooldown = CustomGameOptions.ProtectCd;
                gaRole.UsesLeft = CustomGameOptions.MaxProtects;
            }

            else if (roleEnum == RoleEnum.Doomsayer)
            {
                var doomRole = Role.GetRole<Doomsayer>(oldVamp);
                doomRole.GuessedCorrectly = 0;
                doomRole.Cooldown = CustomGameOptions.ObserveCooldown;
                doomRole.LastObservedPlayer = null;
            }

            else if (roleEnum == RoleEnum.Trapper)
            {
                var trapperRole = Role.GetRole<Trapper>(oldVamp);
                trapperRole.Cooldown = CustomGameOptions.TrapCooldown;
                trapperRole.UsesLeft = CustomGameOptions.MaxTraps;
                trapperRole.trappedPlayers.Clear();
                trapperRole.traps.ClearTraps();
            }

            else if (!(oldVamp.Is(RoleEnum.Altruist) || oldVamp.Is(RoleEnum.Amnesiac)))
            {
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            var otherRole = Role.GetRole(oldVamp);
            role.Kills = otherRole.Kills;
            role.CorrectKills = otherRole.CorrectKills;
            role.IncorrectKills = otherRole.IncorrectKills;
            role.CorrectAssassinKills = otherRole.CorrectAssassinKills;
            role.IncorrectAssassinKills = otherRole.IncorrectAssassinKills;
            otherRole.Kills = killsList.Kills;
            otherRole.CorrectKills = killsList.CorrectKills;
            otherRole.IncorrectKills = killsList.IncorrectKills;
            otherRole.CorrectAssassinKills = killsList.CorrectAssassinKills;
            otherRole.IncorrectAssassinKills = killsList.IncorrectAssassinKills;

            if (CustomGameOptions.NewVampCanAssassin) Ability.AbilityDictionary.Remove(oldVamp.PlayerId);

            PlayerControl_Die.CheckEnd();
            return; 
        }
    }
}
