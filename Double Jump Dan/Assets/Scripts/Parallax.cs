using UnityEngine;

public class Parallax : MonoBehaviour 
{
    [SerializeField] float amplification;
    [SerializeField] bool onlyAffectXAxis;
    [SerializeField] bool resizeSprite;
    [SerializeField] int startingOffsetX;

    Vector3 offset;
    Camera _camera;
    Transform cameraHolder;
    SpriteRenderer spriteRenderer;
    Vector2 startingSize;

    void Start()
    {
        _camera = LevelManager.Instance.mainCamera;
        cameraHolder = _camera.transform.parent;
        
        if(resizeSprite)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            startingSize = GetComponent<SpriteRenderer>().size;        
        }

        OffsetBackground();
    }

    void OnValidate()
    {
        if(!Application.isPlaying)
            transform.position = new Vector2(startingOffsetX, transform.position.y);
    }

    public void OffsetBackground()
    {
        float cameraSize = _camera.orthographicSize * ((float)Screen.width / Screen.height);
        offset = new Vector3((-cameraSize * amplification) - 2 + startingOffsetX, -_camera.orthographicSize * amplification);
    }

	void LateUpdate() 
	{
        if(!onlyAffectXAxis)
            transform.localPosition = new Vector3(cameraHolder.transform.localPosition.x * amplification + offset.x, cameraHolder.transform.localPosition.y * amplification + offset.y, 0);
        else
            transform.localPosition = new Vector3(cameraHolder.transform.localPosition.x * amplification + offset.x, transform.localPosition.y, 0);

        if(resizeSprite)
            if(cameraHolder.transform.position.x + _camera.orthographicSize * ((float)Screen.width / Screen.height) > transform.position.x + spriteRenderer.bounds.extents.x * 2)
                spriteRenderer.size = new Vector2(spriteRenderer.size.x + startingSize.x, spriteRenderer.size.y);
    }
}