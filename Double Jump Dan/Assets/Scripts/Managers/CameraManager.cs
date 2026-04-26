using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Camera Follow")]
    [SerializeField] Vector2 smoothing;

    bool following;
    Player player;
    BoxCollider2D bounds;
    Vector3 _min, _max;
    float cameraSize;
    Camera _camera;
    Vector3 position;
    const float maxAngle = 10f;
    IEnumerator currentShakeCoroutine;
    Transform parent;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _camera = GetComponent<Camera>();
        
        player = LevelManager.Instance.player;
        player.OnPlayerRespawn += LerpToPlayerSpawnPoint;
        
        bounds = LevelManager.Instance.levelBounds;
        _min = bounds.bounds.min;
        _max = bounds.bounds.max;
        parent = transform.parent;
        
        SnapCamera(LevelManager.Instance.currentSpawnPoint);
    }

    void FixedUpdate()
    {
        position = parent.position;
        following = player.canFollow;
        
        if(following)
        {
            if(Mathf.Abs(position.x - player.transform.position.x) > 0)
                position.x = Mathf.Lerp(position.x, player.transform.position.x, smoothing.x * Time.deltaTime);

            if(Mathf.Abs(position.y - player.transform.position.y) > 0)
                position.y = Mathf.Lerp(position.y, player.transform.position.y, smoothing.y * Time.deltaTime);
        }

        SnapCamera(position);
    }

    public void LerpToPlayerSpawnPoint()
    {
        StartCoroutine(LerpToPlayerSpawnPointCo(LevelManager.Instance.currentSpawnPoint));
    }

    IEnumerator LerpToPlayerSpawnPointCo(Vector3 targetPosition)
    {
        float inTime = 0;
        float duration = 0.75f;
        Vector3 startPosition = transform.position;

        while(inTime < duration)
        {
            inTime += Time.unscaledDeltaTime;
            
            float t = inTime / duration;
            float smoothT = 1 - Mathf.Pow(1 - t, 4);

            position = Vector3.Lerp(startPosition, new Vector3(targetPosition.x, targetPosition.y, startPosition.z), smoothT);
            SnapCamera(position);

            yield return null;
        }
    }

    public void SnapToPlayerPosition()
    {
        SnapCamera(player.transform.position);
    }

    public void SnapCamera(Vector2 _position)
    {
        cameraSize = _camera.orthographicSize * ((float)Screen.width / Screen.height);

        position.x = _position.x;
        position.y = _position.y;

        position.x = Mathf.Clamp(position.x, _min.x + cameraSize, _max.x - cameraSize);
        position.y = Mathf.Clamp(position.y, _min.y + _camera.orthographicSize, _max.y - _camera.orthographicSize);

        parent.position = new Vector3(position.x, position.y, -20);
    }

    public void Shake(Properties properties)
    {
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
        }

        currentShakeCoroutine = ShakeCo(properties);
        StartCoroutine(currentShakeCoroutine);
    }

    IEnumerator ShakeCo(Properties properties)
    {
        Vector3 originalPosition = transform.localPosition;

        float completionPercent = 0;
        float movePercent = 0;

        float angle_radians = properties.angle * Mathf.Deg2Rad - Mathf.PI;
        Vector3 previousWaypoint = Vector3.zero;
        Vector3 currentWaypoint = Vector3.zero;
        float moveDistance = 0;
        float speed = 0;

        Quaternion targetRotation = Quaternion.identity;
        Quaternion previousRotation = Quaternion.identity;

        do
        {
            if (movePercent >= 1 || completionPercent == 0)
            {
                float dampingFactor = DampingCurve(completionPercent, properties.dampingPercent);
                float noiseAngle = (Random.value - 0.5f) * Mathf.PI;
                angle_radians += Mathf.PI + noiseAngle * properties.noisePercent;
                currentWaypoint = originalPosition + new Vector3(Mathf.Cos(angle_radians), Mathf.Sin(angle_radians)) * properties.strength * dampingFactor;
                previousWaypoint = transform.localPosition;
                moveDistance = Vector3.Distance(currentWaypoint, previousWaypoint);

                targetRotation = Quaternion.Euler(new Vector3(currentWaypoint.y, currentWaypoint.x).normalized * properties.rotationPercent * dampingFactor * maxAngle);
                previousRotation = transform.localRotation;

                speed = Mathf.Lerp(properties.minSpeed, properties.maxSpeed, dampingFactor);

                movePercent = 0;
            }

            completionPercent += Time.deltaTime / properties.duration;
            movePercent += Time.deltaTime / moveDistance * speed;

            if(float.IsNaN(movePercent))
                movePercent = 0;

            transform.localPosition = Vector3.Lerp(previousWaypoint, currentWaypoint, movePercent);
            transform.localRotation = Quaternion.Slerp(previousRotation, targetRotation, movePercent);

            yield return null;
        } 
        while(moveDistance > 0);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    float DampingCurve(float x, float dampingPercent)
    {
        x = Mathf.Clamp01(x);
        float a = Mathf.Lerp(2, 0.25f, dampingPercent);
        float b = 1 - Mathf.Pow(x, a);
        return b * b * b;
    }

    [System.Serializable]
    public class Properties
    {
        public float angle;
        public float strength;
        public float maxSpeed;
        public float minSpeed;
        public float duration;
        [Range(0, 1)]
        public float noisePercent;
        [Range(0, 1)]
        public float dampingPercent;
        [Range(0, 1)]
        public float rotationPercent;
    }
}