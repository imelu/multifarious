using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

public class ResourceUI : MonoBehaviour
{
    [SerializeField] private List<TMP_Text> _resourceCostTexts = new List<TMP_Text>();

    [SerializeField] private List<Image> _resourceBars = new List<Image>();
    [SerializeField] private List<Image> _resourceBars2 = new List<Image>();

    public void SetResourceUIs(ResourceManager.ResourceCount[] cost, bool isNegative)
    {
        for (int i = 0; i < cost.Length; i++)
        {
            if(isNegative && cost[i].count > 0) _resourceCostTexts[i].text = "-" + cost[i].count.ToString();
            else _resourceCostTexts[i].text = cost[i].count.ToString();
        }
    }

    public void SetResourceUIs(ResourceManager.ResourceCount[] cost, ResourceManager.ResourceCount[] total, bool outOfTotal)
    {
        for(int i = 0; i < cost.Length; i++)
        {
            if(outOfTotal)_resourceCostTexts[i].text = "<b>" + cost[i].count.ToString() + "</b>/" + total[i].count.ToString();
            else _resourceCostTexts[i].text = cost[i].count.ToString();

            _resourceBars[i].fillAmount = (float)cost[i].count / (float)total[i].count;
        }
    }

    public void SetResourceUIs(ResourceManager.ResourceCount[] currentCost, ResourceManager.ResourceCount[] changeInCost, ResourceManager.ResourceCount[] total, bool isTotal)
    {
        for (int i = 0; i < currentCost.Length; i++)
        {
            //if(isTotal)_resourceCostTexts[i].text = total[i].count.ToString();
            //else _resourceCostTexts[i].text = (currentCost[i].count + changeInCost[i].count).ToString();

            _resourceCostTexts[i].text = "<b>"+(currentCost[i].count + changeInCost[i].count).ToString() + "</b>/" + total[i].count.ToString();

            if (changeInCost[i].count > 0)
            {
                _resourceBars[i].fillAmount = (float)currentCost[i].count / (float)total[i].count;
                _resourceBars2[i].fillAmount = _resourceBars[i].fillAmount + (float)changeInCost[i].count / (float)total[i].count;
            }
            else
            {
                _resourceBars[i].fillAmount = (float)(currentCost[i].count + changeInCost[i].count) / (float)total[i].count;
                _resourceBars2[i].fillAmount = (float)currentCost[i].count / (float)total[i].count;
            }
            
        }
    }
}
