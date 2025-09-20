using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ShopContext;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image icon;
    [SerializeField] private Button actionButton;

    private ItemSO _item;
    private Shop _shop;
    private ItemContext _context;
    private Sprite _soldSprite;

    public void Setup(ItemSO item, Shop shop, ItemContext context)
    {
        _item = item;
        _shop = shop;
        _context = context;

        nameText.text = item.Name;
        priceText.text = item.Price.ToString();
        icon.sprite = item.Icon;

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => OnActionClicked());

        UpdateButtonLabel();
    }

    private void OnActionClicked()
    {
        //If this is a Shop item, then the button calls Buy, if it's a player item instead, call Sell.
        if (_context == ItemContext.Shop)
            _shop.Buy(_item);
        else
            _shop.Sell(_item);
    }

    private void UpdateButtonLabel()
    {
        //Adjust the button's text to match the context of this item.
        actionButton.GetComponentInChildren<TextMeshProUGUI>().text =
            _context == ItemContext.Shop ? "Buy" : "Sell";
    }

    public void Refresh(int shopStock, int playerStock)
    {
        int amount = _context == ItemContext.Player ? playerStock : shopStock;

        nameText.text = $"{_item.Name} x{amount}";
        
        actionButton.interactable = amount > 0;
    }

    public int ItemID => _item.ID; //Return ID of this item.
}