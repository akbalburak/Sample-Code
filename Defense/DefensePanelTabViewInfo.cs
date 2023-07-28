using Assets.Scripts.ApiModels;
using Assets.Scripts.Enums;
using Assets.Scripts.Interfaces;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Controllers.Defense
{
    public class DefensePanelTabViewInfo : BaseLanguageBehaviour, ITabView
    {
        [SerializeField] private TMP_Text _itemNameWithQuantity;
        [SerializeField] private TMP_Text _itemDescription;

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
        public void LoadData()
        {
            _itemNameWithQuantity.text = DefenseBusSystem.GetDefenseUnitName(_currentDefense.Defense);
            _itemDescription.text = DefenseBusSystem.GetDefenseUnitDescription(_currentDefense.Defense);
        }

        // PREFAB LISTENERS.
        public void OnClickTabItem()
        {
            LoadData();
        }
    }
}