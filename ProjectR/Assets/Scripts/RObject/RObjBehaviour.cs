using UnityEngine;

public class RObjectBehaviour : MonoBehaviour
{
    public RObject RObj { get; set; }

    private SpriteRenderer spriteRenderer;

    public void Awake()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
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
        spriteRenderer.sprite = RObj.VisualImage;
    }
}