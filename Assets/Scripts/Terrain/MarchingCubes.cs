using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MarchingCubes
{

    public static MeshData CreateMesh(MapData mapData)
    {

        LinkedList<Vector3> vertices = new LinkedList<Vector3>();
        int numberOfVertices = 0;
        Vector3Int gridSize = mapData.size;

        for (int x = 0; x < gridSize.x - 1; x++)
        {
            for (int y = 0; y < gridSize.y - 1; y++)
            {
                for (int z = 0; z < gridSize.z - 1; z++)
                {

                    int[] cubeValues = GetCube(x, y, z, mapData.densityTensor);
                    int cubeIndex = 0;

                    for (int i = 0; i < 8; i++)
                    {
                        if (cubeValues[i] < 0)
                            cubeIndex += 1 << i;
                    }

                    if (!(cubeIndex == 0 || cubeIndex == 255))
                    {

                        Vector3[] adjustedEdgePositions = CalculateSurfaceLevels(LookupTables.edgeTable[cubeIndex], cubeValues);

                        for (int i = 0; LookupTables.triTable[cubeIndex, i] != -1; i++)
                        {

                            //Vector3 localPosition = LookupTables.edgeIndexToPositionTable[LookupTables.triTable[cubeIndex, i]];
                            Vector3 localPosition = adjustedEdgePositions[LookupTables.triTable[cubeIndex, i]];
                            localPosition.x += x;
                            localPosition.y += y;
                            localPosition.z += z;

                            vertices.AddLast(localPosition);
                            numberOfVertices++;

                        }
                    }
                }
            }
        }

        Vector3[] verticesArray = new Vector3[numberOfVertices];
        int[] triangles = new int[numberOfVertices];

        for (int i = 0; i < numberOfVertices; i++)
        {

            verticesArray[i] = vertices.First.Value;
            vertices.RemoveFirst();
            triangles[i] = i;

        }

        MeshData mesh = new MeshData(verticesArray, triangles);

        return mesh;

    }

    private static Vector3[] CalculateSurfaceLevels(int edges, int[] cubeValues)
    {

        Vector3[] surfaceLevels = new Vector3[12];
        LookupTables.edgeIndexToPositionTable.CopyTo(surfaceLevels, 0);

        for (int i = 0; i < 12; i++)
        {
            if (Convert.ToBoolean(1 << i & edges))
            {

                float value1 = cubeValues[LookupTables.edgeToCornerTable[i, 0]];
                float value2 = cubeValues[LookupTables.edgeToCornerTable[i, 1]] - value1;
                float surfaceLevel = -value1;

                surfaceLevel /= value2;

                if (surfaceLevels[i].x == 0.5f)
                    surfaceLevels[i].x = surfaceLevel;
                if (surfaceLevels[i].y == 0.5f)
                    surfaceLevels[i].y = surfaceLevel;
                if (surfaceLevels[i].z == 0.5f)
                    surfaceLevels[i].z = surfaceLevel;

            }
        }

        return surfaceLevels;

    }

    private static int[] GetCube(int x, int y, int z, int[,,] densityTensor)
    {

        int[] cube = new int[8];

        cube[0] = densityTensor[x, y, z];
        cube[1] = densityTensor[x + 1, y, z];
        cube[2] = densityTensor[x + 1, y, z + 1];
        cube[3] = densityTensor[x, y, z + 1];
        cube[4] = densityTensor[x, y + 1, z];
        cube[5] = densityTensor[x + 1, y + 1, z];
        cube[6] = densityTensor[x + 1, y + 1, z + 1];
        cube[7] = densityTensor[x, y + 1, z + 1];

        return cube;

    }

}
