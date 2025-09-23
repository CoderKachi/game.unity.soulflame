using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve animationCurve;
    [SerializeField]
    private Color flashColour = Color.gray;
    [SerializeField]
    private int maxDamageFlashThreshold = 10;
    private float maxFlashTime = 0.25f;

    private SpriteRenderer[] spriteRenderers;
    private Material[] materials;




    [SerializeField]
    float healthMax;
    
    float healthCurrent = 100f;

    private void Awake()
    {
        
        healthCurrent = healthMax;
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        InitialiseMaterials();
    }

    public void TakeDamage(int value) {
        healthCurrent -= value;
        StartCoroutine(DamageFlash(value));
        if (healthCurrent <= 0)
        {
            Die();
        }
    }

    public void Heal(int value)
    {
        healthCurrent += value;
    }

    public float GetHealth() {
        return healthCurrent;
    }

    public float GetHealthAsPercent()
    {
        return healthCurrent / healthMax;
    }

    public void Die()
    {
        
        Destroy(this.gameObject);
    }



    private void InitialiseMaterials()
    {
        materials = new Material[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            materials[i] = spriteRenderers[i].material;
        }
    }

    private IEnumerator DamageFlash(float damage)
    {
        float maxFlashValue = Mathf.Min(1f, (float)(damage / maxDamageFlashThreshold));
        float flashTime = maxFlashValue * maxFlashTime;
        float currentFlashValue = 0f;
        Debug.Log("damage Taken(Enumerator");
        SetFlashColour();
        
        float elapsedTime = 0f;
        while (elapsedTime < flashTime)
        {
            elapsedTime += Time.deltaTime;

            currentFlashValue = Mathf.Lerp(maxFlashValue, 0f, (elapsedTime/flashTime));
            SetFlashValue(currentFlashValue);
            
            yield return null;
        }

    }

    private void SetFlashColour()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetColor("_FlashColour", flashColour);
        }
    }

    private void SetFlashValue(float value)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat("_FlashValue", value);
        }
    }

}
