using UnityEditor;
using UnityEngine;

namespace DDARoguelike.Editor
{
    public static class EnemySpawnPointSetup
    {
        private const string EnemySpawnPointScriptGuid = "e2f3a4b5c6d708192a3b4c5d6e7f8091";

        [MenuItem("Tools/DDA Roguelike/Setup EnemySpawnPoints On Normal Rooms")]
        public static void SetupAllNormalRooms()
        {
            string[] roomGuids = AssetDatabase.FindAssets("NormalRoom t:Prefab", new[] { "Assets/04Prefabs/Room" });
            int updatedRooms = 0;
            int updatedPoints = 0;

            for (int i = 0; i < roomGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(roomGuids[i]);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

                if (!fileName.StartsWith("NormalRoom0"))
                {
                    continue;
                }

                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
                EnemySpawnPoint[] existing = prefabRoot.GetComponentsInChildren<EnemySpawnPoint>(true);

                for (int e = 0; e < existing.Length; e++)
                {
                    Object.DestroyImmediate(existing[e]);
                }

                Transform[] transforms = prefabRoot.GetComponentsInChildren<Transform>(true);
                int pointsInRoom = 0;

                for (int t = 0; t < transforms.Length; t++)
                {
                    Transform current = transforms[t];

                    if (current == null || current == prefabRoot.transform)
                    {
                        continue;
                    }

                    if (!current.name.StartsWith("EnemyPosition"))
                    {
                        continue;
                    }

                    if (current.childCount > 0)
                    {
                        continue;
                    }

                    EnemySpawnPoint spawnPoint = current.gameObject.AddComponent<EnemySpawnPoint>();
                    pointsInRoom++;
                    updatedPoints++;
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                updatedRooms++;
                Debug.Log($"[EnemySpawnPointSetup] {fileName}: {pointsInRoom} spawn points.");
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[EnemySpawnPointSetup] Updated {updatedRooms} rooms, {updatedPoints} spawn points. Assign enemy prefabs in Inspector.");
        }
    }
}
