using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] [Tooltip("Number of chunks to spawn on map setup")] 
    private int initialChunks = 7;
    [SerializeField] [Tooltip("Offset between chunks")] 
    private float offset = 10f;
    [SerializeField][Range(1, 10)][Tooltip("Number of chunks to spawn ahead")]
    private int chunksToSpawn = 1;
    [SerializeField][Range(0, 10)][Tooltip("Number of chunks to keep behind")]
    private int chunksToRemove = 1;
    [SerializeField] [Tooltip("Location the player needs to cross to generate new chunks")]
    private int crossChunk;
    [SerializeField] [Tooltip("Chunk rotation in degrees")]
    private Vector3 chunkRotation;
    
    [Header("Prefabs")]
    [SerializeField] [Tooltip("Prefab pool")]
    private Chunk[] chunkPrefabs;
    
    private List<Chunk> chunkTempPrefabs;
    private List<Chunk> chunksSpawned;
    private int chunkCount;
    private Vector3 playerPosition;
    
    [Header("Temp")]
    [SerializeField] private Transform playerTransform;
    
    /// <summary>
    /// Spawn prefab with type chunk in world position
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    private Chunk SpawnChunk(Chunk prefab, Vector3 position)
    {
        Chunk chunk = Instantiate(prefab, position, Quaternion.Euler(chunkRotation));
        
        // set object parent and name
        GameObject go = chunk.gameObject;
        go.transform.parent = transform;
        go.name = $"Chunk_{chunkCount}";

        return chunk;
    }

    /// <summary>
    /// Destroy object with type chunk
    /// </summary>
    /// <param name="chunk"></param>
    private void DestroyChunk(Chunk chunk)
    {
        Destroy(chunk.gameObject);
    }

    private void GenerateMap()
    {
                
        // Set Chunk needed to cross from last spawned Chunk
        crossChunk = Mathf.Max(0, crossChunk);
        int crossIndex = (chunksSpawned.Count - 1) - crossChunk;
        crossIndex = Mathf.Max(crossIndex, 0);
        
        // When player passes crossChunk
        if (playerPosition.z >= chunksSpawned[crossIndex].position.z)
        {
            // Remove Chunks behind
            for (int i = 0; i < chunksToRemove; i++)
            {
                DestroyChunk(chunksSpawned[0]);
                chunksSpawned.Remove(chunksSpawned[0]);
            }
            
            // Spawn Chunks forward
            for (int i = 0; i < chunksToSpawn; i++)
            {
                // Find new position from last spawned Chunk
                Chunk chunkLast = chunksSpawned.Last();
                Vector3 newPosition = new Vector3(chunkLast.position.x, chunkLast.position.y, offset * chunkCount);

                // Create temp list of Chunks that connect
                chunkTempPrefabs = new List<Chunk>();
                foreach (var chunkPrefab in chunkPrefabs)
                {
                    if (chunkPrefab.pathStart == chunkLast.pathEnd)
                    {
                        chunkTempPrefabs.Add(chunkPrefab);
                    }
                }
                
                // Spawn random Chunk from temp list
                int randomIndex = Random.Range(0, chunkTempPrefabs.Count);
                Chunk newChunk = SpawnChunk(chunkTempPrefabs[randomIndex], newPosition);

                chunksSpawned.Add(newChunk);
                chunkCount++;
            }
        }
        
        Debug.DrawLine(chunksSpawned[crossIndex].position + Vector3.left * 10f, 
                            chunksSpawned[crossIndex].position + Vector3.right * 10f,
                                Color.blue);
    }
    
    private void Update()
    {
        playerPosition = playerTransform.transform.position;
        GenerateMap();
        
        Debug.DrawLine(playerPosition + Vector3.left * 10f, playerPosition + Vector3.right * 10f, Color.yellow);
    }
    
    private void Start()
    {
        // create new chunks list
        chunksSpawned = new List<Chunk>();
        
        // reset chunkCount
        chunkCount = 0;
        
        // add initial chunks
        for (int i = 0; i < initialChunks; i++)
        {
            // spawn first chunks
            if (i == 0)
            {
                // spawn main chunk
                Chunk initialChunk = SpawnChunk(chunkPrefabs[0], transform.position);
                
                chunksSpawned.Add(initialChunk);
                chunkCount++;
                continue;
            }
            
            Chunk chunkLast = chunksSpawned.Last();

            Vector3 newPosition = new Vector3(chunkLast.transform.position.x, chunkLast.transform.position.y, offset * chunkCount);
            
            chunkTempPrefabs = new List<Chunk>();
            foreach (var chunkPrefab in chunkPrefabs)
            {
                // save possible chunks that connect
                if (chunkPrefab.pathStart == chunkLast.pathEnd)
                {
                    chunkTempPrefabs.Add(chunkPrefab);
                }
            }
            
            // spawn main chunks
            int randomIndex = Random.Range(0, chunkTempPrefabs.Count);
            Chunk newChunk = SpawnChunk(chunkTempPrefabs[randomIndex], newPosition);

            chunksSpawned.Add(newChunk);
            chunkCount++;
        }
    }
}