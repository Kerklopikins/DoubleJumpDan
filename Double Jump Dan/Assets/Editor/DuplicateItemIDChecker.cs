using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DuplicateItemIDChecker : EditorWindow
{
    ItemManager itemManager;
    List<Item> allItems = new List<Item>();
    List<string> conflitedIDs = new List<string>();
    bool foundDuplicate;
    bool checkedForDuplicates;

    [MenuItem("Window/Duplicate Item ID Checker")]
    public static void ShowWindow()
    {
        GetWindow(typeof(DuplicateItemIDChecker));
    }

    void Awake()
    {
        itemManager = GameObject.FindWithTag("Managers").GetComponent<ItemManager>();

        foreach(Item gun in itemManager.guns)
            allItems.Add(gun);

        foreach(Item hat in itemManager.hats)
            allItems.Add(hat);

        foreach(Item skin in itemManager.skins)
            allItems.Add(skin);

        foreach(Item upgrade in itemManager.upgrades)
            allItems.Add(upgrade);   
    }

    void OnGUI()
    {
        GUILayout.Label("Welcome to the Duplicate Item ID Checker!", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("All this tool does is check for any duplicate ID's in all items.\nStarting items with an ID of '1111' are not counted as they are default.", MessageType.Info);

        if(GUILayout.Button("Find Duplicates!"))
        {
            conflitedIDs.Clear();
            checkedForDuplicates = true;

            foreach(Item item in allItems)
            {
                for(int i = 0; i < allItems.Count; i++)
                {
                    if(item.name != allItems[i].name && item.itemID != 1111)
                    {
                        if(item.itemID == allItems[i].itemID)
                        {
                            conflitedIDs.Add(item.name + " " + item.itemID + " is same as " + allItems[i].name + " " + allItems[i].itemID);
                            foundDuplicate = true;
                        }
                    }
                }
            }

            if(conflitedIDs.Count == 0)
                foundDuplicate = false;
        }
        
        if(checkedForDuplicates)
        {
            if(foundDuplicate)
                EditorGUILayout.HelpBox(string.Join("\n", conflitedIDs), MessageType.Warning);
            else
                EditorGUILayout.HelpBox("No duplicates found!", MessageType.Info);
        }
    }
}