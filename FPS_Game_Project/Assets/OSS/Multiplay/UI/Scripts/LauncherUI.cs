using System;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace OSS.Multiplay.UI
{
    public class LauncherUI : MonoBehaviour
    {
        [SerializeField] private Launcher launcher;
        
        [Tooltip("MainMenu root canvas")] private Transform mainMenuRootTransform;
        [Tooltip("MainMenu 방 생성 버튼")] private Button mainMenuCreateRoomButton;
        [Tooltip("MainMenu 방 참가 버튼")] private Button mainMenuJoinRoomButton;
        [Tooltip("MainMenu 종료 버튼")] private Button mainMenuQuitButton;
        
        [Tooltip("방 생성 UI 부모 transform")] private Transform createRoomRootTransform;
        [Tooltip("방 생성 메뉴, 방 이름 입력 창")] private InputField createRoomInputField;
        [Tooltip("방 생성 메뉴, 생성 버튼")] private Button createRoomButton;
        
        
        [Tooltip("방 참가 UI 부모 canvas")] private Transform joinRoomRootTransform;
        [Tooltip("방 목록 Scroll view")] private GameObject joinRoomListScrollViewContent;

        [Tooltip("뒤로가기 버든")] private Button backButton;

        [SerializeField] private GameObject roomButtonItemPrefab;
        private readonly List<Button> roomButtonList = new List<Button>();

        private enum MenuState
        {
            MainMenu,
            CreateRoom,
            JoinRoom,
            Quit
        };

        private MenuState menuState = MenuState.MainMenu;

        private void Awake()
        {
            mainMenuRootTransform = transform.Find("MainMenu");
            mainMenuCreateRoomButton = mainMenuRootTransform.Find("Create Room").GetComponent<Button>();
            mainMenuCreateRoomButton.onClick.AddListener(delegate { SetMenuEnabled(MenuState.CreateRoom); } );
            mainMenuJoinRoomButton = mainMenuRootTransform.Find("Join Room").GetComponent<Button>();
            mainMenuJoinRoomButton.onClick.AddListener(delegate { SetMenuEnabled(MenuState.JoinRoom); } );
            mainMenuQuitButton = mainMenuRootTransform.Find("Quit").GetComponent<Button>();
            mainMenuQuitButton.onClick.AddListener(delegate { SetMenuEnabled(MenuState.Quit); } );
            

            createRoomRootTransform = transform.Find("Create Room");
            createRoomInputField = createRoomRootTransform.Find("InputField").GetComponent<InputField>();
            createRoomButton = createRoomRootTransform.Find("Create Room").GetComponent<Button>();
            createRoomButton.onClick.AddListener(CreateRoomButtonClick);
            
            
            joinRoomRootTransform = transform.Find("Join Room");
            joinRoomListScrollViewContent = joinRoomRootTransform.Find("Room List/Viewport/Content").gameObject;

            backButton = transform.Find("Back").GetComponent<Button>();
            backButton.onClick.AddListener(delegate { SetMenuEnabled(MenuState.MainMenu); });
        }

        private void OnEnable()
        {
            SetMenuEnabled(MenuState.MainMenu);
        }

        private void SetMenuEnabled(in MenuState menuState)
        {
            this.menuState = menuState;
            switch (menuState)
            {
                case MenuState.MainMenu:
                    createRoomRootTransform.gameObject.SetActive(false);
                    joinRoomRootTransform.gameObject.SetActive(false);
            
                    mainMenuRootTransform.gameObject.SetActive(true);
                    break;
                case MenuState.CreateRoom:
                    joinRoomRootTransform.gameObject.SetActive(false);
                    mainMenuRootTransform.gameObject.SetActive(false);
                    
                    createRoomRootTransform.gameObject.SetActive(true);
                    break;
                case MenuState.JoinRoom:
                    createRoomRootTransform.gameObject.SetActive(false);
                    mainMenuRootTransform.gameObject.SetActive(false);
            
                    joinRoomRootTransform.gameObject.SetActive(true);
                    break;
                case MenuState.Quit:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(menuState), menuState, null);
            }
        }

        private void CreateRoomButtonClick()
        {
            string roomName = createRoomInputField.text;
            if (string.IsNullOrEmpty(roomName) == true) return;
            
            launcher.CreateRoom(roomName);
        }

        public void UpdateRoomList(in List<RoomInfo> roomInfoList)
        {
            foreach (RoomInfo roomInfo in roomInfoList)
            {
                int index = roomButtonList.FindIndex(roomButton => roomButton.name == roomInfo.Name);
                if (index != -1 && roomInfo.RemovedFromList)
                {
                    Destroy(roomButtonList[index].gameObject);
                    roomButtonList.RemoveAt(index);
                }

                if (index == -1 && roomInfo.IsOpen == true && (roomInfo.PlayerCount < roomInfo.MaxPlayers))
                {
                    GameObject roomButtonGameObject = GameObject.Instantiate(roomButtonItemPrefab);
                    RectTransform roomButtonRectTransform = (RectTransform)roomButtonGameObject.transform;
                    roomButtonRectTransform.SetParent(joinRoomListScrollViewContent.transform);
                    int lastIndex = roomButtonList.Count;
                    Vector3 roomButtonPosition = roomButtonRectTransform.position;
                    roomButtonPosition.y = lastIndex * roomButtonRectTransform.rect.height * -1;
                    roomButtonRectTransform.position = roomButtonPosition;
                    RectTransform roomButtonParentRectTransform = (RectTransform)roomButtonRectTransform.parent.transform;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(roomButtonParentRectTransform);

                    Button roomButton = roomButtonGameObject.GetComponent<Button>();
                    roomButton.name = roomInfo.Name;
                    roomButton.GetComponentInChildren<Text>().text = roomInfo.Name;
                    roomButton.onClick.AddListener(delegate { launcher.JoinRoom(roomInfo.Name); });
                    roomButtonList.Add(roomButton);
                }
            }
        }
    }
}
