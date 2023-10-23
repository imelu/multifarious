using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AbilityWheel : MonoBehaviour, IMenu
{
    [SerializeField] private Transform AbilityParent;

    private List<Image> _radialImages = new List<Image>();
    private List<Image> _abilityIcons = new List<Image>();

    private int _hoverIndex;

    [SerializeField] private Color _hoverCol;
    [SerializeField] private Color _normalCol;
    [SerializeField] private Color _selectedCol;

    private int _abilityCount;
    private bool _selected;

    private void Start()
    {
        foreach(Transform child in AbilityParent.transform)
        {
            _radialImages.Add(child.GetComponent<Image>());
            _abilityIcons.Add(child.GetChild(0).GetComponentInChildren<Image>());
        }
    }

    public void DisplayAbilities(List<IDefenseAbility> abilities)
    {
        _selected = false;
        _abilityCount = abilities.Count;
        foreach (Image img in _radialImages) img.gameObject.SetActive(false);
        for (int i = 0; i < abilities.Count; i++)
        {
            _radialImages[i].gameObject.SetActive(true);
            _abilityIcons[i].sprite = abilities[i].sprite;
            _radialImages[i].fillAmount = 1 / (float)abilities.Count;
            _radialImages[i].transform.localEulerAngles = new Vector3(0, 0, -i * 360 / abilities.Count);
            _abilityIcons[i].transform.parent.localEulerAngles = new Vector3(0, 0, -180 / abilities.Count);
            _abilityIcons[i].GetComponent<RectTransform>().localPosition = new Vector3(0, 150, 0);
            _radialImages[i].color = _normalCol;
        }
    }

    public int SelectAbility()
    {
        _selected = true;
        _radialImages[_hoverIndex].color = _selectedCol;
        return _hoverIndex;
    }

    private void ReleaseHover()
    {
        _radialImages[_hoverIndex].color = _normalCol;
    }

    private void HoverSelection()
    {
        _radialImages[_hoverIndex].color = _hoverCol;
    }

    public void Direction(Vector3 direction)
    {
        if (_abilityCount == 0 || _selected) return;
        direction = Quaternion.AngleAxis(180, Vector3.up) * direction;
        float angle = 180 + (Mathf.Atan2(direction.x, direction.z)) * 180 / Mathf.PI;
        if(angle == 180 && _hoverIndex >= 0)
        {
            /*ReleaseHover();
            _hoverIndex = -1;
            return;*/
        }
        float valPerAbility = 360 / _abilityCount;
        int hoveredAbility = Mathf.FloorToInt(angle / valPerAbility);
        if(hoveredAbility != _hoverIndex)
        {
            if(_hoverIndex >= 0) ReleaseHover();
            _hoverIndex = hoveredAbility;
            HoverSelection();
        }
    }

    public void Cancel()
    {
        GetComponent<CanvasGroup>().alpha = 0;
    }

    public void Confirm()
    {
    }

    public void Down()
    {
    }

    public void Left()
    {
    }

    public void Right()
    {
    }

    public void Up()
    {
    }
}
