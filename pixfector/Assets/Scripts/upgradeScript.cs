using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class upgradeScript
{

    static List<Upgrade> upgrades = new List<Upgrade>();
    static List<GameObject> upgradeButtons = new List<GameObject>();
    public static GameObject scrollViewContent;
    public static GameObject upgradeButton;
    public static GameObject world;
    public static int currentY;
    public static int countButtons;
    public static int margin;
    public static int buttonHeight;
    public static void addUpgrade(float basePrice, float exponentialCost, string upgradeText, string effectText, UpgradeType upgradeType,int maxLevel,int currentLevel)
    {
        Upgrade upgrade = new Upgrade(basePrice, exponentialCost, upgradeText, effectText, upgradeType,maxLevel,currentLevel);
        GameObject btn = GameObject.Instantiate(upgradeButton);
        upgrade.setButton(btn);
        btn.GetComponentInChildren<Text>().text = "ffs";
        btn.transform.parent = scrollViewContent.transform;
 
        
        
     
        currentY += (margin + buttonHeight);
        scrollViewContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, currentY);
        btn.transform.localPosition = new Vector3(0, -currentY);

        


        upgrades.Add(upgrade);
    }

    public static void updateValues()
    {
        foreach (var item in upgrades)
        {
            item.updateButton();
        }
    }
    public static float getUpgradeValue(UpgradeType upgradeType)
    {
        foreach (var item in upgrades)
        {
            if (item.upgradeType == upgradeType) return item.getEffectValue();
        }
        return -1;
    }
    public class Upgrade
    {

        public Upgrade(float basePrice,float exponentialCost, string upgradeText, string effectText, UpgradeType upgradeType,int maxLevel,int currentLevel)
        {
            this.basePrice = basePrice;
            exp = exponentialCost;
            this.maxLevel = maxLevel;
            this.upgradeText = upgradeText;
            this.effectText = effectText;
            this.upgradeType = upgradeType;
            this.level = currentLevel;
        }
        float basePrice;//base Price
        float exp;//expGrowth Price
        float level;//level
        float maxLevel;//maxLevel
        string upgradeText;
        public UpgradeType upgradeType;
        GameObject button;
        string effectText;
        float upgradeCount;
        double currentUgradePrice;
        /*
         * Upgrade Count n and Owned Money c
         * */
        public bool calcUpgradeCost()
        {
            upgradeCount = MenuManageScript.upgradeCount;
            if (upgradeCount == -1)
            {
                upgradeCount = (int)Mathf.Floor(Mathf.Log((float) ((MenuManageScript.money*(exp - 1)) / (basePrice *(Mathf.Pow(exp,level)))) + 1,exp));
                if (upgradeCount == 0) upgradeCount = 1;
            }
            
            if (level + upgradeCount > maxLevel) upgradeCount = maxLevel - level;
            
            currentUgradePrice = basePrice * (Mathf.Pow(exp, level) * ((Mathf.Pow(exp, upgradeCount) - 1) / (exp - 1)));
          
  
            return currentUgradePrice <= MenuManageScript.money;

        }
        static string iconName = "IconUpgrade";
        static string iconCurrencyName = "IconCurrency";
        static string levelName = "TextLevel";
        static string upgradeAmountName = "TextUpgrade";
        static string upgradeCostName = "TextPrice";
        static string upgradeCostImageName = "ImagePrice";
        static string effectName = "TextEffect";
        static string titleName = "TextTitle";

        public void updateButton()
        {
            Text[] texts = button.GetComponentsInChildren<Text>();
            Image img = button.GetComponentInChildren<Image>();
            if (level != maxLevel&&calcUpgradeCost())
            {
                button.GetComponent<Button>().interactable = true;
            }
            else
            {
                button.GetComponent<Button>().interactable = false;
            }
            foreach (Transform child in button.transform)
            {
                
            
                if(child.name==iconName)
                {
                    child.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image " + upgradeText);
                }
                else if (child.name == levelName)
                {
                    child.GetComponent<Text>().text = "Lv. " + level;
                }
                else if (child.name == upgradeCostName)
                {
                    if (level != maxLevel) child.GetComponent<Text>().text = MenuManageScript.getFormattedValue(currentUgradePrice,2);
                    else
                    {
                        child.GetComponent<Text>().enabled = false;
                    }
                }
                else if (child.name == iconCurrencyName)
                {
                    if (level == maxLevel) child.GetComponent<Image>().enabled = false;
                   
                }
                else if (child.name == titleName)
                {
                    child.GetComponent<Text>().text = upgradeText;
                }
                else if (child.name == upgradeAmountName)
                {
                    if(level != maxLevel) child.GetComponent<Text>().text = "Buy " + upgradeCount + " for";
                    else child.GetComponent<Text>().text = "MAXED";
                }
                else if (child.name == effectName)
                {
                    switch (upgradeType)
                    {
                        case UpgradeType.pixelCritChance:
                            {
                                child.GetComponent<Text>().text = MenuManageScript.getFormattedValue(getEffectValue() * 100, 2) + effectText;
                                break;
                            }
                        case UpgradeType.pixelCritDamage:
                            {
                                child.GetComponent<Text>().text = MenuManageScript.getFormattedValue(getEffectValue() * 100, 2) + effectText;
                                break;
                            }
                        default:
                            {

                                child.GetComponent<Text>().text = MenuManageScript.getFormattedValue(getEffectValue(), 2) + effectText;
                                break;
                            }
                    }

                }
            
            }
        }
        public float getEffectValue()
        {
            switch(upgradeType)
            {
                case UpgradeType.pixelAttackSpeed:
                    {
                        //  return -(-24f * level + 5f) / 95f;
                          return (-24f * level + 499f) / 95f;
                       // return 0.02f * level * level - 0.56f * level + 5.54f;

                    }
                case UpgradeType.pixelCritChance:
                    {
                        //  return -(-24f * level + 5f) / 95f;
                        return 0.01f * level;

                    }
                case UpgradeType.pixelCritDamage:
                    {
                        //  return -(-24f * level + 5f) / 95f;
                        return 1 + (level * 0.1f);

                    }
                case UpgradeType.clickRadius:
                    {
                        //  return -(-24f * level + 5f) / 95f;
                        return level/2;

                    }
            }
            return level;
        }
        public void setButton(GameObject button)
        {
            this.button = button;
            button.GetComponent<Button>().onClick.AddListener(clickUpgrade);
        }
        public void clickUpgrade()
        {
            Debug.Log("Clicked on " + upgradeType);
            if (upgradeType == UpgradeType.pixelAttackSpeed)
            {
                float oldAttackSpeed = getEffectValue();
           
                level += upgradeCount;
                MenuManageScript.money -= currentUgradePrice;
                updateButton();
                world.GetComponent<worldscript>().updateAttackSpeedOfPixels(oldAttackSpeed);

                upgradeScript.updateValues();
            }
            else
            {
                level += upgradeCount;
                MenuManageScript.money -= currentUgradePrice;
                upgradeScript.updateValues();
            }
        }
    }

    public enum UpgradeType
    {
        pixelBaseDamage, pixelAttackSpeed, pixelCritChance, pixelCritDamage,
        clickBaseDamage, clickRadius, clickCritChance, clickCritDamage,
        moneyPerKill, overkillChainChance, overkillChainDamage
    }

}
