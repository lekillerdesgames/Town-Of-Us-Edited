using HarmonyLib;
using TownOfUsEdited.Roles;
using UnityEngine;
using TownOfUsEdited.CrewmateRoles.InvestigatorMod;
using TownOfUsEdited.CrewmateRoles.TrapperMod;
using System.Collections.Generic;
using TownOfUsEdited.CrewmateRoles.DeputyMod;
using TownOfUsEdited.CrewmateRoles.ImitatorMod;
using TownOfUsEdited.CrewmateRoles.MayorMod;
using TownOfUsEdited.CrewmateRoles.PoliticianMod;
using TownOfUsEdited.CrewmateRoles.ProsecutorMod;
using TownOfUsEdited.CrewmateRoles.SwapperMod;
using TownOfUsEdited.CrewmateRoles.VigilanteMod;
using TownOfUsEdited.ImpostorRoles.HypnotistMod;
using TownOfUsEdited.Modifiers.AssassinMod;
using TownOfUsEdited.NeutralRoles.DoomsayerMod;
using AddButton = TownOfUsEdited.CrewmateRoles.SwapperMod.AddButton;
using TownOfUsEdited.CovenRoles.RitualistMod;
using TownOfUsEdited.Roles.AssassinMod;

namespace TownOfUsEdited.Patches
{

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    class AddMeetingButtons
    {
        public static void Prefix(MeetingHud __instance)
        {
            if (StartImitate.ImitatingPlayer != null && !StartImitate.ImitatingPlayer.Is(RoleEnum.Traitor))
            {
                List<RoleEnum> trappedPlayers = null;
                PlayerControl confessingPlayer = null;

                if (PlayerControl.LocalPlayer == StartImitate.ImitatingPlayer)
                {
                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Investigator)) Footprint.DestroyAll(Role.GetRole<Investigator>(PlayerControl.LocalPlayer));

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Engineer))
                    {
                        var engineerRole = Role.GetRole<Engineer>(PlayerControl.LocalPlayer);
                        Object.Destroy(engineerRole.UsesText);
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Tracker))
                    {
                        var trackerRole = Role.GetRole<Tracker>(PlayerControl.LocalPlayer);
                        trackerRole.TrackerArrows.Values.DestroyAll();
                        trackerRole.TrackerArrows.Clear();
                        Object.Destroy(trackerRole.UsesText);
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.VampireHunter))
                    {
                        var vhRole = Role.GetRole<VampireHunter>(PlayerControl.LocalPlayer);
                        Object.Destroy(vhRole.UsesText);
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Transporter))
                    {
                        var transporterRole = Role.GetRole<Transporter>(PlayerControl.LocalPlayer);
                        Object.Destroy(transporterRole.UsesText);
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Veteran))
                    {
                        var veteranRole = Role.GetRole<Veteran>(PlayerControl.LocalPlayer);
                        Object.Destroy(veteranRole.UsesText);
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Trapper))
                    {
                        var trapperRole = Role.GetRole<Trapper>(PlayerControl.LocalPlayer);
                        Object.Destroy(trapperRole.UsesText);
                        trapperRole.traps.ClearTraps();
                        trappedPlayers = trapperRole.trappedPlayers;
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Oracle))
                    {
                        var oracleRole = Role.GetRole<Oracle>(PlayerControl.LocalPlayer);
                        confessingPlayer = oracleRole.Confessor;
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Detective))
                    {
                        var detecRole = Role.GetRole<Detective>(PlayerControl.LocalPlayer);
                        detecRole.ClosestPlayer = null;
                        detecRole.ExamineButton.gameObject.SetActive(false);
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Knight))
                    {
                        var knightRole = Role.GetRole<Knight>(PlayerControl.LocalPlayer);
                        UnityEngine.Object.Destroy(knightRole.UsesText);
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Doctor))
                    {
                        var docRole = Role.GetRole<Doctor>(PlayerControl.LocalPlayer);
                        UnityEngine.Object.Destroy(docRole.UsesText);
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Aurial))
                    {
                        var aurialRole = Role.GetRole<Aurial>(PlayerControl.LocalPlayer);
                        aurialRole.SenseArrows.Values.DestroyAll();
                        aurialRole.SenseArrows.Clear();
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Politician))
                    {
                        var politicianRole = Role.GetRole<Politician>(PlayerControl.LocalPlayer);
                        politicianRole.ClosestPlayer = null;
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Jailor))
                    {
                        var jailorRole = Role.GetRole<Jailor>(PlayerControl.LocalPlayer);
                        jailorRole.ClosestPlayer = null;
                    }

                    try
                    {
                        DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
                    }
                    catch { }

                    if (!PlayerControl.LocalPlayer.Is(RoleEnum.Investigator) && !PlayerControl.LocalPlayer.Is(RoleEnum.Mystic)
                        && !PlayerControl.LocalPlayer.Is(RoleEnum.Spy)) DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
                }

                if (StartImitate.ImitatingPlayer.Is(RoleEnum.Medium))
                {
                    var medRole = Role.GetRole<Medium>(StartImitate.ImitatingPlayer);
                    medRole.MediatedPlayers.Values.DestroyAll();
                    medRole.MediatedPlayers.Clear();
                }

                if (StartImitate.ImitatingPlayer.Is(RoleEnum.Snitch))
                {
                    var snitchRole = Role.GetRole<Snitch>(StartImitate.ImitatingPlayer);
                    snitchRole.SnitchArrows.Values.DestroyAll();
                    snitchRole.SnitchArrows.Clear();
                    snitchRole.ImpArrows.DestroyAll();
                    snitchRole.ImpArrows.Clear();
                }

                var role = Role.GetRole(StartImitate.ImitatingPlayer);
                var killsList = (role.Kills, role.CorrectKills, role.IncorrectKills, role.CorrectAssassinKills, role.IncorrectAssassinKills);
                Role.RoleDictionary.Remove(StartImitate.ImitatingPlayer.PlayerId);
                var imitator = new Imitator(StartImitate.ImitatingPlayer);
                imitator.trappedPlayers = trappedPlayers;
                imitator.confessingPlayer = confessingPlayer;
                var newRole = Role.GetRole(StartImitate.ImitatingPlayer);
                newRole.RemoveFromRoleHistory(newRole.RoleType);
                newRole.Kills = killsList.Kills;
                newRole.CorrectKills = killsList.CorrectKills;
                newRole.IncorrectKills = killsList.IncorrectKills;
                newRole.CorrectAssassinKills = killsList.CorrectAssassinKills;
                newRole.IncorrectAssassinKills = killsList.IncorrectAssassinKills;
                newRole.DeathReason = role.DeathReason;
                Role.GetRole<Imitator>(StartImitate.ImitatingPlayer).ImitatePlayer = null;
                StartImitate.ImitatingPlayer = null;
                Utils.Unmorph(imitator.Player);
            }
            AddButtonDeputy.AddDepButtons(__instance);
            AddButtonImitator.AddImitatorButtons(__instance);
            AddRevealButton.AddMayorButtons(__instance);
            AddRevealButtonPolitician.AddPoliticianButtons(__instance);
            AddProsecute.AddProsecuteButton(__instance);
            AddButton.AddSwapperButtons(__instance);
            AddButtonRitualist.AddRitualistButtons(__instance);
            AddButtonVigi.AddVigilanteButtons(__instance);
            AddHysteriaButton.AddHypnoButtons(__instance);
            AddButtonAssassin.AddAssassinButtons(__instance);
            AddButtonAssassin2.AddAssassinButtons(__instance);
            AddButtonDoom.AddDoomsayerButtons(__instance);
            return;
        }
    }
}
