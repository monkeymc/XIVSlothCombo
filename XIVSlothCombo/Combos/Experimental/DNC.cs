using Dalamud.Game.ClientState.JobGauge.Types;
using XIVSlothCombo.Combos.PvE.Content;
using XIVSlothCombo.Combos.PvE;
using XIVSlothCombo.CustomComboNS;
using XIVSlothCombo.Extensions;

namespace XIVSlothCombo.Combos.Experimental
{
    internal static class IDNC
    {
        internal class DNC_ST_EZMode : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.DNC_ST_MultiButton;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is not DNC.Cascade) return actionID;

                #region Variables
                DNCGauge? gauge = GetJobGauge<DNCGauge>();

                var flow = HasEffect(DNC.Buffs.SilkenFlow) || HasEffect(DNC.Buffs.FlourishingFlow);
                var symmetry = HasEffect(DNC.Buffs.SilkenSymmetry) || HasEffect(DNC.Buffs.FlourishingSymmetry);
                var targetHpThresholdFeather = DNC.Config.DNC_ST_Adv_FeatherBurstPercent;
                var targetHpThresholdStandard = DNC.Config.DNC_ST_Adv_SSBurstPercent;
                var targetHpThresholdTechnical = DNC.Config.DNC_ST_Adv_TSBurstPercent;
                var gcd = GetCooldown(DNC.Fountain).CooldownTotal;

                var needToTech =
                    IsEnabled(CustomComboPreset.DNC_ST_Adv_TS) && // Enabled
                    GetCooldownRemainingTime(DNC.TechnicalStep) < (2.5 - 0.5) && // Up or about to be (some anti-drift)
                    !HasEffect(DNC.Buffs.StandardStep) && // After Standard
                    GetCooldownRemainingTime(DNC.Devilment) < (6 + 2.5 - 0.5) &&
                    GetTargetHPPercent() > targetHpThresholdTechnical &&// HP% check
                    LevelChecked(DNC.TechnicalStep);

                var needToStandardOrFinish =
                    IsEnabled(CustomComboPreset.DNC_ST_Adv_SS) && // Enabled
                    GetCooldownRemainingTime(DNC.StandardStep) < (2.5 - 0.5) && // Up or about to be (some anti-drift)
                    GetTargetHPPercent() > targetHpThresholdStandard && // HP% check
                    (IsOffCooldown(DNC.TechnicalStep) || // Checking burst is ready for standard
                     GetCooldownRemainingTime(DNC.TechnicalStep) > 5) && // Don't mangle
                    LevelChecked(DNC.StandardStep);

                var needToFinish =
                    HasEffect(DNC.Buffs.FinishingMoveReady) &&
                    !HasEffect(DNC.Buffs.LastDanceReady);

                var needToStandard =
                    !HasEffect(DNC.Buffs.FinishingMoveReady) &&
                    !HasEffect(DNC.Buffs.TechnicalFinish) &&
                    (IsOffCooldown(DNC.Flourish) || GetCooldownRemainingTime(DNC.Flourish) > 5) &&
                    (!DNC.TechnicalStep.LevelChecked() || GetCooldownRemainingTime(DNC.Devilment) > 6);
                #endregion

                #region Dance Fills
                // ST Standard (Dance) Steps & Fill
                if (HasEffect(DNC.Buffs.StandardStep))
                    return gauge.CompletedSteps < 2
                        ? gauge.NextStep
                        : DNC.StandardFinish2;

                // ST Technical (Dance) Steps & Fill
                if (HasEffect(DNC.Buffs.TechnicalStep))
                {
                    if (gauge.CompletedSteps < 4)
                    {
                        return gauge.NextStep;
                    }
                    else
                    {
                        if (GetCooldownRemainingTime(DNC.Devilment) < 1)
                            return DNC.TechnicalFinish4;
                        else
                            return DRG.Stardiver;
                    }
                }
                #endregion

