using TownOfUsEdited.Extensions;
using UnityEngine;

namespace TownOfUsEdited.Roles
{
    public class Chameleon : Role
    {
        public bool Enabled;
        public float Cooldown;
        public bool coolingDown => Cooldown > 0f;
        public float TimeRemaining;

        public Chameleon(PlayerControl player) : base(player)
        {
            Name = "Chameleon";
            ImpostorText = () => "Turn Invisible Temporarily";
            TaskText = () => "Turn invisible to catch killers";
            Color = Patches.Colors.Chameleon;
            Cooldown = CustomGameOptions.ChamSwoopCooldown;
            RoleType = RoleEnum.Chameleon;
            Faction = Faction.Crewmates;
            AddToRoleHistory(RoleType);
        }

        public bool IsSwooped => TimeRemaining > 0f;

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
            Cooldown = CustomGameOptions.ChamSwoopCooldown;
            Utils.Unmorph(Player);
            Player.myRend().color = Color.white;
        }
    }
}