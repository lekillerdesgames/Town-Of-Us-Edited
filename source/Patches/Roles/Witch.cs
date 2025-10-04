using System.Collections.Generic;
using TMPro;

namespace TownOfUsEdited.Roles
{
    public class Witch : Role
    {
        public KillButton _spellButton;
        public TextMeshPro SpellText;

        public Witch(PlayerControl player) : base(player)
        {
            Name = "Witch";
            ImpostorText = () => "Cast Spells On Players";
            TaskText = () => "Cast spells on <color=#00FFFF>Crewmates</color> to kill them after the next meeting\nFake Tasks:";
            Color = Patches.Colors.Impostor;
            RoleType = RoleEnum.Witch;
            AddToRoleHistory(RoleType);
            Faction = Faction.Impostors;
            Alignment = Alignment.ImpostorKilling;
        }

        public PlayerControl ClosestPlayer;
        public List<byte> CursedList = new List<byte>();

        public KillButton SpellButton
        {
            get => _spellButton;
            set
            {
                _spellButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }
    }
}