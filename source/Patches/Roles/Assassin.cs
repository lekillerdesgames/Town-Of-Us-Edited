using System.Collections.Generic;
using System.Linq;
using TMPro;
using TownOfUsEdited.Patches;
using UnityEngine;
using TownOfUsEdited.NeutralRoles.ExecutionerMod;
using TownOfUsEdited.NeutralRoles.GuardianAngelMod;
using TownOfUsEdited.CrewmateRoles.VampireHunterMod;

namespace TownOfUsEdited.Roles
{
    public class Assassin : Role, IGuesser
    {
        public Dictionary<byte, (GameObject, GameObject, GameObject, TMP_Text)> Buttons { get; set; } = new();


        private Dictionary<string, Color> ColorMapping = new();

        public Dictionary<string, Color> SortedColorMapping;

        public Dictionary<byte, string> Guesses = new();


        public Assassin(PlayerControl player) : base(player)
        {
            Name = "Assassin";
            ImpostorText = () => "Kill Everyone During Meetings";
            TaskText = () => "Guess the roles of <color=#00FFFF>Crewmates</color> mid-meeting to kill them!\nFake Tasks:";
            Color = Patches.Colors.Impostor;
            RoleType = RoleEnum.Assassin;
            AddToRoleHistory(RoleType);
            Faction = Faction.Impostors;
            Alignment = Alignment.ImpostorKilling;

            RemainingKills = CustomGameOptions.AssassinKills;

            // Adds all the roles that have a non-zero chance of being in the game.
            if (CustomGameOptions.ChameleonOn > 0) ColorMapping.Add("Chameleon", Colors.Chameleon);
            if (CustomGameOptions.SheriffOn > 0 || (CustomGameOptions.VampireHunterOn > 0 && CustomGameOptions.GameMode == GameMode.Classic && CustomGameOptions.VampireOn > 0 && CustomGameOptions.BecomeOnVampDeaths == BecomeEnum.Sheriff)) ColorMapping.Add("Sheriff", Colors.Sheriff);
            if (CustomGameOptions.KnightOn > 0) ColorMapping.Add("Knight", Colors.Knight);
            if (CustomGameOptions.FighterOn > 0) ColorMapping.Add("Fighter", Colors.Fighter);
            if (CustomGameOptions.JailorOn > 0) ColorMapping.Add("Jailor", Colors.Jailor);
            if (CustomGameOptions.DeputyOn > 0) ColorMapping.Add("Deputy", Colors.Deputy);
            if (CustomGameOptions.EngineerOn > 0) ColorMapping.Add("Engineer", Colors.Engineer);
            if (CustomGameOptions.LighterOn > 0) ColorMapping.Add("Lighter", Colors.Lighter);
            if (CustomGameOptions.InformantOn > 0) ColorMapping.Add("Informant", Colors.Informant);
            if (CustomGameOptions.SwapperOn > 0) ColorMapping.Add("Swapper", Colors.Swapper);
            if (CustomGameOptions.SuperstarOn > 0) ColorMapping.Add("Superstar", Colors.Superstar);
            if (CustomGameOptions.AvengerOn > 0) ColorMapping.Add("Avenger", Colors.Avenger);
            if (CustomGameOptions.InvestigatorOn > 0) ColorMapping.Add("Investigator", Colors.Investigator);
            if (CustomGameOptions.MedicOn > 0) ColorMapping.Add("Medic", Colors.Medic);
            if (CustomGameOptions.AstralOn > 0) ColorMapping.Add("Astral", Colors.Astral);
            if (CustomGameOptions.LookoutOn > 0) ColorMapping.Add("Lookout", Colors.Lookout);
            if (CustomGameOptions.SeerOn > 0) ColorMapping.Add("Seer", Colors.Seer);
            if (CustomGameOptions.SpyOn > 0) ColorMapping.Add("Spy", Colors.Spy);
            if (CustomGameOptions.SnitchOn > 0) ColorMapping.Add("Snitch", Colors.Snitch);
            if (CustomGameOptions.AltruistOn > 0) ColorMapping.Add("Altruist", Colors.Altruist);
            if (CustomGameOptions.VigilanteOn > 0 || (CustomGameOptions.VampireHunterOn > 0 && CustomGameOptions.GameMode == GameMode.Classic && CustomGameOptions.VampireOn > 0 && CustomGameOptions.BecomeOnVampDeaths == BecomeEnum.Vigilante)) ColorMapping.Add("Vigilante", Colors.Vigilante);
            if (CustomGameOptions.VeteranOn > 0 || (CustomGameOptions.VampireHunterOn > 0 && CustomGameOptions.GameMode == GameMode.Classic && CustomGameOptions.VampireOn > 0 && CustomGameOptions.BecomeOnVampDeaths == BecomeEnum.Veteran)) ColorMapping.Add("Veteran", Colors.Veteran);
            if (CustomGameOptions.HunterOn > 0 || (CustomGameOptions.VampireHunterOn > 0 && CustomGameOptions.GameMode == GameMode.Classic && CustomGameOptions.VampireOn > 0 && CustomGameOptions.BecomeOnVampDeaths == BecomeEnum.Hunter)) ColorMapping.Add("Hunter", Colors.Hunter);
            if (CustomGameOptions.TrackerOn > 0) ColorMapping.Add("Tracker", Colors.Tracker);
            if (CustomGameOptions.TrapperOn > 0) ColorMapping.Add("Trapper", Colors.Trapper);
            if (CustomGameOptions.TransporterOn > 0) ColorMapping.Add("Transporter", Colors.Transporter);
            if (CustomGameOptions.MediumOn > 0) ColorMapping.Add("Medium", Colors.Medium);
            if (CustomGameOptions.MysticOn > 0) ColorMapping.Add("Mystic", Colors.Mystic);
            if (CustomGameOptions.CaptainOn > 0) ColorMapping.Add("Captain", Colors.Captain);
            if (CustomGameOptions.DoctorOn > 0) ColorMapping.Add("Doctor", Colors.Doctor);
            if (CustomGameOptions.TimeLordOn > 0) ColorMapping.Add("Time Lord", Colors.TimeLord);
            if (CustomGameOptions.CrusaderOn > 0) ColorMapping.Add("Crusader", Colors.Crusader);
            if (CustomGameOptions.BodyguardOn > 0) ColorMapping.Add("Bodyguard", Colors.Bodyguard);
            if (CustomGameOptions.ParanoïacOn > 0) ColorMapping.Add("Paranoïac", Colors.Paranoïac);
            if (CustomGameOptions.DetectiveOn > 0) ColorMapping.Add("Detective", Colors.Detective);
            if (CustomGameOptions.ImitatorOn > 0) ColorMapping.Add("Imitator", Colors.Imitator);
            if (CustomGameOptions.VampireHunterOn > 0 && CustomGameOptions.GameMode == GameMode.Classic && CustomGameOptions.VampireOn > 0) ColorMapping.Add("Vampire Hunter", Colors.VampireHunter);
            if (CustomGameOptions.ProsecutorOn > 0) ColorMapping.Add("Prosecutor", Colors.Prosecutor);
            if (CustomGameOptions.OracleOn > 0) ColorMapping.Add("Oracle", Colors.Oracle);
            if (CustomGameOptions.AurialOn > 0) ColorMapping.Add("Aurial", Colors.Aurial);
            if (CustomGameOptions.WardenOn > 0) ColorMapping.Add("Warden", Colors.Warden);
            if (CustomGameOptions.PoliticianOn > 0) ColorMapping.Add("Politician", Colors.Politician);
            if (CustomGameOptions.PoliticianOn > 0) ColorMapping.Add("Mayor", Colors.Mayor);

            // Add Neutral roles if enabled
            if (CustomGameOptions.AssassinGuessNeutralBenign)
            {
                if (CustomGameOptions.AmnesiacOn > 0 || (CustomGameOptions.ExecutionerOn > 0 && CustomGameOptions.OnTargetDead == OnTargetDead.Amnesiac) || (CustomGameOptions.GuardianAngelOn > 0 && CustomGameOptions.GaOnTargetDeath == BecomeOptions.Amnesiac)) ColorMapping.Add("Amnesiac", Colors.Amnesiac);
                if (CustomGameOptions.GuardianAngelOn > 0) ColorMapping.Add("Guardian Angel", Colors.GuardianAngel);
                if (CustomGameOptions.SurvivorOn > 0 || (CustomGameOptions.ExecutionerOn > 0 && CustomGameOptions.OnTargetDead == OnTargetDead.Survivor) || (CustomGameOptions.GuardianAngelOn > 0 && CustomGameOptions.GaOnTargetDeath == BecomeOptions.Survivor)) ColorMapping.Add("Survivor", Colors.Survivor);
            }
            if (CustomGameOptions.AssassinGuessNeutralEvil)
            {
                if (CustomGameOptions.DoomsayerOn > 0) ColorMapping.Add("Doomsayer", Colors.Doomsayer);
                if (CustomGameOptions.ExecutionerOn > 0) ColorMapping.Add("Executioner", Colors.Executioner);
                if (CustomGameOptions.TrollOn > 0) ColorMapping.Add("Troll", Colors.Troll);
                if (CustomGameOptions.VultureOn > 0) ColorMapping.Add("Vulture", Colors.Vulture);
                if (CustomGameOptions.SoulCollectorOn > 0) ColorMapping.Add("Soul Collector", Colors.SoulCollector);
                if (CustomGameOptions.JesterOn > 0 || (CustomGameOptions.ExecutionerOn > 0 && CustomGameOptions.OnTargetDead == OnTargetDead.Jester) || (CustomGameOptions.GuardianAngelOn > 0 && CustomGameOptions.GaOnTargetDeath == BecomeOptions.Jester)) ColorMapping.Add("Jester", Colors.Jester);
            }
            if (CustomGameOptions.AssassinGuessNeutralKilling)
            {
                if (CustomGameOptions.ArsonistOn > 0 && !PlayerControl.LocalPlayer.Is(RoleEnum.Arsonist)) ColorMapping.Add("Arsonist", Colors.Arsonist);
                if (CustomGameOptions.SerialKillerOn > 0 && !PlayerControl.LocalPlayer.Is(RoleEnum.SerialKiller)) ColorMapping.Add("Serial Killer", Colors.SerialKiller);
                if (CustomGameOptions.DoppelgangerOn > 0 && !PlayerControl.LocalPlayer.Is(RoleEnum.Doppelganger)) ColorMapping.Add("Doppelganger", Colors.Doppelganger);
                if (CustomGameOptions.MutantOn > 0 && !PlayerControl.LocalPlayer.Is(RoleEnum.Mutant)) ColorMapping.Add("Mutant", Colors.Mutant);
                if (CustomGameOptions.InfectiousOn > 0 && !PlayerControl.LocalPlayer.Is(RoleEnum.Infectious)) ColorMapping.Add("Infectious", Colors.Infectious);
                if (CustomGameOptions.GlitchOn > 0 && !PlayerControl.LocalPlayer.Is(RoleEnum.Glitch)) ColorMapping.Add("The Glitch", Colors.Glitch);
                if (CustomGameOptions.PlaguebearerOn > 0 && !PlayerControl.LocalPlayer.Is(RoleEnum.Plaguebearer)) ColorMapping.Add("Plaguebearer", Colors.Plaguebearer);
                if (CustomGameOptions.AttackerOn > 0 && !PlayerControl.LocalPlayer.Is(RoleEnum.Attacker)) ColorMapping.Add("Attacker", Colors.Attacker);
                if (CustomGameOptions.GameMode == GameMode.Classic && CustomGameOptions.VampireOn > 0 && !PlayerControl.LocalPlayer.Is(RoleEnum.Vampire)) ColorMapping.Add("Vampire", Colors.Vampire);
                if (CustomGameOptions.WerewolfOn > 0 && !PlayerControl.LocalPlayer.Is(RoleEnum.Maul)) ColorMapping.Add("Maul", Colors.Werewolf);
                if (!PlayerControl.LocalPlayer.Is(RoleEnum.Juggernaut) && CustomGameOptions.JuggernautOn > 0) ColorMapping.Add("Juggernaut", Colors.Juggernaut);
            }
            if (CustomGameOptions.AssassinGuessCoven)
            {
                ColorMapping.Add("Coven", Colors.Coven);
                if (CustomGameOptions.RitualistOn > 0) ColorMapping.Add("Ritualist", Colors.Coven);
                if (CustomGameOptions.HexMasterOn > 0) ColorMapping.Add("Hex Master", Colors.Coven);
                if (CustomGameOptions.CovenLeaderOn > 0) ColorMapping.Add("Coven Leader", Colors.Coven);
                if (CustomGameOptions.SpiritualistOn > 0) ColorMapping.Add("Spiritualist", Colors.Coven);
                if (CustomGameOptions.VoodooMasterOn > 0) ColorMapping.Add("Voodoo Master", Colors.Coven);
                if (CustomGameOptions.PotionMasterOn > 0) ColorMapping.Add("Potion Master", Colors.Coven);
            }

            // Add vanilla crewmate if enabled
            if (CustomGameOptions.AssassinCrewmateGuess)
            {
            ColorMapping.Add("Crewmate", Colors.Crewmate);
            }
            //Add modifiers if enabled
            if (CustomGameOptions.AssassinGuessModifiers && CustomGameOptions.BaitOn > 0) ColorMapping.Add("Bait", Colors.Bait);
            if (CustomGameOptions.AssassinGuessModifiers && CustomGameOptions.AftermathOn > 0) ColorMapping.Add("Aftermath", Colors.Aftermath);
            if (CustomGameOptions.AssassinGuessModifiers && CustomGameOptions.DiseasedOn > 0) ColorMapping.Add("Diseased", Colors.Diseased);
            if (CustomGameOptions.AssassinGuessModifiers && CustomGameOptions.FrostyOn > 0) ColorMapping.Add("Frosty", Colors.Frosty);
            if (CustomGameOptions.AssassinGuessModifiers && CustomGameOptions.MultitaskerOn > 0) ColorMapping.Add("Multitasker", Colors.Multitasker);
            if (CustomGameOptions.AssassinGuessModifiers && CustomGameOptions.TorchOn > 0) ColorMapping.Add("Torch", Colors.Torch);
            if (CustomGameOptions.AssassinGuessModifiers && CustomGameOptions.VengefulOn > 0) ColorMapping.Add("Vengeful", Colors.Vengeful);
            if (CustomGameOptions.AssassinGuessLovers && CustomGameOptions.LoversOn > 0) ColorMapping.Add("Lover", Colors.Lovers);

            // Sorts the list alphabetically. 
            SortedColorMapping = ColorMapping.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        public bool GuessedThisMeeting { get; set; } = false;

        public int RemainingKills { get; set; }

        public List<string> PossibleGuesses => SortedColorMapping.Keys.ToList();
    }
}
