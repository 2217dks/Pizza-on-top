using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PizzaOnTop.Environment
{
    [RequireComponent(typeof(Tilemap))]
    public class CrumblingTilemap2D : MonoBehaviour
    {
        [Header("Tile Crumble & Reappear Settings")]
        [SerializeField] private float crumbleDelay = 0.35f;      // Warning shake duration before tile vanishes
        [SerializeField] private float respawnDelay = 3.0f;      // Seconds before platform reappears
        [SerializeField] private bool crumbleConnectedGroup = true; // Crumble all connected adjacent tiles together!
        [SerializeField] private float shakeIntensity = 0.08f;    // Shake intensity during warning

        private Tilemap tilemap;
        private Transform playerTransform;
        private Collider2D playerCollider;

        // Store original tiles for auto-restoration
        private readonly Dictionary<Vector3Int, TileBase> originalTiles = new Dictionary<Vector3Int, TileBase>();
        private readonly HashSet<Vector3Int> activeCrumblingCells = new HashSet<Vector3Int>();

        private void Awake()
        {
            tilemap = GetComponent<Tilemap>();
        }

        private void Start()
        {
            FindPlayer();
        }

        private void Update()
        {
            if (playerTransform == null || playerCollider == null)
            {
                FindPlayer();
                if (playerTransform == null || playerCollider == null) return;
            }

            CheckPlayerStandingOnTile();
        }

        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerCollider = playerObj.GetComponent<Collider2D>();
            }
        }

        private void CheckPlayerStandingOnTile()
        {
            // Calculate position under player's feet
            Vector3 footPos;
            if (playerCollider != null)
            {
                Bounds bounds = playerCollider.bounds;
                footPos = new Vector3(bounds.center.x, bounds.min.y - 0.1f, transform.position.z);
            }
            else
            {
                footPos = playerTransform.position + Vector3.down * 0.1f;
            }

            Vector3Int cellPos = tilemap.WorldToCell(footPos);

            if (!tilemap.HasTile(cellPos) && playerCollider != null)
            {
                Bounds bounds = playerCollider.bounds;
                cellPos = tilemap.WorldToCell(new Vector3(bounds.min.x + 0.05f, bounds.min.y - 0.1f, transform.position.z));
                if (!tilemap.HasTile(cellPos))
                {
                    cellPos = tilemap.WorldToCell(new Vector3(bounds.max.x - 0.05f, bounds.min.y - 0.1f, transform.position.z));
                }
            }

            if (tilemap.HasTile(cellPos) && !activeCrumblingCells.Contains(cellPos))
            {
                List<Vector3Int> group = crumbleConnectedGroup ? GetConnectedTileCluster(cellPos) : new List<Vector3Int> { cellPos };
                StartCoroutine(RoutineCrumbleGroup(group));
            }
        }

        private List<Vector3Int> GetConnectedTileCluster(Vector3Int startCell)
        {
            List<Vector3Int> cluster = new List<Vector3Int>();
            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

            queue.Enqueue(startCell);
            visited.Add(startCell);

            Vector3Int[] directions = {
                Vector3Int.left, Vector3Int.right, Vector3Int.up, Vector3Int.down,
                new Vector3Int(-1, 1, 0), new Vector3Int(1, 1, 0),
                new Vector3Int(-1, -1, 0), new Vector3Int(1, -1, 0)
            };

            while (queue.Count > 0)
            {
                Vector3Int current = queue.Dequeue();
                if (tilemap.HasTile(current) && !activeCrumblingCells.Contains(current))
                {
                    cluster.Add(current);

                    foreach (Vector3Int dir in directions)
                    {
                        Vector3Int neighbor = current + dir;
                        if (tilemap.HasTile(neighbor) && !visited.Contains(neighbor) && !activeCrumblingCells.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            return cluster;
        }

        private IEnumerator RoutineCrumbleGroup(List<Vector3Int> cellGroup)
        {
            foreach (Vector3Int cell in cellGroup)
            {
                activeCrumblingCells.Add(cell);
                if (!originalTiles.ContainsKey(cell))
                {
                    originalTiles[cell] = tilemap.GetTile(cell);
                }
                // Lock tile transform flags so matrix shaking works
                tilemap.SetTileFlags(cell, TileFlags.None);
            }

            // 1. Shaking warning effect on tile group before vanishing!
            float timer = 0f;
            while (timer < crumbleDelay)
            {
                timer += Time.deltaTime;
                foreach (Vector3Int cell in cellGroup)
                {
                    Vector3 shakeOffset = (Vector3)Random.insideUnitCircle * shakeIntensity;
                    tilemap.SetTransformMatrix(cell, Matrix4x4.TRS(shakeOffset, Quaternion.identity, Vector3.one));
                }
                yield return null;
            }

            // Reset transform matrices
            foreach (Vector3Int cell in cellGroup)
            {
                tilemap.SetTransformMatrix(cell, Matrix4x4.identity);
            }

            // 2. Clear entire connected platform cluster -> Player falls!
            foreach (Vector3Int cell in cellGroup)
            {
                tilemap.SetTile(cell, null);
            }
            Debug.Log($"[CrumblingTilemap2D] Connected platform group of {cellGroup.Count} tiles crumbled!");

            // 3. Wait respawn delay
            yield return new WaitForSeconds(respawnDelay);

            // 4. Safety Check: Wait until player bounds completely clear the group area!
            while (IsPlayerInsideGroupArea(cellGroup))
            {
                yield return new WaitForSeconds(0.2f);
            }

            // 5. Safely restore entire group
            foreach (Vector3Int cell in cellGroup)
            {
                RestoreTile(cell);
            }
        }

        private bool IsPlayerInsideGroupArea(List<Vector3Int> cellGroup)
        {
            if (playerCollider == null) return false;

            Bounds pBounds = playerCollider.bounds;

            foreach (Vector3Int cell in cellGroup)
            {
                Vector3 worldCenter = tilemap.GetCellCenterWorld(cell);
                Vector3 cellSize = tilemap.cellSize;
                Bounds cellBounds = new Bounds(worldCenter, cellSize * 0.95f);

                if (pBounds.Intersects(cellBounds))
                {
                    return true;
                }
            }

            return false;
        }

        public void RestoreTile(Vector3Int cellPos)
        {
            if (originalTiles.TryGetValue(cellPos, out TileBase tile))
            {
                tilemap.SetTile(cellPos, tile);
                tilemap.SetTransformMatrix(cellPos, Matrix4x4.identity);
            }
            activeCrumblingCells.Remove(cellPos);
        }

        public void ResetAllCrumblingTiles()
        {
            StopAllCoroutines();
            foreach (var kvp in originalTiles)
            {
                tilemap.SetTile(kvp.Key, kvp.Value);
                tilemap.SetTransformMatrix(kvp.Key, Matrix4x4.identity);
            }
            activeCrumblingCells.Clear();
        }
    }
}
