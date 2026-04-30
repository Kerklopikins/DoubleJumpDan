using UnityEngine;

public class MainMenuCamera: MonoBehaviour
{
    public float zippy;
    Vector2 input;
    Vector2 position;
    SpriteRenderer spriteRenderer;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

   void Update()
    {
        float cameraHalfWidth =  Camera.main.orthographicSize * ((float)Screen.width / Screen.height);
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        position += input * zippy * Time.deltaTime;
        position = new Vector2(Mathf.Clamp(position.x, -cameraHalfWidth + spriteRenderer.size.x / 2, cameraHalfWidth - spriteRenderer.size.x / 2), Mathf.Clamp(position.y, -Camera.main.orthographicSize + spriteRenderer.size.y / 2, Camera.main.orthographicSize - spriteRenderer.size.y / 2));
        transform.position = position;
    }
}