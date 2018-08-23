
using UnityEngine;

public class Lerp
{

    public enum AnimationType
    {
        NONE, TRANSFORM, DAMAGE, DAMAGEBACKGROUND, ATTACKTRANS, ATTACKSCALE, COLOR
    }
    public enum FunctionType
    {
        LINEAR, SMOOTHO, SMOOTHI, SMOOTHIO
    }
    public enum VectorType
    {
        TRANSLATE, SCALE, ROTATE
    }
    public enum LerpResult
    {
        NONE,DISABLEBACKGROUD,DISABLEMENU,SHOWMENU,NOTFINISHED,
        DELETEATTACKANIMATIONINSTANCE,
        DELETEBACKGROUNDANIMATIONINSTANCE,
        DELETEDAMAGEANIMATIONINSTANCE
    }
    System.Object colorObj;
    Transform transform;
    VectorType vectorType;
    FunctionType functionType;
    LerpResult lerpResult;
    Vector3 start;
    Vector3 end;
    Color startColor;
    Color endColor;
    public AnimationType animationType;
    float currentTime;
    float animationLength;
    bool reverse;
    int currentStage;
    float initialDelay;
    public Lerp(Transform transform, VectorType vectorType, FunctionType functionType, Vector3 start, Vector3 end, float animationLength, bool reverse, AnimationType animationType,float initialDelay,LerpResult lerpResult)
    {
        this.animationType = animationType;
        this.transform = transform;
        this.vectorType = vectorType;
        this.functionType = functionType;
        this.start = start;
        this.end = end;
        this.animationLength = (reverse ? animationLength / 2 : animationLength);
        currentTime = -initialDelay;
        this.reverse = reverse;
        currentStage = 0;
        this.lerpResult = lerpResult;
    }
    public Lerp(Object colorObj, AnimationType animationType, FunctionType functionType, Color start, Color end, float animationLength, bool reverse,  float initialDelay,LerpResult lerpResult)
    {
        this.animationType = animationType;
        this.functionType = functionType;
        this.startColor = start;
        this.endColor = end;
        this.animationLength = (reverse ? animationLength / 2 : animationLength);
        currentTime = -initialDelay;
        this.reverse = reverse;
        currentStage = 0;
        this.lerpResult = lerpResult;
        this.colorObj = colorObj;
    }
    public void changeEndVector(Vector3 end)
    {
        if(animationType==AnimationType.DAMAGE)
        {
            this.end = end;
        }
    }
    public bool isFinished()
    {
        return currentTime >= animationLength;
    }
    public void addTime(float delta)
    {
        currentTime += delta;
    }
    public LerpResult lerp()
    {
        
        if (currentTime <= 0) return LerpResult.NOTFINISHED;

        float t = currentTime / animationLength;
        switch (functionType)
        {
            case FunctionType.LINEAR:
                {

                    break;
                }
            case FunctionType.SMOOTHI:
                {
                    t = Mathf.Sin(t * Mathf.PI);
                    break;
                }
            case FunctionType.SMOOTHO:
                {
                    t = 1f - Mathf.Cos(t * Mathf.PI);
                    break;
                }
            case FunctionType.SMOOTHIO:
                {
                    t = t * t * t * (t * (6f * t - 15f) + 10f);
                    break;
                }
        }
        if (animationType != AnimationType.COLOR)
        {
            Vector3 lerpVector = Vector3.zero;
            if (currentStage == 0)
            {
                lerpVector = Vector3.Lerp(start, end, t);
            }
            else
            {
                lerpVector = Vector3.Lerp(end, start, t);
            }
            switch (vectorType)
            {
                case VectorType.TRANSLATE:
                    {
                        transform.position = lerpVector;
                        break;
                    }
                case VectorType.SCALE:
                    {
                        transform.localScale = lerpVector;
                        break;
                    }
                case VectorType.ROTATE:
                    {
                        Debug.LogError("Rotation not implemented");
                        break;
                    }
         
            }
        }
        else
        {
            
            //string name = colorObj.GetType().ToString();
            //Debug.Log(name);
            if(colorObj is UnityEngine.UI.Image )
            {
                ((UnityEngine.UI.Image)colorObj).color = Color.Lerp(startColor, endColor, t);
            }




        }
        if (isFinished())
        {
            if (reverse)
            {
                reverse = false;
                currentStage = 1;
                currentTime = 0;
            }
            else
            {
                return lerpResult;
            }
        }
        return LerpResult.NOTFINISHED;

    }
}

