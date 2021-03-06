﻿
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
public class worldscript : MonoBehaviour
{

    // Use this for initialization



    //  public float damage;
    // public float critChance;
    //  public float critMultiplyer;
    //   public float attackSpeed;
    public float attackSpeedDistributionModifier;
    public int width;
    int lowerBound = 0;
    public int upperBound = 0;
    public Color pixelColor;
    public Color backgroundPixelColor;

    public bool loadGame = false;
    public float damageAnimationLength;
    public float attackAnimationLength;
    public float transformAnimationLength;
    public float attackAnimationStretch;
    public float maxLifeDistributionModifier;
    public float attackReposValue;
    public float vertLifeIncreaseModifier;
    public float pixelValueModifier;
    public GameObject pixelPrefab;
    GameObject oldPicBgPrefab;
    public GameObject bgPicPrefab;
    public GameObject bgPixelPrefab;
    public Camera cam;
    public GameObject fakePixels;
    public GameObject canvas;
    public GameObject background;
    public GameObject endPixel;
    GameObject removeRowCanditate;
    public double initMoney;

    //public Camera camera;
    public Dictionary<Vector2, GameObject> pixels = new Dictionary<Vector2, GameObject>();
    List<GameObject> infectPixels = new List<GameObject>();
    List<GameObject> tempPixels = new List<GameObject>();
    Texture2D bgTexture;
    public GameObject rainbowBackground;

    public string nameGoodPixel;
    public string nameBadPixel;
    public string nameEndPixel;



