using Assets.Scripts.ApiModels;
using Assets.Scripts.Enums;
using System;
using UnityEngine;

public static class DefenseBusSystem
{
    // A SINGLE DEFENSE UNIT PRODUCED.
    public class DefenseProducedEventArgs : EventArgs
    {
        /// <summary>
        /// This is the progress informations.
        /// </summary>
        public UserPlanetDefenseProgDTO Progress;

        /// <summary>
        /// Total produced count.
        /// </summary>
        public int ProducedCount;

        /// <summary>
        /// When all the production for the given defense is completed.
        /// </summary>
        public bool IsAllDone;

        public DefenseProducedEventArgs(UserPlanetDefenseProgDTO progress, int producedCount, bool isAllDone)
        {
            this.Progress = progress;
            this.ProducedCount = producedCount;
            IsAllDone = isAllDone;
        }

        public string GetDefenseName()
        {
            return DefenseBusSystem.GetDefenseUnitName(this.Progress.DefenseId);
        }
    }

    public static event EventHandler<DefenseProducedEventArgs> OnDefenseProducing;
    public static void CallDefenseProducing(DefenseProducedEventArgs args) => OnDefenseProducing?.Invoke(null, args);

    public static event EventHandler<DefenseProducedEventArgs> OnDefenseProduced;
    public static void CallDefenseProduced(DefenseProducedEventArgs args) => OnDefenseProduced?.Invoke(null, args);


    // ALL THE DEFENSE UNIT FOR A PLANET IS COMPLETED.
    public class AllDefenseProducedEventArgs : EventArgs
    {
        public int UserPlanetID;
        public AllDefenseProducedEventArgs(int userPlanetID)
        {
            UserPlanetID = userPlanetID;
        }
    }

    public static event EventHandler<AllDefenseProducedEventArgs> OnAllDefensesProduced;
    public static void CallAllDefensesProduced(AllDefenseProducedEventArgs args) => OnAllDefensesProduced?.Invoke(null, args);


    // WHEN THE DEFENSE UNIT COUNT CHANGING IN A PLANET.
    public class DefenseUnitCountChangingEventArgs : EventArgs
    {
        public int UserPlanetID;
        public Defenses Defense;
        public int ChangeCount;
        public DefenseUnitCountChangingEventArgs(int userPlanetID, Defenses defense, int changeCount)
        {
            UserPlanetID = userPlanetID;
            Defense = defense;
            ChangeCount = changeCount;
        }
    }

    public static event EventHandler<DefenseUnitCountChangingEventArgs> OnDefenseUnitCountChanging;
    public static void CallAddDefenseUnit(DefenseUnitCountChangingEventArgs args)
    {
        OnDefenseUnitCountChanging?.Invoke(null, args);
    }
    public static void CallRemoveDefenseUnit(DefenseUnitCountChangingEventArgs args)
    {
        OnDefenseUnitCountChanging?.Invoke(null, args);
    }


    // WHEN THE DEFENSE UNIT COUNT CHANGED IN A PLANET.
    public class DefenseUnitCountChangedEventArgs : EventArgs
    {
        public int UserPlanetID;
        public Defenses Defense;
        public int OldCount;
        public int NewCount;

        public int ChangeCount => NewCount - OldCount;

        public DefenseUnitCountChangedEventArgs(int userPlanetID, Defenses defense, int newCount, int oldCount)
        {
            this.UserPlanetID = userPlanetID;
            this.Defense = defense;
            this.NewCount = newCount;
            this.OldCount = oldCount;
        }
    }

    public static event EventHandler<DefenseUnitCountChangedEventArgs> OnDefenseUnitCountChanged;
    public static void CallDefenseUnitCountChanged(DefenseUnitCountChangedEventArgs args)
    {
        OnDefenseUnitCountChanged?.Invoke(null, args);
    }


    // RETURNS THE DEFENSE NAME.
    public delegate string DefenseNameData(Defenses defense);

    public static DefenseNameData OnGetDefenseUnitName;
    public static string GetDefenseUnitName(Defenses defense) => OnGetDefenseUnitName?.Invoke(defense);


    // RETURNS THE DEFENSE DESCRIPTION.
    public delegate string DefenseDescriptionData(Defenses defense);
    public static DefenseDescriptionData OnGetDefenseUnitDescription;
    public static string GetDefenseUnitDescription(Defenses defense) => OnGetDefenseUnitDescription?.Invoke(defense);


    // RETURNS THE DEFENSE IMAGE.
    public delegate Sprite DefenseImageData(Defenses defense);
    public static DefenseImageData OnGetDefenseUnitImage;
    public static Sprite GetDefenseUnitImage(Defenses defense) => OnGetDefenseUnitImage?.Invoke(defense);


    // RETURNS THE DEFENSE PODIUM.
    public delegate GameObject DefenseGameObjectData(Defenses defense);
    public static DefenseGameObjectData OnGetDefenseUnitPodium;
    public static GameObject GetDefenseUnitPodium(Defenses defense) => OnGetDefenseUnitPodium?.Invoke(defense);


    // RETURNS THE DEFENSE UNIT COUNT IN GIVEN PLANET.
    public delegate int DefenseCountData(int userPlanetId, Defenses defense);
    public static DefenseCountData OnGetDefenseUnitCountData;
    public static int GetDefenseUnitCount(int userPlanetId, Defenses defense)
    {
        return OnGetDefenseUnitCountData?.Invoke(userPlanetId, defense) ?? 0;
    }


    // RETURN THE PRODUCING TIME FOR A DEFENSE UNIT.
    public delegate double DefenseProduceTime(int userPlanetId, Defenses defense);
    public static DefenseProduceTime OnGetDefenseUnitProduceTime;
    public static double GetDefenseUnitProduceTime(int userPlanetId, Defenses defense)
    {
        return OnGetDefenseUnitProduceTime?.Invoke(userPlanetId, defense) ?? 0;
    }


    // RETURNS THE TOTAL PRODUCTION TIME IN THE QUEUE OF THE CURRENT PLANET.
    public delegate double DefenseProductionTotalQueueTime(int userPlanetId);
    public static DefenseProductionTotalQueueTime OnGetDefenseProductionTotalQueueTime;
    public static double GetDefenseProductionTotalQueueTime(int userPlanetId)
    {
        return OnGetDefenseProductionTotalQueueTime?.Invoke(userPlanetId) ?? 0;
    }

    // SHOWS THE DEFENSE PANEL.
    public static Action OnShowDefensePanel;
    public static void CallShowDefensePanel()
    {
        OnShowDefensePanel?.Invoke();
    }

    // DEFENSE PRODUCTION
    public class OnDefenseProductionStartData : EventArgs
    {
        public UserPlanetDefenseProgDTO ProductionData;
        public int UserPlanetId;
        public ResourcesDTO Cost;
        public double TotalProductionTime;
    }

    // WHEN DEFENSE PRODUCTION STARTING.
    public static EventHandler<OnDefenseProductionStartData> OnDefenseProductionStarting;
    public static void CallDefenseProductionStarting(object sender, OnDefenseProductionStartData productionData)
    {
        OnDefenseProductionStarting?.Invoke(sender, productionData);
    }

    // WHEN DEFENSE PRODUCTION STARTED.
    public static EventHandler<OnDefenseProductionStartData> OnDefenseProductionStarted;
    public static void CallDefenseProductionStarted(object sender, OnDefenseProductionStartData productionData)
    {
        OnDefenseProductionStarted?.Invoke(sender, productionData);
    }
}
