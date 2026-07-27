using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DDARoguelike
{
    public class StagePlayRecordStore
    {
        private const string RecordFileName = "stage-play-records.json";

        [Serializable]
        private class StagePlayRecordCollection
        {
            [SerializeField] private List<StagePlayRecord> records = new List<StagePlayRecord>();

            public List<StagePlayRecord> Records => records;
        }

        private readonly string filePath;

        public string FilePath => filePath;

        public StagePlayRecordStore()
        {
            filePath = Path.Combine(Application.persistentDataPath, RecordFileName);
        }

        public bool Append(StagePlayRecord record)
        {
            if (record == null)
            {
                Debug.LogError($"[{nameof(StagePlayRecordStore)}] Cannot save a null record.");
                return false;
            }

            StagePlayRecordCollection collection = LoadCollection();
            collection.Records.Add(record);

            try
            {
                string directoryPath = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string json = JsonUtility.ToJson(collection, true);
                File.WriteAllText(filePath, json);
                Debug.Log($"[{nameof(StagePlayRecordStore)}] Saved stage play record to {filePath}.");
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[{nameof(StagePlayRecordStore)}] Failed to save stage play records to {filePath}: {exception.Message}");
                return false;
            }
        }

        private StagePlayRecordCollection LoadCollection()
        {
            if (!File.Exists(filePath))
            {
                return new StagePlayRecordCollection();
            }

            try
            {
                string json = File.ReadAllText(filePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.LogWarning(
                        $"[{nameof(StagePlayRecordStore)}] Record file is empty. A new collection will be created: {filePath}");
                    return new StagePlayRecordCollection();
                }

                StagePlayRecordCollection collection =
                    JsonUtility.FromJson<StagePlayRecordCollection>(json);

                if (collection == null || collection.Records == null)
                {
                    Debug.LogError(
                        $"[{nameof(StagePlayRecordStore)}] Record file is invalid. A new collection will be created: {filePath}");
                    return new StagePlayRecordCollection();
                }

                return collection;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[{nameof(StagePlayRecordStore)}] Failed to read stage play records from {filePath}: {exception.Message}");
                return new StagePlayRecordCollection();
            }
        }
    }
}
