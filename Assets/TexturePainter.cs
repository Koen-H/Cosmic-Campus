using UnityEngine;

public class TexturePainter : MonoBehaviour
{
    public Texture2D baseTexture;
    public Texture2D brushTexture;
    public float brushRadius = 0.1f;

    private MeshRenderer meshRenderer;
    private Texture2D textureInstance;
    private Texture2D brushInstance;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // Create a new instance of the texture so we're not modifying the original
        textureInstance = Instantiate(baseTexture);
        meshRenderer.material.mainTexture = textureInstance;

        // Resize brush based on the brush radius
        int brushSize = Mathf.FloorToInt(brushRadius * baseTexture.width);
        brushInstance = Resize(brushTexture, brushSize, brushSize);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                DrawOnTexture(hit.textureCoord);
            }
        }
    }

    private void DrawOnTexture(Vector2 texCoords)
    {
        int x = Mathf.FloorToInt(texCoords.x * baseTexture.width - brushInstance.width / 2);
        int y = Mathf.FloorToInt(texCoords.y * baseTexture.height - brushInstance.height / 2);

        Color[] brushPixels = brushInstance.GetPixels();

        for (int brushX = 0; brushX < brushInstance.width; brushX++)
        {
            for (int brushY = 0; brushY < brushInstance.height; brushY++)
            {
                int textureX = x + brushX;
                int textureY = y + brushY;

                if (textureX >= 0 && textureX < baseTexture.width &&
                    textureY >= 0 && textureY < baseTexture.height)
                {
                    textureInstance.SetPixel(textureX, textureY, brushPixels[brushX + brushY * brushInstance.width]);
                }
            }
        }

        textureInstance.Apply();
    }

    // Resizes a texture
    private Texture2D Resize(Texture2D texture2D, int targetWidth, int targetHeight)
    {
        RenderTexture rt = new RenderTexture(targetWidth, targetHeight, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetWidth, targetHeight);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();
        return result;
    }
}
