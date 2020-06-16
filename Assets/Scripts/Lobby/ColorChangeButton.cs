using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorChangeButton : MonoBehaviour
{
    [SerializeField] Color color;
    [SerializeField] float colorUIBrigtness = 0.2f;

    public Color GetButtonColor()
    {
        return color;
    }

    public void SetColor(Color color)
    {
        this.color = color;
    }

    private void Awake()
    {
        //Se muestra el color algo más brillante en la interfaz
        gameObject.GetComponent<Image>().color = color + new Color(colorUIBrigtness, colorUIBrigtness, colorUIBrigtness, 1.0f);
    }
}
