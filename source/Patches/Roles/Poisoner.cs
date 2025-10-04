using UnityEngine;
using TownOfUsEdited.Modifiers.UnderdogMod;
using TownOfUsEdited.Roles.Modifiers;
using TMPro;

namespace TownOfUsEdited.Roles
{
    public class Poisoner : Role

    {
        public PlayerControl ClosestPlayer;
        public bool coolingDown => Cooldown > 0f;
        public float Cooldown;
        public PlayerControl PoisonedPlayer;
        public float TimeRemaining;
        public bool Enabled = false;

        public Poisoner(PlayerControl player) : base(player)
        {
            Name = "Poisoner";
            ImpostorText = () => "Poison The Crewmates";
            TaskText = () => "Poison a <color=#00FFFF>Crewmate</color> to kill them in a few seconds\nFake Tasks:";
            Color = Palette.ImpostorRed;
            Cooldown = CustomGameOptions.PoisonCD;
            RoleType = RoleEnum.Poisoner;
            AddToRoleHistory(RoleType);
            Faction = Faction.Impostors;
            Alignment = Alignment.ImpostorKilling;
            PoisonedPlayer = null;
        }
        public bool Poisoned => TimeRemaining > 0f;
        public void Poison()
        {
            Enabled = true;
            if (!Utils.Rewinding()) TimeRemaining -= Time.deltaTime;
            if (MeetingHud.Instance)
            {
                TimeRemaining = 0;
            }
            if (TimeRemaining <= 0)
            {
                PoisonKill();
            }
        }
        public void PoisonKill()
        {
            Enabled = false;
            var target = PoisonedPlayer;
            if (target.Is(ModifierEnum.Diseased))
            {
                Cooldown = CustomGameOptions.PoisonCD * CustomGameOptions.DiseasedMultiplier;
            }
            else if (Player.Is(ModifierEnum.Underdog))
            {
                Cooldown = PerformKill.LastImp() ? CustomGameOptions.PoisonCD - CustomGameOptions.UnderdogKillBonus :
                PerformKill.IncreasedKC() ? CustomGameOptions.PoisonCD : (CustomGameOptions.PoisonCD + CustomGameOptions.UnderdogKillBonus);
            }
            else if (Player.Is(ModifierEnum.Bloodlust))
            {
                var modifier = Modifier.GetModifier<Bloodlust>(PlayerControl.LocalPlayer);
                var num = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown / 2;
                modifier.KilledThisRound += 1;
                if (modifier.KilledThisRound >= 2) Cooldown = num;
                else Cooldown = CustomGameOptions.PoisonCD;
            }
            else if (Player.Is(ModifierEnum.Lucky))
            {
                var num = UnityEngine.Random.RandomRange(1f, 60f);
                Cooldown = num;
            }
            else Cooldown = CustomGameOptions.PoisonCD;

            // Kill Check
            if (!PoisonedPlayer.Is(RoleEnum.Pestilence) && !PoisonedPlayer.IsOnAlert() && !Player.IsJailed()
            && !PoisonedPlayer.HasPotionShield() && !PoisonedPlayer.IsProtected() || !PoisonedPlayer.IsBarriered() || !PoisonedPlayer.IsGuarded2())
            {
                Utils.Rpc(CustomRPC.PoisonKill, PlayerControl.LocalPlayer.PlayerId, true); // Successed Kill
                Utils.MurderPlayer(Player, PoisonedPlayer, false);
                var targetRole = Role.GetRole(PoisonedPlayer);
                targetRole.DeathReason = DeathReasons.Poisoned;
                Utils.Rpc(CustomRPC.SetDeathReason, PoisonedPlayer.PlayerId, (byte)DeathReasons.Poisoned);
                SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.KillSfx, false, 0.5f);
                PoisonedPlayer = null;
                return;
            }
            Utils.Rpc(CustomRPC.PoisonKill, PlayerControl.LocalPlayer.PlayerId, false); // Failed Kill
            PoisonedPlayer = null;
        }
        public float PoisonTimer()
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