using UnityEngine;
using Weapons;

[CreateAssetMenu(fileName = "ItemSO", menuName = "ScriptableObject/Shop/Item")]
public class ItemSO : ScriptableObject
{
    [HideInInspector] public int ID;
    public string Name;
    public int Price;
    public Type Type;
    public Rarity Rarity;
    public Sprite Icon;
}