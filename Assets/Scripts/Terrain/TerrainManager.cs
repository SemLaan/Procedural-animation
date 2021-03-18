using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainManager : MonoBehaviour
{

    public Vector3Int worldChunkSize;
    public Vector3Int chunkSize;
    private Vector3Int worldGridSize;
    public int[,,] densityTensor;
    public Material material;

    private Chunk[,,] chunks;

    public static MeshGenerator meshGenerator;


    private void Awake()
    {
        RegenerateTerrain();
    }


    public void RegenerateTerrain()
    {

        foreach (Transform child in transform)
        {

            Destroy(child.gameObject);

        }

        meshGenerator = FindObjectOfType<MeshGenerator>();

        material.SetFloat("minHeight", 0f);
        material.SetFloat("maxHeight", 150f);

        worldGridSize = worldChunkSize * (chunkSize - Vector3Int.one);
        worldGridSize += Vector3Int.one;
        chunks = new Chunk[worldChunkSize.x, worldChunkSize.y, worldChunkSize.z];

        GenerateTerrainDensity();
        GenerateTerrainMesh();

    }



    private void GenerateTerrainMesh()
    {

        for (int x = 0; x < worldChunkSize.x; x++)
        {
            for (int y = 0; y < worldChunkSize.y; y++)
            {
                for (int z = 0; z < worldChunkSize.z; z++)
                {

                    int[,,] chunkDensity = new int[chunkSize.x, chunkSize.y, chunkSize.z];

                    Vector3Int ChunkPositionInGrid = new Vector3Int(x, y, z) * (chunkSize - Vector3Int.one);

                    for (int i = 0; i < chunkSize.x; i++)
                    {
                        for (int j = 0; j < chunkSize.y; j++)
                        {
                            for (int k = 0; k < chunkSize.z; k++)
                            {
                                chunkDensity[i, j, k] = densityTensor[i + ChunkPositionInGrid.x, j + ChunkPositionInGrid.y, k + ChunkPositionInGrid.z];
                            }
                        }
                    }

                    MapData chunkData = new MapData(chunkDensity, chunkSize);

                    Chunk chunk = new Chunk(chunkData, new Vector3Int(x, y, z), transform, material);

                    chunks[x, y, z] = chunk;

                }
            }
        }

    }

    private void GenerateTerrainDensity()
    {

        densityTensor = new int[worldGridSize.x, worldGridSize.y, worldGridSize.z];

        for (int x = 0; x < worldGridSize.x; x++)
        {
            for (int y = 0; y < worldGridSize.y; y++)
            {
                for (int z = 0; z < worldGridSize.z; z++)
                {

                    int densityValue = 0;

                    //densityValue = y > 20 ? 1 : -1;
                    float height = Mathf.PerlinNoise(x * 0.01f, z * 0.01f) * 130;
                    height += Mathf.PerlinNoise(x * 0.03f, z * 0.03f) * 10;
                    height += Mathf.PerlinNoise(x * 0.09f, z * 0.09f) * 5;
                    height += Mathf.PerlinNoise(x * 0.018f, z * 0.018f) * 2.5f;
                    height += Mathf.PerlinNoise(x * 0.036f, z * 0.036f) * 1.25f;

                    if (height < 55)
                    {

                        height *= 0.05f;
                        height += 52;

                    }

                    densityValue = (int)((y - height) * 50);


                    densityTensor[x, y, z] = densityValue;

                }
            }
        }
    }
}
