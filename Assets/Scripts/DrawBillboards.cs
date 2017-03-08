//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.VR;
//using Random = UnityEngine.Random;

//[ExecuteInEditMode]
//public class DrawBillboards : MonoBehaviour
//{
//    public Material Mat;
//    public Mesh QuadMesh;

//    private ComputeBuffer QuadUVs;
//    private ComputeBuffer QuadIndices;
//    private ComputeBuffer QuadVertices;

//    private ComputeBuffer CBPositions;
//    private ComputeBuffer CBNodesInfo;
//    private ComputeBuffer CBWedgesInfo;

//    public const int NumGroupsMax = 6;
//    public const int NumInstancesMax = 1000;

//    //[HideInInspector]
//    public int NumGroups = 6;

//    //[HideInInspector]
//    public const int NumInstances = 150;

//    [Range(0, 100)]
//    public float GroupSpacing = 70;

//    [Range(0, 100)]
//    public float WedgeAngleMax = 70;

//    public float Damping = 0.1f;
//    //public float GroupPowFactor = 1;
//    public float HueCircleRadius = 10;

//    public bool UseHCL;
//    public bool ShowAtoms;
//    public bool ShowChains;
//    public bool ShowResidues;
//    public bool ShowSecondaryStructures;


//    public float ChainDistance;
//    public float AtomDistance;
//    public float ResidueDistance;
//    public float SecondaryStructureDistance;

//    //public bool ComputeGroupingForces;
//    //public bool ComputeNodeDistancesForces;
//    //public bool ComputCircleSnappingForces;

//    //public float GroupingForce = 1;
//    //public float NodeDistanceForce = 1;
//    //public float CircleSnappingForce = 1;

//    //public float NodeGroupDistanceRadius = 3;
//    //public float NodeAlienDistanceRadius = 3;

//    //***********************************//

//    [HideInInspector]
//    public Vector4[] GroupCentroids = new Vector4[NumGroupsMax];

//    [HideInInspector]
//    public Vector2[] GroupWedgesInfo = new Vector2[NumGroupsMax];

//    /*******/

//    [HideInInspector]
//    public Vector4[] WedgeInfo = new Vector4[NumInstancesMax];

//    [HideInInspector]
//    public Vector4[] WedgePositions = new Vector4[NumInstancesMax];

//    [HideInInspector]
//    public Vector4[] WedgeVelocities = new Vector4[NumInstancesMax];

//    //*****//
    
//    //public float[] WedgeAngles = new float[NumGroupsMax];
    
//    public int[] GroupIndices = new int[NumGroupsMax];
    
//    public float[] WedgeSize = new float[NumGroupsMax];

//    [HideInInspector]
//    public float[] WedgeAngles = new float[NumGroupsMax];

//    [HideInInspector]
//    public float[] WedgeRadius = new float[NumGroupsMax];

//    [HideInInspector]
//    public float[] WedgeSizeScaled = new float[NumGroupsMax];
    
//    [HideInInspector]
//    public float[] WedgeAngularVelocity = new float[NumInstancesMax];
    
//    //***********************************//

//    [HideInInspector]
//    public Vector4[] Info2 = new Vector4[NumInstancesMax];

//    [HideInInspector]
//    public Vector4[] Positions2 = new Vector4[NumInstancesMax];

//    [HideInInspector]
//    public Vector4[] Velocities2 = new Vector4[NumInstancesMax];

//    //***********************************//

//    void OnEnable()
//    {
//        InitSystem();
//    }

//    void InitSystem()
//    {
//        if (CBPositions == null)
//        {

//            InitPosition();
//            CreateResources();
//        }
//    }

//    void InitPosition()
//    {
//        NumGroups = SceneManager.Get.IngredientGroups.Count;

//        var wedgeSize = 360.0f / NumGroups;

//        // Groups