                #region Weaves
                // ST Devilment
                if (IsEnabled(CustomComboPreset.DNC_ST_Adv_Devilment) &&
                    CanWeave(actionID) &&
                    LevelChecked(DNC.Devilment) &&
                    GetCooldownRemainingTime(DNC.Devilment) < 0.05 &&
                    (HasEffect(DNC.Buffs.TechnicalFinish) ||
                     WasLastAction(DNC.TechnicalFinish4) ||
                     !LevelChecked(DNC.TechnicalStep)))
                    return DNC.Devilment;

                // ST Flourish
                if (IsEnabled(CustomComboPreset.DNC_ST_Adv_Flourish) &&
                    CanWeave(actionID) &&
                    ActionReady(DNC.Flourish) &&
                    !WasLastWeaponskill(DNC.TechnicalFinish4) &&
                    IsOnCooldown(DNC.Devilment) &&
                    (GetCooldownRemainingTime(DNC.Devilment) > 50 ||
                     (HasEffect(DNC.Buffs.Devilment) &&
                      GetBuffRemainingTime(DNC.Buffs.Devilment) < 19)) &&
                    !HasEffect(DNC.Buffs.ThreeFoldFanDance) &&
                    !HasEffect(DNC.Buffs.FourFoldFanDance) &&
                    !HasEffect(DNC.Buffs.FlourishingSymmetry) &&
                    !HasEffect(DNC.Buffs.FlourishingFlow) &&
                    !HasEffect(DNC.Buffs.FinishingMoveReady) &&
                    ((CombatEngageDuration().TotalSeconds < 20 &&
                      HasEffect(DNC.Buffs.TechnicalFinish)) ||
                     CombatEngageDuration().TotalSeconds > 20))
                    return DNC.Flourish;

                if (IsEnabled(CustomComboPreset.DNC_ST_Adv_Flourish_ForcedTripleWeave) &&
                    (HasEffect(DNC.Buffs.ThreeFoldFanDance) ||
                     HasEffect(DNC.Buffs.FourFoldFanDance)) &&
                     CombatEngageDuration().TotalSeconds > 20 &&
                     HasEffect(DNC.Buffs.TechnicalFinish) &&
                     GetCooldownRemainingTime(DNC.Flourish) > 58)
                {
                    if (HasEffect(DNC.Buffs.ThreeFoldFanDance) && CanDelayedWeave(actionID))
                        return DNC.FanDance3;
                    if (HasEffect(DNC.Buffs.FourFoldFanDance))
                        return DNC.FanDance4;
                }

                // ST Interrupt
                if (IsEnabled(CustomComboPreset.DNC_ST_Adv_Interrupt) &&
                CanInterruptEnemy() &&
                CanWeave(actionID) &&
                ActionReady(All.HeadGraze) &&
                !HasEffect(DNC.Buffs.TechnicalFinish))
                    return All.HeadGraze;

                // Variant Cure
                if (IsEnabled(CustomComboPreset.DNC_Variant_Cure) &&
                    IsEnabled(Variant.VariantCure) &&
                    PlayerHealthPercentageHp() <= GetOptionValue(DNC.Config.DNCVariantCurePercent))
                    return Variant.VariantCure;

                // Variant Rampart
                if (IsEnabled(CustomComboPreset.DNC_Variant_Rampart) &&
                    IsEnabled(Variant.VariantRampart) &&
                    IsOffCooldown(Variant.VariantRampart) &&
                    CanWeave(actionID))
                    return Variant.VariantRampart;

