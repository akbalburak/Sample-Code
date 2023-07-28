using Assets.Scripts.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Controllers.Defense
{
    public class DefensePanelItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _countText;

        [SerializeField] private Image _iconImage;
        
        [SerializeField] private GameObject _lockObject;

        [SerializeField] private Color _notInventedDefenseColor;

        private Defenses _defense;

        // UNITY EVENTS.
        private void OnEnable()
        {
            DefenseBusSystem.OnDefenseUnitCountChanged += OnDefenseUnitCountChanged;
        }
        private void OnDisable()
        {
            DefenseBusSystem.OnDefenseUnitCountChanged -= OnDefenseUnitCountChanged;
        }

        // EVENTS.
        private void OnDefenseUnitCountChanged(object sender, DefenseBusSystem.DefenseUnitCountChangedEventArgs e)
        {
            if (e.Defense != _defense)
                return;

            if (e.UserPlanetID != GlobalPlanetController.Instance.CurrentPlanet.UserPlanetId)
                return;

            UpdateQuantity();
        }

        // CORE
        public void LoadData(Defenses defense)
        {
            _defense = defense;

            int currentPlanetId = GlobalPlanetController.Instance.CurrentPlanet.UserPlanetId;

            _nameText.text = DefenseBusSystem.GetDefenseUnitName(defense);
            _iconImage.sprite = DefenseBusSystem.GetDefenseUnitImage(defense);
            _countText.text = $"{DefenseBusSystem.GetDefenseUnitCount(currentPlanetId, defense)}";

            if (!TechnologyController.Instance.IsInvented(TechnologyCategories.Savunmalar, (int)defense))
            {
                _iconImage.color = _notInventedDefenseColor;
                _lockObject.SetActive(true);
            }
            else
            {
                _iconImage.color = Color.white;
                _lockObject.SetActive(false);
            }

            UpdateQuantity();

            gameObject.SetActive(true);
        }
        private void UpdateQuantity()
        {
            int currentPlanetId = GlobalPlanetController.Instance.CurrentPlanet.UserPlanetId;
            _countText.text = $"{DefenseBusSystem.GetDefenseUnitCount(currentPlanetId, _defense)}";
        }

        // PREFAB LISTENERS.
        public void OnClickItem()
        {
            DefensePanel.CallDefenseSelected(this, new DefensePanel.DefenseSelectionChangeArgs
            {
                Defense = _defense,
                DefenseData = DataController.Instance.GetDefense(_defense),
            });
        }
    }
}