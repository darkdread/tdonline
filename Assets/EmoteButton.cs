using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
public class EmoteButton : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI textUi;

    public void SetEmoteButton(Sprite sprite, string displayString){
        image.sprite = sprite;
        textUi.text = displayString;
    }
}
