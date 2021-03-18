using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{

    GameObject meshObject;
    Vector3 position;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    public Chunk(MapData mapData, Vector3Int coord, Transform parent, Material material)
    {

        position = coord * (mapData.size - Vector3Int.one);

        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshRenderer.sharedMaterial = material;
        meshObject.transform.parent = parent;
        meshObject.transform.position = position;

        TerrainManager.meshGenerator.RequestMesh(mapData, OnMeshReceived);

    }

    public void UpdateChunk(MapData mapData)
    {

        TerrainManager.meshGenerator.RequestMesh(mapData, OnMeshReceived);

    }

    private void OnMeshReceived(MeshData meshData)
    {
        Mesh mesh = meshData.CreateMesh();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

}
