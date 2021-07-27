using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace OSS.Multiplay
{
    public class Multiplay : MonoBehaviourPunCallbacks
    {
        [SerializeField] private GameObject playerPrefab;
        private GameObject player;
        private void Start()
        {
            player = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
        }

        public override void OnLeftRoom()
        {
            Destroy(player);
        }
    }
}
