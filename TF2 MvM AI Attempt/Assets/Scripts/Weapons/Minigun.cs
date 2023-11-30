using System.Collections;
using UnityEngine;

//The script was made by following a tutorial by LlamAcademy. However, there are edits and additions to the script by me.
//Link to the tutorial video: https://www.youtube.com/watch?v=cI3E7_f74MA
public class Minigun : MonoBehaviour
{
    [SerializeField] private bool AddBulletSpread = true;
    private Vector3 BulletSpreadVariance = new Vector3(0.01f, 0.01f, 0.01f);

    [SerializeField] private ParticleSystem ShootingSystem;
    [SerializeField] private Transform BulletSpawnPoint;
    [SerializeField] private ParticleSystem ImpactParticleSystem;
    [SerializeField] private TrailRenderer BulletTrail;
    [SerializeField] private float ShootDelay = 0.2f;
    [SerializeField] private LayerMask Mask;
    [SerializeField] private float BulletSpeed = 100;
    [SerializeField] public int BulletDamage = 0;

    private Animator Animator;
    private float LastShootTime;

    private void Awake()
    {
        Animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        BulletDamage = Random.Range(5,10);

        if(transform.parent.CompareTag("Player") && Input.GetMouseButton(1))
        {
            Animator.SetBool("IsRevved", true);
            if(Input.GetMouseButton(0) || Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
        }
        else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) { Animator.SetBool("IsRevved", false); }
    }
    public void Shoot()
    {
        if (LastShootTime + ShootDelay < Time.time)
        {
            //Apparently object pooling can be used to fix some performance related issues
            //but I've decided to keep it as it is for the time being.

            Animator.SetBool("IsShooting", true);
            ShootingSystem.Play();
            Vector3 direction = GetDirection();

            if (Physics.Raycast(BulletSpawnPoint.position, direction, out RaycastHit hit, float.MaxValue, Mask))
            {
                TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);

                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, true));

                ApplyDamage(hit.collider);

                LastShootTime = Time.time;
            }
            else
            {
                TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);

                StartCoroutine(SpawnTrail(trail, BulletSpawnPoint.position + GetDirection() * 100, Vector3.zero, false));

                LastShootTime = Time.time;
            }
        }
    }

    private Vector3 GetDirection()
    {
        Vector3 direction = transform.forward;

        if (AddBulletSpread)
        {
            direction += new Vector3(
                Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
                Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
                Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)
            );

            direction.Normalize();
        }

        return direction;
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 HitPoint, Vector3 HitNormal, bool MadeImpact)
    {
        Vector3 startPosition = Trail.transform.position;
        float distance = Vector3.Distance(Trail.transform.position, HitPoint);
        float remainingDistance = distance;

        while (remainingDistance > 0)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (remainingDistance / distance));

            remainingDistance -= BulletSpeed * Time.deltaTime;

            yield return null;
        }
        Animator.SetBool("IsShooting", false);
        Trail.transform.position = HitPoint;
        if (MadeImpact)
        {
            Instantiate(ImpactParticleSystem, HitPoint, Quaternion.LookRotation(HitNormal));
        }

        Destroy(Trail.gameObject, Trail.time);
    }

    public void ApplyDamage(Collider other) 
    {
        //TODO: Gotta fix this function because robots can deal damage to each other.
        other.GetComponent<Health>().TakeDamage(BulletDamage);
        Debug.Log(other.name + "'s current HP is " + other.GetComponent<Health>().currentHealth);
    }
}