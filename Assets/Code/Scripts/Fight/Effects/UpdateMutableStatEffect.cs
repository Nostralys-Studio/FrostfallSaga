using System;
using FrostfallSaga.Core.Fight;
using FrostfallSaga.Fight.Fighters;
using UnityEngine;

namespace FrostfallSaga.Fight.Effects
{
    /// <summary>
    ///     Effect that applies heal to the target fighter.
    /// </summary>
    [Serializable]
    public class UpdateMutableStatEffect : AEffect
    {
        [field: SerializeField] public EFighterMutableStat StatToUpdate;
        [field: SerializeField] public int Amount;
        [field: SerializeField] public bool UsePercentage;

        public override void ApplyEffect(
            Fighter receiver,
            bool isMasterstroke,
            Fighter initiator = null,
            bool adjustGodFavorsPoints = true
        )
        {
            float finalUpdateAmount = Amount;
            if (UsePercentage) finalUpdateAmount = receiver.GetMutableStat(StatToUpdate) * Amount / 100f;

            // Do the update
            receiver.UpdateMutableStat(StatToUpdate, finalUpdateAmount);
            Debug.Log($"{receiver.name} {StatToUpdate} updated by {finalUpdateAmount}.");

            // Increase god favors points if enbabled
            if (adjustGodFavorsPoints && initiator != null)
            {
                EGodFavorsAction actionDone = Amount > 0 ? EGodFavorsAction.BUFF : EGodFavorsAction.DEBUFF;
                receiver.TryIncreaseGodFavorsPointsForAction(actionDone);
            }
        }

        public override void RestoreEffect(Fighter receiver)
        {
            int finalUpdateAmount = Amount;
            if (UsePercentage) finalUpdateAmount = (int)(receiver.GetMutableStat(StatToUpdate) * Amount / 100f);

            receiver.UpdateMutableStat(StatToUpdate, -finalUpdateAmount, true, false);
        }

        public override int GetPotentialEffectDamages(Fighter initiator, Fighter receiver, bool canMasterstroke)
        {
            return 0;
        }

        public override int GetPotentialEffectHeal(Fighter initiator, Fighter receiver, bool canMasterstroke)
        {
            return 0;
        }

        public override string GetUIEffectDescription()
        {
            if (UsePercentage)
            {
                string verb = Amount > 0 ? "Increases" : "Decreases";
                return $"{verb} {StatToUpdate} by <b>{Amount}%</b>.";
            }

            string sign = Amount > 0 ? "+" : "-";
            return $"{sign}{Amount} {StatToUpdate}";
        }
    }
}