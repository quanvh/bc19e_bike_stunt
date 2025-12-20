using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bacon
{
    [Serializable]
    public class PlayerModel : PlayerModelBase
    {
        private readonly int maxDaily = 7;

        public PlayerModel()
        {
            ResetData();
            ResetCustomData();
        }

        public bool ShowDaily => !dailyReward && !dailyRewardShow && dayLogin < maxDaily;


        [Header("CUSTOM DATA")]
        public int magnet;
        public int nitro;
        public int multiCoin;
        public int trialCar;
        public int version;
        public int CoinFlip;
        public int freeCoin;


        private void ResetCustomData()
        {
            magnet = 0;
            nitro = 0;
            multiCoin = 0;
            trialCar = 0;
            freeCoin = 0;
        }
    }



    public enum PRICE_TYPE
    {
        FREE, GOLD, GEM, ADS, LEVEL, CARD
    }
}