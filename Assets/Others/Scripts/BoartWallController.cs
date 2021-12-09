using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoartWallController : MonoBehaviour
{
    [SerializeField] private bool useDestroy = false;

    [SerializeField] private ParticleSystem[] destroyParticle = new ParticleSystem[0];
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<FlySerferController>())
            if (useDestroy)
                StartCoroutine(DestroyObj());
    }

    private IEnumerator DestroyObj()
    {
        useDestroy = false;

        for (int i = 0; i < destroyParticle.Length; i++)
        {
            if (destroyParticle[i] != null) destroyParticle[i].Play();
            yield return new WaitForFixedUpdate();
        }
    }
}
