using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Component.GUI;
using XIVSlothCombo.Combos;

namespace XIVSlothCombo.CustomComboNS.Functions
{
    internal abstract partial class CustomComboFunctions
    {
        public static List<uint> OGCD = [];

        internal uint Invoke(uint actionID)
        {
            if (OGCD.Count > 0 && (CanWeave(actionID, 0.5) || !HasTarget()))
            {
                if (!HasCharges(OGCD.First()) || JustUsed(OGCD.First()))
                {
                    OGCD.RemoveAt(0);
                }

                try
                {
                    return OGCD.First();
                } 
                catch 
                {
                    return actionID;
                }
            }

            return actionID;
        }
    }
}
