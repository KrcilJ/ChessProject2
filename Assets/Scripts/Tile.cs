using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color dark, light, red;
    [SerializeField] SpriteRenderer renderer;
    private Color originalColor;
    public void isLight(bool isLight)
    {
        renderer.color = isLight ? light : dark;
    }
    public void tileRed()
    {
        if (renderer.color != red)
        {
            originalColor = renderer.color;
        }
        renderer.color = red;
    }
    public void resetColor()
    {
        if (renderer.color == red)
        {
            renderer.color = originalColor;
        }

    }
}
