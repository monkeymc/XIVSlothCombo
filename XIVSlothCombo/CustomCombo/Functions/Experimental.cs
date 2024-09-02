using System.Collections.Generic;
using System.Linq;
using ECommons.Automation;
using ECommons.DalamudServices;
using XIVSlothCombo.Data;
using XIVSlothCombo.Services;

namespace XIVSlothCombo.CustomComboNS.Functions
{
    internal abstract partial class CustomComboFunctions
    {
        public static List<uint> OGCD = [];
        internal static Chat Chat = new Chat();

        internal uint Invoke(uint actionID)
        {
            if (OGCD.Count > 0 && (CanWeave(actionID) || !HasTarget()))
            {
                if (!HasCharges(OGCD.First()) || JustUsed(OGCD.First()))
                {
                    OGCD.RemoveAt(0);
                }

                try
                {
                    if (CanWeave(actionID))
                    {
                        Auto(OGCD.First());
                    }
                    else
                    {
                        return OGCD.First();
                    }
                }
                catch
                {
                    return actionID;
                }
            }

            return actionID;
        }

        internal void Auto(uint actionID)
        {
            Chat.ExecuteCommand($"/ac \"{ActionWatching.GetActionName(actionID)}\"");
        }
    }
}
