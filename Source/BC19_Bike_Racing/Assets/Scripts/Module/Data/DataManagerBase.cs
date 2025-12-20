using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Bacon
{
    public class DataManagerBase : MonoBehaviour
    {
        protected static string SAVEFILE = "game";

        [Header("DATA")]
        [SerializeField] protected CarData carData;
        [SerializeField] protected bool AutoUnlockMode = true;
        [SerializeField] protected LevelData levelData;
        [SerializeField] protected CustomData wheelData;
        [SerializeField] protected CustomData decalData;
        [SerializeField] protected CustomData volantData;
        [SerializeField] protected CustomData colorData;
        [SerializeField] protected CustomData charData;
        [SerializeField] protected CustomData helmetData;


        [HideInInspector] public bool dataLoaded;

        protected CarData cars = null;

        protected LevelData levels = null;

        protected CustomData wheels = null;

        protected CustomData decals = null;

        protected CustomData volants = null;

        protected CustomData colors = null;

        protected CustomData chars = null;

        protected CustomData helmets = null;

        protected List<CustomData> customs = null;

        [Header("DEBUG"), Space]
        public bool DebugMode = false;
        public bool AF_Conversion = false;

        public RemoteConfig remoteConfig;

        public PlayerModel _player;

        [HideInInspector] public bool sessionVip = false;

        private bool isDiskFull = false;

        public Action DiskFullAction;

        public int CurrentMode
        {
            get
            {
                return _player.currentMode;
            }
            set
            {
                _player.currentMode = value;
                var _level = levels.AllLevels.Where(t => (int)t.Mode == value && t.Unlock == true).
                    OrderByDescending(l => l.ID).FirstOrDefault();
                _player.currentLevel = _level != null && _level.ID > 0 ? _level.ID : 1;
            }
        }

        public LevelModel CurrentLevel
        {
            get
            {
                if (_player.currentLevel <= 0) return levels.AllLevels.FirstOrDefault();
                return levels.AllLevels.Where(t => t.ID == _player.currentLevel && (int)t.Mode == _player.currentMode)
                    .FirstOrDefault();
            }
        }

        public int TotalLevel
        {
            get
            {
                return levels.AllLevels.Count;
            }
        }

        public int TotalLevelUnlock
        {
            get
            {
                return levels.AllLevels.Where(t => t.Unlock == true).Count();
            }
        }

        public int TotalMode
        {
            get
            {
                return levels.Modes.Count;
            }
        }

        public CarModel CurrentCar
        {
            get
            {
                if (_player.currentCar <= 0) return cars.listCars.FirstOrDefault();
                return cars.listCars.Where(t => t.ID == _player.currentCar).FirstOrDefault();
            }
        }

        public int TotalCar
        {
            get
            {
                return cars.listCars.Count;
            }
        }

        public int TotalCarUnlock
        {
            get
            {
                return cars.listCars.Where(t => t.Unlock == true).Count();
            }
        }

        public CustomModel CurrentWheel
        {
            get
            {
                if (_player.currentWheel <= 0) return wheels.listItems.FirstOrDefault();
                return wheels.listItems.Where(t => t.ID == _player.currentWheel).FirstOrDefault();
            }
        }

        public CustomModel CurrentChar
        {
            get
            {
                if (_player.currentChar <= 0) return chars.listItems.FirstOrDefault();
                return chars.listItems.Where(t => t.ID == _player.currentChar).FirstOrDefault();
            }
        }

        public CustomModel CurrentHelmet
        {
            get
            {
                if (_player.currentHelmet <= 0) return helmets.listItems.FirstOrDefault();
                return helmets.listItems.Where(t => t.ID == _player.currentHelmet).FirstOrDefault();
            }
        }

        public int TotalColor
        {
            get
            {
                return colors.listItems.Count;
            }
        }

        public int TotalVolant
        {
            get
            {
                return volants.listItems.Count;
            }
        }

        public int TotalChar
        {
            get
            {
                return chars.listItems.Count;
            }
        }

        public int TotalHelmet
        {
            get
            {
                return helmets.listItems.Count;
            }
        }

        public int TotalWheel
        {
            get
            {
                return wheels.listItems.Count;
            }
        }

        public bool IsInterStart
        {
            get
            {
                return Remote.InterStartLevel && TotalLevelUnlock >= Remote.InterStartFromLevel;
            }
        }

        public bool VipUser => _player.VipUser || sessionVip;

        public RemoteConfig Remote => _player.remoteConfig;

        public List<CarModel> GetBikeList
        {
            get
            {
                return cars.listCars;
            }
        }
        public List<CustomModel> GetCharList
        {
            get
            {
                return chars.listItems;
            }
        }

        public List<CustomModel> GetHelmetList
        {
            get
            {
                return helmets.listItems;
            }
        }

        public List<CustomModel> GetWheelList
        {
            get
            {
                return wheels.listItems;
            }
        }

        public bool UserOrganic => _player.remoteConfig.RemoteAds && _player.organic;

        protected virtual void Awake()
        {
            SAVEFILE = Application.identifier;
            customs = new List<CustomData>();
            percent = UnityEngine.Random.Range(1, 100);
        }

        public CustomModel GetCustom(CUSTOM_TYPE type, int _id)
        {
            foreach (var item in customs)
            {
                if (item != null && item.type == type)
                    return item.listItems.Where(t => t.ID == _id).FirstOrDefault();
            }
            return null;
        }



        public IEnumerator DoLoadData()
        {
            if (carData) cars = Instantiate(carData);
            if (levelData) levels = Instantiate(levelData);
            if (wheelData)
            {
                wheels = Instantiate(wheelData);
                customs.Add(wheels);
            }
            if (decalData)
            {
                decals = Instantiate(decalData);
                customs.Add(decals);
            }
            if (volantData)
            {
                volants = Instantiate(volantData);
                customs.Add(volants);
            }
            if (colorData)
            {
                colors = Instantiate(colorData);
                customs.Add(colors);
            }
            if (charData)
            {
                chars = Instantiate(charData);
                customs.Add(chars);
            }
            if (helmetData)
            {
                helmets = Instantiate(helmetData);
                customs.Add(helmets);
            }

            if (Load<PlayerModel>() is PlayerModel saveData)
            {
                Debug.Log("Load data success");
                _player = saveData;
            }
            else
            {
                _player = new PlayerModel();
                _player.currentCar = carData.FirstUnlock;
                _player.currentMode = levelData.FirstMode;
                _player.currentLevel = levelData.FirstUnlock;
                if (wheelData) _player.currentWheel = wheelData.FirstUnlock;
                if (decalData) _player.currentDecal = decalData.FirstUnlock;
                if (volantData) _player.currentVolant = volantData.FirstUnlock;
                if (colorData) _player.currentColor = colorData.FirstUnlock;
                if (charData) _player.currentChar = charData.FirstUnlock;
                if (helmetData) _player.currentHelmet = helmetData.FirstUnlock;
                Save();
            }

            if (cars) cars.UpdateFromSaveData(_player.listCar);
            if (levels) levels.UpdateFromSaveData(_player.levelData);
            if (chars) chars.UpdateFromSaveData(_player.listChar);
            if (colors) colors.UpdateFromSaveData(_player.listColor);
            if (wheels) wheels.UpdateFromSaveData(_player.listWheel);
            if (decals) decals.UpdateFromSaveData(_player.listDecal);
            if (volants) volants.UpdateFromSaveData(_player.listVolant);
            if (helmets) helmets.UpdateFromSaveData(_player.listHelmet);

            CheckDailyLogin();

            yield return new WaitForEndOfFrame();
            dataLoaded = true;
        }


        protected object Load<T>()
        {
            return FileDataHelper.Load<T>(SAVEFILE);
        }


        public void Save()
        {
            if (cars != null)
            {
                _player.listCar = cars.SaveDataList;
            }
            if (levels != null)
            {
                _player.levelData = levels.SaveDataList;
            }
            if (chars != null)
            {
                _player.listChar = chars.SaveDataList;
            }
            if (colors != null)
            {
                _player.listColor = colors.SaveDataList;
            }
            if (wheels != null)
            {
                _player.listWheel = wheels.SaveDataList;
            }
            if (decals != null)
            {
                _player.listDecal = decals.SaveDataList;
            }
            if (volants != null)
            {
                _player.listVolant = volants.SaveDataList;
            }
            if (helmets != null)
            {
                _player.listHelmet = helmets.SaveDataList;
            }
            Save<PlayerModel>(_player, (ex) =>
            {
                isDiskFull = IsDiskFull(ex);
                Debug.Log("SAVE GAME ERROR: " + ex.Message);
                if (isDiskFull)
                {
                    DiskFullAction?.Invoke();
                }
            });
        }

        protected void Save<T>(object saveData, Action<Exception> error = null)
        {
            FileDataHelper.Save<T>(SAVEFILE, saveData, delegate (Exception ex)
            {
                error?.Invoke(ex);
            });
        }

        protected bool IsDiskFull(Exception ex)
        {
            const int HR_ERROR_HANDLE_DISK_FULL = unchecked((int)0x80070027);
            const int HR_ERROR_DISK_FULL = unchecked((int)0x80070070);

            return ex.HResult == HR_ERROR_HANDLE_DISK_FULL
                || ex.HResult == HR_ERROR_DISK_FULL;
        }


        private readonly bool resetDaily = false;
        public void CheckDailyLogin()
        {
            if ((DateTime.UtcNow - _player.lastClaim).Days >= 1)
            {
                if (resetDaily)
                    _player.dayLogin = 1;
                else if (_player.dailyReward) _player.dayLogin++;
                _player.ResetDaily();
            }
            else
            {
                if (DateTime.UtcNow.Date == _player.lastClaim.AddDays(1).Date)
                {
                    if (_player.dailyReward)
                        _player.dayLogin++;
                    _player.ResetDaily();
                }
            }
            if ((DateTime.UtcNow.Date - _player.lastLogin.Date).Days >= 1)
            {
                _player.dailyRewardShow = false;
            }
            _player.lastLogin = DateTime.UtcNow;

            Save();
        }

        public IEnumerator DetectCountry()
        {
            UnityWebRequest request = UnityWebRequest.Get("https://api.country.is/");
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Get country error: " + request.error);
            }
            else
            {
                Country res = JsonUtility.FromJson<Country>(request.downloadHandler.text);
                DataManager.Instance._player.country = res.country;
            }
        }

        public void UserConversion(Dictionary<string, object> _conversionData)
        {
            if (AF_Conversion && _conversionData.ContainsKey("af_status"))
            {
                if (_conversionData["af_status"].ToString().ToLower().Equals("organic"))
                {
                    _player.organic = true;
                    FirebaseLogger.RegularEvent("User_Organic");
                }
                else
                {
                    FirebaseLogger.RegularEvent("User_Acquire");
                }
            }
        }

        [Button(ButtonSizes.Large), PropertyOrder(-1), PropertySpace]
        public void ResetData()
        {
            PlayerPrefs.DeleteAll();
            FileDataHelper.Delete(SAVEFILE);
        }


        private int percent = 0;
        public bool LoadAdsOpen()
        {
            return !_player.chooseLang || !_player.chooseAge;
        }
    }
    public class Country
    {
        public string ip;
        public string country;
    }
}