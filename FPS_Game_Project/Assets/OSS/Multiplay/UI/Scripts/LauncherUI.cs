using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OSS.Multiplay.UI
{
    public class LauncherUI : MonoBehaviour
    {
        [SerializeField] private Launcher launcher;

        [Tooltip("MainMenu root canvas")] private Transform mainMenuRootTransform;
        [Tooltip("MainMenu 방 생성 버튼")] private Button mainMenuStartButton;
        [Tooltip("MainMenu 방 참가 버튼")] private Button mainMenuJoinRoomButton;
        [Tooltip("MainMenu 종료 버튼")] private Button mainMenuQuitButton;

        [Tooltip("방 생성 UI 부모 transform")] private Transform createRoomRootTransform;
        [Tooltip("방 생성 메뉴, 방 이름 입력 창")] private InputField createRoomInputField;
        [Tooltip("방 생성 메뉴, 생성 버튼")] private Button createRoomButton;


        [Tooltip("방 참가 UI 부모 canvas")] private Transform roomListRootTransform;
        [Tooltip("방 목록 Scroll view")] private GameObject roomListScrollViewContent;

        [Tooltip("뒤로가기 버든")] private Button backButton;

        [SerializeField] private GameObject roomButtonItemPrefab;
        private readonly List<Button> roomButtonList = new List<Button>();

        public enum MenuState
        {
            MainMenu,
            Launch_Start,
            Launch_JoinRoom,
            Quit
        };
        
        private Dictionary<MenuState, Coroutine> buttonCoroutine = new Dictionary<MenuState, Coroutine>();

        private void Awake()
        {
            SetupButtonEvents();
            
            roomListRootTransform = transform.Find("RoomList");
            roomListScrollViewContent = roomListRootTransform.Find("Room List/Viewport/Content").gameObject;
        }
        
        private void SetupButtonEvents()
        {
            mainMenuRootTransform = transform.Find("MainMenu");
            
            mainMenuStartButton = mainMenuRootTransform.Find("Start").GetComponent<Button>();

            EventTrigger eventTriggerMainMenuStart =
                GetOrAddComponent<EventTrigger>(mainMenuStartButton.gameObject);
            EventTrigger.Entry entryPointerEnterMainMenuStart =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
            entryPointerEnterMainMenuStart.callback.AddListener(delegate
            {
                OnPointerEnterDelegate(MenuState.Launch_Start, mainMenuStartButton);
            });
            eventTriggerMainMenuStart.triggers.Add(entryPointerEnterMainMenuStart);

            EventTrigger.Entry entryPointerExitMainMenuStart =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
            entryPointerExitMainMenuStart.callback.AddListener(delegate
            {
                OnPointerExitDelegate(MenuState.Launch_Start, mainMenuStartButton);
            });
            eventTriggerMainMenuStart.triggers.Add(entryPointerExitMainMenuStart);

            EventTrigger.Entry entryPointerClickMainMenuStart =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
            entryPointerClickMainMenuStart.callback.AddListener(delegate
            {
                OnPointerClickDelegate(MenuState.Launch_Start, mainMenuStartButton);
            });
            eventTriggerMainMenuStart.triggers.Add(entryPointerClickMainMenuStart);

            
            mainMenuQuitButton = mainMenuRootTransform.Find("Quit").GetComponent<Button>();

            EventTrigger eventTriggerMainMenuQuit =
                GetOrAddComponent<EventTrigger>(mainMenuQuitButton.gameObject);
            EventTrigger.Entry entryPointerEnterMainMenuQuit =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
            entryPointerEnterMainMenuQuit.callback.AddListener(delegate
            {
                OnPointerEnterDelegate(MenuState.Quit, mainMenuQuitButton);
            });
            eventTriggerMainMenuQuit.triggers.Add(entryPointerEnterMainMenuQuit);

            EventTrigger.Entry entryPointerExitMainMenuQuit =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
            entryPointerExitMainMenuQuit.callback.AddListener(delegate
            {
                OnPointerExitDelegate(MenuState.Quit, mainMenuQuitButton);
            });
            eventTriggerMainMenuQuit.triggers.Add(entryPointerExitMainMenuQuit);

            EventTrigger.Entry entryPointerClickMainMenuQuit =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
            entryPointerClickMainMenuQuit.callback.AddListener(delegate
            {
                OnPointerClickDelegate(MenuState.Quit, mainMenuQuitButton);
            });
            eventTriggerMainMenuQuit.triggers.Add(entryPointerClickMainMenuQuit);
        }

        private T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>() ?? gameObject.gameObject.AddComponent<T>();

            return component;
        }

        private Animator mainMenuAnimator;
        private void OnEnable()
        {
            mainMenuRootTransform = transform.Find("MainMenu");
            mainMenuAnimator = mainMenuRootTransform.GetComponent<Animator>();
            mainMenuAnimator.Play("OnEnable");
        }

        public IEnumerator SetMenuEnabled(MenuState menuState)
        {
            mainMenuAnimator.Play("OnDisable");

            yield return new WaitForSeconds(0.1f);
            
            switch (menuState)
            {
                case MenuState.MainMenu:
                    roomListRootTransform.gameObject.SetActive(false);

                    mainMenuRootTransform.gameObject.SetActive(true);
                    break;
                case MenuState.Launch_Start:
                    mainMenuRootTransform.gameObject.SetActive(false);

                    roomListRootTransform.gameObject.SetActive(true);
                    break;
                case MenuState.Quit:
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(menuState), menuState, null);
            }
        }

        private void OnPointerEnterDelegate(in MenuState menuState, in Button button)
        {
            button.GetComponent<Animation>().Play("OnPointerEnter");
        }

        private void OnPointerExitDelegate(in MenuState menuState, in Button button)
        {
            button.GetComponent<Animation>().Play("OnPointerExit");
        }

        private void OnPointerClickDelegate(in MenuState menuState, in Button button)
        {
            StartCoroutine(SetMenuEnabled(menuState));
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

                if (index != -1 || roomInfo.IsOpen != true || (roomInfo.PlayerCount >= roomInfo.MaxPlayers)) continue;

                GameObject roomButtonGameObject = GameObject.Instantiate(roomButtonItemPrefab);
                RectTransform roomButtonRectTransform = (RectTransform) roomButtonGameObject.transform;
                roomButtonRectTransform.SetParent(roomListScrollViewContent.transform);
                int lastIndex = roomButtonList.Count;
                Vector3 roomButtonPosition = roomButtonRectTransform.position;
                roomButtonPosition.y = lastIndex * roomButtonRectTransform.rect.height * -1;
                roomButtonRectTransform.position = roomButtonPosition;
                RectTransform roomButtonParentRectTransform = (RectTransform) roomButtonRectTransform.parent.transform;
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