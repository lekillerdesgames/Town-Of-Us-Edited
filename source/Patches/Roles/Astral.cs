using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Reactor.Utilities.Extensions;
using TownOfUsEdited.CrewmateRoles.MedicMod;
using TownOfUsEdited.CrewmateRoles.TimeLordMod;
using TownOfUsEdited.Extensions;
using TownOfUsEdited.Roles.Modifiers;
using UnityEngine;

namespace TownOfUsEdited.Roles
{
    public class Astral : Role
    {
        public bool Enabled = false;
        public float TimeRemaining;
        public float Cooldown;
        public bool coolingDown => Cooldown > 0f;
        public PlayerControl AstralBody;

        public Astral(PlayerControl player) : base(player)
        {
            Name = "Astral";
            ImpostorText = () => "Project Your Spirit To Investigate";
            TaskText = () => "Use Your Ability To Turn Into Ghost";
            Color = Patches.Colors.Astral;
            Cooldown = CustomGameOptions.GhostCD;
            RoleType = RoleEnum.Astral;
            Faction = Faction.Crewmates;
            Alignment = Alignment.CrewmateInvestigative;
            AddToRoleHistory(RoleType);
        }

        public bool UsingGhost => TimeRemaining > 0f;

        public float AstralTimer()
        {
            if (!coolingDown) return 0f;
            else if (!PlayerControl.LocalPlayer.inVent)
            {
                Cooldown -= Time.deltaTime;
                return Cooldown;
            }
            else return Cooldown;
        }

        public void DecreaseCD()
        {
            TimeRemaining -= Time.deltaTime;
            Enabled = true;
        }

        public void Die(PlayerControl player)
        {
            Utils.CreateDummy(player, player);
            var role = Role.GetRole<Astral>(player);
            new Astral(role.AstralBody);

            var hudManager = DestroyableSingleton<HudManager>.Instance;
            var amOwner = player.AmOwner;
            RoleManager.Instance.SetRole(player, RoleTypes.CrewmateGhost);
            if (amOwner)
            {
                try
                    {
                        if (Minigame.Instance)
                        {
                            Minigame.Instance.Close();
                            Minigame.Instance.Close();
                        }

                        if (MapBehaviour.Instance)
                        {
                            MapBehaviour.Instance.Close();
                            MapBehaviour.Instance.Close();
                        }
                    }
                    catch
                    {
                    }
                hudManager.ShadowQuad.gameObject.SetActive(false);
                player.nameText().GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
                player.RpcSetScanner(false);
            }

            if (PlayerControl.LocalPlayer == player)
            {
                HudManager.Instance.Chat.gameObject.SetActive(false);
                role.RegenTask();
            }
        }

        public void ResetCD()
        {
            TimeRemaining = 0f;
            Enabled = false;
            Cooldown = CustomGameOptions.GhostCD;
        }

        public void Revive(PlayerControl player)
        {
            var role = Role.GetRole<Astral>(player);
            Role.RoleDictionary.Remove(role.AstralBody.PlayerId);
            if (Modifier.ModifierDictionary.ContainsKey(role.AstralBody.PlayerId)) Modifier.ModifierDictionary.Remove(role.AstralBody.PlayerId);
            var position = role.AstralBody.GetTruePosition();

            var revived = new List<PlayerControl>();

            player.Revive();
            revived.Add(player);
            RoleManager.Instance.SetRole(player, RoleTypes.Crewmate);
            Murder.KilledPlayers.Remove(
                    Murder.KilledPlayers.FirstOrDefault(x => x.PlayerId == player.PlayerId));

            var usedPosition = new Vector2(position.x, position.y + 0.3636f);
            player.transform.position = new Vector2(usedPosition.x, usedPosition.y);

            if (Patches.SubmergedCompatibility.isSubmerged() && PlayerControl.LocalPlayer.PlayerId == player.PlayerId)
            {
                Patches.SubmergedCompatibility.ChangeFloor(player.transform.position.y > -7);
            }

            if (revived.Any(x => x.AmOwner))
                try
                {
                    Minigame.Instance.Close();
                    Minigame.Instance.Close();
                }
                catch
                {
                }

            if (PlayerControl.LocalPlayer == player)
            {
                player.myTasks.RemoveAt(1);
                if (!MeetingHud.Instance)
                {
                    HudManager.Instance.Chat.gameObject.SetActive(false);
                    PlayerControl.LocalPlayer.NetTransform.Halt();
                }
                else
                {
                    HudManager.Instance.Chat.gameObject.SetActive(true);
                    MeetingHud.Instance.amDead = false;
                    MeetingHud.Instance.SkipVoteButton.AmDead = false;
                    MeetingHud.Instance.Glass.sprite = default;
                    MeetingHud.Instance.Glass.color = Palette.White;
                    if (CustomGameOptions.SkipButtonDisable == DisableSkipButtonMeetings.No)
                    {
                        MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(true);
                    }
                }
            }

            role.AstralBody.Data.Disconnected = true;
            GameData.Instance.AllPlayers.Remove(role.AstralBody.Data);
            role.AstralBody.gameObject.Destroy();
            role.AstralBody = null;

            if (Utils.CommsCamouflaged()) Utils.Camouflage(player);
        }
    }
}