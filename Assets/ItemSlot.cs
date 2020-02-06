using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public GameObject item;
    public Image itemImage;
    public Sprite emptySlotSprite;

    public bool IsEmpty(){
        return itemImage.sprite == emptySlotSprite;
    }

    public void SetEmpty(){
        itemImage.sprite = emptySlotSprite;
    }
}
