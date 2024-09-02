using System;
using System.Collections.Generic;
using System.Linq;
using ECommons;
using ECommons.Automation;
using ECommons.GameFunctions;
using XIVSlothCombo.Data;

namespace XIVSlothCombo.CustomComboNS.Functions
{
    internal abstract partial class CustomComboFunctions
    {
        internal static List<uint> oGCD = [];
        internal static Chat Chat = new Chat();

        internal uint Invoke(uint actionID)
        {
            if (oGCD.Count != 0
                && (CanWeave(actionID)
                || !HasTarget()))
            {
                if (!HasCharges(oGCD.First())
                    || JustUsed(oGCD.First()))
                    oGCD.RemoveAt(0);
                else
                {
                    if (CanWeave(actionID))
                        Auto(oGCD.First(), actionID);
                    else
                        return oGCD.First();
                }
            }

            return actionID;
        }

        public static void Next(string[]? args)
        {
            if (args != null && args.Length > 0)
                oGCD.Toggle(Convert.ToUInt16(args[0]));
        }

        internal void Auto(uint oGCD, uint actionId)
        {
            if (CanWeave(actionId)
                && InCombat())
                Chat.ExecuteCommand($"/ac \"{ActionWatching.GetActionName(oGCD)}\"");
        }

        public static bool IsOffCooldown(uint actionID) => !GetCooldown(actionID).IsCooldown || (IsOnCooldown(actionID) && GetCooldownRemainingTime(actionID) < .5);
    }
}
