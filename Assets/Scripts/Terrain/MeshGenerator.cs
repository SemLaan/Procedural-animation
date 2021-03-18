using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MeshGenerator : MonoBehaviour
{


    Queue<MapThreadInfo<MeshData>> meshThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();



    private void Update()
    {

        if (meshThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshThreadInfoQueue.Count; i++)
            {

                MapThreadInfo<MeshData> threadInfo = meshThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);

            }
        }

    }


    public void RequestMesh(MapData mapData, Action<MeshData> callback)
    {

        ThreadStart threadStart = delegate {
            MeshThread(mapData, callback);
        };

        new Thread(threadStart).Start();

    }

    private void MeshThread(MapData mapData, Action<MeshData> callback)
    {

        MeshData mesh = MarchingCubes.CreateMesh(mapData);
        lock (meshThreadInfoQueue)
        {
            meshThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, mesh));
        }

    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

    }

}

public struct MapData
{

    public int[,,] densityTensor;
    public Vector3Int size;

    public MapData(int[,,] densityTensor, Vector3Int size)
    {

        this.densityTensor = densityTensor;
        this.size = size;

    }

}

public struct MeshData
{

    public Vector3[] vertices;
    public int[] triangles;

    public MeshData(Vector3[] vertices, int[] triangles)
    {

        this.vertices = vertices;
        this.triangles = triangles;

    }

    public Mesh CreateMesh()
    {

        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;

    }

}
