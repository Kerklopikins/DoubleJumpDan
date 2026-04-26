using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GunInfo))]
public class GunInfoEditor : Editor
{

    GunInfo.FireMode fireMode;
    GunInfo.FireRate _fireRate;

    public override void OnInspectorGUI()
    {
        GunInfo script = (GunInfo)target;

        script.fireMode = (GunInfo.FireMode)EditorGUILayout.EnumPopup("Fire Mode", script.fireMode);
        script._fireRate = (GunInfo.FireRate)EditorGUILayout.EnumPopup("Fire Rate", script._fireRate);
        script.damage = EditorGUILayout.IntSlider("Damage", script.damage, 0, 100);
        script.startingAmmo = EditorGUILayout.Slider("Starting Ammo", script.startingAmmo, 0, 100);
        script.startingAmmo = Mathf.RoundToInt(script.startingAmmo);
        
        if(script.fireMode == GunInfo.FireMode.Burst)
        {
            script.startingAmmo = script.burstsPerMagazine * script.shotsPerBurst;
            script.shotsPerBurst = EditorGUILayout.IntSlider("Shots Per Burst", script.shotsPerBurst, 1, 6);
            script.burstsPerMagazine = EditorGUILayout.IntSlider("Bursts Per Magazine", script.burstsPerMagazine, 1, 12);
            script.burstCoolDownTime = EditorGUILayout.Slider("Burst Cool Down Time", script.burstCoolDownTime, 0, 2);
        }
        
        script.aimPointOffset = EditorGUILayout.FloatField("Aim Point Offset", script.aimPointOffset);
        script.lowRumbleAmount = EditorGUILayout.Slider("Low Rumble Amount", script.lowRumbleAmount, 0, 1);
        script.highRumbleAmount = EditorGUILayout.Slider("High Rumble Amount", script.highRumbleAmount, 0, 1);
        script.rumbleDuration = EditorGUILayout.Slider("Rumble Duration", script.rumbleDuration, 0, 1);

        if(GUI.changed)
            EditorUtility.SetDirty(script);
    }
}