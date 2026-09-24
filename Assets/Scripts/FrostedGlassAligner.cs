using UnityEngine;

[ExecuteAlways]
public class FrostedGlassAligner : MonoBehaviour
{
    private Canvas rootCanvas;
    private RectTransform myRect;

    private void OnEnable()
    {
        Align();
    }

    private void Start()
    {
        Align();
    }

    private void LateUpdate()
    {
        Align();
    }

    public void Align()
    {
        if (myRect == null) myRect = GetComponent<RectTransform>();
        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null && myRect != null)
        {
            RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                myRect.sizeDelta = canvasRect.rect.size;
                myRect.position = rootCanvas.transform.position;
                myRect.rotation = rootCanvas.transform.rotation;
                myRect.localScale = Vector3.one;
            }
        }
    }
}
