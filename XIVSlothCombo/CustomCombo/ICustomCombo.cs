using System;
using System.Collections.Generic;
using System.Linq;
using ECommons.Automation;
using ECommons;
using XIVSlothCombo.Data;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ECommons.Logging;

namespace XIVSlothCombo.CustomComboNS.Functions
{
    internal abstract partial class CustomComboFunctions
    {
        internal static List<uint> oGCD = [];
        internal static Chat Chat = new Chat();

        internal uint Invoke(uint actionID)
        {
            if (oGCD.Count != 0)
            {
                if (!HasCharges(oGCD.First())
                    || JustUsed(oGCD.First()))
                    oGCD.RemoveAt(0);

                if (CanWeave(actionID)
                    || !HasTarget())
                    return oGCD.First();
            }

            return actionID;
        }

        public static void Next(string[]? args)
        {
            if (args != null && args.Length > 0)
                oGCD.Toggle(Convert.ToUInt16(args[1]));
        }

        internal static void Auto(uint oGCD, uint actionId)
        {
            if (CanWeave(actionId)
                && InCombat()
                && HasTarget())
                Chat.ExecuteCommand($"/ac \"{ActionWatching.GetActionName(oGCD)}\"");
        }

        public static bool IsOffCooldown(uint actionID) => !GetCooldown(actionID).IsCooldown
            || (IsOnCooldown(actionID) && GetCooldownRemainingTime(actionID) <= .5);

        public unsafe static bool LB_READY() => UIState.Instance()->LimitBreakController.CurrentUnits == 2500;
    }
}
