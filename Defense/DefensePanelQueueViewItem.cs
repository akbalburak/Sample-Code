using Assets.Scripts.ApiModels;
using Assets.Scripts.Enums;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Controllers.Defense
{
    public class DefensePanelQueueViewItem : BaseLanguageBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _quantityText;

        private int _userPlanetId;
        private int _headquaterLevel;

        private UserPlanetDefenseProgDTO _progressItem;

        public void LoadProgress(UserPlanetDefenseProgDTO progItem)
        {
            _headquaterLevel = GlobalBuildingController.Instance.GetCurrentPlanetBuildingLevel(Buildings.YönetimMerkezi) + 1;
            _userPlanetId = GlobalPlanetController.Instance.CurrentPlanet.UserPlanetId;

            _progressItem = progItem;

            _iconImage.sprite = DefenseBusSystem.GetDefenseUnitImage(_progressItem.DefenseId);

            RefreshQuantity();

            CancelInvoke(nameof(RefreshQuantity));
            InvokeRepeating(nameof(RefreshQuantity), 0, 1);
        }

        public void RefreshQuantity()
        {
            _quantityText.text = $"{_progressItem.DefenseCount}x";

            if (_progressItem.DefenseCount <= 0)
                Destroy(gameObject);
        }

        public void OnClickFastComplete()
        {
            UserPlanetDefenseProgDTO activeDefenseProg = LoginController.Instance.CurrentUser.UserPlanetDefenseProgs.Find(x => x.UserPlanetId == _userPlanetId);

            int requiredDarkMaterialQuantity = CalculateRequiredDarkMaterial(activeDefenseProg, _progressItem);
            if (requiredDarkMaterialQuantity == 0)
                return;

            string defenseName = DefenseBusSystem.GetDefenseUnitName(_progressItem.DefenseId);

            if (LoginController.Instance.CurrentUser.UserData.DarkMaterial < requiredDarkMaterialQuantity)
            {
                string qcontent = base.GetLanguageText("HýzlýTamamlaÜretimYetersizUyarý", defenseName, requiredDarkMaterialQuantity.ToString());

                GlobalPanelController.GPC.ShowPanel(PanelTypes.YesNoPanel).GetComponent<YesNoPanelController>().LoadData(base.GetLanguageText("Uyarý"), qcontent, (bool onClick) =>
                {
                    if (onClick)
                    {
                        StoreController.Instance.ShowStorePanel(StoreCategories.Kaynaklar);
                    }
                });

                return;
            }

            string content = base.GetLanguageText("HýzlýTamamlaSavunmaUyarý", defenseName, requiredDarkMaterialQuantity.ToString());

            GlobalPanelController.GPC.ShowPanel(PanelTypes.YesNoPanel).GetComponent<YesNoPanelController>()
                .LoadData(base.GetLanguageText("Uyarý"), content, (bool onClick) =>
                {
                    if (onClick == true)
                    {
                        FastCompleteForDefenseDTO requestData = new FastCompleteForDefenseDTO
                        {
                            DefenseProgId = _progressItem.UserPlanetDefenseProgId,
                            UserPlanetId = _userPlanetId
                        };

                        LoadingController.Instance.ShowLoading();

                        TCPServer.Instance.SendToServer(ActionTypes.FastCompleteForDefenses, requestData, (response) =>
                        {
                            LoadingController.Instance.CloseLoading();

                            if (response.IsSuccess)
                            {
                                FastCompleteForDefenseResponseDTO responseData = response.GetData<FastCompleteForDefenseResponseDTO>();
                                LoginController.Instance.CurrentUser.UserData.DarkMaterial -= responseData.SpentDarkMaterial;

                                UserPlanetDefenseProgDTO progress = LoginController.Instance.CurrentUser.UserPlanetDefenseProgs
                                .Find(x => x.UserPlanetDefenseProgId == responseData.UserPlanetDefenseProgId);

                                if (progress != null)
                                {
                                    DefenseBusSystem.CallDefenseProducing(new DefenseBusSystem.DefenseProducedEventArgs(
                                        progress: progress,
                                        producedCount: progress.DefenseCount,
                                        isAllDone: true
                                    ));
                                }

                                GlobalPanelController.GPC.ShowPanel(PanelTypes.FooterInfoPanel)
                                .GetComponent<FooterInfoPanelController>()
                                .LoadText(base.GetLanguageText("SavunmaÜretimHýzlýTamamlandý", defenseName), FooterInfoPanelController.FooterInfoTypes.Success);

                                Destroy(gameObject);
                            }
                            else
                            {
                                GlobalPanelController.GPC.ShowPanel(PanelTypes.FooterInfoPanel)
                                .GetComponent<FooterInfoPanelController>()
                                .LoadText(base.GetLanguageText("SavunmaÜretimHIzlýHata", defenseName), FooterInfoPanelController.FooterInfoTypes.Failed);
                            }
                        });
                    }
                });
        }

        private int CalculateRequiredDarkMaterial(UserPlanetDefenseProgDTO activeProg, UserPlanetDefenseProgDTO prog)
        {
            DateTime currentDate = DateTime.UtcNow;

            double defenseProduceTime = DefenseBusSystem.GetDefenseUnitProduceTime(_userPlanetId, prog.DefenseId);

            defenseProduceTime *= (int)Math.Ceiling(prog.DefenseCount / (float)_headquaterLevel);

            if (activeProg.UserPlanetDefenseProgId == prog.UserPlanetDefenseProgId)
                defenseProduceTime -= (currentDate - prog.LastVerifyDate).TotalSeconds;

            defenseProduceTime -= prog.PassedSeconds;

            int totalRequiredDarkMaterial = GlobalResourceController.GRC.CalculateDarkMaterialCost(defenseProduceTime);

            return totalRequiredDarkMaterial;
        }
    }
}