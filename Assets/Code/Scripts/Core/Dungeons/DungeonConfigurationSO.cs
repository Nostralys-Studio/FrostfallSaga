using System;
using FrostfallSaga.Grid;
using FrostfallSaga.Core.Rewards;
using UnityEngine;

namespace FrostfallSaga.Core.Dungeons
{
    [CreateAssetMenu(fileName = "DungeonConfiguration", menuName = "ScriptableObjects/Dungeons/DungeonConfiguration",
        order = 0)]
    public class DungeonConfigurationSO : ScriptableObject
    {
        [field: SerializeField] public DungeonFightConfiguration BossFightConfiguration { get; private set; }
        [field: SerializeField] public DungeonFightConfiguration[] PreBossFightConfigurations { get; private set; }
        [field: SerializeField] public RewardConfiguration RewardConfiguration { get; private set; }
        [field: SerializeField] public Reward EarnedReward { get; private set; }
        [field: SerializeField] public BiomeTypeSO BiomeType { get; private set; }

        public Action<DungeonConfigurationSO> onDungeonCompleted;

        public void CompleteDungeon()
        {
            // Reward the player
            EarnedReward = RewardConfiguration.GenerateReward();
            HeroTeam.HeroTeam.Instance.CollectReward(EarnedReward);

            onDungeonCompleted?.Invoke(this);
        }
    }
}