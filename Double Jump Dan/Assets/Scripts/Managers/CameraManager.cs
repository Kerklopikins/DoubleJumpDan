using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Camera Follow")]
    [SerializeField] float followSpeed;
    [SerializeField] float followSmoothTime;
    [SerializeField] bool enableLookahead;
    [SerializeField] float lookaheadDistance;
    [SerializeField] float lookaheadSpeed;
    [SerializeField] float lookaheadSmoothTime;
    [SerializeField] float lookaheadDeadzone;

    Vector3 currentVelocity;
    float lookaheadVelocity;
    float currentLookaheadOffset;
    float targetLookaheadOffset;
    
    Vector2 previousTargetPosition;
    Vector2 targetVelocity;

    bool following;
    Transform target;
    Player player;
    BoxCollider2D bounds;
    Vector3 boundsMin, boundsMax;
    float cameraSize;
    Camera _camera;
    Vector3 position;
    const float maxAngle = 10f;
    IEnumerator currentShakeCoroutine;
    Transform parent;
    GameInputManager gameInputManager;
    
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _camera = GetComponent<Camera>();
        gameInputManager = GameInputManager.Instance;

        player = LevelManager.Instance.player;
        player.OnPlayerRespawn += OnPlayerRespawn;

        target = player.transform;

        bounds = LevelManager.Instance.levelBounds;
        boundsMin = bounds.bounds.min;
        boundsMax = bounds.bounds.max;
        parent = transform.parent;
        
        previousTargetPosition = LevelManager.Instance.currentSpawnPoint;
        SnapCameraToPosition(LevelManager.Instance.currentSpawnPoint);
    }

    void FixedUpdate()
    {
        following = player.canFollow;

        if(!following)
            return;

        targetVelocity = ((Vector2)target.position - previousTargetPosition) / Time.deltaTime;
        previousTargetPosition = (Vector2)target.position;

        if(enableLookahead)
        {
            if(Mathf.Abs(targetVelocity.x) > lookaheadDeadzone && gameInputManager.HorizontalMoveInput() != 0)
            {
                //CAMERA add a look bellow when falling and maybe when going up as well
                targetLookaheadOffset = Mathf.Lerp(targetLookaheadOffset, targetVelocity.normalized.x * lookaheadDistance, lookaheadSpeed * Time.deltaTime);
                //print()
            }
            else
            {
                targetLookaheadOffset = 0;        
                targetLookaheadOffset = Mathf.Lerp(targetLookaheadOffset, 0, lookaheadSpeed * Time.deltaTime);
            }

            currentLookaheadOffset = Mathf.SmoothDamp(currentLookaheadOffset, targetLookaheadOffset, ref lookaheadVelocity, lookaheadSmoothTime);
        }
        else
        {
            currentLookaheadOffset = 0;
            targetLookaheadOffset = 0;
        }

        Vector3 smoothedPosition = Vector3.SmoothDamp(parent.position, new Vector3(target.position.x + currentLookaheadOffset, target.position.y, -20), ref currentVelocity, followSmoothTime / followSpeed);
            
        smoothedPosition.z = -20;
        MoveAndClampCamera(smoothedPosition);
    }

    public void OnPlayerRespawn()
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
            MoveAndClampCamera(position);

            yield return null;
        }

        currentLookaheadOffset = 0;
        targetLookaheadOffset  = 0;
        currentVelocity = Vector2.zero;
        lookaheadVelocity = 0;

        previousTargetPosition = target.position;
    }

    public void SnapToPlayerPosition()
    {
        SnapCameraToPosition(target.position);
    }

    public void MoveAndClampCamera(Vector2 _position)
    {
        cameraSize = _camera.orthographicSize * ((float)Screen.width / Screen.height);

        position.x = _position.x;
        position.y = _position.y;

        position.x = Mathf.Clamp(position.x, boundsMin.x + cameraSize, boundsMax.x - cameraSize);
        position.y = Mathf.Clamp(position.y, boundsMin.y + _camera.orthographicSize, boundsMax.y - _camera.orthographicSize);

        parent.position = new Vector3(position.x, position.y, -20);
        previousTargetPosition = target.position;
    }

    void SnapCameraToPosition(Vector2 _position)
    {
        MoveAndClampCamera(_position);

        currentLookaheadOffset = 0;
        targetLookaheadOffset = 0;
        currentVelocity = Vector2.zero;
        lookaheadVelocity = 0;

        parent.position = new Vector3(position.x, position.y, -20);
        previousTargetPosition = target.position;
    }

    public void Shake(Properties properties)
    {
        if(!GameManager.Instance.cameraShake)
            return;
            
        if(currentShakeCoroutine != null)
            StopCoroutine(currentShakeCoroutine);

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
            if(movePercent >= 1 || completionPercent == 0)
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

    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Draw the lookahead target position
        if(enableLookahead && Application.isPlaying && target != null)
        {
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.8f);
            Vector3 lookaheadWorld = new Vector3(target.position.x + currentLookaheadOffset, target.position.y, 0f);
            Gizmos.DrawWireSphere(lookaheadWorld, 0.25f);
            Gizmos.DrawLine(target.position, lookaheadWorld);
        }
    }
#endif

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