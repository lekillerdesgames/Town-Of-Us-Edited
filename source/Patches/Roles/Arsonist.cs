﻿using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities;
using TownOfUsEdited.CrewmateRoles.ClericMod;
using TownOfUsEdited.CrewmateRoles.MedicMod;
using TownOfUsEdited.Extensions;
using TownOfUsEdited.Patches;

namespace TownOfUsEdited.Roles
{
    public class Arsonist : Role
    {
        private KillButton _igniteButton;
        public bool ArsonistWins;
        public float Cooldown;
        public bool coolingDown => Cooldown > 0f;
        public PlayerControl ClosestPlayerDouse;
        public PlayerControl ClosestPlayerIgnite;
        public List<byte> DousedPlayers = new List<byte>();
        public bool LastKiller = false;

        public int DousedAlive => DousedPlayers.Count(x => Utils.PlayerById(x) != null && Utils.PlayerById(x).Data != null && !Utils.PlayerById(x).Data.IsDead && !Utils.PlayerById(x).Data.Disconnected);


        public Arsonist(PlayerControl player) : base(player)
        {
            Name = "Arsonist";
            ImpostorText = () => "Douse Players And Ignite The Light";
            TaskText = () => "Douse players and ignite to kill all douses\nFake Tasks:";
            Color = Patches.Colors.Arsonist;
            Cooldown = CustomGameOptions.DouseCd;
            RoleType = RoleEnum.Arsonist;
            AddToRoleHistory(RoleType);
            Faction = Faction.NeutralKilling;
        }

        public KillButton IgniteButton
        {
            get => _igniteButton;
            set
            {
                _igniteButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

        internal override void NeutralWin(LogicGameFlowNormal __instance)
        {
            if (Player.Data.IsDead || Player.Data.Disconnected) return;

            var alivecrewkiller = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(Faction.Crewmates) && x.IsCrewKiller() && !x.Data.IsDead && !x.Data.Disconnected).ToList();

            if (PlayerControl.AllPlayerControls.ToArray().Count(x => !x.Data.IsDead && !x.Data.Disconnected) <= 2 &&
                    PlayerControl.AllPlayerControls.ToArray().Count(x => !x.Data.IsDead && !x.Data.Disconnected &&
                    (x.Data.IsImpostor() || x.Is(Faction.NeutralKilling) || x.Is(Faction.Coven))) == 1 && (alivecrewkiller.Count <= 0 || !CustomGameOptions.CrewKillersContinue))
            {
                Utils.Rpc(CustomRPC.ArsonistWin, Player.PlayerId);
                Wins();
                PluginSingleton<TownOfUsEdited>.Instance.Log.LogMessage("GAME OVER REASON: Arsonist Win");
                return;
            }

            return;
        }


        public void Wins()
        {
            ArsonistWins = true;
            if (AmongUsClient.Instance.AmHost) Utils.EndGame();
        }

        protected override void IntroPrefix(IntroCutscene._ShowTeam_d__38 __instance)
        {
            var arsonistTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            arsonistTeam.Add(PlayerControl.LocalPlayer);
            __instance.teamToShow = arsonistTeam;
        }

        public float DouseTimer()
        {
            if (!coolingDown) return 0f;
            else return Cooldown;
        }

        public void Ignite()
        {
            foreach (var playerId in DousedPlayers)
            {
                var player = Utils.PlayerById(playerId);
                if (!player.Is(RoleEnum.Pestilence) && !player.IsShielded() && !player.IsProtected() && !player.IsBarriered() && player != ShowShield.FirstRoundShielded)
                {
                    Utils.RpcMultiMurderPlayer(Player, player);
                }
                else if (player.IsShielded())
                {
                    foreach (var medic in player.GetMedic())
                    {
                        Utils.Rpc(CustomRPC.AttemptSound, medic.Player.PlayerId, player.PlayerId);
                        StopKill.BreakShield(medic.Player.PlayerId, player.PlayerId, CustomGameOptions.ShieldBreaks);
                    }
                }
                else if (player.IsBarriered())
                {
                    foreach (var cleric in player.GetCleric())
                    {
                        StopAttack.NotifyCleric(cleric.Player.PlayerId, false);
                    }
                }
            }
        }
    }
}
