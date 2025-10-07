using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public class DamageInfo
{


    [HideInInspector] public Transform attacker;
    public int damage;
    public DamageType damageType;
    public Vector3 attackDir = Vector3.one * -1;
    public bool dealsGrayStaminaDamage = false;

}


public class HitboxComponent : MonoBehaviour
{
     public BoxCollider hitboxCollider;

    public DamageInfo damageInfo;
    
    public UnityEvent<HealthComponent> hitboxCollided = new();
    private void Awake()
    {
        if (hitboxCollider == null)
        {
            hitboxCollider = GetComponent<BoxCollider>();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.TryGetComponent(out HealthComponent hp))
        { 
            hitboxCollided.Invoke(hp);
        }
    }
}
public enum DamageType
{
    Ball,
    Skill,
    Environment,
    Other
}