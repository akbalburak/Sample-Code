using Assets.Scripts.ApiModels;
using Assets.Scripts.Enums;
using Assets.Scripts.Extends;
using Assets.Scripts.Interfaces;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Controllers.Defense
{
    public class DefensePanelTabViewProduce : BaseLanguageBehaviour, ITabView
    {
        [SerializeField] private CostResourceController _costController;

        [SerializeField] private Slider _produceQuantitySlider;

        [SerializeField] private Button _produceButton;

        [SerializeField] private GameObject _alertObject;
        [SerializeField] private TMP_Text _alertObjectText;

        [SerializeField] private TMP_Text _defenseNameText;
        [SerializeField] private TMP_Text _currentProduceQuantityText;
        [SerializeField] private TMP_Text _produceTimeText;
        [SerializeField] private TMP_Text _bonusQuaterXText;

        [SerializeField] private string BeginProduceSFX;

        private DefensePanel.DefenseSelectionChangeArgs _currentDefense;

        private int _userPlanetId;
        private double _productionTime;
        private int _headquaterLevel;
        private int _maksProduceInDb;

        // UNITY EVENTS.
        private void Start()
        {
            _maksProduceInDb = DataController.Instance.GetParameter(ParameterTypes.MaximumProduceQuantity).ParameterIntValue;
        }
        private void OnEnable()
        {
            _produceQuantitySlider.onValueChanged.AddListener(OnSliderValueChanged);
            DefensePanel.OnDefenseSelectionChanged += OnDefenseSelectionChanged;

            _currentDefense = DefensePanel.GetSelectedDefense();
            LoadDefenseDetails();
        }
        private void OnDisable()
        {
            DefensePanel.OnDefenseSelectionChanged -= OnDefenseSelectionChanged;
            _produceQuantitySlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        // EVENTS.
        private void OnDefenseSelectionChanged(object sender, DefensePanel.DefenseSelectionChangeArgs e)
        {
            _currentDefense = e;
            LoadDefenseDetails();
        }

        // CORE.
        private void LoadDefenseDetails()
        {
            _userPlanetId = GlobalPlanetController.Instance.CurrentPlanet.UserPlanetId;
            _productionTime = DefenseBusSystem.GetDefenseUnitProduceTime(_userPlanetId, _currentDefense.Defense);
            _headquaterLevel = GlobalBuildingController.Instance.GetCurrentPlanetBuildingLevel(Buildings.YönetimMerkezi) + 1;

            _bonusQuaterXText.text = $"{_headquaterLevel}x";
            _defenseNameText.text = DefenseBusSystem.GetDefenseUnitName(_currentDefense.Defense);

            _produceQuantitySlider.maxValue = GetDefenseMaksQuantity();
            _produceQuantitySlider.minValue = 1;
            _produceQuantitySlider.SetValueWithoutNotify(1);
            OnSliderValueChanged(_produceQuantitySlider.value);
        }

        // HELPRERS.
        private bool IsReadyToProduce()
        {
            if (LoginController.Instance.CurrentUser.UserData.IsInHoliday)
            {
                SetError(base.GetLanguageText("TatilModuAktif"));

                return false;
            }

            bool isRobotUpgrading = LoginController.Instance.CurrentUser.UserPlanetsBuildingsUpgs
                .Exists(x => x.UserPlanetId == _userPlanetId && x.BuildingId == Buildings.RobotFabrikası);

            if (isRobotUpgrading)
            {
                SetError(base.GetLanguageText("RobotFabYükseltmeVar"));

                return false;
            }

            bool isManagementBuildingUpgrading = LoginController.Instance.CurrentUser.UserPlanetsBuildingsUpgs
                .Exists(x => x.UserPlanetId == _userPlanetId && x.BuildingId == Buildings.YönetimMerkezi);

            if (isManagementBuildingUpgrading)
            {
                SetError(base.GetLanguageText("YönetimMerkeziYükseltmeHata"));

                return false;
            }

            if (GlobalBuildingController.Instance.GetCurrentPlanetBuildingLevel(Buildings.RobotFabrikası) == 0)
            {
                SetError(base.GetLanguageText("RobotFabYok"));

                return false;
            }

            if (!TechnologyController.Instance.IsInvented(TechnologyCategories.Savunmalar, (int)_currentDefense.Defense))
            {
                SetError(base.GetLanguageText("SavunmaKoşulOlumsuz"));

                return false;
            }

            if (!GlobalPlanetController.Instance.CurrentPlanet.IsReqsEnough(GetDefenseCost()))
            {
                SetError(base.GetLanguageText("YetersizKaynak"));

                return false;
            }

            SetError(string.Empty);

            return true;
        }
        private void SetError(string message)
        {
            _alertObject.SetActive(!string.IsNullOrEmpty(message));
            _alertObjectText.text = message;
        }

        // CALCULATION HELPERS.
        private ResourcesDTO GetDefenseCost()
        {
            return _currentDefense.DefenseData.GetCost * GetDefenseQuantity();
        }
        private int GetDefenseMaksQuantity()
        {

            ResourcesDTO baseCost = _currentDefense.DefenseData.GetCost;

            int maksProdCount = 1;

            while (GlobalPlanetController.Instance.CurrentPlanet.IsReqsEnough(baseCost * maksProdCount) && maksProdCount < _maksProduceInDb)
                maksProdCount++;

            if (!GlobalPlanetController.Instance.CurrentPlanet.IsReqsEnough(baseCost * maksProdCount))
                maksProdCount -= 1;

            return maksProdCount;
        }
        private int GetDefenseQuantity()
        {
            return (int)_produceQuantitySlider.value;
        }

        // PREFAB LISTENERS.
        public void OnClickProduce()
        {
            string defenseName = DefenseBusSystem.GetDefenseUnitName(_currentDefense.Defense);

            GlobalPanelController.GPC.ShowPanel(PanelTypes.YesNoPanel).GetComponent<YesNoPanelController>()
               .LoadData(base.GetLanguageText("Uyarı"), base.GetLanguageText("ÜretimOnay", defenseName), (bool onYes) =>
               {
                   if (onYes)
                   {
                       int quantity = Mathf.Clamp(GetDefenseQuantity(), 1, _maksProduceInDb);

                       _produceButton.interactable = false;

                       LoadingController.Instance.ShowLoading();

                       TCPServer.Instance.SendToServer(ActionTypes.AddDefenseToDefenseQueue, new DefenseAddQueueRequestDTO
                       {
                           Quantity = quantity,
                           DefenseID = _currentDefense.Defense,
                           UserPlanetID = _userPlanetId
                       }, (response) =>
                       {
                           LoadingController.Instance.CloseLoading();

                           if (response.IsSuccess)
                           {
                               UserPlanetDefenseProgDTO responseData = response.GetData<UserPlanetDefenseProgDTO>();

                               DefenseBusSystem.CallDefenseProductionStarting(this, new DefenseBusSystem.OnDefenseProductionStartData
                               {
                                   Cost = _currentDefense.DefenseData.GetCost * responseData.DefenseCount,
                                   UserPlanetId = responseData.UserPlanetId,
                                   ProductionData = responseData,
                                   TotalProductionTime = DefenseBusSystem.GetDefenseProductionTotalQueueTime(responseData.UserPlanetId)
                               });

                               AudioController.Instance.PlaySoundOnCamera(BeginProduceSFX);
                           }

                       });
                   }
               });
        }
        public void OnClickTabItem()
        {
            LoadDefenseDetails();
        }
        public void OnClickSetQuantity()
        {
            if (!IsReadyToProduce())
                return;

            Sprite defenseImage = DefenseBusSystem.GetDefenseUnitImage(_currentDefense.Defense);

            string defenseName = DefenseBusSystem.GetDefenseUnitName(_currentDefense.Defense);

            GlobalPanelController.GPC.ShowPanel(PanelTypes.QuantityPanel)
                .GetComponent<QuantityItemPanel>()
                .LoadData(defenseImage, defenseName, GetDefenseQuantity(), GetDefenseMaksQuantity(), (e) =>
                {
                    _produceQuantitySlider.value = e.Quantity;
                });
        }
        public void OnSliderValueChanged(float value)
        {
            _currentProduceQuantityText.text = base.GetLanguageText("MiktarX", $"{Environment.NewLine}{(int)value}");

            _costController.SetResources(GetDefenseCost());

            double produceTime = _productionTime * Math.Ceiling(GetDefenseQuantity() / (float)_headquaterLevel);
            _produceTimeText.text = base.GetLanguageText("ÜretimSüresiX", TimeExtends.GetCountdownText(produceTime));

            bool isReadyToProduce = IsReadyToProduce();

            _produceButton.interactable = isReadyToProduce;
            _produceQuantitySlider.interactable = isReadyToProduce;
        }
    }
}