//        var ingredientsTotal = 0;
//        foreach (var group in SceneManager.Get.IngredientGroups)
//        {
//            ingredientsTotal += group.Ingredients.Count;
//        }


//        for (var i = 0; i < NumGroups; i++)
//        {
//            var group = SceneManager.Get.IngredientGroups[GroupIndices[i]];

//            var angle = i * wedgeSize * Mathf.Deg2Rad;

//            WedgeInfo[i] = new Vector4((int)i, 1, 0);
//            WedgeAngles[i] = angle * Mathf.Rad2Deg + 180;
//            WedgeVelocities[i] = Vector4.zero;

//            //var size = 10;
//            //var size = group.Ingredients.Count;
//            //WedgeSize[i] = size;

//        }

//        // Ingredients

//        for (var i = 0; i < NumInstances; i++)
//        {
//            Info2[i] = new Vector4(Random.Range(0, NumGroups), 1, 0.25f);
//            Positions2[i] = Vector4.zero;
//            Velocities2[i] = Vector4.zero;
//        }
//    }

//    void Update_()
//    {
//        //InitSystem();
//        //UpdatePositions();
//        ////// Clear wedges info
//        ////for (var i = 0; i < NumGroupsMax; i++)
//        ////{
//        ////    GroupWedgesInfo[i] = new Vector2(1000, 0);
//        ////}

//        ////// Compute wedges info
//        ////for (var i = 0; i < NumInstances; i++)
//        ////{
//        ////    if (WedgePositions[i].w < 0) continue;

//        ////    var groupID = (int)WedgeInfo[i].x;
//        ////    var position = (Vector2)WedgePositions[i];
//        ////    var dist = position.magnitude;

//        ////    GroupWedgesInfo[groupID].x = Mathf.Min(GroupWedgesInfo[groupID].x, dist);
//        ////    GroupWedgesInfo[groupID].y = Mathf.Max(GroupWedgesInfo[groupID].y, dist);
//        ////}

//        //if (EditorApplication.isPlaying)
//        //{
            
//        //}
//    }

//    float NormalizeAngled(float angle)
//    {
//        if (angle >= 0)
//        {
//            return angle % 360;
//        }

//        return 360 - Mathf.Abs(angle) % 360;
//    }

//    float GetAngleDiff(float angle1, float angle2)
//    {
//        var n1 = NormalizeAngled(angle1);
//        var n2 = NormalizeAngled(angle2);

//        var diff = Mathf.Abs(n1 - n2);
//        if (diff > 180) return 360 - diff;
//        return diff;
//    }

//    int GetAngleDir(float angle1, float angle2)
//    {
//        var n1 = NormalizeAngled(angle1);
//        var n2 = NormalizeAngled(angle2);

//        var diff = Mathf.Abs(n1 - n2);
//        if (diff < 180)
//        {
//            if (n1 > n2) return 1;
//            return -1;
//        }

//        if (n1 < n2) return 1;
//        return -1;
//    }

//    Vector2 GetPositionFromAngle(float angle)
//    {
//        angle = angle * Mathf.Deg2Rad;
//        return new Vector2(Mathf.Cos(angle) * HueCircleRadius, Mathf.Sin(angle) * HueCircleRadius);
//    }


//    void UpdatePositions()
//    {
//        var numPixels = (float) (Screen.height*Screen.width);

//        for (var i = 0; i < NumGroups; i++)
//        {
//            var groupDisplayInfo = CPUBuffers.Get.IngredientGroupsDisplayInfo[GroupIndices[i]];

//            var visibleIngredientsCount = 0;
//            foreach (var ingredient in SceneManager.Get.IngredientGroups[GroupIndices[i]].Ingredients)
//            {
//                var displayInfo = CPUBuffers.Get.IngredientsDisplayInfo[ingredient._ingredientId];
//                var ratio = (float)displayInfo.b/ (float)displayInfo.a;

//                var screenCoverage = displayInfo.c / (float)(Screen.height * Screen.width);

