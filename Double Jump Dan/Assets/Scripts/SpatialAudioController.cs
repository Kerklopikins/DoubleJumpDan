using UnityEngine;

public class SpatialAudioController : MonoBehaviour
{
    [SerializeField] float minDistance;
    [SerializeField] float maxDistance;
    [SerializeField] bool showDistance;

    float maxVolume;
    AudioManager audioManager;
    Camera _camera;
    AudioSource audioSource;
    Vector3 difference;
    float distance;
    float t;
    float pitch = 1;

    void Start()
    {
        audioManager = AudioManager.Instance;    
        _camera = LevelManager.Instance.mainCamera;
        audioSource = GetComponent<AudioSource>();

        maxVolume = audioManager.sfxVolumePercent;
    }

    void Update()
    {
        if(maxVolume != audioManager.sfxVolumePercent)
            maxVolume = audioManager.sfxVolumePercent;
        
        audioSource.pitch = pitch * Time.timeScale;

        difference = new Vector3(_camera.transform.position.x - transform.position.x, _camera.transform.position.y - transform.position.y, 0);
        distance = difference.magnitude;
        
        t = Mathf.Clamp01((distance - minDistance) / (maxDistance - minDistance));
        audioSource.volume = Mathf.Lerp(maxVolume, 0, t);
    }
    
    public void SetPitch(float _pitch)
    {
        pitch = _pitch;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        
        if(showDistance)
        {
            Gizmos.DrawWireSphere(transform.position, minDistance);
            Gizmos.DrawWireSphere(transform.position, maxDistance);
        }
    }
}