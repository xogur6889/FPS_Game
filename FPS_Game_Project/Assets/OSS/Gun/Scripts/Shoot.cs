using UnityEngine;

namespace OSS.Gun
{
    public class Shoot : MonoBehaviour
    {
        [Tooltip("총알 발사 지점"), SerializeField] private Transform shootPoint;

        private Camera playerCamera;

        [Tooltip("총알 궤적"), SerializeField] private GameObject projectilePrefab;
        
        private void Start()
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        private void Update()
        {
            // if (Input.GetButton("Fire"))
            if (Input.GetButtonDown("Fire"))
            {
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward,
                    out RaycastHit hit))
                {
                    Vector3 direction = hit.point - shootPoint.position;
                    direction.Normalize();

                    GameObject projectile = Instantiate(projectilePrefab);
                    projectile.transform.position = shootPoint.position;
                    projectile.transform.forward = direction;
                }
            }
        }
    }
}
