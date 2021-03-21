using UnityEngine;

public class RObjectBehaviour : MonoBehaviour
{
    public RObject rObj { get; set; }

    private SpriteRenderer spriteRenderer;

    public void Awake()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    }

    public void Init(RObject rObj)
    {
        this.rObj = rObj;
    }

    private void Update()
    {
        transform.position = WorldManager.MapPosToWorldPosition(rObj.MapPosition);
        VisualUpdate();
    }

    public void VisualUpdate()
    {
        rObj.VisualUpdate(Time.deltaTime);
        spriteRenderer.sprite = rObj.VisualImage;
    }
}