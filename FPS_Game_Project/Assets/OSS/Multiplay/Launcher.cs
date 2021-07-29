using System.Collections.Generic;
using OSS.Multiplay.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace OSS.Multiplay
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private LauncherUI launcherUI;

        private void Awake()
        {
            if (PhotonNetwork.IsConnected == true) return;
            
            // PhotonNetwork.LoadLevel() 함수를 마스터 클라이언트에서 호출 시 같은 방에 있는 모든 클라이언트들의 씬도 자동으로 변경하도록 함.
            PhotonNetwork.AutomaticallySyncScene = true;

            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = "0.1";
        }

        public override void OnJoinedLobby()
        {
            Debug.Log("OnJoinedLobby() function was called");
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("OnConnectedToMaster function called");
            PhotonNetwork.JoinLobby();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("OnDisconnected was called {0}", cause);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log("OnJoinRoomFailed() function was called");
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom() function was called, now this client is in a room");
            PhotonNetwork.LoadLevel(1);
        }

        public void CreateRoom(in string roomName)
        {
            RoomOptions roomOptions = new RoomOptions {MaxPlayers = 3};
            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }

        public void JoinRoom(in string roomName)
        {
            PhotonNetwork.JoinRoom(roomName);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            switch (returnCode)
            {
                case 32766:
                    Debug.LogWarning("같은 이름을 가진 방이 이미 존재합니다. 다른 방 이름을 입력해주세요");
                    break;
                default:
                    Debug.LogWarningFormat("OnCreateRoomFailed was called {0}", message);
                    break;
            }
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            Debug.Log("On Room list update");

            launcherUI.UpdateRoomList(roomList);
        }
    }
}