//                if (screenCoverage > 0.1) visibleIngredientsCount++;
//                else if(ratio > 0.1) visibleIngredientsCount++;
//                //else if(screenCoverage > 0.1) visibleIngredientsCount++;
//                else if (ratio > 0.01 && screenCoverage > 0.01) visibleIngredientsCount++;
//            }

//            var cc = groupDisplayInfo.c/numPixels;
//            var groupCoverage = Mathf.Min(cc*10, 1);

//            var ratio2 = (float)visibleIngredientsCount /(float)SceneManager.Get.IngredientGroups[GroupIndices[i]].Ingredients.Count;
//            ratio2 *= groupCoverage;
//            //var ratio3 = Mathf.Min(groupDisplayInfo.c / (float)(Screen.height * Screen.width) * 2, 1.0f);

//            //if (SceneManager.Get.IngredientGroups[GroupIndices[i]].Ingredients.Count == 1) ratio3 = 1;

//            //var ratio = CPUBuffers.Get.IngredientGroupsDisplayInfo[i].y
//            //WedgeSizeScaled[i] += (WedgeSize[i] * ratio2 * ratio3 - WedgeSizeScaled[i]) * 0.05f;
//            WedgeSizeScaled[i] += (WedgeSize[i] * ratio2 - WedgeSizeScaled[i]) * 0.1f;

//            if(WedgeSizeScaled[i] <= 0.05) WedgeSizeScaled[i] = 0;
//        }
        
//        var acc = 0.0f;
//        for (var i = 0; i < NumGroups; i++)
//        {
//            if (WedgeSizeScaled[i] <= 0) continue;

//            //var diameterInHue = (2 * WedgeSize[i] + GroupSpacing);
//            var diameterInHue = ( GroupSpacing);
//            acc += diameterInHue;
//        }

//        var radiusLeftOver = (360 - acc);
//        //var radiusLeftOver = 360.0f;

//        var calcTotal = 0.0f;
//        for (var i = 0; i < NumGroups; i++)
//        {
//            //if (WedgeSizeScaled[i] <= 0) continue;
//            calcTotal += WedgeSizeScaled[i];
//        }

//        for (var i = 0; i < NumGroups; i++)
//        {
//            //if (WedgeSizeScaled[i] <= 0) continue;
//            var prop = (float)WedgeSizeScaled[i] / calcTotal;
//            WedgeRadius[i] = Mathf.Min((radiusLeftOver * prop) / 2.0f, WedgeAngleMax);
//        }

//        for (var i = 0; i < NumGroups; i++)
//        {
//            //if (WedgeSizeScaled[i] <= 0) continue;

//            var angle1 = WedgeAngles[i];
//            var radius1 = (WedgeRadius[i]) / 360 * (2 * Mathf.PI * HueCircleRadius);
//            var position1 = GetPositionFromAngle(angle1);

//            for (var j = i + 1; j < NumGroups; j++)
//            {
//                //if (WedgeSizeScaled[j] < 0) continue;

//                var angle2 = WedgeAngles[j];
//                var radius2 = (WedgeRadius[j]) * ((2 * Mathf.PI * HueCircleRadius) / 360);
//                var position2 = GetPositionFromAngle(angle2);

//                var dist = Vector2.Distance(position1, position2);
//                var diff = dist - (radius1 + radius2) - GroupSpacing * ((2 * Mathf.PI * HueCircleRadius) / 360);
//                if (diff < 0)
//                {
//                    var dir = GetAngleDir(angle1, angle2);

//                    WedgeAngularVelocity[i] += Mathf.Abs(diff) * dir;
//                    WedgeAngularVelocity[j] += Mathf.Abs(diff) * -dir;
//                }
//            }
//        }

//        // Integration step
//        for (var i = 0; i < NumGroups; i++)
//        {
//            //if (WedgeSizeScaled[i] <= 0) continue;
//            {
//                WedgeAngularVelocity[i] *= Damping;
//                WedgeAngles[i] += WedgeAngularVelocity[i];
//            }
//        }

