using Assets.Scripts.Enums;
using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Controllers.Defense
{
    public class DefensePanelTabViewCondition : MonoBehaviour, ITabView
    {
        [SerializeField] private ConditionViewController _conditionView;

        private DefensePanel.DefenseSelectionChangeArgs _currentDefense;

        // UNITY EVENTS.
        private void OnEnable()
        {
            DefensePanel.OnDefenseSelectionChanged += OnDefenseSelectionChanged;

            _currentDefense = DefensePanel.GetSelectedDefense();
            LoadData();
        }
        private void OnDisable()
        {
            DefensePanel.OnDefenseSelectionChanged -= OnDefenseSelectionChanged;
        }

        // EVENTS.
        private void OnDefenseSelectionChanged(object sender, DefensePanel.DefenseSelectionChangeArgs e)
        {
            _currentDefense = e;
            LoadData();
        }

        // CORE.
        private void LoadData()
        {
            this._conditionView.LoadData(TechnologyCategories.Savunmalar, (int)_currentDefense.Defense);
        }

        // PREFAB LISTENERS.
        public void OnClickTabItem()
        {
            LoadData();
        }
    }
}