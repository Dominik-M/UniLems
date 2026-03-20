using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    const int WIDTH = 180; // Width of the camera frustum
    const int HEIGHT = 100; // Height of the camera frustum

    public float yOffset = 100;
    public float scrollSpeed = 2f;
    public float left = 0.2f;
    public float right = 0.8f;
    public float top = 0.8f;
    public float bottomMax = 0.3f;
    public float bottomMin = 0.2f;

    void Update()
    {
        // normalize mouse position on screen
        Vector2 mousePos = Pointer.current.position.ReadValue();
        float normalizedX = mousePos.x / Screen.width;
        float normalizedY = mousePos.y / Screen.height;
        // ignore area on the bottom
        if (normalizedY > bottomMin)
        {
            Vector3 pos = transform.localPosition;
            if (normalizedX < left)
            {
                pos.x -= scrollSpeed * Time.deltaTime;
            }
            if (normalizedX > right)
            {
                pos.x += scrollSpeed * Time.deltaTime;
            }
            if (normalizedY < bottomMax)
            {
                pos.y -= scrollSpeed * Time.deltaTime;
            }
            if (normalizedY > top)
            {
                pos.y += scrollSpeed * Time.deltaTime;
            }
            pos.x = Mathf.Clamp(pos.x, MinX(), MaxX());
            pos.y = Mathf.Clamp(pos.y, MinY(), MaxY());
            transform.localPosition = pos;
        }
    }

    private float MinX()
    {
        return WIDTH;
    }
    private float MaxX()
    {
        int levelwidth = 0;
        if (Core.Instance.GetCurrentLevel() != null)
            levelwidth = Core.Instance.GetCurrentLevel().Width;
        return Mathf.Max(MinX(), levelwidth - WIDTH); //320;
    }
    private float MinY()
    {
        return HEIGHT * 0.6f;
    }
    private float MaxY()
    {
        int levelheight = 0;
        if (Core.Instance.GetCurrentLevel() != null)
            levelheight = Core.Instance.GetCurrentLevel().Height;
        return Mathf.Max(MinY(), levelheight - HEIGHT);
    }
}
