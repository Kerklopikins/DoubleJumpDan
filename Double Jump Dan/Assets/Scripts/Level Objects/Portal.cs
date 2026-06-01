using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Portal : MonoBehaviour
{
    [Header("Main Stuff")]
    [SerializeField] Vector3 transportPoint;
    [SerializeField] AudioClip portalSound;
    [SerializeField] float flashSpeed;
    [SerializeField] GameObject teleportedEffect;
    [SerializeField] SpriteRenderer glow;

    [Header("Camera Shake")]
    [SerializeField] CameraManager.Properties properties;

    float fadeSpeed = 0.5f;
    float maxDetectionHeight = 5.5f;
    float yOffset = 2.5f;
    float xDistance = 1;
    bool inPortal;
    Player player;
    Rigidbody2D rb2D;
	bool initiated;
	Camera _camera;
    CameraManager cameraManager;
    GameInputManager gameInputManager;

    void Start()
    {
		_camera = LevelManager.Instance.mainCamera;
        cameraManager = _camera.GetComponent<CameraManager>();
        player = LevelManager.Instance.player;
        rb2D = player.GetComponent<Rigidbody2D>();
        gameInputManager = GameInputManager.Instance;
    }

    void Update()
    {
        glow.color = new Color(glow.color.r, glow.color.g, glow.color.b, Mathf.PingPong(Time.time * flashSpeed, 1));
        
        if(ScreenEffectsManager.Instance.WhiteFadedIn() && ScreenEffectsManager.Instance.CanAnimateWhiteFade() && initiated)
        {
            initiated = false;
            player.Teleported(transportPoint);
            ScreenEffectsManager.Instance.AnimateWhiteFade(1, 0, fadeSpeed);
            CameraManager.Instance.SnapToPlayerPosition();
        }

        if(inPortal && initiated)
        {
            player.transform.position = new Vector3(transform.position.x, transform.position.y - 1.375f, 0);
            rb2D.velocity = Vector3.zero;
        }

        float playerXPositionAbs = Mathf.Abs(transform.position.x - player.transform.position.x);

        if(playerXPositionAbs < xDistance && player.transform.position.y > transform.position.y - yOffset)
        {
            if(player.transform.position.y > transform.position.y - yOffset + maxDetectionHeight)
                inPortal = false;
            else
                inPortal = true;

            if(!player.CanHandleInput())
                return;

            if(gameInputManager.GetVerticalInput() > gameInputManager.VerticalInputSensitivity && inPortal && !initiated)
                Teleport();
        }
        else
        {
            inPortal = false;
        }		
    }
    
    void Teleport()
    {
        initiated = true;
        player.Teleporting();

        Instantiate(teleportedEffect, transportPoint, Quaternion.identity);
        AudioManager.Instance.PlaySound2D(portalSound);

        ScreenEffectsManager.Instance.AnimateWhiteFade(0, 1, fadeSpeed);
        
        if(properties.strength > 0)
            CameraManager.Instance.Shake(properties);

        cameraManager.SnapCamera(cameraManager.transform.position);
    }

    Vector3 SnapVector(Vector3 v)
    {
        return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), v.z);
    }
    
    #if UNITY_EDITOR
    void OnValidate()
    {
        transportPoint = SnapVector(transportPoint);

        if(transportPoint == Vector3.zero)
            transportPoint = new Vector3(transform.position.x, transform.position.y, 0);
    }
    
    void OnDrawGizmos()
    {        
        //Gizmos.color = Color.red;
        //Gizmos.DrawSphere(new Vector3(transform.position.x - xDistance, transform.position.y, 0), 0.1f);
        //Gizmos.DrawSphere(new Vector3(transform.position.x + xDistance, transform.position.y, 0), 0.1f);
        //Gizmos.DrawRay(new Vector3(transform.position.x, transform.position.y - yOffset, 0), Vector3.up + new Vector3(0, maxDetectionHeight - 1, 0));
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(2, 6, 1));
        
        Gizmos.color = new Color(1, 0, 1, 0.5f);
        Gizmos.DrawCube(new Vector3(transportPoint.x, transportPoint.y, 0), Vector3.one * 2);

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 10;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontStyle = FontStyle.Bold;

        Handles.Label(transportPoint, "Teleport Point", style);
    }
    #endif
}