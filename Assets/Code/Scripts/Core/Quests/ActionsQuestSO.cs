using System.Linq;
using UnityEngine;

namespace FrostfallSaga.Core.Quests
{
    [CreateAssetMenu(fileName = "ActionsQuest", menuName = "ScriptableObjects/Quests/ActionsQuest", order = 0)]
    public class ActionsQuestSO : AQuestSO
    {
        [SerializeReference] public AQuestAction[] actions;

        [field: SerializeField]
        [field: Tooltip("If the actions should be done in order or not.")]
        public bool OrderedActions { get; private set; }
        public string ConclusionText { get; private set; }

        /// <summary>
        ///     Start listening to the events that will update the action completion.
        ///     If the actions should be done in order, only initialize the first non completed action, otherwise initialize all of
        ///     them.
        /// </summary>
        /// <param name="sceneManager">The specific manager of the scene the action is related to.</param>
        public override void Initialize(MonoBehaviour sceneManager)
        {
            foreach (AQuestAction action in actions)
                if (!action.IsCompleted)
                {
                    action.Initialize(sceneManager);
                    action.onActionCompleted += OnActionCompleted;

                    if (OrderedActions) return;
                }
        }

        private void OnActionCompleted(AQuestAction action)
        {
            if (actions.All(action => action.IsCompleted)) CompleteQuest();
        }
    }
}