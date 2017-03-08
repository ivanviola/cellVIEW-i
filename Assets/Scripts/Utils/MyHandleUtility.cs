using UnityEngine;

public class MyHandleUtility
{

    public static Color xAxisColor = new Color(0.8588235f, 0.2431373f, 0.1137255f, 0.93f);
    public static Color yAxisColor = new Color(0.6039216f, 0.9529412f, 0.282353f, 0.93f);
    public static Color zAxisColor = new Color(0.227451f, 0.4784314f, 0.972549f, 0.93f);
    public static Color centerColor = new Color(0.8f, 0.8f, 0.8f, 0.93f);
    public static Color selectedColor = new Color(0.9647059f, 0.9490196f, 0.1960784f, 0.89f);

    //********//

    private static bool s_UseYSign = false;
    private static bool s_UseYSignZoom = false;

    private static Mesh _coneMesh;
    private static Mesh _cubeMesh;
    private static Mesh _sphereMesh;
    private static Mesh _cylinderMesh;
    private static Mesh _quadMesh;

    public static float acceleration
    {
        get
        {
            return (float)((!Event.current.shift ? 1.0 : 4.0) * (!Event.current.alt ? 1.0 : 0.25));
        }
    }

    public static Mesh CubeMesh
    {
        get
        {
            if (_cubeMesh == null) _cubeMesh = Resources.Load("Meshes/Cube") as Mesh;
            return _cubeMesh;
        }
    }

    public static Mesh ConeMesh
    {
        get
        {
            if (_coneMesh == null) _coneMesh = Resources.Load("Meshes/Cone") as Mesh;
            return _coneMesh;
        }
    }

    public static Mesh CylinderMesh
    {
        get
        {
            if (_cylinderMesh == null) _cylinderMesh = Resources.Load("Meshes/Cylinder") as Mesh;
            return _cylinderMesh;
        }
    }

    public static Mesh SphereMesh
    {
        get
        {
            if (_sphereMesh == null) _sphereMesh = Resources.Load("Meshes/Sphere") as Mesh;
            return _sphereMesh;
        }
    }

    //*******//

    private static Material _handleMaterial;
    private static Material _handleWireMaterial;

    public static Material HandleMaterial
    {
        get
        {
            if (!(bool)((Object)MyHandleUtility._handleMaterial))
                MyHandleUtility._handleMaterial = Resources.Load("Materials/HandleMaterial") as Material;
            return MyHandleUtility._handleMaterial;
        }
    }

    private static Material HandleWireMaterial
    {
        get
        {
            if (!(bool)((Object)MyHandleUtility._handleMaterial))
                MyHandleUtility._handleMaterial = Resources.Load("Materials/HandleMaterial") as Material;
            return MyHandleUtility._handleMaterial;
        }
    }

    internal static void ApplyLineMaterial()
    {
        HandleMaterial.SetPass(0);
    }

    internal static void ApplyShadedMaterial()
    {
        HandleWireMaterial.SetPass(1);
    }

    //*******//


    public static void DrawConeCap(Vector3 position, Quaternion rotation, float size, Color color)
    {
        Shader.SetGlobalColor("_HandleColor", color);
        ApplyShadedMaterial();
        Graphics.DrawMeshNow(ConeMesh, Matrix4x4.TRS(position, rotation, new Vector3(size, size, size)));
    }

    public static void DrawCubeCap(Vector3 position, Quaternion rotation, float size, Color color)
    {
        Shader.SetGlobalColor("_HandleColor", color);
        ApplyShadedMaterial();
        Graphics.DrawMeshNow(CubeMesh, Matrix4x4.TRS(position, rotation, new Vector3(size, size, size)));
    }

    public static void DrawLine(Vector3 p1, Vector3 p2, Color color)
    {
        Shader.SetGlobalColor("_HandleColor", color);

        ApplyLineMaterial();

        GL.PushMatrix();
        GL.Begin(1);
        GL.Vertex(p1);
        GL.Vertex(p2);
        GL.End();
        GL.PopMatrix();
    }

