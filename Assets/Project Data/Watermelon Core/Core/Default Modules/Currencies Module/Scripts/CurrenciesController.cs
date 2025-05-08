using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.LevelSystem;
using YG;

namespace Watermelon
{
    public class CurrenciesController : MonoBehaviour
    {
        private static CurrenciesController currenciesController;

        [SerializeField] CurrenciesDatabase currenciesDatabase;
        public CurrenciesDatabase CurrenciesDatabase => currenciesDatabase;

        private static Currency[] currencies;
        public static Currency[] Currencies => currencies;

        private static Dictionary<CurrencyType, int> currenciesLink;

        [Header("Reward Ads Settings")]
        [SerializeField] int coinsRewardAmount = 75;
        [SerializeField] CurrencyType rewardCurrencyType = CurrencyType.Coin;

        private void OnEnable()
        {
            YandexGame.RewardVideoEvent += OnRewardedAdCompleted;
        }

        private void OnDisable()
        {
            YandexGame.RewardVideoEvent -= OnRewardedAdCompleted;
        }

        public virtual void Initialise()
        {
            currenciesController = this;

            currenciesDatabase.Initialise();

            currencies = currenciesDatabase.Currencies;

            currenciesLink = new Dictionary<CurrencyType, int>();
            for (int i = 0; i < currencies.Length; i++)
            {
                if (!currenciesLink.ContainsKey(currencies[i].CurrencyType))
                {
                    currenciesLink.Add(currencies[i].CurrencyType, i);
                }
                else
                {
                    Debug.LogError($"[Currency System]: Currency with type {currencies[i].CurrencyType} added to database twice!");
                }

                var save = SaveController.GetSaveObject<Currency.Save>("currency" + ":" + (int)currencies[i].CurrencyType);
                currencies[i].SetSave(save);
            }
        }

        public static void ShowRewardAd()
        {
            if (YandexGame.Instance != null)
                YandexGame.RewVideoShow(1); // rewardId = 1 для монет
            else
                Debug.LogError("YandexGame instance is missing!");
        }

        public static void ShowSkipLevelAd()
        {
            if (YandexGame.Instance != null)
                YandexGame.RewVideoShow(2); // rewardId = 2 для пропуска уровня
            else
                Debug.LogError("YandexGame instance is missing!");
        }

        private void OnRewardedAdCompleted(int rewardId)
        {
            switch (rewardId)
            {
                case 1:
                    Add(rewardCurrencyType, coinsRewardAmount);
                    break;
                case 2:
                    LevelController.NextLevelDev();
                    break;
            }
        }

        public static bool HasAmount(CurrencyType currencyType, int amount)
        {
            return currencies[currenciesLink[currencyType]].Amount >= amount;
        }

        public static int Get(CurrencyType currencyType)
        {
            return currencies[currenciesLink[currencyType]].Amount;
        }

        public static Currency GetCurrency(CurrencyType currencyType)
        {
            return currencies[currenciesLink[currencyType]];
        }

        public static void Set(CurrencyType currencyType, int amount)
        {
            Currency currency = currencies[currenciesLink[currencyType]];
            currency.Amount = amount;
            SaveController.MarkAsSaveIsRequired();
            currency.InvokeChangeEvent(0);
        }

        public static void Add(CurrencyType currencyType, int amount)
        {
            Currency currency = currencies[currenciesLink[currencyType]];
            currency.Amount += amount;
            SaveController.MarkAsSaveIsRequired();
            currency.InvokeChangeEvent(amount);
        }

        public static void Substract(CurrencyType currencyType, int amount)
        {
            Currency currency = currencies[currenciesLink[currencyType]];
            currency.Amount -= amount;
            SaveController.MarkAsSaveIsRequired();
            currency.InvokeChangeEvent(-amount);
        }

        public static void SubscribeGlobalCallback(CurrencyChangeDelegate currencyChange)
        {
            for (int i = 0; i < currencies.Length; i++)
            {
                currencies[i].OnCurrencyChanged += currencyChange;
            }
        }

        public static void UnsubscribeGlobalCallback(CurrencyChangeDelegate currencyChange)
        {
            for (int i = 0; i < currencies.Length; i++)
            {
                currencies[i].OnCurrencyChanged -= currencyChange;
            }
        }
    }

    public delegate void CurrencyChangeDelegate(Currency currency, int difference);
}
