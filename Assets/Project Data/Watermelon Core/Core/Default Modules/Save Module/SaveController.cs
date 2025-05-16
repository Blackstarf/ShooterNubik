using System;
using System.Collections;
using UnityEngine;

namespace Watermelon
{
    public static class SaveController
    {
        private const string SAVE_KEY = "SAVE_DATA";
        private const int SAVE_DELAY = 30;

        private static GlobalSave globalSave;

        private static bool isSaveLoaded;
        public static bool IsSaveLoaded => isSaveLoaded;

        private static bool isSaveRequired;

        public static int LevelId { get => globalSave.LevelId; set => globalSave.LevelId = value; }
        public static float GameTime => globalSave.GameTime;
        public static DateTime LastExitTime => globalSave.LastExitTime;

        public static event SimpleCallback OnSaveLoaded;

        public static void Initialise(bool useAutoSave, bool clearSave = false, float overrideTime = -1f)
        {
            if (clearSave)
            {
                InitClear(overrideTime != -1f ? overrideTime : Time.time);
            }
            else
            {
                Load(overrideTime != -1f ? overrideTime : Time.time);
            }

            if (useAutoSave)
            {
                Tween.InvokeCoroutine(AutoSaveCoroutine());
            }
        }

        private static void InitClear(float time)
        {
            globalSave = new GlobalSave();
            globalSave.Init(time);
            isSaveLoaded = true;

            Debug.Log("[SaveController]: Clear save created.");
        }

        private static void Load(float time)
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                globalSave = JsonUtility.FromJson<GlobalSave>(json);
            }
            else
            {
                globalSave = new GlobalSave();
            }

            globalSave.Init(time);
            isSaveLoaded = true;

            Debug.Log("[SaveController]: Save loaded.");
            OnSaveLoaded?.Invoke();
        }

        public static void Save()
        {
            if (!isSaveRequired) return;

            globalSave.Flush();

            string json = JsonUtility.ToJson(globalSave);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();

            Debug.Log("[SaveController]: Game saved.");
            isSaveRequired = false;
        }

        public static void ForceSave()
        {
            globalSave.Flush();

            string json = JsonUtility.ToJson(globalSave);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();

            Debug.Log("[SaveController]: Game force-saved.");
            isSaveRequired = false;
        }

        private static IEnumerator AutoSaveCoroutine()
        {
            WaitForSeconds wait = new WaitForSeconds(SAVE_DELAY);

            while (true)
            {
                yield return wait;
                Save();
            }
        }

        public static void MarkAsSaveIsRequired()
        {
            isSaveRequired = true;
        }

        public static void DeleteSaveFile()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
        }

        public static GlobalSave GetGlobalSave()
        {
            return globalSave;
        }
        public static T GetSaveObject<T>(int hash) where T : ISaveObject, new()
        {
            string key = $"SAVE_OBJ_{hash}";

            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                return JsonUtility.FromJson<T>(json);
            }
            else
            {
                // Если объект не найден — создать новый
                T saveObject = new T();
                string json = JsonUtility.ToJson(saveObject);
                PlayerPrefs.SetString(key, json);
                PlayerPrefs.Save();
                return saveObject;
            }
        }

        public static T GetSaveObject<T>(string uniqueName) where T : ISaveObject, new()
        {
            return GetSaveObject<T>(uniqueName.GetHashCode());
        }
        public static void SaveCustom(GlobalSave customGlobalSave)
        {
            if (customGlobalSave != null)
            {
                customGlobalSave.Flush();
                Debug.Log("[Save Controller]: Custom game save flushed to PlayerPrefs!");
            }
        }
        public static void PresetsSave(string presetKey)
        {
            if (globalSave == null)
            {
                Debug.LogWarning("[Save Controller]: Can't save preset, globalSave is null!");
                return;
            }

            globalSave.Flush(); // Обновляем PlayerPrefs по текущему глобальному состоянию

            // Сохраняем как JSON в отдельный ключ (например, "preset_save_profile1")
            string json = JsonUtility.ToJson(globalSave);
            PlayerPrefs.SetString(presetKey, json);
            PlayerPrefs.Save();

            Debug.Log($"[Save Controller]: Preset saved under key '{presetKey}'");
        }

    }
}
