using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Unity.Netcode;

[System.Serializable]
public class ObstacleDefinition
{
    public GameObject prefab;
    public int minCount = 1;
    public int maxCount = 5;
}

public enum Biome
{
    BrokenRoad,
    DestroedPlaza
}

[System.Serializable]
public class BiomeFloorMapping
{
    public Biome biome;
    public List<GameObject> floorPrefabs;
}

[ExecuteInEditMode]
[DefaultExecutionOrder(-100)]
public class FieldGenerator : MonoBehaviour
{

    public NavMeshSurface surface;


    void Start() { }
    public int width = 10;
    public int height = 10;
    public float tileSize = 1f;
    [Header("Floor Variations")]
    public Biome currentBiome = Biome.BrokenRoad;
    public List<BiomeFloorMapping> biomeFloorMappings;
    public List<ObstacleDefinition> obstacles;
    [Header("Obstacle Settings")]

    public int obstacleSpacing = 1;
    public Transform fieldParent;

    [Header("No Spawn Zones")]
    public List<Collider> noSpawnZones;

    private HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

    private bool IsInsideNoSpawnZone(Vector3 pos)
    {
        if (noSpawnZones == null) return false;
        foreach (var col in noSpawnZones)
            if (col != null && col.bounds.Contains(pos))
                return true;
        return false;
    }


    [ContextMenu("Generate Field")]
    public void GenerateField()
    {

        UnityEngine.Random.InitState(System.Environment.TickCount);

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && !NetworkManager.Singleton.IsServer)
            return;

        if (noSpawnZones != null)
        {
            foreach (var col in noSpawnZones)
                if (col != null)
                    col.enabled = true;
        }
        ClearField();
        switch (currentBiome)
        {
            case Biome.BrokenRoad:
                BrokenRoad();
                break;
            case Biome.DestroedPlaza:
                DestroedPlaza();
                break;
        }

        if (surface != null) surface.BuildNavMesh();

        if (noSpawnZones != null)
        {
            foreach (var col in noSpawnZones)
                if (col != null)
                    col.enabled = false;
        }
    }

    public void ClearField()
    {

        if (fieldParent == null) return;
        for (int i = fieldParent.childCount - 1; i >= 0; i--)
        {

            DestroyImmediate(fieldParent.GetChild(i).gameObject);
        }
        occupied.Clear();
    }

    void GenerateFloor()
    {
        if (biomeFloorMappings == null || biomeFloorMappings.Count == 0) return;
        var mapping = biomeFloorMappings.Find(m => m.biome == currentBiome);
        var prefabs = (mapping != null && mapping.floorPrefabs != null && mapping.floorPrefabs.Count > 0)
            ? mapping.floorPrefabs
            : biomeFloorMappings[0].floorPrefabs;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0f, z * tileSize);

                var obj = Instantiate(prefabs[Random.Range(0, prefabs.Count)], pos, Quaternion.identity, fieldParent);

                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
                {
                    var netObj = obj.GetComponent<NetworkObject>();
                    if (netObj != null)
                        netObj.Spawn();
                }

            }
        }
    }

    void GenerateObstacles()
    {
        if (obstacles == null || obstacles.Count == 0) return;

        var availableCells = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int z = 0; z < height; z++)
            {
                var cell = new Vector2Int(x, z);
                var pos = new Vector3(x * tileSize, 0f, z * tileSize);
                if (occupied.Contains(cell) || IsInsideNoSpawnZone(pos)) continue;
                availableCells.Add(cell);
            }

        ShuffleList(availableCells);
        foreach (var def in obstacles)
        {
            int count = Random.Range(def.minCount, def.maxCount + 1);
            int placed = 0;
            for (int i = 0; i < availableCells.Count && placed < count; )
            {
                var cell = availableCells[i];
                var pos = new Vector3(cell.x * tileSize, 0f, cell.y * tileSize);

                var obj = Instantiate(def.prefab, pos, Quaternion.identity, fieldParent);

                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
                {
                    var netObj = obj.GetComponent<NetworkObject>();
                    if (netObj != null)
                        netObj.Spawn();
                }
                occupied.Add(cell);
                placed++;

                availableCells.RemoveAll(c => Mathf.Abs(c.x - cell.x) <= obstacleSpacing
                                            && Mathf.Abs(c.y - cell.y) <= obstacleSpacing);
            }
        }
    }


    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private void BrokenRoad()
    {
        GenerateFloor();
        GenerateObstacles();
    }

    private void DestroedPlaza()
    {
        GenerateFloor();
        GenerateObstacles();
    }
} 
