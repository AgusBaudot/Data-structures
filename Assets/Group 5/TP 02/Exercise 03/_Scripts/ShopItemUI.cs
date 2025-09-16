using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _priceText;
    private Image _icon;
    private Button _buyButton;

    private ItemSO _item;
    private Shop _shop;
    private bool _purchased;
    private Sprite _soldSprite;

    public void Setup(ItemSO item, Shop shop)
    {
        Debug.Log("Setup");
        #region Cache references
        _nameText = transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        _priceText =  transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();
        _icon = GetComponentInChildren<Image>();
        _buyButton = GetComponentInChildren<Button>();
        #endregion
        
        _item = item;
        _shop = shop;

        
        _nameText.text = item.Name;
        _priceText.text = item.Price.ToString();
        _icon.sprite = item.Icon;

        //_soldSprite = ItemSODatabase.SoldItemSOs[item.Type];
        
        _buyButton.onClick.RemoveAllListeners();
        _buyButton.onClick.AddListener(() => OnBuyClicked());
    }

    private void OnBuyClicked()
    {
        if (!_purchased)
        {
            if (!_shop.Buy(_item)) return;
            _buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Sell!";
        }
        else
        {
            _shop.Sell(_item);
            _buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Buy!";
        }

        _purchased = !_purchased;
    }
    
    public int ItemID => _item.ID;

    //private void Purchased(ItemSO ItemSO)
    //{
    //    if (!_purchased)
    //    {
    //        if (!_shop.Buy(ItemSO)) return;
    //        _buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Sell!";
    //        _icon.sprite = _soldSprite;
    //    }
    //    else
    //    {
    //        _buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Buy!";
    //        _icon.sprite = ItemSO.Icon;
    //        _shop.Sell(ItemSO);
    //    }
    //    _purchased = !_purchased;
    //}
}