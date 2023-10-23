using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeSelectionCard : MonoBehaviour
{
    public Upgrade upgrade;
    [SerializeField] private TMP_Text _upgradeName;
    [SerializeField] private List<Image> _checkMarks;
    public List<Image> checkMarks { get { return _checkMarks; } }
    [SerializeField] private List<Image> _checkBoxSelectors;
    [SerializeField] private ResourceUI _upgradeCost;
    [SerializeField] private Image _hoverBG;

    public void DisplayUpgrade(Upgrade _upgrade)
    {
        if (_upgrade == upgrade) return;
        upgrade = _upgrade;
        _upgradeName.text = upgrade.upgradeName;
        //_upgradeCost.SetResourceUIs(upgrade.resourceCost);
        _checkMarks[0].enabled = upgrade.selectedP1 > 0;
        _checkMarks[1].enabled = upgrade.selectedP2 > 0;
    }

    public void Select(int playerIndex)
    {
        _checkMarks[playerIndex].enabled = !_checkMarks[playerIndex].enabled;
    }

    public void Hovered(int playerIndex)
    {
        _hoverBG.enabled = true;
        foreach (Image img in _checkBoxSelectors) img.enabled = false;
        _checkBoxSelectors[playerIndex].enabled = true;
    }

    public void HoverRelease()
    {
        _hoverBG.enabled = false;
        foreach (Image img in _checkBoxSelectors) img.enabled = false;
    }
}
