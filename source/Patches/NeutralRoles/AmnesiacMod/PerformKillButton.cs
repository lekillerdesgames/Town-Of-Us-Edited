using HarmonyLib;
using TownOfUsEdited.CrewmateRoles.InvestigatorMod;
using TownOfUsEdited.CrewmateRoles.SnitchMod;
using TownOfUsEdited.CrewmateRoles.TrapperMod;
using TownOfUsEdited.Roles;
using UnityEngine;
using System;
using TownOfUsEdited.Extensions;
using TownOfUsEdited.CrewmateRoles.ImitatorMod;
using AmongUs.GameOptions;
using TownOfUsEdited.Roles.Modifiers;
using TownOfUsEdited.ImpostorRoles.BomberMod;
using Assassin = TownOfUsEdited.Roles.Modifiers.Assassin;
using TownOfUsEdited.Patches;
using TownOfUsEdited.CrewmateRoles.MedicMod;
using System.Linq;

namespace TownOfUsEdited.NeutralRoles.AmnesiacMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKillButton
    {
        public static Sprite Sprite => TownOfUsEdited.Arrow;
        public static bool Prefix(KillButton __instance)
        {
            if (__instance != DestroyableSingleton<HudManager>.Instance.KillButton) return true;
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Amnesiac);
            if (!flag) return true;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            var role = Role.GetRole<Amnesiac>(PlayerControl.LocalPlayer);

            var flag2 = __instance.isCoolingDown;
            if (flag2) return false;
            if (!__instance.enabled) return false;
            var maxDistance = LegacyGameOptions.KillDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance];
            if (role == null)
                return false;
            if (role.CurrentTarget == null)
                return false;
            if (Vector2.Distance(role.CurrentTarget.TruePosition,
                PlayerControl.LocalPlayer.GetTruePosition()) > maxDistance) return false;
            var playerId = role.CurrentTarget.ParentId;
            if (role.CurrentTarget.IsDouble())
            {
                var matches = Murder.KilledPlayers.ToArray().Where(x => x.KillerId == role.CurrentTarget.ParentId && x.isDoppel == true).ToList();
                if (matches.Any())
                {
                    foreach (var killedPlayer in matches)
                    {
                        playerId = killedPlayer.PlayerId;
                    }
                }
            }
            var player = Utils.PlayerById(playerId);
            var abilityUsed = Utils.AbilityUsed(PlayerControl.LocalPlayer);
            if (!abilityUsed) return false;
            if ((player.IsInfected() || role.Player.IsInfected()) && !player.Is(RoleEnum.Plaguebearer))
            {
                foreach (var pb in Role.GetRoles(RoleEnum.Plaguebearer)) ((Plaguebearer)pb).RpcSpreadInfection(player, role.Player);
            }

            Utils.Rpc(CustomRPC.Remember, PlayerControl.LocalPlayer.PlayerId, playerId);

            Remember(role, player);
            return false;
        }

        public static void Remember(Amnesiac amneRole, PlayerControl other)
        {
            var role = Utils.GetRole(other);
            var roleInstance = Role.GetRole(other);
            var amnesiac = amneRole.Player;

            var rememberImp = true;
            var rememberNeut = false;
            var rememberCrewmate = false;
            bool isMadmate = false;

            Role newRole;

            if (PlayerControl.LocalPlayer == amnesiac)
            {
                var amnesiacRole = Role.GetRole<Amnesiac>(amnesiac);
                amnesiacRole.BodyArrows.Values.DestroyAll();
                amnesiacRole.BodyArrows.Clear();
                try
                {
                    foreach (var body in amnesiacRole.CurrentTarget.bodyRenderers) body.material.SetFloat("_Outline", 0f);
                }
                catch
                {

                }
            }

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
                case RoleEnum.Hunter:
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
                case RoleEnum.Bodyguard:
                case RoleEnum.Crusader:
                case RoleEnum.Imitator:
                case RoleEnum.VampireHunter:
                case RoleEnum.Prosecutor:
                case RoleEnum.Oracle:
                case RoleEnum.Aurial:
                case RoleEnum.Paranoïac:
                case RoleEnum.Warden:
                case RoleEnum.Politician:
                case RoleEnum.Plumber:
                case RoleEnum.Cleric:

                    rememberImp = false;
                    rememberNeut = false;
                    rememberCrewmate = true;

                    break;

                case RoleEnum.Jester:
                case RoleEnum.Vulture:
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
                case RoleEnum.Pestilence:
                case RoleEnum.Maul:
                case RoleEnum.Doomsayer:
                case RoleEnum.Vampire:
                case RoleEnum.SoulCollector:
                case RoleEnum.Mercenary:

                    rememberImp = false;
                    rememberNeut = true;
                    rememberCrewmate = false;

                    break;

                case RoleEnum.Coven:
                case RoleEnum.Ritualist:
                case RoleEnum.HexMaster:
                case RoleEnum.CovenLeader:
                case RoleEnum.Spiritualist:
                case RoleEnum.VoodooMaster:
                case RoleEnum.PotionMaster:

                    rememberImp = false;
                    rememberNeut = false;
                    rememberCrewmate = false;

                    break;
            }

            newRole = Role.GetRole(other);
            if (newRole.Faction == Faction.Madmates) isMadmate = true;
            newRole.Player = amnesiac;

            if ((role == RoleEnum.Arsonist || role == RoleEnum.Plaguebearer || role == RoleEnum.Pestilence
                 || role == RoleEnum.Grenadier) && PlayerControl.LocalPlayer == other)
            {
                foreach (var visor in PlayerControl.AllPlayerControls)
                {
                    ShowShield.ClearVisor(visor);
                }
            }

            if (role == RoleEnum.Investigator) Footprint.DestroyAll(Role.GetRole<Investigator>(other));

            if (role == RoleEnum.Snitch) CompleteTask.Postfix(amnesiac);

            Role.RoleDictionary.Remove(amnesiac.PlayerId);
            Role.RoleDictionary.Remove(other.PlayerId);
            Role.RoleDictionary.Add(amnesiac.PlayerId, newRole);

            newRole.RegenTask();

            if (PlayerControl.LocalPlayer == amnesiac || PlayerControl.LocalPlayer == other)
            {
                HudManager.Instance.KillButton.buttonLabelText.gameObject.SetActive(false);
                HudManager.Instance.KillButton.gameObject.SetActive(false);
            }

            if (StartImitate.ImitatingPlayers.Contains(other.PlayerId))
            {
                StartImitate.ImitatingPlayers.Remove(other.PlayerId);
                StartImitate.ImitatingPlayers.Add(amneRole.Player.PlayerId);
                newRole.AddToRoleHistory(RoleEnum.Imitator);
            }
            else newRole.AddToRoleHistory(newRole.RoleType);

            if (rememberImp == false)
            {
                if (rememberCrewmate == true)
                {
                    var crewmate = new Crewmate(other);
                    crewmate.RegenTask();
                    if (isMadmate) Utils.TurnMadmate(other, false);
                    Role.GetRole(other).RegenTask();
                }
                else if (rememberNeut)
                {
                    if (role == RoleEnum.Arsonist || role == RoleEnum.Glitch || role == RoleEnum.Plaguebearer ||
                            role == RoleEnum.Pestilence || role == RoleEnum.Maul || role == RoleEnum.Juggernaut
                             || role == RoleEnum.Vampire || role == RoleEnum.SerialKiller || role == RoleEnum.Mutant ||
                             role == RoleEnum.Attacker || role == RoleEnum.Terrorist || role == RoleEnum.Infectious
                             || role == RoleEnum.Doppelganger)
                    {
                        if (other.Is(AbilityEnum.Assassin)) Ability.AbilityDictionary.Remove(other.PlayerId);
                        var survivor = new Survivor(other);
                        survivor.RegenTask();
                        if (CustomGameOptions.AmneTurnNeutAssassin
                        && !CustomGameOptions.AssassinImpostorRole) new Assassin(amnesiac);
                    }
                }
                else
                {
                    var coven = new Coven(other);
                    coven.RegenTask();
                }
            }
            else if (rememberImp == true)
            {
                new Impostor(other);
                RoleManager.Instance.SetRole(amnesiac, RoleTypes.Crewmate);
                Role.GetRole(other).KillCooldown = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if ((player.Data.IsImpostor() && PlayerControl.LocalPlayer.Data.IsImpostor())
                    || (PlayerControl.LocalPlayer.Data.IsImpostor() && player.Is(Faction.Madmates)))
                    {
                        player.nameText().color = Patches.Colors.Impostor;
                    }
                }
                if (CustomGameOptions.AmneTurnImpAssassin
                && !CustomGameOptions.AssassinImpostorRole) new Assassin(amnesiac);
            }

            if (role == RoleEnum.Snitch)
            {
                var snitchRole = Role.GetRole<Snitch>(amnesiac);
                snitchRole.ImpArrows.DestroyAll();
                snitchRole.SnitchArrows.Values.DestroyAll();
                snitchRole.SnitchArrows.Clear();
                CompleteTask.Postfix(amnesiac);
                if (other.AmOwner)
                    foreach (var player in PlayerControl.AllPlayerControls)
                        player.nameText().color = Color.white;
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (role == RoleEnum.Sheriff)
            {
                var sheriffRole = Role.GetRole<Sheriff>(amnesiac);
                sheriffRole.Cooldown = CustomGameOptions.SheriffKillCd;
            }

            else if (role == RoleEnum.Knight)
            {
                var knightRole = Role.GetRole<Knight>(amnesiac);
                knightRole.Cooldown = CustomGameOptions.KnightKCD;
                knightRole.UsesLeft = 1;
            }

            else if (role == RoleEnum.Fighter)
            {
                var fighterRole = Role.GetRole<Fighter>(amnesiac);
                fighterRole.Cooldown = CustomGameOptions.FighterKCD;
            }

            else if (role == RoleEnum.Engineer)
            {
                var engiRole = Role.GetRole<Engineer>(amnesiac);
                engiRole.UsesLeft = CustomGameOptions.MaxFixes;
            }

            else if (role == RoleEnum.Medic)
            {
                var medicRole = Role.GetRole<Medic>(amnesiac);
                medicRole.ShieldedPlayer = null;
            }

            else if (role == RoleEnum.Cleric)
            {
                var clericRole = Role.GetRole<Cleric>(amnesiac);
                clericRole.Cooldown = CustomGameOptions.BarrierCD;
            }

            else if (role == RoleEnum.Plumber)
            {
                var plumberRole = Role.GetRole<Plumber>(amnesiac);
                plumberRole.UsesLeft = CustomGameOptions.MaxBarricades;
                plumberRole.FutureBlocks.Clear();
                plumberRole.Cooldown = CustomGameOptions.FlushCd;
            }

            else if (role == RoleEnum.Mayor)
            {
                var mayorRole = Role.GetRole<Mayor>(amnesiac);
                mayorRole.Revealed = false;
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (role == RoleEnum.Politician)
            {
                var pnRole = Role.GetRole<Politician>(amnesiac);
                pnRole.CampaignedPlayers.RemoveRange(0, pnRole.CampaignedPlayers.Count);
                pnRole.Cooldown = CustomGameOptions.CampaignCd;
            }

            else if (role == RoleEnum.SoulCollector)
            {
                var scRole = Role.GetRole<SoulCollector>(amnesiac);
                scRole.Cooldown = CustomGameOptions.ReapCd;
            }

            else if (role == RoleEnum.Mercenary)
            {
                var mercRole = Role.GetRole<Mercenary>(amnesiac);
                mercRole.Cooldown = CustomGameOptions.MercenaryCD;
                mercRole.Guarded.Clear();
                mercRole.Bribed.Clear();
                mercRole.Alert = false;
            }

            else if (role == RoleEnum.Prosecutor)
            {
                var prosRole = Role.GetRole<Prosecutor>(amnesiac);
                prosRole.Prosecuted = false;
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (role == RoleEnum.Vigilante)
            {
                var vigiRole = Role.GetRole<Vigilante>(amnesiac);
                vigiRole.RemainingKills = CustomGameOptions.VigilanteKills;
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (role == RoleEnum.Veteran)
            {
                var vetRole = Role.GetRole<Veteran>(amnesiac);
                vetRole.UsesLeft = CustomGameOptions.MaxAlerts;
                vetRole.Cooldown = CustomGameOptions.AlertCd;
            }

            else if (role == RoleEnum.Astral)
            {
                var astralRole = Role.GetRole<Astral>(amnesiac);
                astralRole.Cooldown = CustomGameOptions.GhostCD;
                astralRole.Enabled = false;
            }

            else if (role == RoleEnum.Lookout)
            {
                var lookoutRole = Role.GetRole<Lookout>(amnesiac);
                lookoutRole.Cooldown = CustomGameOptions.WatchCD;
            }

            else if (role == RoleEnum.Hunter)
            {
                var hunterRole = Role.GetRole<Hunter>(amnesiac);
                hunterRole.UsesLeft = CustomGameOptions.HunterStalkUses;
                hunterRole.StalkCooldown = CustomGameOptions.HunterStalkCd;
                hunterRole.Cooldown = CustomGameOptions.HunterKillCd;
            }

            else if (role == RoleEnum.Doctor)
            {
                var docRole = Role.GetRole<Doctor>(amnesiac);
                docRole.Cooldown = CustomGameOptions.DocReviveCooldown;
            }

            else if (role == RoleEnum.TimeLord)
            {
                var tlRole = Role.GetRole<TimeLord>(amnesiac);
                tlRole.Cooldown = CustomGameOptions.RewindCooldown;
            }

            else if (role == RoleEnum.Crusader)
            {
                var crusRole = Role.GetRole<Crusader>(amnesiac);
                crusRole.Cooldown = CustomGameOptions.CrusadeCD;
            }

            else if (role == RoleEnum.Tracker)
            {
                var trackerRole = Role.GetRole<Tracker>(amnesiac);
                trackerRole.TrackerArrows.Values.DestroyAll();
                trackerRole.TrackerArrows.Clear();
                trackerRole.UsesLeft = CustomGameOptions.MaxTracks;
                trackerRole.Cooldown = CustomGameOptions.TrackCd;
            }

            else if (role == RoleEnum.VampireHunter)
            {
                var vhRole = Role.GetRole<VampireHunter>(amnesiac);
                if (vhRole.AddedStakes) vhRole.UsesLeft = CustomGameOptions.MaxFailedStakesPerGame;
                else vhRole.UsesLeft = 0;
                vhRole.Cooldown = CustomGameOptions.StakeCd;
            }

            else if (role == RoleEnum.Captain)
            {
                var capRole = Role.GetRole<Captain>(amnesiac);
                capRole.Cooldown = CustomGameOptions.ZoomCooldown;
            }

            else if (role == RoleEnum.Jailor)
            {
                var jailRole = Role.GetRole<Jailor>(amnesiac);
                jailRole.Cooldown = CustomGameOptions.JailCD;
                jailRole.JailedPlayer = null;
            }

            else if (role == RoleEnum.Chameleon)
            {
                var chamRole = Role.GetRole<Chameleon>(amnesiac);
                chamRole.Enabled = false;
                chamRole.Cooldown = CustomGameOptions.ChamSwoopCooldown;
            }

            else if (role == RoleEnum.Detective)
            {
                var detectiveRole = Role.GetRole<Detective>(amnesiac);
                detectiveRole.Cooldown = CustomGameOptions.ExamineCd;
                detectiveRole.CurrentTarget = null;
            }

            else if (role == RoleEnum.Mystic)
            {
                var mysticRole = Role.GetRole<Mystic>(amnesiac);
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (role == RoleEnum.Transporter)
            {
                var tpRole = Role.GetRole<Transporter>(amnesiac);
                tpRole.TransportPlayer1 = null;
                tpRole.TransportPlayer2 = null;
                tpRole.Cooldown = CustomGameOptions.TransportCooldown;
                tpRole.UsesLeft = CustomGameOptions.TransportMaxUses;
            }

            else if (role == RoleEnum.Medium)
            {
                var medRole = Role.GetRole<Medium>(amnesiac);
                medRole.MediatedPlayers.Values.DestroyAll();
                medRole.MediatedPlayers.Clear();
                medRole.Cooldown = CustomGameOptions.MediateCooldown;
            }

            else if (role == RoleEnum.Seer)
            {
                var seerRole = Role.GetRole<Seer>(amnesiac);
                seerRole.Investigated.RemoveRange(0, seerRole.Investigated.Count);
                seerRole.Cooldown = CustomGameOptions.SeerCd;
            }

            else if (role == RoleEnum.Deputy)
            {
                var deputyRole = Role.GetRole<Deputy>(amnesiac);
                deputyRole.Camping = null;
                deputyRole.Killer = null;
                deputyRole.CampedThisRound = false;
            }

            else if (role == RoleEnum.Oracle)
            {
                var oracleRole = Role.GetRole<Oracle>(amnesiac);
                oracleRole.Blessed = null;
                oracleRole.Cooldown = CustomGameOptions.ConfessCd;
            }

            else if (role == RoleEnum.Aurial)
            {
                var aurialRole = Role.GetRole<Aurial>(amnesiac);
                aurialRole.SenseArrows.Values.DestroyAll();
                aurialRole.SenseArrows.Clear();
                if (PlayerControl.LocalPlayer == aurialRole.Player) DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            else if (role == RoleEnum.Arsonist)
            {
                var arsoRole = Role.GetRole<Arsonist>(amnesiac);
                arsoRole.DousedPlayers.RemoveRange(0, arsoRole.DousedPlayers.Count);
                arsoRole.Cooldown = CustomGameOptions.DouseCd;
            }

            else if (role == RoleEnum.Survivor)
            {
                var survRole = Role.GetRole<Survivor>(amnesiac);
                survRole.LastVested = DateTime.UtcNow;
                survRole.UsesLeft = CustomGameOptions.MaxVests;
            }

            else if (role == RoleEnum.GuardianAngel)
            {
                var gaRole = Role.GetRole<GuardianAngel>(amnesiac);
                gaRole.Cooldown = CustomGameOptions.ProtectCd;
                gaRole.UsesLeft = CustomGameOptions.MaxProtects;
            }

            else if (role == RoleEnum.Glitch)
            {
                var glitchRole = Role.GetRole<Glitch>(amnesiac);
                glitchRole.MimicCooldown = CustomGameOptions.MimicCooldown;
                glitchRole.HackCooldown = CustomGameOptions.HackCooldown;
                glitchRole.Cooldown = CustomGameOptions.GlitchKillCooldown;
            }

            else if (role == RoleEnum.Juggernaut)
            {
                var juggRole = Role.GetRole<Juggernaut>(amnesiac);
                juggRole.JuggKills = 0;
                juggRole.Cooldown = CustomGameOptions.JuggKCd;
            }

            else if (role == RoleEnum.Grenadier)
            {
                var grenadeRole = Role.GetRole<Grenadier>(amnesiac);
                grenadeRole.Cooldown = CustomGameOptions.GrenadeCd;
            }

            else if (role == RoleEnum.Assassin)
            {
                new Assassin(amnesiac);
            }

            else if (role == RoleEnum.Morphling)
            {
                var morphlingRole = Role.GetRole<Morphling>(amnesiac);
                morphlingRole.Cooldown = CustomGameOptions.MorphlingCd;
            }

            else if (role == RoleEnum.Escapist)
            {
                var escapistRole = Role.GetRole<Escapist>(amnesiac);
                escapistRole.Cooldown = CustomGameOptions.EscapeCd;
            }

            else if (role == RoleEnum.Hypnotist)
            {
                var hypnotistRole = Role.GetRole<Hypnotist>(amnesiac);
                hypnotistRole.Cooldown = CustomGameOptions.HypnotiseCd;
                hypnotistRole.HypnotisedPlayers.RemoveRange(0, hypnotistRole.HypnotisedPlayers.Count);
                hypnotistRole.HysteriaActive = false;
            }

            else if (role == RoleEnum.Swooper)
            {
                var swooperRole = Role.GetRole<Swooper>(amnesiac);
                swooperRole.Cooldown = CustomGameOptions.SwoopCd;
            }

            else if (role == RoleEnum.Venerer)
            {
                var venererRole = Role.GetRole<Venerer>(amnesiac);
                venererRole.Cooldown = CustomGameOptions.AbilityCd;
            }

            else if (role == RoleEnum.Blackmailer)
            {
                var blackmailerRole = Role.GetRole<Blackmailer>(amnesiac);
                blackmailerRole.Cooldown = CustomGameOptions.BlackmailCd;
                blackmailerRole.Blackmailed = null;
            }

            else if (role == RoleEnum.Manipulator)
            {
                var ManipulatorRole = Role.GetRole<Manipulator>(amnesiac);
                ManipulatorRole.Cooldown = CustomGameOptions.ManipulateCD;
            }

            else if (role == RoleEnum.Converter)
            {
                var converterRole = Role.GetRole<Converter>(amnesiac);
                converterRole.AbilityUsed = false;
            }

            else if (role == RoleEnum.Miner)
            {
                var minerRole = Role.GetRole<Miner>(amnesiac);
                minerRole.Cooldown = CustomGameOptions.MineCd;
            }

            else if (role == RoleEnum.Undertaker)
            {
                var dienerRole = Role.GetRole<Undertaker>(amnesiac);
                dienerRole.Cooldown = CustomGameOptions.DragCd;
            }

            else if (role == RoleEnum.PotionMaster)
            {
                var pmRole = Role.GetRole<PotionMaster>(amnesiac);
                pmRole.PotionCooldown = CustomGameOptions.PotionCD;
            }

            else if (role == RoleEnum.HexMaster)
            {
                var hmRole = Role.GetRole<HexMaster>(amnesiac);
                hmRole.Cooldown = CustomGameOptions.CovenKCD;
            }

            else if (role == RoleEnum.Maul)
            {
                var wwRole = Role.GetRole<Maul>(amnesiac);
                wwRole.RampageCooldown = CustomGameOptions.RampageCd;
                wwRole.Cooldown = CustomGameOptions.RampageKillCd;
            }

            else if (role == RoleEnum.Doomsayer)
            {
                var doomRole = Role.GetRole<Doomsayer>(amnesiac);
                doomRole.GuessedCorrectly = 0;
                doomRole.Cooldown = CustomGameOptions.ObserveCooldown;
                doomRole.LastObservedPlayer = null;
            }

            else if (role == RoleEnum.Vulture)
            {
                var vultureRole = Role.GetRole<Vulture>(amnesiac);
                vultureRole.BodiesEaten = 0;
                vultureRole.Cooldown = CustomGameOptions.VultureCD;
            }

            else if (role == RoleEnum.Plaguebearer)
            {
                var plagueRole = Role.GetRole<Plaguebearer>(amnesiac);
                plagueRole.InfectedPlayers.RemoveRange(0, plagueRole.InfectedPlayers.Count);
                plagueRole.InfectedPlayers.Add(amnesiac.PlayerId);
                plagueRole.LastInfected = DateTime.UtcNow;
            }

            else if (role == RoleEnum.Terrorist)
            {
                var terroristRole = Role.GetRole<Terrorist>(amnesiac);
                terroristRole.Cooldown = CustomGameOptions.TerroristKillCD;
            }

            else if (role == RoleEnum.Pestilence)
            {
                var pestRole = Role.GetRole<Pestilence>(amnesiac);
                pestRole.Cooldown = CustomGameOptions.PestKillCd;
            }

            else if (role == RoleEnum.Vampire)
            {
                var vampRole = Role.GetRole<Vampire>(amnesiac);
                vampRole.Cooldown = CustomGameOptions.BiteCd;
            }
            else if (role == RoleEnum.Doppelganger)
            {
                var doppelRole = Role.GetRole<Doppelganger>(amnesiac);
                doppelRole.Cooldown = CustomGameOptions.DoppelKCD;
            }

            else if (role == RoleEnum.SerialKiller)
            {
                var skRole = Role.GetRole<SerialKiller>(amnesiac);
                skRole.Cooldown = CustomGameOptions.SerialKillerKCD;
                skRole.ConvertCooldown = CustomGameOptions.SerialKillerKCD;
                skRole.Converted = true;
            }

            else if (role == RoleEnum.Mutant)
            {
                var mutant = Role.GetRole<Mutant>(amnesiac);
                mutant.TransformCooldown = CustomGameOptions.TransformCD;
                mutant.Cooldown = CustomGameOptions.MutantKCD;
                mutant.IsTransformed = false;
            }

            else if (role == RoleEnum.Infectious)
            {
                var infectiousRole = Role.GetRole<Infectious>(amnesiac);
                infectiousRole.Cooldown = CustomGameOptions.InfectiousCD;
            }

            else if (role == RoleEnum.Trapper)
            {
                var trapperRole = Role.GetRole<Trapper>(amnesiac);
                trapperRole.Cooldown = CustomGameOptions.TrapCooldown;
                trapperRole.UsesLeft = CustomGameOptions.MaxTraps;
                trapperRole.trappedPlayers.Clear();
                trapperRole.traps.ClearTraps();
            }

            else if (role == RoleEnum.Bomber)
            {
                var bomberRole = Role.GetRole<Bomber>(amnesiac);
                bomberRole.Bomb.ClearBomb();
            }

            else if (!(amnesiac.Is(RoleEnum.Altruist) || amnesiac.Is(RoleEnum.Amnesiac) || amnesiac.Is(Faction.Impostors)))
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
            otherRole.DeathReason = roleInstance.DeathReason;

            if (amnesiac.Is(RoleEnum.Poisoner))
            {
                if (PlayerControl.LocalPlayer == amnesiac)
                {
                    var poisonerRole = Role.GetRole<Poisoner>(amnesiac);
                    poisonerRole.Cooldown = CustomGameOptions.PoisonCD;
                    poisonerRole.PoisonedPlayer = null;
                    DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
                }
            }

            if (amnesiac.Is(Faction.Impostors) && (!amnesiac.Is(RoleEnum.Traitor) || CustomGameOptions.SnitchSeesTraitor))
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
                        snitchRole.SnitchArrows.Add(amnesiac.PlayerId, arrow);
                    }
                    else if (snitchRole.Revealed && PlayerControl.LocalPlayer == amnesiac)
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

            if (amnesiac.Is(Faction.Coven))
            {
                Role.GetRole(amnesiac).KillCooldown = CustomGameOptions.CovenKCD;
            }

            PlayerControl_Die.CheckEnd();
        }
    }
}
