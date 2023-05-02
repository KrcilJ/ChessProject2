using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color dark, light, red;
    [SerializeField] SpriteRenderer renderer;
    //Each tile will have its color saved
    private Color originalColor;

    public void setTileColor(bool isLight)
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
