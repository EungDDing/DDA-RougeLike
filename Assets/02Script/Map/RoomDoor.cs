using System.Collections;
using UnityEngine;

namespace DDARoguelike
{
    public class RoomDoor : MonoBehaviour
    {
        private const string ClosedChildName = "Closed";
        private const int OverlapBufferSize = 8;

        [SerializeField] private float inwardOffset = 2.0f;
        [SerializeField] private float transitionUnlockDelay = 0.45f;

        private readonly Collider2D[] overlapBuffer = new Collider2D[OverlapBufferSize];

        private RoomController ownerRoom;
        private RoomController targetRoom;
        private Vector2Int direction;
        private GameObject closedObject;
        private RoomCamera roomCamera;
        private EnemyPool enemyPool;
        private Collider2D doorCollider;
        private bool suppressUntilExit;

        public RoomController TargetRoom => targetRoom;

        public void Initialize(
            RoomController owner,
            RoomController target,
            Vector2Int doorDirection,
            float doorInwardOffset,
            float doorTransitionUnlockDelay)
        {
            if (owner == null || target == null)
            {
                Debug.LogError($"[{nameof(RoomDoor)}] Owner and target rooms are required on {gameObject.name}.", this);
                return;
            }

            ownerRoom = owner;
            targetRoom = target;
            direction = doorDirection;
            inwardOffset = doorInwardOffset;
            transitionUnlockDelay = doorTransitionUnlockDelay;
            doorCollider = GetComponent<Collider2D>();

            Transform closedTransform = transform.Find(ClosedChildName);

            if (closedTransform == null)
            {
                Debug.LogError($"[{nameof(RoomDoor)}] Child '{ClosedChildName}' was not found on {gameObject.name}.", this);
                return;
            }

            closedObject = closedTransform.gameObject;

            ownerRoom.ClearedChanged += RefreshDoorState;
            targetRoom.ClearedChanged += RefreshDoorState;
            RefreshDoorState();
        }

        private void OnDestroy()
        {
            if (ownerRoom != null)
            {
                ownerRoom.ClearedChanged -= RefreshDoorState;
            }

            if (targetRoom != null)
            {
                targetRoom.ClearedChanged -= RefreshDoorState;
            }
        }

        private void FixedUpdate()
        {
            if (!suppressUntilExit)
            {
                return;
            }

            if (!IsPlayerOverlapping())
            {
                suppressUntilExit = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            TryTransition(other.transform);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            suppressUntilExit = false;
        }

        public void SuppressUntilExit()
        {
            suppressUntilExit = true;
        }

        private void TryTransition(Transform playerTransform)
        {
            if (RoomTransition.IsBusy || suppressUntilExit)
            {
                return;
            }

            if (!IsOpen())
            {
                return;
            }

            if (targetRoom == null)
            {
                return;
            }

            RoomDoor destinationDoor = null;

            if (!targetRoom.TryGetDoor(-direction, out destinationDoor) || destinationDoor == null)
            {
                Debug.LogError(
                    $"[{nameof(RoomDoor)}] Destination door was not found on {targetRoom.name} for direction {-direction}.",
                    this);
                return;
            }

            if (!RoomTransition.TryBegin())
            {
                return;
            }

            PlayerMove playerMove = playerTransform.GetComponent<PlayerMove>();

            if (playerMove == null)
            {
                playerMove = playerTransform.GetComponentInParent<PlayerMove>();
            }

            if (playerMove != null)
            {
                playerMove.SetMovementEnabled(false);
            }

            Rigidbody2D playerRigidbody = playerTransform.GetComponent<Rigidbody2D>();

            if (playerRigidbody == null)
            {
                playerRigidbody = playerTransform.GetComponentInParent<Rigidbody2D>();
            }

            Vector3 destinationPosition = destinationDoor.transform.position;
            destinationPosition.x += direction.x * inwardOffset;
            destinationPosition.y += direction.y * inwardOffset;

            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector2.zero;
                playerRigidbody.position = destinationPosition;
            }
            else
            {
                playerTransform.position = destinationPosition;
            }

            if (roomCamera == null)
            {
                roomCamera = FindFirstObjectByType<RoomCamera>();
            }

            if (roomCamera != null)
            {
                roomCamera.FocusRoom(targetRoom);
            }
            else
            {
                Debug.LogError($"[{nameof(RoomDoor)}] {nameof(RoomCamera)} was not found in the scene.", this);
            }

            if (enemyPool == null)
            {
                enemyPool = FindFirstObjectByType<EnemyPool>();
            }

            if (targetRoom != null)
            {
                targetRoom.TrySpawnEnemies(enemyPool);
                targetRoom.TrySpawnItemBoxOnEnter();
                UpdateBossHpHud(targetRoom);
                UpdateMiniMapHud(targetRoom);
            }

            PlayerItemInventory itemInventory = playerTransform.GetComponent<PlayerItemInventory>();

            if (itemInventory == null)
            {
                itemInventory = playerTransform.GetComponentInParent<PlayerItemInventory>();
            }

            if (itemInventory != null)
            {
                itemInventory.NotifyRoomEntered();
            }

            destinationDoor.SuppressUntilExit();
            StartCoroutine(FinishTransition(playerMove));
        }

        private IEnumerator FinishTransition(PlayerMove playerMove)
        {
            yield return new WaitForSecondsRealtime(transitionUnlockDelay);

            if (playerMove != null)
            {
                playerMove.SetMovementEnabled(true);
            }

            RoomTransition.End();
        }

        private bool IsPlayerOverlapping()
        {
            if (doorCollider == null)
            {
                doorCollider = GetComponent<Collider2D>();
            }

            if (doorCollider == null)
            {
                return false;
            }

            ContactFilter2D filter = new ContactFilter2D();
            filter.NoFilter();
            filter.useTriggers = true;
            int count = doorCollider.Overlap(filter, overlapBuffer);

            for (int i = 0; i < count; i++)
            {
                Collider2D hit = overlapBuffer[i];

                if (hit != null && hit.CompareTag("Player"))
                {
                    return true;
                }
            }

            return false;
        }

        private static void UpdateBossHpHud(RoomController enteredRoom)
        {
            BossHpHud bossHpHud = FindFirstObjectByType<BossHpHud>(FindObjectsInactive.Include);

            if (bossHpHud == null)
            {
                return;
            }

            if (enteredRoom != null && enteredRoom.RoomType == RoomType.Boss)
            {
                bossHpHud.BindBossRoom(enteredRoom);
            }
            else
            {
                bossHpHud.Clear();
            }
        }

        private static void UpdateMiniMapHud(RoomController enteredRoom)
        {
            MiniMapHud miniMapHud = FindFirstObjectByType<MiniMapHud>(FindObjectsInactive.Include);

            if (miniMapHud == null)
            {
                return;
            }

            miniMapHud.NotifyRoomEntered(enteredRoom);
        }

        private bool IsOpen()
        {
            if (closedObject == null)
            {
                return false;
            }

            return !closedObject.activeSelf;
        }

        private void RefreshDoorState()
        {
            if (closedObject == null || ownerRoom == null || targetRoom == null)
            {
                return;
            }

            bool isOpen = ownerRoom.IsCleared;
            closedObject.SetActive(!isOpen);
        }
    }
}
