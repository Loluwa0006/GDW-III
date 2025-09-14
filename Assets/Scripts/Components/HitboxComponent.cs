using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public class DamageInfo
{
    public enum DamageType
    {
        Ball,
        Skill,
        Environment,
        Other
    }

    public Transform attacker;
    public int damage;
    public DamageType damageType;
    public Vector3 attackDir = Vector3.one * -1;

}


public class HitboxComponent : MonoBehaviour
{
    BoxCollider hitboxCollider;

    [SerializeField] DamageInfo damageInfo;
    
    public UnityEvent<HealthComponent> hitboxCollided = new();
    private void Awake()
    {
        hitboxCollider = GetComponent<BoxCollider>();
      
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.TryGetComponent(out HealthComponent hp))
        {
            if (damageInfo.attackDir == Vector3.one * -1)
            {
                damageInfo.attackDir = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.w);
            }
            hp.Damage(damageInfo);
            hitboxCollided.Invoke(hp);
        }
    }
}
