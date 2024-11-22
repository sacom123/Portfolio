using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Weapon/WeaponBase")]
public class WeaponBase : ScriptableObject
{
    [SerializeField] private int WeaponID;

    [SerializeField] private GameObject WeaponPrefab;

    [SerializeField] private GameObject BackWeaponPrefab;

    [SerializeField] private attribute attribute;

    [SerializeField] public WeaponType weaponType;

    [SerializeField] private ItemType ItemType;

    [SerializeField] private ItemClass itemClass;

    [SerializeField] private misc misc;
    
    [SerializeField] private int amount;

    [SerializeField] private float AttackSpeed;

    [SerializeField] private GameObject PlayerAura;

    public int quantity = 0;

    public Sprite WeaponSprite;

    public string itemDes_Text;

    public int price;    

    public WeaponBase myItem { get; set; }

    public InventorySlot activeSlot { get; set; }

    public int _WeaponID => WeaponID;

    public GameObject Weapon => WeaponPrefab;

    public GameObject BackWeapon => BackWeaponPrefab;

    public attribute Attribute => attribute;

    public WeaponType Type => weaponType;

    public ItemClass Item => itemClass;

    public ItemType GetItemType => ItemType;

    public int Atk
    {
        get => amount;
        set => amount = value;
    }

    public int _Amount => amount;

    public float _AttackSpeed => AttackSpeed;
    public GameObject PA => PlayerAura;
    public int WeaponLevel = 0;
    [HideInInspector]
    public int WeaponMaxLevel = 5;

    [HideInInspector] public GameObject currentWeapon;
    [HideInInspector] public Transform weaponHolder;

    public IEnumerator ChangeWeaponCoroutine(string weaponType, string weaponName)
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }

        GameObject newWeapon = Resources.Load<GameObject>($"Weapons/{weaponName}");

        if (newWeapon != null)
        {
            currentWeapon = Instantiate(newWeapon, weaponHolder.position, weaponHolder.rotation, weaponHolder);
            Debug.Log($"{weaponName} 무기가 장착되었습니다.");
        }
        else
        {
            Debug.LogWarning("해당 이름의 무기를 찾을 수 없습니다.");
        }

        yield return null;
    }
     public event Action OnQuantityChanged;

    public void DecreaseQuantity(int amount)
    {
        quantity = amount;
        OnQuantityChanged?.Invoke(); // 수량이 변경될 때 이벤트 호출
    }

}

public enum attribute
{
    light = 0,
    Drak = 1,
    Fire = 2,
    Water = 3
}

public enum WeaponType
{
    defaultWeapon = 0,
    Sword = 1,
    Axe = 2,
    Dagger = 3,
    bow = 4,
    boss = 5,
    Casque = 6,
    Armor = 7,
    Pants = 8,
    Shoes = 9
}

public enum ItemType
{
    defaultItem = 0,
    Weapon = 1,
    item = 2,
    misc = 3,
    potion=4,
    toTutorialWeapon=5,
        etc=6
}

public enum ItemClass
{
    Coin = 0,
    Hpotion = 1,
    Mpotion = 2,
}

public enum ItemTypeToTutorial
{
    defalutWeapon=0,
    toTutorialWeapon=1
}
public enum misc
{

}