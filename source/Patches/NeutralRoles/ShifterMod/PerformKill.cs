using HarmonyLib;
using TownOfUsEdited.CrewmateRoles.InvestigatorMod;
using TownOfUsEdited.CrewmateRoles.SnitchMod;
using TownOfUsEdited.CrewmateRoles.TrapperMod;
using TownOfUsEdited.Roles;
using UnityEngine;
using System;
using TownOfUsEdited.Extensions;
using TownOfUsEdited.CrewmateRoles.ImitatorMod;
using TownOfUsEdited.Roles.Modifiers;
using TownOfUsEdited.ImpostorRoles.BomberMod;
using Assassin = TownOfUsEdited.Roles.Modifiers.Assassin;
using Reactor.Utilities;
using System.Collections;
using System.Linq;
using TownOfUsEdited.CovenRoles.CovenMod;

namespace TownOfUsEdited.Patches.NeutralRoles.ShifterMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKill
    {
        public static Sprite Sprite => TownOfUsEdited.Arrow;
        public static bool Prefix(KillButton __instance)
        {
            if (__instance != DestroyableSingleton<HudManager>.Instance.KillButton) return true;
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Shifter);
            if (!flag) return true;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            var role = Role.GetRole<Shifter>(PlayerControl.LocalPlayer);

            var flag2 = __instance.isCoolingDown;
            if (flag2) return false;
            if (!__instance.enabled) return false;
            if (role == null)
                return false;
            if (role.ClosestPlayer == null)
                return false;
            var playerId = role.ClosestPlayer.PlayerId;
            var player = Utils.PlayerById(playerId);
            var abilityUsed = Utils.AbilityUsed(PlayerControl.LocalPlayer);
            if (!abilityUsed) return false;
            if ((player.IsInfected() || role.Player.IsInfected()) && !player.Is(RoleEnum.Plaguebearer))
            {
                foreach (var pb in Role.GetRoles(RoleEnum.Plaguebearer)) ((Plaguebearer)pb).RpcSpreadInfection(player, role.Player);
            }

            var interact = Utils.Interact(PlayerControl.LocalPlayer, role.ClosestPlayer);
            if (interact[4] == true)
            {
                Coroutines.Start(Shift(role, player));
                Utils.Rpc(CustomRPC.Shift, PlayerControl.LocalPlayer.PlayerId, playerId);
                role.Cooldown = CustomGameOptions.ShiftCD;
                return false;
            }
            if (interact[0] == true)
            {
                role.Cooldown = CustomGameOptions.ShiftCD;
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

        public static IEnumerator Shift(Shifter shiftRole, PlayerControl other)
        {
            var role = Utils.GetRole(other);
            var shifter = shiftRole.Player;

            shiftRole.Cooldown = CustomGameOptions.ShiftCD;

            yield return new WaitForSeconds(1);

            if (PlayerControl.LocalPlayer == other)
            {
                Coroutines.Start(Utils.FlashCoroutine(Patches.Colors.Shifter, 0.5f));
            }

            var shiftImp = true;
            var shiftNeut = false;
            var shiftCrewmate = false;

            Role newRole;

            switch (role)
            {
                case RoleEnum.Sheriff:
                case RoleEnum.Fighter:
                case RoleEnum.Knight:
                case RoleEnum.Engineer:
                case RoleEnum.Mayor:
                case RoleEnum.Swapper:
                case RoleEnum.Investigator:
                case RoleEnum.Medic:
                case RoleEnum.Seer:
                case RoleEnum.Spy:
                case RoleEnum.Snitch:
                case RoleEnum.Altruist:
                case RoleEnum.Vigilante:
                case RoleEnum.Veteran:
                case RoleEnum.Astral:
                case RoleEnum.Lookout:
                case RoleEnum.Crewmate:
                case RoleEnum.Tracker:
                case RoleEnum.Transporter:
                case RoleEnum.Informant:
                case RoleEnum.Medium:
                case RoleEnum.Mystic:
                case RoleEnum.Trapper:
                case RoleEnum.Detective:
                case RoleEnum.Captain:
                case RoleEnum.Avenger:
                case RoleEnum.Chameleon:
                case RoleEnum.Jailor:
                case RoleEnum.Deputy:
                case RoleEnum.Doctor:
                case RoleEnum.TimeLord:
                case RoleEnum.Crusader:
                case RoleEnum.Bodyguard:
                case RoleEnum.Imitator:
                case RoleEnum.VampireHunter:
                case RoleEnum.Prosecutor:
                case RoleEnum.Oracle:
                case RoleEnum.Aurial:
                case RoleEnum.Hunter:
                case RoleEnum.Paranoïac:
                case RoleEnum.Warden:
                case RoleEnum.Politician:
                case RoleEnum.Plumber:
                case RoleEnum.Cleric:

                    shiftImp = false;
                    shiftNeut = false;
                    shiftCrewmate = true;

                    break;

                case RoleEnum.Jester:
                case RoleEnum.Troll:
                case RoleEnum.Executioner:
                case RoleEnum.Arsonist:
                case RoleEnum.SerialKiller:
                case RoleEnum.Doppelganger:
                case RoleEnum.Mutant:
                case RoleEnum.Infectious:
                case RoleEnum.Amnesiac:
                case RoleEnum.Shifter:
                case RoleEnum.Glitch:
                case RoleEnum.Juggernaut:
                case RoleEnum.Survivor:
                case RoleEnum.GuardianAngel:
                case RoleEnum.Plaguebearer:
                case RoleEnum.Attacker:
                case RoleEnum.Terrorist:
                case RoleEnum.Maul:
                case RoleEnum.Doomsayer:
                case RoleEnum.Vulture:
                case RoleEnum.Vampire:
                case RoleEnum.SoulCollector:
                case RoleEnum.Mercenary:

                    shiftImp = false;
                    shiftNeut = true;
                    shiftCrewmate = false;

                    break;

                case RoleEnum.Coven:
                case RoleEnum.Ritualist:
                case RoleEnum.HexMaster:
                case RoleEnum.CovenLeader:
                case RoleEnum.Spiritualist:
                case RoleEnum.VoodooMaster:
                case RoleEnum.PotionMaster:

                    shiftImp = false;
                    shiftNeut = false;
                    shiftCrewmate = false;

                    break;
            }

            newRole = Role.GetRole(other);
            newRole.Player = shifter;

            if (newRole.ExtraButtons.Any())
            {
                foreach (var button in newRole.ExtraButtons)
                {
                    if (PlayerControl.LocalPlayer == other)
                    {
                        button.gameObject.SetActive(false);
                    }
                }
            }
            if (newRole.ButtonLabels.Any())
            {
                foreach (var label in newRole.ButtonLabels)
                {
                    if (PlayerControl.LocalPlayer == other)
                    {
                        label.gameObject.SetActive(false);
                    }
                }
            }

            if ((role == RoleEnum.Arsonist || role == RoleEnum.Plaguebearer || role == RoleEnum.Pestilence
                 || role == RoleEnum.Grenadier) && PlayerControl.LocalPlayer == other)
            {
                foreach (var visor in PlayerControl.AllPlayerControls)
                {
                    ShowShield.ClearVisor(visor);
                }
            }

            if (role == RoleEnum.Manipulator)
            {
                var manipRole = Role.GetRole<Manipulator>(other);
                manipRole.StopManipulation();
                Utils.Rpc(CustomRPC.SetManipulateOff, other.PlayerId);
            }

            if (role == RoleEnum.Lookout)
            {
                var lookoutRole = Role.GetRole<Lookout>(other);
                lookoutRole.StopWatching();
                Utils.Rpc(CustomRPC.StopWatch, other.PlayerId);
            }

            if (role == RoleEnum.Engineer)
            {
                var engineerRole = Role.GetRole<Engineer>(other);
                UnityEngine.Object.Destroy(engineerRole.UsesText);
            }

            else if (role == RoleEnum.Knight)
            {
                var knightRole = Role.GetRole<Knight>(other);
                UnityEngine.Object.Destroy(knightRole.UsesText);
            }

            else if (role == RoleEnum.Tracker)
            {
                var trackerRole = Role.GetRole<Tracker>(other);
                trackerRole.TrackerArrows.Values.DestroyAll();
                trackerRole.TrackerArrows.Clear();
                UnityEngine.Object.Destroy(trackerRole.UsesText);
            }

            else if (role == RoleEnum.Transporter)
            {
                var transporterRole = Role.GetRole<Transporter>(other);
                UnityEngine.Object.Destroy(transporterRole.UsesText);
            }

            else if (role == RoleEnum.Veteran)
            {
                var veteranRole = Role.GetRole<Veteran>(other);
                UnityEngine.Object.Destroy(veteranRole.UsesText);
            }

            else if (role == RoleEnum.Trapper)
            {
                var trapperRole = Role.GetRole<Trapper>(other);
                UnityEngine.Object.Destroy(trapperRole.UsesText);
                trapperRole.traps.ClearTraps();
            }

            else if (role == RoleEnum.Doctor)
            {
                var docRole = Role.GetRole<Doctor>(other);
                UnityEngine.Object.Destroy(docRole.UsesText);
            }

            else if (role == RoleEnum.Detective)
            {
                var detecRole = Role.GetRole<Detective>(other);
                detecRole.ExamineButton.gameObject.SetActive(false);
            }

            else if (role == RoleEnum.Survivor)
            {
                var survRole = Role.GetRole<Survivor>(other);
                UnityEngine.Object.Destroy(survRole.UsesText);
            }

            else if (role == RoleEnum.GuardianAngel)
            {
                var gaRole = Role.GetRole<GuardianAngel>(other);
                UnityEngine.Object.Destroy(gaRole.UsesText);
            }

            else if (role == RoleEnum.Glitch && PlayerControl.LocalPlayer == other)
            {
                var glitch = Role.GetRole<Glitch>(other);
                glitch.MimicButton.gameObject.SetActive(false);
                glitch.HackButton.gameObject.SetActive(false);
            }

            if (PlayerControl.LocalPlayer == other || PlayerControl.LocalPlayer == shifter)
            {
                HudManager.Instance.KillButton.buttonLabelText.gameObject.SetActive(false);
                HudManager.Instance.KillButton.gameObject.SetActive(false);
                try
                {
                    CovenUpdate.SabotageButton.gameObject.SetActive(false);
                }
                catch {}
            }

            if (role == RoleEnum.Investigator) Footprint.DestroyAll(Role.GetRole<Investigator>(other));

            if (role == RoleEnum.Snitch) CompleteTask.Postfix(shifter);

            Role.RoleDictionary.Remove(shifter.PlayerId);
            Role.RoleDictionary.Remove(other.PlayerId);
            Role.RoleDictionary.Add(shifter.PlayerId, newRole);

            newRole.RegenTask();

            if (StartImitate.ImitatingPlayers.Contains(other.PlayerId))
            {
                StartImitate.ImitatingPlayers.Remove(other.PlayerId);
                StartImitate.ImitatingPlayers.Add(shiftRole.Player.PlayerId);
                newRole.AddToRoleHistory(RoleEnum.Imitator);
            }
            else newRole.AddToRoleHistory(newRole.RoleType);

            if (shiftImp == false)
            {
                if (shiftCrewmate)
                {
                    if (PlayerControl.LocalPlayer == other)
                    {
                    var newshifter = new Shifter(other);
                    var newshifterRole = Role.GetRole<Shifter>(other);
                    newshifter.RegenTask();
                    newshifterRole.Cooldown = CustomGameOptions.ShiftCD;
                    }
                    else
                    {
                    var newshifter = new Shifter(other);
                    var newshifterRole = Role.GetRole<Shifter>(other);
                    newshifter.RegenTask();
                    newshifterRole.Cooldown = CustomGameOptions.ShiftCD;
                    }
                }
                else if (shiftNeut)
                {
                    if (PlayerControl.LocalPlayer == other)
                    {
                    var newshifter = new Shifter(other);
                    var newshifterRole = Role.GetRole<Shifter>(other);
                    newshifter.RegenTask();
                    newshifterRole.Cooldown = CustomGameOptions.ShiftCD;
                    if (role == RoleEnum.Arsonist || role == RoleEnum.Glitch || role == RoleEnum.Plaguebearer ||
                            role == RoleEnum.Pestilence || role == RoleEnum.Maul || role == RoleEnum.Juggernaut
                             || role == RoleEnum.Vampire || role == RoleEnum.SerialKiller || role == RoleEnum.Mutant
                             || role == RoleEnum.Attacker || role == RoleEnum.Terrorist || role == RoleEnum.Infectious
                             || role == RoleEnum.Doppelganger)
                    {
                        if (other.Is(AbilityEnum.Assassin)) Ability.AbilityDictionary.Remove(other.PlayerId);
                        if (CustomGameOptions.ShiftTurnNeutAssassin
                        && !CustomGameOptions.AssassinImpostorRole) new Assassin(shifter);
                    }
                    }
                    else
                    {
                    var newshifter = new Shifter(other);
                    var newshifterRole = Role.GetRole<Shifter>(other);
                    newshifter.RegenTask();
                    newshifterRole.Cooldown = CustomGameOptions.ShiftCD;
                    if (role == RoleEnum.Arsonist || role == RoleEnum.Glitch || role == RoleEnum.Plaguebearer ||
                            role == RoleEnum.Pestilence || role == RoleEnum.Maul || role == RoleEnum.Juggernaut
                             || role == RoleEnum.Vampire || role == RoleEnum.SerialKiller || role == RoleEnum.Mutant
                             || role == RoleEnum.Attacker || role == RoleEnum.Terrorist || role == RoleEnum.Infectious
                             || role == RoleEnum.Doppelganger)
                    {
                        if (other.Is(AbilityEnum.Assassin)) Ability.AbilityDictionary.Remove(other.PlayerId);
                        if (CustomGameOptions.ShiftTurnNeutAssassin
                        && !CustomGameOptions.AssassinImpostorRole) new Assassin(shifter);
                    }
                    }
                }
                else
                {
                    if (PlayerControl.LocalPlayer == other)
                    {
                    var newshifter = new Shifter(other);
                    var newshifterRole = Role.GetRole<Shifter>(other);
                    newshifter.RegenTask();
                    newshifterRole.Cooldown = CustomGameOptions.ShiftCD;
                    }
                    else
                    {
                    var newshifter = new Shifter(other);
                    var newshifterRole = Role.GetRole<Shifter>(other);
                    newshifter.RegenTask();
                    newshifterRole.Cooldown = CustomGameOptions.ShiftCD;
                    }
                }
            }
            else if (shiftImp == true)
            {
                if (PlayerControl.LocalPlayer == other)
                {
                new Shifter(other);
                var newshifterRole = Role.GetRole<Shifter>(other);
                Role.GetRole(shifter).KillCooldown = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                newshifterRole.Cooldown = CustomGameOptions.ShiftCD;
                DestroyableSingleton<HudManager>.Instance.SabotageButton.Hide();
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data.IsImpostor() && PlayerControl.LocalPlayer.Data.IsImpostor())
                    {
                        player.nameText().color = Patches.Colors.Impostor;
                    }
                }
                if (other.Is(ModifierEnum.Disperser) || other.Is(ModifierEnum.DoubleShot) || other.Is(ModifierEnum.Underdog) || other.Is(ModifierEnum.Lucky) || other.Is(ModifierEnum.Bloodlust))
                {
                    Modifier.ModifierDictionary.Remove(other.PlayerId);
                }
                if (other.Is(AbilityEnum.Assassin))
                {
                    Ability.AbilityDictionary.Remove(other.PlayerId);
                }
                if (CustomGameOptions.ShiftTurnImpAssassin
                && !CustomGameOptions.AssassinImpostorRole) new Assassin(shifter);
                }
                else
                {
                new Shifter(other);
                var newshifterRole = Role.GetRole<Shifter>(other);
                Role.GetRole(shifter).KillCooldown = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                newshifterRole.Cooldown = CustomGameOptions.ShiftCD;
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data.IsImpostor() && PlayerControl.LocalPlayer.Data.IsImpostor())
                    {
                        player.nameText().color = Patches.Colors.Impostor;
                    }
                }
                if (other.Is(ModifierEnum.Disperser) || other.Is(ModifierEnum.DoubleShot) || other.Is(ModifierEnum.Underdog) || other.Is(ModifierEnum.Lucky) || other.Is(ModifierEnum.Bloodlust))
                {
                    Modifier.ModifierDictionary.Remove(other.PlayerId);
                }
                if (other.Is(AbilityEnum.Assassin))
                {
                    Ability.AbilityDictionary.Remove(other.PlayerId);
                }
                if (CustomGameOptions.ShiftTurnImpAssassin
                && !CustomGameOptions.AssassinImpostorRole) new Assassin(shifter);
                }
                if (shifter.Is(RoleEnum.Poisoner))
                {
                    if (PlayerControl.LocalPlayer == shifter)
                    {
                        var poisonerRole = Role.GetRole<Poisoner>(shifter);
                        poisonerRole.Cooldown = CustomGameOptions.PoisonCD;
                        poisonerRole.PoisonedPlayer = null;
                        DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
                    }
                }
            }

            if (role == RoleEnum.Snitch)
            {
                var snitchRole = Role.GetRole<Snitch>(shifter);
                snitchRole.ImpArrows.DestroyAll();
                snitchRole.SnitchArrows.Values.DestroyAll();
                snitchRole.SnitchArrows.Clear();
                CompleteTask.Postfix(shifter);
                if (other.AmOwner)
                    foreach (var player in PlayerControl.AllPlayerControls)
                        player.nameText().color = Color.white;
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (role == RoleEnum.Sheriff)
            {
                var sheriffRole = Role.GetRole<Sheriff>(shifter);
                sheriffRole.Cooldown = CustomGameOptions.SheriffKillCd;
            }

            else if (role == RoleEnum.Knight)
            {
                var knightRole = Role.GetRole<Knight>(shifter);
                knightRole.Cooldown = CustomGameOptions.KnightKCD;
                knightRole.UsesLeft = 1;
            }

            else if (role == RoleEnum.Fighter)
            {
                var fighterRole = Role.GetRole<Fighter>(shifter);
                fighterRole.Cooldown = CustomGameOptions.FighterKCD;
            }

            else if (role == RoleEnum.Engineer)
            {
                var engiRole = Role.GetRole<Engineer>(shifter);
                engiRole.UsesLeft = CustomGameOptions.MaxFixes;
            }

            else if (role == RoleEnum.Crusader)
            {
                var crusRole = Role.GetRole<Crusader>(shifter);
                crusRole.Cooldown = CustomGameOptions.CrusadeCD;
            }

            else if (role == RoleEnum.Cleric)
            {
                var clericRole = Role.GetRole<Cleric>(shifter);
                clericRole.Cooldown = CustomGameOptions.BarrierCD;
            }

            else if (role == RoleEnum.Plumber)
            {
                var plumberRole = Role.GetRole<Plumber>(shifter);
                plumberRole.UsesLeft = CustomGameOptions.MaxBarricades;
                plumberRole.FutureBlocks.Clear();
                plumberRole.Cooldown = CustomGameOptions.FlushCd;
            }

            else if (role == RoleEnum.Mercenary)
            {
                var mercRole = Role.GetRole<Mercenary>(shifter);
                mercRole.Cooldown = CustomGameOptions.MercenaryCD;
                mercRole.Guarded.Clear();
                mercRole.Bribed.Clear();
                mercRole.Alert = false;
            }

            else if (role == RoleEnum.Medic)
            {
                var medicRole = Role.GetRole<Medic>(shifter);
                medicRole.ShieldedPlayer = null;
            }

            else if (role == RoleEnum.Mayor)
            {
                var mayorRole = Role.GetRole<Mayor>(shifter);
                mayorRole.Revealed = false;
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (role == RoleEnum.Politician)
            {
                var pnRole = Role.GetRole<Politician>(shifter);
                pnRole.CampaignedPlayers.RemoveRange(0, pnRole.CampaignedPlayers.Count);
                pnRole.Cooldown = CustomGameOptions.CampaignCd;
            }

            else if (role == RoleEnum.Prosecutor)
            {
                var prosRole = Role.GetRole<Prosecutor>(shifter);
                prosRole.Prosecuted = false;
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (role == RoleEnum.Deputy)
            {
                var deputyRole = Role.GetRole<Deputy>(shifter);
                deputyRole.Camping = null;
                deputyRole.Killer = null;
                deputyRole.CampedThisRound = false;
            }

            else if (role == RoleEnum.Vigilante)
            {
                var vigiRole = Role.GetRole<Vigilante>(shifter);
                vigiRole.RemainingKills = CustomGameOptions.VigilanteKills;
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (role == RoleEnum.Veteran)
            {
                var vetRole = Role.GetRole<Veteran>(shifter);
                vetRole.UsesLeft = CustomGameOptions.MaxAlerts;
                vetRole.Cooldown = CustomGameOptions.AlertCd;
            }

            else if (role == RoleEnum.Astral)
            {
                var astralRole = Role.GetRole<Astral>(shifter);
                astralRole.Cooldown = CustomGameOptions.GhostCD;
                astralRole.Enabled = false;
            }

            else if (role == RoleEnum.Lookout)
            {
                var lookoutRole = Role.GetRole<Lookout>(shifter);
                lookoutRole.Cooldown = CustomGameOptions.WatchCD;
            }

            else if (role == RoleEnum.Hunter)
            {
                var hunterRole = Role.GetRole<Hunter>(shifter);
                hunterRole.UsesLeft = CustomGameOptions.HunterStalkUses;
                hunterRole.StalkCooldown = CustomGameOptions.HunterStalkCd;
                hunterRole.Cooldown = CustomGameOptions.HunterKillCd;
            }

            else if (role == RoleEnum.Doctor)
            {
                var docRole = Role.GetRole<Doctor>(shifter);
                docRole.Cooldown = CustomGameOptions.DocReviveCooldown;
            }

            else if (role == RoleEnum.TimeLord)
            {
                var tlRole = Role.GetRole<TimeLord>(shifter);
                tlRole.Cooldown = CustomGameOptions.RewindCooldown;
            }

            else if (role == RoleEnum.Tracker)
            {
                var trackerRole = Role.GetRole<Tracker>(shifter);
                trackerRole.TrackerArrows.Values.DestroyAll();
                trackerRole.TrackerArrows.Clear();
                trackerRole.UsesLeft = CustomGameOptions.MaxTracks;
                trackerRole.Cooldown = CustomGameOptions.TrackCd;
            }

            else if (role == RoleEnum.VampireHunter)
            {
                var vhRole = Role.GetRole<VampireHunter>(shifter);
                if (vhRole.AddedStakes) vhRole.UsesLeft = CustomGameOptions.MaxFailedStakesPerGame;
                else vhRole.UsesLeft = 0;
                vhRole.Cooldown = CustomGameOptions.StakeCd;
            }

            else if (role == RoleEnum.Captain)
            {
                var capRole = Role.GetRole<Captain>(shifter);
                capRole.Cooldown = CustomGameOptions.ZoomCooldown;
            }

            else if (role == RoleEnum.Jailor)
            {
                var jailRole = Role.GetRole<Jailor>(shifter);
                jailRole.Cooldown = CustomGameOptions.JailCD;
                jailRole.JailedPlayer = null;
            }

            else if (role == RoleEnum.Chameleon)
            {
                var chamRole = Role.GetRole<Chameleon>(shifter);
                if (chamRole.IsSwooped)
                {
                chamRole.UnSwoop();
                Utils.Rpc(CustomRPC.ChameleonUnSwoop, shifter.PlayerId);
                }
                chamRole.Enabled = false;
                chamRole.Cooldown = CustomGameOptions.ChamSwoopCooldown;
            }

            else if (role == RoleEnum.Detective)
            {
                var detectiveRole = Role.GetRole<Detective>(shifter);
                detectiveRole.LastExamined = DateTime.UtcNow;
                detectiveRole.CurrentTarget = null;
            }

            else if (role == RoleEnum.Mystic)
            {
                var mysticRole = Role.GetRole<Mystic>(shifter);
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (role == RoleEnum.Transporter)
            {
                var tpRole = Role.GetRole<Transporter>(shifter);
                tpRole.TransportPlayer1 = null;
                tpRole.TransportPlayer2 = null;
                tpRole.Cooldown = CustomGameOptions.TransportCooldown;
                tpRole.UsesLeft = CustomGameOptions.TransportMaxUses;
            }

            else if (role == RoleEnum.Medium)
            {
                var medRole = Role.GetRole<Medium>(shifter);
                medRole.MediatedPlayers.Values.DestroyAll();
                medRole.MediatedPlayers.Clear();
                medRole.Cooldown = CustomGameOptions.MediateCooldown;
            }

            else if (role == RoleEnum.Seer)
            {
                var seerRole = Role.GetRole<Seer>(shifter);
                seerRole.Investigated.RemoveRange(0, seerRole.Investigated.Count);
                seerRole.Cooldown = CustomGameOptions.SeerCd;
            }

            else if (role == RoleEnum.Oracle)
            {
                var oracleRole = Role.GetRole<Oracle>(shifter);
                oracleRole.Blessed = null;
                oracleRole.Cooldown = CustomGameOptions.ConfessCd;
            }

            else if (role == RoleEnum.Aurial)
            {
                var aurialRole = Role.GetRole<Aurial>(shifter);
                aurialRole.SenseArrows.Values.DestroyAll();
                aurialRole.SenseArrows.Clear();
                if (PlayerControl.LocalPlayer == aurialRole.Player) DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (role == RoleEnum.Arsonist)
            {
                var arsoRole = Role.GetRole<Arsonist>(shifter);
                arsoRole.DousedPlayers.RemoveRange(0, arsoRole.DousedPlayers.Count);
                arsoRole.Cooldown = CustomGameOptions.DouseCd;
            }

            else if (role == RoleEnum.SoulCollector)
            {
                var scRole = Role.GetRole<SoulCollector>(shifter);
                scRole.Cooldown = CustomGameOptions.ReapCd;
            }

            else if (role == RoleEnum.Survivor)
            {
                var survRole = Role.GetRole<Survivor>(shifter);
                survRole.LastVested = DateTime.UtcNow;
                survRole.UsesLeft = CustomGameOptions.MaxVests;
            }

            else if (role == RoleEnum.GuardianAngel)
            {
                var gaRole = Role.GetRole<GuardianAngel>(shifter);
                gaRole.Cooldown = CustomGameOptions.ProtectCd;
                gaRole.UsesLeft = CustomGameOptions.MaxProtects;
            }

            else if (role == RoleEnum.Glitch)
            {
                var glitchRole = Role.GetRole<Glitch>(shifter);
                if (glitchRole.IsUsingMimic)
                {
                Utils.Unmorph(glitchRole.Player);
                glitchRole.IsUsingMimic = false;
                }
                glitchRole.MimicCooldown = CustomGameOptions.MimicCooldown;
                glitchRole.HackCooldown = CustomGameOptions.HackCooldown;
                glitchRole.Cooldown = CustomGameOptions.GlitchKillCooldown;
                glitchRole.MimicButton.gameObject.SetActive(false);
                glitchRole.HackButton.gameObject.SetActive(false);
            }

            else if (role == RoleEnum.Juggernaut)
            {
                var juggRole = Role.GetRole<Juggernaut>(shifter);
                juggRole.JuggKills = 0;
                juggRole.Cooldown = CustomGameOptions.JuggKCd;
            }

            else if (role == RoleEnum.Grenadier)
            {
                var grenadeRole = Role.GetRole<Grenadier>(shifter);
                grenadeRole.Cooldown = CustomGameOptions.GrenadeCd;
            }

            else if (role == RoleEnum.Assassin)
            {
                new Assassin(shifter);
            }

            else if (role == RoleEnum.Morphling)
            {
                var morphlingRole = Role.GetRole<Morphling>(shifter);
                if (morphlingRole.Morphed)
                {
                morphlingRole.Unmorph();
                Utils.Rpc(CustomRPC.UnMorph, shifter.PlayerId);
                morphlingRole.MorphedPlayer = null;
                }
                morphlingRole.Cooldown = CustomGameOptions.MorphlingCd;
            }

            else if (role == RoleEnum.Escapist)
            {
                var escapistRole = Role.GetRole<Escapist>(shifter);
                escapistRole.Cooldown = CustomGameOptions.EscapeCd;
            }

            else if (role == RoleEnum.Hypnotist)
            {
                var hypnotistRole = Role.GetRole<Hypnotist>(shifter);
                hypnotistRole.Cooldown = CustomGameOptions.HypnotiseCd;
                hypnotistRole.HypnotisedPlayers.RemoveRange(0, hypnotistRole.HypnotisedPlayers.Count);
                hypnotistRole.HysteriaActive = false;
            }

            else if (role == RoleEnum.Swooper)
            {
                var swooperRole = Role.GetRole<Swooper>(shifter);
                if (swooperRole.IsSwooped)
                {
                swooperRole.UnSwoop();
                Utils.Rpc(CustomRPC.UnSwoop, shifter.PlayerId);
                }
                swooperRole.Enabled = false;
                swooperRole.Cooldown = CustomGameOptions.SwoopCd;
            }

            else if (role == RoleEnum.Venerer)
            {
                var venererRole = Role.GetRole<Venerer>(shifter);
                venererRole.Cooldown = CustomGameOptions.AbilityCd;
            }

            else if (role == RoleEnum.Blackmailer)
            {
                var blackmailerRole = Role.GetRole<Blackmailer>(shifter);
                blackmailerRole.Cooldown = CustomGameOptions.BlackmailCd;
                blackmailerRole.Blackmailed = null;
            }

            else if (role == RoleEnum.Manipulator)
            {
                var ManipulatorRole = Role.GetRole<Manipulator>(shifter);
                ManipulatorRole.Cooldown = CustomGameOptions.ManipulateCD;
            }

            else if (role == RoleEnum.Converter)
            {
                var converterRole = Role.GetRole<Converter>(shifter);
                converterRole.AbilityUsed = true;
            }

            else if (role == RoleEnum.Miner)
            {
                var minerRole = Role.GetRole<Miner>(shifter);
                minerRole.Cooldown = CustomGameOptions.MineCd;
            }

            else if (role == RoleEnum.Undertaker)
            {
                var dienerRole = Role.GetRole<Undertaker>(shifter);
                dienerRole.Cooldown = CustomGameOptions.DragCd;
            }

            else if (role == RoleEnum.PotionMaster)
            {
                var pmRole = Role.GetRole<PotionMaster>(shifter);
                pmRole.PotionCooldown = CustomGameOptions.PotionCD;
            }

            else if (role == RoleEnum.HexMaster)
            {
                var hmRole = Role.GetRole<HexMaster>(shifter);
                hmRole.Cooldown = CustomGameOptions.CovenKCD;
            }

            else if (role == RoleEnum.Maul)
            {
                var wwRole = Role.GetRole<Maul>(shifter);
                wwRole.RampageCooldown = CustomGameOptions.RampageCd;
                wwRole.Cooldown = CustomGameOptions.RampageKillCd;
            }

            else if (role == RoleEnum.Doomsayer)
            {
                var doomRole = Role.GetRole<Doomsayer>(shifter);
                doomRole.GuessedCorrectly = 0;
                doomRole.Cooldown = CustomGameOptions.ObserveCooldown;
                doomRole.LastObservedPlayer = null;
            }

            else if (role == RoleEnum.Vulture)
            {
                var vultureRole = Role.GetRole<Vulture>(shifter);
                vultureRole.BodiesEaten = 0;
                vultureRole.Cooldown = CustomGameOptions.VultureCD;
            }

            else if (role == RoleEnum.Plaguebearer)
            {
                var plagueRole = Role.GetRole<Plaguebearer>(shifter);
                plagueRole.InfectedPlayers.RemoveRange(0, plagueRole.InfectedPlayers.Count);
                plagueRole.InfectedPlayers.Add(shifter.PlayerId);
                plagueRole.LastInfected = DateTime.UtcNow;
            }

            else if (role == RoleEnum.Terrorist)
            {
                var terroristRole = Role.GetRole<Terrorist>(shifter);
                terroristRole.Cooldown = CustomGameOptions.TerroristKillCD;
            }

            else if (role == RoleEnum.Pestilence)
            {
                var pestRole = Role.GetRole<Pestilence>(shifter);
                pestRole.Cooldown = CustomGameOptions.PestKillCd;
            }

            else if (role == RoleEnum.Vampire)
            {
                var vampRole = Role.GetRole<Vampire>(shifter);
                vampRole.Cooldown = CustomGameOptions.BiteCd;
            }

            else if (role == RoleEnum.Doppelganger)
            {
                var doppelRole = Role.GetRole<Doppelganger>(shifter);
                doppelRole.Cooldown = CustomGameOptions.DoppelKCD;
            }

            else if (role == RoleEnum.SerialKiller)
            {
                var skRole = Role.GetRole<SerialKiller>(shifter);
                skRole.Cooldown = CustomGameOptions.SerialKillerKCD;
                skRole.ConvertCooldown = CustomGameOptions.SerialKillerKCD;
                skRole.Converted = true;
            }

            else if (role == RoleEnum.Infectious)
            {
                var infectiousRole = Role.GetRole<Infectious>(shifter);
                infectiousRole.Cooldown = CustomGameOptions.InfectiousCD;
            }

            else if (role == RoleEnum.Mutant)
            {
                var mutant = Role.GetRole<Mutant>(shifter);
                mutant.TransformCooldown = CustomGameOptions.TransformCD;
                mutant.Cooldown = CustomGameOptions.MutantKCD;
                mutant.IsTransformed = false;
                other.MyPhysics.SetBodyType(PlayerBodyTypes.Normal);
                Utils.Rpc(CustomRPC.UnTransform, other.PlayerId);
            }

            else if (role == RoleEnum.Trapper)
            {
                var trapperRole = Role.GetRole<Trapper>(shifter);
                trapperRole.Cooldown = CustomGameOptions.TrapCooldown;
                trapperRole.UsesLeft = CustomGameOptions.MaxTraps;
                trapperRole.trappedPlayers.Clear();
                trapperRole.traps.ClearTraps();
            }

            else if (role == RoleEnum.Bomber)
            {
                var bomberRole = Role.GetRole<Bomber>(shifter);
                bomberRole.Bomb.ClearBomb();
            }

            else if (!(shifter.Is(RoleEnum.Altruist) || shifter.Is(RoleEnum.Amnesiac) || shifter.Is(RoleEnum.Shifter) || shifter.Is(Faction.Impostors)))
            {
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            var killsList = (newRole.Kills, newRole.CorrectKills, newRole.IncorrectKills, newRole.CorrectAssassinKills, newRole.IncorrectAssassinKills);
            var otherRole = Role.GetRole(other);
            newRole.Kills = otherRole.Kills;
            newRole.CorrectKills = otherRole.CorrectKills;
            newRole.IncorrectKills = otherRole.IncorrectKills;
            newRole.CorrectAssassinKills = otherRole.CorrectAssassinKills;
            newRole.IncorrectAssassinKills = otherRole.IncorrectAssassinKills;
            otherRole.Kills = killsList.Kills;
            otherRole.CorrectKills = killsList.CorrectKills;
            otherRole.IncorrectKills = killsList.IncorrectKills;
            otherRole.CorrectAssassinKills = killsList.CorrectAssassinKills;
            otherRole.IncorrectAssassinKills = killsList.IncorrectAssassinKills;
            otherRole.RegenTask();
            Utils.Unmorph(other);
            other.myRend().color = Color.white;

            if (shifter.Is(Faction.Impostors) && (!shifter.Is(RoleEnum.Traitor) || CustomGameOptions.SnitchSeesTraitor))
            {
                foreach (var snitch in Role.GetRoles(RoleEnum.Snitch))
                {
                    var snitchRole = (Snitch)snitch;
                    if (snitchRole.TasksDone && PlayerControl.LocalPlayer.Is(RoleEnum.Snitch))
                    {
                        var gameObj = new GameObject();
                        var arrow = gameObj.AddComponent<ArrowBehaviour>();
                        gameObj.transform.parent = PlayerControl.LocalPlayer.gameObject.transform;
                        var renderer = gameObj.AddComponent<SpriteRenderer>();
                        renderer.sprite = Sprite;
                        arrow.image = renderer;
                        gameObj.layer = 5;
                        snitchRole.SnitchArrows.Add(shifter.PlayerId, arrow);
                    }
                    else if (snitchRole.Revealed && PlayerControl.LocalPlayer == shifter)
                    {
                        var gameObj = new GameObject();
                        var arrow = gameObj.AddComponent<ArrowBehaviour>();
                        gameObj.transform.parent = PlayerControl.LocalPlayer.gameObject.transform;
                        var renderer = gameObj.AddComponent<SpriteRenderer>();
                        renderer.sprite = Sprite;
                        arrow.image = renderer;
                        gameObj.layer = 5;
                        snitchRole.ImpArrows.Add(arrow);
                    }
                }
            }

            shiftRole.Cooldown = CustomGameOptions.ShiftCD;

            if (shifter.Is(Faction.Coven))
            {
                Role.GetRole(shifter).KillCooldown = CustomGameOptions.CovenKCD;
            }

            yield return new WaitForSeconds(0.5f);

            PlayerControl_Die.CheckEnd();

            yield return false;
        }
    }
}