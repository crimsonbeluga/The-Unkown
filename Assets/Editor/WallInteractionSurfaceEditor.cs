using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WallInteractionSurface))]
public class WallInteractionSurfaceEditor : Editor
{
    SerializedProperty interactions;
    SerializedProperty cornerBoxSize;
    SerializedProperty cornerBoxOffset;

    void OnEnable()
    {
        interactions = serializedObject.FindProperty("interactions");
        cornerBoxSize = serializedObject.FindProperty("cornerBoxSize");
        cornerBoxOffset = serializedObject.FindProperty("cornerBoxOffset");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Surface Interactions", EditorStyles.boldLabel);

        for (int i = 0; i < interactions.arraySize; i++)
        {
            SerializedProperty element = interactions.GetArrayElementAtIndex(i);
            SerializedProperty side = element.FindPropertyRelative("side");
            SerializedProperty isWalkable = element.FindPropertyRelative("isWalkable");
            SerializedProperty isClimbable = element.FindPropertyRelative("isClimbable");
            SerializedProperty isMantleable = element.FindPropertyRelative("isMantleable");
            SerializedProperty isWallJumpable = element.FindPropertyRelative("isWallJumpable");

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Interaction {i + 1}", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(side);
            EditorGUILayout.PropertyField(isWalkable);
            EditorGUILayout.PropertyField(isClimbable);
            EditorGUILayout.PropertyField(isMantleable);
            EditorGUILayout.PropertyField(isWallJumpable);

            if (isClimbable.boolValue && isWallJumpable.boolValue)
            {
                EditorGUILayout.HelpBox("This side cannot be both Climbable and WallJumpable.", MessageType.Warning);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Climbable Only")) isWallJumpable.boolValue = false;
                if (GUILayout.Button("WallJumpable Only")) isClimbable.boolValue = false;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Add New Interaction"))
        {
            interactions.arraySize++;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Mantle Corner Bounds", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(cornerBoxSize);
        EditorGUILayout.PropertyField(cornerBoxOffset);

        serializedObject.ApplyModifiedProperties();
    }
}