                if (CanWeave(actionID) && !WasLastWeaponskill(DNC.TechnicalFinish4))
                {
                    // FD3
                    if (HasEffect(DNC.Buffs.ThreeFoldFanDance))
                    {
                        // Burst FD3
                        if (HasEffect(DNC.Buffs.Devilment))
                        {
                            return DNC.FanDance3;
                        }

                        // FD3 Pooling
                        if (GetCooldownRemainingTime(DNC.Devilment) > 15)
                        {
                            return DNC.FanDance3;
                        }
                    }

                    if (HasEffect(DNC.Buffs.FourFoldFanDance))
                    {
                        return DNC.FanDance4;
                    }

                    // ST Feathers & Fans
                    if (LevelChecked(DNC.FanDance1) && gauge.Feathers > 0)
                    {
                        // FD1 HP% Dump
                        if (GetTargetHPPercent() <= targetHpThresholdFeather)
                            return DNC.FanDance1;

                        if (DNC.Devilment.LevelChecked())
                        {
                            // Burst FD1
                            if (HasEffect(DNC.Buffs.Devilment))
                                return DNC.FanDance1;

                            // FD1 Pooling
                            if (GetCooldownRemainingTime(DNC.Devilment) > 10
                                && gauge.Feathers > 3
                                && (HasEffect(DNC.Buffs.SilkenSymmetry) || HasEffect(DNC.Buffs.SilkenFlow)))
                                return DNC.FanDance1;
                        }
                        else
                            return DNC.FanDance1;
                    }

                    // ST Panic Heals
                    if (IsEnabled(CustomComboPreset.DNC_ST_Adv_PanicHeals))
                    {
                        if (ActionReady(DNC.CuringWaltz) &&
                            PlayerHealthPercentageHp() < DNC.Config.DNC_ST_Adv_PanicHealWaltzPercent)
                            return DNC.CuringWaltz;

                        if (ActionReady(All.SecondWind) &&
                            PlayerHealthPercentageHp() < DNC.Config.DNC_ST_Adv_PanicHealWindPercent)
                            return All.SecondWind;
                    }

                    // ST Improvisation
                    if (IsEnabled(CustomComboPreset.DNC_ST_Adv_Improvisation) &&
                        ActionReady(DNC.Improvisation) &&
                        !HasEffect(DNC.Buffs.TechnicalFinish))
                        return DNC.Improvisation;
                }
                #endregion

                #region BURST
                if (HasEffect(DNC.Buffs.Devilment))
                {
                    if (gauge.Esprit >= 85)
                        return DNC.SaberDance;

                    if (HasEffect(DNC.Buffs.LastDanceReady))
                        return DNC.LastDance;

                    if (HasEffect(DNC.Buffs.FlourishingFinish))
                    {
                        if (gauge.Esprit >= 50)
                            return DNC.SaberDance;

                        return DNC.Tillana;
                    }

                    if (HasEffect(DNC.Buffs.DanceOfTheDawnReady))
                        return OriginalHook(DNC.DanceOfTheDawn);

                    if (HasEffect(DNC.Buffs.FinishingMoveReady) && IsOffCooldown(DNC.StandardStep))
                        return OriginalHook(DNC.FinishingMove);

                    if (gauge.Esprit >= 50)
                        return DNC.SaberDance;

                    if (HasEffect(DNC.Buffs.FlourishingStarfall))
                        return DNC.StarfallDance;

                    if (flow)
                        return DNC.Fountainfall;

                    if (symmetry)
                        return DNC.ReverseCascade;
                }
                #endregion

                #region GCD
                // ST Technical Step
                if (needToTech)
                    return DNC.TechnicalStep;

                // ST Last Dance
                if (IsEnabled(CustomComboPreset.DNC_ST_Adv_LD) && // Enabled
                    HasEffect(DNC.Buffs.LastDanceReady) && // Ready
                    (HasEffect(DNC.Buffs.TechnicalFinish) || // Has Tech
                     !(IsOnCooldown(DNC.TechnicalStep) && // Or can't hold it for tech
                       GetCooldownRemainingTime(DNC.TechnicalStep) < 20 &&
                       GetBuffRemainingTime(DNC.Buffs.LastDanceReady) > GetCooldownRemainingTime(DNC.TechnicalStep) + 4) ||
                     GetBuffRemainingTime(DNC.Buffs.LastDanceReady) < 4)) // Or last second
                    return DNC.LastDance;

                // ST Standard Step (Finishing Move)
                if (needToStandardOrFinish && needToFinish)
                    return OriginalHook(DNC.FinishingMove);