//        //**************************************//


//        //// Apply group forces
//        //if (ComputeGroupingForces)
//        //{
//        //    for (var i = 0; i < NumInstances; i++)
//        //    {
//        //        //if (WedgePositions[i].w < 0) continue;

//        //        var wedgeId = (int)Info2[i].x;
//        //        var position = (Vector2)Positions2[i];
//        //        var wedgeCentroid = GetPositionFromAngle(WedgeAngles[wedgeId]);
//        //        var diff = position - wedgeCentroid;

//        //        var forceDir = diff.normalized * -Mathf.Pow(diff.magnitude, GroupPowFactor) * GroupingForce;
//        //        //var forceDir = diff.normalized * -diff.magnitude;

//        //        Velocities2[i] += new Vector4(forceDir.x, forceDir.y, 0, 0);
//        //    }
//        //}

//        //// Apply group forces
//        //if (true)
//        //{
//        //    for (var i = 0; i < NumInstances; i++)
//        //    {
//        //        //if (WedgePositions[i].w < 0) continue;

//        //        var wedgeId = (int)Info2[i].x;
//        //        var position = (Vector2)Positions2[i];
//        //        var wedgeCentroid = GetPositionFromAngle(WedgeAngles[wedgeId]);
//        //        var diff = position - wedgeCentroid;

//        //        var forceDir = diff.normalized * -Mathf.Pow(diff.magnitude, GroupPowFactor) * GroupingForce;
//        //        //var forceDir = diff.normalized * -diff.magnitude;

//        //        Velocities2[i] += new Vector4(forceDir.x, forceDir.y, 0, 0);


//        //        Ray ray = new Ray(startingPoint, direction);
//        //        float distance = Vector3.Cross(ray.direction, point - ray.origin).magnitude;
//        //    }
//        //}


//        //// Apply cicle snapping forces
//        //if (ComputCircleSnappingForces)
//        //{
//        //    for (var i = 0; i < NumInstances; i++)
//        //    {
//        //        var position = (Vector2)Positions2[i];

//        //        for (int j = 0; j < NumGroups; j++)
//        //        {
//        //            var r = (WedgeSize[i] + WedgeRadius[i]) / 360 * (2 * Mathf.PI * HueCircleRadius);
//        //            var v = Mathf.PI * r * r;

//        //            var v2 = v / (150.0f / NumGroups);
//        //            var r2 = Mathf.Sqrt(v2 / Mathf.PI);
//        //            wedgeSpacing.Add(r2);
//        //        }
//        //    }
//        //}

//        //// Apply inter-nodes forces
//        //if (ComputeNodeDistancesForces)
//        //{
//        //    //var wedgeSpacing = new List<float>();
//        //    //for (int i = 0; i < NumGroups; i++)
//        //    //{
//        //    //    var r = (WedgeSize[i] + WedgeRadius[i]) / 360 * (2 * Mathf.PI * HueCircleRadius);
//        //    //    var v = Mathf.PI*r*r;

//        //    //    var v2 = v / (150.0f/NumGroups);
//        //    //    var r2 = Mathf.Sqrt(v2 / Mathf.PI);
//        //    //    wedgeSpacing.Add(r2);
//        //    //}


//        //    for (var i = 0; i < NumInstances; i++)
//        //    {
//        //        //if (WedgePositions[i].w < 0) continue;

//        //        var groupID = (int)Info2[i].x;

//        //        Vector2 fieldDir;
//        //        Vector2 deltaVelocity;

//        //        for (var j = 0; j < NumInstances; j++)
//        //        {
//        //            if (i == j) continue;