    public static void DrawPolyLine(Color color, params Vector3[] points)
    {
        Shader.SetGlobalColor("_HandleColor", color);
        ApplyLineMaterial();

        GL.PushMatrix();
        GL.Begin(1);
        for (int index = 1; index < points.Length; ++index)
        {
            GL.Vertex(points[index]);
            GL.Vertex(points[index - 1]);
        }
        GL.End();
        GL.PopMatrix();
    }


    public static void DrawWireArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius, Color color)
    {
        Vector3[] dest = new Vector3[60];
        SetDiscSectionPoints(dest, 60, center, normal, from, angle, radius);
        DrawPolyLine(color, dest);
    }

    public static void DrawWireDisc(Vector3 center, Vector3 normal, float radius, Color color)
    {
        Vector3 from = Vector3.Cross(normal, Vector3.up);
        if ((double)from.sqrMagnitude < 1.0 / 1000.0)
            from = Vector3.Cross(normal, Vector3.right);
        DrawWireArc(center, normal, from, 360f, radius, color);
    }

    //public static void DrawBoundingSphere(Vector3 position, Vector3 normal, float radius, Color color)
    //{
    //    Shader.SetGlobalColor("_HandleColor", Color.cyan);
    //    ApplyLineMaterial();

    //    DrawWireDisc(position, normal, radius, color);
    //    DrawWireDisc(position, normal, radius, color);
    //}

    public static void DrawBounds(Vector3 position, Quaternion rotation, Bounds bounds, Color color)
    {
        Shader.SetGlobalColor("_HandleColor", Color.cyan);
        ApplyLineMaterial();

        GL.Begin(GL.LINES);

        /* This section of the code makes sure that the box ajusts to the rotation of the object*/
        GL.Vertex3(position.x + bounds.size.x / 2, position.y + bounds.size.y / 2, position.z + bounds.size.z / 2);
        GL.Vertex3(position.x + bounds.size.x / 2 - bounds.size.x , position.y + bounds.size.y / 2, position.z + bounds.size.z / 2);

        GL.Vertex3(position.x + bounds.size.x / 2, position.y + bounds.size.y / 2, position.z + bounds.size.z / 2);
        GL.Vertex3(position.x + bounds.size.x / 2, position.y + bounds.size.y / 2 - bounds.size.y , position.z + bounds.size.z / 2);

        GL.Vertex3(position.x + bounds.size.x / 2, position.y + bounds.size.y / 2, position.z + bounds.size.z / 2);
        GL.Vertex3(position.x + bounds.size.x / 2, position.y + bounds.size.y / 2, position.z + bounds.size.z / 2 - bounds.size.z );
        
        GL.Vertex3(position.x - bounds.size.x / 2, position.y + bounds.size.y / 2, position.z + bounds.size.z / 2);
        GL.Vertex3(position.x - bounds.size.x / 2 + bounds.size.x , position.y + bounds.size.y / 2, position.z + bounds.size.z / 2);

        GL.Vertex3(position.x - bounds.size.x / 2, position.y + bounds.size.y / 2, position.z + bounds.size.z / 2);
        GL.Vertex3(position.x - bounds.size.x / 2, position.y + bounds.size.y / 2 - bounds.size.y , position.z + bounds.size.z / 2);

        GL.Vertex3(position.x - bounds.size.x / 2, position.y + bounds.size.y / 2, position.z + bounds.size.z / 2);
        GL.Vertex3(position.x - bounds.size.x / 2, position.y + bounds.size.y / 2, position.z + bounds.size.z / 2 - bounds.size.z );
        
        GL.Vertex3(position.x + bounds.size.x / 2, position.y - bounds.size.y / 2, position.z + bounds.size.z / 2);
        GL.Vertex3(position.x + bounds.size.x / 2 - bounds.size.x , position.y - bounds.size.y / 2, position.z + bounds.size.z / 2);

        GL.Vertex3(position.x + bounds.size.x / 2, position.y - bounds.size.y / 2, position.z + bounds.size.z / 2);
        GL.Vertex3(position.x + bounds.size.x / 2, position.y - bounds.size.y / 2 + bounds.size.y , position.z + bounds.size.z / 2);

        GL.Vertex3(position.x + bounds.size.x / 2, position.y - bounds.size.y / 2, position.z + bounds.size.z / 2);
        GL.Vertex3(position.x + bounds.size.x / 2, position.y - bounds.size.y / 2, position.z + bounds.size.z / 2 - bounds.size.z );
        
        GL.Vertex3(position.x - bounds.size.x / 2, position.y - bounds.size.y / 2, position.z + bounds.size.z / 2);
        GL.Vertex3(position.x - bounds.size.x / 2 + bounds.size.x , position.y - bounds.size.y / 2, position.z + bounds.size.z / 2);

        GL.Vertex3(position.x - bounds.size.x / 2, position.y - bounds.size.y / 2, position.z + bounds.size.z / 2);
        GL.Vertex3(position.x - bounds.size.x / 2, position.y - bounds.size.y / 2 + bounds.size.y , position.z + bounds.size.z / 2);

        GL.Vertex3(position.x - bounds.size.x / 2, position.y - bounds.size.y / 2, position.z + bounds.size.z / 2);
        GL.Vertex3(position.x - bounds.size.x / 2, position.y - bounds.size.y / 2, position.z + bounds.size.z / 2 - bounds.size.z );
        
        GL.Vertex3(position.x - bounds.size.x / 2, position.y - bounds.size.y / 2, position.z - bounds.size.z / 2);
        GL.Vertex3(position.x - bounds.size.x / 2 + bounds.size.x , position.y - bounds.size.y / 2, position.z - bounds.size.z / 2);

        GL.Vertex3(position.x - bounds.size.x / 2, position.y - bounds.size.y / 2, position.z - bounds.size.z / 2);
        GL.Vertex3(position.x - bounds.size.x / 2, position.y - bounds.size.y / 2 + bounds.size.y , position.z - bounds.size.z / 2);

        GL.Vertex3(position.x - bounds.size.x / 2, position.y - bounds.size.y / 2, position.z - bounds.size.z / 2);
        GL.Vertex3(position.x - bounds.size.x / 2, position.y - bounds.size.y / 2, position.z - bounds.size.z / 2 + bounds.size.z );
        
        GL.Vertex3(position.x - bounds.size.x / 2, position.y + bounds.size.y / 2, position.z - bounds.size.z / 2);
        GL.Vertex3(position.x - bounds.size.x / 2 + bounds.size.x , position.y + bounds.size.y / 2, position.z - bounds.size.z / 2);

        GL.Vertex3(position.x - bounds.size.x / 2, position.y + bounds.size.y / 2, position.z - bounds.size.z / 2);
        GL.Vertex3(position.x - bounds.size.x / 2, position.y + bounds.size.y / 2 - bounds.size.y , position.z - bounds.size.z / 2);

        GL.Vertex3(position.x - bounds.size.x / 2, position.y + bounds.size.y / 2, position.z - bounds.size.z / 2);
        GL.Vertex3(position.x - bounds.size.x / 2, position.y + bounds.size.y / 2, position.z - bounds.size.z / 2 + bounds.size.z );
        
        GL.Vertex3(position.x + bounds.size.x / 2, position.y + bounds.size.y / 2, position.z - bounds.size.z / 2);
        GL.Vertex3(position.x + bounds.size.x / 2 - bounds.size.x , position.y + bounds.size.y / 2, position.z - bounds.size.z / 2);

        GL.Vertex3(position.x + bounds.size.x / 2, position.y + bounds.size.y / 2, position.z - bounds.size.z / 2);
        GL.Vertex3(position.x + bounds.size.x / 2, position.y + bounds.size.y / 2 - bounds.size.y , position.z - bounds.size.z / 2);

        GL.Vertex3(position.x + bounds.size.x / 2, position.y + bounds.size.y / 2, position.z - bounds.size.z / 2);
        GL.Vertex3(position.x + bounds.size.x / 2, position.y + bounds.size.y / 2, position.z - bounds.size.z / 2 + bounds.size.z );

        GL.Vertex3(position.x + bounds.size.x / 2, position.y - bounds.size.y / 2, position.z - bounds.size.z / 2);
        GL.Vertex3(position.x + bounds.size.x / 2 - bounds.size.x , position.y - bounds.size.y / 2, position.z - bounds.size.z / 2);

        GL.Vertex3(position.x + bounds.size.x / 2, position.y - bounds.size.y / 2, position.z - bounds.size.z / 2);
        GL.Vertex3(position.x + bounds.size.x / 2, position.y - bounds.size.y / 2 + bounds.size.y , position.z - bounds.size.z / 2);

        GL.Vertex3(position.x + bounds.size.x / 2, position.y - bounds.size.y / 2, position.z - bounds.size.z / 2);
        GL.Vertex3(position.x + bounds.size.x / 2, position.y - bounds.size.y / 2, position.z - bounds.size.z / 2 + bounds.size.z );
        GL.End();
    }

    //*******//

    public static float GetHandleSize(Vector3 position)
    {
        Camera main = Camera.main;
       
        Transform transform = main.transform;
        Vector3 position1 = transform.position;
        float z = Vector3.Dot(position - position1, transform.TransformDirection(new Vector3(0.0f, 0.0f, 1f)));
        return 80f / Mathf.Max((main.WorldToScreenPoint(position1 + transform.TransformDirection(new Vector3(0.0f, 0.0f, z))) - main.WorldToScreenPoint(position1 + transform.TransformDirection(new Vector3(1f, 0.0f, z)))).magnitude, 0.0001f);
    }

    public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 rhs = point - lineStart;
        Vector3 vector3 = lineEnd - lineStart;
        float magnitude = vector3.magnitude;
        Vector3 lhs = vector3;
        if ((double)magnitude > 9.99999997475243E-07)
            lhs /= magnitude;
        float num = Mathf.Clamp(Vector3.Dot(lhs, rhs), 0.0f, magnitude);
        return lineStart + lhs * num;
    }

    public static Vector2 WorldToGUIPoint(Vector3 world)
    {
        var absolutePos = (Vector2)Camera.main.WorldToScreenPoint(world);
        absolutePos.y = (float)Screen.height - absolutePos.y;
        return (absolutePos);
        //return GUIClip.Clip(absolutePos);
    }

    public static void DebugD(Vector3 p1)
    {
        Debug.Log(MyHandleUtility.WorldToGUIPoint(p1));
        Debug.Log((Vector3)Event.current.mousePosition);
        Debug.Log("***");
    }

    public static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        return Vector3.Magnitude(MyHandleUtility.ProjectPointLine(point, lineStart, lineEnd) - point);
    }

    public static float DistanceToLine(Vector3 p1, Vector3 p2)
    {
        p1 = (Vector3)MyHandleUtility.WorldToGUIPoint(p1);
        p2 = (Vector3)MyHandleUtility.WorldToGUIPoint(p2);
        float num = MyHandleUtility.DistancePointLine((Vector3)Event.current.mousePosition, p1, p2);
        if ((double)num < 0.0)
            num = 0.0f;
        return num;
    }

    public static float DistanceToCircle(Vector3 position, float radius)
    {
        Vector2 vector2_1 = MyHandleUtility.WorldToGUIPoint(position);
        Camera current = Camera.current;
        Vector2 zero = Vector2.zero;
        if ((bool)((Object)current))
        {
            Vector2 vector2_2 = MyHandleUtility.WorldToGUIPoint(position + current.transform.right * radius);
            radius = (vector2_1 - vector2_2).magnitude;
        }
        float magnitude = (vector2_1 - Event.current.mousePosition).magnitude;
        if ((double)magnitude < (double)radius)
            return 0.0f;
        return magnitude - radius;
    }

    public static float DistanceToPolyLine(params Vector3[] points)
    {
        float num1 = DistanceToLine(points[0], points[1]);
        for (int index = 2; index < points.Length; ++index)
        {
            float num2 = DistanceToLine(points[index - 1], points[index]);
            if ((double)num2 < (double)num1)
                num1 = num2;
        }
        return num1;
    }

    public static float DistanceToArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius)
    {
        Vector3[] dest = new Vector3[60];
        MyHandleUtility.SetDiscSectionPoints(dest, 60, center, normal, from, angle, radius);
        return DistanceToPolyLine(dest);
    }

    public static float DistanceToDisc(Vector3 center, Vector3 normal, float radius)
    {
        Vector3 from = Vector3.Cross(normal, Vector3.up);
        if ((double)from.sqrMagnitude < 1.0 / 1000.0)
            from = Vector3.Cross(normal, Vector3.right);
        return DistanceToArc(center, normal, from, 360f, radius);
    }

    internal static void SetDiscSectionPoints(Vector3[] dest, int count, Vector3 center, Vector3 normal, Vector3 from, float angle, float radius)
    {
        from.Normalize();
        Quaternion quaternion = Quaternion.AngleAxis(angle / (float)(count - 1), normal);
        Vector3 vector3 = from * radius;
        for (int index = 0; index < count; ++index)
        {
            dest[index] = center + vector3;
            vector3 = quaternion * vector3;
        }
    }

    public static Vector3 ClosestPointToArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius)
    {
        Vector3[] dest = new Vector3[60];
        SetDiscSectionPoints(dest, 60, center, normal, from, angle, radius);
        return ClosestPointToPolyLine(dest);
    }

    public static Vector3 ClosestPointToDisc(Vector3 center, Vector3 normal, float radius)
    {
        Vector3 from = Vector3.Cross(normal, Vector3.up);
        if ((double)from.sqrMagnitude < 1.0 / 1000.0)
            from = Vector3.Cross(normal, Vector3.right);
        return ClosestPointToArc(center, normal, from, 360f, radius);
    }

    public static Vector3 ClosestPointToPolyLine(params Vector3[] vertices)
    {
        float num1 = DistanceToLine(vertices[0], vertices[1]);
        int index1 = 0;
        for (int index2 = 2; index2 < vertices.Length; ++index2)
        {
            float num2 = DistanceToLine(vertices[index2 - 1], vertices[index2]);
            if ((double)num2 < (double)num1)
            {
                num1 = num2;
                index1 = index2 - 1;
            }
        }
        Vector3 vector3_1 = vertices[index1];
        Vector3 vector3_2 = vertices[index1 + 1];
        Vector2 vector2_1 = Event.current.mousePosition - MyHandleUtility.WorldToGUIPoint(vector3_1);
        Vector2 vector2_2 = MyHandleUtility.WorldToGUIPoint(vector3_2) - MyHandleUtility.WorldToGUIPoint(vector3_1);
        float magnitude = vector2_2.magnitude;
        float num3 = Vector3.Dot((Vector3)vector2_2, (Vector3)vector2_1);
        if ((double)magnitude > 9.99999997475243E-07)
            num3 /= magnitude * magnitude;
        float t = Mathf.Clamp01(num3);
        return Vector3.Lerp(vector3_1, vector3_2, t);
    }

    internal static float GetParametrization(Vector2 x0, Vector2 x1, Vector2 x2)
    {
        return (float)-((double)Vector2.Dot(x1 - x0, x2 - x1) / (double)(x2 - x1).sqrMagnitude);
    }

    public static float CalcLineTranslation(Vector2 src, Vector2 dest, Vector3 srcPosition, Vector3 constraintDir)
    {
        float num = 1f;
        Vector3 forward = Camera.main.transform.forward;
        if ((double)Vector3.Dot(constraintDir, forward) < 0.0)
            num = -1f;
        Vector3 vector3 = constraintDir;
        vector3.y = -vector3.y;
        Camera current = Camera.main;
        Vector2 x1 = (Vector2)current.WorldToScreenPoint(srcPosition);
        Vector2 x2 = (Vector2)current.WorldToScreenPoint(srcPosition + constraintDir * num);
        Vector2 x0_1 = dest;
        Vector2 x0_2 = src;
        if (x1 == x2)
            return 0.0f;
        x0_1.y = -x0_1.y;
        x0_2.y = -x0_2.y;
        float parametrization = GetParametrization(x0_2, x1, x2);
        return (GetParametrization(x0_1, x1, x2) - parametrization) * num;
    }

    public static float niceMouseDelta
    {
        get
        {
            Vector2 delta = Event.current.delta;
            delta.y = -delta.y;
            if ((double)Mathf.Abs(Mathf.Abs(delta.x) - Mathf.Abs(delta.y)) / (double)Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y)) > 0.100000001490116)
                s_UseYSign = (double)Mathf.Abs(delta.x) <= (double)Mathf.Abs(delta.y);
            if (s_UseYSign)
                return Mathf.Sign(delta.y) * delta.magnitude * acceleration;
            return Mathf.Sign(delta.x) * delta.magnitude * acceleration;
        }
    }

    public static float niceMouseDeltaZoom
    {
        get
        {
            Vector2 vector2 = -Event.current.delta;
            if ((double)Mathf.Abs(Mathf.Abs(vector2.x) - Mathf.Abs(vector2.y)) / (double)Mathf.Max(Mathf.Abs(vector2.x), Mathf.Abs(vector2.y)) > 0.100000001490116)
                s_UseYSignZoom = (double)Mathf.Abs(vector2.x) <= (double)Mathf.Abs(vector2.y);
            if (s_UseYSignZoom)
                return Mathf.Sign(vector2.y) * vector2.magnitude * acceleration;
            return Mathf.Sign(vector2.x) * vector2.magnitude * acceleration;
        }
    }


    public static void DrawWireMesh(Mesh mesh, Transform transform, Color color)
    {
        Shader.SetGlobalColor("_HandleColor", color);
        Shader.SetGlobalInt("_EnableShading", 0);

        HandleMaterial.SetPass(2);

        GL.wireframe = true;
        Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix);
        GL.wireframe = false;

        HandleMaterial.SetPass(3);

        GL.wireframe = true;
        Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix);
        GL.wireframe = false;

        //handleMaterial.SetPass(1);

        //GL.wireframe = true;
        //Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix);
        //GL.wireframe = false;
    }

    public static float SnapValue(float val, float snap)
    {
        if ((Event.current.keyCode == KeyCode.LeftControl) && (double)snap > 0.0)
            return Mathf.Round(val / snap) * snap;
        return val;
    }

    public static float GetMouseDelta()
    {
        Vector2 delta = Event.current.delta;
        delta.y = -delta.y;
        if ((double)Mathf.Abs(Mathf.Abs(delta.x) - Mathf.Abs(delta.y)) /
            (double)Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y)) > 0.100000001490116)
        {
            if ((double)Mathf.Abs(delta.x) <= (double)Mathf.Abs(delta.y))
            {
                return Mathf.Sign(delta.y) * delta.magnitude * 1;
            }
        }
        return Mathf.Sign(delta.x) * delta.magnitude * 1;
    }

    public static void DrawTwoShadedWireDisc(Vector3 position, Vector3 axis, Vector3 from, float degrees, float radius, Color color)
    {
        DrawWireArc(position, axis, from, degrees, radius, color);
        //DrawWireArc(position, axis, from, degrees - 360f, radius, color);
    }

    internal static void DrawTwoShadedWireDisc(Vector3 position, Vector3 axis, float radius, Color color)
    {
        DrawWireDisc(position, axis, radius, color);
    }

    public static void DoRadiusHandle(Quaternion rotation, Vector3 position, float radius, Color color)
    {
        float num = 90f;
        Vector3[] array = new Vector3[]
        {
                rotation * Vector3.right,
                rotation * Vector3.up,
                rotation * Vector3.forward,
                rotation * -Vector3.right,
                rotation * -Vector3.up,
                rotation * -Vector3.forward
        };
        Vector3 vector;
        if (Camera.current.orthographic)
        {
            
        }
        else
        {
            vector = position - Camera.current.transform.position;
            float sqrMagnitude = vector.sqrMagnitude;
            float num2 = radius * radius;
            float num3 = num2 * num2 / sqrMagnitude;
            float num4 = num3 / num2;
            if (num4 < 1f)
            {
                float num5 = Mathf.Sqrt(num2 - num3);
                num = Mathf.Atan2(num5, Mathf.Sqrt(num3)) * 57.29578f;
                DrawWireDisc(position - num2 * vector / sqrMagnitude, vector, num5, color);
            }
            else
            {
                num = -1000f;
            }

            if (true)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (num4 < 1f)
                    {
                        float num6 = Vector3.Angle(vector, array[j]);
                        num6 = 90f - Mathf.Min(num6, 180f - num6);
                        float num7 = Mathf.Tan(num6 * 0.0174532924f);
                        float num8 = Mathf.Sqrt(num3 + num7 * num7 * num3) / radius;
                        if (num8 < 1f)
                        {
                            float num9 = Mathf.Asin(num8) * 57.29578f;
                            Vector3 vector2 = Vector3.Cross(array[j], vector).normalized;
                            vector2 = Quaternion.AngleAxis(num9, array[j]) * vector2;
                            DrawTwoShadedWireDisc(position, array[j], vector2, (90f - num9) * 2f, radius, color);
                        }
                        else
                        {
                            DrawTwoShadedWireDisc(position, array[j], radius, color);
                        }
                    }
                    else
                    {
                        DrawTwoShadedWireDisc(position, array[j], radius, color);
                    }
                }
            }
        }
        //Color color = Handles.color;
        //for (int k = 0; k < 6; k++)
        //{
        //    int controlID = GUIUtility.GetControlID(Handles.s_RadiusHandleHash, FocusType.Keyboard);
        //    float num10 = Vector3.Angle(array[k], -vector);
        //    if ((num10 > 5f && num10 < 175f) || GUIUtility.hotControl == controlID)
        //    {
        //        Color color2 = color;
        //        if (num10 > num + 5f)
        //        {
        //            color2.a = Mathf.Clamp01(Handles.backfaceAlphaMultiplier * color.a * 2f);
        //        }
        //        else
        //        {
        //            color2.a = Mathf.Clamp01(color.a * 2f);
        //        }
        //        Handles.color = color2;
        //        Vector3 vector3 = position + radius * array[k];
        //        bool changed = GUI.changed;
        //        GUI.changed = false;
        //        vector3 = Slider1D.Do(controlID, vector3, array[k], HandleUtility.GetHandleSize(vector3) * 0.03f, new Handles.DrawCapFunction(Handles.DotCap), 0f);
        //        if (GUI.changed)
        //        {
        //            radius = Vector3.Distance(vector3, position);
        //        }
        //        GUI.changed |= changed;
        //    }
        //}
        //Handles.color = color;
        //return radius;
    }
}
