using HarmonyLib;
using TownOfUsEdited.Extensions;
using TownOfUsEdited.Roles;
using UnityEngine;

namespace TownOfUsEdited.NeutralRoles.PhantomMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPriority(Priority.Last)]
    public class Hide
    {
        public static void Postfix(HudManager __instance)
        {
            foreach (var role in Role.GetRoles(RoleEnum.Phantom))
            {
                var phantom = (Phantom)role;
                if (phantom.Player == null || phantom.Player.Data.Disconnected) return;
                var caught = phantom.Caught;
                if (!caught)
                {
                    phantom.Fade();
                }
                else if (phantom.Faded)
                {
                    Utils.Unmorph(phantom.Player);
                    phantom.Player.myRend().color = Color.white;
                    phantom.Player.gameObject.layer = LayerMask.NameToLayer("Ghost");
                    phantom.Faded = false;
                    phantom.Player.Collider.enabled = false;
                    phantom.Player.MyPhysics.ResetMoveState();
                }
            }
        }
    }
}