                // ST Standard Step
                if (needToStandardOrFinish && needToStandard)
                    return DNC.StandardStep;

                // Emergency Starfall usage
                if (HasEffect(DNC.Buffs.FlourishingStarfall) &&
                    GetBuffRemainingTime(DNC.Buffs.FlourishingStarfall) < 4)
                    return DNC.StarfallDance;

                // ST Dance of the Dawn
                if (IsEnabled(CustomComboPreset.DNC_ST_Adv_DawnDance) &&
                    HasEffect(DNC.Buffs.DanceOfTheDawnReady) &&
                    LevelChecked(DNC.DanceOfTheDawn) &&
                    (GetCooldownRemainingTime(DNC.TechnicalStep) > 5 ||
                     IsOffCooldown(DNC.TechnicalStep)) && // Tech is up
                    (gauge.Esprit >= DNC.Config.DNC_ST_Adv_SaberThreshold || // above esprit threshold use
                     (HasEffect(DNC.Buffs.TechnicalFinish) && // will overcap with Tillana if not used
                      IsNotEnabled(CustomComboPreset.DNC_ST_Adv_TillanaOverEsprit) &&
                      gauge.Esprit >= 50) ||
                     (GetBuffRemainingTime(DNC.Buffs.DanceOfTheDawnReady) < 5 && gauge.Esprit >= 50))) // emergency use
                    return OriginalHook(DNC.DanceOfTheDawn);

                // ST Saber Dance (Emergency Use)
                if (IsEnabled(CustomComboPreset.DNC_ST_Adv_SaberDance) &&
                    LevelChecked(DNC.SaberDance) &&
                    (gauge.Esprit >= DNC.Config.DNC_ST_Adv_SaberThreshold || // above esprit threshold use
                     (HasEffect(DNC.Buffs.TechnicalFinish) && // will overcap with Tillana if not used
                      IsNotEnabled(CustomComboPreset.DNC_ST_Adv_TillanaOverEsprit) &&
                      gauge.Esprit >= 50)))
                    return LevelChecked(DNC.DanceOfTheDawn) &&
                           HasEffect(DNC.Buffs.DanceOfTheDawnReady)
                        ? OriginalHook(DNC.DanceOfTheDawn)
                        : DNC.SaberDance;

                if (HasEffect(DNC.Buffs.FlourishingStarfall))
                    return DNC.StarfallDance;

                // ST Tillana
                if (HasEffect(DNC.Buffs.FlourishingFinish) &&
                    IsEnabled(CustomComboPreset.DNC_ST_Adv_Tillana))
                {
                    if (gauge.Esprit >= 50)
                        return DNC.SaberDance;
                    else
                        return DNC.Tillana;
                }

                // ST Saber Dance
                if (IsEnabled(CustomComboPreset.DNC_ST_Adv_SaberDance) &&
                    LevelChecked(DNC.SaberDance) &&
                    gauge.Esprit >= DNC.Config.DNC_ST_Adv_SaberThreshold || // Above esprit threshold use
                    (HasEffect(DNC.Buffs.TechnicalFinish) && gauge.Esprit >= 50) && // Burst
                    (GetCooldownRemainingTime(DNC.TechnicalStep) > 5 ||
                     IsOffCooldown(DNC.TechnicalStep))) // Tech is up
                    return DNC.SaberDance;

                // ST combos and burst attacks
                if (LevelChecked(DNC.Fountain) &&
                    lastComboMove is DNC.Cascade &&
                    comboTime is < 2 and > 0)
                    return DNC.Fountain;
                if (LevelChecked(DNC.Fountainfall) && flow)
                    return DNC.Fountainfall;
                if (LevelChecked(DNC.ReverseCascade) && symmetry)
                    return DNC.ReverseCascade;
                if (LevelChecked(DNC.Fountain) && lastComboMove is DNC.Cascade && comboTime > 0)
                    return DNC.Fountain;
                #endregion

                return actionID;
            }
        }
    }

}