//        //            var radius = NodeAlienDistanceRadius * ((2 * Mathf.PI * HueCircleRadius) / 360); ;
//        //            if ((int)Info2[i].x == (int)Info2[j].x)
//        //            {
//        //                radius = NodeGroupDistanceRadius * ((2 * Mathf.PI * HueCircleRadius) / 360); ;
//        //                //radius = wedgeSpacing[(int)Info2[i].x];
//        //            }
//        //            else
//        //            {
//        //                continue;
//        //            }

//        //            fieldDir = (Vector2)Positions2[i] - (Vector2)Positions2[j];

//        //            var length = fieldDir.magnitude;
//        //            if (length >= radius)
//        //            {
//        //                continue;
//        //            }

//        //            if (length <= 0)
//        //            {
//        //                fieldDir = new Vector2(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
//        //                length = 0.1f;
//        //            }

//        //            // Normalize direction
//        //            fieldDir = fieldDir.normalized;

//        //            // If using linear falloff, scale with distance.
//        //            var fieldStrength = NodeDistanceForce * (1.0f - (length / radius));

//        //            // Accumulate forces
//        //            deltaVelocity = fieldDir * fieldStrength;
//        //            Velocities2[i] += (Vector4)(deltaVelocity);
//        //        }
//        //    }
//        //}

//        //// Intergrate forces
//        //for (var i = 0; i < NumInstances; i++)
//        //{
//        //    Velocities2[i] *= Damping;
//        //    Positions2[i] += Velocities2[i];
//        //}


//        //for (var i = 0; i < NumInstances; i++)
//        //{
//        //    if (WedgeInfo[i].y < 0) continue;

//        //    var angle = WedgeAngles[i];
//        //    var wedgeID = (int)WedgeInfo[i].x;

//        //    for (var j = 0; j < NumInstances; j++)
//        //    {
//        //        if (WedgeInfo[j].y < 0) continue;

//        //        var wedgeID2 = (int)WedgeInfo[j].x;
//        //        if (i == j || wedgeID == wedgeID2) continue;

//        //        var angle2 = WedgeAngles[j];

//        //        var delta2 = GetAngleDiff(angle, angle2);
//        //        var dir2 = GetAngleDir(angle, angle2);
//        //        var length2 = Mathf.Abs(delta2);

//        //        var 



//        //        if (length2 > NodeAlienDistanceRadius) continue;

//        //        var strength2 = NodeDistanceForce * (1.0f - (length2 / NodeAlienDistanceRadius));

//        //        // Accumulate forces
//        //        var deltaAngularVelocity2 = dir2 * strength2;
//        //        WedgeAngularVelocity[i] += deltaAngularVelocity2;
//        //    }
//        //}

//        /*********/

//        //for (var i = 0; i < NumInstances; i++)
//        //{
//        //    if (WedgeInfo[i].y < 0) continue;

//        //    var angle = WedgeAngles[i];
//        //    var wedgeID = (int)WedgeInfo[i].x;

//        //    var skipNode = false;

//        //    for (var j = 0; j < NumInstances; j++)
//        //    {
//        //        var angle2 = WedgeAngles[j];
//        //        var wedgeID2 = (int)WedgeInfo[j].x;

//        //        if (i != j && wedgeID == wedgeID2)
//        //        {
//        //            var angleDiff = GetAngleDiff(angle, angle2);
//        //            if (angleDiff > WedgeAngleMax)
//        //            {
//        //                skipNode = true;
//        //                break;
//        //            }
//        //        }
//        //    }

//        //    if(skipNode) continue;

//        //    var wedgeAngle = WedgeAngles[wedgeID];
//        //    var wedgeRadius = WedgeSize[wedgeID];

//        //    var delta = GetAngleDiff(angle, wedgeAngle);
//        //    var dir = GetAngleDir(angle, wedgeAngle);
//        //    var length = Mathf.Abs(delta);
//        //    if(length > wedgeRadius) continue;
//        //    var strength = GroupingForce * (1.0f - (length / wedgeRadius));

//        //    // Accumulate forces
//        //    var deltaAngularVelocity = dir * strength ;
//        //    WedgeAngularVelocity[i] += deltaAngularVelocity;
//        //}





