using System.Linq;
using HarmonyLib;
using TownOfUsEdited.Roles;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using System;
using TownOfUsEdited.CrewmateRoles.ImitatorMod;
using TownOfUsEdited.CrewmateRoles.InvestigatorMod;
using TownOfUsEdited.CrewmateRoles.TrapperMod;
using AmongUs.GameOptions;
using TownOfUsEdited.CrewmateRoles.MedicMod;
using Reactor.Utilities.Extensions;
using TownOfUsEdited.ImpostorRoles.SpiritMod;
using TownOfUsEdited.ImpostorRoles.FreezerMod;
using TownOfUsEdited.ChooseImpGhostRole;
using TownOfUsEdited.CrewmateRoles.GuardianMod;
using TownOfUsEdited.CrewmateRoles.HelperMod;
using TownOfUsEdited.ChooseCrewGhostRoles;
using TownOfUsEdited.ImpostorRoles.BlinderMod;
using TownOfUsEdited.CrewmateRoles.AltruistMod;
using TownOfUsEdited.CrewmateRoles.TimeLordMod;
using TownOfUsEdited.ImpostorRoles.ConverterMod;
using System.Collections;
using Reactor.Utilities;
using System.Collections.Generic;
using TownOfUsEdited.Extensions;
using TMPro;
using TownOfUsEdited.CovenRoles.CovenMod;
using TownOfUsEdited.Roles.Modifiers;
using Assassin2 = TownOfUsEdited.Roles.Modifiers.Assassin;
using Assassin = TownOfUsEdited.Roles.Assassin;

namespace TownOfUsEdited.Patches
{
    public static class FreePlayerPatch
    {
        private static Action ChangeRole(Type role)
        {
            void Listener()
            {
                ApplyRoleChange(role, false);
            }

            return Listener;
        }

