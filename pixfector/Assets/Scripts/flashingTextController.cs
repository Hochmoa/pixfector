using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextManager : MonoBehaviour
{

    private static flashingText flashingText;
    private static GameObject canvas;
    private static float positioningDistributionModifier = 50f;
    public static int maxTextCount=6000;
    static List<flashingText> texts = new List<flashingText>();
    public static void Initialize()
    {
      
        flashingText = Resources.Load<flashingText>("Prefabs/PopupTextParent");
        canvas = GameObject.Find("Canvas");
        
    }
    public static void CreateText(double val, Transform trans, bool isCrit,float delay)
    {
        //if (texts.Count >= maxTextCount) return;
        flashingText instance = Instantiate(flashingText);
        texts.Add(instance);
        Vector2 pos = Camera.main.WorldToScreenPoint(trans.position);
        float distX = positioningDistributionModifier / 2 - Random.value * positioningDistributionModifier;
        float distY = positioningDistributionModifier / 2 - Random.value * positioningDistributionModifier;
        pos = new Vector2(pos.x + distX, pos.y + distY);
        instance.SetText(val);
        instance.SetCrit(isCrit);
        instance.SetDelay(delay);
        instance.transform.SetParent(canvas.transform, false);
        instance.transform.SetSiblingIndex(0);
        instance.transform.position = pos;

        if(texts.Count>maxTextCount)
        {
            for (int i = 0; i < texts.Count-maxTextCount; i++)
            {
                Destroy(texts[0].gameObject);
                texts.RemoveAt(0);
            }
        }
    }
    public static void Unsubscribe(flashingText text)
    {
        texts.Remove(text);
    }

}