    void Start()
    {
        
        canvas.GetComponent<MenuManageScript>().buildUpgradeMenu();
        TextManager.Initialize();
        setPixelColor(pixelColor);
        setPixelScriptValues();
        canvas.GetComponent<MenuManageScript>().setSliders(pixelColor);
        bgTexture = rainbowBackground.GetComponent<SpriteRenderer>().sprite.texture;

        background.transform.localScale = new Vector3(fakePixels.transform.localScale.x, 10 * upperBound, 1);


        if (loadGame)
        {

        }
        else
        {

            for (int height = 0; height <= upperBound; height++)
            {
                createRow(height);

            }
            for (int i = 0; i < width; i += 2)
            {
                GameObject start;
                pixels.TryGetValue(new Vector2(i, 0), out start);
                start.GetComponent<SpriteRenderer>().color = pixelColor;
                start.tag = nameGoodPixel;
                infectPixels.Add(start);
                start.transform.Translate(new Vector3(0, 0, -1));
            }


        }
        canvas.GetComponent<MenuManageScript>().changeMoney(initMoney);

    }
    public void updateAttackSpeedOfPixels(float oldAttackSpeed)
    {
        float newAttackSpeed = upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.pixelAttackSpeed);
        foreach (var item in infectPixels)
        {

            float cooldown = item.GetComponent<pixelScript>().coolDown;
            float newCoolDown = cooldown * newAttackSpeed / oldAttackSpeed;
            item.GetComponent<pixelScript>().coolDown = newCoolDown;


        }
        pixelScript.setAttackSpeed(false);
    }
    public void setPixelColor(Color color)
    {
        pixelScript.pixelColor = color;
        fakePixels.GetComponent<SpriteRenderer>().color = color;
        foreach (var item in pixels)
        {
            if (item.Value == null)
            {
                Debug.Log("Already Destroyed: " + item.Key.ToString());
            }
            else if (item.Value.tag == nameGoodPixel)
            {
                item.Value.GetComponent<SpriteRenderer>().color = color;
            }
        }
    }
    private void applyClickDamage(int distance)
    {

        Vector2 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
        pos = cam.ScreenToWorldPoint(pos);
        pos = new Vector2((int)Math.Round(pos.x), (int)Math.Round(pos.y));

        GameObject center;
        bool ok = pixels.TryGetValue(pos, out center);
        if (!ok) return;
        center.name = "Center";
        int x = -distance;
        int y = -distance;
        
        GameObject affectedPixel = getPixelRelativeToPixel(center, ref x, ref y);
        int sizeX = distance * 2 + 1+x;
        int sizeY = distance * 2 + 1+y;
        if(affectedPixel.transform.position.x+sizeX>=width)
        {
            sizeX -= (int)(affectedPixel.transform.position.x + sizeX- width);
        }
        List<KeyValuePair<float, GameObject>> affectedPixels = new List<KeyValuePair<float, GameObject>>();
        bool toLeft = false;
        for (int j = 0; j < sizeY; j++)
        {

            //das problem so loesen: es wird ein rechteck bestimmt, welches am besten passt
            for (int i = 0; i < sizeX; i++)
            {

                affectedPixels.Add(new KeyValuePair<float, GameObject>(Vector2.Distance(center.transform.position, affectedPixel.transform.position), affectedPixel));
                if (i != sizeX - 1)
                {
                    if (toLeft)
                    {
                        affectedPixel = affectedPixel.GetComponent<neighbourScript>().neighbours[3];
                    }
                    else
                    {
                        affectedPixel = affectedPixel.GetComponent<neighbourScript>().neighbours[1];
                    }
                }

            }

            affectedPixels.Add(new KeyValuePair<float, GameObject>(Vector2.Distance(center.transform.position, affectedPixel.transform.position), affectedPixel));
            affectedPixel = affectedPixel.GetComponent<neighbourScript>().neighbours[0];
            if (affectedPixel.tag==nameEndPixel) break;
            toLeft = !toLeft;


        }
        affectedPixels = affectedPixels.OrderBy(xx => xx.Key).ToList();
        foreach (var item in affectedPixels)
        {
            if (item.Key > distance) continue;
            //   item.Value.GetComponent<>
            bool isCrit;
            float delay = item.Key / 10f;
            if (item.Value.GetComponent<pixelScript>().tryToDamage(calculateDamage(out isCrit, AttackType.CLICK), isCrit, delay))
            {
                canvas.GetComponent<MenuManageScript>().changeMoney(item.Value.GetComponent<pixelScript>().getValue());
                getNeighbours(item.Value);
            }
        }
    }

    private GameObject getPixelRelativeToPixel(GameObject pixel, ref int x, ref int y)
    {
        if (x > 0 || y > 0)
        {
            Debug.LogError("Error: relatedPixel not leftBottom");
        }
        while (x != 0)
        {
           
            if (pixel.GetComponent<neighbourScript>().neighbours[3].tag == nameEndPixel) break;
            pixel = pixel.GetComponent<neighbourScript>().neighbours[3];
            x++;
        }
        while (y != 0)
        {
           
            if (pixel.GetComponent<neighbourScript>().neighbours[2].tag == nameEndPixel) return pixel;
            pixel = pixel.GetComponent<neighbourScript>().neighbours[2];
            y++;
        }
        return pixel;
    }
    bool isTimeScale = false;
    public bool hasDragged;
    private void Update()
    {

        if (!hasDragged&&Input.GetMouseButtonUp(0)&&!EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Apply Click Dmg");
            applyClickDamage((int)upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.clickRadius));
        }
        if (Input.GetMouseButtonDown(0)) hasDragged = false;
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            Time.timeScale = isTimeScale?0.5f:1f;
            isTimeScale = !isTimeScale;
        }
            
        List<GameObject> toRemove = new List<GameObject>();
        int initInfCount = infectPixels.Count;
        for (int i = 0; i < initInfCount; i++)
        {
            if(infectPixels[i]==null)
            {
                toRemove.Add(infectPixels[i]);
                continue;
            }
            if (infectPixels[i].GetComponent<pixelScript>().tryAttack(0)==-1)
            {
                toRemove.Add(infectPixels[i]);
            }
        }
        foreach (var item in toRemove)
        {

            if (item != null) item.GetComponent<SpriteRenderer>().color = pixelScript.pixelColor;
            infectPixels.Remove(item);
        }
        toRemove = null;

        tryRemoveRow();




    }
    public float calculateDamage(out bool isCrit, AttackType type)
    {
        float dmg = type == AttackType.PIXEL ? upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.pixelBaseDamage) : upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.clickBaseDamage);
       // dmg *= 10000000000000f;
        if (UnityEngine.Random.value < (type == AttackType.PIXEL ? upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.pixelCritChance) : upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.clickCritChance)))
        {
            dmg *= type == AttackType.PIXEL ? upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.pixelCritDamage) : upgradeScript.getUpgradeValue(upgradeScript.UpgradeType.clickCritDamage);
            isCrit = true;
            // cam.GetComponent<cameraShake>().shakeDuration = 0.2f;
            //cam.GetComponent<cameraShake>().shakeAmount = 0.3f;
        }
        else isCrit = false;
        return dmg;
    }
   
    private GameObject createPixel(float x, float y, string tagName)
    {
        GameObject p = GameObject.Instantiate(pixelPrefab, new Vector3(x, y), Quaternion.identity);
        pixels.Add(new Vector2(x, y), p);

        p.GetComponent<pixelScript>().setTag(tagName);
        Color cRainbow = bgTexture.GetPixel(1, (int)y % 400);
        p.GetComponent<SpriteRenderer>().color = cRainbow;
        p.GetComponent<pixelScript>().bgColor = new Color(cRainbow.r / 2, cRainbow.g / 2, cRainbow.b / 2);
        if (y == lowerBound && removeRowCanditate == null)
        {
            removeRowCanditate = p;
        }

        getNeighbours(p);
        return p;

    }
    private void createRow(int height)
    {
        for (int i = 0; i < width; i++)
        {
            GameObject p=createPixel(i, height, nameBadPixel);
         //   p.GetComponent<neighbourScript>().neighbours[2].GetComponent<neighbourScript>().neighbours[0] = p;
        }
    }
    private bool isRowFull(GameObject p)
    {

        if (p == null) return false;
        if (p.CompareTag(nameEndPixel)) return false;
        GameObject rRef = p;
        GameObject lRef = p;
        int c = 1;
        while (rRef.GetComponent<neighbourScript>().neighbours[1] != null &&
            !rRef.GetComponent<neighbourScript>().neighbours[1].CompareTag(nameEndPixel) &&
            !rRef.GetComponent<pixelScript>().isAnimating() &&
            rRef.GetComponent<neighbourScript>().neighbours[1].CompareTag(nameGoodPixel))
        {
            c++;
            rRef = rRef.GetComponent<neighbourScript>().neighbours[1];
        }
        while (lRef.GetComponent<neighbourScript>().neighbours[3] != null &&
            !lRef.GetComponent<neighbourScript>().neighbours[3].CompareTag(nameEndPixel) &&
            !lRef.GetComponent<pixelScript>().isAnimating() &&
            lRef.GetComponent<neighbourScript>().neighbours[3].CompareTag(nameGoodPixel))
        {
            c++;
            lRef = lRef.GetComponent<neighbourScript>().neighbours[3];

        }
        if (c == width)
        {
            return true;
        }
        return false;

    }
    private bool tryRemoveRow()
    {

        if (isRowFull(removeRowCanditate))
        {
            if (isRowFull(removeRowCanditate.GetComponent<neighbourScript>().neighbours[0]))
            {
                removeRow(removeRowCanditate);
                tryRemoveRow();
                return true;
            }
        }
        return false;
    }

    private void removeRow(GameObject p)
    {
        removeRowCanditate = removeRowCanditate.GetComponent<neighbourScript>().neighbours[0];
        GameObject lRef = p;

        GameObject rRef = p;

        while (!rRef.GetComponent<neighbourScript>().neighbours[1].CompareTag(nameEndPixel))
        {
            GameObject rm = rRef;
            rRef = rRef.GetComponent<neighbourScript>().neighbours[1];
            rm.GetComponent<neighbourScript>().neighbours[0].GetComponent<neighbourScript>().neighbours[2]=endPixel ;
            killPixel(rm);
        }

        while (!lRef.GetComponent<neighbourScript>().neighbours[3].CompareTag(nameEndPixel))
        {
            GameObject rm = lRef;
            lRef = lRef.GetComponent<neighbourScript>().neighbours[3];
            rm.GetComponent<neighbourScript>().neighbours[0].GetComponent<neighbourScript>().neighbours[2] = endPixel;
            killPixel(rm);
        }
        killPixel(lRef);
        killPixel(rRef);
        killPixel(p);
        lowerBound++;
        cam.GetComponent<cameraScript>().moveUp();
        upperBound++;
        createRow(upperBound);
        if (lowerBound % 400 == 250)
        {
            if (oldPicBgPrefab != null) Destroy(oldPicBgPrefab);
            oldPicBgPrefab = bgPicPrefab;
            bgPicPrefab = Instantiate(bgPicPrefab);
            bgPicPrefab.transform.parent = transform;
            bgPicPrefab.transform.Translate(new Vector3(0, 399.5f));

            cam.GetComponent<cameraScript>().upperBound += 400;
        }
        updatePositioning();
    }
    private void killPixel(GameObject p)
    {
        p.GetComponent<pixelScript>().prepareForKill();
        pixels.Remove(new Vector2(p.transform.position.x, p.transform.position.y));
        Destroy(p);
    }
    private bool isProtected(GameObject p)
    {
        //   if (p.transform.position.y <= progress) return true;
        return false;
    }

    public void getNeighbours(GameObject p)
    {
        int x = (int)p.transform.position.x;
        int y = (int)p.transform.position.y;
        bool[] borders = isBorderPixel(p);
        for (int i = 0; i < borders.Length; i++)
        {
            if(borders[i])p.GetComponent<neighbourScript>().neighbours[i] = endPixel;
        }

        GameObject neigbour;
        bool found = pixels.TryGetValue(new Vector2(x, y + 1), out neigbour);
        if (found)
        {

            p.GetComponent<neighbourScript>().neighbours[0] = neigbour;
            neigbour.GetComponent<neighbourScript>().neighbours[2] = p;
        }
        found = pixels.TryGetValue(new Vector2(x + 1, y), out neigbour);
        if (found)
        {

            p.GetComponent<neighbourScript>().neighbours[1] = neigbour;
            neigbour.GetComponent<neighbourScript>().neighbours[3] = p;
        }
        found = pixels.TryGetValue(new Vector2(x, y - 1), out neigbour);
        if (found)
        {

            p.GetComponent<neighbourScript>().neighbours[2] = neigbour;
            neigbour.GetComponent<neighbourScript>().neighbours[0] = p;
        }
        found = pixels.TryGetValue(new Vector2(x - 1, y), out neigbour);
        if (found)
        {

            p.GetComponent<neighbourScript>().neighbours[3] = neigbour;
            neigbour.GetComponent<neighbourScript>().neighbours[1] = p;
        }

        if (!p.GetComponent<pixelScript>().CompareTag(nameBadPixel)) infectPixels.Add(p);
    }

    private bool[] isBorderPixel(GameObject p)
    {
        bool[] borders = new bool[] {false,false,false,false};
        float x = p.transform.position.x;
        float y = p.transform.position.y;
        if (y == upperBound) borders[0] = true;
        if (x == width - 1) borders[1] = true;
        if (y == 0) borders[2] = true;
        if (x == 0) borders[3] = true;




        return borders;
    }



    public void updatePositioning()
    {
        cam.GetComponent<cameraScript>().lowerBound++;

        fakePixels.transform.localScale = new Vector3(fakePixels.transform.localScale.x, 10 * lowerBound + 1, 0);
        background.transform.localScale = new Vector3(fakePixels.transform.localScale.x, 10 * upperBound, 1);


    }
    public void removeBackgroundPixels()
    {
        foreach (var item in pixels)
        {
            Destroy(item.Value.GetComponent<pixelScript>().bgInstance);
        }
    }
    private void setPixelScriptValues()
    {
        pixelScript.pixelColor = pixelColor;
        pixelScript.bgPrefab = bgPixelPrefab;
        pixelScript.nameGoodPixel = nameGoodPixel;
        pixelScript.nameBadPixel = nameBadPixel;
        pixelScript.nameEndPixel = nameEndPixel;
        pixelScript.world = this.gameObject;
        pixelScript.canvas = this.canvas;
        pixelScript.attackReposValue = attackReposValue;
        pixelScript.attackSpeedDistributionModifier = attackSpeedDistributionModifier;
        pixelScript.attackAnimationStretch = attackAnimationStretch;
        pixelScript.maxLifeDistributionModifier = maxLifeDistributionModifier;
        pixelScript.width = width;
        pixelScript.vertLifeIncreaseModifier = vertLifeIncreaseModifier;

        pixelScript.setAttackSpeed(true);

    }
}

public enum AttackType
{
    CLICK, PIXEL
}