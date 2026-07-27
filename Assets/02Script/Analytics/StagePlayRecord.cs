using System;
using UnityEngine;

namespace DDARoguelike
{
    [Serializable]
    public class StagePlayRecord
    {
        [SerializeField] private string runId;
        [SerializeField] private string recordedAtUtc;
        [SerializeField] private float clearTimeSeconds;
        [SerializeField] private float remainingHpRatio;
        [SerializeField] private float totalDamageDealt;
        [SerializeField] private int hitCount;
        [SerializeField] private float accuracy;
        [SerializeField] private float averageDps;
        [SerializeField] private float longestNoDamageSeconds;

        public string RunId => runId;
        public string RecordedAtUtc => recordedAtUtc;
        public float ClearTimeSeconds => clearTimeSeconds;
        public float RemainingHpRatio => remainingHpRatio;
        public float TotalDamageDealt => totalDamageDealt;
        public int HitCount => hitCount;
        public float Accuracy => accuracy;
        public float AverageDps => averageDps;
        public float LongestNoDamageSeconds => longestNoDamageSeconds;

        public StagePlayRecord(
            string recordRunId,
            string recordRecordedAtUtc,
            float recordClearTimeSeconds,
            float recordRemainingHpRatio,
            float recordTotalDamageDealt,
            int recordHitCount,
            float recordAccuracy,
            float recordAverageDps,
            float recordLongestNoDamageSeconds)
        {
            runId = recordRunId;
            recordedAtUtc = recordRecordedAtUtc;
            clearTimeSeconds = recordClearTimeSeconds;
            remainingHpRatio = recordRemainingHpRatio;
            totalDamageDealt = recordTotalDamageDealt;
            hitCount = recordHitCount;
            accuracy = recordAccuracy;
            averageDps = recordAverageDps;
            longestNoDamageSeconds = recordLongestNoDamageSeconds;
        }
    }
}
