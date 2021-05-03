using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpellCollisionTrigger : MonoBehaviour
{
    public List<AuricaSpell.ManaType> AcceptedManaTypes;
    public UnityEvent OnSpellCollide;
    

    void OnCollisionEnter(Collision collision) {
        CheckCollision(collision.gameObject);
    }

    void OnTriggerEnter(Collider collider) {
        CheckCollision(collider.gameObject); 
    }

    void CheckCollision(GameObject collision) {
        if (collision.gameObject.tag == "Spell") {
            if (AcceptedManaTypes.Count > 0) {
                Spell spell = collision.gameObject.GetComponent<Spell>();
                if (spell != null) {
                    if ( !AcceptedManaTypes.Contains(spell.auricaSpell.manaType) ) return;
                }
            }
            
            Trigger();
        }
    }

    public void Trigger() {
        OnSpellCollide.Invoke();
    }
}
