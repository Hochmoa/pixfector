using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraScript : MonoBehaviour
{
    public float lowerBound;
    public float upperBound;
    public float panSpeedModifier;
    public float panDistanceModifier;
    public float showBtnUpperThreshold;
    public float showBtnLowerThreshold;
    public GameObject moveUpBtn;
    public GameObject world;
    Vector3 startPos;
    Vector3 endPos;
    public float lerpTime = 0.3f;
    float currentLerpTime = 0;
    bool lerping = false;
    bool camSticky=true;
    Color startColor;
    Color endColor;
    public float btnLerpTime = 0.5f;
    float currentBtnLerpTime = 0;
    bool lerpingBtn = false;
    bool btnEnabled = false;
    int transType;
    float oldPos = -1;
    float newPos = -1;
    float maxTVal = 0;
    float moveCamTrehshold = 10;
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition=Vector3.zero;
    public bool movementDisabled = false;
    void Start()
    {
        moveUpBtn.SetActive(btnEnabled);
    }

    void Update()
    {
        manageLerp();
        if(!camSticky)manageInput();
        checkBounds();
        
    }
  private void checkBounds()
    {
        if(transform.position.y < GetComponent<Camera>().orthographicSize-0.5f)
        {
            transform.position = new Vector3(transform.position.x,GetComponent<Camera>().orthographicSize-0.5f,transform.position.z);
            velocity = Vector3.zero;
            lerping = false;
        }
        if (transform.position.y > upperBound - GetComponent<Camera>().orthographicSize)
        {
            transform.position = new Vector3(transform.position.x, upperBound - GetComponent<Camera>().orthographicSize, transform.position.z);
            velocity = Vector3.zero;
            lerping = false;
        }
    }
    int oldFingerCount = 0;
    private void manageInput()
    {
        // if(!movementDisabled&&Input.GetMouseButton(0)&&((Input.touchCount==1&&oldFingerCount<=1)))
        if (!movementDisabled && Input.GetMouseButton(0))
        {
      
            oldFingerCount = Input.touchCount;
            oldPos = newPos;
            newPos = Input.mousePosition.y;
            lerping = false;
            velocity = Vector3.zero;
            targetPosition = Vector3.zero;
            if (oldPos != -1&&Mathf.Abs(oldPos-newPos)>moveCamTrehshold)
            {
              //  Debug.Log("Moving for " + (oldPos - newPos) * panSpeedModifier);
                Vector3 newPosition = transform.position;
                newPosition.y += ((oldPos - newPos) * panSpeedModifier);
                transform.position = newPosition;
                world.GetComponent<worldscript>().hasDragged = true;
            }
        }
        else
        {
            
            if(oldPos!=-1&&newPos!=-1&&!lerping)
            {
                velocity = Vector3.zero;
                smoothDampTime = 0.3f;
                targetPosition = new Vector3(transform.position.x, transform.position.y + panDistanceModifier * (oldPos - newPos) * panSpeedModifier, transform.position.z);
            }
            oldPos = -1;
            newPos = -1;
        }
        
    }
 
    private void manageLerp()
    {
        if (!lerpingBtn)
        {
            float pos = transform.position.y;
            if (!btnEnabled)
            {
                if (pos < lowerBound-showBtnLowerThreshold || pos > lowerBound + showBtnUpperThreshold)
                {
                    lerpingBtn = true;
                    startColor = new Color(1,1,1,0);
                    endColor = Color.white;
                    currentBtnLerpTime = 0;
                    if(pos < lowerBound + showBtnLowerThreshold)
                    {
                        moveUpBtn.transform.localScale = new Vector3(1, 1, 0);
                    }
                    else 
                    {
                        moveUpBtn.transform.localScale = new Vector3(1, -1, 0);
                    }
                    moveUpBtn.SetActive(true);
                }
            }
            else if (btnEnabled)
            {
                if (pos > lowerBound- showBtnLowerThreshold && pos < lowerBound+showBtnUpperThreshold)
                {
                    lerpingBtn = true;
                    startColor = Color.white;
                    endColor = new Color(1, 1, 1, 0);
                    currentBtnLerpTime = 0;
                }


            }


        }
        else 
        {
            currentBtnLerpTime += Time.deltaTime;
            if (currentBtnLerpTime > btnLerpTime)
            {
                currentBtnLerpTime = btnLerpTime;
            }
            float t = currentBtnLerpTime / btnLerpTime;
            //t = t * t * t * (t * (6f * t - 15f) + 10f);
            moveUpBtn.GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(startColor, endColor, t);
            if (t >= 1)
            {
                lerpingBtn = false;
                btnEnabled = !btnEnabled;
                if (!btnEnabled) moveUpBtn.SetActive(false);
            }
        }


        //lerp!
        if (lerping)
        {
            currentLerpTime += Time.deltaTime;
            if (currentLerpTime > lerpTime)
            {
                currentLerpTime = lerpTime;
            }
            float t = currentLerpTime / lerpTime;
           // if(transType==0)t = t * t * t * (t * (6f * t - 15f) + 10f);
            if(transType==1) t = Mathf.Sin(t * Mathf.PI) / 1.3f;
          //  if(transType==2) t = Mathf.Sin(t * Mathf.PI);
            //     if (transType == 0) t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);

            if (maxTVal<t)
            {
                maxTVal = t;
                transform.position = Vector3.Lerp(startPos, endPos, t);
            }
            else
            {
                lerping = false;
                maxTVal = 0;
            }

        }
        if(targetPosition!=Vector3.zero) transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothDampTime);
        if (velocity == Vector3.zero) targetPosition = Vector3.zero;

    }
    float smoothDampTime;
    public void moveUp()
    {
        if (!camSticky) return;
        targetPosition = new Vector3(transform.position.x, lowerBound, transform.position.z);
        velocity = new Vector3(0, lowerBound-transform.position.y, 0);
        smoothDampTime = 0.1f;
       
        /*  velocity = Vector3.zero;
          targetPosition = Vector3.zero;
          //if (!lerping)
      //    {
              lerping = true;
              startPos = transform.position;
              endPos = new Vector3(transform.position.x, lowerBound, transform.position.z);
              currentLerpTime = 0;
              transType = 0;
        //  }
        //  else
        //  {
              //currentLerpTime -= (lowerBound - transform.position.y) / lerpTime;
        //      endPos = new Vector3(transform.position.x, lowerBound+10, transform.position.z);
       //   }*/
    }
    public void setLowerBound(float lower)
    {
        lowerBound = lower;
    }
    public void setUpperBound(float upper)
    {
        upperBound = upper;
    }

    public void stickyCamValueChanged()
    {
        camSticky = !camSticky;

        moveUp();
    }
}
