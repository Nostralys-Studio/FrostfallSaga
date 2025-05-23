using System;
using FrostfallSaga.Fight.FightCells.Impediments;
using UnityEngine;

namespace FrostfallSaga.Fight.FightCells.FightCellAlterations
{
    [Serializable]
    /// <summary>
    /// Alteration that adds an impediment to a cell.
    /// </summary>
    public class AddImpedimentAlteration : AFightCellAlteration
    {
        [field: SerializeField]
        [field: Header("Impediment alteration definition")]
        public AImpedimentSO Impediment { get; private set; }

        private FightCell _currentlyModifiedCell;

        public AddImpedimentAlteration() : base(
            "Add impediment",
            "Adds an impediment to the cell.", 
            null, 
            false, 
            0, 
            false, 
            false
        )
        {
        }

        public AddImpedimentAlteration(
            string name,
            string description,
            Sprite icon,
            bool isPermanent,
            int duration
        ) : base(name, description, icon, isPermanent, duration, false, false)
        {
        }

        #region Application

        public override void Apply(FightCell fightCell)
        {
            Impediment.SpawnController.onSpawnEnded += OnImpedimentGameObjectSpawned;

            _currentlyModifiedCell = fightCell;
            Impediment.SpawnController.SpawnGameObject(fightCell.transform, Impediment.Prefab);
        }

        private void OnImpedimentGameObjectSpawned(GameObject impedimentGameObject)
        {
            _currentlyModifiedCell.SetImpediment(Impediment, impedimentGameObject);
            _currentlyModifiedCell = null;
            Impediment.SpawnController.onSpawnEnded -= OnImpedimentGameObjectSpawned;
            onAlterationApplied?.Invoke(_currentlyModifiedCell, this);
        }

        #endregion

        #region Removal

        public override void Remove(FightCell fightCell)
        {
            Impediment.destroyController.onDestroyEnded += OnImpedimentGameObjectDestroyed;

            _currentlyModifiedCell = fightCell;
            Impediment.destroyController.DestroyGameObject(fightCell.GetImpedimentGameObject());
        }

        private void OnImpedimentGameObjectDestroyed()
        {
            _currentlyModifiedCell.SetImpediment(null, null);
            _currentlyModifiedCell = null;
            Impediment.destroyController.onDestroyEnded -= OnImpedimentGameObjectDestroyed;
            onAlterationRemoved?.Invoke(_currentlyModifiedCell, this);
        }

        #endregion
    }
}