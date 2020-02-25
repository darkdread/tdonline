using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ImageWithText : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI resourceCountText;

    public void SetImageSprite(Sprite sprite){
        image.sprite = sprite;
    }

    public void SetResourceText(string text){
        resourceCountText.text = text;
    }
}
