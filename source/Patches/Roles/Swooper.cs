using System;
using TMPro;
using TownOfUsEdited.Extensions;
using UnityEngine;

namespace TownOfUsEdited.Roles
{
    public class Swooper : Role
    {
        public KillButton _swoopButton;
        public TextMeshPro SwoopText;
        public bool Enabled;
        public float Cooldown;
        public bool coolingDown => Cooldown > 0f;
        public float TimeRemaining;

        public Swooper(PlayerControl player) : base(player)
        {
            Name = "Swooper";
            ImpostorText = () => "Turn Invisible Temporarily";
            TaskText = () => "Turn invisible and sneakily kill\nFake Tasks:";
            Color = Patches.Colors.Impostor;
            Cooldown = CustomGameOptions.SwoopCd;
            RoleType = RoleEnum.Swooper;
            AddToRoleHistory(RoleType);
            Faction = Faction.Impostors;
            Alignment = Alignment.ImpostorConcealing;
        }

        public bool IsSwooped => TimeRemaining > 0f;

        public KillButton SwoopButton
        {
            get => _swoopButton;
            set
            {
                _swoopButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

        public float SwoopTimer()
        {
            if (!coolingDown) return 0f;
            else if (!PlayerControl.LocalPlayer.inVent)
            {
                Cooldown -= Time.deltaTime;
                return Cooldown;
            }
            else return Cooldown;
        }

        public void Swoop()
        {
            Enabled = true;
            TimeRemaining -= Time.deltaTime;
            if (Player.Data.IsDead)
            {
                TimeRemaining = 0f;
            }
        }


        public void UnSwoop()
        {
            Enabled = false;
            Cooldown = CustomGameOptions.SwoopCd;
            Utils.Unmorph(Player);
            Player.myRend().color = Color.white;
        }
    }
}