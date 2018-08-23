using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class flashingText : MonoBehaviour {


    private Text damageText;
    bool isCrit;
	// Use this for initialization
	void Start () {

        damageText = GetComponentInChildren<Text>();
        startPos = transform.position;
        endPos = new Vector3(startPos.x, startPos.y+100, 0);
        startColor = Color.white;
        endColor = Color.clear;
        if(isCrit)
        {
            damageText.fontSize = 70;
            startColor= new Color(1, 0.5f, 0);
         //   damageText.GetComponent<Outline>().effectColor = Color.black;
        }

    }
    
    Vector3 startPos;
    Vector3 endPos;
    Color startColor;
    Color endColor;
    float currentMoveTime;
    float currentFadeTime = 0;
    float moveTime = 0.5f;
    float fadeTime = 0.2f;
    bool done = false;
    float maxVal = 0;
    bool wasSetVisible = false;
	// Update is called once per frame
	void Update () {
        if (!done)
        {
            currentMoveTime += Time.deltaTime;
            if (currentMoveTime < 0) return;
            if(!wasSetVisible)
            {
                damageText.color = startColor;
                
              //  gameObject.SetActive(true);
                wasSetVisible = true;
            }
            float t = currentMoveTime / moveTime;
            t = t * (2-t);
            transform.position = Vector3.Lerp(startPos, endPos, t);
            if (t <maxVal) done = true;
            maxVal = t;
        }
       else
        {
            currentFadeTime += Time.deltaTime;
            float f = currentFadeTime / fadeTime;
            damageText.color = Color.Lerp(startColor, endColor, f);
            if (f >= 1)
            {
                TextManager.Unsubscribe(this);
                Destroy(gameObject);
            }
        }
      
	}
    public void SetText(double text)
    {
        Start();
        damageText.text = MenuManageScript.getFormattedValue(text,0);
    }
    public void SetCrit(bool crit)
    {
        isCrit = crit;

    }
    public void SetDelay(float delay)
    {
        currentMoveTime = -delay;
        damageText.color = Color.clear;
        damageText.GetComponent<Outline>().effectColor = Color.clear;
        // gameObject.SetActive(false);
    }
}
