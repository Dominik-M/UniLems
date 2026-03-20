using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    public float selectionRadius = 0.2f;
    public Sprite cursorImageNormal;
    public Sprite cursorImageHover;

    private SpriteRenderer sr;
    private GuyBehavior hoveredGuy;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Follow mouse
        Vector2 mousePos = Pointer.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        mouseWorldPos.z = 0;
        transform.position = mouseWorldPos;

        FindGuyUnderMouse(mouseWorldPos);
        if (hoveredGuy != null)
        {
            sr.sprite = cursorImageHover;
            if ((Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
            || (Mouse.current.leftButton.wasReleasedThisFrame))
            {
                Core.Instance.GuyClicked(hoveredGuy);
            }
        }
        else
        {
            sr.sprite = cursorImageNormal;
        }
    }

    void FindGuyUnderMouse(Vector3 pos)
    {
        // Wir nutzen Physics2D, um nach dem BoxCollider2D der Guys zu suchen
        Collider2D hit = Physics2D.OverlapCircle(pos, selectionRadius);

        if (hit != null && hit.TryGetComponent<GuyBehavior>(out hoveredGuy))
        {
            // Optional: Highlighte den Guy (z.B. weiﬂe Outline oder Gizmo)
            Debug.DrawLine(pos, hoveredGuy.transform.position, Color.green);
        }
        else
        {
            hoveredGuy = null;
        }
    }
}