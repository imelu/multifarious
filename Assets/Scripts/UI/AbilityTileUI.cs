using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class AbilityTileUI : MonoBehaviour
{
    private Image _img;

    private AbilityTileState _state;
    public AbilityTileState state { get { return _state; } set { _state = value; UpdateVisual(); } }

    [SerializeField] private Color _nonSelectable;
    [SerializeField] private Color _selectable;
    [SerializeField] private Color _selected;
    [SerializeField] private Color _selectionFailed;
    private Color _initialHoverColor;
    private Vector3 _initialHoverPosition;

    [SerializeField] private Upgrade _upgrade;
    public Upgrade upgrade { get { return _upgrade; } }

    private Image _hoverSprite;
    private GameObject _stackIndicator;
    private TMP_Text _stackNumber;

    private int _upgradeStackNumber;
    public int upgradeStackNumber { get { return _upgradeStackNumber; } set { _upgradeStackNumber = value; } }

    private int _selectedStackNumber;
    public int selectedStackNumber { get { return _selectedStackNumber; } }

    private void Start()
    {
        _img = GetComponent<Image>();
        _img.sprite = _upgrade.uiSprite;
        _hoverSprite = transform.GetChild(0).GetComponent<Image>();
        _stackIndicator = transform.GetChild(1).gameObject;
        _stackNumber = _stackIndicator.GetComponentInChildren<TMP_Text>();
        _initialHoverColor = _hoverSprite.color;
        _initialHoverPosition = _hoverSprite.GetComponent<RectTransform>().anchoredPosition;

    }

    private void UpdateVisual()
    {
        switch (state)
        {
            case AbilityTileState.nonSelectable:
                _img.color = _nonSelectable;
                break;

            case AbilityTileState.selectable:
                _img.color = _selectable;
                break;

            case AbilityTileState.selected:
                _img.color = _selected;
                break;
        }
        _hoverSprite.enabled = false;
    }

    public void Hover()
    {
        _hoverSprite.enabled = true;
    }

    public void ReleaseHover()
    {
        _hoverSprite.enabled = false;
    }

    public void SetStack(int stackNumber)
    {
        if (stackNumber > 0 && !_stackIndicator.activeInHierarchy && upgradeStackNumber > 1 && upgrade.isStackable) _stackIndicator.SetActive(true);
        else if (stackNumber == 0 && _stackIndicator.activeInHierarchy) _stackIndicator.SetActive(false);

        _stackNumber.text = stackNumber.ToString();
        _selectedStackNumber = stackNumber;
    }

    public void SelectionFailed()
    {
        DOTween.Kill(_hoverSprite);
        DOTween.Kill(_hoverSprite.transform);
        _hoverSprite.GetComponent<RectTransform>().anchoredPosition = _initialHoverPosition;
        //_hoverSprite.transform.localScale = Vector3.one;
        _hoverSprite.DOColor(_selectionFailed, 0.2f).OnComplete(() => _hoverSprite.DOColor(_initialHoverColor, 0.2f));
        _hoverSprite.transform.DOShakePosition(0.3f, new Vector3(6f, 0, 0), 20, 20, false, false, ShakeRandomnessMode.Full);
        //_hoverSprite.transform.DOShakeScale(0.3f, 0.2f, 10, 20, true, ShakeRandomnessMode.Full);
    }
}