        public static void ApplyRoleChange(Type role, bool spawn)
        {
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Snitch))
            {
                var snitch = Role.GetRole<Snitch>(PlayerControl.LocalPlayer);
                snitch.SnitchArrows.Values.DestroyAll();
                snitch.SnitchArrows.Clear();
                snitch.ImpArrows.DestroyAll();
                snitch.ImpArrows.Clear();
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Haunter) || PlayerControl.LocalPlayer.Is(RoleEnum.Phantom)
            || PlayerControl.LocalPlayer.Is(RoleEnum.Spirit) || PlayerControl.LocalPlayer.Is(RoleEnum.Doppelganger) || PlayerControl.LocalPlayer.Is(RoleEnum.Morphling) ||
            PlayerControl.LocalPlayer.Is(RoleEnum.Chameleon) || PlayerControl.LocalPlayer.Is(RoleEnum.Swooper) ||
            PlayerControl.LocalPlayer.Is(RoleEnum.Glitch) || PlayerControl.LocalPlayer.Is(RoleEnum.Reviver))
            {
                Utils.Unmorph(PlayerControl.LocalPlayer);
                PlayerControl.LocalPlayer.myRend().color = Color.white;
                PlayerControl.LocalPlayer.MyPhysics.ResetMoveState();
            }

            if (PlayerControl.LocalPlayer.Is(Faction.Coven))
            {
                CovenUpdate.SabotageButton.gameObject.SetActive(false);
            }

            if (PlayerControl.LocalPlayer.Is(AbilityEnum.Assassin))
            {
                Ability.AbilityDictionary.Remove(PlayerControl.LocalPlayer.PlayerId);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Amnesiac))
            {
                var amne = Role.GetRole<Amnesiac>(PlayerControl.LocalPlayer);
                if (amne.BodyArrows.Count != 0)
                {
                    amne.BodyArrows.Values.DestroyAll();
                    amne.BodyArrows.Clear();
                }
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Vulture))
            {
                var vulture = Role.GetRole<Vulture>(PlayerControl.LocalPlayer);
                if (vulture.BodyArrows.Count != 0)
                {
                    vulture.BodyArrows.Values.DestroyAll();
                    vulture.BodyArrows.Clear();
                }
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.BountyHunter))
            {
                var bh = Role.GetRole<BountyHunter>(PlayerControl.LocalPlayer);
                if (bh.TargetArrow.Count != 0)
                {
                    bh.TargetArrow.Values.DestroyAll();
                    bh.TargetArrow.Clear();
                }
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Warden))
            {
                var warden = Role.GetRole<Warden>(PlayerControl.LocalPlayer);
                if (warden.Fortified != null) ShowShield.ResetVisor(warden.Fortified, warden.Player);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Medic))
            {
                var medic = Role.GetRole<Medic>(PlayerControl.LocalPlayer);
                if (medic.ShieldedPlayer != null) ShowShield.ResetVisor(medic.ShieldedPlayer, medic.Player);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Cleric))
            {
                var cleric = Role.GetRole<Cleric>(PlayerControl.LocalPlayer);
                if (cleric.Barriered != null) cleric.UnBarrier();
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Plumber))
            {
                var plumberRole = Role.GetRole<Plumber>(PlayerControl.LocalPlayer);
                foreach (GameObject barricade in plumberRole.Barricades)
                {
                    UnityEngine.Object.Destroy(barricade);
                }
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Plumber))
            {
                var plumberRole = Role.GetRole<Plumber>(PlayerControl.LocalPlayer);
                plumberRole.Vent = null;
                UnityEngine.Object.Destroy(plumberRole.UsesText);
            }

            if (StartImitate.ImitatingPlayers.Contains(PlayerControl.LocalPlayer.PlayerId)) StartImitate.ImitatingPlayers.Remove(PlayerControl.LocalPlayer.PlayerId);

            if (PlayerControl.LocalPlayer.Is(RoleEnum.GuardianAngel))
            {
                var ga = Role.GetRole<GuardianAngel>(PlayerControl.LocalPlayer);
                ga.UnProtect();
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Glitch))
            {
                var glitch = Role.GetRole<Glitch>(PlayerControl.LocalPlayer);
                glitch.MimicButton.gameObject.SetActive(false);
                glitch.HackButton.gameObject.SetActive(false);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Medium))
            {
                var medRole = Role.GetRole<Medium>(PlayerControl.LocalPlayer);
                medRole.MediatedPlayers.Values.DestroyAll();
                medRole.MediatedPlayers.Clear();
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Investigator)) Footprint.DestroyAll(Role.GetRole<Investigator>(PlayerControl.LocalPlayer));

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Sheriff) || PlayerControl.LocalPlayer.Is(RoleEnum.Knight)) HudManager.Instance.KillButton.buttonLabelText.gameObject.SetActive(false);

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Engineer))
            {
                var engineerRole = Role.GetRole<Engineer>(PlayerControl.LocalPlayer);
                UnityEngine.Object.Destroy(engineerRole.UsesText);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Paranoïac))
            {
                var paranoRole = Role.GetRole<Paranoïac>(PlayerControl.LocalPlayer);
                UnityEngine.Object.Destroy(paranoRole.UsesText);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Tracker))
            {
                var trackerRole = Role.GetRole<Tracker>(PlayerControl.LocalPlayer);
                trackerRole.TrackerArrows.Values.DestroyAll();
                trackerRole.TrackerArrows.Clear();
                UnityEngine.Object.Destroy(trackerRole.UsesText);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Knight))
            {
                var knightRole = Role.GetRole<Knight>(PlayerControl.LocalPlayer);
                UnityEngine.Object.Destroy(knightRole.UsesText);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Transporter))
            {
                var transporterRole = Role.GetRole<Transporter>(PlayerControl.LocalPlayer);
                UnityEngine.Object.Destroy(transporterRole.UsesText);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Veteran))
            {
                var veteranRole = Role.GetRole<Veteran>(PlayerControl.LocalPlayer);
                UnityEngine.Object.Destroy(veteranRole.UsesText);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Trapper))
            {
                var trapperRole = Role.GetRole<Trapper>(PlayerControl.LocalPlayer);
                UnityEngine.Object.Destroy(trapperRole.UsesText);
                trapperRole.traps.ClearTraps();
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Doctor))
            {
                var docRole = Role.GetRole<Doctor>(PlayerControl.LocalPlayer);
                UnityEngine.Object.Destroy(docRole.UsesText);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Detective))
            {
                var detecRole = Role.GetRole<Detective>(PlayerControl.LocalPlayer);
                detecRole.ExamineButton.gameObject.SetActive(false);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Aurial))
            {
                var aurialRole = Role.GetRole<Aurial>(PlayerControl.LocalPlayer);
                aurialRole.SenseArrows.Values.DestroyAll();
                aurialRole.SenseArrows.Clear();
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Survivor))
            {
                var survRole = Role.GetRole<Survivor>(PlayerControl.LocalPlayer);
                UnityEngine.Object.Destroy(survRole.UsesText);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Mercenary))
            {
                var mercRole = Role.GetRole<Mercenary>(PlayerControl.LocalPlayer);
                mercRole.GuardButton.SetTarget(null);
                mercRole.GuardButton.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(mercRole.UsesText);
                UnityEngine.Object.Destroy(mercRole.GoldText);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.GuardianAngel))
            {
                var gaRole = Role.GetRole<GuardianAngel>(PlayerControl.LocalPlayer);
                UnityEngine.Object.Destroy(gaRole.UsesText);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.VampireHunter))
            {
                var vhRole = Role.GetRole<VampireHunter>(PlayerControl.LocalPlayer);
                UnityEngine.Object.Destroy(vhRole.UsesText);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Shooter))
            {
                var shooterRole = Role.GetRole<Shooter>(PlayerControl.LocalPlayer);
                UnityEngine.Object.Destroy(shooterRole.UsesText);
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.Warlock))
            {
                var warlockRole = Role.GetRole<Warlock>(PlayerControl.LocalPlayer);
                UnityEngine.Object.Destroy(warlockRole.ChargeText);
            }

            if (Role.GetRole(PlayerControl.LocalPlayer) != null)
            {
                if (Role.GetRole(PlayerControl.LocalPlayer).ExtraButtons.Any())
                {
                    foreach (var button in Role.GetRole(PlayerControl.LocalPlayer).ExtraButtons)
                    {
                        button.gameObject.SetActive(false);
                    }
                }
                if (Role.GetRole(PlayerControl.LocalPlayer).ButtonLabels.Any())
                {
                    foreach (var label in Role.GetRole(PlayerControl.LocalPlayer).ButtonLabels)
                    {
                        if (label != null) label.gameObject.SetActive(false);
                    }
                }
            }

            DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            DestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.gameObject.SetActive(false);
            DestroyableSingleton<HudManager>.Instance.SabotageButton.gameObject.SetActive(false);

            if (Role.RoleDictionary.ContainsKey(PlayerControl.LocalPlayer.PlayerId)) Role.RoleDictionary.Remove(PlayerControl.LocalPlayer.PlayerId);

            Role newRole;

            if (role == typeof(Aurial)) newRole = new Aurial(PlayerControl.LocalPlayer);
            else if (role == typeof(Guardian)) newRole = new Guardian(PlayerControl.LocalPlayer);
            else if (role == typeof(Haunter)) newRole = new Haunter(PlayerControl.LocalPlayer);
            else if (role == typeof(Helper)) newRole = new Helper(PlayerControl.LocalPlayer);
            else if (role == typeof(Astral)) newRole = new Astral(PlayerControl.LocalPlayer);
            else if (role == typeof(Captain)) newRole = new Captain(PlayerControl.LocalPlayer);
            else if (role == typeof(Chameleon)) newRole = new Chameleon(PlayerControl.LocalPlayer);
            else if (role == typeof(Detective)) newRole = new Detective(PlayerControl.LocalPlayer);
            else if (role == typeof(Informant)) newRole = new Informant(PlayerControl.LocalPlayer);
            else if (role == typeof(Investigator)) newRole = new Investigator(PlayerControl.LocalPlayer);
            else if (role == typeof(Lookout)) newRole = new Lookout(PlayerControl.LocalPlayer);
            else if (role == typeof(Mystic)) newRole = new Mystic(PlayerControl.LocalPlayer);
            else if (role == typeof(Oracle)) newRole = new Oracle(PlayerControl.LocalPlayer);
            else if (role == typeof(Seer)) newRole = new Seer(PlayerControl.LocalPlayer);
            else if (role == typeof(Snitch)) newRole = new Snitch(PlayerControl.LocalPlayer);
            else if (role == typeof(Spy)) newRole = new Spy(PlayerControl.LocalPlayer);
            else if (role == typeof(Tracker)) newRole = new Tracker(PlayerControl.LocalPlayer);
            else if (role == typeof(Trapper)) newRole = new Trapper(PlayerControl.LocalPlayer);
            else if (role == typeof(Avenger)) newRole = new Avenger(PlayerControl.LocalPlayer);
            else if (role == typeof(Deputy)) newRole = new Deputy(PlayerControl.LocalPlayer);
            else if (role == typeof(Fighter)) newRole = new Fighter(PlayerControl.LocalPlayer);
            else if (role == typeof(Hunter)) newRole = new Hunter(PlayerControl.LocalPlayer);
            else if (role == typeof(Jailor)) newRole = new Jailor(PlayerControl.LocalPlayer);
            else if (role == typeof(Knight)) newRole = new Knight(PlayerControl.LocalPlayer);
            else if (role == typeof(Sheriff)) newRole = new Sheriff(PlayerControl.LocalPlayer);
            else if (role == typeof(VampireHunter)) newRole = new VampireHunter(PlayerControl.LocalPlayer);
            else if (role == typeof(Veteran)) newRole = new Veteran(PlayerControl.LocalPlayer);
            else if (role == typeof(Vigilante)) newRole = new Vigilante(PlayerControl.LocalPlayer);
            else if (role == typeof(Altruist)) newRole = new Altruist(PlayerControl.LocalPlayer);
            else if (role == typeof(Bodyguard)) newRole = new Bodyguard(PlayerControl.LocalPlayer);
            else if (role == typeof(Crusader)) newRole = new Crusader(PlayerControl.LocalPlayer);
            else if (role == typeof(Cleric)) newRole = new Cleric(PlayerControl.LocalPlayer);
            else if (role == typeof(Doctor)) newRole = new Doctor(PlayerControl.LocalPlayer);
            else if (role == typeof(Medic)) newRole = new Medic(PlayerControl.LocalPlayer);
            else if (role == typeof(Engineer)) newRole = new Engineer(PlayerControl.LocalPlayer);
            else if (role == typeof(Plumber)) newRole = new Plumber(PlayerControl.LocalPlayer);
            else if (role == typeof(Imitator)) newRole = new Imitator(PlayerControl.LocalPlayer);
            else if (role == typeof(Medium)) newRole = new Medium(PlayerControl.LocalPlayer);
            else if (role == typeof(Paranoïac)) newRole = new Paranoïac(PlayerControl.LocalPlayer);
            else if (role == typeof(Politician)) newRole = new Politician(PlayerControl.LocalPlayer);
            else if (role == typeof(Mayor)) newRole = new Mayor(PlayerControl.LocalPlayer);
            else if (role == typeof(Prosecutor)) newRole = new Prosecutor(PlayerControl.LocalPlayer);
            else if (role == typeof(TimeLord)) newRole = new TimeLord(PlayerControl.LocalPlayer);
            else if (role == typeof(Swapper)) newRole = new Swapper(PlayerControl.LocalPlayer);
            else if (role == typeof(Transporter)) newRole = new Transporter(PlayerControl.LocalPlayer);
            else if (role == typeof(Warden)) newRole = new Warden(PlayerControl.LocalPlayer);
            else if (role == typeof(Amnesiac)) newRole = new Amnesiac(PlayerControl.LocalPlayer);
            else if (role == typeof(Mercenary)) newRole = new Mercenary(PlayerControl.LocalPlayer);
            else if (role == typeof(GuardianAngel)) newRole = new GuardianAngel(PlayerControl.LocalPlayer);
            else if (role == typeof(Shifter)) newRole = new Shifter(PlayerControl.LocalPlayer);
            else if (role == typeof(Survivor)) newRole = new Survivor(PlayerControl.LocalPlayer);
            else if (role == typeof(Doomsayer)) newRole = new Doomsayer(PlayerControl.LocalPlayer);
            else if (role == typeof(Executioner)) newRole = new Executioner(PlayerControl.LocalPlayer);
            else if (role == typeof(Jester)) newRole = new Jester(PlayerControl.LocalPlayer);
            else if (role == typeof(Phantom)) newRole = new Phantom(PlayerControl.LocalPlayer);
            else if (role == typeof(SoulCollector)) newRole = new SoulCollector(PlayerControl.LocalPlayer);
            else if (role == typeof(Troll)) newRole = new Troll(PlayerControl.LocalPlayer);
            else if (role == typeof(Vulture)) newRole = new Vulture(PlayerControl.LocalPlayer);
            else if (role == typeof(Arsonist)) newRole = new Arsonist(PlayerControl.LocalPlayer);
            else if (role == typeof(Attacker)) newRole = new Attacker(PlayerControl.LocalPlayer);
            else if (role == typeof(Terrorist)) newRole = new Terrorist(PlayerControl.LocalPlayer);
            else if (role == typeof(Doppelganger)) newRole = new Doppelganger(PlayerControl.LocalPlayer);
            else if (role == typeof(Infectious)) newRole = new Infectious(PlayerControl.LocalPlayer);
            else if (role == typeof(Maul)) newRole = new Maul(PlayerControl.LocalPlayer);
            else if (role == typeof(Mutant)) newRole = new Mutant(PlayerControl.LocalPlayer);
            else if (role == typeof(Juggernaut)) newRole = new Juggernaut(PlayerControl.LocalPlayer);
            else if (role == typeof(Plaguebearer)) newRole = new Plaguebearer(PlayerControl.LocalPlayer);
            else if (role == typeof(Pestilence)) newRole = new Pestilence(PlayerControl.LocalPlayer);
            else if (role == typeof(SerialKiller)) newRole = new SerialKiller(PlayerControl.LocalPlayer);
            else if (role == typeof(Glitch)) newRole = new Glitch(PlayerControl.LocalPlayer);
            else if (role == typeof(Vampire)) newRole = new Vampire(PlayerControl.LocalPlayer);
            else if (role == typeof(Impostor)) newRole = new Impostor(PlayerControl.LocalPlayer);
            else if (role == typeof(Blinder)) newRole = new Blinder(PlayerControl.LocalPlayer);
            else if (role == typeof(Freezer)) newRole = new Freezer(PlayerControl.LocalPlayer);
            else if (role == typeof(Spirit)) newRole = new Spirit(PlayerControl.LocalPlayer);
            else if (role == typeof(Assassin)) newRole = new Assassin(PlayerControl.LocalPlayer);
            else if (role == typeof(Escapist)) newRole = new Escapist(PlayerControl.LocalPlayer);
            else if (role == typeof(Grenadier)) newRole = new Grenadier(PlayerControl.LocalPlayer);
            else if (role == typeof(Morphling)) newRole = new Morphling(PlayerControl.LocalPlayer);
            else if (role == typeof(Swooper)) newRole = new Swooper(PlayerControl.LocalPlayer);
            else if (role == typeof(Venerer)) newRole = new Venerer(PlayerControl.LocalPlayer);
            else if (role == typeof(Bomber)) newRole = new Bomber(PlayerControl.LocalPlayer);
            else if (role == typeof(BountyHunter)) newRole = new BountyHunter(PlayerControl.LocalPlayer);
            else if (role == typeof(Conjurer)) newRole = new Conjurer(PlayerControl.LocalPlayer);
            else if (role == typeof(Manipulator)) newRole = new Manipulator(PlayerControl.LocalPlayer);
            else if (role == typeof(Poisoner)) newRole = new Poisoner(PlayerControl.LocalPlayer);
            else if (role == typeof(Shooter)) newRole = new Shooter(PlayerControl.LocalPlayer);
            else if (role == typeof(Traitor)) newRole = new Traitor(PlayerControl.LocalPlayer);
            else if (role == typeof(Warlock)) newRole = new Warlock(PlayerControl.LocalPlayer);
            else if (role == typeof(Witch)) newRole = new Witch(PlayerControl.LocalPlayer);
            else if (role == typeof(Blackmailer)) newRole = new Blackmailer(PlayerControl.LocalPlayer);
            else if (role == typeof(Converter)) newRole = new Converter(PlayerControl.LocalPlayer);
            else if (role == typeof(Hypnotist)) newRole = new Hypnotist(PlayerControl.LocalPlayer);
            else if (role == typeof(Janitor)) newRole = new Janitor(PlayerControl.LocalPlayer);
            else if (role == typeof(Mafioso)) newRole = new Mafioso(PlayerControl.LocalPlayer);
            else if (role == typeof(Miner)) newRole = new Miner(PlayerControl.LocalPlayer);
            else if (role == typeof(Reviver)) newRole = new Reviver(PlayerControl.LocalPlayer);
            else if (role == typeof(Undertaker)) newRole = new Undertaker(PlayerControl.LocalPlayer);
            else if (role == typeof(Coven)) newRole = new Coven(PlayerControl.LocalPlayer);
            else if (role == typeof(CovenLeader)) newRole = new CovenLeader(PlayerControl.LocalPlayer);
            else if (role == typeof(HexMaster)) newRole = new HexMaster(PlayerControl.LocalPlayer);
            else if (role == typeof(PotionMaster)) newRole = new PotionMaster(PlayerControl.LocalPlayer);
            else if (role == typeof(Ritualist)) newRole = new Ritualist(PlayerControl.LocalPlayer);
            else if (role == typeof(Spiritualist)) newRole = new Spiritualist(PlayerControl.LocalPlayer);
            else if (role == typeof(VoodooMaster)) newRole = new VoodooMaster(PlayerControl.LocalPlayer);
            else newRole = new Crewmate(PlayerControl.LocalPlayer);

            var flag = role == typeof(Guardian) || role == typeof(Haunter) || role == typeof(Helper) ||
            role == typeof(Spirit) || role == typeof(Blinder) || role == typeof(Freezer) || role == typeof(Phantom);

            if (PlayerControl.LocalPlayer.Data.IsDead && !flag)
            {
                Murder.KilledPlayers.Remove(
                    Murder.KilledPlayers.FirstOrDefault(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId));
                if (!spawn) PlayerControl.LocalPlayer.myTasks.RemoveAt(1);
                RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Crewmate);
                Utils.ShowDeadBodies = false;
                foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>())
                {
                    if (deadBody.ParentId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        deadBody.gameObject.Destroy();
                    }
                }
            }
            else if (!PlayerControl.LocalPlayer.Data.IsDead && flag)
            {
                RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.CrewmateGhost);
                Utils.ShowDeadBodies = true;
            }

            if (PlayerControl.LocalPlayer.Is(RoleEnum.GuardianAngel))
            {
                var ga = Role.GetRole<GuardianAngel>(PlayerControl.LocalPlayer);
                var players = PlayerControl.AllPlayerControls.ToArray().Where(x => x != PlayerControl.LocalPlayer && !x.Data.IsDead).ToList();
                if (players.Count > 0)
                {
                    var rand = UnityEngine.Random.RandomRangeInt(0, players.Count);
                    var pc = players[rand];
                    ga.target = pc;
                }
            }
            else if (PlayerControl.LocalPlayer.Is(RoleEnum.Executioner))
            {
                var exe = Role.GetRole<Executioner>(PlayerControl.LocalPlayer);
                var players = PlayerControl.AllPlayerControls.ToArray().Where(x => x != PlayerControl.LocalPlayer && !x.Data.IsDead && x.Is(Faction.Crewmates)).ToList();
                if (players.Count > 0)
                {
                    var rand = UnityEngine.Random.RandomRangeInt(0, players.Count);
                    var pc = players[rand];
                    exe.target = pc;
                }
            }

            if (role == typeof(Haunter))
            {
                var haunterRole = Role.GetRole<Haunter>(PlayerControl.LocalPlayer);
                haunterRole.Fade();
            }
            else if (role == typeof(Phantom))
            {
                var phantomRole = Role.GetRole<Phantom>(PlayerControl.LocalPlayer);
                phantomRole.Fade();
            }
            else if (role == typeof(Spirit))
            {
                var spiritRole = Role.GetRole<Spirit>(PlayerControl.LocalPlayer);
                spiritRole.Fade();
            }
            else if (role == typeof(Assassin))
            {
                new Assassin2(PlayerControl.LocalPlayer);
            }

            if (!spawn) newRole.RegenTask();
        }
        // Code from mira api, link: https://github.com/All-Of-Us-Mods/MiraAPI/blob/d0161ba85ca6647fe6deed8d4603feae429aad21/MiraAPI/Patches/Roles/TaskAdderPatch.cs
        private static Scroller Scroller;
        [HarmonyPatch(typeof(TaskAdderGame), nameof(TaskAdderGame.ShowFolder))]
        public class PatchFolder
        {
            public static bool Prefix(TaskAdderGame __instance, [HarmonyArgument(0)] TaskFolder taskFolder)
            {
                var stringBuilder = new Il2CppSystem.Text.StringBuilder(64);
                __instance.Hierarchy.Add(taskFolder);
                for (var i = 0; i < __instance.Hierarchy.Count; i++)
                {
                    stringBuilder.Append(__instance.Hierarchy.ToArray()[i].FolderName);
                    stringBuilder.Append("\\");
                }

                __instance.PathText.text = stringBuilder.ToString();
                for (var j = 0; j < __instance.ActiveItems.Count; j++)
                {
                    Object.Destroy(__instance.ActiveItems.ToArray()[j].gameObject);
                }

                __instance.ActiveItems.Clear();
                var num = 0f;
                var num2 = 0f;
                var num3 = 0f;
                for (int k = 0; k < taskFolder.SubFolders.Count; k++)
                {
                    TaskFolder taskFolder2 = Object.Instantiate<TaskFolder>(taskFolder.SubFolders[k], __instance.TaskParent);
                    taskFolder2.gameObject.SetActive(true);
                    taskFolder2.Parent = __instance;
                    taskFolder2.transform.localPosition = new Vector3(num, num2, 0f);
                    taskFolder2.transform.localScale = Vector3.one;
                    num3 = Mathf.Max(num3, taskFolder2.Text.bounds.size.y + 1.1f);
                    num += __instance.folderWidth;
                    if (num > __instance.lineWidth)
                    {
                        num = 0f;
                        num2 -= num3;
                        num3 = 0f;
                    }
                    __instance.ActiveItems.Add(taskFolder2.transform);
                    if (taskFolder2 != null && taskFolder2.Button != null)
                    {
                        ControllerManager.Instance.AddSelectableUiElement(taskFolder2.Button, false);
                    }
                }

                var flag = false;
                var list = taskFolder.TaskChildren.ToArray().OrderBy(t => t.TaskType).ToList();

                for (var l = 0; l < list.Count; l++)
                {
                    var taskAddButton = Object.Instantiate(__instance.TaskPrefab);
                    taskAddButton.MyTask = list.ToArray()[l];
                    switch (taskAddButton.MyTask.TaskType)
                    {
                        case TaskTypes.DivertPower:
                            {
                                var targetSystem = taskAddButton.MyTask.Cast<DivertPowerTask>().TargetSystem;
                                taskAddButton.Text.text = TranslationController.Instance.GetString(
                                    StringNames.DivertPowerTo,
                                    TranslationController.Instance.GetString(targetSystem));
                                break;
                            }
                        case TaskTypes.FixWeatherNode:
                            {
                                var nodeId = taskAddButton.MyTask.Cast<WeatherNodeTask>().NodeId;
                                taskAddButton.Text.text =
                                    TranslationController.Instance.GetString(
                                        StringNames.FixWeatherNode) + " " +
                                    TranslationController.Instance.GetString(
                                        WeatherSwitchGame.ControlNames[nodeId]);
                                break;
                            }
                        default:
                            taskAddButton.Text.text =
                                TranslationController.Instance.GetString(taskAddButton.MyTask.TaskType);
                            break;
                    }

                    __instance.AddFileAsChildCustom(taskAddButton, ref num, ref num2, ref num3);
                    if (taskAddButton.Button == null)
                    {
                        continue;
                    }

                    ControllerManager.Instance.AddSelectableUiElement(taskAddButton.Button);
                    if (__instance.Hierarchy.Count == 1 || flag)
                    {
                        continue;
                    }

                    ControllerManager.Instance.SetDefaultSelection(taskAddButton.Button);
                    flag = true;
                }

                if (taskFolder.FolderName == "Crewmate Roles")
                {
                    List<Type> Roles = [ typeof(Crewmate), typeof(Guardian), typeof(Haunter), typeof(Helper),
                    typeof(Astral), typeof(Aurial), typeof(Captain), typeof(Chameleon), typeof(Detective), typeof(Informant), typeof(Investigator),
                    typeof(Lookout), typeof(Mystic), typeof(Seer), typeof(Snitch), typeof(Spy), typeof(Tracker),
                    typeof(Trapper), typeof(Avenger), typeof(Deputy), typeof(Fighter), typeof(Hunter),
                    typeof(Knight), typeof(Sheriff), typeof(VampireHunter), typeof(Veteran), typeof(Vigilante),
                    typeof(Jailor), typeof(Mayor), typeof(Politician), typeof(Prosecutor), typeof(Swapper), typeof(TimeLord),
                    typeof(Altruist), typeof(Bodyguard), typeof(Cleric), typeof(Crusader), typeof(Doctor), typeof(Medic),
                    typeof(Oracle), typeof(Warden), typeof(Engineer), typeof(Imitator),
                    typeof(Medium), typeof(Paranoïac), typeof(Plumber), typeof(Transporter) ];
                    while (Roles.Count > 0)
                    {
                        Type role = Roles[0];
                        var taskAddButton = Object.Instantiate<TaskAddButton>(__instance.RoleButton);
                        taskAddButton.SafePositionWorld = __instance.SafePositionWorld;
                        taskAddButton.Text.text = "<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Masked\">Be_" + role.Name + ".exe";
                        taskAddButton.Role = DestroyableSingleton<CrewmateRole>.Instance;
                        taskAddButton.name = role.Name;
                        if (taskAddButton.Button != null)
                        {
                            __instance.AddFileAsChildCustom(taskAddButton, ref num, ref num2, ref num3);
                            var taskevent = new Button.ButtonClickedEvent();
                            taskevent.AddListener(ChangeRole(role));
                            taskAddButton.Button.OnClick = taskevent;
                        }
                        Roles.Remove(role);
                    }
                }
                if (taskFolder.FolderName == "Impostor Roles")
                {
                    List<Type> Roles = [ typeof(Impostor), typeof(Blinder), typeof(Freezer), typeof(Spirit), typeof(Assassin), typeof(Escapist), typeof(Grenadier),
                    typeof(Morphling), typeof(Swooper), typeof(Venerer), typeof(Bomber), typeof(BountyHunter), typeof(Conjurer),
                    typeof(Manipulator), typeof(Poisoner), typeof(Shooter), typeof(Traitor), typeof(Warlock), typeof(Witch), typeof(Blackmailer),
                    typeof(Converter), typeof(Hypnotist), typeof(Janitor), typeof(Mafioso), typeof(Miner), typeof(Reviver),
                    typeof(Undertaker) ];
                    while (Roles.Count > 0)
                    {
                        Type role = Roles[0];
                        var taskAddButton = Object.Instantiate<TaskAddButton>(__instance.RoleButton);
                        taskAddButton.SafePositionWorld = __instance.SafePositionWorld;
                        taskAddButton.Text.text = "<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Masked\">Be_" + role.Name + ".exe";
                        taskAddButton.Role = DestroyableSingleton<CrewmateRole>.Instance;
                        taskAddButton.name = role.Name;
                        if (taskAddButton.Button != null)
                        {
                            __instance.AddFileAsChildCustom(taskAddButton, ref num, ref num2, ref num3);
                            var taskevent = new Button.ButtonClickedEvent();
                            taskevent.AddListener(ChangeRole(role));
                            taskAddButton.Button.OnClick = taskevent;
                        }
                        Roles.Remove(role);
                    }
                }
                if (taskFolder.FolderName == "Neutral Roles")
                {
                    List<Type> Roles = [ typeof(Amnesiac), typeof(GuardianAngel), typeof(Mercenary), typeof(Shifter), typeof(Survivor), typeof(Doomsayer),
                    typeof(Executioner), typeof(Jester), typeof(Phantom), typeof(Troll), typeof(Vulture),
                    typeof(Arsonist), typeof(Attacker), typeof(Terrorist), typeof(Doppelganger), typeof(Infectious), typeof(Maul),
                    typeof(Mutant), typeof(Juggernaut), typeof(Plaguebearer), typeof(Pestilence), typeof(SerialKiller), typeof(SoulCollector), typeof(Glitch),
                    typeof(Vampire) ];
                    while (Roles.Count > 0)
                    {
                        Type role = Roles[0];
                        var taskAddButton = Object.Instantiate<TaskAddButton>(__instance.RoleButton);
                        taskAddButton.SafePositionWorld = __instance.SafePositionWorld;
                        taskAddButton.Text.text = "<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Masked\">Be_" + role.Name + ".exe";
                        taskAddButton.Role = DestroyableSingleton<CrewmateRole>.Instance;
                        taskAddButton.name = role.Name;
                        if (taskAddButton.Button != null)
                        {
                            __instance.AddFileAsChildCustom(taskAddButton, ref num, ref num2, ref num3);
                            var taskevent = new Button.ButtonClickedEvent();
                            taskevent.AddListener(ChangeRole(role));
                            taskAddButton.Button.OnClick = taskevent;
                        }
                        Roles.Remove(role);
                    }
                }
                if (taskFolder.FolderName == "Coven Roles")
                {
                    List<Type> Roles = [ typeof(Coven), typeof(CovenLeader), typeof(HexMaster), typeof(PotionMaster), typeof(Ritualist),
                    typeof(Spiritualist), typeof(VoodooMaster) ];
                    while (Roles.Count > 0)
                    {
                        Type role = Roles[0];
                        var taskAddButton = Object.Instantiate<TaskAddButton>(__instance.RoleButton);
                        taskAddButton.SafePositionWorld = __instance.SafePositionWorld;
                        taskAddButton.Text.text = "<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Masked\">Be_" + role.Name + ".exe";
                        taskAddButton.Role = DestroyableSingleton<CrewmateRole>.Instance;
                        taskAddButton.name = role.Name;
                        if (taskAddButton.Button != null)
                        {
                            __instance.AddFileAsChildCustom(taskAddButton, ref num, ref num2, ref num3);
                            var taskevent = new Button.ButtonClickedEvent();
                            taskevent.AddListener(ChangeRole(role));
                            taskAddButton.Button.OnClick = taskevent;
                        }
                        Roles.Remove(role);
                    }
                }

                foreach (var chip in __instance.ActiveItems)
                {
                    chip.GetComponentInChildren<TextMeshPro>().EnableMasking();
                    chip.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                }

                if (Scroller && Scroller != null)
                {
                    Scroller.CalculateAndSetYBounds(__instance.ActiveItems.Count, 6, 3, 1f);
                    Scroller.SetYBoundsMin(0.0f);
                    Scroller.ScrollToTop();
                }

		        if (__instance.Hierarchy.Count == 1)
		        {
			        ControllerManager.Instance.SetBackButton(__instance.BackButton);
			        return false;
		        }

		        ControllerManager.Instance.SetBackButton(__instance.FolderBackButton);
                return false;
            }
        }
        [HarmonyPatch(typeof(TaskAddButton), nameof(TaskAddButton.Role), MethodType.Setter)]
        public class FixColor
        {
            public static bool Prefix(TaskAddButton __instance)
            {
                if (__instance.Text.text != null)
                {
                    if (__instance.Text.text.Contains("Be_Crewmate.exe")) __instance.FileImage.color = Patches.Colors.Crewmate;
                    else if (__instance.Text.text.Contains("Be_Aurial.exe")) __instance.FileImage.color = Patches.Colors.Aurial;
                    else if (__instance.Text.text.Contains("Be_Guardian.exe")) __instance.FileImage.color = Patches.Colors.Guardian;
                    else if (__instance.Text.text.Contains("Be_Haunter.exe")) __instance.FileImage.color = Patches.Colors.Haunter;
                    else if (__instance.Text.text.Contains("Be_Helper.exe")) __instance.FileImage.color = Patches.Colors.Helper;
                    else if (__instance.Text.text.Contains("Be_Astral.exe")) __instance.FileImage.color = Patches.Colors.Astral;
                    else if (__instance.Text.text.Contains("Be_Captain.exe")) __instance.FileImage.color = Patches.Colors.Captain;
                    else if (__instance.Text.text.Contains("Be_Chameleon.exe")) __instance.FileImage.color = Patches.Colors.Chameleon;
                    else if (__instance.Text.text.Contains("Be_Detective.exe")) __instance.FileImage.color = Patches.Colors.Detective;
                    else if (__instance.Text.text.Contains("Be_Informant.exe")) __instance.FileImage.color = Patches.Colors.Informant;
                    else if (__instance.Text.text.Contains("Be_Investigator.exe")) __instance.FileImage.color = Patches.Colors.Investigator;
                    else if (__instance.Text.text.Contains("Be_Lookout.exe")) __instance.FileImage.color = Patches.Colors.Lookout;
                    else if (__instance.Text.text.Contains("Be_Mystic.exe")) __instance.FileImage.color = Patches.Colors.Mystic;
                    else if (__instance.Text.text.Contains("Be_Oracle.exe")) __instance.FileImage.color = Patches.Colors.Oracle;
                    else if (__instance.Text.text.Contains("Be_Seer.exe")) __instance.FileImage.color = Patches.Colors.Seer;
                    else if (__instance.Text.text.Contains("Be_Snitch.exe")) __instance.FileImage.color = Patches.Colors.Snitch;
                    else if (__instance.Text.text.Contains("Be_Spy.exe")) __instance.FileImage.color = Patches.Colors.Spy;
                    else if (__instance.Text.text.Contains("Be_Tracker.exe")) __instance.FileImage.color = Patches.Colors.Tracker;
                    else if (__instance.Text.text.Contains("Be_Trapper.exe")) __instance.FileImage.color = Patches.Colors.Trapper;
                    else if (__instance.Text.text.Contains("Be_Avenger.exe")) __instance.FileImage.color = Patches.Colors.Avenger;
                    else if (__instance.Text.text.Contains("Be_Deputy.exe")) __instance.FileImage.color = Patches.Colors.Deputy;
                    else if (__instance.Text.text.Contains("Be_Fighter.exe")) __instance.FileImage.color = Patches.Colors.Fighter;
                    else if (__instance.Text.text.Contains("Be_Hunter.exe")) __instance.FileImage.color = Patches.Colors.Hunter;
                    else if (__instance.Text.text.Contains("Be_Jailor.exe")) __instance.FileImage.color = Patches.Colors.Jailor;
                    else if (__instance.Text.text.Contains("Be_Knight.exe")) __instance.FileImage.color = Patches.Colors.Knight;
                    else if (__instance.Text.text.Contains("Be_Sheriff.exe")) __instance.FileImage.color = Patches.Colors.Sheriff;
                    else if (__instance.Text.text.Contains("Be_VampireHunter.exe")) __instance.FileImage.color = Patches.Colors.VampireHunter;
                    else if (__instance.Text.text.Contains("Be_Veteran.exe")) __instance.FileImage.color = Patches.Colors.Veteran;
                    else if (__instance.Text.text.Contains("Be_Vigilante.exe")) __instance.FileImage.color = Patches.Colors.Vigilante;
                    else if (__instance.Text.text.Contains("Be_Altruist.exe")) __instance.FileImage.color = Patches.Colors.Altruist;
                    else if (__instance.Text.text.Contains("Be_Bodyguard.exe")) __instance.FileImage.color = Patches.Colors.Bodyguard;
                    else if (__instance.Text.text.Contains("Be_Crusader.exe")) __instance.FileImage.color = Patches.Colors.Crusader;
                    else if (__instance.Text.text.Contains("Be_Cleric.exe")) __instance.FileImage.color = Patches.Colors.Cleric;
                    else if (__instance.Text.text.Contains("Be_Doctor.exe")) __instance.FileImage.color = Patches.Colors.Doctor;
                    else if (__instance.Text.text.Contains("Be_Medic.exe")) __instance.FileImage.color = Patches.Colors.Medic;
                    else if (__instance.Text.text.Contains("Be_Engineer.exe")) __instance.FileImage.color = Patches.Colors.Engineer;
                    else if (__instance.Text.text.Contains("Be_Plumber.exe")) __instance.FileImage.color = Patches.Colors.Plumber;
                    else if (__instance.Text.text.Contains("Be_Imitator.exe")) __instance.FileImage.color = Patches.Colors.Imitator;
                    else if (__instance.Text.text.Contains("Be_Medium.exe")) __instance.FileImage.color = Patches.Colors.Medium;
                    else if (__instance.Text.text.Contains("Be_Paranoïac.exe")) __instance.FileImage.color = Patches.Colors.Paranoïac;
                    else if (__instance.Text.text.Contains("Be_Politician.exe")) __instance.FileImage.color = Patches.Colors.Politician;
                    else if (__instance.Text.text.Contains("Be_Mayor.exe")) __instance.FileImage.color = Patches.Colors.Mayor;
                    else if (__instance.Text.text.Contains("Be_Prosecutor.exe")) __instance.FileImage.color = Patches.Colors.Prosecutor;
                    else if (__instance.Text.text.Contains("Be_TimeLord.exe")) __instance.FileImage.color = Patches.Colors.TimeLord;
                    else if (__instance.Text.text.Contains("Be_Swapper.exe")) __instance.FileImage.color = Patches.Colors.Swapper;
                    else if (__instance.Text.text.Contains("Be_Transporter.exe")) __instance.FileImage.color = Patches.Colors.Transporter;
                    else if (__instance.Text.text.Contains("Be_Warden.exe")) __instance.FileImage.color = Patches.Colors.Warden;
                    else if (__instance.Text.text.Contains("Be_Amnesiac.exe")) __instance.FileImage.color = Patches.Colors.Amnesiac;
                    else if (__instance.Text.text.Contains("Be_GuardianAngel.exe")) __instance.FileImage.color = Patches.Colors.GuardianAngel;
                    else if (__instance.Text.text.Contains("Be_Mercenary.exe")) __instance.FileImage.color = Patches.Colors.Mercenary;
                    else if (__instance.Text.text.Contains("Be_Shifter.exe")) __instance.FileImage.color = Patches.Colors.Shifter;
                    else if (__instance.Text.text.Contains("Be_Survivor.exe")) __instance.FileImage.color = Patches.Colors.Survivor;
                    else if (__instance.Text.text.Contains("Be_Doomsayer.exe")) __instance.FileImage.color = Patches.Colors.Doomsayer;
                    else if (__instance.Text.text.Contains("Be_Executioner.exe")) __instance.FileImage.color = Patches.Colors.Executioner;
                    else if (__instance.Text.text.Contains("Be_Jester.exe")) __instance.FileImage.color = Patches.Colors.Jester;
                    else if (__instance.Text.text.Contains("Be_Phantom.exe")) __instance.FileImage.color = Patches.Colors.Phantom;
                    else if (__instance.Text.text.Contains("Be_SoulCollector.exe")) __instance.FileImage.color = Patches.Colors.SoulCollector;
                    else if (__instance.Text.text.Contains("Be_Troll.exe")) __instance.FileImage.color = Patches.Colors.Troll;
                    else if (__instance.Text.text.Contains("Be_Vulture.exe")) __instance.FileImage.color = Patches.Colors.Vulture;
                    else if (__instance.Text.text.Contains("Be_Arsonist.exe")) __instance.FileImage.color = Patches.Colors.Arsonist;
                    else if (__instance.Text.text.Contains("Be_Attacker.exe")) __instance.FileImage.color = Patches.Colors.Attacker;
                    else if (__instance.Text.text.Contains("Be_Terrorist.exe")) __instance.FileImage.color = Patches.Colors.Terrorist;
                    else if (__instance.Text.text.Contains("Be_Doppelganger.exe")) __instance.FileImage.color = Patches.Colors.Doppelganger;
                    else if (__instance.Text.text.Contains("Be_Infectious.exe")) __instance.FileImage.color = Patches.Colors.Infectious;
                    else if (__instance.Text.text.Contains("Be_Maul.exe")) __instance.FileImage.color = Patches.Colors.Werewolf;
                    else if (__instance.Text.text.Contains("Be_Mutant.exe")) __instance.FileImage.color = Patches.Colors.Mutant;
                    else if (__instance.Text.text.Contains("Be_Juggernaut.exe")) __instance.FileImage.color = Patches.Colors.Juggernaut;
                    else if (__instance.Text.text.Contains("Be_Plaguebearer.exe")) __instance.FileImage.color = Patches.Colors.Plaguebearer;
                    else if (__instance.Text.text.Contains("Be_Pestilence.exe")) __instance.FileImage.color = Patches.Colors.Pestilence;
                    else if (__instance.Text.text.Contains("Be_SerialKiller.exe")) __instance.FileImage.color = Patches.Colors.SerialKiller;
                    else if (__instance.Text.text.Contains("Be_Glitch.exe")) __instance.FileImage.color = Patches.Colors.Glitch;
                    else if (__instance.Text.text.Contains("Be_Vampire.exe")) __instance.FileImage.color = Patches.Colors.Vampire;
                    else if (__instance.Text.text.Contains("Be_Impostor.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Blinder.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Freezer.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Spirit.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Assassin.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Escapist.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Grenadier.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Morphling.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Swooper.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Venerer.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Bomber.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_BountyHunter.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Conjurer.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Manipulator.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Poisoner.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Shooter.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Traitor.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Warlock.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Witch.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Blackmailer.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Converter.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Hypnotist.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Janitor.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Mafioso.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Miner.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Reviver.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Undertaker.exe")) __instance.FileImage.color = Patches.Colors.Impostor;
                    else if (__instance.Text.text.Contains("Be_Coven.exe")) __instance.FileImage.color = Patches.Colors.Coven;
                    else if (__instance.Text.text.Contains("Be_CovenLeader.exe")) __instance.FileImage.color = Patches.Colors.Coven;
                    else if (__instance.Text.text.Contains("Be_HexMaster.exe")) __instance.FileImage.color = Patches.Colors.Coven;
                    else if (__instance.Text.text.Contains("Be_PotionMaster.exe")) __instance.FileImage.color = Patches.Colors.Coven;
                    else if (__instance.Text.text.Contains("Be_Ritualist.exe")) __instance.FileImage.color = Patches.Colors.Coven;
                    else if (__instance.Text.text.Contains("Be_Spiritualist.exe")) __instance.FileImage.color = Patches.Colors.Coven;
                    else if (__instance.Text.text.Contains("Be_VoodooMaster.exe")) __instance.FileImage.color = Patches.Colors.Coven;
                }

                __instance.RolloverHandler.OutColor = __instance.FileImage.color;
                return false;
            }
        }
        [HarmonyPatch(typeof(TaskAddButton), nameof(TaskAddButton.Update))]
        public class ButtonUpdate
        {
            public static bool Prefix(TaskAddButton __instance)
            {
                if (__instance.FileImage == null) return true;
                __instance.Overlay.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                __instance.Overlay.enabled = Role.GetRole(PlayerControl.LocalPlayer) != null && Role.GetRole(PlayerControl.LocalPlayer).GetType().Name == __instance.name;
                return false;
            }
        }
        [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoleOnDeath))]
        public class PreventVanillaGA
        {
            public static bool Prefix()
            {
                return false;
            }
        }
        [HarmonyPatch(typeof(TaskAdderGame), nameof(TaskAdderGame.Begin))]
        public class AddRolesFolder
        {
            public static void Postfix(TaskAdderGame __instance)
            {
                GameObject inner = new("Inner");
                inner.transform.SetParent(__instance.TaskParent.transform, false);

                Scroller = __instance.TaskParent.gameObject.AddComponent<Scroller>();
                Scroller.allowX = false;
                Scroller.allowY = true;
                Scroller.Inner = inner.transform;

                GameObject hitbox = new("Hitbox")
                {
                    layer = 5,
                };
                hitbox.transform.SetParent(__instance.TaskParent.transform, false);
                hitbox.transform.localScale = new Vector3(7.5f, 6.5f, 1);
                hitbox.transform.localPosition = new Vector3(2.8f, -2.2f, 0);

                var mask = hitbox.AddComponent<SpriteMask>();
                mask.sprite = TownOfUsEdited.NextButton;
                mask.alphaCutoff = 0.0f;

                var collider = hitbox.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(1f, 1f);
                collider.enabled = true;

                Scroller.ClickMask = collider;

                __instance.TaskPrefab.GetComponent<PassiveButton>().ClickMask = collider;
                __instance.RoleButton.GetComponent<PassiveButton>().ClickMask = collider;
                __instance.RootFolderPrefab.GetComponent<PassiveButton>().ClickMask = collider;
                __instance.RootFolderPrefab.gameObject.SetActive(false);

                __instance.TaskParent = inner.transform;

                var rolesFolder = Object.Instantiate(__instance.RootFolderPrefab, Scroller.Inner);
                rolesFolder.gameObject.SetActive(false);
                rolesFolder.FolderName = "Crewmate Roles";
                rolesFolder.name = "CrewsFolder";
                rolesFolder.currentFolderColor = Palette.CrewmateBlue;
                rolesFolder.folderSpriteRenderer.color = rolesFolder.currentFolderColor;
		        rolesFolder.buttonRolloverHandler.OutColor = rolesFolder.currentFolderColor;
		        rolesFolder.buttonRolloverHandler.UnselectedColor = rolesFolder.currentFolderColor;

                var rolesFolder2 = Object.Instantiate(__instance.RootFolderPrefab, Scroller.Inner);
                rolesFolder2.gameObject.SetActive(false);
                rolesFolder2.FolderName = "Impostor Roles";
                rolesFolder2.name = "ImpsFolder";
                rolesFolder2.currentFolderColor = Palette.ImpostorRed;
                rolesFolder2.folderSpriteRenderer.color = rolesFolder2.currentFolderColor;
		        rolesFolder2.buttonRolloverHandler.OutColor = rolesFolder2.currentFolderColor;
		        rolesFolder2.buttonRolloverHandler.UnselectedColor = rolesFolder2.currentFolderColor;

                var rolesFolder3 = Object.Instantiate(__instance.RootFolderPrefab, Scroller.Inner);
                rolesFolder3.gameObject.SetActive(false);
                rolesFolder3.FolderName = "Neutral Roles";
                rolesFolder3.name = "NeutsFolder";
                rolesFolder3.currentFolderColor = Color.gray;
                rolesFolder3.folderSpriteRenderer.color = rolesFolder3.currentFolderColor;
		        rolesFolder3.buttonRolloverHandler.OutColor = rolesFolder3.currentFolderColor;
		        rolesFolder3.buttonRolloverHandler.UnselectedColor = rolesFolder3.currentFolderColor;

                var rolesFolder4 = Object.Instantiate(__instance.RootFolderPrefab, Scroller.Inner);
                rolesFolder4.gameObject.SetActive(false);
                rolesFolder4.FolderName = "Coven Roles";
                rolesFolder4.name = "CovensFolder";
                rolesFolder4.currentFolderColor = Colors.Coven;
                rolesFolder4.folderSpriteRenderer.color = rolesFolder4.currentFolderColor;
		        rolesFolder4.buttonRolloverHandler.OutColor = rolesFolder4.currentFolderColor;
		        rolesFolder4.buttonRolloverHandler.UnselectedColor = rolesFolder4.currentFolderColor;

                __instance.Root.SubFolders.Add(rolesFolder);
                __instance.Root.SubFolders.Add(rolesFolder2);
                __instance.Root.SubFolders.Add(rolesFolder3);
                __instance.Root.SubFolders.Add(rolesFolder4);

                __instance.GoToRoot();
            }
        }
        private static void AddFileAsChildCustom(
        this TaskAdderGame instance,
        TaskAddButton item,
        ref float xCursor,
        ref float yCursor,
        ref float maxHeight)
        {
            item.transform.SetParent(instance.TaskParent);
            item.transform.localPosition = new Vector3(xCursor, yCursor, 0f);
            item.transform.localScale = Vector3.one;
            maxHeight = Mathf.Max(maxHeight, item.Text.bounds.size.y + 1.3f);
            xCursor += instance.fileWidth;
            if (xCursor > instance.lineWidth)
            {
                xCursor = 0f;
                yCursor -= maxHeight;
                maxHeight = 0f;
            }

            instance.ActiveItems.Add(item.transform);
        }
        [HarmonyPatch(typeof(TaskAdderGame), nameof(TaskAdderGame.PopulateRoot))]
        public class TaskAdderRolesPatch
        {
            public static bool Prefix([HarmonyArgument(0)] TaskAdderGame.FolderType folderType)
            {
                if (folderType == TaskAdderGame.FolderType.Roles) return false;
                return true;
            }
        }
        [HarmonyPatch(typeof(TutorialManager), nameof(TutorialManager.Awake))]
        public class StartFreeplayPatch
        {
            public static void Postfix()
            {
                Coroutines.Start(Reset());
            }
            public static IEnumerator Reset()
            {
                while (!ShipStatus.Instance)
                {
                    yield return null;
                }
                while (!PlayerControl.LocalPlayer)
                {
                    yield return null;
                }
                while (!GameManager.Instance)
                {
                    yield return null;
                }
                yield return new WaitForSeconds(0.1f);
                Role.RoleHistory.Clear();
                Modifier.ModifierDictionary.Clear();
                Ability.AbilityDictionary.Clear();
                foreach (var player in PlayerControl.AllPlayerControls.ToArray().Where(x => x != PlayerControl.LocalPlayer).ToList())
                {
                    if (Role.RoleDictionary.ContainsKey(player.PlayerId)) Role.RoleDictionary.Remove(player.PlayerId);
                    new Crewmate(player);
                }
                ApplyRoleChange(typeof(Crewmate), true);
                Utils.ShowDeadBodies = false;
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
                Murder.KilledPlayers.Clear();
                PerformRewind.Revived.Clear();
                KillButtonTarget.DontRevive = byte.MaxValue;
                ConverterHudManagerUpdate.DontRevive = byte.MaxValue;
                HudUpdate.Zooming = false;
                HudUpdate.ZoomStart();
                yield break;
            }
        }
    }
}