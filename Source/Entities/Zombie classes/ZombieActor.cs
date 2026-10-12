using System.Collections;
using UnityEngine;

public class ZombieActor : EntityActor
{
    public override void UpdatePosition(float xPos, float yPos)
    {
        base.UpdatePosition(xPos, yPos+1);
    }

    /*
    const float animationSpeed = 0.02f;
    private readonly Vector3 animationVector = new Vector3(0, animationSpeed, 0);
    public void RiseAnimation()
    {
        transform.localScale = new Vector3(1, 0.1f, 1);
    }
    private void Update()
    {
        if (transform.localScale.y != 1)
            if (transform.localScale.y > 1)
                transform.localScale = Vector3.one;
            else
                transform.localScale = transform.localScale + animationVector;
    }

    */


    private const float animationDurationDefault = 0.3f;

    public void RiseAnimation(float duration = animationDurationDefault)
    {
        StartCoroutine(Rise(duration));
    }

    private IEnumerator Rise(float duration = animationDurationDefault)
    {
        Vector3 startScale = new Vector3(1f, 0.1f, 1f);
        Vector3 endScale = Vector3.one;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        transform.localScale = endScale;
    }
    /* private IEnumerator Fall(float height)
    {
        float targetY = transform.position.y;
        float startY = targetY + height;

        Vector3 position = transform.position;
        position.y = startY;
        transform.position = position;

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / animationDuration;
            float y = Mathf.Lerp(startY, targetY, t);

            position = transform.position;
            position.y = y;

            transform.position = position;

            yield return null;
        }

        position = transform.position;
        position.y = targetY;
        transform.position = position;
    }*/
}