//        //// Interagrate forces
//        //for (var i = 0; i < NumInstances; i++)
//        //{
//        //    if (GroupFlags[(int)WedgeInfo[i].x] && WedgePositions[i].w >= 0)
//        //    {
//        //        WedgePositions[i].w = -1;
//        //        WedgeVelocities[i] = Vector4.zero;
//        //    }

//        //    if (!GroupFlags[(int)WedgeInfo[i].x] && WedgePositions[i].w < 0)
//        //    {
//        //        WedgePositions[i].w = 1;
//        //    }

//        //    if (WedgePositions[i].w >= 0)
//        //    {
//        //        WedgeVelocities[i] *= Damping;
//        //        WedgePositions[i] += WedgeVelocities[i];
//        //    }
//        //    else
//        //    {
//        //        int a = 0;
//        //    }
//        //}

//        //// Apply cicle snapping forces
//        //if (ComputCircleSnappingForces)
//        //{
//        //    for (var i = 0; i < NumInstances; i++)
//        //    {
//        //        if(WedgePositions[i].w < 0) continue;

//        //        var position = (Vector2)WedgePositions[i];
//        //        var dist = position.magnitude - HueCircleRadius;

//        //        var forceDir = position.normalized * -dist * CircleSnappingForce;
//        //        WedgeVelocities[i] += new Vector4(forceDir.x, forceDir.y, 0, 0);
//        //    }
//        //}

//        //// Apply group forces
//        //if (ComputeGroupingForces)
//        //{
//        //    for (var i = 0; i < NumInstances; i++)
//        //    {
//        //        if (WedgePositions[i].w < 0) continue;

//        //        var position = (Vector2)WedgePositions[i];
//        //        var groupCentroid = (Vector2)GroupCentroids[(int)WedgeInfo[i].x];
//        //        var diff = position - groupCentroid;

//        //        var forceDir = diff.normalized * - Mathf.Pow(diff.magnitude, GroupPowFactor) * GroupingForce;
//        //        //var forceDir = diff.normalized * -diff.magnitude;

//        //        WedgeVelocities[i] += new Vector4(forceDir.x, forceDir.y, 0, 0);
//        //    }
//        //}

//        //// Apply inter-nodes forces
//        //if (ComputeNodeDistancesForces)
//        //{
//        //    for (var i = 0; i < NumInstances; i++)
//        //    {
//        //        if (WedgePositions[i].w < 0) continue;

//        //        var groupID = (int)WedgeInfo[i].x;

//        //        Vector2 fieldDir;
//        //        Vector2 deltaVelocity;

//        //        for (var j = 0; j < NumInstances; j++)
//        //        {
//        //            if (WedgePositions[j].w < 0 | i == j) continue;

//        //            var radius = NodeAlienDistanceRadius;
//        //            if ((int)WedgeInfo[i].x == (int)WedgeInfo[j].x)
//        //            {
//        //                radius = NodeGroupDistanceRadius;
//        //            }

//        //            fieldDir = (Vector2)WedgePositions[i] - (Vector2)WedgePositions[j];

//        //            var length = fieldDir.magnitude;
//        //            if (length >= radius)
//        //            {
//        //                continue;
//        //            }

//        //            if (length <= 0)
//        //            {
//        //                fieldDir = new Vector2(Random.Range(0.0f,1.0f), Random.Range(0.0f, 1.0f));
//        //                length = 0.1f;
//        //            }

//        //            // Normalize direction
//        //            fieldDir = fieldDir.normalized;

//        //            // If using linear falloff, scale with distance.
//        //            var fieldStrength = NodeDistanceForce * (1.0f - (length / radius));

//        //            // Accumulate forces
//        //            deltaVelocity = fieldDir * fieldStrength;
//        //            WedgeVelocities[i] += (Vector4)(deltaVelocity);
//        //        }
//        //    }
//        //}

