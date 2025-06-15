using HarmonyLib;
using Hazel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUsEdited.CrewmateRoles.MedicMod;
using TownOfUsEdited.Extensions;
using TownOfUsEdited.Patches;
using TownOfUsEdited.Roles;
using TownOfUsEdited.Roles.Modifiers;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using Object = UnityEngine.Object;
using PerformKill = TownOfUsEdited.Modifiers.UnderdogMod.PerformKill;
using Random = UnityEngine.Random;
using AmongUs.GameOptions;
using TownOfUsEdited.CrewmateRoles.TrapperMod;
using TownOfUsEdited.ImpostorRoles.BomberMod;
using TownOfUsEdited.CrewmateRoles.VampireHunterMod;
using TownOfUsEdited.CrewmateRoles.ImitatorMod;
using Reactor.Networking;
using Reactor.Networking.Extensions;
using static TownOfUsEdited.Roles.Glitch;
using TownOfUsEdited.Patches.NeutralRoles;
using TownOfUsEdited.CrewmateRoles.DetectiveMod;
using TownOfUsEdited.NeutralRoles.SoulCollectorMod;
using TownOfUsEdited.CrewmateRoles.TimeLordMod;
using TownOfUsEdited.ImpostorRoles.TraitorMod;
using TownOfUsEdited.CrewmateRoles.ClericMod;

namespace TownOfUsEdited
{
    [HarmonyPatch]
    public static class Utils
    {
        internal static bool ShowDeadBodies = false;
        private static NetworkedPlayerInfo voteTarget = null;

        public static void Morph(PlayerControl player, PlayerControl MorphedPlayer, bool resetAnim = false, bool playAnim = false)
        {
            if (PlayerControl.LocalPlayer.IsHypnotised()) return;
            if (CamouflageUnCamouflage.IsCamoed) return;

            if (player.GetCustomOutfitType() != CustomPlayerOutfitType.Morph)
            {
                Coroutines.Start(MorphAnimation(player, MorphedPlayer, playAnim));
            }
        }

        public static IEnumerator MorphAnimation(PlayerControl player, PlayerControl MorphedPlayer, bool playAnim)
        {
            Color startingColor = Palette.PlayerColors[player.Data.DefaultOutfit.ColorId];
            Color endingColor = Palette.PlayerColors[MorphedPlayer.Data.DefaultOutfit.ColorId];
            Color startingShadowColor = Palette.ShadowColors[player.Data.DefaultOutfit.ColorId];
            Color endingShadowColor = Palette.ShadowColors[MorphedPlayer.Data.DefaultOutfit.ColorId];
            float duration = 2f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                if (player.Data.IsDead)
                {
                    Unmorph(player);
                    yield break;
                }
                if (!playAnim) continue;
                player.cosmetics.SetBodyCosmeticsVisible(false);
                Color color = Color.Lerp(startingColor, endingColor, t / duration);
                Color shadowColor = Color.Lerp(startingShadowColor, endingShadowColor, t / duration);
                player.myRend().material.SetColor(PlayerMaterial.BackColor, shadowColor);
                player.myRend().material.SetColor(PlayerMaterial.BodyColor, color);
                player.myRend().material.SetColor(PlayerMaterial.VisorColor, Palette.VisorColor);
                yield return null;
            }
            player.myRend().material.SetColor(PlayerMaterial.BackColor, startingShadowColor);
            player.myRend().material.SetColor(PlayerMaterial.BodyColor, startingColor);
            player.myRend().material.SetColor(PlayerMaterial.VisorColor, Palette.VisorColor);
            player.cosmetics.SetBodyCosmeticsVisible(true);
            player.SetOutfit(CustomPlayerOutfitType.Morph, MorphedPlayer.Data.DefaultOutfit);
        }

        public static void Swoop(PlayerControl player, bool playAnim = false)
        {
            if (PlayerControl.LocalPlayer.IsHypnotised()) return;
            Coroutines.Start(SwoopAnimation(player, playAnim));
        }

