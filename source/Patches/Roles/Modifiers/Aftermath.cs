using Reactor.Utilities;
using System;
using System.Collections;
using TownOfUsEdited.Extensions;
using TownOfUsEdited.ImpostorRoles.BomberMod;
using UnityEngine;
using TownOfUsEdited.Modifiers.UnderdogMod;
using Object = UnityEngine.Object;
using TownOfUsEdited.ImpostorRoles.ImpostorMod;
using Hazel;
using System.Linq;
using TownOfUsEdited.Patches;
using TownOfUsEdited.ImpostorRoles.MorphlingMod;
using TownOfUsEdited.ImpostorRoles.SwooperMod;

namespace TownOfUsEdited.Roles.Modifiers
{
    public class Aftermath : Modifier
    {
        public Aftermath(PlayerControl player) : base(player)
        {
            Name = "Aftermath";
            TaskText = () => "Force your killer to use their ability";
            Color = Patches.Colors.Aftermath;
            ModifierType = ModifierEnum.Aftermath;
        }

        public static void ForceAbility(PlayerControl player, PlayerControl corpse)
        {
            if (!player.AmOwner) return;
            DeadBody db = null;
            var bodies = Object.FindObjectsOfType<DeadBody>();
            foreach (var body in bodies)
            {
                try
                {
                    if (body?.ParentId == corpse.PlayerId) { db = body; break; }
                }
                catch
                {
                }
            }
            Coroutines.Start(delay(player, corpse, db));
        }

