using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;

[CustomEditor(typeof(Gun))]
[CanEditMultipleObjects]
public class GunEditor : Editor
{
    Gun.ProjectileType projectileType;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Gun script = (Gun)target;
        
        EditorGUILayout.LabelField("Reloading", EditorStyles.boldLabel);
        script.reloadTime = EditorGUILayout.Slider("Reload Time", script.reloadTime, 0, 4);
        script.maxReloadAngle = EditorGUILayout.Slider("Max Reload Angle", script.maxReloadAngle, 0, 90);
        
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Audio Settings", EditorStyles.boldLabel);
        script.shootSound = (AudioClip)EditorGUILayout.ObjectField("Shoot Sound", script.shootSound, typeof(AudioClip), false);
        script.reloadSound = (AudioClip)EditorGUILayout.ObjectField("Reload Sound", script.reloadSound, typeof(AudioClip), false);
        
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Projectile Settings", EditorStyles.boldLabel);
        script.destroyedEffect = (GameObject)EditorGUILayout.ObjectField("Destroyed Effect", script.destroyedEffect, typeof(GameObject), false);
        script.projectileType = (Gun.ProjectileType)EditorGUILayout.EnumPopup("Projectile Type", script.projectileType);

        serializedObject.Update();
        SerializedProperty arrayProp = serializedObject.FindProperty("projectileFirePoints");
        EditorGUILayout.PropertyField(arrayProp, true);

        script.barrelLength = EditorGUILayout.FloatField("Barrel Length", script.barrelLength);
        
        if(script.projectileType == Gun.ProjectileType.RaycastBased)
        {
            SerializedProperty arrayProp2 = serializedObject.FindProperty("bullets");
            EditorGUILayout.PropertyField(arrayProp2, true);
        }

        serializedObject.ApplyModifiedProperties();
        
        if(script.projectileType == Gun.ProjectileType.GameObjectBased)
        {
            script.projectile = (GameObject)EditorGUILayout.ObjectField("Projectile", script.projectile, typeof(GameObject), false);

            script.speed = EditorGUILayout.Slider("Projectile Speed", script.speed, 0, 150);
            script.lifeTime = EditorGUILayout.Slider("Projectile Life Time", script.lifeTime, 0, 5);
            script.useRaycastEndPoint = EditorGUILayout.Toggle("Use Raycast End Point", script.useRaycastEndPoint);
            script.showTrajectory = EditorGUILayout.Toggle("Show Trajectory", script.showTrajectory);
            script.ricochet = EditorGUILayout.Toggle("Ricochet", script.ricochet);
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Recoil Settings", EditorStyles.boldLabel);
        script.kickMinMax = EditorGUILayout.Vector2Field("Kick Min-Max", script.kickMinMax);
        script.recoilMoveSettleTime = EditorGUILayout.Slider("Recoil Move Settle Time", script.recoilMoveSettleTime, 0, 1);
        
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Muzzle Flash Settings", EditorStyles.boldLabel);
        script.useMuzzleFlash = EditorGUILayout.Toggle("Use Muzzle Flash", script.useMuzzleFlash);
        
        if(script.useMuzzleFlash)
        {
            script.flash = (GameObject)EditorGUILayout.ObjectField("Muzzle Flash", script.flash, typeof(GameObject), true);
            script.flashTime = EditorGUILayout.Slider("Muzzle Flash Time", script.flashTime, 0, 0.5f);

            script.useSmokeParticles = EditorGUILayout.Toggle("Use Smoke Particle Effect", script.useSmokeParticles);

            if(script.useSmokeParticles)
                script.smoke = (ParticleSystem)EditorGUILayout.ObjectField("Smoke Particle System", script.smoke, typeof(ParticleSystem), true);
        }
        
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Shell Settings", EditorStyles.boldLabel);
        script.ejectShells = EditorGUILayout.Toggle("Eject Shells", script.ejectShells);

        if(script.ejectShells)
        {
            script.shell = (Transform)EditorGUILayout.ObjectField("Shell", script.shell, typeof(Transform), false);
            script.shellEjectionPoint = (Transform)EditorGUILayout.ObjectField("Shell Ejection Point", script.shellEjectionPoint, typeof(Transform), true);
            script.onlyEjectShellsWhenReloading = EditorGUILayout.Toggle("Only Eject Shells When Reloading", script.onlyEjectShellsWhenReloading);

        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Misc Settings", EditorStyles.boldLabel);
        script.gunAnimator = (Animator)EditorGUILayout.ObjectField("Gun Animator", script.gunAnimator, typeof(Animator), false);
        script.hasGlow = EditorGUILayout.Toggle("Has Glow", script.hasGlow);

        if(script.hasGlow)
            script.glowSprite = (SpriteRenderer)EditorGUILayout.ObjectField("Glow Sprite", script.glowSprite, typeof(SpriteRenderer), true);

        //DrawDefaultInspector();
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Camera Shake Settings", EditorStyles.boldLabel);
        SerializedProperty shakeProperties = serializedObject.FindProperty("properties");
        serializedObject.Update();
        EditorGUILayout.PropertyField(shakeProperties, true);

        EditorGUILayout.HelpBox("Animator Parameters:\ntrigger 'Shoot'", MessageType.Info);

        serializedObject.ApplyModifiedProperties();

        if(GUI.changed)
            EditorUtility.SetDirty(script);
    }
}