        public static IEnumerator SwoopAnimation(PlayerControl player, bool playAnim)
        {
            Color startingColor = Palette.PlayerColors[player.Data.DefaultOutfit.ColorId];
            Color endingColor = Color.clear;
            float duration = 2f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                if (player.Data.IsDead)
                {
                    Unmorph(player);
                    yield break;
                }
                if (!playAnim) continue;
                player.cosmetics.SetBodyCosmeticsVisible(false);
                if (player == PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data.IsDead || (PlayerControl.LocalPlayer.Data.IsImpostor() && player.Data.IsImpostor())) endingColor.a = 0.1f;
                Color color = Color.Lerp(startingColor, endingColor, t / duration);
                player.myRend().color = color;
                yield return null;
            }
            var finalColor = Color.clear;
            if (player == PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data.IsDead || (PlayerControl.LocalPlayer.Data.IsImpostor() && player.Data.IsImpostor())) finalColor.a = 0.1f;
            player.SetOutfit(CustomPlayerOutfitType.Swooper, new NetworkedPlayerInfo.PlayerOutfit()
            {
                ColorId = player.CurrentOutfit.ColorId,
                HatId = "",
                SkinId = "",
                VisorId = "",
                PlayerName = " ",
                PetId = ""
            });
            player.myRend().color = finalColor;
            player.nameText().color = Color.clear;
            player.cosmetics.colorBlindText.color = Color.clear;
            player.cosmetics.SetBodyCosmeticsVisible(true);
        }

        public static void Unmorph(PlayerControl player)
        {
            if (PlayerControl.LocalPlayer.IsHypnotised()) return;
            if (CamouflageUnCamouflage.IsCamoed)
            {
                player.SetOutfit(CustomPlayerOutfitType.Camouflage, new NetworkedPlayerInfo.PlayerOutfit()
                {
                    ColorId = player.GetDefaultOutfit().ColorId,
                    HatId = "",
                    SkinId = "",
                    VisorId = "",
                    PlayerName = " ",
                    PetId = ""
                });
                PlayerMaterial.SetColors(Color.grey, player.myRend());
                player.nameText().color = Color.clear;
                player.cosmetics.colorBlindText.color = Color.clear;
            }
            else
            {
                player.SetOutfit(CustomPlayerOutfitType.Default);
                if (!player.Is(ModifierEnum.Shy) || player.Data.IsDead || player.Data.Disconnected) return;
                player.SetHatAndVisorAlpha(1f);
                player.cosmetics.skin.layer.color = player.cosmetics.skin.layer.color.SetAlpha(1f);
                foreach (var rend in player.cosmetics.currentPet.renderers)
                    rend.color = rend.color.SetAlpha(1f);
                foreach (var shadow in player.cosmetics.currentPet.shadows)
                    shadow.color = shadow.color.SetAlpha(1f);
            }
        }

        public static void TurnMadmate(PlayerControl Madmate, bool SpawnedAs)
        {
            var madmate = Role.GetRole(Madmate);
            madmate.oldColor = madmate.Color;
            madmate.Color = Palette.ImpostorRed;
            madmate.Faction = Faction.Madmates;
            if (!madmate.Name.Contains("Mad"))
            {
            madmate.Name = "Mad " + madmate.Name;
            }
            if (Madmate.Is(RoleEnum.Vigilante))
            {
            madmate.TaskText = () => "Guess the roles of <color=#00FFFF>Crewmates</color> mid-meeting to kill them!";
            var vigiRole = Role.GetRole<Vigilante>(Madmate);
            var ColorMapping = new Dictionary<string, Color>();
            if (CustomGameOptions.AssassinCrewmateGuess) ColorMapping.Add("Crewmate", Colors.Crewmate);
            if (CustomGameOptions.ChameleonOn > 0) ColorMapping.Add("Chameleon", Colors.Chameleon);
            if (CustomGameOptions.SheriffOn > 0 || (CustomGameOptions.VampireHunterOn > 0 && CustomGameOptions.GameMode == GameMode.Classic && CustomGameOptions.VampireOn > 0 && CustomGameOptions.BecomeOnVampDeaths == BecomeEnum.Sheriff)) ColorMapping.Add("Sheriff", Colors.Sheriff);
            if (CustomGameOptions.KnightOn > 0) ColorMapping.Add("Knight", Colors.Knight);
            if (CustomGameOptions.FighterOn > 0) ColorMapping.Add("Fighter", Colors.Fighter);
            if (CustomGameOptions.JailorOn > 0) ColorMapping.Add("Jailor", Colors.Jailor);
            if (CustomGameOptions.DeputyOn > 0) ColorMapping.Add("Deputy", Colors.Deputy);
            if (CustomGameOptions.EngineerOn > 0) ColorMapping.Add("Engineer", Colors.Engineer);
            if (CustomGameOptions.InformantOn > 0) ColorMapping.Add("Informant", Colors.Informant);
            if (CustomGameOptions.SwapperOn > 0) ColorMapping.Add("Swapper", Colors.Swapper);
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
            if (CustomGameOptions.ParanoïacOn > 0) ColorMapping.Add("Paranoïac", Colors.Paranoïac);
            if (CustomGameOptions.DetectiveOn > 0) ColorMapping.Add("Detective", Colors.Detective);
            if (CustomGameOptions.ImitatorOn > 0) ColorMapping.Add("Imitator", Colors.Imitator);
            if (CustomGameOptions.VampireHunterOn > 0 && CustomGameOptions.GameMode == GameMode.Classic && CustomGameOptions.VampireOn > 0) ColorMapping.Add("Vampire Hunter", Colors.VampireHunter);
            if (CustomGameOptions.ProsecutorOn > 0) ColorMapping.Add("Prosecutor", Colors.Prosecutor);
            if (CustomGameOptions.OracleOn > 0) ColorMapping.Add("Oracle", Colors.Oracle);
            if (CustomGameOptions.AurialOn > 0) ColorMapping.Add("Aurial", Colors.Aurial);
            vigiRole.SortedColorMapping = ColorMapping.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }
            else if (Madmate.Is(RoleEnum.Crewmate))
            {
                madmate.TaskText = () => "Help the Impostors to win!";
            }
            else if (Madmate.Is(RoleEnum.Captain))
            {
                madmate.TaskText = () => "Zoom out to locate the <color=#00FFFF>Crewmates</color>";
            }
            else if (Madmate.Is(RoleEnum.Doctor))
            {
                madmate.TaskText = () => "Revive the Impostors";
            }
            else if (Madmate.Is(RoleEnum.Engineer))
            {
                madmate.TaskText = () => "Vent and Sabotage";
            }
            else if (Madmate.Is(RoleEnum.Fighter))
            {
                madmate.TaskText = () => "Kill all Crewmates";
            }
            else if (Madmate.Is(RoleEnum.Hunter))
            {
                madmate.TaskText = () => "Kill every non-Impostors killer";
            }
            else if (Madmate.Is(RoleEnum.Imitator))
            {
                madmate.TaskText = () => "Steal other Crewmates roles!";
            }
            else if (Madmate.Is(RoleEnum.Jailor))
            {
                madmate.TaskText = () => "Jail and execute other players!";
            }
            else if (Madmate.Is(RoleEnum.Knight))
            {
                madmate.TaskText = () => "Take down one ennemy";
            }
            else if (Madmate.Is(RoleEnum.Mayor))
            {
                madmate.TaskText = () => "Use your vote advantage to benefit the Impostors";
            }
            else if (Madmate.Is(RoleEnum.Medic))
            {
                madmate.TaskText = () => "Cast a shield on an Impostor to protect them";
            }
            else if (Madmate.Is(RoleEnum.Mystic))
            {
                madmate.TaskText = () => "Find out who the other killers are";
            }
            else if (Madmate.Is(RoleEnum.Oracle))
            {
                madmate.TaskText = () => "Reveal and accuse a fake Impostor";
            }
            else if (Madmate.Is(RoleEnum.Prosecutor))
            {
                madmate.TaskText = () => "Eliminate one ennemy";
            }
            else if (Madmate.Is(RoleEnum.Sheriff))
            {
                madmate.TaskText = () => "Help the Impostors to kill crewmates";
            }
            else if (Madmate.Is(RoleEnum.Snitch))
            {
                madmate.TaskText = () => "Complete your tasks to reveal alignments!";
            }
            else if (Madmate.Is(RoleEnum.Swapper))
            {
                madmate.TaskText = () => "Swap the votes to save Impostors";
            }
            else if (Madmate.Is(RoleEnum.VampireHunter))
            {
                madmate.TaskText = () => "Hunt down the Vampires and then, hunt down the Crewmates";
            }
            else if (Madmate.Is(RoleEnum.Bodyguard))
            {
                madmate.TaskText = () => "Protect the Impostors from incoming attacks";
            }
            else if (Madmate.Is(RoleEnum.Crusader))
            {
                madmate.TaskText = () => "Crusade an Impostor to make them invincible!";
            }
            var taskText= $"{madmate.TaskText()}"; // Using a var, else Stack Overflow lol
            madmate.TaskText = () => $"{taskText}\nFake Tasks:";
            if (SpawnedAs == false)
            {
                madmate.RegenTask();
                if (PlayerControl.LocalPlayer == Madmate)
                {
                    Coroutines.Start(FlashCoroutine(Palette.ImpostorRed, 0.5f));
                }
            }

            PlayerControl_Die.CheckEnd();
        }

        public static void TurnCrewmateTeam(PlayerControl Crewmate)
        {
            var crewmate = Role.GetRole(Crewmate);
            crewmate.Color = crewmate.oldColor;
            crewmate.Faction = Faction.Crewmates;
            if (crewmate.Name.Contains("Mad"))
            {
                crewmate.Name = crewmate.Name[4..];
            }
            if (Crewmate.Is(RoleEnum.Vigilante))
            {
            crewmate.TaskText = () => "Guess the roles of <color=#FF0000>Impostors</color> mid-meeting to kill them!";
            var vigiRole = Role.GetRole<Vigilante>(Crewmate);
            var ColorMapping = new Dictionary<string, Color>();
            if (CustomGameOptions.AssassinCrewmateGuess) ColorMapping.Remove("Crewmate");
            if (CustomGameOptions.ChameleonOn > 0) ColorMapping.Remove("Chameleon");
            if (CustomGameOptions.SheriffOn > 0 || (CustomGameOptions.VampireHunterOn > 0 && CustomGameOptions.GameMode == GameMode.Classic && CustomGameOptions.VampireOn > 0 && CustomGameOptions.BecomeOnVampDeaths == BecomeEnum.Sheriff)) ColorMapping.Remove("Sheriff");
            if (CustomGameOptions.KnightOn > 0) ColorMapping.Remove("Knight");
            if (CustomGameOptions.FighterOn > 0) ColorMapping.Remove("Fighter");
            if (CustomGameOptions.JailorOn > 0) ColorMapping.Remove("Jailor");
            if (CustomGameOptions.DeputyOn > 0) ColorMapping.Remove("Deputy");
            if (CustomGameOptions.EngineerOn > 0) ColorMapping.Remove("Engineer");
            if (CustomGameOptions.InformantOn > 0) ColorMapping.Remove("Informant");
            if (CustomGameOptions.SwapperOn > 0) ColorMapping.Remove("Swapper");
            if (CustomGameOptions.AvengerOn > 0) ColorMapping.Remove("Avenger");
            if (CustomGameOptions.InvestigatorOn > 0) ColorMapping.Remove("Investigator");
            if (CustomGameOptions.MedicOn > 0) ColorMapping.Remove("Medic");
            if (CustomGameOptions.AstralOn > 0) ColorMapping.Remove("Astral");
            if (CustomGameOptions.LookoutOn > 0) ColorMapping.Remove("Lookout");
            if (CustomGameOptions.SeerOn > 0) ColorMapping.Remove("Seer");
            if (CustomGameOptions.SpyOn > 0) ColorMapping.Remove("Spy");
            if (CustomGameOptions.SnitchOn > 0) ColorMapping.Remove("Snitch");
            if (CustomGameOptions.AltruistOn > 0) ColorMapping.Remove("Altruist");
            if (CustomGameOptions.VigilanteOn > 0 || (CustomGameOptions.VampireHunterOn > 0 && CustomGameOptions.GameMode == GameMode.Classic && CustomGameOptions.VampireOn > 0 && CustomGameOptions.BecomeOnVampDeaths == BecomeEnum.Vigilante)) ColorMapping.Remove("Vigilante");
            if (CustomGameOptions.VeteranOn > 0 || (CustomGameOptions.VampireHunterOn > 0 && CustomGameOptions.GameMode == GameMode.Classic && CustomGameOptions.VampireOn > 0 && CustomGameOptions.BecomeOnVampDeaths == BecomeEnum.Veteran)) ColorMapping.Remove("Veteran");
            if (CustomGameOptions.HunterOn > 0 || (CustomGameOptions.VampireHunterOn > 0 && CustomGameOptions.GameMode == GameMode.Classic && CustomGameOptions.VampireOn > 0 && CustomGameOptions.BecomeOnVampDeaths == BecomeEnum.Hunter)) ColorMapping.Remove("Hunter");
            if (CustomGameOptions.TrackerOn > 0) ColorMapping.Remove("Tracker");
            if (CustomGameOptions.TrapperOn > 0) ColorMapping.Remove("Trapper");
            if (CustomGameOptions.TransporterOn > 0) ColorMapping.Remove("Transporter");
            if (CustomGameOptions.MediumOn > 0) ColorMapping.Remove("Medium");
            if (CustomGameOptions.MysticOn > 0) ColorMapping.Remove("Mystic");
            if (CustomGameOptions.CaptainOn > 0) ColorMapping.Remove("Captain");
            if (CustomGameOptions.ParanoïacOn > 0) ColorMapping.Remove("Paranoïac");
            if (CustomGameOptions.DetectiveOn > 0) ColorMapping.Remove("Detective");
            if (CustomGameOptions.ImitatorOn > 0) ColorMapping.Remove("Imitator");
            if (CustomGameOptions.VampireHunterOn > 0 && CustomGameOptions.GameMode == GameMode.Classic && CustomGameOptions.VampireOn > 0) ColorMapping.Remove("Vampire Hunter");
            if (CustomGameOptions.ProsecutorOn > 0) ColorMapping.Remove("Prosecutor");
            if (CustomGameOptions.OracleOn > 0) ColorMapping.Remove("Oracle");
            if (CustomGameOptions.AurialOn > 0) ColorMapping.Remove("Aurial");
            vigiRole.SortedColorMapping = ColorMapping.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }
            else if (Crewmate.Is(RoleEnum.Crewmate))
            {
                crewmate.TaskText = () => "Do your tasks and eject the <color=#FF0000FF>Impostors</color>";
            }
            else if (Crewmate.Is(RoleEnum.Captain))
            {
                crewmate.TaskText = () => "Zoom out to catch the <color=#FF0000FF>Impostors</color>";
            }
            else if (Crewmate.Is(RoleEnum.Doctor))
            {
                crewmate.TaskText = () => "Revive the <color=#00FFFF>Crewmates</color>";
            }
            else if (Crewmate.Is(RoleEnum.Engineer))
            {
                crewmate.TaskText = () => "Vent around and fix sabotages";
            }
            else if (Crewmate.Is(RoleEnum.Fighter))
            {
                var oldRole = Role.GetRole(Crewmate);
                var killsList = (oldRole.CorrectKills, oldRole.IncorrectKills, oldRole.CorrectAssassinKills, oldRole.IncorrectAssassinKills);
                Role.RoleDictionary.Remove(Crewmate.PlayerId);
                if (PlayerControl.LocalPlayer == Crewmate)
                {
                    var role = new Sheriff(PlayerControl.LocalPlayer);
                    role.CorrectKills = killsList.CorrectKills;
                    role.IncorrectKills = killsList.IncorrectKills;
                    role.CorrectAssassinKills = killsList.CorrectAssassinKills;
                    role.IncorrectAssassinKills = killsList.IncorrectAssassinKills;
                    role.RegenTask();
                }
                else
                {
                    var role = new Sheriff(Crewmate);
                    role.CorrectKills = killsList.CorrectKills;
                    role.IncorrectKills = killsList.IncorrectKills;
                    role.CorrectAssassinKills = killsList.CorrectAssassinKills;
                    role.IncorrectAssassinKills = killsList.IncorrectAssassinKills;
                    //Not necessary to regen task here, I only do it so it looks good on mci lol
                    role.RegenTask();
                }
            }
            else if (Crewmate.Is(RoleEnum.Hunter))
            {
                crewmate.TaskText = () => "Stalk and kill <color=#FF0000>Impostors</color>, but not <color=#00FFFF>Crewmates</color>";
            }
            else if (Crewmate.Is(RoleEnum.Imitator))
            {
                crewmate.TaskText = () => "Use dead roles to benefit the crew";
            }
            else if (Crewmate.Is(RoleEnum.Jailor))
            {
                crewmate.TaskText = () => "Execute the <color=#FF0000FF>Impostors</color>";
            }
            else if (Crewmate.Is(RoleEnum.Mayor))
            {
                crewmate.TaskText = () => "Reveal yourself when the time is right";
            }
            else if (Crewmate.Is(RoleEnum.Medic))
            {
                crewmate.TaskText = () => "Protect a <color=#00FFFF>Crewmate</color> with a shield";
            }
            else if (Crewmate.Is(RoleEnum.Mystic))
            {
                crewmate.TaskText = () => "Know When and Where Kills Happen";
            }
            else if (Crewmate.Is(RoleEnum.Oracle))
            {
                crewmate.TaskText = () => "Get another player to confess on your passing";
            }
            else if (Crewmate.Is(RoleEnum.Prosecutor))
            {
                crewmate.TaskText = () => "Choose to exile anyone you want";
            }
            else if (Crewmate.Is(RoleEnum.Sheriff))
            {
                crewmate.TaskText = () => "Shoot the <color=#FF0000>Impostors</color> but don't kill <color=#00FFFF>Crewmates</color>";
            }
            else if (Crewmate.Is(RoleEnum.Snitch))
            {
                var role = Role.GetRole<Snitch>(Crewmate);
                crewmate.TaskText = () =>
                role.TasksDone
                    ? "Find the arrows pointing to the <color=#FF0000>Impostors</color>!"
                    : "Complete all your tasks to discover the <color=#FF0000>Impostors</color>!";
            }
            else if (Crewmate.Is(RoleEnum.Swapper))
            {
                crewmate.TaskText = () => "Swap two people's votes to save the Crew!";
            }
            else if (Crewmate.Is(RoleEnum.VampireHunter))
            {
                crewmate.TaskText = () => "Stake the <color=#262626FF>Vampires</color>";
            }
            else if (Crewmate.Is(RoleEnum.Bodyguard))
            {
                crewmate.TaskText = () => "Protect crewmates against <color=#FF0000FF>Impostor</color> attacks";
            }
            else if (Crewmate.Is(RoleEnum.Crusader))
            {
                crewmate.TaskText = () => "Trick the <color=#FF0000FF>Impostors</color>";
            }
            crewmate.RegenTask();
        }

        public static void GroupCamouflage()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                Camouflage(player);
            }
        }

        public static void Camouflage(PlayerControl player)
        {
            if (PlayerControl.LocalPlayer.IsHypnotised()) return;
            if (player.GetCustomOutfitType() != CustomPlayerOutfitType.Camouflage &&
                    player.GetCustomOutfitType() != CustomPlayerOutfitType.Swooper &&
                    player.GetCustomOutfitType() != CustomPlayerOutfitType.PlayerNameOnly)
            {
                player.SetOutfit(CustomPlayerOutfitType.Camouflage, new NetworkedPlayerInfo.PlayerOutfit()
                {
                    ColorId = player.GetDefaultOutfit().ColorId,
                    HatId = "",
                    SkinId = "",
                    VisorId = "",
                    PlayerName = " ",
                    PetId = ""
                });
                PlayerMaterial.SetColors(Color.grey, player.myRend());
                player.nameText().color = Color.clear;
                player.cosmetics.colorBlindText.color = Color.clear;
            }
        }

        public static void UnCamouflage()
        {
            if (PlayerControl.LocalPlayer.IsHypnotised()) return;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Is(RoleEnum.Swooper))
                {
                    var swooper = Role.GetRole<Swooper>(player);
                    if (swooper.IsSwooped) continue;
                }
                else if (player.Is(RoleEnum.Venerer))
                {
                    var venerer = Role.GetRole<Venerer>(player);
                    if (venerer.IsCamouflaged) continue;
                }
                else if (player.Is(RoleEnum.Morphling))
                {
                    var morphling = Role.GetRole<Morphling>(player);
                    if (morphling.Morphed) continue;
                }
                else if (player.Is(RoleEnum.Glitch))
                {
                    var glitch = Role.GetRole<Glitch>(player);
                    if (glitch.IsUsingMimic) continue;
                }
                else if (CamouflageUnCamouflage.IsCamoed) continue;
                Unmorph(player);
            }
        }

        public static void AddUnique<T>(this Il2CppSystem.Collections.Generic.List<T> self, T item)
            where T : IDisconnectHandler
        {
            if (!self.Contains(item)) self.Add(item);
        }

        public static bool IsLover(this PlayerControl player)
        {
            return player.Is(ModifierEnum.Lover);
        }

        public static bool Is(this PlayerControl player, RoleEnum roleType)
        {
            return Role.GetRole(player)?.RoleType == roleType;
        }

        public static bool Is(this PlayerControl player, ModifierEnum modifierType)
        {
            return Modifier.GetModifiers(player).Any(x => x.ModifierType == modifierType);
        }

        public static bool Is(this PlayerControl player, AbilityEnum abilityType)
        {
            return Ability.GetAbility(player)?.AbilityType == abilityType;
        }

        public static bool Is(this PlayerControl player, Faction faction)
        {
            return Role.GetRole(player)?.Faction == faction;
        }

        public static List<PlayerControl> GetCrewmates(List<PlayerControl> impostors)
        {
            return PlayerControl.AllPlayerControls.ToArray().Where(
                player => !impostors.Any(imp => imp.PlayerId == player.PlayerId)
            ).ToList();
        }

        public static List<PlayerControl> GetPlayers(
            List<NetworkedPlayerInfo> infected)
        {
            var players = new List<PlayerControl>();
            foreach (var Data in infected)
            {
                if (!Data.Disconnected)
                {
                    players.Add(Data.Object);
                }
            }

            return players;
        }

        public static IEnumerable<GameObject> GetAllChilds(this GameObject Go)
        {
            for (var i = 0; i < Go.transform.childCount; i++)
            {
                yield return Go.transform.GetChild(i).gameObject;
            }
        }

        public static RoleEnum GetRole(PlayerControl player)
        {
            if (player == null) return RoleEnum.None;
            if (player.Data == null) return RoleEnum.None;

            var role = Role.GetRole(player);
            if (role != null) return role.RoleType;

            return player.Data.IsImpostor() ? RoleEnum.Impostor : RoleEnum.Crewmate;
        }

        public static PlayerControl PlayerById(byte id)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == id)
                    return player;

            return null;
        }

        public static PlayerControl PlayerByData(NetworkedPlayerInfo data)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
                if (player.Data == data)
                    return player;

            return null;
        }

        public static PlayerControl PlayerByOutfit(NetworkedPlayerInfo.PlayerOutfit outfit)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
                if (player.Data.DefaultOutfit == outfit)
                    return player;

            return null;
        }

        public static bool CommsCamouflaged()
        {
            if (!CustomGameOptions.ColourblindComms) return false;
            if (PlayerControl.LocalPlayer.AreCommsAffected()) return true;
            return false;
        }

        public static bool IsCrewKiller(this PlayerControl player)
        {
            if (!CustomGameOptions.CrewKillersContinue) return false;
            if (player.Is(RoleEnum.Mayor) || player.Is(RoleEnum.Politician) || player.Is(RoleEnum.Swapper) ||
                player.Is(RoleEnum.Sheriff) || player.Is(RoleEnum.Bodyguard) ||
                player.Is(RoleEnum.Avenger) || player.Is(RoleEnum.Fighter) || player.Is(RoleEnum.Crusader) ||
                player.Is(RoleEnum.VampireHunter) || player.Is(RoleEnum.Knight) || player.Is(ModifierEnum.Vengeful) ||
                player.Is(RoleEnum.TimeLord)) return true;
            else if (player.Is(RoleEnum.Hunter))
            {
                var hunter = Role.GetRole<Hunter>(player);
                if (hunter.UsesLeft > 0 || (hunter.StalkedPlayer != null && !hunter.StalkedPlayer.Data.IsDead && !hunter.StalkedPlayer.Data.Disconnected && hunter.StalkedPlayer.Is(Faction.NeutralKilling)) ||
                hunter.CaughtPlayers.Count(player => !player.Data.IsDead && !player.Data.Disconnected && player.Is(Faction.NeutralKilling)) > 0) return true;
            }
            else if (player.Is(RoleEnum.Imitator))
            {
                if (PlayerControl.AllPlayerControls.ToArray().Count(x => x.Data.IsDead && !x.Data.Disconnected &&
                x.IsCrewKiller()) > 0) return true;
            }
            else if (player.Is(RoleEnum.Jailor))
            {
                var jailor = Role.GetRole<Jailor>(player);
                if (jailor.CanJail) return true;
            }
            else if (player.Is(RoleEnum.Deputy))
            {
                var dep = Role.GetRole<Deputy>(player);
                if (dep.Killer != null && !dep.Killer.Data.IsDead && !dep.Killer.Data.Disconnected) return true;
            }
            else if (player.Is(RoleEnum.Prosecutor))
            {
                var pros = Role.GetRole<Prosecutor>(player);
                if (!pros.Prosecuted) return true;
            }
            else if (player.Is(RoleEnum.Sorcerer))
            {
                var sorcerer = Role.GetRole<Sorcerer>(player);
                if (!sorcerer.UsedPoison) return true;
            }
            else if (player.Is(RoleEnum.Veteran))
            {
                var vet = Role.GetRole<Veteran>(player);
                if (vet.UsesLeft > 0 || vet.Enabled) return true;
            }
            else if (player.Is(RoleEnum.Vigilante))
            {
                var vigi = Role.GetRole<Vigilante>(player);
                if (vigi.RemainingKills > 0 && CustomGameOptions.VigilanteGuessNeutralKilling) return true;
            }
            return false;
        }

        public static bool IsExeTarget(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Executioner).Any(role =>
            {
                var exeTarget = ((Executioner)role).target;
                return exeTarget != null && player.PlayerId == exeTarget.PlayerId;
            });
        }

        public static bool IsBlessed(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Oracle).Any(role =>
            {
                var oracle = (Oracle)role;
                var blessedPlayer = oracle.Blessed;
                return blessedPlayer != null && player.PlayerId == blessedPlayer.PlayerId && !oracle.Player.Data.IsDead && !oracle.Player.Data.Disconnected;
            });
        }

        public static bool IsBarriered(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Cleric).Any(role =>
            {
                var cleric = (Cleric)role;
                var barrieredPlayer = cleric.Barriered;
                return barrieredPlayer != null && player.PlayerId == barrieredPlayer.PlayerId && !cleric.Player.Data.Disconnected;
            });
        }


        public static bool IsHacked(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Glitch).Any(role =>
            {
                var glitch = (Glitch)role;
                var hackedPlayer = glitch.Hacked;
                return hackedPlayer != null && player.PlayerId == hackedPlayer.PlayerId && !hackedPlayer.Data.IsDead && !glitch.Player.Data.IsDead;
            });
        }

        public static bool IsHypnotised(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Hypnotist).Any(role =>
            {
                var hypnotist = (Hypnotist)role;
                return hypnotist.HypnotisedPlayers.Contains(player.PlayerId) && hypnotist.HysteriaActive && !hypnotist.Player.Data.IsDead && !hypnotist.Player.Data.Disconnected && !player.Data.IsDead;
            });
        }

        public static bool IsShielded(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Medic).Any(role =>
            {
                var shieldedPlayer = ((Medic)role).ShieldedPlayer;
                return shieldedPlayer != null && player.PlayerId == shieldedPlayer.PlayerId;
            });
        }

        public static bool IsFortified(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Warden).Any(role =>
            {
                var warden = (Warden)role;
                var fortifiedPlayer = warden.Fortified;
                return fortifiedPlayer != null && player.PlayerId == fortifiedPlayer.PlayerId && !warden.Player.Data.IsDead && !warden.Player.Data.Disconnected;
            });
        }

        public static bool IsBlackmailed(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Blackmailer).Any(role =>
            {
                var bmer = (Blackmailer)role;
                return bmer.Blackmailed == player && !player.Data.IsDead && !player.Data.Disconnected;
            });
        }

        public static bool HasPotionShield(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.PotionMaster).Any(role =>
            {
                var pm = (PotionMaster)role;
                return !pm.Player.Data.Disconnected && !pm.Player.Data.IsDead && pm.UsingPotion && pm.Potion == "Shield" && pm.Player == player;
            });
        }

        public static List<Cleric> GetCleric(this PlayerControl player)
        {
            List<Cleric> clerics = new List<Cleric>();
            foreach (var role in Role.GetRoles(RoleEnum.Cleric))
            {
                var cleric = (Cleric)role;
                if (cleric.Barriered == player) clerics.Add(cleric);
            }
            return clerics;
        }

        public static List<Medic> GetMedic(this PlayerControl player)
        {
            List<Medic> medics = new List<Medic>();
            foreach (var role in Role.GetRoles(RoleEnum.Medic))
            {
                var medic = (Medic)role;
                if (medic.ShieldedPlayer == player) medics.Add(medic);
            }
            return medics;
        }

        public static List<Warden> GetWarden(this PlayerControl player)
        {
            List<Warden> wardens = new List<Warden>();
            foreach (var role in Role.GetRoles(RoleEnum.Warden))
            {
                var warden = (Warden)role;
                if (warden.Fortified == player) wardens.Add(warden);
            }
            return wardens;
        }

        public static Jailor GetJailor(this PlayerControl player)
        {
            foreach (var role in Role.GetRoles(RoleEnum.Jailor))
            {
                var jailor = (Jailor)role;
                if (jailor.JailedPlayer == player) return jailor;
            }
            return null;
        }


        public static List<Oracle> GetOracle(this PlayerControl player)
        {
            List<Oracle> oracles = new List<Oracle>();
            foreach (var role in Role.GetRoles(RoleEnum.Oracle))
            {
                var oracle = (Oracle)role;
                if (oracle.Blessed == player) oracles.Add(oracle);
            }
            return oracles;
        }

        public static GuardianAngel GetGA(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.GuardianAngel).FirstOrDefault(role =>
            {
                var protectedPlayer = ((GuardianAngel)role).target;
                return protectedPlayer != null && player.PlayerId == protectedPlayer.PlayerId;
            }) as GuardianAngel;
        }
        public static bool IsOnAlert(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Veteran).Any(role =>
            {
                var veteran = (Veteran)role;
                return veteran != null && veteran.OnAlert && player.PlayerId == veteran.Player.PlayerId;
            });
        }

        public static bool IsJailed(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Jailor).Any(role =>
            {
                var jailor = (Jailor)role;
                return jailor != null && jailor.JailedPlayer != null && player.PlayerId == jailor.JailedPlayer.PlayerId && !jailor.Player.Data.IsDead;
            });
        }

        public static bool IsManipulated(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Manipulator).Any(role =>
            {
                var manip = (Manipulator)role;
                return manip != null && manip.ManipulatedPlayer != null && player.PlayerId == manip.ManipulatedPlayer.PlayerId && !manip.Player.Data.IsDead && !manip.Player.Data.Disconnected;
            });
        }

        public static bool IsFullyManipulated(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Manipulator).Any(role =>
            {
                var manip = (Manipulator)role;
                return manip != null && manip.ManipulatedPlayer != null && player.PlayerId == manip.ManipulatedPlayer.PlayerId && !manip.Player.Data.IsDead && !manip.Player.Data.Disconnected && manip.UsingManipulation;
            });
        }

        public static bool IsCursed(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Witch).Any(role =>
            {
                var witch = (Witch)role;
                return witch.CursedList.Contains(player.PlayerId) && !witch.Player.Data.IsDead && !witch.Player.Data.Disconnected && !player.Data.IsDead && !player.Data.Disconnected;
            });
        }

        public static bool IsVoodoo(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.VoodooMaster).Any(role =>
            {
                var vm = (VoodooMaster)role;
                return vm.VoodooPlayer != null && vm.VoodooPlayer == player && !vm.Player.Data.IsDead && !vm.Player.Data.Disconnected && !player.Data.IsDead && !player.Data.Disconnected;
            });
        }

        public static bool IsHexed(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.HexMaster).Any(role =>
            {
                var hm = (HexMaster)role;
                return hm.Hexed.Contains(player.PlayerId) && !hm.Player.Data.IsDead && !hm.Player.Data.Disconnected && !player.Data.IsDead && !player.Data.Disconnected;
            });
        }

        public static bool IsWatched(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Lookout).Any(role =>
            {
                var lookout = (Lookout)role;
                return lookout != null && lookout.WatchedPlayer != null && player.PlayerId == lookout.WatchedPlayer.PlayerId && !lookout.Player.Data.IsDead && !lookout.Player.Data.Disconnected;
            });
        }

        public static bool IsControlled(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Spiritualist).Any(role =>
            {
                var spirit = (Spiritualist)role;
                return spirit != null && spirit.ControlledPlayer != null && player.PlayerId == spirit.ControlledPlayer.PlayerId && !spirit.Player.Data.IsDead;
            });
        }

        public static bool IsCrusaded(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Crusader).Any(role =>
            {
                var crusader = (Crusader)role;
                return crusader != null && crusader.CrusadedPlayer != null && player.PlayerId == crusader.CrusadedPlayer.PlayerId && !crusader.Player.Data.IsDead;
            });
        }

        public static bool IsGuarded(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Guard).Any(role =>
            {
                var guard = (Guard)role;
                return guard != null && guard.ProtectedPlayer != null && player.PlayerId == guard.ProtectedPlayer.PlayerId && !guard.Player.Data.IsDead;
            });
        }

        public static bool IsGuarded2(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Guardian).Any(role =>
            {
                var guardian = (Guardian)role;
                return guardian != null && guardian.ProtectedPlayer != null && player.PlayerId == guardian.ProtectedPlayer.PlayerId && guardian.Guarding;
            });
        }

        public static bool IsBlinded(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Blinder).Any(role =>
            {
                var blinder = (Blinder)role;
                return blinder != null && blinder.BlindedPlayer != null && player.PlayerId == blinder.BlindedPlayer.PlayerId && blinder.Blinding;
            });
        }

        public static bool IsFrozen(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Freezer).Any(role =>
            {
                var freezer = (Freezer)role;
                return freezer != null && freezer.FrozenPlayer != null && player.PlayerId == freezer.FrozenPlayer.PlayerId && freezer.Freezing;
            });
        }

        public static bool IsVesting(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Survivor).Any(role =>
            {
                var surv = (Survivor)role;
                return surv != null && surv.Vesting && player.PlayerId == surv.Player.PlayerId;
            });
        }

        public static bool IsProtected(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.GuardianAngel).Any(role =>
            {
                var gaTarget = ((GuardianAngel)role).target;
                var ga = (GuardianAngel)role;
                return gaTarget != null && ga.Protecting && player.PlayerId == gaTarget.PlayerId;
            });
        }

        public static bool IsInfected(this PlayerControl player)
        {
            return Role.GetRoles(RoleEnum.Plaguebearer).Any(role =>
            {
                var plaguebearer = (Plaguebearer)role;
                return plaguebearer != null && (plaguebearer.InfectedPlayers.Contains(player.PlayerId) || player.PlayerId == plaguebearer.Player.PlayerId);
            });
        }

        public static bool IsDouble(this DeadBody body)
        {
            return Role.GetRoles(RoleEnum.Doppelganger).Any(role =>
            {
                var doppel = (Doppelganger)role;
                return doppel != null && doppel.Bodies.Contains(body);
            });
        }

        public static bool Rewinding()
        {
            return Role.GetRoles(RoleEnum.TimeLord).Any(role =>
            {
                var tl = (TimeLord)role;
                return tl != null && tl.Rewinding;
            });
        }

        public static List<bool> Interact(PlayerControl player, PlayerControl target, bool toKill = false)
        {
            bool fullCooldownReset = false;
            bool gaReset = false;
            bool zeroSecReset = false;
            bool abilityUsed = false;
            bool bodyguardprotect = false;
            bool crusaderprotect = false;
            var checkHack = AbilityUsed(player, target);
            if (!checkHack) return new List<bool> { false, false, false, true, false };
            if (!player.Is(RoleEnum.Cleric) && (target.IsInfected() || player.IsInfected()))
            {
                foreach (var pb in Role.GetRoles(RoleEnum.Plaguebearer)) ((Plaguebearer)pb).RpcSpreadInfection(target, player);
            }
            if (target == ShowShield.FirstRoundShielded && toKill)
            {
                zeroSecReset = true;
            }
            else if (target.IsFortified())
            {
                zeroSecReset = true;
                Coroutines.Start(FlashCoroutine(Colors.Warden));
                foreach (var warden in target.GetWarden())
                {
                    Rpc(CustomRPC.Fortify, (byte)1, warden.Player.PlayerId);
                }
            }
            else if (target.Is(RoleEnum.Pestilence))
            {
                if (player.IsShielded())
                {
                    foreach (var medic in player.GetMedic())
                    {
                        Rpc(CustomRPC.AttemptSound, medic.Player.PlayerId, player.PlayerId);
                        StopKill.BreakShield(medic.Player.PlayerId, player.PlayerId, CustomGameOptions.ShieldBreaks);
                    }

                    if (CustomGameOptions.ShieldBreaks) fullCooldownReset = true;
                    else zeroSecReset = true;
                }
                else if (player.IsProtected() || player.IsBarriered() || target.IsGuarded2() || target.HasPotionShield())
                {
                    gaReset = true;
                    if (player.IsBarriered())
                    {
                        foreach (var cleric in player.GetCleric())
                        {
                            StopAttack.NotifyCleric(cleric.Player.PlayerId, false);
                        }
                    }
                }
                else RpcMurderPlayer(target, player);
            }
            else if (target.IsOnAlert())
            {
                if (player.Is(RoleEnum.Pestilence)) zeroSecReset = true;
                else if (player.IsShielded())
                {
                    foreach (var medic in player.GetMedic())
                    {
                        Rpc(CustomRPC.AttemptSound, medic.Player.PlayerId, player.PlayerId);
                        StopKill.BreakShield(medic.Player.PlayerId, player.PlayerId, CustomGameOptions.ShieldBreaks);
                    }

                    if (CustomGameOptions.ShieldBreaks) fullCooldownReset = true;
                    else zeroSecReset = true;
                }
                else if (player.IsProtected() || player.IsBarriered() || target.IsGuarded2() || target.HasPotionShield())
                {
                    gaReset = true;
                    if (player.IsBarriered())
                    {
                        foreach (var cleric in player.GetCleric())
                        {
                            StopAttack.NotifyCleric(cleric.Player.PlayerId, false);
                        }
                    }
                }
                else RpcMurderPlayer(target, player);
                if (toKill && CustomGameOptions.KilledOnAlert)
                {
                    if (target.IsShielded())
                    {
                        foreach (var medic in target.GetMedic())
                        {
                            Rpc(CustomRPC.AttemptSound, medic.Player.PlayerId, target.PlayerId);
                            StopKill.BreakShield(medic.Player.PlayerId, target.PlayerId, CustomGameOptions.ShieldBreaks);
                        }

                        if (CustomGameOptions.ShieldBreaks) fullCooldownReset = true;
                        else zeroSecReset = true;
                        Coroutines.Start(FlashCoroutine(new Color(0f, 0.5f, 0f, 1f)));
                    }
                    else if (target.IsProtected() || target.IsBarriered() || target.IsGuarded2() || target.HasPotionShield())
                    {
                        gaReset = true;
                        if (target.IsBarriered())
                        {
                            foreach (var cleric in target.GetCleric())
                            {
                                StopAttack.NotifyCleric(cleric.Player.PlayerId);
                            }
                        }
                    }
                    else
                    {
                        if (player.Is(RoleEnum.SerialKiller))
                        {
                            var sk = Role.GetRole<SerialKiller>(player);
                            sk.Cooldown = CustomGameOptions.SerialKillerKCD;                
                        }
                        else if (player.Is(RoleEnum.Doppelganger))
                        {
                            var doppel = Role.GetRole<Doppelganger>(player);
                            doppel.Cooldown = CustomGameOptions.DoppelKCD;                
                        }
                        else if (player.Is(RoleEnum.Glitch))
                        {
                            var glitch = Role.GetRole<Glitch>(player);
                            glitch.Cooldown = CustomGameOptions.GlitchKillCooldown;
                        }
                        else if (player.Is(RoleEnum.Infectious))
                        {
                            var infectious = Role.GetRole<Infectious>(player);
                            if (infectious.Infected.Contains(target.PlayerId))
                            {
                                infectious.Cooldown = CustomGameOptions.InfectiousInfectedCD;
                            }
                            else infectious.Cooldown = CustomGameOptions.InfectiousCD;
                        }
                        else if (player.Is(RoleEnum.Mutant))
                        {
                            var mutant = Role.GetRole<Mutant>(player);
                            if (mutant.IsTransformed)
                            {
                                mutant.Cooldown = CustomGameOptions.TransformKCD;
                            }
                            else
                            {
                                mutant.Cooldown = CustomGameOptions.MutantKCD;
                            }
                        }
                        else if (player.Is(RoleEnum.Juggernaut))
                        {
                            var jugg = Role.GetRole<Juggernaut>(player);
                            jugg.JuggKills += 1;
                            jugg.Cooldown = CustomGameOptions.JuggKCd - CustomGameOptions.ReducedKCdPerKill * jugg.JuggKills;
                        }
                        else if (player.Is(RoleEnum.Pestilence))
                        {
                            var pest = Role.GetRole<Pestilence>(player);
                            pest.Cooldown = CustomGameOptions.PestKillCd;
                        }
                        else if (player.Is(RoleEnum.Player))
                        {
                            var playerRole = Role.GetRole<Player>(player);
                            playerRole.Cooldown = CustomGameOptions.BattleRoyaleKillCD;
                        }
                        else if (player.Is(RoleEnum.Terrorist))
                        {
                            var terrorist = Role.GetRole<Terrorist>(player);
                            terrorist.Cooldown = CustomGameOptions.TerroristKillCD;
                        }
                        else if (player.Is(RoleEnum.Vampire))
                        {
                            var vamp = Role.GetRole<Vampire>(player);
                            vamp.Cooldown = CustomGameOptions.BiteCd;
                        }
                        else if (player.Is(RoleEnum.VampireHunter))
                        {
                            var vh = Role.GetRole<VampireHunter>(player);
                            vh.Cooldown = CustomGameOptions.StakeCd;
                        }
                        else if (player.Is(RoleEnum.Maul))
                        {
                            var ww = Role.GetRole<Maul>(player);
                            ww.Cooldown = CustomGameOptions.RampageKillCd;
                        }
                        else if (player.Is(RoleEnum.SoulCollector))
                        {
                            var sc = Role.GetRole<SoulCollector>(player);
                            sc.Cooldown = CustomGameOptions.ReapCd;
                        }
                        else if (player.Is(Faction.Impostors))
                        {
                            var impRole = Role.GetRole(player);
                            impRole.KillCooldown = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                        }
                        RpcMurderPlayer(player, target);
                        abilityUsed = true;
                        fullCooldownReset = true;
                        gaReset = false;
                        zeroSecReset = false;
                    }
                }
            }
            else if (target.IsShielded() && toKill)
            {
                foreach (var medic in target.GetMedic())
                {
                    Rpc(CustomRPC.AttemptSound, medic.Player.PlayerId, target.PlayerId);
                    StopKill.BreakShield(medic.Player.PlayerId, target.PlayerId, CustomGameOptions.ShieldBreaks);
                }

                if (CustomGameOptions.ShieldBreaks) fullCooldownReset = true;
                else zeroSecReset = true;
                Coroutines.Start(Utils.FlashCoroutine(new Color(0f, 0.5f, 0f, 1f)));
            }
            else if ((target.IsVesting() || target.IsProtected() || target.IsBarriered() || target.IsGuarded2() || target.HasPotionShield()) && toKill)
            {
                gaReset = true;
                if (target.IsBarriered())
                {
                    foreach (var cleric in target.GetCleric())
                    {
                        StopAttack.NotifyCleric(cleric.Player.PlayerId);
                    }
                }
            }
            else if (toKill)
            {
                if (player.Is(RoleEnum.SerialKiller))
                {
                    var sk = Role.GetRole<SerialKiller>(player);
                    sk.Cooldown = CustomGameOptions.SerialKillerKCD;                
                }
                else if (player.Is(RoleEnum.Doppelganger))
                {
                    var doppel = Role.GetRole<Doppelganger>(player);
                    doppel.Cooldown = CustomGameOptions.DoppelKCD;                
                }
                else if (player.Is(RoleEnum.Glitch))
                {
                    var glitch = Role.GetRole<Glitch>(player);
                    glitch.Cooldown = CustomGameOptions.GlitchKillCooldown;
                }
                else if (player.Is(RoleEnum.Infectious))
                {
                    var infectious = Role.GetRole<Infectious>(player);
                    if (infectious.Infected.Contains(target.PlayerId))
                    {
                        infectious.Cooldown = CustomGameOptions.InfectiousInfectedCD;
                    }
                    else infectious.Cooldown = CustomGameOptions.InfectiousCD;
                }
                else if (player.Is(RoleEnum.Mutant))
                {
                    var mutant = Role.GetRole<Mutant>(player);
                    if (mutant.IsTransformed)
                    {
                        mutant.Cooldown = CustomGameOptions.TransformKCD;
                    }
                    else
                    {
                        mutant.Cooldown = CustomGameOptions.MutantKCD;
                    }
                }
                else if (player.Is(RoleEnum.Juggernaut))
                {
                    var jugg = Role.GetRole<Juggernaut>(player);
                    jugg.JuggKills += 1;
                    jugg.Cooldown = CustomGameOptions.JuggKCd - CustomGameOptions.ReducedKCdPerKill * jugg.JuggKills;
                }
                else if (player.Is(RoleEnum.Pestilence))
                {
                    var pest = Role.GetRole<Pestilence>(player);
                    pest.Cooldown = CustomGameOptions.PestKillCd;
                }
                else if (player.Is(RoleEnum.Player))
                {
                    var playerRole = Role.GetRole<Player>(player);
                    playerRole.Cooldown = CustomGameOptions.BattleRoyaleKillCD;
                }
                else if (player.Is(RoleEnum.Terrorist))
                {
                    var terrorist = Role.GetRole<Terrorist>(player);
                    terrorist.Cooldown = CustomGameOptions.TerroristKillCD;
                }
                else if (player.Is(RoleEnum.Vampire))
                {
                    var vamp = Role.GetRole<Vampire>(player);
                    vamp.Cooldown = CustomGameOptions.BiteCd;
                }
                else if (player.Is(RoleEnum.VampireHunter))
                {
                    var vh = Role.GetRole<VampireHunter>(player);
                    vh.Cooldown = CustomGameOptions.StakeCd;
                }
                else if (player.Is(RoleEnum.Maul))
                {
                    var ww = Role.GetRole<Maul>(player);
                    ww.Cooldown = CustomGameOptions.RampageKillCd;
                }
                else if (player.Is(RoleEnum.SoulCollector))
                {
                    var sc = Role.GetRole<SoulCollector>(player);
                    sc.Cooldown = CustomGameOptions.ReapCd;
                }
                else if (player.Is(Faction.Impostors))
                {
                    var impRole = Role.GetRole(player);
                    impRole.KillCooldown = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                }
                var bodyguards = Role.AllRoles.Where(x => x.RoleType == RoleEnum.Bodyguard && !x.Player.Data.IsDead && !x.Player.Data.Disconnected).Cast<Bodyguard>();
                foreach (var role in bodyguards)
                {
                    if (!MeetingHud.Instance)
                    {
                    if (Vector2.Distance(player.GetTruePosition(),
                    role.Player.GetTruePosition()) < CustomGameOptions.BodyguardRadius && bodyguards != null
                    && target != role.Player && !player.Is(RoleEnum.Crusader) && !target.IsCrusaded() && (!role.Player.Is(ModifierEnum.Madmate) || !player.Is(Faction.Impostors))
                    && !player.Is(RoleEnum.Bodyguard))
                    {
                        Utils.Interact(role.Player, player, true);
                        Utils.Interact(player, role.Player, true);
                        role.DeathReason = DeathReasons.Suicided;
                        Utils.Rpc(CustomRPC.SetDeathReason, role.Player.PlayerId, (byte)DeathReasons.Suicided);
                        bodyguardprotect = true;
                        role.Protected = true;
                    }
                    }
                }
                if (target.IsCrusaded())
                {
                   var crusaders = Role.AllRoles.Where(x => x.RoleType == RoleEnum.Crusader && !x.Player.Data.IsDead && !x.Player.Data.Disconnected).Cast<Crusader>();
                   foreach (var role in crusaders)
                   {
                        if (role.CrusadedPlayer == target && (!role.Player.Is(ModifierEnum.Madmate) || !player.Is(Faction.Impostors)))
                        {
                            Utils.Interact(role.Player, player, true);
                            crusaderprotect = true;
                        }
                   } 
                }
                if (bodyguardprotect != true && crusaderprotect != true)
                {
                    RpcMurderPlayer(player, target);
                    abilityUsed = true;
                    fullCooldownReset = true;
                }
            }
            else
            {
                abilityUsed = true;
                fullCooldownReset = true;
            }

            var reset = new List<bool>();
            reset.Add(fullCooldownReset);
            reset.Add(gaReset);
            reset.Add(false);
            reset.Add(zeroSecReset);
            reset.Add(abilityUsed);
            return reset;
        }

        public static bool AbilityUsed(PlayerControl player, PlayerControl target = null)
        {
            if (player.IsHacked())
            {
                Coroutines.Start(AbilityCoroutine.Hack(player));
                return false;
            }
            else if (player.IsJailed()) return false;
            var targetId = byte.MaxValue;
            if (target != null) targetId = target.PlayerId;
            Rpc(CustomRPC.AbilityTrigger, player.PlayerId, targetId);
            return true;
        }

        public static void CreateDummy(PlayerControl requestingPlayer, PlayerControl srcPlayer = null)
        {
            var position = requestingPlayer.GetTruePosition();
            var playerControl = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
            var playerId = playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();

            // Code from Reactor, link: https://github.com/NuclearPowered/Reactor/blob/e27a79249ea706318f3c06f3dc56a5c42d65b1cf/Reactor.Debugger/Window/Tabs/GameTab.cs#L70
            var data = GameData.Instance.AddDummy(playerControl);
            var pc = srcPlayer;
            AmongUsClient.Instance.Spawn(data);
            AmongUsClient.Instance.Spawn(playerControl);
            playerControl.isDummy = true;

            playerControl.transform.position = new Vector2(position.x, position.y + 0.3636f);
            playerControl.GetComponent<DummyBehaviour>().enabled = true;
            playerControl.NetTransform.enabled = false;

            if (srcPlayer != null)
            {
                playerControl.SetName(pc.GetDefaultOutfit().PlayerName);

                var color = pc.GetDefaultOutfit().ColorId;
                playerControl.SetColor(color);
                playerControl.SetHat(pc.GetDefaultOutfit().HatId, color);
                playerControl.SetPet(pc.GetDefaultOutfit().PetId);
                playerControl.SetSkin(pc.GetDefaultOutfit().SkinId, color);
                playerControl.SetVisor(pc.GetDefaultOutfit().VisorId, color);
                playerControl.SetNamePlate(pc.GetDefaultOutfit().NamePlateId);

                if (pc.Is(ModifierEnum.Giant)) new Giant(playerControl);
                else if (pc.Is(ModifierEnum.Mini)) new Mini(playerControl);
            }
            else
            {
                playerControl.SetName($"Dummy {playerId}");

                var color = (int)UnityEngine.Random.Range(0, Palette.PlayerColors.Length);
                playerControl.SetColor(color);
                playerControl.SetHat(HatManager.Instance.allHats[UnityEngine.Random.Range(0, HatManager.Instance.allHats.Count)].ProdId, color);
                playerControl.SetPet(HatManager.Instance.allPets[UnityEngine.Random.Range(0, HatManager.Instance.allPets.Count)].ProdId);
                playerControl.SetSkin(HatManager.Instance.allSkins[UnityEngine.Random.Range(0, HatManager.Instance.allSkins.Count)].ProdId, color);
                playerControl.SetVisor(HatManager.Instance.allVisors[UnityEngine.Random.Range(0, HatManager.Instance.allVisors.Count)].ProdId, color);
                playerControl.SetNamePlate(HatManager.Instance.allNamePlates[UnityEngine.Random.Range(0, HatManager.Instance.allNamePlates.Count)].ProdId);
            }
            data.PlayerLevel = playerId;

            data.SetTasks(new Il2CppStructArray<byte>(0));

            if (requestingPlayer.Is(RoleEnum.Astral)) Role.GetRole<Astral>(requestingPlayer).AstralBody = playerControl;
        }

        public static Il2CppSystem.Collections.Generic.List<PlayerControl> GetClosestPlayers(Vector2 truePosition, float radius, bool includeDead)
        {
            Il2CppSystem.Collections.Generic.List<PlayerControl> playerControlList = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            float lightRadius = radius * ShipStatus.Instance.MaxLightRadius;
            Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int index = 0; index < allPlayers.Count; ++index)
            {
                NetworkedPlayerInfo playerInfo = allPlayers[index];
                if (!playerInfo.Disconnected && (!playerInfo.Object.Data.IsDead || includeDead))
                {
                    Vector2 vector2 = new Vector2(playerInfo.Object.GetTruePosition().x - truePosition.x, playerInfo.Object.GetTruePosition().y - truePosition.y);
                    float magnitude = ((Vector2)vector2).magnitude;
                    if (magnitude <= lightRadius)
                    {
                        PlayerControl playerControl = playerInfo.Object;
                        playerControlList.Add(playerControl);
                    }
                }
            }
            return playerControlList;
        }

        public static PlayerControl GetClosestPlayer(PlayerControl refPlayer, List<PlayerControl> AllPlayers, bool blockPhysics = false)
        {
            var num = double.MaxValue;
            var refPosition = refPlayer.GetTruePosition();
            PlayerControl result = null;
            foreach (var player in AllPlayers)
            {
                if (player.PlayerId == refPlayer.PlayerId || player.inVent || player.Data.IsDead) continue;
                var playerPosition = player.GetTruePosition();
                var distBetweenPlayers = Vector2.Distance(refPosition, playerPosition);
                var isClosest = distBetweenPlayers < num;
                if (!isClosest) continue;
                var vector = playerPosition - refPosition;
                if (!blockPhysics && PhysicsHelpers.AnyNonTriggersBetween(
                    refPosition, vector.normalized, vector.magnitude, Constants.ShipAndObjectsMask
                )) continue;
                num = distBetweenPlayers;
                result = player;
            }
            
            return result;
        }
        public static void SetTarget(
            ref PlayerControl closestPlayer,
            KillButton button,
            float maxDistance = float.NaN,
            List<PlayerControl> targets = null
        )
        {
            if (!button.isActiveAndEnabled) return;
            if (PlayerControl.LocalPlayer.IsJailed()) return;

            button.SetTarget(
                SetClosestPlayer(ref closestPlayer, maxDistance, targets)
            );
        }

        public static void SetTargetPlayer(
            ref PlayerControl closestPlayer,
            KillButton button,
            PlayerControl srcPlayer,
            float maxDistance = float.NaN,
            List<PlayerControl> targets = null
        )
        {
            if (!button.isActiveAndEnabled) return;
            if (PlayerControl.LocalPlayer.IsJailed()) return;

            button.SetTarget(
                SetClosestPlayerTarget(ref closestPlayer, srcPlayer, maxDistance, targets)
            );
        }

        public static PlayerControl SetClosestPlayer(
            ref PlayerControl closestPlayer,
            float maxDistance = float.NaN,
            List<PlayerControl> targets = null
        )
        {
            if (PlayerControl.LocalPlayer.inVent) return closestPlayer = null;
            if (float.IsNaN(maxDistance))
                maxDistance = LegacyGameOptions.KillDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance];
            var player = GetClosestPlayer(
                PlayerControl.LocalPlayer,
                targets ?? PlayerControl.AllPlayerControls.ToArray().ToList()
            );
            var closeEnough = player == null || (
                GetDistBetweenPlayers(PlayerControl.LocalPlayer, player) < maxDistance
            );
            return closestPlayer = closeEnough ? player : null;
        }

        public static PlayerControl SetClosestPlayerTarget(
            ref PlayerControl closestPlayer,
            PlayerControl target,
            float maxDistance = float.NaN,
            List<PlayerControl> targets = null
        )
        {
            if (PlayerControl.LocalPlayer.inVent) return closestPlayer = null;
            if (float.IsNaN(maxDistance))
                maxDistance = LegacyGameOptions.KillDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance];
            var player = GetClosestPlayer(
                target,
                targets ?? PlayerControl.AllPlayerControls.ToArray().ToList()
            );
            var closeEnough = player == null || (
                GetDistBetweenPlayers(target, player) < maxDistance
            );
            return closestPlayer = closeEnough ? player : null;
        }

        public static double GetDistBetweenPlayers(PlayerControl player, PlayerControl refplayer)
        {
            var truePosition = refplayer.GetTruePosition();
            var truePosition2 = player.GetTruePosition();
            return Vector2.Distance(truePosition, truePosition2);
        }

        public static void RpcMurderPlayer(PlayerControl killer, PlayerControl target)
        {
            MurderPlayer(killer, target, true);
            Rpc(CustomRPC.BypassKill, killer.PlayerId, target.PlayerId);
        }

        public static void RpcMultiMurderPlayer(PlayerControl killer, PlayerControl target)
        {
            MurderPlayer(killer, target, false);
            Rpc(CustomRPC.BypassMultiKill, killer.PlayerId, target.PlayerId);
            if (killer.Is(RoleEnum.Arsonist))
            {
                var targetRole = Role.GetRole(target);
                targetRole.DeathReason = DeathReasons.Burned;
                Rpc(CustomRPC.SetDeathReason, target.PlayerId, (byte)DeathReasons.Burned);
            }
            else if (killer.Is(RoleEnum.Bomber))
            {
                var targetRole = Role.GetRole(target);
                targetRole.DeathReason = DeathReasons.Exploded;
                Rpc(CustomRPC.SetDeathReason, target.PlayerId, (byte)DeathReasons.Exploded);
            }
            else if (killer.Is(RoleEnum.HexMaster))
            {
                var targetRole = Role.GetRole(target);
                targetRole.DeathReason = DeathReasons.Hexed;
                Rpc(CustomRPC.SetDeathReason, target.PlayerId, (byte)DeathReasons.Hexed);
            }
        }

        public static void MurderPlayer(PlayerControl killer, PlayerControl target, bool jumpToBody = false)
        {
            var data = target.Data;
            if (data != null && !data.IsDead)
            {
                if (target.isDummy && AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay)
                {
                    var astral = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.Is(RoleEnum.Astral));
                    if (astral != null)
                    {
                        var astralRole = Role.GetRole<Astral>(astral);
                        if (astralRole.AstralBody != null && astralRole.AstralBody == target)
                        {
                            var position = target.GetTruePosition();
                            if (PlayerControl.LocalPlayer == astralRole.Player)
                            {
                                var position2 = PlayerControl.LocalPlayer.transform.position;
                                TimeLordPatches.Positions.Add((Vector2.zero, Time.time, "Teleport", position2, 0, null));
                            }
                            astralRole.Player.NetTransform.SnapTo(new Vector2(position.x, position.y + 0.3636f));
                            astralRole.TimeRemaining = 0f;
                            astralRole.TurnBack(astralRole.Player);
                            MurderPlayer(killer, astral, jumpToBody);
                        }
                    }
                    return;
                }

                killer.isKilling = true;
                target.isKilling = true;

                Animations.StopAllAnimations(killer);
                Animations.StopAllAnimations(target);

                if (ShowShield.DiedFirst == "") ShowShield.DiedFirst = target.GetDefaultOutfit().PlayerName;

                if (target.GetAppearance().SizeFactor == new Vector3(0.4f, 0.4f, 1f))
                {
                    target.transform.localPosition += new Vector3(0f, SizePatch.Radius * 0.75f, 0f);
                }
                else if (killer.GetAppearance().SizeFactor == new Vector3(0.4f, 0.4f, 1f))
                {
                    target.transform.localPosition -= new Vector3(0f, SizePatch.Radius * 0.75f, 0f);
                }

                if (killer.Is(RoleEnum.SoulCollector) && killer != target)
                {
                    var sc = Role.GetRole<SoulCollector>(killer);
                    var bodyPos = target.transform.position;
                    bodyPos.y -= 0.3f;
                    bodyPos.x -= 0.11f;
                    sc.Souls.Add(SoulExtensions.CreateSoul(bodyPos, target));
                }

                if (killer.Is(ModifierEnum.Shy) && killer.GetCustomOutfitType() == CustomPlayerOutfitType.Default)
                {
                    var shy = Modifier.GetModifier<Shy>(killer);
                    shy.Opacity = 1f;
                    Modifiers.ShyMod.HudManagerUpdate.SetVisiblity(killer, shy.Opacity);
                    shy.Moving = true;
                }

                if (target.IsProtected())
                {
                    var ga = target.GetGA();
                    ga.UnProtect();
                }

                // I do both cause desync sometimes
                if (PlayerControl.LocalPlayer.Is(RoleEnum.Deputy))
                {
                    var deputy = Role.GetRole<Deputy>(PlayerControl.LocalPlayer);
                    if (target == deputy.Camping)
                    {
                        deputy.Killer = killer;
                        Rpc(CustomRPC.Camp, PlayerControl.LocalPlayer.PlayerId, (byte)1, deputy.Killer.PlayerId);
                        deputy.Camping = null;
                        Coroutines.Start(FlashCoroutine(Color.red));
                    }
                }
                foreach (var role in Role.GetRoles(RoleEnum.Deputy))
                {
                    var dep = (Deputy)role;
                    if (target == dep.Camping)
                    {
                        dep.Killer = killer;
                        dep.Camping = null;
                    }
                }

                if (PlayerControl.LocalPlayer == target)
                {
                    try
                    {
                        PlayerMenu.singleton.Menu.Close();
                    }
                    catch { }
                    try
                    {
                        TraitorMenu.singleton.Menu.Close();
                    }
                    catch { }
                }

                if (PlayerControl.LocalPlayer == target && PlayerControl.LocalPlayer.IsHypnotised())
                {
                    var unhypno = false;
                    foreach (var role in Role.GetRoles(RoleEnum.Hypnotist))
                    {
                        var hypno = (Hypnotist)role;
                        hypno.HypnotisedPlayers.Remove(target.PlayerId);
                        if (!unhypno)
                        {
                            hypno.UnHysteria();
                            unhypno = true;
                        }
                    }
                }
                if (target.Is(RoleEnum.Hypnotist))
                {
                    var hypno = Role.GetRole<Hypnotist>(target);
                    hypno.HysteriaActive = false;
                    if (!PlayerControl.LocalPlayer.IsHypnotised()) hypno.UnHysteria();
                }
                if (target.IsHypnotised())
                {
                    foreach (var role in Role.GetRoles(RoleEnum.Hypnotist))
                    {
                        var hypno = (Hypnotist)role;
                        hypno.HypnotisedPlayers.Remove(target.PlayerId);
                    }
                }

                int currentOutfitType = 0;
                if (PlayerControl.LocalPlayer == target)
                {
                    target.SetOutfit(CustomPlayerOutfitType.Default);
                    currentOutfitType = (int)killer.CurrentOutfitType;
                    killer.CurrentOutfitType = PlayerOutfitType.Default;
                }

                if (CustomGameOptions.GameMode == GameMode.BattleRoyale || CustomGameOptions.GameMode == GameMode.Chaos
                || PlayerControl.LocalPlayer.Is(RoleEnum.Mystic) || (PlayerControl.LocalPlayer.Data.IsDead &&
                Utils.ShowDeadBodies == true && CustomGameOptions.DeadSeeRoles) || target.Is(ModifierEnum.Superstar))
                {
                    var deadplayers = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Data.IsDead || x.Data.Disconnected).ToList();
                    var popup = GameManagerCreator.Instance.HideAndSeekManagerPrefab.DeathPopupPrefab;
                    Object.Instantiate(popup, HudManager.Instance.transform.parent).Show(target, deadplayers.Count);
                }

                if (killer == PlayerControl.LocalPlayer)
                    SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.KillSfx, false, 0.8f);

                if (!killer.Is(Faction.Crewmates) && killer != target
                    && GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal) Role.GetRole(killer).Kills += 1;

                if (killer.Is(RoleEnum.Sheriff) && !killer.Is(Faction.Madmates))
                {
                    var sheriff = Role.GetRole<Sheriff>(killer);
                    if (target.Is(Faction.Impostors) ||
                        target.Is(Faction.NeutralKilling) && CustomGameOptions.SheriffKillsNK ||
                        target.Is(Faction.Madmates) && CustomGameOptions.SheriffKillsMad ||
                        target.Is(Faction.NeutralEvil) && CustomGameOptions.SheriffKillsNE ||
                        target.Is(Faction.Coven) && CustomGameOptions.SheriffKillsCoven ||
                        target.Is(Faction.NeutralBenign) && CustomGameOptions.SheriffKillsNB) sheriff.CorrectKills += 1;
                    else if (killer == target) sheriff.IncorrectKills += 1;
                }

                if (killer.Is(RoleEnum.Knight) && !killer.Is(Faction.Madmates))
                {
                    var knight = Role.GetRole<Knight>(killer);
                    if (!target.Is(Faction.Crewmates)) knight.CorrectKills += 1;
                    else knight.IncorrectKills += 1;
                }

                if (killer.Is(RoleEnum.VampireHunter) && !killer.Is(Faction.Madmates))
                {
                    var vh = Role.GetRole<VampireHunter>(killer);
                    if (killer != target) vh.CorrectKills += 1;
                }

                if (killer.Is(RoleEnum.Veteran) && !killer.Is(Faction.Madmates))
                {
                    var veteran = Role.GetRole<Veteran>(killer);
                    if (!target.Is(Faction.Crewmates)) veteran.CorrectKills += 1;
                    else if (killer != target) veteran.IncorrectKills += 1;
                }

                if (killer.Is(RoleEnum.Hunter) && !killer.Is(Faction.Madmates))
                {
                    var hunter = Role.GetRole<Hunter>(killer);
                    if (target.Is(RoleEnum.Doomsayer) || target.Is(Faction.Impostors) || target.Is(Faction.NeutralKilling))
                    {
                        hunter.CorrectKills += 1;
                    }
                    else
                    {
                        hunter.IncorrectKills += 1;
                    }
                }

                if (killer.Is(RoleEnum.Fighter) && !killer.Is(Faction.Madmates))
                {
                    var fighter = Role.GetRole<Fighter>(killer);
                    if (target.Is(RoleEnum.Doomsayer) || target.Is(Faction.Impostors) || target.Is(Faction.NeutralKilling))
                    {
                        fighter.CorrectKills += 1;
                    }
                    else
                    {
                        fighter.IncorrectKills += 1;
                    }
                }

                if (killer.Is(RoleEnum.Avenger) && !killer.Is(Faction.Madmates))
                {
                    var avenger = Role.GetRole<Avenger>(killer);
                    if (target.Is(RoleEnum.Doomsayer) || target.Is(Faction.Impostors) || target.Is(Faction.NeutralKilling))
                    {
                        avenger.CorrectKills += 1;
                    }
                    else
                    {
                        avenger.IncorrectKills += 1;
                    }
                }

                if (target.Is(ModifierEnum.Superstar))
                {
                    Coroutines.Start(Utils.FlashCoroutine(Colors.Superstar, 0.5f));
                }

                if (target.IsManipulated())
                {
                    var manipulator = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.Is(RoleEnum.Manipulator) && Role.GetRole<Manipulator>(x).ManipulatedPlayer == target);
                    var manipRole = Role.GetRole<Manipulator>(manipulator);
                    manipRole.StopManipulation();
                    Utils.Rpc(CustomRPC.SetManipulateOff, manipulator.PlayerId);
                }

                if (target.IsWatched())
                {
                    var lookout = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.Is(RoleEnum.Lookout) && Role.GetRole<Lookout>(x).WatchedPlayer == target);
                    var lookoutRole = Role.GetRole<Lookout>(lookout);
                    lookoutRole.StopWatching();
                    if (CustomGameOptions.WatchedKnows) Utils.Rpc(CustomRPC.StopWatch, lookout.PlayerId);
                }

                if (target.Is(RoleEnum.Manipulator))
                {
                    var manipRole = Role.GetRole<Manipulator>(target);
                    manipRole.StopManipulation();
                    Utils.Rpc(CustomRPC.SetManipulateOff, target.PlayerId);
                }

                if (target.Is(RoleEnum.Lookout))
                {
                    var lookoutRole = Role.GetRole<Lookout>(target);
                    lookoutRole.StopWatching();
                    if (CustomGameOptions.WatchedKnows) Utils.Rpc(CustomRPC.StopWatch, target.PlayerId);
                }

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Detective))
                {
                    var detective = Role.GetRole<Detective>(PlayerControl.LocalPlayer);
                    if (PlayerControl.LocalPlayer != target && !PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        var bodyPos = target.transform.position;
                        bodyPos.y -= 0.3f;
                        bodyPos.x -= 0.11f;
                        detective.CrimeScenes.Add(CrimeSceneExtensions.CreateCrimeScene(bodyPos, target));
                    }
                }

                if (PlayerControl.LocalPlayer == target && PlayerControl.LocalPlayer.Is(RoleEnum.Aurial))
                {
                    var aurial = Role.GetRole<Aurial>(PlayerControl.LocalPlayer);
                    aurial.SenseArrows.Values.DestroyAll();
                    aurial.SenseArrows.Clear();
                }

                GameManager.Instance.LogicFlow.CheckEndCriteria();

                if (target.AmOwner)
                {
                    try
                    {
                        if (Minigame.Instance)
                        {
                            Minigame.Instance.Close();
                            Minigame.Instance.Close();
                        }

                        if (MapBehaviour.Instance)
                        {
                            MapBehaviour.Instance.Close();
                            MapBehaviour.Instance.Close();
                        }
                    }
                    catch
                    {
                    }

                    DestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(killer.Data, data);
                    DestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
                    target.nameText().GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
                    target.RpcSetScanner(false);

                    if (CustomGameOptions.GameMode == GameMode.BattleRoyale) Utils.ShowDeadBodies = true;
                }

                if (killer.AmOwner && jumpToBody)
                {
                    var position = PlayerControl.LocalPlayer.transform.position;
                    TimeLordPatches.Positions.Add((Vector2.zero, Time.time, "Teleport", position, 0, null));
                }

                if (jumpToBody)
                {
                    killer.MyPhysics.StartCoroutine(killer.KillAnimations.Random().CoPerformKill(killer, target));
                }
                else target.MyPhysics.StartCoroutine(target.KillAnimations.Random().CoPerformKill(target, target));

                target.gameObject.layer = LayerMask.NameToLayer("Ghost");
                target.Visible = false;

                if (PlayerControl.LocalPlayer == target) killer.CurrentOutfitType = (PlayerOutfitType)currentOutfitType;

                if (target.Is(ModifierEnum.Frosty))
                {
                    var frosty = Modifier.GetModifier<Frosty>(target);
                    frosty.Chilled = killer;
                    frosty.LastChilled = DateTime.UtcNow;
                    frosty.IsChilled = true;
                }

                var deadBody = new DeadPlayer
                {
                    PlayerId = target.PlayerId,
                    KillerId = killer.PlayerId,
                    KillTime = DateTime.UtcNow
                };

                Murder.KilledPlayers.Add(deadBody);

                if (killer.Is(RoleEnum.SoulCollector) && killer != target)
                {
                    foreach (var body in GameObject.FindObjectsOfType<DeadBody>())
                    {
                        if (body.ParentId == target.PlayerId)
                        {
                            if (PlayerControl.LocalPlayer == killer || PlayerControl.LocalPlayer == target) Coroutines.Start(PerformKillButton.RemoveBody(body));
                            else body.gameObject.Destroy();
                        }
                    }
                }

                if (MeetingHud.Instance) target.Exiled();

                killer.isKilling = false;
                target.isKilling = false;

                if (target.AmOwner)
                {
                    UpdateTaskText(target);
                }

                var alivesnonkiller = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Is(Faction.Impostors) && !x.Is(Faction.NeutralKilling) && !x.Is(Faction.Madmates) && !x.Is(Faction.Coven) && !x.Data.IsDead && !x.Data.Disconnected).ToList();

                if (alivesnonkiller.Count <= 0 && CustomGameOptions.GameMode == GameMode.Chaos && AmongUsClient.Instance.AmHost)
                {
                    Role.ImpostorWins = true;
                    Rpc(CustomRPC.ImpostorWin);
                    EndGame(GameOverReason.ImpostorsByVote);
                    PluginSingleton<TownOfUsEdited>.Instance.Log.LogMessage("GAME OVER REASON: Chaos GameMode Impostor Win");
                }

                if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
                {
                    RoleManager.Instance.SetRole(target, RoleTypes.CrewmateGhost);
                    if (target == PlayerControl.LocalPlayer) Utils.ShowDeadBodies = true;
                }

                PlayerControl_Die.Postfix();

                if (!killer.AmOwner) return;

                if (target.Is(ModifierEnum.Bait) && !killer.Is(RoleEnum.SoulCollector))
                {
                    BaitReport(killer, target);
                }

                if (target.Is(ModifierEnum.Aftermath))
                {
                    Aftermath.ForceAbility(killer, target);
                }

                if (target.Is(RoleEnum.Reviver))
                {
                    var reviver = Role.GetRole<Reviver>(target);
                    if (reviver.UsedRevive) Unmorph(target);
                    Utils.Rpc(CustomRPC.UnMorphReviver, target.PlayerId);
                }

                if (!jumpToBody) return;

                if (killer.Data.IsImpostor() && GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek
                && killer == PlayerControl.LocalPlayer)
                {
                    Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = GameOptionsManager.Instance.currentHideNSeekGameOptions.KillCooldown;
                    return;
                }

                if (killer.Data.IsImpostor() && CustomGameOptions.GameMode == GameMode.Chaos &&
                killer == PlayerControl.LocalPlayer)
                {
                    Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = 0.01f;
                    return;
                }

                if (killer.Data.IsImpostor() && CustomGameOptions.GameMode == GameMode.Werewolf &&
                killer == PlayerControl.LocalPlayer)
                {
                    Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = CustomGameOptions.WerewolfKillCD;
                    return;
                }

                if (killer == PlayerControl.LocalPlayer && killer.Is(RoleEnum.BountyHunter))
                {
                    var bh = Role.GetRole<BountyHunter>(killer);
                    if (bh.BountyTarget == target)
                    {
                        Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = CustomGameOptions.TargetShortCooldown;
                        bh.BountyTarget = null; //Forces to reload target
                    }
                    else
                    {
                        Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = CustomGameOptions.TargetLongCooldown;
                        bh.BountyTarget = null; //Forces to reload target
                    }
                    return;
                }

                if (killer == PlayerControl.LocalPlayer && killer.Is(RoleEnum.Warlock))
                {
                    var warlock = Role.GetRole<Warlock>(killer);
                    if (warlock.Charging)
                    {
                        warlock.UsingCharge = true;
                        warlock.ChargeUseDuration = warlock.ChargePercent * CustomGameOptions.ChargeUseDuration / 100f;
                        if (warlock.ChargeUseDuration == 0f) warlock.ChargeUseDuration += 0.01f;
                        Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = 0.01f;
                        return;
                    }
                    else if (warlock.UsingCharge)
                    {
                        Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = 0.01f;
                        return;
                    }
                }

                if (killer == PlayerControl.LocalPlayer && killer.Is(RoleEnum.Mafioso))
                {
                    Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = CustomGameOptions.MafiosoKillTimer;
                    return;
                }

                if (target.Is(ModifierEnum.Diseased) && killer.Is(RoleEnum.Maul))
                {
                    var werewolf = Role.GetRole<Maul>(killer);
                    werewolf.Cooldown = CustomGameOptions.RampageKillCd * CustomGameOptions.DiseasedMultiplier;
                    return;
                }

                if (target.Is(ModifierEnum.Diseased) && killer.Is(RoleEnum.SoulCollector))
                {
                    var sc = Role.GetRole<SoulCollector>(killer);
                    sc.Cooldown = CustomGameOptions.ReapCd * CustomGameOptions.DiseasedMultiplier;
                    return;
                }

                if (target.Is(ModifierEnum.Diseased) && killer.Is(RoleEnum.Vampire))
                {
                    var vampire = Role.GetRole<Vampire>(killer);
                    vampire.Cooldown = CustomGameOptions.BiteCd * CustomGameOptions.DiseasedMultiplier;
                    return;
                }

                if (target.Is(ModifierEnum.Diseased) && killer.Is(RoleEnum.SerialKiller))
                {
                    var sk = Role.GetRole<SerialKiller>(killer);
                    sk.Cooldown = CustomGameOptions.SerialKillerKCD * CustomGameOptions.DiseasedMultiplier;
                    return;
                }

                if (target.Is(ModifierEnum.Diseased) && killer.Is(RoleEnum.Doppelganger))
                {
                    var doppel = Role.GetRole<Doppelganger>(killer);
                    doppel.Cooldown = CustomGameOptions.DoppelKCD * CustomGameOptions.DiseasedMultiplier;
                    return;
                }

                if (target.Is(ModifierEnum.Diseased) && killer.Is(RoleEnum.Mutant))
                {
                    var mutant = Role.GetRole<Mutant>(killer);
                    if (mutant.IsTransformed != true)
                    {
                        mutant.Cooldown = CustomGameOptions.MutantKCD * CustomGameOptions.DiseasedMultiplier;
                    }
                    else
                    {
                        mutant.Cooldown = CustomGameOptions.TransformKCD * CustomGameOptions.DiseasedMultiplier;
                    }
                    return;
                }

                if (target.Is(ModifierEnum.Diseased) && killer.Is(RoleEnum.Infectious))
                {
                    var infectious = Role.GetRole<Infectious>(killer);
                    if (infectious.Infected.Contains(target.PlayerId))
                    {
                        infectious.Cooldown = CustomGameOptions.InfectiousInfectedCD * CustomGameOptions.DiseasedMultiplier;
                    }
                    else infectious.Cooldown = CustomGameOptions.InfectiousCD * CustomGameOptions.DiseasedMultiplier;
                    return;
                }

                if (target.Is(ModifierEnum.Diseased) && killer.Is(RoleEnum.Glitch))
                {
                    var glitch = Role.GetRole<Glitch>(killer);
                    glitch.Cooldown = CustomGameOptions.GlitchKillCooldown * CustomGameOptions.DiseasedMultiplier;
                    return;
                }

                if (target.Is(ModifierEnum.Diseased) && killer.Is(RoleEnum.Mafioso))
                {
                    Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = CustomGameOptions.MafiosoKillTimer * CustomGameOptions.DiseasedMultiplier;
                    return;
                }

                if (target.Is(ModifierEnum.Diseased) && killer.Is(RoleEnum.Juggernaut))
                {
                    var juggernaut = Role.GetRole<Juggernaut>(killer);
                    juggernaut.Cooldown = (CustomGameOptions.JuggKCd - CustomGameOptions.ReducedKCdPerKill * juggernaut.JuggKills) * CustomGameOptions.DiseasedMultiplier;
                    return;
                }

                if (target.Is(ModifierEnum.Diseased) && killer.Is(ModifierEnum.Underdog))
                {
                    var lowerKC = (GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown - CustomGameOptions.UnderdogKillBonus) * CustomGameOptions.DiseasedMultiplier;
                    var normalKC = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * CustomGameOptions.DiseasedMultiplier;
                    var upperKC = (GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown + CustomGameOptions.UnderdogKillBonus) * CustomGameOptions.DiseasedMultiplier;
                    Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = PerformKill.LastImp() ? lowerKC : (PerformKill.IncreasedKC() ? normalKC : upperKC);
                    return;
                }

                if (target.Is(ModifierEnum.Diseased) && killer.Is(ModifierEnum.Bloodlust))
                {
                    var role = Modifier.GetModifier<Bloodlust>(PlayerControl.LocalPlayer);
                    var num = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown / 2 * CustomGameOptions.DiseasedMultiplier;
                    role.KilledThisRound += 1;
                    if (role.KilledThisRound >= 2)
                    {
                        Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = num;
                        return;
                    }
                }

                if (target.Is(ModifierEnum.Diseased) && killer.Is(ModifierEnum.Lucky))
                {
                    var num = Random.RandomRange(1f, 60f);
                    Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = num * CustomGameOptions.DiseasedMultiplier;
                    return;
                }

                if (target.Is(ModifierEnum.Diseased) && killer.Data.IsImpostor())
                {
                    Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * CustomGameOptions.DiseasedMultiplier;
                    return;
                }

                if (killer.Is(RoleEnum.Shooter))
                {
                    var role = Role.GetRole<Shooter>(killer);
                    if (role.UsesLeft > 0)
                    {
                        role.UsesLeft--;
                        Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = 0.01f;
                        return;
                    }
                }

                if (killer.Is(ModifierEnum.Underdog))
                {
                    var lowerKC = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown - CustomGameOptions.UnderdogKillBonus;
                    var normalKC = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                    var upperKC = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown + CustomGameOptions.UnderdogKillBonus;
                    Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = PerformKill.LastImp() ? lowerKC : (PerformKill.IncreasedKC() ? normalKC : upperKC);
                    return;
                }

                if (killer.Is(ModifierEnum.Lucky))
                {
                    var num = Random.RandomRange(1f, 60f);
                    Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = num;
                    return;
                }

                if (killer.Is(ModifierEnum.Bloodlust))
                {
                    var role = Modifier.GetModifier<Bloodlust>(PlayerControl.LocalPlayer);
                    var num = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown / 2;
                    role.KilledThisRound += 1;
                    if (role.KilledThisRound >= 2)
                    {
                        Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = num;
                        return;
                    }
                }

                if (killer.Data.IsImpostor() && !killer.Is(RoleEnum.BountyHunter))
                {
                    Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                    return;
                }
            }
        }

        public static void UpdateTaskText(PlayerControl player)
        {
            var targetRole = Role.GetRole(player);
            if (targetRole != null) targetRole.RegenTask();
        }
        public static void BaitReport(PlayerControl killer, PlayerControl target)
        {
            Coroutines.Start(BaitReportDelay(killer, target));
        }

        public static IEnumerator BaitReportDelay(PlayerControl killer, PlayerControl target)
        {
            var extraDelay = Random.RandomRangeInt(0, (int) (100 * (CustomGameOptions.BaitMaxDelay - CustomGameOptions.BaitMinDelay) + 1));
            if (CustomGameOptions.BaitMaxDelay <= CustomGameOptions.BaitMinDelay)
                yield return new WaitForSeconds(CustomGameOptions.BaitMaxDelay + 0.01f);
            else
                yield return new WaitForSeconds(CustomGameOptions.BaitMinDelay + 0.01f + extraDelay/100f);
            var bodies = Object.FindObjectsOfType<DeadBody>();
            if (AmongUsClient.Instance.AmHost)
            {
                foreach (var body in bodies)
                {
                    try
                    {
                        if (body.ParentId == target.PlayerId) { killer.ReportDeadBody(target.Data); break; }
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                foreach (var body in bodies)
                {
                    try
                    {
                        if (body.ParentId == target.PlayerId)
                        {
                            Rpc(CustomRPC.BaitReport, killer.PlayerId, target.PlayerId);
                            break;
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        public static IEnumerator FlashCoroutine(Color color, float waitfor = 1f, float alpha = 0.3f)
        {
            var startTime = DateTime.UtcNow;
            while (true)
            {
                var now = DateTime.UtcNow;
                var seconds = (now - startTime).TotalSeconds;
                if (seconds < waitfor)
                {
                    color.a = alpha;
                    if (HudManager.InstanceExists && HudManager.Instance.FullScreen)
                    {
                        var fullscreen = DestroyableSingleton<HudManager>.Instance.FullScreen;
                        fullscreen.enabled = true;
                        fullscreen.gameObject.active = true;
                        fullscreen.color = color;
                    }
                    yield return null;
                }
                else
                {
                    if (HudManager.InstanceExists && HudManager.Instance.FullScreen)
                    {
                        var fullscreen = DestroyableSingleton<HudManager>.Instance.FullScreen;
                        if (fullscreen.color.Equals(color))
                        {
                            fullscreen.color = new Color(1f, 0f, 0f, 0.37254903f);
                            fullscreen.gameObject.SetActive(false);
                        }
                    }
                    yield break;
                }
            }
        }

        public static IEnumerable<(T1, T2)> Zip<T1, T2>(List<T1> first, List<T2> second)
        {
            return first.Zip(second, (x, y) => (x, y));
        }

        public static void RemoveTasks(PlayerControl player)
        {
            var totalTasks = GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks + GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks +
                             GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks;


            foreach (var task in player.myTasks)
                if (task.TryCast<NormalPlayerTask>() != null)
                {
                    var normalPlayerTask = task.Cast<NormalPlayerTask>();

                    var updateArrow = normalPlayerTask.taskStep > 0;

                    normalPlayerTask.taskStep = 0;
                    normalPlayerTask.Initialize();
                    if (normalPlayerTask.TaskType == TaskTypes.PickUpTowels)
                        foreach (var console in Object.FindObjectsOfType<TowelTaskConsole>())
                            console.Image.color = Color.white;
                    normalPlayerTask.taskStep = 0;
                    if (normalPlayerTask.TaskType == TaskTypes.UploadData)
                        normalPlayerTask.taskStep = 1;
                    if ((normalPlayerTask.TaskType == TaskTypes.EmptyGarbage || normalPlayerTask.TaskType == TaskTypes.EmptyChute)
                        && (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 0 ||
                        GameOptionsManager.Instance.currentNormalGameOptions.MapId == 3 ||
                        GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4))
                        normalPlayerTask.taskStep = 1;
                    if (updateArrow)
                        normalPlayerTask.UpdateArrowAndLocation();

                    var taskInfo = player.Data.FindTaskById(task.Id);
                    taskInfo.Complete = false;
                }
        }

        public static void RecreateTasks(NetworkedPlayerInfo playerById)
        {
            //Modified vanilla code from ShipStatus.Begin
            IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
		    HashSet<TaskTypes> hashSet = new HashSet<TaskTypes>();
		    List<byte> list = new List<byte>(10);
		    List<NormalPlayerTask> list2 = Enumerable.ToList<NormalPlayerTask>(ShipStatus.Instance.CommonTasks);
		    list2.Shuffle();
		    list2.ForEach(delegate(NormalPlayerTask t)
		    {
			    t.Length = NormalPlayerTask.TaskLength.Common;
		    });
		    int @int = currentGameOptions.GetInt(Int32OptionNames.NumCommonTasks);
		    for (int i = 0; i < @int; i++)
		    {
			    if (list2.Count == 0)
			    {
				    Debug.LogWarning("Not enough common tasks");
				    break;
			    }
			    int index = list2.RandomIdx<NormalPlayerTask>();
			    list.Add((byte)list2[index].Index);
			    list2.RemoveAt(index);
		    }
		    List<NormalPlayerTask> list3 = Enumerable.ToList<NormalPlayerTask>(ShipStatus.Instance.LongTasks);
		    list3.ForEach(delegate(NormalPlayerTask t)
		    {
			    t.Length = NormalPlayerTask.TaskLength.Long;
		    });
		    list3.Shuffle();
            int int5 = currentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
            for (int i = 0; i < @int5; i++)
		    {
			    if (list3.Count == 0)
			    {
				    Debug.LogWarning("Not enough long tasks");
				    break;
			    }
			    int index = list3.RandomIdx<NormalPlayerTask>();
			    list.Add((byte)list3[index].Index);
			    list3.RemoveAt(index);
		    }
		    List<NormalPlayerTask> list4 = Enumerable.ToList<NormalPlayerTask>(ShipStatus.Instance.ShortTasks);
		    list4.ForEach(delegate(NormalPlayerTask t)
		    {
			    t.Length = NormalPlayerTask.TaskLength.Short;
		    });
		    list4.Shuffle();
            int int6 = currentGameOptions.GetInt(Int32OptionNames.NumShortTasks);
            for (int i = 0; i < @int6; i++)
		    {
			    if (list4.Count == 0)
			    {
				    Debug.LogWarning("Not enough short tasks");
				    break;
			    }
			    int index = list4.RandomIdx<NormalPlayerTask>();
			    list.Add((byte)list4[index].Index);
			    list4.RemoveAt(index);
		    }
			if (playerById.Object && !playerById.Object.GetComponent<DummyBehaviour>().enabled)
			{
				byte[] taskTypeIds = list.ToArray();
				playerById.SetTasks(taskTypeIds);
			}
        }

        public static void DestroyAll(this IEnumerable<Component> listie)
        {
            foreach (var item in listie)
            {
                if (item == null) continue;
                Object.Destroy(item);
                if (item.gameObject == null) return;
                Object.Destroy(item.gameObject);
            }
        }

        public static void EndGame(GameOverReason reason = GameOverReason.ImpostorsByVote, bool showAds = false)
        {
            GameManager.Instance.RpcEndGame(reason, showAds);
        }


        public static void Rpc(params object[] data)
        {
            if (data[0] is not CustomRPC) throw new ArgumentException($"first parameter must be a {typeof(CustomRPC).FullName}");

            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        254, SendOption.Reliable, -1);

            if (data.Length == 1)
            {
                writer.Write((int)(CustomRPC)data[0]);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                return;
            }

            writer.Write((int)(CustomRPC)data[0]);

            foreach (var item in data[1..])
            {

                if (item is bool boolean)
                {
                    writer.Write(boolean);
                }
                else if (item is int integer)
                {
                    writer.Write(integer);
                }
                else if (item is uint uinteger)
                {
                    writer.Write(uinteger);
                }
                else if (item is float Float)
                {
                    writer.Write(Float);
                }
                else if (item is byte Byte)
                {
                    writer.Write(Byte);
                }
                else if (item is sbyte sByte)
                {
                    writer.Write(sByte);
                }
                else if (item is Vector2 vector)
                {
                    writer.Write(vector);
                }
                else if (item is Vector3 vector3)
                {
                    writer.Write(vector3);
                }
                else if (item is string str)
                {
                    writer.Write(str);
                }
                else if (item is byte[] array)
                {
                    writer.WriteBytesAndSize(array);
                }
                else
                {
                    Logger<TownOfUsEdited>.Error($"unknown data type entered for rpc write: item - {nameof(item)}, {item.GetType().FullName}, rpc - {data[0]}");
                }
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        [HarmonyPatch(typeof(MedScanMinigame), nameof(MedScanMinigame.FixedUpdate))]
        class MedScanMinigameFixedUpdatePatch
        {
            static void Prefix(MedScanMinigame __instance)
            {
                if (CustomGameOptions.ParallelMedScans)
                {
                    //Allows multiple medbay scans at once
                    __instance.medscan.CurrentUser = PlayerControl.LocalPlayer.PlayerId;
                    __instance.medscan.UsersList.Clear();
                }
            }
        }
      
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
        class StartMeetingPatch {
            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)]NetworkedPlayerInfo meetingTarget) {
                voteTarget = meetingTarget;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        class MeetingHudUpdatePatch {
            static void Postfix(MeetingHud __instance) {
                // Deactivate skip Button if skipping on emergency meetings is disabled 
                if ((voteTarget == null && CustomGameOptions.SkipButtonDisable == DisableSkipButtonMeetings.Emergency) || (CustomGameOptions.SkipButtonDisable == DisableSkipButtonMeetings.Always)) {
                    __instance.SkipVoteButton.gameObject.SetActive(false);
                }
            }
        }

        //Submerged utils
        public static object TryCast(this Il2CppObjectBase self, Type type)
        {
            return AccessTools.Method(self.GetType(), nameof(Il2CppObjectBase.TryCast)).MakeGenericMethod(type).Invoke(self, Array.Empty<object>());
        }
        public static IList createList(Type myType)
        {
            Type genericListType = typeof(List<>).MakeGenericType(myType);
            return (IList)Activator.CreateInstance(genericListType);
        }

        public static Dictionary<T2, T3> TryToDictionary<T1, T2, T3>(this IEnumerable<T1> source, Func<T1, T2> keySelector, Func<T1, T3> valueSelector)
        {
            var dict = new Dictionary<T2, T3>();

            foreach (var item in source)
                dict.TryAdd(keySelector(item), valueSelector(item));

            return dict;
        }

        public static void ResetCustomTimers()
        {
            #region CrewmateRoles
            if (PlayerControl.LocalPlayer.Is(RoleEnum.TimeLord))
            {
                var tl = Role.GetRole<TimeLord>(PlayerControl.LocalPlayer);
                tl.Cooldown = CustomGameOptions.RewindCooldown;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Medium))
            {
                var medium = Role.GetRole<Medium>(PlayerControl.LocalPlayer);
                medium.Cooldown = CustomGameOptions.MediateCooldown;
            }
            foreach (var role in Role.GetRoles(RoleEnum.Medium))
            {
                var medium = (Medium)role;
                medium.MediatedPlayers.Values.DestroyAll();
                medium.MediatedPlayers.Clear();
            }
            foreach (var role in Role.GetRoles(RoleEnum.Deputy))
            {
                var deputy = (Deputy)role;
                deputy.Camping = null;
                deputy.Killer = null;
                deputy.CampedThisRound = false;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Doctor))
            {
                var doctor = Role.GetRole<Doctor>(PlayerControl.LocalPlayer);
                doctor.Cooldown = CustomGameOptions.DocReviveCooldown;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Politician))
            {
                var politician = Role.GetRole<Politician>(PlayerControl.LocalPlayer);
                politician.Cooldown = CustomGameOptions.InitialCooldowns;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Jailor))
            {
                var jailor = Role.GetRole<Jailor>(PlayerControl.LocalPlayer);
                jailor.Cooldown = CustomGameOptions.JailCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Captain))
            {
                var cap = Role.GetRole<Captain>(PlayerControl.LocalPlayer);
                cap.Cooldown = CustomGameOptions.ZoomCooldown;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Seer))
            {
                var seer = Role.GetRole<Seer>(PlayerControl.LocalPlayer);
                seer.Cooldown = CustomGameOptions.SeerCd;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Oracle))
            {
                var oracle = Role.GetRole<Oracle>(PlayerControl.LocalPlayer);
                oracle.Cooldown = CustomGameOptions.ConfessCd;
                oracle.BlessCooldown = CustomGameOptions.BlessCD;
            }
            foreach (var role in Role.GetRoles(RoleEnum.Oracle))
            {
                var oracle = (Oracle)role;
                oracle.Blessed = null;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Cleric))
            {
                var cleric = Role.GetRole<Cleric>(PlayerControl.LocalPlayer);
                cleric.Cooldown = CustomGameOptions.BarrierCD;
            }
            foreach (var role in Role.GetRoles(RoleEnum.Cleric))
            {
                var cler = (Cleric)role;
                cler.CleansedPlayers.Clear();
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Sheriff))
            {
                var sheriff = Role.GetRole<Sheriff>(PlayerControl.LocalPlayer);
                sheriff.Cooldown = CustomGameOptions.SheriffKillCd;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Knight))
            {
                var knight = Role.GetRole<Knight>(PlayerControl.LocalPlayer);
                knight.Cooldown = CustomGameOptions.KnightKCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Crusader))
            {
                var crusader = Role.GetRole<Crusader>(PlayerControl.LocalPlayer);
                crusader.Cooldown = CustomGameOptions.CrusadeCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Fighter))
            {
                var fighter = Role.GetRole<Fighter>(PlayerControl.LocalPlayer);
                fighter.Cooldown = CustomGameOptions.FighterKCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Hunter))
            {
                var hunter = Role.GetRole<Hunter>(PlayerControl.LocalPlayer);
                hunter.Cooldown = CustomGameOptions.HunterKillCd;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Tracker))
            {
                var tracker = Role.GetRole<Tracker>(PlayerControl.LocalPlayer);
                tracker.Cooldown = CustomGameOptions.TrackCd;
                tracker.UsesLeft = CustomGameOptions.MaxTracks;
                if (CustomGameOptions.ResetOnNewRound)
                {
                    tracker.TrackerArrows.Values.DestroyAll();
                    tracker.TrackerArrows.Clear();
                }
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.VampireHunter))
            {
                var vh = Role.GetRole<VampireHunter>(PlayerControl.LocalPlayer);
                vh.Cooldown = CustomGameOptions.StakeCd;
            }
            foreach (var vh in Role.GetRoles(RoleEnum.VampireHunter))
            {
                var vhRole = (VampireHunter)vh;
                if (!vhRole.AddedStakes)
                {
                    vhRole.UsesLeft = CustomGameOptions.MaxFailedStakesPerGame;
                    vhRole.AddedStakes = true;
                }
                var vamps = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(RoleEnum.Vampire) && !x.Data.IsDead && !x.Data.Disconnected).ToList();
                if (vamps.Count == 0 && !StartImitate.ImitatingPlayers.Contains(vh.Player.PlayerId) && !vh.Player.Data.IsDead && !vh.Player.Data.Disconnected)
                {
                    var vhPlayer = vhRole.Player;

                    if (CustomGameOptions.BecomeOnVampDeaths == BecomeEnum.Sheriff)
                    {
                        Role.RoleDictionary.Remove(vhPlayer.PlayerId);
                        var kills = ((VampireHunter)vh).CorrectKills;
                        var sheriff = new Sheriff(vhPlayer);
                        sheriff.CorrectKills = kills;
                        sheriff.RegenTask();
                    }
                    else if (CustomGameOptions.BecomeOnVampDeaths == BecomeEnum.Veteran)
                    {
                        if (PlayerControl.LocalPlayer == vhPlayer) Object.Destroy(((VampireHunter)vh).UsesText);
                        Role.RoleDictionary.Remove(vhPlayer.PlayerId);
                        var kills = ((VampireHunter)vh).CorrectKills;
                        var vet = new Veteran(vhPlayer);
                        vet.CorrectKills = kills;
                        vet.RegenTask();
                        vet.Cooldown = CustomGameOptions.AlertCd;
                    }
                    else if (CustomGameOptions.BecomeOnVampDeaths == BecomeEnum.Vigilante)
                    {
                        Role.RoleDictionary.Remove(vhPlayer.PlayerId);
                        var kills = ((VampireHunter)vh).CorrectKills;
                        var vigi = new Vigilante(vhPlayer);
                        vigi.CorrectKills = kills;
                        vigi.RegenTask();
                    }
                    else
                    {
                        Role.RoleDictionary.Remove(vhPlayer.PlayerId);
                        var kills = ((VampireHunter)vh).CorrectKills;
                        var crew = new Crewmate(vhPlayer);
                        crew.CorrectKills = kills;
                        crew.RegenTask();
                    }
                }
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Transporter))
            {
                var transporter = Role.GetRole<Transporter>(PlayerControl.LocalPlayer);
                transporter.Cooldown = CustomGameOptions.TransportCooldown;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Veteran))
            {
                var veteran = Role.GetRole<Veteran>(PlayerControl.LocalPlayer);
                veteran.Cooldown = CustomGameOptions.AlertCd;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Astral))
            {
                var astral = Role.GetRole<Astral>(PlayerControl.LocalPlayer);
                astral.Cooldown = CustomGameOptions.GhostCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Lookout))
            {
                var lookout = Role.GetRole<Lookout>(PlayerControl.LocalPlayer);
                lookout.Cooldown = CustomGameOptions.WatchCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Trapper))
            {
                var trapper = Role.GetRole<Trapper>(PlayerControl.LocalPlayer);
                trapper.Cooldown = CustomGameOptions.TrapCooldown;
                trapper.trappedPlayers.Clear();
                if (CustomGameOptions.TrapsRemoveOnNewRound) trapper.traps.ClearTraps();
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Detective))
            {
                var detective = Role.GetRole<Detective>(PlayerControl.LocalPlayer);
                detective.LastExamined = DateTime.UtcNow;
                detective.ClosestPlayer = null;
                detective.CurrentTarget = null;
                if (PlayerControl.LocalPlayer.Data.IsDead)
                {
                    detective.InvestigatingScene = null;
                    CrimeSceneExtensions.ClearCrimeScenes(detective.CrimeScenes);
                }
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Chameleon))
            {
                var chameleon = Role.GetRole<Chameleon>(PlayerControl.LocalPlayer);
                chameleon.Cooldown = CustomGameOptions.ChamSwoopCooldown;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Plumber))
            {
                var plumber = Role.GetRole<Plumber>(PlayerControl.LocalPlayer);
                plumber.Cooldown = CustomGameOptions.FlushCd;
            }
            foreach (var role in Role.GetRoles(RoleEnum.Plumber))
            {
                var plumber = (Plumber)role;
                foreach (var ventId in plumber.FutureBlocks)
                {
                    plumber.VentsBlocked.Add(ventId);
                    GameObject barricade = new GameObject("Barricade");
                    Vent trueVent = null;
                    foreach (var vent in ShipStatus.Instance.AllVents)
                    {
                        if (vent.Id == ventId) trueVent = vent;
                    }
                    var pos = trueVent.transform.position;
                    if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 5) pos.y -= 0.1f;
                    else if (GameOptionsManager.Instance.currentNormalGameOptions.MapId != 2) pos.y -= 0.04f;
                    pos.z -= 0.00001f;
                    barricade.transform.localPosition = pos;
                    SpriteRenderer render = barricade.AddComponent<SpriteRenderer>();
                    render.sprite = TownOfUsEdited.BarricadeSprite;
                    plumber.Barricades.Add(barricade);
                }
                plumber.FutureBlocks.Clear();
            }
            #endregion
            #region NeutralRoles
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Survivor))
            {
                var surv = Role.GetRole<Survivor>(PlayerControl.LocalPlayer);
                surv.LastVested = DateTime.UtcNow;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Vulture))
            {
                var vulture = Role.GetRole<Vulture>(PlayerControl.LocalPlayer);
                vulture.Cooldown = CustomGameOptions.VultureCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Shifter))
            {
                var shifter = Role.GetRole<Shifter>(PlayerControl.LocalPlayer);
                shifter.Cooldown = CustomGameOptions.ShiftCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Mercenary))
            {
                var merc = Role.GetRole<Mercenary>(PlayerControl.LocalPlayer);
                merc.Cooldown = CustomGameOptions.MercenaryCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.SoulCollector))
            {
                var sc = Role.GetRole<SoulCollector>(PlayerControl.LocalPlayer);
                sc.Cooldown = CustomGameOptions.ReapCd;
                sc.ClosestPlayer = null;
            }
            foreach (var role in Role.GetRoles(RoleEnum.SoulCollector))
            {
                var sc = (SoulCollector)role;
                SoulExtensions.ClearSouls(sc.Souls);
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Vampire))
            {
                var vamp = Role.GetRole<Vampire>(PlayerControl.LocalPlayer);
                vamp.Cooldown = CustomGameOptions.BiteCd;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.WhiteWolf))
            {
                var whitewolf = Role.GetRole<WhiteWolf>(PlayerControl.LocalPlayer);
                whitewolf.Cooldown = CustomGameOptions.WerewolfKillCD;
                whitewolf.RampageCooldown = CustomGameOptions.RampageCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.GuardianAngel))
            {
                var ga = Role.GetRole<GuardianAngel>(PlayerControl.LocalPlayer);
                ga.Cooldown = CustomGameOptions.ProtectCd;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Arsonist))
            {
                var arsonist = Role.GetRole<Arsonist>(PlayerControl.LocalPlayer);
                arsonist.Cooldown = CustomGameOptions.DouseCd;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Glitch))
            {
                var glitch = Role.GetRole<Glitch>(PlayerControl.LocalPlayer);
                glitch.MimicCooldown = CustomGameOptions.MimicCooldown;
                glitch.HackCooldown = CustomGameOptions.HackCooldown;
                glitch.Cooldown = CustomGameOptions.GlitchKillCooldown;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Juggernaut))
            {
                var juggernaut = Role.GetRole<Juggernaut>(PlayerControl.LocalPlayer);
                juggernaut.Cooldown = CustomGameOptions.JuggKCd - CustomGameOptions.ReducedKCdPerKill * juggernaut.JuggKills;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Maul))
            {
                var werewolf = Role.GetRole<Maul>(PlayerControl.LocalPlayer);
                werewolf.RampageCooldown = CustomGameOptions.RampageCd;
                werewolf.Cooldown = CustomGameOptions.RampageKillCd;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Plaguebearer))
            {
                var plaguebearer = Role.GetRole<Plaguebearer>(PlayerControl.LocalPlayer);
                plaguebearer.LastInfected = DateTime.UtcNow;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Terrorist))
            {
                var terrorist = Role.GetRole<Terrorist>(PlayerControl.LocalPlayer);
                terrorist.Cooldown = CustomGameOptions.TerroristKillCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Doppelganger))
            {
                var doppel = Role.GetRole<Doppelganger>(PlayerControl.LocalPlayer);
                doppel.Cooldown = CustomGameOptions.DoppelKCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.SerialKiller))
            {
                var sk = Role.GetRole<SerialKiller>(PlayerControl.LocalPlayer);
                sk.Cooldown = CustomGameOptions.SerialKillerKCD;
                sk.ConvertCooldown = CustomGameOptions.SerialKillerKCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Mutant))
            {
                var mutant = Role.GetRole<Mutant>(PlayerControl.LocalPlayer);
                mutant.Cooldown = CustomGameOptions.MutantKCD;
                mutant.TransformCooldown = CustomGameOptions.TransformCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Infectious))
            {
                var infectious = Role.GetRole<Infectious>(PlayerControl.LocalPlayer);
                infectious.Cooldown = CustomGameOptions.InfectiousCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Pestilence))
            {
                var pest = Role.GetRole<Pestilence>(PlayerControl.LocalPlayer);
                pest.Cooldown = CustomGameOptions.PestKillCd;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Doomsayer))
            {
                var doom = Role.GetRole<Doomsayer>(PlayerControl.LocalPlayer);
                doom.Cooldown = CustomGameOptions.ObserveCooldown;
                doom.LastObservedPlayer = null;
            }
            #endregion
            #region ImposterRoles
            if (PlayerControl.LocalPlayer.Data.IsImpostor() && CustomGameOptions.GameMode == GameMode.Werewolf)
            {
                Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = CustomGameOptions.WerewolfKillCD;
            }
            if (PlayerControl.LocalPlayer.Data.IsImpostor() && CustomGameOptions.GameMode != GameMode.Werewolf && CustomGameOptions.GameMode != GameMode.Chaos
            && !PlayerControl.LocalPlayer.Is(RoleEnum.Mafioso))
            {
                Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.TalkativeWolf))
            {
                var tw = Role.GetRole<TalkativeWolf>(PlayerControl.LocalPlayer);
                tw.RampageCooldown = CustomGameOptions.RampageCD;
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.BlackWolf))
            {
                var bw = Role.GetRole<BlackWolf>(PlayerControl.LocalPlayer);
                bw.RampageCooldown = CustomGameOptions.RampageCD;
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Werewolf))
            {
                var ww = Role.GetRole<Werewolf>(PlayerControl.LocalPlayer);
                ww.RampageCooldown = CustomGameOptions.RampageCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Manipulator))
            {
                var Manipulator= Role.GetRole<Manipulator>(PlayerControl.LocalPlayer);
                Manipulator.Cooldown = CustomGameOptions.ManipulateCD;
                Manipulator.IsManipulating = false;
                Manipulator.ManipulatedPlayer = null;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Mafioso))
            {
                Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = CustomGameOptions.MafiosoKillTimer;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.BountyHunter))
            {
                var bh = Role.GetRole<BountyHunter>(PlayerControl.LocalPlayer);
                bh.BountyTarget = null; //Forces to reset the target
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Poisoner))
            {
                var poisoner = Role.GetRole<Poisoner>(PlayerControl.LocalPlayer);
                if (PlayerControl.LocalPlayer.Is(ModifierEnum.Underdog))
                {
                    poisoner.Cooldown = PerformKill.LastImp() ? CustomGameOptions.PoisonCD - CustomGameOptions.UnderdogKillBonus :
                    PerformKill.IncreasedKC() ? CustomGameOptions.PoisonCD : (CustomGameOptions.PoisonCD + CustomGameOptions.UnderdogKillBonus);
                }
                else if (PlayerControl.LocalPlayer.Is(ModifierEnum.Lucky))
                {
                    var num = Random.RandomRange(1f, 60f);
                    poisoner.Cooldown = num;
                }
                else poisoner.Cooldown = CustomGameOptions.PoisonCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Shooter))
            {
                var shooter = Role.GetRole<Shooter>(PlayerControl.LocalPlayer);
                shooter.UsesLeft = 0;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Hypnotist))
            {
                var hypno = Role.GetRole<Hypnotist>(PlayerControl.LocalPlayer);
                hypno.Cooldown = CustomGameOptions.InitialCooldowns;
            }
            var hasHypnoed = false;
            foreach (var role in Role.GetRoles(RoleEnum.Hypnotist))
            {
                var hypno = (Hypnotist)role;
                if ((PlayerControl.LocalPlayer.Data.IsDead || hypno.Player.Data.IsDead) && hypno.HysteriaActive && hypno.HypnotisedPlayers.Contains(PlayerControl.LocalPlayer.PlayerId))
                {
                    hypno.HysteriaActive = false;
                    if (!PlayerControl.LocalPlayer.IsHypnotised()) hypno.UnHysteria();
                }
                else if (hypno.HysteriaActive && hypno.HypnotisedPlayers.Contains(PlayerControl.LocalPlayer.PlayerId) && !hasHypnoed)
                {
                    hypno.Hysteria();
                    hasHypnoed = true;
                }
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Escapist))
            {
                var escapist = Role.GetRole<Escapist>(PlayerControl.LocalPlayer);
                escapist.Cooldown = CustomGameOptions.EscapeCd;
                escapist.EscapeButton.graphic.sprite = TownOfUsEdited.MarkSprite;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Blackmailer))
            {
                var blackmailer = Role.GetRole<Blackmailer>(PlayerControl.LocalPlayer);
                blackmailer.Cooldown = CustomGameOptions.BlackmailCd;
                if (blackmailer.Player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    blackmailer.Blackmailed?.myRend().material.SetFloat("_Outline", 0f);
                }
            }
            foreach (var role in Role.GetRoles(RoleEnum.Blackmailer))
            {
                var blackmailer = (Blackmailer)role;
                blackmailer.Blackmailed = null;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Bomber))
            {
                var bomber = Role.GetRole<Bomber>(PlayerControl.LocalPlayer);
                bomber.PlantButton.graphic.sprite = TownOfUsEdited.PlantSprite;
                bomber.Bomb.ClearBomb();
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Grenadier))
            {
                var grenadier = Role.GetRole<Grenadier>(PlayerControl.LocalPlayer);
                grenadier.Cooldown = CustomGameOptions.GrenadeCd;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Converter))
            {
                var converter = Role.GetRole<Converter>(PlayerControl.LocalPlayer);
                converter.Cooldown = CustomGameOptions.ConvertCD + converter.ReviveCount * 5f;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Miner))
            {
                var miner = Role.GetRole<Miner>(PlayerControl.LocalPlayer);
                miner.Cooldown = CustomGameOptions.MineCd;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Morphling))
            {
                var morphling = Role.GetRole<Morphling>(PlayerControl.LocalPlayer);
                morphling.Cooldown = CustomGameOptions.MorphlingCd;
                morphling.MorphButton.graphic.sprite = TownOfUsEdited.SampleSprite;
                morphling.SampledPlayer = null;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Swooper))
            {
                var swooper = Role.GetRole<Swooper>(PlayerControl.LocalPlayer);
                swooper.Cooldown = CustomGameOptions.SwoopCd;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Venerer))
            {
                var venerer = Role.GetRole<Venerer>(PlayerControl.LocalPlayer);
                venerer.Cooldown = CustomGameOptions.AbilityCd;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Undertaker))
            {
                var undertaker = Role.GetRole<Undertaker>(PlayerControl.LocalPlayer);
                undertaker.Cooldown = CustomGameOptions.DragCd;
                undertaker.DragDropButton.graphic.sprite = TownOfUsEdited.DragSprite;
                undertaker.CurrentlyDragging = null;
            }
            if (PlayerControl.LocalPlayer.Is(ModifierEnum.Lucky) && !PlayerControl.LocalPlayer.Is(RoleEnum.Poisoner))
            {
                var num = Random.RandomRange(1f, 60f);
                Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = num;
            }
            #endregion
            #region Coven Roles
            if (PlayerControl.LocalPlayer.Is(Faction.Coven))
            {
                Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = CustomGameOptions.CovenKCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.HexMaster))
            {
                var hm = Role.GetRole<HexMaster>(PlayerControl.LocalPlayer);
                hm.Cooldown = CustomGameOptions.CovenKCD;
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.PotionMaster))
            {
                var pm = Role.GetRole<PotionMaster>(PlayerControl.LocalPlayer);
                pm.PotionCooldown = CustomGameOptions.PotionCD;
            }
            #endregion
            #region Modifiers
            if (PlayerControl.LocalPlayer.Is(ModifierEnum.Taskmaster) && !PlayerControl.LocalPlayer.Is(RoleEnum.Vampire) &&
                !PlayerControl.LocalPlayer.Is(RoleEnum.Traitor) && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                var taskinfos = PlayerControl.LocalPlayer.Data.Tasks.ToArray();
                var tasksLeft = taskinfos.Count(x => !x.Complete);
                if (tasksLeft != 0)
                {
                    var i = Random.RandomRangeInt(PlayerControl.LocalPlayer.myTasks.Count - taskinfos.Count, PlayerControl.LocalPlayer.myTasks.Count);
                    while (true)
                    {
                        var task = PlayerControl.LocalPlayer.myTasks[i];
                        if (task.TryCast<NormalPlayerTask>() != null)
                        {
                            var normalPlayerTask = task.Cast<NormalPlayerTask>();

                            if (normalPlayerTask.IsComplete)
                            {
                                i++;
                                if (i >= PlayerControl.LocalPlayer.myTasks.Count) i = 0;
                                continue;
                            }

                            if (normalPlayerTask.TaskType == TaskTypes.PickUpTowels)
                            {
                                normalPlayerTask.Data = new Il2CppStructArray<byte>([250, 250, 250, 250, 250, 250, 250, 250]);
                                foreach (var console in Object.FindObjectsOfType<TowelTaskConsole>())
                                    console.Image.color = Color.clear;
                            }
                            while (normalPlayerTask.taskStep < normalPlayerTask.MaxStep) normalPlayerTask.NextStep();

                            break;
                        }
                        else
                        {
                            i++;
                            if (i >= PlayerControl.LocalPlayer.myTasks.Count) i = 0;
                        }
                    }
                }
            }
            #endregion
        }

        internal static void SetTarget(object value, KillButton killButton, float naN)
        {
            throw new NotImplementedException();
        }

    }
}
