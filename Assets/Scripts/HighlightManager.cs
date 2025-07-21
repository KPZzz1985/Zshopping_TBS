using UnityEngine;
using System.Collections.Generic;
using ZShopping.Units;
using UnityEngine.AI;

public class HighlightManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject unitHighlightPrefab;
    public GameObject moveHighlightPrefab;
    public GameObject attackHighlightPrefab;

    [Header("Settings")]
    public float tileSize = 1f;

    private GameObject unitHighlight;
    private readonly List<GameObject> moveHighlights = new List<GameObject>();
    private readonly List<GameObject> attackHighlights = new List<GameObject>();




    public void ClearHighlights()
    {
        if (unitHighlight != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(unitHighlight);
#else
            Destroy(unitHighlight);
#endif
            unitHighlight = null;
        }
        foreach (var go in moveHighlights)
        {
#if UNITY_EDITOR
            DestroyImmediate(go);
#else
            Destroy(go);
#endif
        }
        moveHighlights.Clear();

        foreach (var go in attackHighlights)
        {
#if UNITY_EDITOR
            DestroyImmediate(go);
#else
            Destroy(go);
#endif
        }
        attackHighlights.Clear();
    }




    public void ShowUnitHighlight(UnitBase unit)
    {
        ClearHighlights();
        if (unitHighlightPrefab != null && unit != null)
        {
            unitHighlight = Instantiate(unitHighlightPrefab, unit.transform.position, Quaternion.identity);

            var unitAnim = unitHighlight.GetComponent<Animation>();
#if UNITY_EDITOR
            if (unitAnim != null) DestroyImmediate(unitAnim);
#else
            if (unitAnim != null) Destroy(unitAnim);
#endif

            unitHighlight.transform.SetParent(unit.transform);
            unitHighlight.transform.localPosition = Vector3.zero;
        }
    }




    public void ShowMoveHighlights(UnitBase unit)
    {
        if (moveHighlightPrefab == null || unit == null) return;


        Vector3 unitPos = unit.transform.position;
        float x0 = Mathf.Round(unitPos.x / tileSize) * tileSize;
        float z0 = Mathf.Round(unitPos.z / tileSize) * tileSize;
        Vector3 origin = new Vector3(x0, unitPos.y, z0);
        int range = unit.moveRange;
        float maxDist = range * tileSize + 0.01f;

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dz = -range; dz <= range; dz++)
            {
                Vector3 pos = origin + new Vector3(dx * tileSize, 0.02f, dz * tileSize);
                if (Vector3.Distance(origin, pos) <= maxDist)
                {

                    NavMeshHit navHit;
                    if (NavMesh.SamplePosition(pos, out navHit, tileSize * 0.5f, NavMesh.AllAreas))
                    {

                        NavMeshPath path = new NavMeshPath();
                        if (NavMesh.CalculatePath(origin, navHit.position, NavMesh.AllAreas, path) 
                            && path.status == NavMeshPathStatus.PathComplete)
                        {

                            Vector3 markerPos = new Vector3(pos.x, navHit.position.y + 0.02f, pos.z);
                            var marker = Instantiate(moveHighlightPrefab, markerPos, Quaternion.identity);

                            var moveAnim = marker.GetComponent<Animation>();
#if UNITY_EDITOR
                            if (moveAnim != null) DestroyImmediate(moveAnim);
#else
                            if (moveAnim != null) Destroy(moveAnim);
#endif
                            moveHighlights.Add(marker);
                        }
                    }
                }
            }
        }
    }




    public void ShowAttackHighlights(UnitBase unit, List<UnitBase> enemies)
    {
        if (attackHighlightPrefab == null || unit == null || enemies == null) return;

        foreach (var go in attackHighlights)
        {
#if UNITY_EDITOR
            DestroyImmediate(go);
#else
            Destroy(go);
#endif
        }
        attackHighlights.Clear();

        Vector3 origin = unit.transform.position;
        float range = unit.attackRange + 0.01f;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            if (Vector3.Distance(origin, enemy.transform.position) <= range)
            {
                // Instantiate highlight marker in world space at a fixed offset above the enemy
                Vector3 pos = enemy.transform.position + Vector3.up * 0.1f;
                var marker = Instantiate(attackHighlightPrefab, pos, Quaternion.identity);

                var atkAnim = marker.GetComponent<Animation>();
#if UNITY_EDITOR
                if (atkAnim != null) DestroyImmediate(atkAnim);
#else
                if (atkAnim != null) Destroy(atkAnim);
#endif
                attackHighlights.Add(marker);
            }
        }
    }
} 
