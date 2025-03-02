using System.Collections;
using TownOfUsEdited.Roles;
using UnityEngine;

namespace TownOfUsEdited.NeutralRoles.VultureMod
{
    public class Coroutine
    {
        private static readonly int BodyColor = Shader.PropertyToID("_BodyColor");
        private static readonly int BackColor = Shader.PropertyToID("_BackColor");

        public static IEnumerator EatCoroutine(DeadBody body, Vulture role)
        {
            KillButtonTarget.SetTarget(DestroyableSingleton<HudManager>.Instance.KillButton, null, role);
            SpriteRenderer renderer = null;
            foreach (var body2 in body.bodyRenderers) renderer = body2;
            var backColor = renderer.material.GetColor(BackColor);
            var bodyColor = renderer.material.GetColor(BodyColor);
            var newColor = new Color(1f, 1f, 1f, 0f);
            for (var i = 0; i < 60; i++)
            {
                if (body == null) yield break;
                renderer.color = Color.Lerp(backColor, newColor, i / 60f);
                renderer.color = Color.Lerp(bodyColor, newColor, i / 60f);
                yield return null;
            }

            Object.Destroy(body.gameObject);
            role.BodiesEaten++;
            if (role.BodiesEaten == CustomGameOptions.VultureBodies)
            {
                role.Wins();
                Utils.Rpc(CustomRPC.VultureWin, role.Player.PlayerId);
                if (!CustomGameOptions.NeutralEvilWinEndsGame)
                {
                    Utils.Interact(role.Player, role.Player, true);
                    role.DeathReason = DeathReasons.Won;
                    Utils.Rpc(CustomRPC.SetDeathReason, role.Player.PlayerId, (byte)DeathReasons.Won);
                }
            }
        }
    }
}