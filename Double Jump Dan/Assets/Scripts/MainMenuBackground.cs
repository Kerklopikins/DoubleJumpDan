using UnityEngine;

public class MainMenuBackground : MonoBehaviour
{
    [SerializeField] MeshRenderer groundPlane;
    [SerializeField] MeshRenderer mountains;
    [SerializeField] MeshRenderer mountainsShadow;
    [SerializeField] Transform mountainsShadowParent;
    [SerializeField] float[] shadowScaling;

    LocalWorldManager localWorldManager;
    WorldManager worldManager;

    void Start()
    {
        localWorldManager = GameObject.FindWithTag("Level Managers").GetComponent<LocalWorldManager>();
        worldManager = WorldManager.Instance;
        worldManager.OnTimeOfDayChanged += OnTimeOfDayChanged;
    }

    void OnTimeOfDayChanged()
    {
        mountains.material.color = worldManager.mainMaterial.color;
        groundPlane.material.color = worldManager.mainMaterial.color;

        switch(localWorldManager.timeOfDay)
        {   
            case LocalWorldManager.TimeOfDay.Day:
                mountainsShadow.gameObject.SetActive(true);
                mountainsShadowParent.transform.localScale = new Vector3(mountainsShadowParent.transform.localScale.x, mountainsShadowParent.transform.localScale.y, shadowScaling[0]);
                break;
            case LocalWorldManager.TimeOfDay.Sunset:
                mountainsShadow.gameObject.SetActive(true);
                mountainsShadowParent.transform.localScale = new Vector3(mountainsShadowParent.transform.localScale.x, mountainsShadowParent.transform.localScale.y, shadowScaling[1]);
                break;
            case LocalWorldManager.TimeOfDay.Night:
                mountainsShadow.gameObject.SetActive(true);
                mountainsShadowParent.transform.localScale = new Vector3(mountainsShadowParent.transform.localScale.x, mountainsShadowParent.transform.localScale.y, shadowScaling[0]);
                break;
            case LocalWorldManager.TimeOfDay.Sunrise:
                mountainsShadow.gameObject.SetActive(false);
                break;
        }
    }
}