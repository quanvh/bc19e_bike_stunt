using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Bacon
{
    [Serializable]
    public class PlayerModelBase
    {
        [Header("PLAYER INFO")]
        public string name;
        public bool organic;
        public int age;
        public string country;

        [Header("GENERAL SETTING")]
        public float sound;
        public float music;
        public int quality;
        public bool vibrate;

        [Header("INGAME CONTROL"), Space]
        public DriveDirect driveDirect;
        public DriveMobile driveType;

        [Header("CURRENT GAME DATA"), Space]
        public int currentGold;
        public int currentGem;
        public int currentCar;
        public int currentChar;
        public int currentHelmet;
        public int currentLevel;
        public int currentMode;
        public int currentWheel;
        public int currentDecal;
        public int currentColor;
        public int currentVolant;

        [Header("DAILY REWARD"), Space]
        public int dayLogin;
        public bool dailyReward;
        public bool dailyRewardShow;
        public int dailyGacha;
        public int dailySpin;
        public DateTime lastLogin;
        public DateTime lastClaim;

        [Header("CURRENT PLAYER DATA"), Space]
        public bool chooseAge;
        public bool chooseLang;
        public bool showRate;
        public bool vip;
        public bool subscriber;
        public bool removeAds;
        public int completeCount;
        public int spinCount;
        public int gachaCount;
        public int failCount;


        [FoldoutGroup("DATA ASSET", expanded: false), Tooltip("Show for debug")] public List<LevelModelSave> levelData;
        [FoldoutGroup("DATA ASSET"), Tooltip("Show for debug"), Space] public List<CarModelSave> listCar;
        [FoldoutGroup("DATA ASSET"), Tooltip("Show for debug"), Space] public List<CustomModelSave> listChar;
        [FoldoutGroup("DATA ASSET"), Tooltip("Show for debug"), Space] public List<CustomModelSave> listColor;
        [FoldoutGroup("DATA ASSET"), Tooltip("Show for debug"), Space] public List<CustomModelSave> listVolant;
        [FoldoutGroup("DATA ASSET"), Tooltip("Show for debug"), Space] public List<CustomModelSave> listDecal;
        [FoldoutGroup("DATA ASSET"), Tooltip("Show for debug"), Space] public List<CustomModelSave> listWheel;
        [FoldoutGroup("DATA ASSET"), Tooltip("Show for debug"), Space] public List<CustomModelSave> listHelmet;

        [Header("REMOTE CONFIG")]
        public RemoteConfig remoteConfig;


        public bool VipUser => vip || subscriber || removeAds;

        protected void ResetData()
        {
            System.Random rnd = new System.Random();
            name = string.Format("BC{0:00000}", rnd.Next(0, 99999));
            organic = false;
            country = "";
            chooseAge = false;
            chooseLang = false;

            //level + car data
            levelData = new List<LevelModelSave>();
            listCar = new List<CarModelSave>();
            listWheel = new List<CustomModelSave>();
            listDecal = new List<CustomModelSave>();
            listVolant = new List<CustomModelSave>();
            listColor = new List<CustomModelSave>();
            listChar = new List<CustomModelSave>();
            listHelmet = new List<CustomModelSave>();

            currentCar = 1;
            currentChar = 0;
            currentHelmet = 0;
            currentLevel = 1;
            currentMode = 1;
            currentWheel = 1;
            currentDecal = 0;
            currentColor = 0;
            currentVolant = 1;


            // ----- Setting System -----
            sound = 1f;
            music = 1f;
            quality = 3;
            vibrate = true;
            age = 0;

            // ----- Setting Steering -----
            driveType = DriveMobile.Steer;
            driveDirect = DriveDirect.Left;

            vip = false;
            subscriber = false;
            removeAds = false;
            showRate = false;

            currentGold = 0;
            currentGem = 0;
            dayLogin = 1;

            lastLogin = DateTime.UtcNow;

            completeCount = 0;
            failCount = 0;

            remoteConfig = new RemoteConfig();

            ResetDaily();
        }

        public void ResetDaily()
        {
            dailyReward = false;
            dailyRewardShow = false;
            dailyGacha = 1;
            dailySpin = 1;
        }
    }

    public enum DriveDirect { Left, Right }
    public enum DriveMobile { Steer, Button, Tilt }
}