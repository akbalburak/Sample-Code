using Assets.Scripts.ApiModels;
using Assets.Scripts.Enums;
using Assets.Scripts.Extends;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Controllers.Defense
{
    public class DefensePanelQueueView : BaseLanguageBehaviour
    {
        [SerializeField] private GameObject _defenseQueueContainerView;

        [SerializeField] private DefensePanelQueueViewItem _defenseQueueItem;
        [SerializeField] private ScrollRect _defenseQueueItemContainer;

        [SerializeField] private TMP_Text _countdownText;
        [SerializeField] private TMP_Text _totalCountdownText;

        private int _userPlanetId;
        private int _headquaterLevel;

        private void OnEnable()
        {
            DefenseBusSystem.OnDefenseProduced += OnDefenseProduced;
            DefenseBusSystem.OnDefenseProductionStarted += OnDefenseProductionStarted;

            LoadDefenseQueue();
        }

        private void OnDisable()
        {
            DefenseBusSystem.OnDefenseProduced -= OnDefenseProduced;
            DefenseBusSystem.OnDefenseProductionStarted -= OnDefenseProductionStarted;
        }

        private void OnDefenseProductionStarted(object sender, DefenseBusSystem.OnDefenseProductionStartData e)
        {
            if (e.UserPlanetId != _userPlanetId)
                return;

            LoadDefenseQueue();
        }
        private void OnDefenseProduced(object sender, DefenseBusSystem.DefenseProducedEventArgs e)
        {
            if (e.Progress.UserPlanetId != _userPlanetId)
                return;

            LoadDefenseQueue();
        }

        private void LoadDefenseQueue()
        {
            _headquaterLevel = GlobalBuildingController.Instance.GetCurrentPlanetBuildingLevel(Buildings.YönetimMerkezi) + 1;
            _userPlanetId = GlobalPlanetController.Instance.CurrentPlanet.UserPlanetId;

            foreach (Transform child in _defenseQueueItemContainer.content)
                Destroy(child.gameObject);

            var planetProgs = LoginController.Instance.CurrentUser.UserPlanetDefenseProgs
                .Where(x => x.UserPlanetId == _userPlanetId)
                .ToList();

            foreach (UserPlanetDefenseProgDTO progress in planetProgs)
            {
                DefensePanelQueueViewItem queueItem = Instantiate(_defenseQueueItem, _defenseQueueItemContainer.content);
                queueItem.LoadProgress(progress);
            }

            _defenseQueueContainerView.SetActive(planetProgs.Count > 0);

            CancelInvoke(nameof(RefreshCountdown));
            InvokeRepeating(nameof(RefreshCountdown), 0, 1);
        }

        private void RefreshCountdown()
        {
            UserPlanetDefenseProgDTO prog = LoginController.Instance.CurrentUser.UserPlanetDefenseProgs.Find(x => x.UserPlanetId == _userPlanetId);

            if (prog == null)
            {
                _countdownText.text = $"-";
                _totalCountdownText.text = $"-";
                return;
            }

            double coundownOneItem = DefenseBusSystem.GetDefenseUnitProduceTime(_userPlanetId, prog.DefenseId);

            DateTime completeTime = prog.LastVerifyDate.AddSeconds(-prog.PassedSeconds).AddSeconds(coundownOneItem);

            double leftSeconds = (completeTime - DateTime.UtcNow).TotalSeconds;
            double totalCountdown = DefenseBusSystem.GetDefenseProductionTotalQueueTime(_userPlanetId);

            _countdownText.text = $"{TimeExtends.GetCountdownText(leftSeconds)}";
            _totalCountdownText.text = $"{TimeExtends.GetCountdownText(totalCountdown)}";
        }
    }
}