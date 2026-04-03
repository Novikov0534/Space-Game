using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;

public class OverlapHighlighterWindow : EditorWindow
{
    private static bool highlightEnabled = false;

    [MenuItem("Tools/Overlap Highlighter")]
    public static void ShowWindow()
    {
        GetWindow<OverlapHighlighterWindow>("Overlap Highlighter");
    }

    private void OnEnable()
    {
        if (highlightEnabled)
            SceneView.duringSceneGui += HighlightOverlaps;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= HighlightOverlaps;
    }

    private void OnGUI()
    {
        GUILayout.Label("Overlap Highlighter", EditorStyles.boldLabel);

        bool newValue = GUILayout.Toggle(highlightEnabled, "Enable Highlight");
        if (newValue != highlightEnabled)
        {
            highlightEnabled = newValue;

            if (highlightEnabled)
                SceneView.duringSceneGui += HighlightOverlaps;
            else
                SceneView.duringSceneGui -= HighlightOverlaps;

            SceneView.RepaintAll();
        }
    }

    private void HighlightOverlaps(SceneView sceneView)
    {
        Collider2D[] allColliders;

        // Проверяем, редактируем ли мы префаб
        PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            allColliders = prefabStage.prefabContentsRoot.GetComponentsInChildren<Collider2D>();
        }
        else
        {
            allColliders = GameObject.FindObjectsOfType<Collider2D>();
        }

        Color highlightColor = new Color(1f, 0f, 0f, 0.3f);
        float minOverlapArea = 0.1f;

        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

        for (int i = 0; i < allColliders.Length; i++)
        {
            Collider2D colA = allColliders[i];
            if (colA == null) continue;

            for (int j = i + 1; j < allColliders.Length; j++)
            {
                Collider2D colB = allColliders[j];
                if (colB == null) continue;

                if (colA.bounds.Intersects(colB.bounds))
                {
                    float overlapX = Mathf.Min(colA.bounds.max.x, colB.bounds.max.x) - Mathf.Max(colA.bounds.min.x, colB.bounds.min.x);
                    float overlapY = Mathf.Min(colA.bounds.max.y, colB.bounds.max.y) - Mathf.Max(colA.bounds.min.y, colB.bounds.min.y);
                    float overlapArea = overlapX * overlapY;

                    if (overlapArea >= minOverlapArea)
                    {
                        DrawBounds(colA.bounds, highlightColor);
                        DrawBounds(colB.bounds, highlightColor);
                    }
                }
            }
        }
    }

    private void DrawBounds(Bounds bounds, Color color)
    {
        Vector3[] verts = new Vector3[]
        {
            new Vector3(bounds.min.x, bounds.min.y, 0),
            new Vector3(bounds.max.x, bounds.min.y, 0),
            new Vector3(bounds.max.x, bounds.max.y, 0),
            new Vector3(bounds.min.x, bounds.max.y, 0)
        };
        Handles.DrawSolidRectangleWithOutline(verts, color, Color.red);
    }
}
