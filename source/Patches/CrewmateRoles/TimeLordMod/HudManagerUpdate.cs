using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Reactor.Utilities;
using TownOfUsEdited.Roles;
using TownOfUsEdited.Roles.Modifiers;
using UnityEngine;

namespace TownOfUsEdited.CrewmateRoles.TimeLordMod
{
    public class TimeLordPatches
    {
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public class HudManagerUpdate
        {
            public static Vector2 playerVector2 = Vector2.zero;
            public static void Postfix(HudManager __instance)
            {
                UpdateRewindButton(__instance);
                UpdatePositions();
            }

            public static void UpdateRewindButton(HudManager __instance)
            {
                if (PlayerControl.AllPlayerControls.Count <= 1) return;
                if (PlayerControl.LocalPlayer == null) return;
                if (PlayerControl.LocalPlayer.Data == null) return;
                if (!PlayerControl.LocalPlayer.Is(RoleEnum.TimeLord)) return;
                var rewindButton = DestroyableSingleton<HudManager>.Instance.KillButton;
                var rewindText = __instance.KillButton.buttonLabelText;

                var role = Role.GetRole<TimeLord>(PlayerControl.LocalPlayer);

                rewindButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started ||
                        AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay));
                rewindText.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started ||
                    AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay));
                if (role.UsingRewind)
                {
                    rewindButton.SetCoolDown(role.TimeRemaining, CustomGameOptions.RewindDuration);
                }
                else
                {
                    rewindButton.SetCoolDown(role.RewindTimer(), CustomGameOptions.RewindCooldown);
                }

                var renderer = rewindButton.graphic;
                if (rewindButton.enabled)
                {
                    rewindText.color = Palette.EnabledColor;
                    rewindText.material.SetFloat("_Desat", 0f);
                    renderer.color = Palette.EnabledColor;
                    renderer.material.SetFloat("_Desat", 0f);
                    return;
                }

                rewindText.color = Palette.DisabledClear;
                rewindText.material.SetFloat("_Desat", 1f);
                renderer.color = Palette.DisabledClear;
                renderer.material.SetFloat("_Desat", 1f);
            }

            public static bool Waiting = false;
            public static bool WaitForAnimationToFinish = false;
            private static bool IsPlayingVentAnimation()
            {
                return PlayerControl.LocalPlayer.MyPhysics.Animations.IsPlayingEnterVentAnimation() || PlayerControl.LocalPlayer.walkingToVent; // Took it from CustomNetworkTransform
            }
            public static void UpdatePositions()
            {
                if (PlayerControl.AllPlayerControls.Count <= 1) return;
                if (PlayerControl.LocalPlayer == null) return;
                if (PlayerControl.LocalPlayer.Data == null) return;
                if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started
                && AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay) return;
                bool usingSpecialAnimation = PlayerControl.LocalPlayer.onLadder || PlayerControl.LocalPlayer.inMovingPlat ||
                PlayerControl.LocalPlayer.NetTransform.IsInMiddleOfAnimationThatMakesPlayerInvisible();
                if (WaitForAnimationToFinish && usingSpecialAnimation)
                {
                    return;
                }
                else if (WaitForAnimationToFinish && !usingSpecialAnimation)
                {
                    PlayerControl.LocalPlayer.NetTransform.Halt();
                    PlayerControl.LocalPlayer.moveable = false;
                    WaitForAnimationToFinish = false;
                    return;
                }

                if (PlayerControl.LocalPlayer.Data.IsDead && !isDead && (!PlayerControl.LocalPlayer.Is(RoleEnum.Astral) || !Role.GetRole<Astral>(PlayerControl.LocalPlayer).Enabled))
                {
                    isDead = true;
                    deathTime = Time.time;
                    System.Console.WriteLine($"Death time: {deathTime}");
                }
                else if (!PlayerControl.LocalPlayer.Data.IsDead && isDead) isDead = false;
                if (Utils.Rewinding())
                {
                    var pos = Positions;
                    if (pos.Count > 0)
                    {
                        var DecontaDoors = GameObject.FindObjectsOfType<PlainDoor>().Where(x => x.Room == SystemTypes.Decontamination);
                        foreach (var decontaDoor in DecontaDoors)
                        {
                            decontaDoor.SetDoorway(true);
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
                        PlayerControl.LocalPlayer.moveable = false;
                        var list = pos.Last();
                        var position = list.Item1;
                        var time = list.Item2;
                        var animation = list.Item3;
                        var position2 = list.Item4;
                        var ventId = list.Item5;
                        if (PlayerControl.LocalPlayer.MyPhysics.myPlayer.onLadder || PlayerControl.LocalPlayer.inMovingPlat || Waiting)
                        {
                            if (animation != "None") pos.Remove(list);
                            return;
                        }
                        if (IsPlayingVentAnimation()) return;
                        if (animation != "None" && !PlayerControl.LocalPlayer.Data.IsDead)
                        {
                            if (animation == "Zipline")
                            {
                                var AllZiplines = GameObject.FindObjectsOfType<ZiplineConsole>();
                                var Zipline = AllZiplines.OrderBy(x => Vector3.Distance(x.transform.position, PlayerControl.LocalPlayer.transform.position)).ElementAt(0);
                                Coroutines.Start(UseConsole(null, Zipline));
                            }
                            else if (animation == "Ladder")
                            {
                                var AllLadders = GameObject.FindObjectsOfType<Ladder>();
                                var Ladder = AllLadders.OrderBy(x => Vector3.Distance(x.transform.position, PlayerControl.LocalPlayer.transform.position)).ElementAt(0);
                                Coroutines.Start(UseConsole(Ladder, null));
                            }
                            else if (animation == "DoorOpen" || animation == "DoorClose")
                            {
                                var Door = list.Item6;
                                if (animation == "DoorOpen")
                                {
                                    Door.SetDoorway(true);
                                    System.Console.WriteLine("Reversing Closing Door");
                                }
                                else
                                {
                                    Door.SetDoorway(false);
                                    System.Console.WriteLine("Reversing Open Door");
                                }
                            }
                            else if (animation == "Teleport")
                            {
                                PlayerControl.LocalPlayer.transform.position = position2;
                                PlayerControl.LocalPlayer.NetTransform.SnapTo(position2);
                                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(position2);
                            }
                            else if (animation == "Vent" && !PlayerControl.LocalPlayer.inVent)
                            {
                                var AllVents = ShipStatus.Instance.AllVents;
                                var Vent = AllVents.OrderBy(x => Vector3.Distance(x.transform.position, PlayerControl.LocalPlayer.transform.position)).ElementAt(0);
                                var pos2 = new Vector2(Vent.transform.position.x, Vent.transform.position.y + 0.3636f);

                                PlayerControl.LocalPlayer.transform.position = pos2;
                                PlayerControl.LocalPlayer.NetTransform.SnapTo(pos2);
                                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(pos2);
                                Vent.Use();
                            }
                            else if (PlayerControl.LocalPlayer.inVent)
                            {
                                var AllVents = GameObject.FindObjectsOfType<Vent>();
                                var Vent = AllVents.OrderBy(x => Vector3.Distance(x.transform.position, PlayerControl.LocalPlayer.transform.position)).ElementAt(0);
                                var nextmoves = Positions.ToArray().Where(x => x.Item3 == "VentMove").ToList();
                                if (nextmoves.Count > 0)
                                {
                                    var nextmove = nextmoves.Last();
                                    if (nextmove.Item2 >= time)
                                    {
                                        var movingVent = AllVents.FirstOrDefault(x => x.Id == ventId);
                                        if (!Vent.TryMoveToVent(movingVent, out string error))
                                        {
                                            Logger.GlobalInstance.Warning("Local Player failed to move to vent because of " + error, null);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var AllPlatforms = GameObject.FindObjectsOfType<PlatformConsole>();
                                var Platform = AllPlatforms.FirstOrDefault(); // There is only one so...
                                Coroutines.Start(UseConsole(null, null, Platform));
                            }
                            pos.Remove(list);
                            return;
                        }
                        else if (PlayerControl.LocalPlayer.inVent)
                        {
                            Vent.currentVent.Use();
                        }
                        if (Patches.SubmergedCompatibility.isSubmerged())
                        {
                            Patches.SubmergedCompatibility.ChangeFloor(position2.y > -7);
                        }
                        playerVector2 = new Vector2(-position.x, -position.y);
                        if (isDead && deathTime >= time && PlayerControl.LocalPlayer.Data.IsDead)
                        {
                            PerformRewind.Revive(PlayerControl.LocalPlayer);
                            Utils.Rpc(CustomRPC.RewindRevive, PlayerControl.LocalPlayer.PlayerId);
                            PluginSingleton<TownOfUsEdited>.Instance.Log.LogInfo("Reviving (Rewind)");
                        }
                        pos.Remove(list);
                    }
                    var modifiers = Modifier.GetModifiers(PlayerControl.LocalPlayer);
                    var speedFactor = 1.0f;
                    if (modifiers != null && modifiers.Any())
                    {
                        var modifier = modifiers.FirstOrDefault(x => x is IVisualAlteration);
                        if (modifier is IVisualAlteration alteration)
                        {
                            alteration.TryGetModifiedAppearance(out VisualAppearance appearance);
                            speedFactor = appearance.SpeedFactor;
                        }
                    }
                    PlayerControl.LocalPlayer.MyPhysics.body.velocity = playerVector2 * PlayerControl.LocalPlayer.MyPhysics.TrueSpeed * speedFactor;
                }
                else
                {
                    var toRemove = Positions.ToArray().Where(x => (Time.time - x.Item2) > CustomGameOptions.RewindDuration).ToList();
                    foreach (var list in toRemove)
                    {
                        Positions.Remove(list);
                    }
                }
            }

            public static IEnumerator UseConsole(Ladder Ladder = null, ZiplineConsole Zipline = null, PlatformConsole Platform = null)
            {
                if (Ladder != null)
                {
                    var physics = PlayerControl.LocalPlayer.MyPhysics;
                    physics.RpcClimbLadder(Ladder);
                    Waiting = true;
                    yield return new WaitForSeconds(3);
                    Waiting = false;
                }
                else if (Zipline != null)
                {
                    Zipline.zipline.Use(Zipline.atTop, Zipline);
                    Waiting = true;
                    yield return new WaitForSeconds(6);
                    Waiting = false;
                }
                else if (Platform != null)
                {
                    Platform.Platform.Use();
                    Waiting = true;
                    yield return new WaitForSeconds(5);
                    Waiting = false;
                }
            }
        }

        public static List<(Vector2, float, string, Vector3, int, SomeKindaDoor)> Positions = new List<(Vector2, float, string, Vector3, int, SomeKindaDoor)>();
        public static bool isDead = false;
        public static float deathTime;

        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
        public class InfectedPlayerKeyboardUpdate
        {
            public static void Postfix(KeyboardJoystick __instance)
            {
                if (Utils.Rewinding()) return;
                if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started
                && AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay) return;
                if (PlayerControl.AllPlayerControls.Count <= 1) return;
                if (PlayerControl.LocalPlayer == null) return;
                if (PlayerControl.LocalPlayer.Data == null) return;
                string usingSpecialAnimation = "None";
                if (PlayerControl.LocalPlayer.onLadder)
                {
                    usingSpecialAnimation = "Ladder";
                }
                else if (PlayerControl.LocalPlayer.inMovingPlat && (GameOptionsManager.Instance?.currentNormalGameOptions?.MapId == 5 || AmongUsClient.Instance.TutorialMapId == 5))
                {
                    usingSpecialAnimation = "Zipline";
                }
                else if (PlayerControl.LocalPlayer.inMovingPlat)
                {
                    usingSpecialAnimation = "Platform";
                }
                else if (PlayerControl.LocalPlayer.inVent)
                {
                    usingSpecialAnimation = "Vent";
                }
                Vector2 playerVector = Vector2.zero;
                playerVector.x = 0f;
                playerVector.y = 0f;
                if (PlayerControl.LocalPlayer.Is(RoleEnum.Manipulator) && Role.GetRole<Manipulator>(PlayerControl.LocalPlayer).UsingManipulation)
                {
                    playerVector.Normalize();
                    var position2 = PlayerControl.LocalPlayer.transform.position;

                    var vel2 = playerVector;
                    Positions.Add((vel2, Time.time, usingSpecialAnimation, position2, 0, null));
                    return;
                }
                if (playerVector == Vector2.zero)
                {
                    if (KeyboardJoystick.player.GetButton(40))
                    {
                        playerVector.x = playerVector.x + 1;
                    }
                    if (KeyboardJoystick.player.GetButton(39))
                    {
                        playerVector.x = playerVector.x - 1;
                    }
                    if (KeyboardJoystick.player.GetButton(44))
                    {
                        playerVector.y = playerVector.y + 1;
                    }
                    if (KeyboardJoystick.player.GetButton(42))
                    {
                        playerVector.y = playerVector.y - 1;
                    }
                }
                playerVector.Normalize();
                var position = PlayerControl.LocalPlayer.transform.position;

                var vel = playerVector;
                if (PlayerControl.LocalPlayer.IsFullyManipulated()) vel = PlayerControl.LocalPlayer.MyPhysics.body.velocity;
                Positions.Add((vel, Time.time, usingSpecialAnimation, position, 0, null));
            }
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FlipX), MethodType.Setter)]
        public class PlayerAnimationRewindFix
        {
            public static void Prefix([HarmonyArgument(0)] ref bool value)
            {
                if (!Utils.Rewinding()) return;
                value = !value;
            }
        }

        [HarmonyPatch(typeof(ZiplineConsole), nameof(ZiplineConsole.CanUse))]
        public class FixZiplineConsole
        {
            public static void Postfix([HarmonyArgument(0)] NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse)
            {
                if (!Utils.Rewinding()) return;
                if (pc.IsDead) return;
                canUse = true;
                couldUse = true;
            }
        }

        [HarmonyPatch(typeof(Ladder), nameof(Ladder.CanUse))]
        public class FixLadder
        {
            public static void Postfix([HarmonyArgument(0)] NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse)
            {
                if (!Utils.Rewinding()) return;
                if (pc.IsDead) return;
                canUse = true;
                couldUse = true;
            }
        }

        [HarmonyPatch(typeof(ManualDoor), nameof(ManualDoor.SetDoorway))]
        public class FixManualDoors
        {
            public static void Postfix(ManualDoor __instance, [HarmonyArgument(0)] bool open)
            {
                if (Utils.Rewinding()) return;
                var mapId = GameOptionsManager.Instance.currentNormalGameOptions?.MapId;
                if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) mapId = (byte)AmongUsClient.Instance.TutorialMapId;
                if (mapId == 1) return;
                if (open)
                {
                    Positions.Add((Vector2.zero, Time.time, "DoorClose", PlayerControl.LocalPlayer.transform.position, 0, __instance));
                }
                else
                {
                    Positions.Add((Vector2.zero, Time.time, "DoorOpen", PlayerControl.LocalPlayer.transform.position, 0, __instance));
                }
            }
        }

        [HarmonyPatch(typeof(MushroomWallDoor), nameof(MushroomWallDoor.SetDoorway))]
        public class FixMushroomDoors
        {
            public static void Postfix(MushroomWallDoor __instance, [HarmonyArgument(0)] bool open)
            {
                if (Utils.Rewinding()) return;
                if (open)
                {
                    Positions.Add((Vector2.zero, Time.time, "DoorClose", PlayerControl.LocalPlayer.transform.position, 0, __instance));
                }
                else
                {
                    Positions.Add((Vector2.zero, Time.time, "DoorOpen", PlayerControl.LocalPlayer.transform.position, 0, __instance));
                }
            }
        }

        [HarmonyPatch(typeof(PlainDoor), nameof(PlainDoor.SetDoorway))]
        public class FixPlainDoors
        {
            public static void Postfix(PlainDoor __instance, [HarmonyArgument(0)] bool open)
            {
                if (Utils.Rewinding()) return;
                if (__instance.Room == SystemTypes.Decontamination) return;
                if (open)
                {
                    Positions.Add((Vector2.zero, Time.time, "DoorClose", PlayerControl.LocalPlayer.transform.position, 0, __instance));
                }
                else
                {
                    Positions.Add((Vector2.zero, Time.time, "DoorOpen", PlayerControl.LocalPlayer.transform.position, 0, __instance));
                }
            }
        }

        [HarmonyPatch(typeof(Vent), nameof(Vent.ClickRight))]
        public class VentMoveRight
        {
            public static bool Prefix(Vent __instance)
            {
                if (PlayerControl.LocalPlayer.Data.IsDead) return true;
                if (Utils.Rewinding()) return false;
                string str;
                var position = PlayerControl.LocalPlayer.transform.position;
                if (!__instance.TryMoveToVent(__instance.Right, out str))
                {
                    Logger.GlobalInstance.Warning("Local Player failed to move to Right vent because of " + str, null);
                }
                else Positions.Add((Vector2.zero, Time.time, "VentMove", position, __instance.Id, null));
                return false;
            }
        }

        [HarmonyPatch(typeof(Vent), nameof(Vent.ClickCenter))]
        public class VentMoveCenter
        {
            public static bool Prefix(Vent __instance)
            {
                if (PlayerControl.LocalPlayer.Data.IsDead) return true;
                if (Utils.Rewinding()) return false;
                string str;
                var position = PlayerControl.LocalPlayer.transform.position;
                if (!__instance.TryMoveToVent(__instance.Center, out str))
                {
                    Logger.GlobalInstance.Warning("Local Player failed to move to Center vent because of " + str, null);
                }
                else Positions.Add((Vector2.zero, Time.time, "VentMove", position, __instance.Id, null));
                return false;
            }
        }

        [HarmonyPatch(typeof(Vent), nameof(Vent.ClickLeft))]
        public class VentMoveLeft
        {
            public static bool Prefix(Vent __instance)
            {
                if (PlayerControl.LocalPlayer.Data.IsDead) return true;
                if (Utils.Rewinding()) return false;
                string str;
                var position = PlayerControl.LocalPlayer.transform.position;
                if (!__instance.TryMoveToVent(__instance.Left, out str))
                {
                    Logger.GlobalInstance.Warning("Local Player failed to move to Left vent because of " + str, null);
                }
                else Positions.Add((Vector2.zero, Time.time, "VentMove", position, __instance.Id, null));
                return false;
            }
        }
    }
}