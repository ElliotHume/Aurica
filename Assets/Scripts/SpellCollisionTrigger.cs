using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpellCollisionTrigger : MonoBehaviour
{
    public bool AcceptAllSpells = false;
    public List<AuricaSpell.ManaType> AcceptedManaTypes;
    public List<AuricaSpell> AcceptedSpells;
    public UnityEvent OnSpellCollide;
    

    void OnCollisionEnter(Collision collision) {
        CheckCollision(collision.gameObject);
    }

    void OnTriggerEnter(Collider collider) {
        CheckCollision(collider.gameObject); 
    }

    void CheckCollision(GameObject collision) {
        if (collision.gameObject.tag == "Spell") {
            if (AcceptAllSpells) {
                Trigger();
                return;
            }
            if (AcceptedManaTypes.Count > 0 || AcceptedSpells.Count > 0) {
                Spell spell = collision.gameObject.GetComponent<Spell>();
                if (spell != null) {
                    if ( AcceptedSpells.Contains(spell.auricaSpell) || AcceptedManaTypes.Contains(spell.auricaSpell.manaType) ) {
                        Trigger();
                    }
                }
            }
        }
    }

    public void Trigger() {
        Debug.Log("TRIGGER spell collision object: "+gameObject);
        OnSpellCollide.Invoke();
    }
}
