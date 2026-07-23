using System.Collections;
using UnityEngine;

namespace DDARoguelike
{
    public class RoomDoor : MonoBehaviour
    {
        private const string ClosedChildName = "Closed";
        private const string PlayerTag = "Player";
        private const float InwardOffset = 1.0f;
        private const float TransitionUnlockDelay = 0.2f;

        private RoomController ownerRoom;
        private RoomController targetRoom;
        private Vector2Int direction;
        private GameObject closedObject;
        private RoomCamera roomCamera;

        public void Initialize(RoomController owner, RoomController target, Vector2Int doorDirection)
        {
            if (owner == null || target == null)
            {
                Debug.LogError($"[{nameof(RoomDoor)}] Owner and target rooms are required on {gameObject.name}.", this);
                return;
            }

            ownerRoom = owner;
            targetRoom = target;
            direction = doorDirection;

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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(PlayerTag))
            {
                return;
            }

            TryTransition(other.transform);
        }

        private void TryTransition(Transform playerTransform)
        {
            if (RoomTransition.IsBusy)
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
            destinationPosition.x += direction.x * InwardOffset;
            destinationPosition.y += direction.y * InwardOffset;

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

            StartCoroutine(FinishTransition(playerMove));
        }

        private IEnumerator FinishTransition(PlayerMove playerMove)
        {
            yield return new WaitForSecondsRealtime(TransitionUnlockDelay);

            if (playerMove != null)
            {
                playerMove.SetMovementEnabled(true);
            }

            RoomTransition.End();
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

            bool isOpen = ownerRoom.IsCleared && targetRoom.IsCleared;
            closedObject.SetActive(!isOpen);
        }
    }
}
