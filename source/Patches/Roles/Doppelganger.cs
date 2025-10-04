using System;
using System.Linq;
using Il2CppSystem.Collections.Generic;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUsEdited.CrewmateRoles.MedicMod;
using TownOfUsEdited.Extensions;
using TownOfUsEdited.Roles.Modifiers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUsEdited.Roles
{
    public class Doppelganger : Role, IVisualAlteration
    {
        public Doppelganger(PlayerControl owner) : base(owner)
        {
            Name = "Doppelganger";
            ImpostorText = () => "Who Is Who?";
            TaskText = () => "Kill everyone and transform into them\nFake Tasks:";
            Color = Patches.Colors.Doppelganger;
            RoleType = RoleEnum.Doppelganger;
            AddToRoleHistory(RoleType);
            Faction = Faction.NeutralKilling;
            Cooldown = CustomGameOptions.DoppelKCD;
        }

        public PlayerControl ClosestPlayer;
        public PlayerControl TransformedPlayer;
        public PlayerControl OldTransformed;
        public List<DeadBody> Bodies { get; set; } = new List<DeadBody>();
        public List<byte> Players { get; set; } = new List<byte>();
        public float Cooldown;
        public bool coolingDown => Cooldown > 0f;
        public bool DoppelgangerWins;

        internal override void NeutralWin(LogicGameFlowNormal __instance)
        {
            if (Player.Data.IsDead || Player.Data.Disconnected) return;

            var alivecrewkiller = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(Faction.Crewmates) && x.IsCrewKiller() && !x.Data.IsDead && !x.Data.Disconnected).ToList();

            if (PlayerControl.AllPlayerControls.ToArray().Count(x => !x.Data.IsDead && !x.Data.Disconnected) <= 2 &&
                    PlayerControl.AllPlayerControls.ToArray().Count(x => !x.Data.IsDead && !x.Data.Disconnected &&
                    (x.Data.IsImpostor() || x.Is(Faction.NeutralKilling) || x.Is(Faction.Coven))) == 1 && (alivecrewkiller.Count <= 0 || !CustomGameOptions.CrewKillersContinue))
            {
                Utils.Rpc(CustomRPC.DoppelgangerWin, Player.PlayerId);
                Wins();
                Utils.EndGame();
                PluginSingleton<TownOfUsEdited>.Instance.Log.LogMessage("GAME OVER REASON: Doppelganger Win");
                return;
            }

            return;
        }

        public void Kill(PlayerControl target)
        {
            // Check if the Doppelganger can kill
            if (Cooldown > 0)
                return;

            Utils.Interact(PlayerControl.LocalPlayer, target, true);

            bool isReviver = target.Is(RoleEnum.Reviver) && Role.GetRole<Reviver>(target).UsedRevive == true;

            if (!isReviver && target.Data.IsDead)
            {
                Transform(PlayerControl.LocalPlayer, target);
                Utils.Rpc(CustomRPC.DoppelMorph, PlayerControl.LocalPlayer.PlayerId, target.PlayerId);
            }

            // Set the last kill time
            Cooldown = CustomGameOptions.DoppelKCD;
        }
        public void Transform(PlayerControl doppel, PlayerControl target)
        {
            foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>())
            {
                if (deadBody.ParentId == target.PlayerId)
                {
                    deadBody.gameObject.Destroy();
                }
            }
            DeadBody deadBody2 = Object.Instantiate<DeadBody>(GameManager.Instance.deadBodyPrefab[0]);
		    deadBody2.enabled = false;
		    deadBody2.ParentId = (TransformedPlayer != null) ? TransformedPlayer.PlayerId : doppel.PlayerId;
            Bodies.Add(deadBody2);
            foreach (SpriteRenderer b in deadBody2.bodyRenderers)
		    {
			    PlayerMaterial.SetColors((TransformedPlayer != null) ? TransformedPlayer.GetDefaultOutfit().ColorId : doppel.GetDefaultOutfit().ColorId, b);
		    }
			PlayerMaterial.SetColors((TransformedPlayer != null) ? TransformedPlayer.GetDefaultOutfit().ColorId : doppel.GetDefaultOutfit().ColorId, deadBody2.bloodSplatter);
            var position = target.GetTruePosition();
            Vector2 vector = new Vector2(position.x, position.y + 0.1616f);
            deadBody2.enabled = true;
            deadBody2.transform.position = vector;
            var deadBody3 = new DeadPlayer
            {
                PlayerId = target.PlayerId,
                KillerId = (TransformedPlayer != null) ? TransformedPlayer.PlayerId : doppel.PlayerId,
                isDoppel = true,
                KillTime = DateTime.UtcNow
            };

            Murder.KilledPlayers.Add(deadBody3);
            OldTransformed = (TransformedPlayer != null) ? TransformedPlayer : doppel;
            // Assigning it a value just so the code doesn't create any error but it won't be used
            NetworkedPlayerInfo.PlayerOutfit outfit = target.GetDefaultOutfit();
            if (Players.Contains(target.PlayerId)) outfit = target.GetOutfit(CustomPlayerOutfitType.Morph);
            Utils.Unmorph(target);
            if (TransformedPlayer == null)
            {
                Utils.Morph(target, doppel);
            }
            else Utils.Morph(target, TransformedPlayer);

            if (Players.Contains(target.PlayerId))
            {
                Utils.Unmorph(doppel);
                doppel.SetOutfit(CustomPlayerOutfitType.Morph, outfit);
                TransformedPlayer = Utils.PlayerByOutfit(outfit);
            }
            else
            {
                Utils.Unmorph(doppel);
                Utils.Morph(doppel, target);
                Players.Add(target.PlayerId);
                TransformedPlayer = target;
            }

            if (PlayerControl.LocalPlayer == doppel || PlayerControl.LocalPlayer == target)
            {
                FollowerCamera cam = Camera.main.GetComponent<FollowerCamera>();
                cam.Locked = false;
            }
        }
        public float KillTimer()
        {
            if (!coolingDown) return 0f;
            else if (!PlayerControl.LocalPlayer.inVent)
            {
                Cooldown -= Time.deltaTime;
                return Cooldown;
            }
            else return Cooldown;
        }

        public bool TryGetModifiedAppearance(out VisualAppearance appearance)
        {
            if (TransformedPlayer != null)
            {
                appearance = TransformedPlayer.GetDefaultAppearance();
                var modifiers = Modifier.GetModifiers(TransformedPlayer);
                var modifier = modifiers.FirstOrDefault(x => x is IVisualAlteration);
                if (modifier is IVisualAlteration alteration)
                    alteration.TryGetModifiedAppearance(out appearance);
                return true;
            }

            appearance = Player.GetDefaultAppearance();
            return false;
        }

        public void Wins()
        {
            DoppelgangerWins = true;
            if (AmongUsClient.Instance.AmHost) Utils.EndGame();
        }

        protected override void IntroPrefix(IntroCutscene._ShowTeam_d__38 __instance)
        {
            var doppelTeam = new List<PlayerControl>();
            doppelTeam.Add(PlayerControl.LocalPlayer);
            __instance.teamToShow = doppelTeam;
        }
    }
}