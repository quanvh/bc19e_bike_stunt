using System;
using System.Linq;
using UnityEngine;


namespace Bacon
{
    public class DataManager : DataManagerBase
    {
        public static DataManager Instance;

        [Header("GAME BUILD"), Space]
        public int version = 0;

        [Space] public string appID_ios = "";

        [Space] public string bundle_android = "";


        public bool ShowDaily => _player.ShowDaily;

        [HideInInspector] public bool isLuckySpinCustom;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }


        public CustomModel BaseColor(int carId)
        {
            return colors.listItems.Where(t => t.ID == GetCar(carId).BaseColor).FirstOrDefault();
        }

        public CustomModel GetColor(int colorId)
        {
            return GetCustom(CUSTOM_TYPE.COLOR, colorId);
        }

        public CustomModel GetVolant(int volantId)
        {
            return GetCustom(CUSTOM_TYPE.VOLANT, volantId);
        }

        public CustomModel GetChar(int charId)
        {
            return GetCustom(CUSTOM_TYPE.CHARACTER, charId);
        }

        public CustomModel GetHelmet(int helmetId)
        {
            return GetCustom(CUSTOM_TYPE.HELMET, helmetId);
        }

        public CustomModel GetWheel(int wheelId)
        {
            return GetCustom(CUSTOM_TYPE.WHEEL, wheelId);
        }

        public CustomModel GetWheelItem(int wheelId, int offset = 0)
        {
            int _wheelId = (wheelId + offset) % (TotalWheel + 1);
            if (_wheelId == 0)
            {
                _wheelId = offset < 0 ? TotalWheel : 1;
            }
            return GetCustom(CUSTOM_TYPE.WHEEL, _wheelId);
        }

        public CustomModel GetCharItem(int charId, int offset = 0)
        {
            int _charId = (charId + offset) % (TotalChar + 1);
            if (_charId == 0)
            {
                _charId = offset < 0 ? TotalChar : 1;
            }
            return GetCustom(CUSTOM_TYPE.CHARACTER, _charId);
        }

        public CustomModel GetHelmetItem(int helmetId, int offset = 0)
        {
            int _helmetId = (helmetId + offset) % (TotalHelmet + 1);
            if (_helmetId == 0)
            {
                _helmetId = offset < 0 ? TotalHelmet : 1;
            }
            return GetCustom(CUSTOM_TYPE.HELMET, _helmetId);
        }

        public CustomModel GetRandomItem(CUSTOM_TYPE type)
        {
            foreach (var item in customs)
            {
                if (item != null && item.type == type)
                    return item.listItems.Where(t => !t.Unlock).OrderBy(i => Guid.NewGuid()).First();
            }
            return null;

        }

        public CarModel GetRandomCar()
        {
            return cars.listCars.Where(t => !t.Unlock).OrderBy(i => Guid.NewGuid()).First();
        }


        public CarModel GetCar(int carId, int offset = 0)
        {
            int _carId = (carId + offset) % (TotalCar + 1);
            if (_carId == 0)
            {
                _carId = offset < 0 ? TotalCar : 1;
            }

            return cars.listCars.Where(t => t.ID == _carId).FirstOrDefault();
        }

        public void UnlockCar(int carId)
        {
            foreach (var car in cars.listCars)
            {
                if (car.ID == carId)
                    car.Unlock = true;
            }
        }

        public void UnlockChar(int charId)
        {
            foreach (var charItem in chars.listItems)
            {
                if (charItem.ID == charId)
                    charItem.Unlock = true;
            }
        }

        public void UnlockHelmet(int helmetId)
        {
            foreach (var helmet in helmets.listItems)
            {
                if (helmet.ID == helmetId)
                    helmet.Unlock = true;
            }
        }

        public void UnlockWheel(int wheelId)
        {
            foreach (var wheel in wheels.listItems)
            {
                if (wheel.ID == wheelId)
                    wheel.Unlock = true;
            }
        }

        public void UnlockAllCar()
        {
            foreach (var car in cars.listCars)
            {
                car.Unlock = true;
            }
        }

        public LevelMode GetMode(int modeId)
        {
            return levels.Modes.Where(t => t.ID == modeId).FirstOrDefault();
        }

        public LevelModel GetLevel(int _levelId)
        {
            return levels.GetLevel(_levelId, _player.currentMode);
        }

        public LevelModel GetLevel(int _levelId, LEVEL_MODE _mode)
        {
            return levels.AllLevels.Where(t => t.ID == _levelId && t.Mode == _mode).FirstOrDefault();
        }


        public Action<int> OnUnlockMode;
        [HideInInspector] public bool isUnlockMode = false;
        [HideInInspector] public int modeToUnlock = 0;
        public void SetLevelData()
        {
            isUnlockMode = false;
            CheckModeUnlock();
            var nextLevel = levels.AllLevels.Where(t => t.ID > _player.currentLevel && t.Mode == CurrentLevel.Mode)
                .OrderBy(t => t.ID).FirstOrDefault();
            if (nextLevel != null && nextLevel.ID != 0)
            {
                _player.currentLevel = nextLevel.ID;
                nextLevel.Unlock = true;
            }
            else if (AutoUnlockMode && _player.currentMode + 1 <= TotalMode)
            {
                isUnlockMode = true;

                _player.currentMode++;
                modeToUnlock = _player.currentMode;
                OnUnlockMode?.Invoke(_player.currentMode);

                levels.UnlockMode(_player.currentMode);
                _player.currentLevel = levels.FirstLevel(_player.currentMode).ID;

            }
            _player.completeCount++;
            Save();
        }

        public void CheckModeUnlock()
        {
            foreach (var mode in levels.Modes)
            {
                if (!mode.IsUnlock && mode.UnlockType == PRICE_TYPE.LEVEL && _player.currentLevel == mode.Price)
                {
                    isUnlockMode = true;
                    modeToUnlock = mode.ID;
                    levels.UnlockMode(mode.ID);
                    OnUnlockMode?.Invoke(mode.ID);
                }
            }
        }

        public void UnlockAllLevel()
        {
            foreach (var level in levels.AllLevels)
            {
                level.Unlock = true;
            }
        }
    }
}