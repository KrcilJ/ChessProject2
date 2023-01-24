using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
public class Tile : MonoBehaviour
{
   [SerializeField] private Color dark, light;
    [SerializeField] SpriteRenderer renderer;

    public void isLight(bool isLight){
        renderer.color = isLight ? light : dark;
    }

   
       
   
      
}
