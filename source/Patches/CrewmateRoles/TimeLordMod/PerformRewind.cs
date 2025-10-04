using System;
using System.Linq;
using HarmonyLib;
using Object = UnityEngine.Object;
using Reactor.Utilities;
using TownOfUsEdited.CrewmateRoles.MedicMod;
using TownOfUsEdited.Patches;
using TownOfUsEdited.Roles;
using AmongUs.GameOptions;
using UnityEngine;
using System.Collections.Generic;

namespace TownOfUsEdited.CrewmateRoles.TimeLordMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformRewind

    {
        public static List<byte> Revived = new List<byte>();
        public static bool Prefix(KillButton __instance)
        {
            if (__instance != DestroyableSingleton<HudManager>.Instance.KillButton) return true;
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.TimeLord);
            if (!flag) return true;
            var role = Role.GetRole<TimeLord>(PlayerControl.LocalPlayer);
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            if (role.Cooldown > 0) return false;
            if (role.Rewinding) return false;
            if (!__instance.enabled) return false;
            var abilityUsed = Utils.AbilityUsed(PlayerControl.LocalPlayer);
            if (!abilityUsed) return false;

            Rewind(role);
            Utils.Rpc(CustomRPC.Rewind, PlayerControl.LocalPlayer.PlayerId);
            return false;
        }

        public static void Rewind(TimeLord role)
        {
            //System.Console.WriteLine("START...");
            role.TimeRemaining = CustomGameOptions.RewindDuration;
            role.Rewind();
            Coroutines.Start(Utils.FlashCoroutine(Colors.TimeLord, CustomGameOptions.RewindDuration));
            if (Minigame.Instance)
                try
                {
                    Minigame.Instance.Close();
                    Minigame.Instance.Close();
                }
                catch
                {
                }
            bool usingSpecialAnimation = PlayerControl.LocalPlayer.onLadder || PlayerControl.LocalPlayer.inMovingPlat ||
            PlayerControl.LocalPlayer.NetTransform.IsInMiddleOfAnimationThatMakesPlayerInvisible();
            if (!usingSpecialAnimation)
            {
                PlayerControl.LocalPlayer.NetTransform.Halt();
                PlayerControl.LocalPlayer.moveable = false;
            }
            else
            {
                TimeLordPatches.HudManagerUpdate.WaitForAnimationToFinish = true;
            }
            foreach (var astrals in Role.GetRoles(RoleEnum.Astral))
            {
                var astral = (Astral)astrals;
                if (astral.Enabled)
                {
                    astral.TimeRemaining = 0f;
                    astral.Revive(astral.Player);
                    astral.ResetCD();
                }
            }
        }

        public static void StopDragging(byte playerId)
        {
            foreach (var role in Role.GetRoles(RoleEnum.Undertaker))
            {
                var undertaker = (Undertaker)role;
                if (undertaker.CurrentlyDragging != null && undertaker.CurrentlyDragging.ParentId == playerId)
                {
                    Vector3 position = undertaker.Player.transform.position;

                    if (Patches.SubmergedCompatibility.isSubmerged())
                    {
                        if (position.y > -7f)
                        {
                            position.z = 0.0208f;
                        }
                        else
                        {
                            position.z = -0.0273f;
                        }
                    }

                    position.y -= 0.3636f;

                    var body = undertaker.CurrentlyDragging;
                    if (undertaker.Player == PlayerControl.LocalPlayer)
                    {
                        foreach (var body2 in undertaker.CurrentlyDragging.bodyRenderers) body2.material.SetFloat("_Outline", 0f);
                    }
                    undertaker.CurrentlyDragging = null;

                    body.transform.position = position;
                }
            }
            foreach (var role in Role.GetRoles(RoleEnum.Doctor))
            {
                var doctor = (Doctor)role;
                if (doctor.CurrentlyDragging != null && doctor.CurrentlyDragging.ParentId == playerId)
                {
                    Vector3 position = doctor.Player.transform.position;

                    if (Patches.SubmergedCompatibility.isSubmerged())
                    {
                        if (position.y > -7f)
                        {
                            position.z = 0.0208f;
                        }
                        else
                        {
                            position.z = -0.0273f;
                        }
                    }

                    position.y -= 0.3636f;

                    var body = doctor.CurrentlyDragging;
                    if (doctor.Player == PlayerControl.LocalPlayer)
                    {
                        foreach (var body2 in doctor.CurrentlyDragging.bodyRenderers) body2.material.SetFloat("_Outline", 0f);
                    }
                    doctor.CurrentlyDragging = null;

                    body.transform.position = position;
                }
            }
        }

        public static void Revive(PlayerControl player)
        {
            DeadBody target = null;
            foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>())
            {
                if (deadBody.ParentId == player.PlayerId) target = deadBody;
            }
            if (target != null) StopDragging(target.ParentId);
            var doubleBodies = Murder.KilledPlayers.ToArray().Where(x => x.isDoppel && x.PlayerId == player.PlayerId).ToList();
            if (doubleBodies.Count > 0)
            {
                var doubleBody = doubleBodies.FirstOrDefault();
                foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>())
                {
                    if (deadBody.ParentId == doubleBody.KillerId && deadBody.IsDouble()) target = deadBody;
                }
            }
            if (target == null)
            {
                PluginSingleton<TownOfUsEdited>.Instance.Log.LogWarning("Body is null! Aborting revive...");
                return;
            }
            foreach (var poisoner in Role.GetRoles(RoleEnum.Poisoner))
            {
                var poisonerRole = (Poisoner)poisoner;
                if (poisonerRole.PoisonedPlayer == player) poisonerRole.PoisonedPlayer = null;
            }
            player.Revive();
            RoleManager.Instance.SetRole(player, RoleTypes.Crewmate);
            Revived.Add(player.PlayerId);
            var position = target.TruePosition;

            if (PlayerControl.LocalPlayer == player) player.myTasks.RemoveAt(1);

            var usedPosition = new Vector2(position.x, position.y + 0.3636f);
            player.transform.position = new Vector2(usedPosition.x, usedPosition.y);

            if (Patches.SubmergedCompatibility.isSubmerged() && PlayerControl.LocalPlayer.PlayerId == player.PlayerId)
            {
                Patches.SubmergedCompatibility.ChangeFloor(player.transform.position.y > -7);
            }

            if (target != null)
                Object.Destroy(target.gameObject);

            player.moveable = false;
        }

        public static void StopRewind(TimeLord role)
        {
            //System.Console.WriteLine("STOP...");
            role.StopRewind();
            if (!PlayerControl.LocalPlayer.MyPhysics.myPlayer.onLadder && !PlayerControl.LocalPlayer.inMovingPlat && !PlayerControl.LocalPlayer.inVent) PlayerControl.LocalPlayer.moveable = true;
            Patches.SubmergedCompatibility.CheckOutOfBoundsElevator(PlayerControl.LocalPlayer);
            foreach (var playerid in Revived)
            {
                Murder.KilledPlayers.Remove(
                    Murder.KilledPlayers.FirstOrDefault(x => x.PlayerId == playerid));
            }
            var DecontaDoors = GameObject.FindObjectsOfType<PlainDoor>().Where(x => x.Room == SystemTypes.Decontamination);
            foreach (var decontaDoor in DecontaDoors)
            {
                decontaDoor.SetDoorway(false);
            }
            var mapId = GameOptionsManager.Instance.currentNormalGameOptions?.MapId;
            if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) mapId = (byte)AmongUsClient.Instance.TutorialMapId;
            if (mapId == 1)
            {
                var DecontaDoors2 = GameObject.FindObjectsOfType<ManualDoor>();
                foreach (var manualDoor in DecontaDoors2)
                {
                    manualDoor.SetDoorway(true);
                }
            }
        }
    }
}