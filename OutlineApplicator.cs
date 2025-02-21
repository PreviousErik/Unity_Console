using UnityEngine;

public class OutlineApplicator : MonoBehaviour
{
    public Color outlineColor;
    public float outlineThickness;

    private static OutlineApplicator instance;

    private Camera outlineCamera;
    [SerializeField]
    private Material outlineMaterial;

    void Awake()
    {
        if (outlineMaterial == null)
        {
            Debug.LogWarning("The \"outlineMaterial\" Needs to be set in the inspector");
            return;
        }
        if (instance != null)
        {
            Debug.LogWarning("Duplicate instances of type OutlineApplicator found!");
            return;
        }
        instance = this;

        // Create an outline material using the shader
        outlineMaterial.SetColor("_OutlineColor", outlineColor);
        outlineMaterial.SetFloat("_Thickness", outlineThickness);

        // Create a secondary camera for rendering outlines
        GameObject cameraObj = new GameObject("OutlineCamera");
        outlineCamera = cameraObj.AddComponent<Camera>();
        outlineCamera.enabled = false; // Only use it for the outline render pass
        DontDestroyOnLoad(cameraObj);
    }

    public static void HighlightObject(GameObject obj) => instance.Highlight(obj);

    private void Highlight(GameObject obj)
    {
        Debug.Log("Applied outline too " + obj.name);

        int originalLayer = obj.layer;  
        obj.layer = LayerMask.NameToLayer("Outline");

        outlineCamera.targetTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 16);
        outlineCamera.RenderWithShader(outlineMaterial.shader, "");

        obj.layer = originalLayer;

        Graphics.Blit(outlineCamera.targetTexture, null as RenderTexture);

        RenderTexture.ReleaseTemporary(outlineCamera.targetTexture);
    }
}
