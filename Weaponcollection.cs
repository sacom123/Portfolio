using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEditor.Rendering.Universal;
using UnityEngine;


[CreateAssetMenu(fileName = "WeaponCollection", menuName = "Weapon/WeaponCollection")]
public class Weaponcollection : ScriptableObject
{
    [Header("��� ���� ����")]
    [Tooltip("1��°�� ������ ���� No ����")]
    [SerializeField]
    private List<WeaponBase> FirstWeapon;

    [Tooltip("���������� 2��° ����")]
    [SerializeField]
    private List<WeaponBase> SecondWeapon;

    [Tooltip("���������� 3��° ����")]
    [SerializeField]
    private List<WeaponBase> ThirdWeapon;

    [SerializeField]
    public List<WeaponBase> potion;

    [SerializeField]
    public List<WeaponBase> item;

    [SerializeField]
    public List<WeaponBase> etc;

    [SerializedDictionary("������ Ÿ��","WeaponBase")]
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
            Debug.LogWarning("FirstWeapon ����Ʈ�� ��� �ְų� null�Դϴ�.");
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
        foreach (var itemList in ItemBase.Values) // ��� ������ ����Ʈ�� ��ȸ
        {
            foreach (var item in itemList)
            {
                if (item._WeaponID == netid)
                {
                    return item; // ID�� ��ġ�ϸ� �ش� ������ ��ȯ
                }
            }
        }
        Debug.LogWarning($"������ ID {netid}�� �ش��ϴ� �����Ͱ� �����ϴ�.");
        return null;
    }

    // ������ Ÿ���̶� �ش��ϴ� ������ �ڵ� �־��ָ� �ڵ����� ã�µ� ��ųʸ��� Ÿ�Կ� ���� ���������� ���� �Ǿ������ſ���
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
