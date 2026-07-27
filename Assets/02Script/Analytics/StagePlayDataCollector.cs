using System;
using UnityEngine;

namespace DDARoguelike
{
    public class StagePlayDataCollector : MonoBehaviour
    {
        private const string PlayerAttackerName = "Player";
        private const string BombAttackerName = "Bomb";
        private const float MinimumStageDuration = 0.0001f;

        [SerializeField] private RoomGenerator roomGenerator;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerAttack playerAttack;

        private StagePlayRecordStore recordStore;
        private RoomController bossRoom;
        private Enemy[] trackedEnemies = Array.Empty<Enemy>();
        private bool isStageActive;
        private bool isRecordFinalized;
        private float stageStartTime;
        private float lastHitTime;
        private float longestNoDamageSeconds;
        private float totalDamageDealt;
        private int hitCount;
        private int firedProjectileCount;
        private int hitProjectileCount;

        private void Awake()
        {
            recordStore = new StagePlayRecordStore();
            ValidateReferences();
        }

        private void OnEnable()
        {
            if (roomGenerator != null)
            {
                roomGenerator.StageStarted += HandleStageStarted;
            }

            if (playerHealth != null)
            {
                playerHealth.Damaged += HandlePlayerDamaged;
            }

            if (playerAttack != null)
            {
                playerAttack.ShotsFired += HandleShotsFired;
            }

        }

        private void OnDisable()
        {
            if (roomGenerator != null)
            {
                roomGenerator.StageStarted -= HandleStageStarted;
            }

            if (playerHealth != null)
            {
                playerHealth.Damaged -= HandlePlayerDamaged;
            }

            if (playerAttack != null)
            {
                playerAttack.ShotsFired -= HandleShotsFired;
            }

            UnsubscribeFromBossRoom();
        }

        private void HandleStageStarted(RoomController generatedBossRoom)
        {
            UnsubscribeFromBossRoom();
            ResetStageCounters();

            if (generatedBossRoom == null)
            {
                Debug.LogError($"[{nameof(StagePlayDataCollector)}] Boss room is null.", this);
                return;
            }

            if (generatedBossRoom.RoomType != RoomType.Boss)
            {
                Debug.LogError(
                    $"[{nameof(StagePlayDataCollector)}] Expected a boss room but received {generatedBossRoom.RoomType}.",
                    generatedBossRoom);
                return;
            }

            if (generatedBossRoom.IsCleared)
            {
                Debug.LogError(
                    $"[{nameof(StagePlayDataCollector)}] Boss room is already cleared when the stage starts.",
                    generatedBossRoom);
                return;
            }

            bossRoom = generatedBossRoom;
            bossRoom.ClearedChanged += HandleBossRoomCleared;

            trackedEnemies = bossRoom.GetComponentsInChildren<Enemy>(true);

            for (int i = 0; i < trackedEnemies.Length; i++)
            {
                Enemy enemy = trackedEnemies[i];

                if (enemy != null)
                {
                    enemy.Damaged += HandleEnemyDamaged;
                }
            }

            stageStartTime = Time.time;
            lastHitTime = stageStartTime;
            isStageActive = true;

            Debug.Log(
                $"[{nameof(StagePlayDataCollector)}] Stage recording started. Boss enemies: {trackedEnemies.Length}.",
                this);
        }

        private void HandlePlayerDamaged(int appliedDamage, string attackerName)
        {
            if (!isStageActive || appliedDamage <= 0)
            {
                return;
            }

            float now = Time.time;
            longestNoDamageSeconds = Mathf.Max(longestNoDamageSeconds, now - lastHitTime);
            lastHitTime = now;
            hitCount++;
        }

        private void HandleShotsFired(int count)
        {
            if (!isStageActive || count <= 0)
            {
                return;
            }

            firedProjectileCount += count;
        }

        private void HandleEnemyDamaged(float appliedDamage, string attackerName)
        {
            if (!isStageActive || appliedDamage <= 0.0f)
            {
                return;
            }

            if (attackerName == PlayerAttackerName)
            {
                totalDamageDealt += appliedDamage;
                hitProjectileCount++;
            }
            else if (attackerName == BombAttackerName)
            {
                totalDamageDealt += appliedDamage;
            }
        }

        private void HandleBossRoomCleared()
        {
            if (!isStageActive || isRecordFinalized || bossRoom == null || !bossRoom.IsCleared)
            {
                return;
            }

            isRecordFinalized = true;
            float stageEndTime = Time.time;
            float clearTimeSeconds = Mathf.Max(MinimumStageDuration, stageEndTime - stageStartTime);
            longestNoDamageSeconds =
                Mathf.Max(longestNoDamageSeconds, stageEndTime - lastHitTime);

            float remainingHpRatio = 0.0f;

            if (playerHealth != null && playerHealth.MaxHp > 0)
            {
                remainingHpRatio = Mathf.Clamp01(
                    playerHealth.CurrentHp / (float)playerHealth.MaxHp);
            }

            float accuracy = firedProjectileCount > 0
                ? Mathf.Clamp01(hitProjectileCount / (float)firedProjectileCount)
                : 0.0f;

            float averageDps = totalDamageDealt / clearTimeSeconds;
            StagePlayRecord record = new StagePlayRecord(
                Guid.NewGuid().ToString("N"),
                DateTime.UtcNow.ToString("O"),
                clearTimeSeconds,
                remainingHpRatio,
                totalDamageDealt,
                hitCount,
                accuracy,
                averageDps,
                longestNoDamageSeconds);

            isStageActive = false;
            recordStore.Append(record);

            Debug.Log(
                $"[{nameof(StagePlayDataCollector)}] Stage record completed. "
                + $"ClearTime={clearTimeSeconds:F2}, HP={remainingHpRatio:F2}, "
                + $"Damage={totalDamageDealt:F2}, Hits={hitCount}, Accuracy={accuracy:F2}, "
                + $"DPS={averageDps:F2}, "
                + $"LongestNoDamage={longestNoDamageSeconds:F2}.",
                this);

            UnsubscribeFromBossRoom();
        }

        private void ResetStageCounters()
        {
            isStageActive = false;
            isRecordFinalized = false;
            stageStartTime = 0.0f;
            lastHitTime = 0.0f;
            longestNoDamageSeconds = 0.0f;
            totalDamageDealt = 0.0f;
            hitCount = 0;
            firedProjectileCount = 0;
            hitProjectileCount = 0;
        }

        private void UnsubscribeFromBossRoom()
        {
            if (bossRoom != null)
            {
                bossRoom.ClearedChanged -= HandleBossRoomCleared;
            }

            for (int i = 0; i < trackedEnemies.Length; i++)
            {
                Enemy enemy = trackedEnemies[i];

                if (enemy != null)
                {
                    enemy.Damaged -= HandleEnemyDamaged;
                }
            }

            trackedEnemies = Array.Empty<Enemy>();
            bossRoom = null;
        }

        private void ValidateReferences()
        {
            if (roomGenerator == null)
            {
                Debug.LogError(
                    $"[{nameof(StagePlayDataCollector)}] RoomGenerator is not assigned on {gameObject.name}.",
                    this);
            }

            if (playerHealth == null)
            {
                Debug.LogError(
                    $"[{nameof(StagePlayDataCollector)}] PlayerHealth is not assigned on {gameObject.name}.",
                    this);
            }

            if (playerAttack == null)
            {
                Debug.LogError(
                    $"[{nameof(StagePlayDataCollector)}] PlayerAttack is not assigned on {gameObject.name}.",
                    this);
            }

        }
    }
}