//        //// Interagrate forces
//        //for (var i = 0; i < NumInstances; i++)
//        //{
//        //    if (GroupFlags[(int)WedgeInfo[i].x] && WedgePositions[i].w >= 0)
//        //    {
//        //        WedgePositions[i].w = -1;
//        //        WedgeVelocities[i] = Vector4.zero;
//        //    }

//        //    if (!GroupFlags[(int)WedgeInfo[i].x] && WedgePositions[i].w < 0)
//        //    {
//        //        WedgePositions[i].w = 1;
//        //    }

//        //    if (WedgePositions[i].w >= 0)
//        //    {
//        //        WedgeVelocities[i] *= Damping;
//        //        WedgePositions[i] += WedgeVelocities[i];
//        //    }
//        //    else
//        //    {
//        //        int a = 0;
//        //    }
//        //}

//        var pointCount = 0;

//        for (var i = 0; i < NumGroups; i++)
//        {
//            if (WedgeSizeScaled[i] <= 0) continue;

//            var wedgeLength = Mathf.Max(WedgeRadius[i]*2 - 15, 0);
//            var numPoints = Mathf.Max((WedgeSizeScaled[i] / (float)calcTotal) * NumInstancesMax, 1);
//            var angleInc = wedgeLength / numPoints;
            
//            for (int j = 0; j < numPoints; j++)
//            {
//                if (pointCount >= Positions2.Length)
//                {
//                    continue;
//                }

//                var aa = WedgeAngles[i] - WedgeRadius[i] + j*angleInc;
//                var angle = NormalizeAngled(aa) * Mathf.Deg2Rad;
//                Positions2[pointCount] = new Vector4(Mathf.Cos(angle) * HueCircleRadius, Mathf.Sin(angle) * HueCircleRadius, 0, 1);

//                pointCount++;
//            }
//        }

//        int a = 0;
//    }

//    void OnRenderImage(RenderTexture src, RenderTexture dst)
//    //void OnRenderObject()
//    {
//        ColorManager.Get.UseHCL = UseHCL;
//        ColorManager.Get.ShowAtoms = ShowAtoms;
//        ColorManager.Get.ShowChains = ShowChains;
//        ColorManager.Get.ShowResidues = ShowResidues;
//        ColorManager.Get.ShowSecondaryStructures = ShowSecondaryStructures;

//        ColorManager.Get.ChainDistance = ChainDistance;
//        ColorManager.Get.AtomDistance = AtomDistance;
//        ColorManager.Get.ResidueDistance = ResidueDistance;
//        ColorManager.Get.SecondaryStructureDistance = SecondaryStructureDistance;

//        InitSystem();
//        UpdatePositions();
//        ColorManager.Get.UpdateColors(GroupIndices, WedgeAngles, WedgeRadius);

//        Graphics.SetRenderTarget(src);

//        GL.Clear(true, true, new Color(0, 0, 0, 0));

//        InitSystem();

//        for (var i = 0; i < NumGroups; i++)
//        {
//            var angle = WedgeAngles[i] * Mathf.Deg2Rad;

//            WedgeInfo[i].z = (WedgeRadius[i]) / 360 * (2 * Mathf.PI * HueCircleRadius);
//            WedgePositions[i] = new Vector4(Mathf.Cos(angle) * HueCircleRadius, Mathf.Sin(angle) * HueCircleRadius, 0, 1);
//            //WedgePositions[i] = transform.TransformPoint(localPos);
//        }

//        /***********/

//        Mat.SetFloat("_HueCircleRadius", HueCircleRadius);
//        Mat.SetVector("_CameraUp", Camera.current.transform.up);
//        Mat.SetVector("_CameraRight", Camera.current.transform.right);

//        Mat.SetBuffer("_QuadUVs", QuadUVs);
//        Mat.SetBuffer("_QuadIndices", QuadIndices);
//        Mat.SetBuffer("_QuadVertices", QuadVertices);

