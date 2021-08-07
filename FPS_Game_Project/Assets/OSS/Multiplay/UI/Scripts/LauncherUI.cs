using System;
using System.Collections;
using System.Collections.Generic;
using OSS.Launcher.UI;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OSS.Multiplay.UI
{
    public class LauncherUI : MonoBehaviour
    {
        [SerializeField] private Launcher launcher;

        [SerializeField] private GameObject roomButtonItemPrefab;
        private readonly List<Button> roomButtonList = new List<Button>();
        
        private MainMenu mainMenu;
        private RoomList roomList;

        public enum MenuState
        {
            MainMenu,
            LaunchStart,
            Quit
        }

        private void Awake()
        {
            Setup();
            SetupButtonEvents();
        }

        private void Setup()
        {
            Transform mainMenuRootTransform = transform.Find("MainMenu");
            mainMenu = new MainMenu(mainMenuRootTransform);
        }
        
        private void SetupButtonEvents()
        {
            Transform mainMenuRootTransform = transform.Find("MainMenu");
            
            EventTrigger eventTriggerMainMenuStart =
                GetOrAddComponent<EventTrigger>(mainMenuStartButton.gameObject);
            EventTrigger.Entry entryPointerEnterMainMenuStart =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
            entryPointerEnterMainMenuStart.callback.AddListener(delegate
            {
                OnPointerEnterDelegate(mainMenuStartButton);
            });
            eventTriggerMainMenuStart.triggers.Add(entryPointerEnterMainMenuStart);

            EventTrigger.Entry entryPointerExitMainMenuStart =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
            entryPointerExitMainMenuStart.callback.AddListener(delegate
            {
                OnPointerExitDelegate(mainMenuStartButton);
            });
            eventTriggerMainMenuStart.triggers.Add(entryPointerExitMainMenuStart);

            EventTrigger.Entry entryPointerClickMainMenuStart =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
            entryPointerClickMainMenuStart.callback.AddListener(delegate
            {
                OnPointerClickDelegate(MenuState.LaunchStart);
            });
            eventTriggerMainMenuStart.triggers.Add(entryPointerClickMainMenuStart);

            
            mainMenuQuitButton = mainMenuRootTransform.Find("Quit").GetComponent<Button>();

            EventTrigger eventTriggerMainMenuQuit =
                GetOrAddComponent<EventTrigger>(mainMenuQuitButton.gameObject);
            EventTrigger.Entry entryPointerEnterMainMenuQuit =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
            entryPointerEnterMainMenuQuit.callback.AddListener(delegate
            {
                OnPointerEnterDelegate(mainMenuQuitButton);
            });
            eventTriggerMainMenuQuit.triggers.Add(entryPointerEnterMainMenuQuit);

            EventTrigger.Entry entryPointerExitMainMenuQuit =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
            entryPointerExitMainMenuQuit.callback.AddListener(delegate
            {
                OnPointerExitDelegate(mainMenuQuitButton);
            });
            eventTriggerMainMenuQuit.triggers.Add(entryPointerExitMainMenuQuit);

            EventTrigger.Entry entryPointerClickMainMenuQuit =
                new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
            entryPointerClickMainMenuQuit.callback.AddListener(delegate
            {
                OnPointerClickDelegate(MenuState.Quit);
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
                case MenuState.LaunchStart:
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

        private void OnPointerEnterDelegate(in Button button)
        {
            button.GetComponent<Animation>().Play("OnPointerEnter");
        }

        private void OnPointerExitDelegate(in Button button)
        {
            button.GetComponent<Animation>().Play("OnPointerExit");
        }

        private void OnPointerClickDelegate(in MenuState menuState)
        {
            StartCoroutine(SetMenuEnabled(menuState));
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

                if (index != -1 || roomInfo.IsOpen != true || roomInfo.PlayerCount >= roomInfo.MaxPlayers) continue;

                GameObject roomButtonGameObject = Instantiate(roomButtonItemPrefab);
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