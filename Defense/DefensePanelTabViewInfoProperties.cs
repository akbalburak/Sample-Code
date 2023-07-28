using Assets.Scripts.Enums;
using Assets.Scripts.Extends;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Controllers.Defense
{
    public class DefensePanelTabViewInfoProperties : BaseLanguageBehaviour
    {
        [SerializeField] private GameObject _propertyDetailContainer;

        [SerializeField] private TMP_Text _propertyTitle;
        [SerializeField] private TMP_Text _propertyValue;

        [SerializeField] private List<DefensePropertyWithButtons> _defensePropertyAndButtons;

        private DefensePanel.DefenseSelectionChangeArgs _currentDefense;

        // UNITY EVENTS.
        private void Start()
        {
            foreach (var property in _defensePropertyAndButtons)
            {
                property.DefensePropertyButton.onClick.AddListener(() =>
                {
                    ShowPropertyDetailMiniPanel(property.DefenseProperty);
                });
            }
        }
        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && _propertyDetailContainer.activeSelf)
                _propertyDetailContainer.SetActive(false);
        }
        private void OnEnable()
        {
            DefensePanel.OnDefenseSelectionChanged += OnDefenseSelectionChanged;

            _currentDefense = DefensePanel.GetSelectedDefense();
            LoadDefenseProperties();
        }
        private void OnDisable()
        {
            DefensePanel.OnDefenseSelectionChanged -= OnDefenseSelectionChanged;
        }

        // EVENTS.
        private void OnDefenseSelectionChanged(object sender, DefensePanel.DefenseSelectionChangeArgs e)
        {
            _currentDefense = e;
            LoadDefenseProperties();
        }

        // CORE.
        public void LoadDefenseProperties()
        {
            #region DEFENSE

            var defense = _defensePropertyAndButtons.Find(x => x.DefenseProperty == DefenseProperties.Defense);

            Tuple<double, double, double> defenseArmor = _currentDefense.DefenseData.GetArmor;
            defense.DefensePropertyText.text = ResourceExtends.ConvertToDottedResource(defenseArmor.Item3);

            #endregion

            #region ATTACK

            var attack = _defensePropertyAndButtons.Find(x => x.DefenseProperty == DefenseProperties.AttackDamage);

            Tuple<double, double, double> defenseAttack = _currentDefense.DefenseData.GetAttackDamage;
            attack.DefensePropertyText.text = ResourceExtends.ConvertToDottedResource(defenseAttack.Item3);

            #endregion

            #region SHOOT COUNT

            var shootCount = _defensePropertyAndButtons.Find(x => x.DefenseProperty == DefenseProperties.AttackCount);

            Tuple<double, double, double> defenseAttackCount = _currentDefense.DefenseData.GetAttackQuantity;
            shootCount.DefensePropertyText.text = ResourceExtends.ConvertToDottedResource(defenseAttackCount.Item3);

            #endregion

        }
        public void ShowPropertyDetailMiniPanel(DefenseProperties property)
        {
            var parent = _defensePropertyAndButtons.Find(x => x.DefenseProperty == property).DefensePropertyButton.transform;
            _propertyDetailContainer.transform.SetParent(parent);

            RectTransform position = _propertyDetailContainer.transform.GetComponent<RectTransform>();

            position.anchoredPosition = new Vector2(0, 100);

            _propertyDetailContainer.transform.SetParent(transform);

            _propertyDetailContainer.SetActive(true);

            _propertyTitle.text = base.GetLanguageText($"DP{(int)property}");

            switch (property)
            {
                case DefenseProperties.Defense:
                    Tuple<double, double, double> defenseHealth = _currentDefense.DefenseData.GetArmor;
                    _propertyValue.text = base.GetLanguageText("Savunma÷zellikleri", ResourceExtends.ConvertToDottedResource(defenseHealth.Item1), ResourceExtends.ConvertToDottedResource(defenseHealth.Item2), ResourceExtends.ConvertToDottedResource(defenseHealth.Item3));
                    break;
                case DefenseProperties.AttackDamage:
                    Tuple<double, double, double> defenseAttack = _currentDefense.DefenseData.GetAttackDamage;
                    _propertyValue.text = base.GetLanguageText("Savunma÷zellikleri", ResourceExtends.ConvertToDottedResource(defenseAttack.Item1), ResourceExtends.ConvertToDottedResource(defenseAttack.Item2), ResourceExtends.ConvertToDottedResource(defenseAttack.Item3));
                    break;
                case DefenseProperties.AttackCount:
                    Tuple<double, double, double> defenseAttackQuantity = _currentDefense.DefenseData.GetAttackQuantity;
                    _propertyValue.text = base.GetLanguageText("Savunma÷zellikleri", ResourceExtends.ConvertToDottedResource(defenseAttackQuantity.Item1), ResourceExtends.ConvertToDottedResource(defenseAttackQuantity.Item2), ResourceExtends.ConvertToDottedResource(defenseAttackQuantity.Item3));
                    break;
            }

        }

        [Serializable]
        public class DefensePropertyWithButtons
        {
            public DefenseProperties DefenseProperty;
            public Button DefensePropertyButton;
            public TMP_Text DefensePropertyText;
        }
    }
}