using System.Collections;
using UnityEngine;

namespace DDARoguelike
{
    public class RoomCamera : MonoBehaviour
    {
        [SerializeField] private float focusDuration = 0.15f;

        private Coroutine focusRoutine;

        public void FocusRoom(RoomController room)
        {
            if (room == null)
            {
                Debug.LogError($"[{nameof(RoomCamera)}] Room is null.", this);
                return;
            }

            if (focusRoutine != null)
            {
                StopCoroutine(focusRoutine);
            }

            focusRoutine = StartCoroutine(FocusRoomRoutine(room.transform.position));
        }

        public void FocusRoomImmediate(RoomController room)
        {
            if (room == null)
            {
                Debug.LogError($"[{nameof(RoomCamera)}] Room is null.", this);
                return;
            }

            if (focusRoutine != null)
            {
                StopCoroutine(focusRoutine);
                focusRoutine = null;
            }

            Vector3 targetPosition = transform.position;
            targetPosition.x = room.transform.position.x;
            targetPosition.y = room.transform.position.y;
            transform.position = targetPosition;
        }

        private IEnumerator FocusRoomRoutine(Vector3 roomPosition)
        {
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = startPosition;
            targetPosition.x = roomPosition.x;
            targetPosition.y = roomPosition.y;

            if (focusDuration <= 0.0f)
            {
                transform.position = targetPosition;
                focusRoutine = null;
                yield break;
            }

            float elapsed = 0.0f;

            while (elapsed < focusDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / focusDuration);
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            transform.position = targetPosition;
            focusRoutine = null;
        }
    }
}
