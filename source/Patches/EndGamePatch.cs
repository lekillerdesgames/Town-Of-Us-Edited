using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using Reactor.Utilities.Extensions;
using TownOfUsEdited.Roles;
using AmongUs.GameOptions;
using TownOfUsEdited.CrewmateRoles.MedicMod;
using Reactor.Utilities;

namespace TownOfUsEdited.Patches {

    static class AdditionalTempData {
        public static List<PlayerRoleInfo> playerRoles = new List<PlayerRoleInfo>();
        public static List<Winners> otherWinners = new List<Winners>();

        public static void clear() {
            playerRoles.Clear();
            otherWinners.Clear();
        }

        internal class PlayerRoleInfo
        {
            public string PlayerName { get; set; }
            public string Role { get; set; }
        }

        internal class Winners
        {
            public string PlayerName { get; set; }
            public RoleEnum Role { get; set; }
        }
    }


    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class OnGameEndPatch {

        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] EndGameResult endGameResult)
        {
            if (AmongUsClient.Instance.AmHost && CustomGameOptions.AutoRejoin) Coroutines.Start(HostManager.AutoRejoin());
            AdditionalTempData.clear();
            var playerRole = "";
            // Had to add this to avoid death reasons appearing two times
            bool hasDeathReason = false;
            // Theres a better way of doing this e.g. switch statement or dictionary. But this works for now.
            foreach (var playerControl in PlayerControl.AllPlayerControls)
            {
                if (playerControl == null || playerControl.Data.Disconnected) continue;
                playerRole = "";
                hasDeathReason = false;
                foreach (var role in Role.RoleHistory.Where(x => x.Key == playerControl.PlayerId))
                {
                    if (role.Value == RoleEnum.Crewmate) { playerRole += "<color=#" + Patches.Colors.Crewmate.ToHtmlStringRGBA() + ">Crewmate</color> > "; }
                    else if (role.Value == RoleEnum.Impostor) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Impostor</color> > "; }
                    else if (role.Value == RoleEnum.Converter) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Converter</color> > "; }
                    else if (role.Value == RoleEnum.Altruist) { playerRole += "<color=#" + Patches.Colors.Altruist.ToHtmlStringRGBA() + ">Altruist</color> > "; }
                    else if (role.Value == RoleEnum.Doctor) { playerRole += "<color=#" + Patches.Colors.Doctor.ToHtmlStringRGBA() + ">Doctor</color> > "; }
                    else if (role.Value == RoleEnum.TimeLord) { playerRole += "<color=#" + Patches.Colors.TimeLord.ToHtmlStringRGBA() + ">Time Lord</color> > "; }
                    else if (role.Value == RoleEnum.Bodyguard) { playerRole += "<color=#" + Patches.Colors.Bodyguard.ToHtmlStringRGBA() + ">Bodyguard</color> > "; }
                    else if (role.Value == RoleEnum.Crusader) { playerRole += "<color=#" + Patches.Colors.Crusader.ToHtmlStringRGBA() + ">Crusader</color> > "; }
                    else if (role.Value == RoleEnum.Jailor) { playerRole += "<color=#" + Patches.Colors.Jailor.ToHtmlStringRGBA() + ">Jailor</color> > "; }
                    else if (role.Value == RoleEnum.Deputy) { playerRole += "<color=#" + Patches.Colors.Deputy.ToHtmlStringRGBA() + ">Deputy</color> > "; }
                    else if (role.Value == RoleEnum.Captain) { playerRole += "<color=#" + Patches.Colors.Captain.ToHtmlStringRGBA() + ">Captain</color> > "; }
                    else if (role.Value == RoleEnum.Avenger) { playerRole += "<color=#" + Patches.Colors.Avenger.ToHtmlStringRGBA() + ">Avenger</color> > "; }
                    else if (role.Value == RoleEnum.Knight) { playerRole += "<color=#" + Patches.Colors.Knight.ToHtmlStringRGBA() + ">Knight</color> > "; }
                    else if (role.Value == RoleEnum.Fighter) { playerRole += "<color=#" + Patches.Colors.Fighter.ToHtmlStringRGBA() + ">Fighter</color> > "; }
                    else if (role.Value == RoleEnum.Mutant) { playerRole += "<color=#" + Patches.Colors.Mutant.ToHtmlStringRGBA() + ">Mutant</color> > "; }
                    else if (role.Value == RoleEnum.Infectious) { playerRole += "<color=#" + Patches.Colors.Infectious.ToHtmlStringRGBA() + ">Infectious</color> > "; }
                    else if (role.Value == RoleEnum.Shifter) { playerRole += "<color=#" + Patches.Colors.Shifter.ToHtmlStringRGBA() + ">Shifter</color> > "; }
                    else if (role.Value == RoleEnum.Engineer) { playerRole += "<color=#" + Patches.Colors.Engineer.ToHtmlStringRGBA() + ">Engineer</color> > "; }
                    else if (role.Value == RoleEnum.Investigator) { playerRole += "<color=#" + Patches.Colors.Investigator.ToHtmlStringRGBA() + ">Investigator</color> > "; }
                    else if (role.Value == RoleEnum.Mayor) { playerRole += "<color=#" + Patches.Colors.Mayor.ToHtmlStringRGBA() + ">Mayor</color> > "; }
                    else if (role.Value == RoleEnum.Medic) { playerRole += "<color=#" + Patches.Colors.Medic.ToHtmlStringRGBA() + ">Medic</color> > "; }
                    else if (role.Value == RoleEnum.Paranoïac) { playerRole += "<color=#" + Patches.Colors.Paranoïac.ToHtmlStringRGBA() + ">Paranoïac</color> > "; }
                    else if (role.Value == RoleEnum.Sheriff) { playerRole += "<color=#" + Patches.Colors.Sheriff.ToHtmlStringRGBA() + ">Sheriff</color> > "; }
                    else if (role.Value == RoleEnum.Swapper) { playerRole += "<color=#" + Patches.Colors.Swapper.ToHtmlStringRGBA() + ">Swapper</color> > "; }
                    else if (role.Value == RoleEnum.Seer) { playerRole += "<color=#" + Patches.Colors.Seer.ToHtmlStringRGBA() + ">Seer</color> > "; }
                    else if (role.Value == RoleEnum.Snitch) { playerRole += "<color=#" + Patches.Colors.Snitch.ToHtmlStringRGBA() + ">Snitch</color> > "; }
                    else if (role.Value == RoleEnum.Spy) { playerRole += "<color=#" + Patches.Colors.Spy.ToHtmlStringRGBA() + ">Spy</color> > "; }
                    else if (role.Value == RoleEnum.Vigilante) { playerRole += "<color=#" + Patches.Colors.Vigilante.ToHtmlStringRGBA() + ">Vigilante</color> > "; }
                    else if (role.Value == RoleEnum.Hunter) { playerRole += "<color=#" + Patches.Colors.Hunter.ToHtmlStringRGBA() + ">Hunter</color> > "; }
                    else if (role.Value == RoleEnum.Arsonist) { playerRole += "<color=#" + Patches.Colors.Arsonist.ToHtmlStringRGBA() + ">Arsonist</color> > "; }
                    else if (role.Value == RoleEnum.Executioner) { playerRole += "<color=#" + Patches.Colors.Executioner.ToHtmlStringRGBA() + ">Executioner</color> > "; }
                    else if (role.Value == RoleEnum.Troll) { playerRole += "<color=#" + Patches.Colors.Troll.ToHtmlStringRGBA() + ">Troll</color> > "; }
                    else if (role.Value == RoleEnum.Glitch) { playerRole += "<color=#" + Patches.Colors.Glitch.ToHtmlStringRGBA() + ">The Glitch</color> > "; }
                    else if (role.Value == RoleEnum.Jester) { playerRole += "<color=#" + Patches.Colors.Jester.ToHtmlStringRGBA() + ">Jester</color> > "; }
                    else if (role.Value == RoleEnum.Vulture) { playerRole += "<color=#" + Patches.Colors.Vulture.ToHtmlStringRGBA() + ">Vulture</color> > "; }
                    else if (role.Value == RoleEnum.Phantom) { playerRole += "<color=#" + Patches.Colors.Phantom.ToHtmlStringRGBA() + ">Phantom</color> > "; }
                    else if (role.Value == RoleEnum.Grenadier) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Grenadier</color> > "; }
                    else if (role.Value == RoleEnum.Janitor) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Janitor</color> > "; }
                    else if (role.Value == RoleEnum.Miner) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Miner</color> > "; }
                    else if (role.Value == RoleEnum.Manipulator) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Manipulator</color> > "; }
                    else if (role.Value == RoleEnum.Morphling) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Morphling</color> > "; }
                    else if (role.Value == RoleEnum.Assassin) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Assassin</color> > "; }
                    else if (role.Value == RoleEnum.Swooper) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Swooper</color> > "; }
                    else if (role.Value == RoleEnum.Undertaker) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Undertaker</color> > "; }
                    else if (role.Value == RoleEnum.Haunter) { playerRole += "<color=#" + Patches.Colors.Haunter.ToHtmlStringRGBA() + ">Haunter</color> > "; }
                    else if (role.Value == RoleEnum.Helper) { playerRole += "<color=#" + Patches.Colors.Helper.ToHtmlStringRGBA() + ">Helper</color> > "; }
                    else if (role.Value == RoleEnum.Guardian) { playerRole += "<color=#" + Patches.Colors.Guardian.ToHtmlStringRGBA() + ">Guardian</color> > "; }
                    else if (role.Value == RoleEnum.Veteran) { playerRole += "<color=#" + Patches.Colors.Veteran.ToHtmlStringRGBA() + ">Veteran</color> > "; }
                    else if (role.Value == RoleEnum.Astral) { playerRole += "<color=#" + Patches.Colors.Astral.ToHtmlStringRGBA() + ">Astral</color> > "; }
                    else if (role.Value == RoleEnum.Lookout) { playerRole += "<color=#" + Patches.Colors.Lookout.ToHtmlStringRGBA() + ">Lookout</color> > "; }
                    else if (role.Value == RoleEnum.Amnesiac) { playerRole += "<color=#" + Patches.Colors.Amnesiac.ToHtmlStringRGBA() + ">Amnesiac</color> > "; }
                    else if (role.Value == RoleEnum.Attacker) { playerRole += "<color=#" + Patches.Colors.Attacker.ToHtmlStringRGBA() + ">Attacker</color> > "; }
                    else if (role.Value == RoleEnum.Terrorist) { playerRole += "<color=#" + Patches.Colors.Terrorist.ToHtmlStringRGBA() + ">Terrorist</color> > "; }
                    else if (role.Value == RoleEnum.Juggernaut) { playerRole += "<color=#" + Patches.Colors.Juggernaut.ToHtmlStringRGBA() + ">Juggernaut</color> > "; }
                    else if (role.Value == RoleEnum.Tracker) { playerRole += "<color=#" + Patches.Colors.Tracker.ToHtmlStringRGBA() + ">Tracker</color> > "; }
                    else if (role.Value == RoleEnum.Transporter) { playerRole += "<color=#" + Patches.Colors.Transporter.ToHtmlStringRGBA() + ">Transporter</color> > "; }
                    else if (role.Value == RoleEnum.Informant) { playerRole += "<color=#" + Patches.Colors.Informant.ToHtmlStringRGBA() + ">Informant</color> > "; }
                    else if (role.Value == RoleEnum.Traitor) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Traitor</color> > "; }
                    else if (role.Value == RoleEnum.Medium) { playerRole += "<color=#" + Patches.Colors.Medium.ToHtmlStringRGBA() + ">Medium</color> > "; }
                    else if (role.Value == RoleEnum.Trapper) { playerRole += "<color=#" + Patches.Colors.Trapper.ToHtmlStringRGBA() + ">Trapper</color> > "; }
                    else if (role.Value == RoleEnum.Survivor) { playerRole += "<color=#" + Patches.Colors.Survivor.ToHtmlStringRGBA() + ">Survivor</color> > "; }
                    else if (role.Value == RoleEnum.GuardianAngel) { playerRole += "<color=#" + Patches.Colors.GuardianAngel.ToHtmlStringRGBA() + ">Guardian Angel</color> > "; }
                    else if (role.Value == RoleEnum.Mystic) { playerRole += "<color=#" + Patches.Colors.Mystic.ToHtmlStringRGBA() + ">Mystic</color> > "; }
                    else if (role.Value == RoleEnum.Blackmailer) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Blackmailer</color> > "; }
                    else if (role.Value == RoleEnum.Plaguebearer) { playerRole += "<color=#" + Patches.Colors.Plaguebearer.ToHtmlStringRGBA() + ">Plaguebearer</color> > "; }
                    else if (role.Value == RoleEnum.Pestilence) { playerRole += "<color=#" + Patches.Colors.Pestilence.ToHtmlStringRGBA() + ">Pestilence</color> > "; }
                    else if (role.Value == RoleEnum.Maul) { playerRole += "<color=#" + Patches.Colors.Werewolf.ToHtmlStringRGBA() + ">Maul</color> > "; }
                    else if (role.Value == RoleEnum.Detective) { playerRole += "<color=#" + Patches.Colors.Detective.ToHtmlStringRGBA() + ">Detective</color> > "; }
                    else if (role.Value == RoleEnum.Escapist) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Escapist</color> > "; }
                    else if (role.Value == RoleEnum.Chameleon) { playerRole += "<color=#" + Patches.Colors.Chameleon.ToHtmlStringRGBA() + ">Chameleon</color> > "; }
                    else if (role.Value == RoleEnum.Imitator) { playerRole += "<color=#" + Patches.Colors.Imitator.ToHtmlStringRGBA() + ">Imitator</color> > "; }
                    else if (role.Value == RoleEnum.Bomber) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Bomber</color> > "; }
                    else if (role.Value == RoleEnum.Conjurer) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Conjurer</color> > "; }
                    else if (role.Value == RoleEnum.BountyHunter) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Bounty Hunter</color> > "; }
                    else if (role.Value == RoleEnum.Doomsayer) { playerRole += "<color=#" + Patches.Colors.Doomsayer.ToHtmlStringRGBA() + ">Doomsayer</color> > "; }
                    else if (role.Value == RoleEnum.Vampire) { playerRole += "<color=#" + Patches.Colors.Vampire.ToHtmlStringRGBA() + ">Vampire</color> > "; }
                    else if (role.Value == RoleEnum.SerialKiller) { playerRole += "<color=#" + Patches.Colors.SerialKiller.ToHtmlStringRGBA() + ">Serial Killer</color> > "; }
                    else if (role.Value == RoleEnum.Doppelganger) { playerRole += "<color=#" + Patches.Colors.Doppelganger.ToHtmlStringRGBA() + ">Doppelganger</color> > "; }
                    else if (role.Value == RoleEnum.VampireHunter) { playerRole += "<color=#" + Patches.Colors.VampireHunter.ToHtmlStringRGBA() + ">Vampire Hunter</color> > "; }
                    else if (role.Value == RoleEnum.Prosecutor) { playerRole += "<color=#" + Patches.Colors.Prosecutor.ToHtmlStringRGBA() + ">Prosecutor</color> > "; }
                    else if (role.Value == RoleEnum.Warlock) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Warlock</color> > "; }
                    else if (role.Value == RoleEnum.Oracle) { playerRole += "<color=#" + Patches.Colors.Oracle.ToHtmlStringRGBA() + ">Oracle</color> > "; }
                    else if (role.Value == RoleEnum.Venerer) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Venerer</color> > "; }
                    else if (role.Value == RoleEnum.Aurial) { playerRole += "<color=#" + Patches.Colors.Aurial.ToHtmlStringRGBA() + ">Aurial</color> > "; }
                    else if (role.Value == RoleEnum.Poisoner) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Poisoner</color> > "; }
                    else if (role.Value == RoleEnum.Shooter) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Shooter</color> > "; }
                    else if (role.Value == RoleEnum.Mafioso) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Mafioso</color> > "; }
                    else if (role.Value == RoleEnum.Reviver) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Reviver</color> > "; }
                    else if (role.Value == RoleEnum.Spirit) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Spirit</color> > "; }
                    else if (role.Value == RoleEnum.Blinder) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Blinder</color> > "; }
                    else if (role.Value == RoleEnum.Freezer) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Freezer</color> > "; }
                    else if (role.Value == RoleEnum.Witch) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Witch</color> > "; }
                    else if (role.Value == RoleEnum.Coven) { playerRole += "<color=#" + Patches.Colors.Coven.ToHtmlStringRGBA() + ">Coven</color> > "; }
                    else if (role.Value == RoleEnum.Ritualist) { playerRole += "<color=#" + Patches.Colors.Coven.ToHtmlStringRGBA() + ">Ritualist</color> > "; }
                    else if (role.Value == RoleEnum.HexMaster) { playerRole += "<color=#" + Patches.Colors.Coven.ToHtmlStringRGBA() + ">Hex Master</color> > "; }
                    else if (role.Value == RoleEnum.CovenLeader) { playerRole += "<color=#" + Patches.Colors.Coven.ToHtmlStringRGBA() + ">Coven Leader</color> > "; }
                    else if (role.Value == RoleEnum.Spiritualist) { playerRole += "<color=#" + Patches.Colors.Coven.ToHtmlStringRGBA() + ">Spiritualist</color> > "; }
                    else if (role.Value == RoleEnum.VoodooMaster) { playerRole += "<color=#" + Patches.Colors.Coven.ToHtmlStringRGBA() + ">Voodoo Master</color> > "; }
                    else if (role.Value == RoleEnum.PotionMaster) { playerRole += "<color=#" + Patches.Colors.Coven.ToHtmlStringRGBA() + ">Potion Master</color> > "; }
                    else if (role.Value == RoleEnum.Politician) { playerRole += "<color=#" + Patches.Colors.Politician.ToHtmlStringRGBA() + ">Politician</color> > "; }
                    else if (role.Value == RoleEnum.Warden) { playerRole += "<color=#" + Patches.Colors.Warden.ToHtmlStringRGBA() + ">Warden</color> > "; }
                    else if (role.Value == RoleEnum.Hypnotist) { playerRole += "<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Hypnotist</color> > "; }
                    else if (role.Value == RoleEnum.SoulCollector) { playerRole += "<color=#" + Patches.Colors.SoulCollector.ToHtmlStringRGBA() + ">Soul Collector</color> > "; }
                    else if (role.Value == RoleEnum.Werewolf) { playerRole += "<color=#" + Patches.Colors.Werewolf.ToHtmlStringRGBA() + ">Werewolf</color> > "; }
                    else if (role.Value == RoleEnum.Villager) { playerRole += "<color=#" + Patches.Colors.Villager.ToHtmlStringRGBA() + ">Villager</color> > "; }
                    else if (role.Value == RoleEnum.Sorcerer) { playerRole += "<color=#" + Patches.Colors.Sorcerer.ToHtmlStringRGBA() + ">Sorcerer</color> > "; }
                    else if (role.Value == RoleEnum.SoulCatcher) { playerRole += "<color=#" + Patches.Colors.SoulCatcher.ToHtmlStringRGBA() + ">Soul Catcher</color> > "; }
                    else if (role.Value == RoleEnum.BlackWolf) { playerRole += "<color=#" + Patches.Colors.BlackWolf.ToHtmlStringRGBA() + ">Black Wolf</color> > "; }
                    else if (role.Value == RoleEnum.WhiteWolf) { playerRole += "<color=#" + Patches.Colors.WhiteWolf.ToHtmlStringRGBA() + ">White Wolf</color> > "; }
                    else if (role.Value == RoleEnum.Guard) { playerRole += "<color=#" + Patches.Colors.Guard.ToHtmlStringRGBA() + ">Guard</color> > "; }
                    else if (role.Value == RoleEnum.TalkativeWolf) { playerRole += "<color=#" + Patches.Colors.TalkativeWolf.ToHtmlStringRGBA() + ">Talkative Wolf</color> > "; }
                    else if (role.Value == RoleEnum.Player) { playerRole += "<color=#" + Patches.Colors.Player.ToHtmlStringRGBA() + ">Player</color> > "; }
                    else if (role.Value == RoleEnum.Spectator) { playerRole += "<color=#" + Patches.Colors.Spectator.ToHtmlStringRGBA() + ">Spectator</color> > "; }
                    else if (role.Value == RoleEnum.Plumber) { playerRole += "<color=#" + Patches.Colors.Plumber.ToHtmlStringRGBA() + ">Plumber</color> > "; }
                    else if (role.Value == RoleEnum.Mercenary) { playerRole += "<color=#" + Patches.Colors.Mercenary.ToHtmlStringRGBA() + ">Mercenary</color> > "; }
                    else if (role.Value == RoleEnum.Cleric) { playerRole += "<color=#" + Patches.Colors.Cleric.ToHtmlStringRGBA() + ">Cleric</color> > "; }
                }
                if (!playerRole.IsNullOrWhiteSpace()) playerRole = playerRole.Remove(playerRole.Length - 3);
                
                if (playerControl.Is(Faction.Madmates))
                {
                    playerRole += " (<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Madmate</color>)";
                }
                if (playerControl.Is(ModifierEnum.Giant))
                {
                    playerRole += " (<color=#" + Patches.Colors.Giant.ToHtmlStringRGBA() + ">Giant</color>)";
                }
                if (playerControl.Is(ModifierEnum.Mini))
                {
                    playerRole += " (<color=#" + Patches.Colors.Mini.ToHtmlStringRGBA() + ">Mini</color>)";
                }
                if (playerControl.Is(ModifierEnum.ButtonBarry))
                {
                    playerRole += " (<color=#" + Patches.Colors.ButtonBarry.ToHtmlStringRGBA() + ">Button Barry</color>)";
                }
                if (playerControl.Is(ModifierEnum.Aftermath))
                {
                    playerRole += " (<color=#" + Patches.Colors.Aftermath.ToHtmlStringRGBA() + ">Aftermath</color>)";
                }
                if (playerControl.Is(ModifierEnum.Scientist))
                {
                    playerRole += " (<color=#" + Patches.Colors.Scientist.ToHtmlStringRGBA() + ">Scientist</color>)";
                }
                if (playerControl.Is(ModifierEnum.Bait))
                {
                    playerRole += " (<color=#" + Patches.Colors.Bait.ToHtmlStringRGBA() + ">Bait</color>)";
                }
                if (playerControl.Is(ModifierEnum.Superstar))
                {
                    playerRole += " (<color=#" + Patches.Colors.Superstar.ToHtmlStringRGBA() + ">Superstar</color>)";
                }
                if (playerControl.Is(ModifierEnum.Diseased))
                {
                    playerRole += " (<color=#" + Patches.Colors.Diseased.ToHtmlStringRGBA() + ">Diseased</color>)";
                }
                if (playerControl.Is(ModifierEnum.Flash))
                {
                    playerRole += " (<color=#" + Patches.Colors.Flash.ToHtmlStringRGBA() + ">Flash</color>)";
                }
                if (playerControl.Is(ModifierEnum.Tiebreaker))
                {
                    playerRole += " (<color=#" + Patches.Colors.Tiebreaker.ToHtmlStringRGBA() + ">Tiebreaker</color>)";
                }
                if (playerControl.Is(ModifierEnum.Taskmaster))
                {
                    playerRole += " (<color=#" + Patches.Colors.Taskmaster.ToHtmlStringRGBA() + ">Taskmaster</color>)";
                }
                if (playerControl.Is(ModifierEnum.Torch))
                {
                    playerRole += " (<color=#" + Patches.Colors.Torch.ToHtmlStringRGBA() + ">Torch</color>)";
                }
                if (playerControl.Is(ModifierEnum.Satellite))
                {
                    playerRole += " (<color=#" + Patches.Colors.Satellite.ToHtmlStringRGBA() + ">Satellite</color>)";
                }
                if (playerControl.Is(ModifierEnum.Lover))
                {
                    playerRole += " (<color=#" + Patches.Colors.Lovers.ToHtmlStringRGBA() + ">Lover</color>)";
                }
                if (playerControl.Is(ModifierEnum.Sleuth))
                {
                    playerRole += " (<color=#" + Patches.Colors.Sleuth.ToHtmlStringRGBA() + ">Sleuth</color>)";
                }
                if (playerControl.Is(ModifierEnum.Radar))
                {
                    playerRole += " (<color=#" + Patches.Colors.Radar.ToHtmlStringRGBA() + ">Radar</color>)";
                }
                if (playerControl.Is(ModifierEnum.Disperser))
                {
                    playerRole += " (<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Disperser</color>)";
                }
                if (playerControl.Is(ModifierEnum.Bloodlust))
                {
                    playerRole += " (<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Bloodlust</color>)";
                }
                if (playerControl.Is(ModifierEnum.Saboteur))
                {
                    playerRole += " (<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Saboteur</color>)";
                }
                if (playerControl.Is(ModifierEnum.Multitasker))
                {
                    playerRole += " (<color=#" + Patches.Colors.Multitasker.ToHtmlStringRGBA() + ">Multitasker</color>)";
                }
                if (playerControl.Is(ModifierEnum.DoubleShot))
                {
                    playerRole += " (<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Double Shot</color>)";
                }
                if (playerControl.Is(ModifierEnum.Underdog))
                {
                    playerRole += " (<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Underdog</color>)";
                }
                if (playerControl.Is(ModifierEnum.Lucky))
                {
                    playerRole += " (<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + ">Lucky</color>)";
                }
                if (playerControl.Is(ModifierEnum.Frosty))
                {
                    playerRole += " (<color=#" + Patches.Colors.Frosty.ToHtmlStringRGBA() + ">Frosty</color>)";
                }
                if (playerControl.Is(ModifierEnum.SixthSense))
                {
                    playerRole += " (<color=#" + Patches.Colors.SixthSense.ToHtmlStringRGBA() + ">Sixth Sense</color>)";
                }
                if (playerControl.Is(ModifierEnum.Shy))
                {
                    playerRole += " (<color=#" + Patches.Colors.Shy.ToHtmlStringRGBA() + ">Shy</color>)";
                }
                if (playerControl.Is(ModifierEnum.Spotter))
                {
                    playerRole += " (<color=#" + Patches.Colors.Spotter.ToHtmlStringRGBA() + ">Spotter</color>)";
                }
                if (playerControl.Is(ModifierEnum.Motionless))
                {
                    playerRole += " (<color=#" + Patches.Colors.Motionless.ToHtmlStringRGBA() + ">Motionless</color>)";
                }
                if (!playerControl.Data.IsDead)
                {
                    playerRole += " | (Alive)";
                }
                if (playerControl.Data.IsDead)
                {
                    var role = Role.GetRole(playerControl);
                    if (role.DeathReason == DeathReasons.Exiled && !hasDeathReason)
                    {
                        playerRole += " | (<color=#FF0000>Exiled</color>)";
                        hasDeathReason = true;
                    }
                    else if (role.DeathReason == DeathReasons.Misfired && !hasDeathReason)
                    {
                        playerRole += " | (<color=#FFFF00FF>Misfired</color>)";
                        hasDeathReason = true;
                    }
                    else if (role.DeathReason == DeathReasons.Suicided && !hasDeathReason)
                    {
                        playerRole += " | (<color=#FF0000>Suicided</color>)";
                        hasDeathReason = true;
                    }
                    foreach (var deadPlayer in Murder.KilledPlayers)
                    {
                        if (deadPlayer.PlayerId == playerControl.PlayerId)
                        {
                            var killerPlayer = Utils.PlayerById(deadPlayer.KillerId);
                            var killerName = killerPlayer.Data.PlayerName;
                            if (role.DeathReason == DeathReasons.Burned && !hasDeathReason)
                            {
                                playerRole += $" | (<color=#FF4D00FF>Burned</color> by {killerName})";
                                hasDeathReason = true;
                            }
                            else if (role.DeathReason == DeathReasons.Hexed && !hasDeathReason)
                            {
                                playerRole += $" | (<color=#bf5fff>Hexed</color> by {killerName})";
                                hasDeathReason = true;
                            }
                            else if (role.DeathReason == DeathReasons.Infected && !hasDeathReason)
                            {
                                playerRole += $" | (<color=#bf9000>Infected</color> by {killerName})";
                                hasDeathReason = true;
                            }
                            else if (role.DeathReason == DeathReasons.Executed && !hasDeathReason)
                            {
                                playerRole += $" | (<color=#FF0000>Executed</color> by {killerName})";
                                hasDeathReason = true;
                            }
                            else if (role.DeathReason == DeathReasons.Poisoned && !hasDeathReason)
                            {
                                playerRole += $" | (<color=#A020F0>Poisoned</color> by {killerName})";
                                hasDeathReason = true;
                            }
                            else if (role.DeathReason == DeathReasons.Cursed && !hasDeathReason)
                            {
                                playerRole += $" | (<color=#FF0000>Cursed</color> by {killerName})";
                                hasDeathReason = true;
                            }
                            else if (role.DeathReason == DeathReasons.Exploded && !hasDeathReason)
                            {
                                playerRole += $" | (<color=#FF0000>Exploded</color> by {killerName})";
                                hasDeathReason = true;
                            }
                            else if (role.DeathReason == DeathReasons.Guessed && !hasDeathReason)
                            {
                                playerRole += $" | (<color=#FF0000>Guessed</color> by {killerName})";
                                hasDeathReason = true;
                            }
                            else if (role.DeathReason == DeathReasons.Killed && !hasDeathReason)
                            {
                                playerRole += $" | (<color=#FF0000>Killed</color> by {killerName})";
                                hasDeathReason = true;
                            }
                        }
                    }
                }
                var player = Role.GetRole(playerControl);
                if ((playerControl.Is(RoleEnum.Phantom) || (playerControl.Is(Faction.Crewmates) && !playerControl.Is(RoleEnum.Spectator))) && CustomGameOptions.GameMode != GameMode.Werewolf)
                {
                    if ((player.TotalTasks - player.TasksLeft)/player.TotalTasks == 1) playerRole += " | Tasks: <color=#" + Color.green.ToHtmlStringRGBA() + $">{player.TotalTasks - player.TasksLeft}/{player.TotalTasks}</color>";
                    else playerRole += $" | Tasks: {player.TotalTasks - player.TasksLeft}/{player.TotalTasks}";
                }
                if (player.Kills > 0 && !playerControl.Is(Faction.Crewmates))
                {
                    playerRole += " |<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + $"> Kills: {player.Kills}</color>";
                }
                if (player.CorrectKills > 0)
                {
                    playerRole += " |<color=#" + Color.green.ToHtmlStringRGBA() + $"> Correct Kills: {player.CorrectKills}</color>";
                }
                if (player.IncorrectKills > 0)
                {
                    playerRole += " |<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + $"> Incorrect Kills: {player.IncorrectKills}</color>";
                }
                if (player.CorrectAssassinKills > 0)
                {
                    playerRole += " |<color=#" + Color.green.ToHtmlStringRGBA() + $"> Correct Guesses: {player.CorrectAssassinKills}</color>";
                }
                if (player.IncorrectAssassinKills > 0)
                {
                    playerRole += " |<color=#" + Patches.Colors.Impostor.ToHtmlStringRGBA() + $"> Incorrect Guesses: {player.IncorrectAssassinKills}</color>";
                }
                var playerName = "";
                foreach (var winner in EndGameResult.CachedWinners)
                {
                    if (winner.PlayerName == playerControl.Data.PlayerName) playerName += $"<color=#EFBF04>{playerControl.Data.PlayerName}</color>";
                }
                if (!CustomGameOptions.NeutralEvilWinEndsGame)
                {
                    if (playerControl.Is(RoleEnum.Doomsayer))
                    {
                        var doom = Role.GetRole<Doomsayer>(playerControl);
                        if (doom.WonByGuessing)
                        {
                            AdditionalTempData.otherWinners.Add(new AdditionalTempData.Winners() { PlayerName = doom.Player.Data.PlayerName, Role = RoleEnum.Doomsayer });
                            playerName += $"<color=#EFBF04>{playerControl.Data.PlayerName}</color>";
                        }
                    }
                    if (playerControl.Is(RoleEnum.Executioner))
                    {
                        var exe = Role.GetRole<Executioner>(playerControl);
                        if (exe.TargetVotedOut)
                        {
                            AdditionalTempData.otherWinners.Add(new AdditionalTempData.Winners() { PlayerName = exe.Player.Data.PlayerName, Role = RoleEnum.Executioner });
                            playerName += $"<color=#EFBF04>{playerControl.Data.PlayerName}</color>";
                        }
                    }
                    if (playerControl.Is(RoleEnum.Jester))
                    {
                        var jest = Role.GetRole<Jester>(playerControl);
                        if (jest.VotedOut)
                        {
                            AdditionalTempData.otherWinners.Add(new AdditionalTempData.Winners() { PlayerName = jest.Player.Data.PlayerName, Role = RoleEnum.Jester });
                            playerName += $"<color=#EFBF04>{playerControl.Data.PlayerName}</color>";
                        }
                    }
                    if (playerControl.Is(RoleEnum.Phantom))
                    {
                        var phan = Role.GetRole<Phantom>(playerControl);
                        if (phan.CompletedTasks)
                        {
                            AdditionalTempData.otherWinners.Add(new AdditionalTempData.Winners() { PlayerName = phan.Player.Data.PlayerName, Role = RoleEnum.Phantom });
                            playerName += $"<color=#EFBF04>{playerControl.Data.PlayerName}</color>";
                        }
                    }
                    if (playerControl.Is(RoleEnum.Vulture))
                    {
                        var vulture = Role.GetRole<Vulture>(playerControl);
                        if (vulture.VultureWins)
                        {
                            AdditionalTempData.otherWinners.Add(new AdditionalTempData.Winners() { PlayerName = vulture.Player.Data.PlayerName, Role = RoleEnum.Vulture });
                            playerName += $"<color=#EFBF04>{playerControl.Data.PlayerName}</color>";
                        }
                    }
                    if (playerControl.Is(RoleEnum.Troll))
                    {
                        var troll = Role.GetRole<Troll>(playerControl);
                        if (troll.TrolledVotedOut)
                        {
                            AdditionalTempData.otherWinners.Add(new AdditionalTempData.Winners() { PlayerName = troll.Player.Data.PlayerName, Role = RoleEnum.Troll });
                            playerName += $"<color=#EFBF04>{playerControl.Data.PlayerName}</color>";
                        }
                    }
                }
                if (playerName == "") playerName += playerControl.Data.PlayerName;

                AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo() { PlayerName = playerName, Role = playerRole });
            }
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public class EndGameManagerSetUpPatch {
        public static void Postfix(EndGameManager __instance)
        {
            if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek) return;

            GameObject bonusText = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            bonusText.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.8f, __instance.WinText.transform.position.z);
            bonusText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            TMPro.TMP_Text textRenderer = bonusText.GetComponent<TMPro.TMP_Text>();
            textRenderer.text = "";

            var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
            GameObject roleSummary = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            roleSummary.transform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -14f); 
            roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

            var roleSummaryText = new StringBuilder();
            roleSummaryText.AppendLine("End game summary:");
            foreach(var data in AdditionalTempData.playerRoles) {
                var role = string.Join(" ", data.Role);
                roleSummaryText.AppendLine($"{data.PlayerName} - {role}");
            }

            if (AdditionalTempData.otherWinners.Count != 0)
            {
                roleSummaryText.AppendLine("\n\n\nOther Winners:");
                foreach (var data in AdditionalTempData.otherWinners)
                {
                    if (data.Role == RoleEnum.Doomsayer) roleSummaryText.AppendLine("<color=#" + Patches.Colors.Doomsayer.ToHtmlStringRGBA() + $">{data.PlayerName}</color>");
                    else if (data.Role == RoleEnum.Executioner) roleSummaryText.AppendLine("<color=#" + Patches.Colors.Executioner.ToHtmlStringRGBA() + $">{data.PlayerName}</color>");
                    else if (data.Role == RoleEnum.Jester) roleSummaryText.AppendLine("<color=#" + Patches.Colors.Jester.ToHtmlStringRGBA() + $">{data.PlayerName}</color>");
                    else if (data.Role == RoleEnum.Troll) roleSummaryText.AppendLine("<color=#" + Patches.Colors.Troll.ToHtmlStringRGBA() + $">{data.PlayerName}</color>");
                    else if (data.Role == RoleEnum.Vulture) roleSummaryText.AppendLine("<color=#" + Patches.Colors.Vulture.ToHtmlStringRGBA() + $">{data.PlayerName}</color>");
                    else if (data.Role == RoleEnum.Phantom) roleSummaryText.AppendLine("<color=#" + Patches.Colors.Phantom.ToHtmlStringRGBA() + $">{data.PlayerName}</color>");
                }
            }

            TMPro.TMP_Text roleSummaryTextMesh = roleSummary.GetComponent<TMPro.TMP_Text>();
            roleSummaryTextMesh.alignment = TMPro.TextAlignmentOptions.TopLeft;
            roleSummaryTextMesh.color = Color.white;
            roleSummaryTextMesh.fontSizeMin = 1.5f;
            roleSummaryTextMesh.fontSizeMax = 1.5f;
            roleSummaryTextMesh.fontSize = 1.5f;
             
            var roleSummaryTextMeshRectTransform = roleSummaryTextMesh.GetComponent<RectTransform>();
            roleSummaryTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
            roleSummaryTextMesh.text = roleSummaryText.ToString();

            AdditionalTempData.clear();
        }
    }
}