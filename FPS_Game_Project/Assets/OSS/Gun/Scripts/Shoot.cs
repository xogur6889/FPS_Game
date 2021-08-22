// using Photon.Pun;
// using UnityEngine;
//
// namespace OSS.Gun
// {
//     public class Shoot : MonoBehaviour
//     {
//         [Tooltip("총알 발사 지점"), SerializeField] private Transform shootPoint;
//
//         private Camera playerCamera;
//
//         [Tooltip("총알 궤적"), SerializeField] private GameObject projectilePrefab;
//         
//         private PhotonView photonView;
//         
//         private void Start()
//         {
//             photonView = GetComponent<PhotonView>();
//             playerCamera = GetComponentInChildren<Camera>();
//         }
//
//         private void Update()
//         {
//             if (photonView.IsMine == false) return;
//             
//             // if (Input.GetButton("Fire"))
//             if (Input.GetButtonDown("Fire"))
//             {
//                 if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward,
//                     out RaycastHit hit))
//                 {
//                     Vector3 direction = hit.point - shootPoint.position;
//                     direction.Normalize();
//
//                     photonView.RPC("Fire", RpcTarget.AllBufferedViaServer, direction);
//                 }
//             }
//         }
//
//         [PunRPC]
//         private void Fire(Vector3 direction)
//         {
//             GameObject projectile = Instantiate(projectilePrefab);
//             projectile.transform.position = shootPoint.position;
//             projectile.transform.forward = direction;
//         }
//     }
// }
