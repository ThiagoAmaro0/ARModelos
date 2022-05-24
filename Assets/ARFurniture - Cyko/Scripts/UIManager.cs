using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static int index;
    public Sprite[] sprites;
    public Image chairImage;

    private void Awake()
    {
        index = 0;
    }

    public void SetImage(int i)
    {
        index = i;
        chairImage.sprite = sprites[i];
    }

}
