using HarmonyLib;
using TownOfUsEdited.Roles;

namespace TownOfUsEdited.NeutralRoles.SurvivorMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPriority(Priority.Last)]
    public class VestUnvest
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(HudManager __instance)
        {
            foreach (var role in Role.GetRoles(RoleEnum.Survivor))
            {
                var surv = (Survivor) role;
                if (surv.Vesting)
                    surv.Vest();
                else if (surv.Enabled) surv.UnVest();
            }
        }
    }
}