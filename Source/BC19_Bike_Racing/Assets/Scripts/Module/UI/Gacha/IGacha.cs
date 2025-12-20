using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bacon
{
    public abstract class IGacha : MonoBehaviour
    {
        public Action OnSpinStart;
        public Action<RewardModel> OnSpinComplete;


        [SerializeField] private RewardData gachaData;
        [SerializeField] private RewardData gachaCustomData;
        public bool InitAtStart;

        public bool RandomReward;

        protected RewardData rewards;
        protected RewardData rewardsGachaCustom;
        protected List<RewardModel> lstReward = new List<RewardModel>();

        private void Awake()
        {
            if (gachaData) rewards = Instantiate(gachaData);
            if (gachaCustomData) rewardsGachaCustom = Instantiate(gachaCustomData);
            lstReward = new List<RewardModel>();
        }

        private void Start()
        {
            if (InitAtStart)
            {
               
                InitGacha();
            }
        }

        public abstract void InitGacha();

        public abstract void StartSpin();

        public abstract int FixedReward();
    }
}