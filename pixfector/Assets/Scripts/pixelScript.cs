using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pixelScript : MonoBehaviour
{


    //-1 = border
    // 1 = normal
    public static Color pixelColor;
    public Color bgColor;
    public static GameObject bgPrefab;
    public GameObject bgInstance;

    public static GameObject world;
    public static GameObject canvas;

    public float life;
    public float maxLife;
    public static float transformAnimationLength;
    public static float damageAnimationLength;
    public static float attackAnimationLength;
    public static float attackAnimationStretch;
    public static float vertLifeIncreaseModifier;
    public static string nameGoodPixel;
    public static string nameBadPixel;
    public static string nameEndPixel;

    public static float attackReposValue;
    public static float attackSpeedDistributionModifier;
    public static int width;
    public static float maxLifeDistributionModifier;
    public float coolDown;

    static float attackSpeedAnimationThreshold = 0.3f;
    static float disableAnimationThreshold = 0.1f;

    List<Lerp> lerpList = new List<Lerp>();
    // Use this for initialization
    void Start()
    {
        life = getInitLife();
        maxLife = life;
        resetAttackCoolDown(true);

            bgInstance = GameObject.Instantiate(bgPrefab, transform);
            bgInstance.transform.parent = null;
            bgInstance.transform.Translate(0, 0, 1);
            bgInstance.GetComponent<SpriteRenderer>().color = bgColor;
        cascadingDamage = -1;
    }
    public void setTag(string tag)
    {

        this.tag = tag;
    }
    public void prepareForKill()
    {
       
        if(transform.position.x%1!=0|| transform.position.y % 1 != 0)
        {
            transform.position = new Vector3(transform.position.x - transform.position.x % 1, transform.position.y - transform.position.y % 1);
            Debug.LogError("Position not on int");
        }
        Destroy(bgInstance);
    }
    public int neighbourCount(string givenTag)
    {
        int c = 0;
        for (int i = 0; i < GetComponent<neighbourScript>().neighbours.Length; i++)
        {
            if (GetComponent<neighbourScript>().neighbours[i] != null && GetComponent<neighbourScript>().neighbours[i].tag != nameEndPixel)
            {
                if (givenTag == "any")
                {
                    c++;
                    continue;

                }
                if (GetComponent<neighbourScript>().neighbours[i].CompareTag(givenTag)) c++;
            }
        }
        return c;
    }
    public float getInitLife()
    {
        float y = transform.position.y;
        if (y < 1) return 1;
        float vertLife = 0;
        if (transform.position.x <= width / 2) vertLife = transform.position.x;
        else if (transform.position.x > width / 2) vertLife = width - transform.position.x;

        // return Mathf.Pow(1.07f, vertLifeIncreaseModifier * vertLife + y * (1 + (maxLifeDistributionModifier / 2 - (Random.value * maxLifeDistributionModifier))));
        // return vertLifeIncreaseModifier*vertLife + y*(1+(maxLifeDistributionModifier/2-(Random.value*maxLifeDistributionModifier)));
        return Mathf.Pow(1.02f, y)+10;
    }

    // Update is called once per frame

    void Update()
    {
        if (coolDown != 0&&tag==nameGoodPixel)
        {
            coolDown -= Time.deltaTime;
            if (coolDown < 0) coolDown = 0;
        }
        List<Lerp> toRemove = new List<Lerp>();
        bool addTransform = false;
        foreach (var item in lerpList)
        {
            item.addTime(Time.deltaTime);
            Lerp.LerpResult result = item.lerp();
            if (result!=Lerp.LerpResult.NOTFINISHED)
            {
                if (life <= 0&&item.animationType== Lerp.AnimationType.DAMAGE)
                {
                    addTransform = true;
                }
                toRemove.Add(item);
                if(result == Lerp.LerpResult.DELETEDAMAGEANIMATIONINSTANCE)
                {
                    lerpDamageInstance = null;
                }
                if (result == Lerp.LerpResult.DELETEBACKGROUNDANIMATIONINSTANCE)
                {
                    lerpBackgroundInstance = null;
                }
                if (result == Lerp.LerpResult.IMMEDIATEATTACK)
                {
                    //coolDown = 0;
                }
            }
        }
        foreach (var item in toRemove)
        {
            lerpList.Remove(item);
        }
        if (addTransform)
        {
           
            if(!transformtriggered)triggerTransformAnimation();
        }
   

    }

    public void resetAttackCoolDown(bool distribute)
    {
       if(distribute) coolDown = upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.pixelAttackSpeed)+Random.value * attackSpeedDistributionModifier - attackSpeedDistributionModifier / 2;
       else coolDown = upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.pixelAttackSpeed);
    }
    bool transformtriggered = false;
    public void triggerTransformAnimation()
    {
        List<Lerp> toRemove = new List<Lerp>();
        foreach (var item in lerpList)
        {
            if(item.animationType==Lerp.AnimationType.DAMAGE)
            {
                toRemove.Add(item);
            }
        }
        foreach (var item in toRemove)
        {
            lerpList.Remove(item);
        }
        gameObject.GetComponent<SpriteRenderer>().color = pixelColor;
        gameObject.tag = nameGoodPixel;
        resetAttackCoolDown(true);
        transformtriggered = true;
        float length = transformAnimationLength;
        //if (life <-maxLife) length = 0;
        world.GetComponent<worldscript>(). getNeighbours(this.gameObject);
        lerpList.Add(new Lerp(gameObject.transform,Lerp.VectorType.SCALE, Lerp.FunctionType.SMOOTHIO,Vector3.zero,new Vector3(100,100),length,false, Lerp.AnimationType.TRANSFORM,0,Lerp.LerpResult.IMMEDIATEATTACK));

    }
    public double getValue()
    {
        return (maxLife* upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.moneyPerKill));
    }
    public void triggerDamageAnimation(float delay)
    {
        if(transformtriggered)return;
        float scale = (life<0?0:life/ maxLife) * 100;
        Vector3 endScale = Vector3.zero;
       
            endScale = new Vector3(scale, scale);

   

        if (lerpDamageInstance == null)
        {
            float length = transformAnimationLength;
           
            lerpDamageInstance = new Lerp(gameObject.transform, Lerp.VectorType.SCALE, Lerp.FunctionType.LINEAR,
                transform.localScale,
                endScale,
                length, false,
                Lerp.AnimationType.DAMAGE, delay, Lerp.LerpResult.DELETEDAMAGEANIMATIONINSTANCE);


            lerpList.Add(lerpDamageInstance);
        }
        else
        {
            lerpDamageInstance.changeEndVector(endScale);
        }
        if (upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.pixelAttackSpeed) >= disableAnimationThreshold)
        {

            if (lerpBackgroundInstance == null)
            {
                lerpBackgroundInstance = new Lerp(bgInstance.transform, Lerp.VectorType.SCALE, Lerp.FunctionType.LINEAR,
                           bgInstance.transform.localScale,
                           endScale,
                           damageAnimationLength, true,
                           Lerp.AnimationType.DAMAGEBACKGROUND, delay, Lerp.LerpResult.DELETEBACKGROUNDANIMATIONINSTANCE);
                lerpList.Add(lerpBackgroundInstance);
            }
        }
    }
    Lerp lerpDamageInstance;
    Lerp lerpBackgroundInstance;
    public void triggerAttackAnimation(float direction)
    {
        Vector3 endScale=Vector3.zero;
        Vector3 endPos=Vector3.zero;
        if (direction == 0|| direction == 2)
        {
            
            endScale = new Vector3(transform.localScale.x, transform.localScale.y * attackAnimationStretch);
            if(direction==0) endPos = new Vector3(transform.position.x, transform.position.y + attackReposValue, transform.position.z);
            if(direction==2) endPos = new Vector3(transform.position.x, transform.position.y - attackReposValue, transform.position.z);
        }
        else if (direction == 1 || direction == 3)
        {

            endScale = new Vector3(transform.localScale.x * attackAnimationStretch, transform.localScale.y);
            if (direction == 1) endPos = new Vector3(transform.position.x + attackReposValue, transform.position.y, transform.position.z);
            if (direction == 3) endPos = new Vector3(transform.position.x - attackReposValue, transform.position.y, transform.position.z);
        }


        if (attackAnimationLength != 0)
        {
            lerpList.Add(new Lerp(transform, Lerp.VectorType.TRANSLATE, Lerp.FunctionType.LINEAR, this.transform.position, endPos, attackAnimationLength, true, Lerp.AnimationType.ATTACKTRANS, 0, Lerp.LerpResult.NONE));
            lerpList.Add(new Lerp(transform, Lerp.VectorType.SCALE, Lerp.FunctionType.LINEAR, this.transform.localScale, endScale, attackAnimationLength, true, Lerp.AnimationType.ATTACKSCALE, 0, Lerp.LerpResult.NONE));
        }
        }
    public bool isImmune()
    {
        //if (currentAnimationType != AnimationType.NONE) return true;
        return false||tag==nameGoodPixel;
    }
    public bool attack(int direction, float damage, bool isCrit)
    {

        resetAttackCoolDown(false);
        if(upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.pixelAttackSpeed) >= disableAnimationThreshold)triggerAttackAnimation(direction);
        return GetComponent<neighbourScript>().neighbours[direction].GetComponent<pixelScript>().tryToDamage(damage, isCrit,0);
    }
    public bool isAnimating()
    {
        return lerpList.Count != 0;
    }
    public bool canAttack()
    {
        return coolDown == 0;
    }
    public bool tryToDamage(float damage, bool isCrit, float delay)
    {
        
        if (isImmune()) return false;
        if (damage < 1) damage = 1;
        TextManager.CreateText(damage, this.transform, isCrit,delay);
        life -= damage;
        
        if (!checkOverkill(damage))triggerDamageAnimation(delay);

        if (life <= 0)
        {
            resetAttackCoolDown(true);
            return true;
        }
        return false;
    }
    public bool checkOverkill(float damage)
    {
        if (damage > maxLife && Random.value * 100 < upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.overkillChainChance))
        {
            gameObject.GetComponent<SpriteRenderer>().color = pixelColor;
            gameObject.tag = nameGoodPixel;
            coolDown = 0;
            transformtriggered = true;
            
            world.GetComponent<worldscript>().getNeighbours(gameObject);
            transform.localScale = new Vector2(100, 100);
            lerpList.RemoveAll(x => true);
            tryAttack(damage* upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.overkillChainDamage)/100);
            return true;
        }
        else cascadingDamage = -1;
       
        return false;
    }
    //return: -1=remove,0=nothing
    public int tryAttack(float val)
    {


        if (neighbourCount(nameBadPixel) == 0)
        {
            return -1;
        }
        if (isAnimating() || !canAttack()) return 0;
        int toInfect=chooseNeighbour();
        if (toInfect == -1)
        {
            return -1;
        }
        bool isCrit = false;
    
        float dmg = val==0?world.GetComponent<worldscript>().calculateDamage(out isCrit, AttackType.PIXEL):val;
        bool died = attack(toInfect, dmg, isCrit);
        if (died)
        {
            canvas.GetComponent<MenuManageScript>().changeMoney(getValue());

        }
        return 0;
    }
    public int chooseNeighbour()
    {
        int r = (int)(UnityEngine.Random.value * 4);
        if (r == 4) r = 0;
        for (int i = 0; i < 4; i++)
        {
            int index = (r + i) % 4;
            if ((index == 2 && GetComponent<neighbourScript>().neighbours[index] == null) ||
                index == 2 && GetComponent<neighbourScript>().neighbours[index].name==nameEndPixel) continue;
            if (GetComponent<neighbourScript>().neighbours[index].CompareTag(nameBadPixel))
            {
                return index;
            }
        }

        return -1;
    }
    public float cascadingDamage;
    public static void setAttackSpeed(bool init)
    {
      
        if (upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.pixelAttackSpeed) < attackSpeedAnimationThreshold)
        {
            damageAnimationLength = upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.pixelAttackSpeed);
            transformAnimationLength = upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.pixelAttackSpeed);
            attackAnimationLength = upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.pixelAttackSpeed);

        }
        else
        {
            if (init)
            {
                damageAnimationLength = attackSpeedAnimationThreshold;
                transformAnimationLength = attackSpeedAnimationThreshold;
                attackAnimationLength = attackSpeedAnimationThreshold;
            }
        }
        int i = 0;
    }
}
