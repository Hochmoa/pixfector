using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuManageScript : MonoBehaviour
{

    public GameObject pixelPanel;
    public GameObject btnOptions;
    public GameObject btnBuy;
    public GameObject darkenBackground;
    public GameObject moneyText;
    Dictionary<Vector2, GameObject> panel = new Dictionary<Vector2, GameObject>();
    int panelX = 4;
    int panelY = 8;
    List<Lerp> lerpList = new List<Lerp>();
    PanelType currentPanel = PanelType.NONE;
    public Color darkenBGColor;
    float pixelShowDelay = 0.03f;
    float panelAnimationSpeed = 0.4f;
    public Camera camera;
    public GameObject toggleSoundButton;
    public GameObject toggleSoundText;
    bool soundOn;

    public GameObject toggleMusicButton;
    public GameObject toggleMusicText;
    bool musicOn;

    public GameObject sliderR;
    public GameObject sliderG;
    public GameObject sliderB;

    public GameObject examplePixel;
    public GameObject world;

    public GameObject panelContentOptions;
    public GameObject panelContentBuy;

    public static double money;

    public static int upgradeCount=1;

    public GameObject scrollViewContent;
    public GameObject upgradeButton;
    public GameObject switchUpgradeCountButton;

    // Use this for initialization
    void Start()
    {
        
        foreach (Transform child in pixelPanel.transform)
        {
            float x = (child.position.x - 270) / 180;
            float y = (child.position.y - 330) / 180;
           // Debug.Log("Saving :" + x + "|" + y);
            panel.Add(new Vector2(x, y), child.gameObject);
            child.localScale = Vector3.zero;
        }
        pixelPanel.SetActive(false);
        darkenBackground.SetActive(false);
        changeMoney(1000000);
    }
    public void switchUpgradeCount()
    {
        if(upgradeCount==1)
        {
            upgradeCount = 10;
            switchUpgradeCountButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "Buy 10";
        }
        else if (upgradeCount == 10)
        {
            upgradeCount = 100;
            switchUpgradeCountButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "Buy 100";
        }
        else if (upgradeCount == 100)
        {
            upgradeCount = -1;
            switchUpgradeCountButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "Buy max";
        }
        else if (upgradeCount == -1)
        {
            upgradeCount = 1;
            switchUpgradeCountButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "Buy 1";
        }
        upgradeScript.updateValues();
    }

    internal void setSliders(Color c)
    {
        sliderR.GetComponent<UnityEngine.UI.Slider>().value = c.r;
        sliderB.GetComponent<UnityEngine.UI.Slider>().value = c.g;
        sliderB.GetComponent<UnityEngine.UI.Slider>().value = c.b;
    }

    public void buildUpgradeMenu()
    {
        upgradeScript.scrollViewContent = scrollViewContent;
        upgradeScript.upgradeButton = upgradeButton;
        upgradeScript.world = world;
        upgradeScript.countButtons = 8;
        upgradeScript.buttonHeight = 257;
        upgradeScript.margin = 10;
      //  upgradeScript.currentY = upgradeScript.buttonHeight;
        upgradeScript.addUpgrade(1, 1.07f, "Pixel Damage", " Damage", upgradeScript.UpgradeType.pixelBaseDamage,999999999);
        upgradeScript.addUpgrade(10, 1.07f, "Pixel Attack Speed", " s", upgradeScript.UpgradeType.pixelAttackSpeed,20);
        upgradeScript.addUpgrade(100, 1.07f, "Pixel Crit Chance", " %", upgradeScript.UpgradeType.pixelCritChance, 999999999);
        upgradeScript.addUpgrade(1000, 1.07f, "Pixel Crit Damage", " %", upgradeScript.UpgradeType.pixelCritDamage, 999999999);
        upgradeScript.addUpgrade(4, 1.07f, "Click Damage", " Damage", upgradeScript.UpgradeType.clickBaseDamage, 999999999);
        upgradeScript.addUpgrade(400, 1.07f, "Click Crit Chance", "  %", upgradeScript.UpgradeType.clickCritChance, 999999999);
        upgradeScript.addUpgrade(4000, 1.07f, "Click Crit Damage", " %", upgradeScript.UpgradeType.clickCritDamage, 999999999);
        upgradeScript.addUpgrade(4000, 1.07f, "Click Radius", " Tiles", upgradeScript.UpgradeType.clickRadius, 5);
        upgradeScript.addUpgrade(4000, 1.07f, "Pixel Value Increase", " %", upgradeScript.UpgradeType.moneyPerKill, 999999999);
        upgradeScript.updateValues();
    }

    /*Sets money and displays instantly, use for load*/

    /*Sets Money and changes display dynamically*/
    public void changeMoney(double val)
    {
        money += val;
        moneyText.GetComponent<UnityEngine.UI.Text>().text = getFormattedValue(money, 2);
        upgradeScript.updateValues();
    }

    public static string getFormattedValue(double val, int floatingPositions)
    {
        if (val < 1000)
        {
            return Math.Round(val, floatingPositions)+"";
        }
        else if (val < 1000000)
        {
            return Math.Round(val / 1000, floatingPositions) + " k";
        }
        else if (val < 1000000000)
        {
            return Math.Round(val / 1000000, floatingPositions) + " m";
        }
        else if (val < 1000000000000)
        {
            return Math.Round(val / 1000000000, floatingPositions) + " b";
        }
        else if (val < 1000000000000000)
        {
            return Math.Round(val / 1000000000000, floatingPositions) + " t";
        }
        return "-1";

    }


    // Update is called once per frame
    void Update()
    {

   
        manageLerp();

    }
    public void manageLerp()
    {
        if (lerpList.Count != 0)
        {
            List<Lerp> toRemove = new List<Lerp>();
            List<Lerp.LerpResult> results = new List<Lerp.LerpResult>();

            foreach (var item in lerpList)
            {
                item.addTime(Time.deltaTime);
                Lerp.LerpResult result = item.lerp();
                if (result != Lerp.LerpResult.NOTFINISHED)
                {
                    toRemove.Add(item);
                    results.Add(result);

                }

            }
            foreach (var item in toRemove)
            {
                lerpList.Remove(item);
            }
            handleLerpResults(results);
        }
    }
    public void handleLerpResults(List<Lerp.LerpResult> results)
    {
        bool fadedInMenu = false;
        bool disabledBackground = false;
        foreach (var result in results)
        {
            switch (result)
            {
                case Lerp.LerpResult.DISABLEBACKGROUD:
                    {
                        if (disabledBackground) continue;
                        disabledBackground = true;
                        darkenBackground.SetActive(false);
                        break;
                    }
                case Lerp.LerpResult.SHOWMENU:
                    {
                        if (fadedInMenu) continue;
                        fadedInMenu = true;
                        if (currentPanel == PanelType.OPTIONS)
                        {
                            panelContentOptions.SetActive(true);
                        }
                        else if (currentPanel == PanelType.BUY)
                        {
                            panelContentBuy.SetActive(true);
                        }
                        break;
                    }

            }
        }
    }
    public void clickOnOptionsBtn()
    {
        if (isAnimatingPanel()) return;
        if (currentPanel == PanelType.NONE)
        {
            pixelPanel.SetActive(true);
            currentPanel = PanelType.OPTIONS;
            //    panelContentOptions.SetActive(true);
            showPanel(true);
        }
    }
    public void clickOnBuyButton()
    {
        if (isAnimatingPanel()) return;
        if (currentPanel == PanelType.NONE)
        {
            pixelPanel.SetActive(true);
            currentPanel = PanelType.BUY;
            showPanel(true);
        }
    }
    public void showPanel(bool show)
    {
        float val = 0;
        int start = panelY - 1;
        int incr = -1;
        int limit = -1;
        camera.GetComponent<cameraScript>().movementDisabled = show;
        if (!show)
        {
            start = 0;
            incr = 1;
            limit = panelY;
            lerpList.Add(new Lerp(darkenBackground.GetComponent<UnityEngine.UI.Image>(), Lerp.AnimationType.COLOR, Lerp.FunctionType.LINEAR, darkenBGColor, Color.clear, 0.5f, false, 0, Lerp.LerpResult.DISABLEBACKGROUD));
            panelContentOptions.SetActive(false);
            panelContentBuy.SetActive(false);

        }
        else
        {
            lerpList.Add(new Lerp(darkenBackground.GetComponent<UnityEngine.UI.Image>(), Lerp.AnimationType.COLOR, Lerp.FunctionType.LINEAR, Color.clear, darkenBGColor, 0.5f, false, 0, Lerp.LerpResult.NONE));
            darkenBackground.SetActive(true);

        }
        for (int i = start; i != limit; i += incr)
        {
            for (int j = 0; j < panelX; j++)
            {
                GameObject item;

                bool found = panel.TryGetValue(new Vector2(j, i), out item);
                if (show) lerpList.Add(new Lerp(item.transform, Lerp.VectorType.SCALE, Lerp.FunctionType.SMOOTHIO, Vector3.zero, transform.localScale, panelAnimationSpeed, false, Lerp.AnimationType.TRANSFORM, val, Lerp.LerpResult.SHOWMENU));
                else
                {

                    lerpList.Add(new Lerp(item.transform, Lerp.VectorType.SCALE, Lerp.FunctionType.SMOOTHIO, transform.localScale, Vector3.zero, panelAnimationSpeed, false, Lerp.AnimationType.TRANSFORM, val, Lerp.LerpResult.NONE));

                }
            }
            val += pixelShowDelay;
        }
    }

    public void clickOnBackground()
    {
        if (isAnimatingPanel()) return;
        if (currentPanel != PanelType.NONE)
        {
            currentPanel = PanelType.NONE;
            showPanel(false);
        }
    }
    public bool isAnimatingPanel()
    {
        return lerpList.Count != 0;
    }
    enum PanelType
    {
        NONE, OPTIONS, BUY
    }
    public void toggleSound()
    {
        soundOn = !soundOn;
        String file = soundOn ? "soundOn" : "soundOff";
        toggleSoundText.GetComponent<UnityEngine.UI.Text>().text = soundOn ? "Sound is on" : "Sound is off";
        toggleSoundButton.GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>(file);
    }
    public void toggleMusic()
    {
        musicOn = !musicOn;
        String file = musicOn ? "musicOn" : "musicOff";
        toggleMusicText.GetComponent<UnityEngine.UI.Text>().text = musicOn ? "Music is on" : "Music is off";
        toggleMusicButton.GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>(file);
    }
    public void pixelColorChanged()
    {

        float r = sliderR.GetComponent<UnityEngine.UI.Slider>().value;
        float g = sliderG.GetComponent<UnityEngine.UI.Slider>().value;
        float b = sliderB.GetComponent<UnityEngine.UI.Slider>().value;
        Color newColor = new Color(r, g, b);
        examplePixel.GetComponent<UnityEngine.UI.Image>().color = newColor;
        world.GetComponent<worldscript>().setPixelColor(newColor);
    }

    public void setUpgradeButtons()
    {

    }
}

