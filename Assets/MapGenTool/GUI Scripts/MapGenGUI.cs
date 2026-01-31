using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenScript))]
public class MapGenGUI : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Expand the 'Generation Presets' in order to adjust the settings for map generation");
        DrawDefaultInspector();
        MapGenScript map_gui = (MapGenScript)target;
        if (GUILayout.Button(" Generate the Map! "))
        {
            map_gui.GenerateMap();
        }
    }
}