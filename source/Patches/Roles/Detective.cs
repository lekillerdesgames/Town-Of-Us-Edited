using System;
using System.Collections.Generic;
using TownOfUsEdited.CrewmateRoles.DetectiveMod;
using UnityEngine;

namespace TownOfUsEdited.Roles
{
    public class Detective : Role
    {
        private KillButton _examineButton;
        public PlayerControl ClosestPlayer;
        public CrimeScene CurrentTarget;
        public CrimeScene InvestigatingScene;
        public List<byte> InvestigatedPlayers = new List<byte>();
        public List<GameObject> CrimeScenes = new List<GameObject>();
        public float Cooldown;
        public bool coolingDown => Cooldown > 0f;

        public Detective(PlayerControl player) : base(player)
        {
            Name = "Detective";
            ImpostorText = () => "Inspect Crime Scenes To Catch The Killer";
            TaskText = () => "Inspect crime scenes, then examine players for clues";
            Color = Patches.Colors.Detective;
            Cooldown = CustomGameOptions.ExamineCd;
            RoleType = RoleEnum.Detective;
            Alignment = Alignment.CrewmateInvestigative;
            Cooldown = CustomGameOptions.ExamineCd;
            AddToRoleHistory(RoleType);
        }

        public KillButton ExamineButton
        {
            get => _examineButton;
            set
            {
                _examineButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

        public float ExamineTimer()
        {
            if (!coolingDown) return 0f;
            else if (!PlayerControl.LocalPlayer.inVent)
            {
                Cooldown -= Time.deltaTime;
                return Cooldown;
            }
            else return Cooldown;
        }
    }
}