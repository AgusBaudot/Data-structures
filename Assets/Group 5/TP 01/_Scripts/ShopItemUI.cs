using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private Image _icon;
    [SerializeField] private Button _buyButton;

    private Item _item;
    private Shop _shop;

    public void Setup(Item item, Shop shop)
    {
        _item = item;
        _shop = shop;
        
        _nameText.text = item.Name;
        _priceText.text = item.Price.ToString();
        
        _buyButton.onClick.RemoveAllListeners();
        _buyButton.onClick.AddListener(() => _shop.Buy(_item));
    }

    public int ItemID => _item.ID;
}