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

        public enum MenuState
        {
            MainMenu,
            Launch_CreateRoom,
            Launch_JoinRoom,
            Quit
        };
        
        private Dictionary<MenuState, Coroutine> buttonCoroutine = new Dictionary<MenuState, Coroutine>();

        private void Awake()
        {
            SetupButtonEvents();

            createRoomRootTransform = transform.Find("Create Room");
            createRoomInputField = createRoomRootTransform.Find("InputField").GetComponent<InputField>();
            createRoomButton = createRoomRootTransform.Find("Create Room").GetComponent<Button>();
            createRoomButton.onClick.AddListener(CreateRoomButtonClick);

            joinRoomRootTransform = transform.Find("Join Room");
            joinRoomListScrollViewContent = joinRoomRootTransform.Find("Room List/Viewport/Content").gameObject;

            backButton = transform.Find("Back").GetComponent<Button>();
            backButton.onClick.AddListener(delegate { SetMenuEnabled(MenuState.MainMenu); });
        }

        private void SetupButtonEvents()
        {
            mainMenuRootTransform = transform.Find("MainMenu");

            
            
            mainMenuCreateRoomButton = mainMenuRootTransform.Find("Create Room").GetComponent<Button>();

            EventTrigger eventTriggerMainMenuCreateRoom =
                GetOrAddComponent<EventTrigger>(mainMenuCreateRoomButton.gameObject);
            EventTrigger.Entry entryPointerEnterMainMenuCreateRoom =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
            entryPointerEnterMainMenuCreateRoom.callback.AddListener(delegate
            {
                OnPointerEnterDelegate(MenuState.Launch_CreateRoom, mainMenuCreateRoomButton);
            });
            eventTriggerMainMenuCreateRoom.triggers.Add(entryPointerEnterMainMenuCreateRoom);

            EventTrigger.Entry entryPointerExitMainMenuCreateRoom =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
            entryPointerExitMainMenuCreateRoom.callback.AddListener(delegate
            {
                OnPointerExitDelegate(MenuState.Launch_CreateRoom, mainMenuCreateRoomButton);
            });
            eventTriggerMainMenuCreateRoom.triggers.Add(entryPointerExitMainMenuCreateRoom);

            EventTrigger.Entry entryPointerClickMainMenuCreateRoom =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
            entryPointerClickMainMenuCreateRoom.callback.AddListener(delegate
            {
                OnPointerClickDelegate(MenuState.Launch_CreateRoom, mainMenuCreateRoomButton);
            });
            eventTriggerMainMenuCreateRoom.triggers.Add(entryPointerClickMainMenuCreateRoom);

            
            
            mainMenuJoinRoomButton = mainMenuRootTransform.Find("Join Room").GetComponent<Button>();

            EventTrigger eventTriggerMainMenuJoinRoom =
                GetOrAddComponent<EventTrigger>(mainMenuJoinRoomButton.gameObject);
            EventTrigger.Entry entryPointerEnterMainMenuJoinRoom =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
            entryPointerEnterMainMenuJoinRoom.callback.AddListener(delegate
            {
                OnPointerEnterDelegate(MenuState.Launch_JoinRoom, mainMenuJoinRoomButton);
            });
            eventTriggerMainMenuJoinRoom.triggers.Add(entryPointerEnterMainMenuJoinRoom);

            EventTrigger.Entry entryPointerExitMainMenuJoinRoom =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
            entryPointerExitMainMenuJoinRoom.callback.AddListener(delegate
            {
                OnPointerExitDelegate(MenuState.Launch_JoinRoom, mainMenuJoinRoomButton);
            });
            eventTriggerMainMenuJoinRoom.triggers.Add(entryPointerExitMainMenuJoinRoom);

            EventTrigger.Entry entryPointerClickMainMenuJoinRoom =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
            entryPointerClickMainMenuJoinRoom.callback.AddListener(delegate
            {
                OnPointerClickDelegate(MenuState.Launch_JoinRoom, mainMenuJoinRoomButton);
            });
            eventTriggerMainMenuJoinRoom.triggers.Add(entryPointerClickMainMenuJoinRoom);

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

        private void OnEnable()
        {
            SetMenuEnabled(MenuState.MainMenu);
        }

        public void SetMenuEnabled(in MenuState menuState)
        {
            switch (menuState)
            {
                case MenuState.MainMenu:
                    createRoomRootTransform.gameObject.SetActive(false);
                    joinRoomRootTransform.gameObject.SetActive(false);

                    mainMenuRootTransform.gameObject.SetActive(true);
                    break;
                case MenuState.Launch_CreateRoom:
                    joinRoomRootTransform.gameObject.SetActive(false);
                    mainMenuRootTransform.gameObject.SetActive(false);

                    createRoomRootTransform.gameObject.SetActive(true);
                    break;
                case MenuState.Launch_JoinRoom:
                    createRoomRootTransform.gameObject.SetActive(false);
                    mainMenuRootTransform.gameObject.SetActive(false);

                    joinRoomRootTransform.gameObject.SetActive(true);
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
            Coroutine coroutine = buttonCoroutine.ContainsKey(menuState) ? buttonCoroutine[menuState] : null;
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            coroutine = StartCoroutine(OnPointerEnterEvent(button));
        }

        private void OnPointerExitDelegate(in MenuState menuState, in Button button)
        {
            Coroutine coroutine = buttonCoroutine.ContainsKey(menuState) ? buttonCoroutine[menuState] : null;
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            coroutine = StartCoroutine(OnPointerExitEvent(button));
        }

        private void OnPointerClickDelegate(in MenuState menuState, in Button button)
        {
            SetMenuEnabled(menuState);
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
                roomButtonRectTransform.SetParent(joinRoomListScrollViewContent.transform);
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

        private IEnumerator OnPointerEnterEvent(Button button)
        {
            Vector3 startScale = button.transform.localScale;
            Vector3 targetScale = new Vector3(1.2f, 1.2f, 1.2f);

            yield return StartCoroutine(OnPointerEventButtonResize(startScale, targetScale, button));
        }

        private IEnumerator OnPointerExitEvent(Button button)
        {
            Vector3 startScale = button.transform.localScale;
            Vector3 targetScale = Vector3.one;

            yield return StartCoroutine(OnPointerEventButtonResize(startScale, targetScale, button));
        }

        private IEnumerator OnPointerEventButtonResize(Vector3 startScale, Vector3 targetScale, Button button)
        {
            Vector3 currentScale = startScale;

            float t = 0;
            while (t <= 1.0f)
            {
                float scale = Mathf.Lerp(startScale.x, targetScale.x, t);
                t += Time.deltaTime * 10;

                currentScale.x = currentScale.y = currentScale.y = scale;

                button.transform.localScale = currentScale;

                yield return null;
            }

            button.transform.localScale = targetScale;
        }
    }
}