using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class MagicalTiledMapFixer : EditorWindow
{
    GameObject grid;
    TilemapRenderer background;
    TilemapRenderer otherBackground;
    TilemapRenderer topTiles;
    TilemapRenderer paths;
    TilemapRenderer other;
    TilemapRenderer spikes;
    TilemapRenderer dirt;
    bool notInNormalLevel;

    [MenuItem("Window/Magical Tiled Map Fixer")]
    public static void ShowWindow()
    {
        GetWindow(typeof(MagicalTiledMapFixer));
    }

    void Awake()
    {
        if(SceneManager.GetActiveScene().name == "Main Menu" || SceneManager.GetActiveScene().name == "Splash Screen")
        {
            notInNormalLevel = true;
            return;
        }

        grid = GameObject.Find("Grid");
        background = grid.transform.Find("Background").GetComponent<TilemapRenderer>();
        otherBackground = grid.transform.Find("Other Background").GetComponent<TilemapRenderer>();
        topTiles = grid.transform.Find("Top Tiles").GetComponent<TilemapRenderer>();
        paths = grid.transform.Find("Paths").GetComponent<TilemapRenderer>();
        other = grid.transform.Find("Other").GetComponent<TilemapRenderer>();
        spikes = grid.transform.Find("Spikes").GetComponent<TilemapRenderer>();
        dirt = grid.transform.Find("out").GetComponent<TilemapRenderer>();
    }

    void OnGUI()
    {
        GUILayout.Label("Welcome to the Magical Tiled Map Fixer tool!", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("All this tool does is set up the map with the correct properties.", MessageType.Info);

        if(notInNormalLevel)
        {
            EditorGUILayout.HelpBox("Go to an actual level please.", MessageType.Warning);
            return;
        }

        if(GUILayout.Button("Fix Tiled Map!"))
        {
            Material mainMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Main Material.mat");
            
            background.sortingLayerName = "Behind Foreground";
            background.sortingOrder = -2;
            background.material = mainMaterial;

            otherBackground.sortingLayerName = "Behind Foreground";
            otherBackground.sortingOrder = -1;
            otherBackground.material = mainMaterial;

            topTiles.sortingLayerName = "Behind Foreground";
            topTiles.sortingOrder = 0;
            topTiles.material = mainMaterial;

            paths.sortingLayerName = "Behind Foreground";
            paths.sortingOrder = 1;
            paths.material = mainMaterial;

            spikes.sortingLayerName = "Foreground";
            spikes.sortingOrder = -1;
            spikes.gameObject.layer = LayerMask.NameToLayer("Collisions");
            spikes.material = mainMaterial; 
            spikes.transform.GetChild(0).GetComponent<PolygonCollider2D>().isTrigger = true;
            spikes.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Collisions");

            if(spikes.transform.GetChild(0).GetComponent<GiveDamage>() == null)
            {
                spikes.transform.GetChild(0).gameObject.AddComponent<GiveDamage>();
                spikes.transform.GetChild(0).gameObject.GetComponent<GiveDamage>().instantKill = true;
            }
        
            other.sortingLayerName = "Foreground";
            other.sortingOrder = 0;
            other.gameObject.layer = LayerMask.NameToLayer("Collisions");
            other.material = mainMaterial;
            other.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Collisions");

            dirt.sortingLayerName = "Foreground";
            dirt.sortingOrder = 0;
            dirt.gameObject.layer = LayerMask.NameToLayer("Collisions");
            dirt.material = mainMaterial;
            dirt.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Collisions");

            EditorUtility.SetDirty(background);
            EditorUtility.SetDirty(otherBackground);
            EditorUtility.SetDirty(topTiles);
            EditorUtility.SetDirty(paths);
            EditorUtility.SetDirty(spikes);
            EditorUtility.SetDirty(other);
            EditorUtility.SetDirty(dirt);

            EditorSceneManager.MarkSceneDirty(background.gameObject.scene);
            EditorSceneManager.MarkSceneDirty(otherBackground.gameObject.scene);
            EditorSceneManager.MarkSceneDirty(topTiles.gameObject.scene);
            EditorSceneManager.MarkSceneDirty(paths.gameObject.scene);
            EditorSceneManager.MarkSceneDirty(spikes.gameObject.scene);
            EditorSceneManager.MarkSceneDirty(other.gameObject.scene);
            EditorSceneManager.MarkSceneDirty(dirt.gameObject.scene);
        }
        
        GUILayout.Space(25);
        GUILayout.Label("Background - Sort Layer and order: 'Behind Foreground', -2", EditorStyles.label);
        GUILayout.Label("Other Background - Sort Layer and order: 'Behind Foreground', -1", EditorStyles.label);
        GUILayout.Label("Top Tiles - Sort Layer and order: 'Behind Foreground', 0", EditorStyles.label);
        GUILayout.Label("Paths - Sort Layer and order: 'Behind Foreground', 1", EditorStyles.label);
        GUILayout.Label("Spikes - Sort Layer and order: 'Foreground', -1", EditorStyles.label);
        GUILayout.Label("Other - Sort Layer and order: 'Foreground', 0", EditorStyles.label);
        GUILayout.Label("out (aka Dirt) - Sort Layer and order: 'Foreground', 0", EditorStyles.label);
    }
}