//        /*********/

//        //Mat.SetBuffer("_WedgesInfo", CBWedgesInfo);
//        Mat.SetInt("_UseHCL", Convert.ToInt32(UseHCL));
//        Mat.SetBuffer("_NodesInfo", CBNodesInfo);
//        Mat.SetBuffer("_InstancePositions", CBPositions);

//        /*********/

//        //Debug.Log(GroupWedgesInfo[0]);
//        //CBWedgesInfo.SetData(GroupWedgesInfo.ToArray());

//        // Draw group constraints
//        //Mat.SetFloat("_GlyphRadius", GroupGlyphRadius);
//        //CBPositions.SetData(GroupCentroids.ToArray());
//        //Mat.SetPass(0);
//        //Graphics.DrawProcedural(MeshTopology.Triangles, QuadIndices.count, NumGroups);

//        //// Draw weges
//        //CBNodesInfo.SetData(WedgeInfo.ToArray());
//        //CBPositions.SetData(WedgePositions.ToArray());
//        //Mat.SetPass(1);
//        //Graphics.DrawProcedural(MeshTopology.Triangles, QuadIndices.count, NumGroups);

//        //// Draw ingredient nodes
//        //CBNodesInfo.SetData(Info2.ToArray());
//        //CBPositions.SetData(Positions2.ToArray());
//        //Mat.SetPass(2);
//        //Graphics.DrawProcedural(MeshTopology.Triangles, QuadIndices.count, NumInstances);



//        // Draw Debug nodes
//        //CBNodesInfo.SetData(Info2.ToArray());
//        CBPositions.SetData(Positions2.ToArray());
//        Mat.SetPass(0);
//        Graphics.DrawProcedural(MeshTopology.Triangles, QuadIndices.count, NumInstancesMax);

//        Graphics.Blit(src, dst);
//    }

//    void CreateResources()
//    {
//        if (QuadUVs == null)
//        {
//            var uvs = QuadMesh.uv;
//            QuadUVs = new ComputeBuffer(uvs.Length, sizeof(float) * 2);
//            QuadUVs.SetData(uvs);
//        }

//        if (QuadIndices == null)
//        {
//            var indices = QuadMesh.triangles;
//            QuadIndices = new ComputeBuffer(indices.Length, sizeof(float) * 1);
//            QuadIndices.SetData(indices);
//        }

//        if (QuadVertices == null)
//        {
//            var vertices = QuadMesh.vertices;
//            QuadVertices = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
//            QuadVertices.SetData(vertices);
//        }

//        if (CBWedgesInfo == null)
//        {
//            CBWedgesInfo = new ComputeBuffer(NumGroupsMax, sizeof(float) * 2);
//        }

//        if (CBNodesInfo == null)
//        {
//            CBNodesInfo = new ComputeBuffer(NumInstancesMax, sizeof(float) * 4);
//        }

//        if (CBPositions == null)
//        {
//            CBPositions = new ComputeBuffer(NumInstancesMax, sizeof(float) * 4);
//        }
//    }

//    // Flush buffers on exit
//    void OnDisable()
//    {
//        if (QuadUVs != null)
//        {
//            QuadUVs.Release();
//            QuadUVs = null;
//        }
//        if (QuadIndices != null)
//        {
//            QuadIndices.Release();
//            QuadIndices = null;
//        }

//        if (QuadVertices != null)
//        {
//            QuadVertices.Release();
//            QuadVertices = null;
//        }

//        if (CBWedgesInfo != null)
//        {
//            CBWedgesInfo.Release();
//            CBWedgesInfo = null;
//        }

//        if (CBNodesInfo != null)
//        {
//            CBNodesInfo.Release();
//            CBNodesInfo = null;
//        }

//        if (CBPositions != null)
//        {
//            CBPositions.Release();
//            CBPositions = null;
//        }
//    }
//}
