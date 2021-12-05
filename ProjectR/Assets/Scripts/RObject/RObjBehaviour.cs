using UnityEngine;

public class RObjectBehaviour : MonoBehaviour
{
    public RObject RObj { get; set; }

    private SpriteRenderer spriteRenderer;
    private new BoxCollider2D collider;

    private void Start()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        collider = gameObject.AddComponent<BoxCollider2D>();
    }

    public void Init(RObject rObj)
    {
        this.RObj = rObj;
    }

    private void Update()
    {
        transform.position = RObj.MapPosition;
        VisualUpdate();
    }

    public void VisualUpdate()
    {
        RObj.VisualUpdate(Time.deltaTime);

        if (spriteRenderer.sprite != RObj.VisualImage)
        {
            spriteRenderer.sprite = RObj.VisualImage;
            collider.offset = RObj.VisualImage.bounds.center;
            collider.size = RObj.VisualImage.bounds.size;
        }
            
    }

    private void OnGUI()
    {
        RObj.OnGUI();
    }
}