using Mirror;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [SyncVar]
    public NetworkIdentity Owner;

    public PlayerManager OnwerManager;

    public TrailRenderer AttackEffect;

    public BoxCollider BoxCollider;

    public string WeaponType;

    public void SetOwner(NetworkIdentity owner)
    {
        this.Owner = owner;
        OnwerManager = Owner.GetComponent<PlayerManager>();
    }

    public void SetCollider()
    {
        Owner.GetComponent<PlayerAttack>().AttackCollider = BoxCollider;
        Owner.GetComponent<PlayerAttack>().AttackEffect = AttackEffect;
        OnwerManager = Owner.GetComponent<PlayerManager>();
    }

    [Command]
    public void CmdSetCOllider()
    {
        Owner.GetComponent<PlayerAttack>().AttackCollider = BoxCollider;
        Owner.GetComponent<PlayerAttack>().AttackEffect = AttackEffect;
        OnwerManager = Owner.GetComponent<PlayerManager>();
    }

    private void Start()
    {
        if(Owner.isLocalPlayer && WeaponType.Equals("SwordHand"))
        {
            SetCollider();
            AttackEffect.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
    }

    [TargetRpc]
    public void RpcAssignToPlayer(NetworkConnection target, Vector3 position, Quaternion rotation, Vector3 localScale,bool active ,NetworkIdentity player)
    {
        transform.SetParent(player.gameObject.GetComponent<PlayerManager>().SetWeaponParent(WeaponType));
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.Euler(0,0,0);
        transform.localScale = localScale;
        transform.gameObject.SetActive(active);
    }

    public GameObject FindWeaponByNetId(uint netId)
    {
        // NetworkIdentity.spawned를 통해 NetId로 오브젝트를 찾음
        if (NetworkServer.spawned.TryGetValue(netId, out NetworkIdentity weaponNetworkIdentity))
        {
            GameObject weapon = weaponNetworkIdentity.gameObject;
            Debug.Log("Weapon found on client: " + weapon.name);
            return weapon;
            
        }
        else
        {
            Debug.LogError("Weapon not found for NetId: " + netId);
            return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Owner.isLocalPlayer) return;
        if (other.CompareTag("Enemy"))
        {   
            OnwerManager.CmdTakeAttack(other.GetComponent<NetworkIdentity>(), Owner);
        }
        
    }

}
