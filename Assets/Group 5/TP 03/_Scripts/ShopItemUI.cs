using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _priceText;
    private Image _icon;
    private Button _buyButton;

    private Item _item;
    private Shop _shop;
    private bool _purchased;
    private Sprite _soldSprite;

    public void Setup(Item item, Shop shop)
    {
        #region Cache references
        _nameText = transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        _priceText =  transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();
        _icon = GetComponentInChildren<Image>();
        _buyButton = GetComponentInChildren<Button>();
        #endregion
        
        _item = item;
        _shop = shop;

        _icon.sprite = item.Icon;
        
        _nameText.text = item.Name;
        _priceText.text = item.Price.ToString();

        _soldSprite = ItemDatabase.SoldItems[item.Type];
        
        _buyButton.onClick.RemoveAllListeners();
        _buyButton.onClick.AddListener(() => Purchased(_item));
    }
    
    public int ItemID => _item.ID;

    private void Purchased(Item item)
    {
        if (!_purchased)
        {
            if (!_shop.Buy(item)) return;
            _buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Sell!";
            _icon.sprite = _soldSprite;
        }
        else
        {
            _buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Buy!";
            _icon.sprite = item.Icon;
            _shop.Sell(item);
        }
        _purchased = !_purchased;
    }
}