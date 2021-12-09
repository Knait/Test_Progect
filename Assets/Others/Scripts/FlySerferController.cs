using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlySerferController : MonoBehaviour
{
    [SerializeField] private float flyForce = 50;
    [SerializeField] private float sleshForce = 100;

    private Rigidbody _rb;

    [Header("Particles")]
    [SerializeField] private ParticleSystem sleshEndParticle;
    [SerializeField] private ParticleSystem gameEndParticle;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
    }

    public void GameStart()
    {
        _rb.isKinematic = false;
        AddSerfForce(flyForce);
    }

    public void GameEnd()
    {
        if (gameEndParticle != null)
            gameEndParticle.Play();

        _rb.AddForce(transform.up * (-sleshForce), ForceMode.Impulse);
    }

    public void SetShesh(float horizontal, float vertical)
    {
        _rb.AddForce(new Vector3(horizontal * sleshForce, 0, vertical * sleshForce), ForceMode.Impulse);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<BoartWallController>())
            StopSlesh();
    }

    private void AddSerfForce(float force)
    {
        _rb.AddForce(transform.up * -force, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<EndExitZone>())
            FSGameController.Instance.GameEnded();

        if (other.GetComponent<ObstacleWallController>())
            FSGameController.Instance.UpdatePoint(1);
    }

    private void StopSlesh()
    {
        if (sleshEndParticle != null)
            sleshEndParticle.Play();

        _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
    }

}
