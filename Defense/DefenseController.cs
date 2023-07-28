using Assets.Scripts.ApiModels;
using Assets.Scripts.Enums;
using Assets.Scripts.Extends;
using Assets.Scripts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Controllers.Defense
{
    public class DefenseController : Singleton<DefenseController>
    {
        [SerializeField] private GameObject _defenseProgressIcon;
        [SerializeField] private List<DefenseImageDTO> _defenseWithImages;

        // CORE
        private void ShowDefensePanel()
        {
            GlobalPanelController.GPC.ClosePanel(PanelTypes.DefensePanel);
            GlobalPanelController.GPC.ShowPanel(PanelTypes.DefensePanel);
        }
        private void ReCalculateDefenseProgress()
        {
            DateTime currentDate = DateTime.UtcNow;

            foreach (UserPlanetDTO userPlanet in LoginController.Instance.CurrentUser.UserPlanets)
            {
                UserPlanetDefenseProgDTO firstDefenseProg = LoginController.Instance.CurrentUser.UserPlanetDefenseProgs.FirstOrDefault(x => x.UserPlanetId == userPlanet.UserPlanetId);
                if (firstDefenseProg == null)
                    continue;

                DateTime lastVerifyDate = firstDefenseProg.LastVerifyDate.AddSeconds(-firstDefenseProg.PassedSeconds);

                double passedTime = (currentDate - lastVerifyDate).TotalSeconds;
                double countdownOneItem = GetDefenseProduceTimeInSeconds(userPlanet.UserPlanetId, firstDefenseProg.DefenseId);

                int totalProducedCount = Mathf.FloorToInt((float)passedTime / (float)countdownOneItem);
                if (totalProducedCount == 0)
                    continue;

                int perQuantity = 1 + GlobalBuildingController.Instance.GetPlanetBuildingLevel(userPlanet.UserPlanetId, Buildings.YönetimMerkezi);

                int totalProducedCountWithHeadQuaters = Mathf.Clamp(totalProducedCount * perQuantity, 0, firstDefenseProg.DefenseCount);

                firstDefenseProg.PassedSeconds = 0;
                firstDefenseProg.LastVerifyDate = firstDefenseProg.LastVerifyDate.AddSeconds(-firstDefenseProg.PassedSeconds + totalProducedCount * countdownOneItem);

                DefenseBusSystem.CallDefenseProducing(new DefenseBusSystem.DefenseProducedEventArgs(
                    progress: firstDefenseProg,
                    producedCount: totalProducedCountWithHeadQuaters,
                    isAllDone: firstDefenseProg.DefenseCount <= 0
                    ));
            }

            RefreshProgressIcon();
        }
        private void RefreshProgressIcon()
        {
            if (LoginController.Instance.CurrentUser.UserPlanetDefenseProgs.Exists(x => x.UserPlanetId == GlobalPlanetController.Instance.CurrentPlanet.UserPlanetId))
            {
                if (!_defenseProgressIcon.activeSelf)
                    _defenseProgressIcon.SetActive(true);
            }
            else
            {
                if (_defenseProgressIcon.activeSelf)
                    _defenseProgressIcon.SetActive(false);
            }
        }

        // UNITY EVENTS
        private void OnEnable()
        {
            BusSystem.Data.OnGameLoaded += OnGameLoaded;
            DefenseBusSystem.OnDefenseProduced += OnDefenseProduced;
            DefenseBusSystem.OnDefenseProducing += OnDefenseProducing;
            DefenseBusSystem.OnDefenseUnitCountChanging += OnDefenseUnitCountChanging;

            DefenseBusSystem.OnGetDefenseUnitName += GetDefenseName;
            DefenseBusSystem.OnGetDefenseUnitDescription += GetDefenseDescription;
            DefenseBusSystem.OnGetDefenseUnitImage += GetDefenseImage;
            DefenseBusSystem.OnGetDefenseUnitCountData += GetDefenseQuantityInPlanet;
            DefenseBusSystem.OnGetDefenseUnitPodium += GetDefensePodium;
            DefenseBusSystem.OnGetDefenseUnitProduceTime += GetDefenseProduceTimeInSeconds;
            DefenseBusSystem.OnGetDefenseProductionTotalQueueTime += GetTotalProductionTimeOfTheQueue;

            DefenseBusSystem.OnShowDefensePanel += ShowDefensePanel;
        }
        private void OnDisable()
        {
            BusSystem.Data.OnGameLoaded -= OnGameLoaded;
            DefenseBusSystem.OnDefenseProduced -= OnDefenseProduced;
            DefenseBusSystem.OnDefenseProducing -= OnDefenseProducing;
            DefenseBusSystem.OnDefenseUnitCountChanging -= OnDefenseUnitCountChanging;

            DefenseBusSystem.OnGetDefenseUnitName -= GetDefenseName;
            DefenseBusSystem.OnGetDefenseUnitDescription -= GetDefenseDescription;
            DefenseBusSystem.OnGetDefenseUnitImage -= GetDefenseImage;
            DefenseBusSystem.OnGetDefenseUnitCountData -= GetDefenseQuantityInPlanet;
            DefenseBusSystem.OnGetDefenseUnitPodium -= GetDefensePodium;
            DefenseBusSystem.OnGetDefenseUnitProduceTime -= GetDefenseProduceTimeInSeconds;
            DefenseBusSystem.OnGetDefenseProductionTotalQueueTime -= GetTotalProductionTimeOfTheQueue;

            DefenseBusSystem.OnShowDefensePanel -= ShowDefensePanel;
        }

        // CALLBACKS.
        private void OnDefenseUnitCountChanging(object sender, DefenseBusSystem.DefenseUnitCountChangingEventArgs e)
        {
            if (e.ChangeCount > 0)
                AddDefense(e.UserPlanetID, e.Defense, e.ChangeCount);
            else
                RemoveDefense(e.UserPlanetID, e.Defense, e.ChangeCount);
        }
        private void OnDefenseProducing(object sender, DefenseBusSystem.DefenseProducedEventArgs e)
        {
            e.Progress.DefenseCount -= e.ProducedCount;

            if (!e.IsAllDone)
            {
                DefenseBusSystem.CallDefenseProduced(e);
                return;
            }

            LoginController.Instance.CurrentUser.UserPlanetDefenseProgs.Remove(e.Progress);

            UserPlanetDefenseProgDTO nextProg = LoginController.Instance.CurrentUser.UserPlanetDefenseProgs.Find(x => x.UserPlanetId == e.Progress.UserPlanetId);
            if (nextProg != null)
                nextProg.LastVerifyDate = e.Progress.LastVerifyDate;

            DefenseBusSystem.CallDefenseProduced(e);

            if (nextProg == null)
            {
                DefenseBusSystem.CallAllDefensesProduced(new DefenseBusSystem.AllDefenseProducedEventArgs(
                    userPlanetID: e.Progress.UserPlanetId
                ));
            }
        }
        private void OnDefenseProduced(object sender, DefenseBusSystem.DefenseProducedEventArgs e)
        {
            DefenseBusSystem.CallAddDefenseUnit(new DefenseBusSystem.DefenseUnitCountChangingEventArgs(
                userPlanetID: e.Progress.UserPlanetId,
                defense: e.Progress.DefenseId,
                changeCount: e.ProducedCount
            ));
        }
        private void OnGameLoaded()
        {
            InvokeRepeating(nameof(ReCalculateDefenseProgress), 0, 1);
        }

        // ADD AND REMOVE METHODS.
        private void AddDefense(int userPlanetId, Defenses defenseId, int quantity)
        {
            UserPlanetDefenseDTO defense = LoginController.Instance.CurrentUser.UserPlanetDefenses.Find(x => x.UserPlanetId == userPlanetId && x.DefenseId == defenseId);

            if (defense == null)
            {
                defense = new UserPlanetDefenseDTO
                {
                    UserPlanetId = userPlanetId,
                    DefenseId = defenseId,
                    DefenseCount = quantity
                };

                LoginController.Instance.CurrentUser.UserPlanetDefenses.Add(defense);
            }
            else
                defense.DefenseCount += quantity;

            DefenseBusSystem.CallDefenseUnitCountChanged(new DefenseBusSystem.DefenseUnitCountChangedEventArgs(
                userPlanetID: userPlanetId,
                defense: defenseId,
                newCount: defense.DefenseCount,
                oldCount: defense.DefenseCount - quantity
            ));
        }
        private void RemoveDefense(int userPlanetId, Defenses defenseId, int quantity)
        {
            UserPlanetDefenseDTO defense = LoginController.Instance.CurrentUser.UserPlanetDefenses.Find(x => x.UserPlanetId == userPlanetId && x.DefenseId == defenseId);

            if (defense == null)
                return;

            defense.DefenseCount -= quantity;

            if (defense.DefenseCount <= 0)
                LoginController.Instance.CurrentUser.UserPlanetDefenses.Remove(defense);

            DefenseBusSystem.CallDefenseUnitCountChanged(new DefenseBusSystem.DefenseUnitCountChangedEventArgs(
                userPlanetID: userPlanetId,
                defense: defenseId,
                newCount: defense.DefenseCount,
                oldCount: defense.DefenseCount - quantity
            ));
        }

        // DATA GET METHODS.
        private Sprite GetDefenseImage(Defenses defense) => _defenseWithImages.Find(x => x.Defense == defense)?.DefenseImage;
        private string GetDefenseName(Defenses defense) => base.GetLanguageText($"D{(int)defense}");
        private string GetDefenseDescription(Defenses defense) => base.GetLanguageText($"DD{(int)defense}");
        private GameObject GetDefensePodium(Defenses defense)
        {
            return _defenseWithImages.Find(x => x.Defense == defense).DefensePodium;
        }

        // USER DATA GET METHODS
        private int GetDefenseQuantityInPlanet(int userPlanetId, Defenses defense)
        {
            UserPlanetDefenseDTO defenseData = LoginController.Instance.CurrentUser.UserPlanetDefenses.Find(x => x.UserPlanetId == userPlanetId && x.DefenseId == defense);

            if (defenseData == null) return 0;
            return defenseData.DefenseCount;
        }
        private double GetDefenseProduceTimeInSeconds(int userPlanetId, Defenses defense)
        {
            int robotFacLevel = GlobalBuildingController.Instance.GetPlanetBuildingLevel(userPlanetId, Buildings.RobotFabrikası);

            double defenseBuildTime = DataController.Instance.CalculateDefenseCountdown(defense, robotFacLevel);

            List<double> rates = new List<double>();

            if (!LoginController.Instance.CurrentUser.UserData.IsStandartPreOver)
                rates.Add(DataController.Instance.GetPremiumValue(PremiumValues.GemiVeSavunmaDahaHızlı).PremiumValue);

            rates.AddRange(RobotController.Instance.GetRobotLevelValue(Robots.SavunmaMühendisi));

            return 1 + defenseBuildTime * rates.ConvertToReduceRate();
        }
        private double GetTotalProductionTimeOfTheQueue(int currentPlanetId)
        {
            UserPlanetDefenseProgDTO prog = LoginController.Instance.CurrentUser.UserPlanetDefenseProgs.Find(x => x.UserPlanetId == currentPlanetId);
            if (prog == null)
                return 0;

            double coundownOneItem = GetDefenseProduceTimeInSeconds(currentPlanetId, prog.DefenseId);

            DateTime completeTime = prog.LastVerifyDate.AddSeconds(-prog.PassedSeconds).AddSeconds(coundownOneItem);

            double leftSeconds = (completeTime - DateTime.UtcNow).TotalSeconds;

            double totalCountdown = LoginController.Instance.CurrentUser.UserPlanetDefenseProgs.Where(x => x.UserPlanetId == currentPlanetId)
                .Select(x =>
                {
                    if (prog == x)
                    {
                        int defenseCount = x.DefenseCount;

                        double cdOfOtherDefense = DefenseBusSystem.GetDefenseUnitProduceTime(currentPlanetId, x.DefenseId);

                        cdOfOtherDefense *= Math.Ceiling(defenseCount / (float)(GlobalBuildingController.Instance.GetCurrentPlanetBuildingLevel(Buildings.YönetimMerkezi) + 1));

                        return cdOfOtherDefense - (coundownOneItem - leftSeconds);
                    }
                    else
                    {

                        int defenseCount = x.DefenseCount;

                        double cdOfOtherDefense = DefenseBusSystem.GetDefenseUnitProduceTime(currentPlanetId, x.DefenseId);

                        cdOfOtherDefense *= Math.Ceiling(defenseCount / (float)(GlobalBuildingController.Instance.GetCurrentPlanetBuildingLevel(Buildings.YönetimMerkezi) + 1));

                        return cdOfOtherDefense;
                    }
                })
                .DefaultIfEmpty(0)
                .Sum();

            return totalCountdown;
        }
    }
}