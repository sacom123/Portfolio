using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEditor.Rendering.Universal;
using UnityEngine;


[CreateAssetMenu(fileName = "WeaponCollection", menuName = "Weapon/WeaponCollection")]
public class Weaponcollection : ScriptableObject
{
    [Header("모든 무기 모음")]
    [Tooltip("1번째로 선택한 무기 No 업글")]
    [SerializeField]
    private List<WeaponBase> FirstWeapon;

    [Tooltip("레벨업으로 2번째 업글")]
    [SerializeField]
    private List<WeaponBase> SecondWeapon;

    [Tooltip("레벨업으로 3번째 업글")]
    [SerializeField]
    private List<WeaponBase> ThirdWeapon;

    [SerializeField]
    public List<WeaponBase> potion;

    [SerializeField]
    public List<WeaponBase> item;

    [SerializeField]
    public List<WeaponBase> etc;

    [SerializedDictionary("아이템 타입","WeaponBase")]
    public SerializedDictionary<ItemType, List<WeaponBase>> ItemBase;


    public List<WeaponBase> GetFirstWeapon() => FirstWeapon;
    public List<WeaponBase> GetSecondWeapon() => SecondWeapon;
    public List<WeaponBase> GetThirdWeapon() => ThirdWeapon;


    public WeaponBase GetWeaponById(int weaponId, PlayerState Pm)
    {
        if (Pm.CurrentLevel > 0 || Pm.CurrentLevel < 10)
        {
            return SearchWeapon(FirstWeapon, weaponId);
        }
        else if (Pm.CurrentLevel > 10 || Pm.CurrentLevel < 20)
        {
            return SearchWeapon(SecondWeapon, weaponId);
        }
        else if (Pm.CurrentLevel > 20 || Pm.CurrentLevel < 30)
        {
            return SearchWeapon(ThirdWeapon, weaponId);
        }
        else
            return null;
    }

    private WeaponBase SearchWeapon(List<WeaponBase> weapons, int weaponid)
    {
        foreach (var weapon in weapons)
        {
            if (weapon._WeaponID == weaponid)
                return weapon;
        }
        return null;
    }

    public List<WeaponBase> GetWeaponBaseList()
    {
        var list = new List<WeaponBase>();
        list.AddRange(FirstWeapon);
        list.AddRange(SecondWeapon);
        list.AddRange(ThirdWeapon);
        return list;   
    }

    public List<WeaponBase> GetSword()
    {
        var swordList = new List<WeaponBase>();
        if (FirstWeapon != null && FirstWeapon.Count > 0)
        {
            swordList.Add(FirstWeapon[0]);
        }
        else
        {
            Debug.LogWarning("FirstWeapon 리스트가 비어 있거나 null입니다.");
        }
        return swordList;
    }

    public List<int> GetItemBaseList()
    {
        var list = new List<int>();
        for (int i = 0; i < FirstWeapon.Count; i++)
            list.Add(FirstWeapon[i]._WeaponID);
        for (int i = 0; i < SecondWeapon.Count; i++)
            list.Add(SecondWeapon[i]._WeaponID);
        for (int i = 0; i < ThirdWeapon.Count; i++)
            list.Add(ThirdWeapon[i]._WeaponID);

        return list;
    }

    public List<WeaponBase> GetItemList()
    {
        var list = new List<WeaponBase>();
        list.AddRange(potion);
        list.AddRange(item);
        return list;
    }

    public List<GameObject> GetItemPreFabs(ItemType Type)
    {
        var list = new List<GameObject>();
        
        foreach (var item in ItemBase[Type])
        {
            list.Add(item.Weapon);
        }
        return list;
    }

    public WeaponBase GetItemBase(int netid)
    {
        foreach (var itemList in ItemBase.Values) // 모든 아이템 리스트를 순회
        {
            foreach (var item in itemList)
            {
                if (item._WeaponID == netid)
                {
                    return item; // ID가 일치하면 해당 아이템 반환
                }
            }
        }
        Debug.LogWarning($"아이템 ID {netid}에 해당하는 데이터가 없습니다.");
        return null;
    }

    // 아이템 타입이랑 해당하는 아이템 코드 넣어주면 자동으로 찾는데 딕셔너리에 타입에 따른 아이템으로 정리 되어있을거에요
    public WeaponBase GetItem(ItemType type,int netid) 
    {
        if(ItemBase.TryGetValue(type,out List<WeaponBase>list))
        {
            foreach (var item in list)
            {
                if (item._WeaponID == netid)
                {
                    return item;
                }
                else
                {
                    return null;
                }
            }
            return null;
        }
        else
            return null;
    }
}
