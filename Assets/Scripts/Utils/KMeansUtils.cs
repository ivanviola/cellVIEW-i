using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KMeansClustering
{
    public static List<Vector4> GetClusters(List<Vector4> atoms, int numCentroids)
    {
        if (ComputeShaderManager.Get.KMeansCS == null) throw new Exception("KMeans compute shader not assigned");

        if (numCentroids <= 0) throw new Exception("Num centroids too low");

        var centroids = new List<Vector4>();
        var centroidStep = Mathf.CeilToInt(atoms.Count / (float)numCentroids);
        for (int i = 0; i < numCentroids; i++)
        {
            if (i*centroidStep < atoms.Count)
            {
                centroids.Add(atoms[i * centroidStep]); 
            }
            else
            {
                centroids.Add(atoms[UnityEngine.Random.Range(0, atoms.Count)]);
            }
        }

        var centroidBuffer = new ComputeBuffer(numCentroids, 4 * sizeof(float));
        centroidBuffer.SetData(centroids.ToArray());

        var pointBuffer = new ComputeBuffer(atoms.Count, 4 * sizeof(float));
        pointBuffer.SetData(atoms.ToArray());

        var membershipBuffer = new ComputeBuffer(atoms.Count, sizeof(int));
        
        ComputeShaderManager.Get.KMeansCS.SetInt("_NumPoints", atoms.Count);
        ComputeShaderManager.Get.KMeansCS.SetInt("_NumCentroids", numCentroids);

        for (int i = 0; i < 5; i++)
        {
            ComputeShaderManager.Get.KMeansCS.SetBuffer(0, "_PointBuffer", pointBuffer);
            ComputeShaderManager.Get.KMeansCS.SetBuffer(0, "_CentroidBuffer", centroidBuffer);
            ComputeShaderManager.Get.KMeansCS.SetBuffer(0, "_MembershipBuffer", membershipBuffer);
            ComputeShaderManager.Get.KMeansCS.Dispatch(0, Mathf.CeilToInt(atoms.Count / 64.0f), 1, 1);
            
            ComputeShaderManager.Get.KMeansCS.SetBuffer(1, "_PointBuffer", pointBuffer);
            ComputeShaderManager.Get.KMeansCS.SetBuffer(1, "_NewCentroidBuffer", centroidBuffer);
            ComputeShaderManager.Get.KMeansCS.SetBuffer(1, "_NewMembershipBuffer", membershipBuffer);
            ComputeShaderManager.Get.KMeansCS.Dispatch(1, Mathf.CeilToInt(numCentroids / 64.0f), 1, 1);
        }

        var newCentroids = new Vector4[numCentroids];
        centroidBuffer.GetData(newCentroids);
        
        pointBuffer.Release();
        centroidBuffer.Release();
        membershipBuffer.Release();

        return newCentroids.ToList();
    }
}

