﻿using TownOfUsEdited.Modifiers.UnderdogMod;

namespace TownOfUsEdited.Roles.Modifiers
{
    public class Underdog : Modifier
    {
        public Underdog(PlayerControl player) : base(player)
        {
            Name = "Underdog";
            TaskText = () => "When you're alone your kill cooldown is shortened";
            Color = Patches.Colors.Impostor;
            ModifierType = ModifierEnum.Underdog;
        }

        public float MaxTimer() => PerformKill.LastImp() ? GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown - CustomGameOptions.UnderdogKillBonus : (PerformKill.IncreasedKC() ? GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown : GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown + CustomGameOptions.UnderdogKillBonus);

        public void SetKillTimer()
        {
            Role.GetRole(PlayerControl.LocalPlayer).KillCooldown = MaxTimer();
        }
    }
}