        private static IEnumerator delay(PlayerControl player, PlayerControl corpse, DeadBody db)
        {
            yield return new WaitForSecondsRealtime(0.2f);
            var role = Role.GetRole(player);

            if (role is Blackmailer blackmailer)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    blackmailer.Blackmailed?.myRend().material.SetFloat("_Outline", 0f);
                    if (blackmailer.Blackmailed != null && blackmailer.Blackmailed.Data.IsImpostor())
                    {
                        if (blackmailer.Blackmailed.GetCustomOutfitType() != CustomPlayerOutfitType.Camouflage &&
                            blackmailer.Blackmailed.GetCustomOutfitType() != CustomPlayerOutfitType.Swooper)
                            blackmailer.Blackmailed.nameText().color = Patches.Colors.Impostor;
                        else blackmailer.Blackmailed.nameText().color = Color.clear;
                    }
                    blackmailer.Blackmailed = player;
                    Utils.Rpc(CustomRPC.Blackmail, player.PlayerId, player.PlayerId, (byte)1);
                }
                else Utils.Rpc(CustomRPC.Blackmail, player.PlayerId, player.PlayerId, (byte)0);
            }
            else if (role is Glitch glitch)
            {
                if (glitch.Player.GetCustomOutfitType() != CustomPlayerOutfitType.Morph) glitch.RpcSetMimicked(corpse);
            }
            else if (role is Escapist escapist)
            {
                if (escapist.EscapePoint != new Vector3(0f, 0f, 0f))
                {
                    Utils.Rpc(CustomRPC.Escape, PlayerControl.LocalPlayer.PlayerId, escapist.EscapePoint);
                    escapist.Cooldown = CustomGameOptions.MineCd;
                    Escapist.Escape(escapist.Player);
                }
            }
            else if (role is Grenadier grenadier)
            {
                if (!grenadier.Enabled)
                {
                    grenadier.TimeRemaining = CustomGameOptions.GrenadeDuration;
                    grenadier.StartFlash();

                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    254, SendOption.Reliable, -1);
                    writer.Write((int)CustomRPC.FlashGrenade);
                    writer.Write((byte)grenadier.Player.PlayerId);
                    writer.Write((byte)grenadier.flashedPlayers.Count);
                    foreach (var flashed in grenadier.flashedPlayers)
                    {
                        writer.Write(flashed.PlayerId);
                    }
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            }
            else if (role is Janitor janitor)
            {
                Utils.Rpc(CustomRPC.JanitorClean, PlayerControl.LocalPlayer.PlayerId, db.ParentId);

                Coroutines.Start(ImpostorRoles.JanitorMod.Coroutine.CleanCoroutine(db, janitor));
            }
            else if (role is Mutant mutant)
            {
                if (mutant.IsTransformed)
                {
                    Utils.Rpc(CustomRPC.UnTransform, mutant.Player.PlayerId);

                    mutant.Player.MyPhysics.SetBodyType(PlayerBodyTypes.Normal);
                    Utils.Rpc(CustomRPC.UnTransform, mutant.Player.PlayerId);
                    mutant.IsTransformed = false;
                    mutant.Cooldown = CustomGameOptions.MutantKCD;
                    mutant.TransformCooldown = CustomGameOptions.TransformCD;
                }
                else
                {
                    mutant.Player.MyPhysics.SetBodyType(PlayerBodyTypes.Seeker);
                    Coroutines.Start(Utils.FlashCoroutine(Patches.Colors.Mutant, 0.5f));
                    Utils.Rpc(CustomRPC.Transform, mutant.Player.PlayerId);
                    mutant.IsTransformed = true;
                    mutant.Cooldown = CustomGameOptions.MutantKCD;
                }
            }
            else if (role is Miner miner)
            {
                var hits = Physics2D.OverlapBoxAll(PlayerControl.LocalPlayer.transform.position, miner.VentSize, 0);
                hits = hits.ToArray().Where(c =>
                        (c.name.Contains("Vent") || !c.isTrigger) && c.gameObject.layer != 8 && c.gameObject.layer != 5)
                    .ToArray();
                if (hits.Count == 0 && !SubmergedCompatibility.GetPlayerElevator(PlayerControl.LocalPlayer).Item1)
                {
                    var position = PlayerControl.LocalPlayer.transform.position;
                    var id = ImpostorRoles.MinerMod.PlaceVent.GetAvailableId();
                    Utils.Rpc(CustomRPC.Mine, id, PlayerControl.LocalPlayer.PlayerId, position, position.z + 0.0004f);
                    ImpostorRoles.MinerMod.PlaceVent.SpawnVent(id, miner, position, position.z + 0.0004f);
                    miner.Cooldown = CustomGameOptions.MineCd;
                }
            }
            else if (role is Converter converter)
            {
                var dead = Utils.PlayerById(db.ParentId);
                converter.ConvertAbility(db);
            }
            else if (role is Morphling morphling)
            {
                if (morphling.Player.GetCustomOutfitType() != CustomPlayerOutfitType.Morph)
                {
                    var shapeshifter = RoleManager.Instance.GetRole(AmongUs.GameOptions.RoleTypes.Shapeshifter).Cast<ShapeshifterRole>();
                    Sprite MorphSprite = shapeshifter.Ability.Image;
                    if (morphling.SampledPlayer == null) morphling._morphButton.graphic.sprite = MorphSprite;
                    morphling.SampledPlayer = corpse;
                    morphling.MorphedPlayer = corpse;
                    Utils.Rpc(CustomRPC.Morph, PlayerControl.LocalPlayer.PlayerId, morphling.SampledPlayer.PlayerId);
                    Coroutines.Start(PerformKillMorphling.Morph(morphling));
                }
            }
            else if (role is Swooper swooper)
            {
                if (swooper.Player.GetCustomOutfitType() != CustomPlayerOutfitType.Swooper)
                {
                    Utils.Rpc(CustomRPC.Swoop, PlayerControl.LocalPlayer.PlayerId);
                    Coroutines.Start(PerformKillSwooper.Swoop(swooper));
                }
            }
            else if (role is Poisoner poisoner)
            {
                if (poisoner.PoisonedPlayer != null) poisoner.PoisonedPlayer = null;
                poisoner.PoisonedPlayer = poisoner.Player;
                poisoner.TimeRemaining = CustomGameOptions.PoisonDuration;
                Utils.Rpc(CustomRPC.Poison, PlayerControl.LocalPlayer.PlayerId, poisoner.Player.PlayerId);
            }
            else if (role is Undertaker undertaker)
            {
                if (undertaker.CurrentlyDragging)
                {
                    Vector3 position = PlayerControl.LocalPlayer.transform.position;

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

                    Utils.Rpc(CustomRPC.Drop, PlayerControl.LocalPlayer.PlayerId, position, position.z);

                    var body = undertaker.CurrentlyDragging;
                    undertaker.CurrentlyDragging = null;
                    body.transform.position = position;
                }

                Utils.Rpc(CustomRPC.Drag, PlayerControl.LocalPlayer.PlayerId, db.ParentId);
                undertaker.CurrentlyDragging = db;
                ImpostorRoles.UndertakerMod.KillButtonTarget.SetTarget(undertaker._dragDropButton, null, undertaker);
                undertaker._dragDropButton.graphic.sprite = TownOfUsEdited.DropSprite;
            }
            else if (role is Venerer venerer)
            {
                if (!venerer.Enabled)
                {
                    Utils.Rpc(CustomRPC.Camouflage, PlayerControl.LocalPlayer.PlayerId, venerer.Kills);
                    venerer.TimeRemaining = CustomGameOptions.AbilityDuration;
                    venerer.Ability();
                }
            }
            else if (role is Bomber bomber)
            {
                bomber.Detonated = false;
                var pos = PlayerControl.LocalPlayer.transform.position;
                pos.z += 0.001f;
                bomber.DetonatePoint = pos;
                bomber.PlantButton.graphic.sprite = TownOfUsEdited.DetonateSprite;
                bomber.TimeRemaining = CustomGameOptions.DetonateDelay;
                bomber.PlantButton.SetCoolDown(bomber.TimeRemaining, CustomGameOptions.DetonateDelay);
                if (PlayerControl.LocalPlayer.Is(ModifierEnum.Underdog))
                {
                    var lowerKC = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown - CustomGameOptions.UnderdogKillBonus + CustomGameOptions.DetonateDelay;
                    var normalKC = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown + CustomGameOptions.DetonateDelay;
                    var upperKC = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown + CustomGameOptions.UnderdogKillBonus + CustomGameOptions.DetonateDelay;
                    Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = PerformKill.LastImp() ? lowerKC : (PerformKill.IncreasedKC() ? normalKC : upperKC);
                }
                else if (PlayerControl.LocalPlayer.Is(ModifierEnum.Lucky))
                {
                    var num = UnityEngine.Random.RandomRange(1f, 60f);
                    Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = num;
                }
                else Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown + CustomGameOptions.DetonateDelay;
                DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
                bomber.Bomb = BombExtentions.CreateBomb(pos);
            }
        }
    }
}