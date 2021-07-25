using System.Linq;
using UnityEngine;

namespace OSS.Gun
{
    [RequireComponent(
        typeof(Rigidbody),
        typeof(CapsuleCollider))
    ]
    public class Projectile : MonoBehaviour
    {
        [Tooltip("발사체 속도"), SerializeField] private float speed = 10.0f;
        [Tooltip("발사체 충돌 시 이펙트"), SerializeField]  private GameObject sparkParticle;
        
        [Tooltip("충돌 여부 감지를 위한 콜라이더")] private CapsuleCollider capsuleCollider;

        private void OnEnable()
        {
            Destroy(gameObject, 3.0f);  // 최대 시간
            
            // 발사체에 맞도록 콜라이더 속성 변경
            capsuleCollider = GetComponent<CapsuleCollider>();
            capsuleCollider.center = new Vector3(0, 0, 0.63f);
            capsuleCollider.radius = 0.02f;
            capsuleCollider.height = 1.15f;
            capsuleCollider.direction = 2;   // z axis
        }

        private void Update()
        {
            // 계속 앞으로 이동
            transform.position += transform.forward * (speed * Time.deltaTime);
        }

        /// <summary>
        /// 충돌이 감지 되었을 때
        /// </summary>
        /// <param name="other"></param>
        private void OnCollisionEnter(Collision other)
        {
            // 충돌 시 충돌 이펙트 생성
            GameObject hitParticle = Instantiate(sparkParticle);
            hitParticle.transform.position = other.contacts.First().point;
            hitParticle.transform.forward = other.contacts.First().normal;

            // 발사체 오브젝트는 제거
            Destroy(gameObject); 
        }

        /// <summary>
        /// 화면에서 사라지면 제거
        /// </summary>
        private void OnBecameInvisible()
        {
            Destroy(gameObject);
        }
    }
}
