using Assets.Scripts.ApiModels;
using Assets.Scripts.Enums;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Controllers.Defense
{
    public class DefensePanel : BasePanelController
    {
        [Header("Panel Properties.")]
        [SerializeField] private DefensePanelItem _defenseItem;
        [SerializeField] private Transform _defenseItemContent;

        [SerializeField] private RawImage _reflectionTextureImage;
        [SerializeField] private TMP_Text _defenseNameText;
        [SerializeField] private TMP_Text _defenseQuantityText;

        private DefenseSelectionChangeArgs _currentSelection;
        private int _userPlanetId;

        private void OnEnable()
        {
            OnDefenseSelectionChanged += OnDefenseSelected;

            BusSystem.User.Research.OnResearchCompleted += OnResearchCompleted;
            BusSystem.Game.OnCurrentSelectedPlanetChanged += OnCurrentSelectedPlanetChanged;
            BusSystem.User.Building.OnBuildingUpgradeCompleted += OnBuildingUpgradeCompleted;

            OnGetSelectedDefense += ReturnSelectedDefense;

            LoadAllDefenses();
        }
        private void OnDisable()
        {
            OnDefenseSelectionChanged -= OnDefenseSelected;

            BusSystem.User.Research.OnResearchCompleted -= OnResearchCompleted;
            BusSystem.Game.OnCurrentSelectedPlanetChanged -= OnCurrentSelectedPlanetChanged;
            BusSystem.User.Building.OnBuildingUpgradeCompleted -= OnBuildingUpgradeCompleted;

            OnGetSelectedDefense -= ReturnSelectedDefense;
        }

        private DefenseSelectionChangeArgs ReturnSelectedDefense()
        {
            return _currentSelection;
        }

        private void OnDefenseSelected(object sender, DefenseSelectionChangeArgs e)
        {
            _currentSelection = e;

            _defenseNameText.text = DefenseBusSystem.GetDefenseUnitName(_currentSelection.Defense);
            _userPlanetId = GlobalPlanetController.Instance.CurrentPlanet.UserPlanetId;

            string unitCountText = $"{DefenseBusSystem.GetDefenseUnitCount(_userPlanetId, e.Defense)}";
            _defenseQuantityText.text = base.GetLanguageText("YeşilMevcutX", unitCountText);

            PodiumController.Instance.ShowPodium(_reflectionTextureImage, e.Defense);
        }
        private void OnBuildingUpgradeCompleted(UserPlanetBuildingUpgDTO upgData)
        {
            if (upgData.BuildingId != Buildings.YönetimMerkezi)
                return;

            if (upgData.UserPlanetId != _userPlanetId)
                return;

            LoadAllDefenses();
        }
        private void OnCurrentSelectedPlanetChanged(UserPlanetDTO selectedPLanet)
        {
            LoadAllDefenses();
        }
        private void OnResearchCompleted(Researches research)
        {
            LoadAllDefenses();
        }

        private void LoadAllDefenses()
        {
            foreach (Transform child in _defenseItemContent)
                Destroy(child.gameObject);

            foreach (DefenseDataDTO defenseData in DataController.Instance.SystemData.Defenses)
            {
                DefensePanelItem defenseItem = Instantiate(_defenseItem, _defenseItemContent);
                defenseItem.LoadData(defenseData.DefenseId);

                if (_currentSelection == null || _currentSelection.Defense == defenseData.DefenseId)
                {
                    CallDefenseSelected(defenseItem, new DefenseSelectionChangeArgs
                    {
                        Defense = defenseData.DefenseId,
                        DefenseData = DataController.Instance.GetDefense(defenseData.DefenseId)
                    });
                }
            }
        }
        public void LoadAllDefensesWithSelection(Defenses defense)
        {
            _currentSelection = new DefenseSelectionChangeArgs
            {
                Defense = defense,
                DefenseData = DataController.Instance.GetDefense(defense)
            };

            LoadAllDefenses();
        }

        protected override void OnTransionCompleted(bool isClosed)
        {
            base.OnTransionCompleted(isClosed);

            if (isClosed)
                PodiumController.Instance.DisablePodium();
        }



        // WHEN THE SELECTION CHANGED.
        public class DefenseSelectionChangeArgs : EventArgs
        {
            public Defenses Defense;
            public DefenseDataDTO DefenseData;
        }

        public static event EventHandler<DefenseSelectionChangeArgs> OnDefenseSelectionChanged;
        public static void CallDefenseSelected(object sender, DefenseSelectionChangeArgs selectData)
        {
            OnDefenseSelectionChanged?.Invoke(sender, selectData);
        }

        // WHEN REQUESTED SELECTED DEFENSE.
        public delegate DefenseSelectionChangeArgs GetSelectedDefenseDelegate();

        public static GetSelectedDefenseDelegate OnGetSelectedDefense;
        public static DefenseSelectionChangeArgs GetSelectedDefense()
        {
            return OnGetSelectedDefense.Invoke();
        }
    }
}