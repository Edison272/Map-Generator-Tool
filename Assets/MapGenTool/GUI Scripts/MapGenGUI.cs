using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenScript))]
public class MapGenGUI : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Expand the 'Generation Presets' in order to adjust the settings for map generation");
        MapGenScript map_gui = (MapGenScript)target;
        if (GUILayout.Button(" Generate the Map! "))
        {
            map_gui.GenerateMap();
        }
        if (GUILayout.Button(" Save the Map! "))
        {
            map_gui.SaveMapFile();
        }
        DrawDefaultInspector();
        
    }
}