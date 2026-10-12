using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public abstract class CollectibleActor : MonoBehaviour
{
    const float MinDistanceToTarget = 0.1f;

    protected abstract float timeToCollect { get; }
    protected abstract float lifetime { get; }
    protected abstract Vector3 destination { get; }
    protected bool collecting { get; private set; } = false;
    protected SpriteRenderer spriteRenderer { get; private set; }
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(lifetime > 0)
            StartCoroutine(WaitForTimeout());
    }
    private IEnumerator WaitForTimeout()
    {
        yield return new WaitForSeconds(lifetime);
        EndLife(true);
    }
    public void OnMouseDown()
    {
        if (collecting)
            return;
        Collect();
    }
    private void Collect()
    {
        collecting = true;
        OnClicked();
        StopCoroutine(WaitForTimeout());
        StartCoroutine(CollectAnimation());
        
    }
    private IEnumerator CollectAnimation()
    {
        float elapsed = 0f;
        Vector3 startPosition = transform.position;

        while (elapsed < timeToCollect)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / timeToCollect);
            transform.position = Vector3.Lerp(startPosition, destination, t);

            yield return null;
        }

        transform.position = destination;
        EndLife(false);
    }

    private void EndLife(bool reasonIsTimeout)
    {
        OnDeath(reasonIsTimeout);
    }
    protected abstract void OnClicked();
    protected virtual void OnDeath(bool reasonIsTimeout) 
    {
        Destroy(gameObject);
    }
}
