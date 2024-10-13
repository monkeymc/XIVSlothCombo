using Dalamud.Game.ClientState.JobGauge.Types;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using XIVSlothCombo.Combos.PvP;
using XIVSlothCombo.CustomComboNS.Functions;
using XIVSlothCombo.Extensions;

namespace XIVSlothCombo.Combos.PvE
{
    internal class IADV : DNC
    {
        internal class CustomCombo : CustomComboFunctions
        {
            public static uint ST(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is not Cascade) return actionID;
                #region Variables
                DNCGauge gauge = GetJobGauge<DNCGauge>();
                var flow = HasEffect(Buffs.SilkenFlow) || HasEffect(Buffs.FlourishingFlow);
                var symmetry = HasEffect(Buffs.SilkenSymmetry) || HasEffect(Buffs.FlourishingSymmetry);
                var GCD = GetCooldown(Fountain).CooldownTotal - 0.5;
                #endregion
                #region Dance Fills
                // SS
                if (HasEffect(Buffs.StandardStep))
                {
                    if (gauge.CompletedSteps < 2)
                    {
                        return gauge.NextStep;
                    }
                    if (HasBattleTarget() && InActionRange(StandardFinish2))
                    {
                        return StandardFinish2;
                    }
                    else
                    {
                        return DRG.Stardiver;
                    }
                }
                // TS
                if (HasEffect(Buffs.TechnicalStep))
                {
                    if (gauge.CompletedSteps < 4)
                    {
                        return gauge.NextStep;
                    }

                    if (HasBattleTarget() && InActionRange(TechnicalFinish4) && GetCooldownRemainingTime(Devilment) < 1)
                    {
                        return TechnicalFinish4;
                    }
                    else
                    {
                        return DRG.Stardiver;
                    }
                }
                #endregion
                bool flourish = LevelChecked(Flourish)
                    && IsOffCooldown(Flourish)
                    && !HasEffect(Buffs.ThreeFoldFanDance)
                    && !HasEffect(Buffs.FourFoldFanDance)
                    && !HasEffect(Buffs.FlourishingSymmetry)
                    && !HasEffect(Buffs.FlourishingFlow)
                    && !HasEffect(Buffs.FinishingMoveReady)
                    && InCombat();
                #region Weaves
                // Devilment
                if (CanWeave(actionID))
                {
                    if (LevelChecked(Devilment)
                        && IsOffCooldown(Devilment)
                        && (HasEffect(Buffs.TechnicalFinish) || WasLastWeaponskill(TechnicalFinish4) || !LevelChecked(TechnicalStep)))
                    {
                        return Devilment;
                    }
                    // Flourish
                    if (flourish
                        && (HasEffect(Buffs.TechnicalFinish) || WasLastWeaponskill(TechnicalFinish4) || HasEffect(Buffs.Devilment) || WasLastAbility(Devilment) || (InCombat() && GetCooldownRemainingTime(Devilment) > 50)))
                    {
                        return Flourish;
                    }
                    // Interrupt
                    if (CanInterruptEnemy() &&
                        IsOffCooldown(All.HeadGraze))
                    {
                        return All.HeadGraze;
                    }
                    // FD
                    if (!WasLastWeaponskill(TechnicalFinish4))
                    {
                        // FD3
                        if (HasEffect(Buffs.ThreeFoldFanDance))
                        {
                            // Burst FD3
                            if (HasEffect(Buffs.Devilment))
                            {
                                return FanDance3;
                            }
                            // FD3 Pooling
                            if (GetBuffRemainingTime(Buffs.ThreeFoldFanDance) < GetCooldownRemainingTime(Devilment)
                                || GetBuffRemainingTime(Buffs.ThreeFoldFanDance) < 10)
                            {
                                return FanDance3;
                            }
                        }
                        // FD4
                        if (HasEffect(Buffs.FourFoldFanDance))
                        {
                            // Burst FD4
                            if (HasEffect(Buffs.Devilment))
                            {
                                return FanDance4;
                            }
                            // FD4 Pooling
                            if (GetBuffRemainingTime(Buffs.FourFoldFanDance) < GetCooldownRemainingTime(Devilment)
                                || GetBuffRemainingTime(Buffs.FourFoldFanDance) < 10)
                            {
                                return FanDance4;
                            }
                        }
                        // FD1
                        if (LevelChecked(FanDance1) && gauge.Feathers > 0)
                        {
                            // FD1 HP% Dump
                            if (GetTargetHPPercent() < 10)
                            {
                                // Protect FD3
                                if (HasEffect(Buffs.ThreeFoldFanDance))
                                {
                                    return FanDance3;
                                }
                                else
                                {
                                    return FanDance1;
                                }
                            }
                            if (Devilment.LevelChecked())
                            {
                                // Burst FD1
                                if (HasEffect(Buffs.Devilment))
                                {
                                    return FanDance1;
                                }
                                // FD1 Pooling
                                if (gauge.Feathers == 4
                                    && (HasEffect(Buffs.SilkenSymmetry) || HasEffect(Buffs.SilkenFlow))
                                    && !WasLastWeaponskill(ReverseCascade)
                                    && !WasLastWeaponskill(Fountainfall))
                                {
                                    if (HasEffect(Buffs.ThreeFoldFanDance))
                                    {
                                        return FanDance3;
                                    }
                                    else
                                    {
                                        return FanDance1;
                                    }
                                }
                            }
                            else
                            {
                                return FanDance1;
                            }
                        }
                    }
                }
                #endregion
                #region GCD
                // TS
                if (IsEnabled(CustomComboPreset.DNC_ST_Adv_TS)
                    && TechnicalStep.LevelChecked()
                    && GetCooldownRemainingTime(Devilment) < 6 + GCD)
                {
                    if (flourish)
                    {
                        return Flourish;
                    }
                    return TechnicalStep;
                }
                #region BURST
                if ((HasEffect(Buffs.Devilment) && GetBuffRemainingTime(Buffs.Devilment) < 20)
                    || GetTargetHPPercent() < 10)
                {
                    if (HasEffect(Buffs.FinishingMoveReady)
                        && !HasEffect(Buffs.LastDanceReady)
                        && IsOffCooldown(StandardStep)
                        && HasBattleTarget()
                        && InActionRange(OriginalHook(FinishingMove)))
                    {
                        return OriginalHook(FinishingMove);
                    }
                    if (HasEffect(Buffs.FlourishingStarfall)
                        && GetBuffRemainingTime(Buffs.Devilment) < 5)
                    {
                        return StarfallDance;
                    }
                    if (gauge.Esprit >= 85)
                    {
                        return SaberDance;
                    }
                    if (HasEffect(Buffs.LastDanceReady))
                    {
                        return LastDance;
                    }
                    if (HasEffect(Buffs.FlourishingFinish))
                    {
                        if (gauge.Esprit >= 50)
                        {
                            return SaberDance;
                        }
                        if (HasBattleTarget()
                            && InActionRange(Tillana))
                        {
                            return Tillana;
                        }
                    }
                    if (HasEffect(Buffs.DanceOfTheDawnReady)
                        && !HasEffect(Buffs.FourFoldFanDance)
                        && !HasEffect(Buffs.FlourishingFinish))
                    {
                        return OriginalHook(DanceOfTheDawn);
                    }
                    if (HasEffect(Buffs.FinishingMoveReady)
                        && IsOffCooldown(StandardStep)
                        && HasBattleTarget()
                        && InActionRange(OriginalHook(FinishingMove)))
                    {
                        return OriginalHook(FinishingMove);
                    }
                    if (gauge.Esprit >= 50)
                    {
                        return SaberDance;
                    }
                    if (HasEffect(Buffs.FlourishingStarfall))
                    {
                        return StarfallDance;
                    }
                    if (HasEffect(Buffs.FinishingMoveReady)
                        && HasBattleTarget()
                        && InActionRange(OriginalHook(FinishingMove))
                        && GetBuffRemainingTime(Buffs.Devilment) < 5
                        && GetCooldownRemainingTime(StandardStep) <= GetBuffRemainingTime(Buffs.Devilment))
                    {
                        return OriginalHook(FinishingMove);
                    }
                    if (flow)
                    {
                        return Fountainfall;
                    }
                    if (symmetry)
                    {
                        return ReverseCascade;
                    }
                    if (LevelChecked(Fountain)
                        && lastComboMove is Cascade
                        && comboTime > 0)
                    {
                        return Fountain;
                    }
                    return actionID;
                }
                #endregion
                // LD
                if (HasEffect(Buffs.LastDanceReady)
                    && (GetBuffRemainingTime(Buffs.LastDanceReady) < GetCooldownRemainingTime(Devilment) || GetBuffRemainingTime(Buffs.LastDanceReady) - GetCooldownRemainingTime(Devilment) < 10))
                {
                    return LastDance;
                }
                // FM
                if (HasEffect(Buffs.FinishingMoveReady)
                    && !HasEffect(Buffs.LastDanceReady)
                    && (GetBuffRemainingTime(Buffs.FinishingMoveReady) < GetCooldownRemainingTime(Devilment) || GetCooldownRemainingTime(Buffs.FinishingMoveReady) - GetCooldownRemainingTime(Devilment) < 10)
                    && GetCooldownRemainingTime(StandardStep) < GCD)
                {
                    return OriginalHook(FinishingMove);
                }
                // SS
                if (IsEnabled(CustomComboPreset.DNC_ST_Adv_SS)
                    && StandardStep.LevelChecked()
                    && GetCooldownRemainingTime(StandardStep) < GCD
                    && (!TechnicalStep.LevelChecked() || GetCooldownRemainingTime(Devilment) > 4 + 6 + GCD)
                    && !HasEffect(Buffs.FinishingMoveReady))
                {
                    return StandardStep;
                }
                // SD
                if (LevelChecked(SaberDance) &&
                    gauge.Esprit >= 85)
                {
                    return SaberDance;
                }
                // FF
                if (LevelChecked(Fountainfall)
                    && flow)
                {
                    return Fountainfall;
                }
                // RC
                if (LevelChecked(ReverseCascade)
                    && symmetry)
                {
                    return ReverseCascade;
                }
                // F
                if (LevelChecked(Fountain)
                    && lastComboMove is Cascade
                    && comboTime > 0)
                {
                    return Fountain;
                }
                // C
                #endregion
                return actionID;
            }
        }
    }
    internal class Caster
    {
        internal class CustomCombo : CustomComboFunctions
        {
            public static uint invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (CanSpellWeave(actionID)
                    && All.LucidDreaming.LevelChecked()
                    && IsOffCooldown(All.LucidDreaming)
                    && LocalPlayer.CurrentMp < 8_000)
                {
                    Auto(All.LucidDreaming, actionID);
                }

                return actionID;
            }
        }
    }

    internal class IMNKPvP : MNKPvP
    {
        internal class IMNKPvP_Burst : CustomComboFunctions
        {
            public static uint invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is Bootshine or TrueStrike or SnapPunch or DragonKick or TwinSnakes or Demolish or PhantomRush)
                {
                    var MOD = WasLastWeaponskill(Enlightenment) ? 8_000 : 0;
                    if (LB_READY() 
                        && (EnemyHealthCurrentHp() > 0 && EnemyHealthCurrentHp() <= 24_000 + MOD))
                    {
                        return Meteordrive;
                    }

                    if (CanWeave(actionID, 0.2))
                    {
                        if (IsOffCooldown(RiddleOfEarth) && PlayerHealthPercentageHp() < 100)
                        {
                            return OriginalHook(RiddleOfEarth);
                        }

                        if (HasEffect(Buffs.EarthResonance)
                            && (GetBuffRemainingTime(Buffs.EarthResonance) < 6
                            || (LocalPlayer.CurrentMp < 2_500 && PlayerHealthPercentageHp() < 60)))
                        {
                            return OriginalHook(EarthsReply);
                        }

                        if ((!HasEffect(Buffs.WindResonance) && HasCharges(ThunderClap))
                            || (HasBattleTarget() && !InMeleeRange() && GetRemainingCharges(ThunderClap) > 1))
                        {
                            return OriginalHook(ThunderClap);
                        }
                    }

                    if (lastComboMove is Demolish)
                    {
                        if(TargetHasEffectAny(PvPCommon.Buffs.Guard))
                        {
                            return DRG.Stardiver;
                        }

                        if (HasCharges(RisingPhoenix)
                            && !HasEffect(Buffs.FireResonance)
                            && InActionRange(OriginalHook(RisingPhoenix)))
                        {
                            return OriginalHook(RisingPhoenix);
                        }

                        if (IsOffCooldown(SixSidedStar))
                        {
                            return OriginalHook(SixSidedStar);
                        }

                        if (IsOffCooldown(Enlightenment))
                        {
                            return OriginalHook(Enlightenment);
                        }

                        return OriginalHook(PhantomRush);
                    }
                }

                return actionID;
            }
        }
    }
}
