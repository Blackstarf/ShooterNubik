using System;
using UnityEngine;

namespace Watermelon
{
    [Serializable]
    public class GlobalSave
    {
        private const string KEY_LEVEL_ID = "global_level_id";
        private const string KEY_GAME_TIME = "global_game_time";
        private const string KEY_LAST_EXIT_TIME = "global_last_exit_time";

        public int LevelId
        {
            get => PlayerPrefs.GetInt(KEY_LEVEL_ID, 0);
            set => PlayerPrefs.SetInt(KEY_LEVEL_ID, value);
        }

        public float GameTime => gameTime + (Time - lastFlushTime);

        private float gameTime;
        public DateTime LastExitTime { get; private set; }

        public float Time { get; set; }
        private float lastFlushTime;

        public void Init(float time)
        {
            Time = time;
            lastFlushTime = Time;

            gameTime = PlayerPrefs.GetFloat(KEY_GAME_TIME, 0f);

            string exitTimeStr = PlayerPrefs.GetString(KEY_LAST_EXIT_TIME, "");
            if (!string.IsNullOrEmpty(exitTimeStr))
            {
                LastExitTime = DateTime.Parse(exitTimeStr);
            }
            else
            {
                LastExitTime = DateTime.Now;
            }
        }

        public void Flush()
        {
            gameTime += Time - lastFlushTime;
            PlayerPrefs.SetFloat(KEY_GAME_TIME, gameTime);

            lastFlushTime = Time;

            LastExitTime = DateTime.Now;
            PlayerPrefs.SetString(KEY_LAST_EXIT_TIME, LastExitTime.ToString());

            PlayerPrefs.Save();
        }

        public T GetSaveObject<T>(int hash) where T : ISaveObject, new()
        {
            string key = $"SAVE_OBJ_{hash}";
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                return JsonUtility.FromJson<T>(json);
            }
            else
            {
                T obj = new T();
                SaveObject(obj, hash); // сразу сохраняем, если нового создаём
                return obj;
            }
        }

        public T GetSaveObject<T>(string uniqueName) where T : ISaveObject, new()
        {
            return GetSaveObject<T>(uniqueName.GetHashCode());
        }

        public void SaveObject<T>(T obj, int hash)
        {
            string key = $"SAVE_OBJ_{hash}";
            string json = JsonUtility.ToJson(obj);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        public void Info()
        {
            Debug.Log($"Level ID: {LevelId}");
            Debug.Log($"Game Time: {gameTime}");
            Debug.Log($"Last Exit Time: {LastExitTime}");
        }
    }
}
