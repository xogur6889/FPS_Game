using System;
using System.Collections;
using System.Collections.Generic;
using OSS.Launcher.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace OSS.Launcher.UI
{
    public class LauncherUI : MonoBehaviour
    {
        [SerializeField] private Multiplay.Launcher launcher;

        [SerializeField] private GameObject roomButtonItemPrefab;
        private readonly List<Button> roomButtonList = new List<Button>();

        private StartMenu startMenu;
        private RoomList roomList;
        private SignIn signIn;
        private SignUp signUp;
        private CreateRoom createRoom;

        private MenuType currentMenuType = MenuType.Start;

        private void Awake()
        {
            Setup();
        }

        private void Setup()
        {
            startMenu = new StartMenu(transform.Find("StartMenu"));
            roomList = new RoomList(transform.Find("RoomList"));
            signIn = new SignIn(transform.Find("Sign In"));
            signUp = new SignUp(transform.Find("Sign Up"));
            createRoom = new CreateRoom(transform.Find("Create Room"));

            SetupButtonEvents();
        }

        private void SetupButtonEvents()
        {
            startMenu.SetupPointEvents(StartMenu.ButtonType.Start,
                () => StartCoroutine(OnPointerEnter(startMenu.buttons[StartMenu.ButtonType.Start])),
                () => StartCoroutine(OnPointerExit(startMenu.buttons[StartMenu.ButtonType.Start])),
                () => StartCoroutine(OnEnableMenu(MenuType.SignIn)));
            startMenu.SetupPointEvents(StartMenu.ButtonType.Quit,
                () => StartCoroutine(OnPointerEnter(startMenu.buttons[StartMenu.ButtonType.Quit])),
                () => StartCoroutine(OnPointerExit(startMenu.buttons[StartMenu.ButtonType.Quit])),
                () =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                });

            startMenu.SetActiveEvents(
                () => StartCoroutine(EnableStartMenu()),
                () => StartCoroutine(DisableStartMenu()));

            signIn.SetupPointEvents(SignIn.ButtonType.Close,
                () =>
                {
                    if (signIn.closeButtonCoroutine != null)
                    {
                        StopCoroutine(signIn.closeButtonCoroutine);
                        signIn.closeButtonCoroutine = null;
                    }

                    signIn.closeButtonCoroutine = StartCoroutine(signIn.PointerEnterCloseButton());
                },
                () =>
                {
                    if (signIn.closeButtonCoroutine != null)
                    {
                        StopCoroutine(signIn.closeButtonCoroutine);
                        signIn.closeButtonCoroutine = null;
                    }

                    signIn.closeButtonCoroutine = StartCoroutine(signIn.PointerExitCloseButton());
                },
                () => StartCoroutine(DisableSignIn(MenuType.Start)));

            signIn.SetupPointEvents(SignIn.ButtonType.SignUp,
                () => { },
                () => { },
                () => StartCoroutine(DisableSignIn(MenuType.SignUp)));
            
            signIn.SetupPointEvents(SignIn.ButtonType.SignIn,
                () => { },
                () => { },
                () => StartCoroutine(DisableSignIn(MenuType.RoomList)));

            signIn.SetActiveEvents(
                () => StartCoroutine(EnableSignIn()),
                () => StartCoroutine(DisableSignIn(MenuType.Start)));

            signUp.SetActiveEvents(
                () => StartCoroutine(EnableSignUp()),
                () => StartCoroutine(DisableSignUp()));
            signUp.SetupPointEvents(SignUp.ButtonType.Close,
                () =>
                {
                    if (signUp.closeButtonCoroutine != null)
                    {
                        StopCoroutine(signUp.closeButtonCoroutine);
                        signUp.closeButtonCoroutine = null;
                    }

                    signUp.closeButtonCoroutine = StartCoroutine(signUp.PointerEnterCloseButton());
                },
                () =>
                {
                    if (signUp.closeButtonCoroutine != null)
                    {
                        StopCoroutine(signUp.closeButtonCoroutine);
                        signUp.closeButtonCoroutine = null;
                    }

                    signUp.closeButtonCoroutine = StartCoroutine(signUp.PointerExitCloseButton());
                },
                () => StartCoroutine(DisableSignUp()));
            
            roomList.SetActiveEvents(
                () => StartCoroutine(EnableRoomList()),
                () => StartCoroutine(DisableRoomList(MenuType.Start)));
            roomList.SetupPointEvents(RoomList.ButtonType.Close,
                () =>
                {
                    if (roomList.closeButtonCoroutine != null)
                    {
                        StopCoroutine(roomList.closeButtonCoroutine);
                        roomList.closeButtonCoroutine = null;
                    }

                    roomList.closeButtonCoroutine = StartCoroutine(roomList.PointerEnterCloseButton());
                },
                () =>
                {
                    if (roomList.closeButtonCoroutine != null)
                    {
                        StopCoroutine(roomList.closeButtonCoroutine);
                        roomList.closeButtonCoroutine = null;
                    }

                    roomList.closeButtonCoroutine = StartCoroutine(roomList.PointerExitCloseButton());
                },
                () => StartCoroutine(DisableRoomList(MenuType.Start)));
            
            roomList.SetupPointEvents(RoomList.ButtonType.CreateRoom,
                () => { },
                () => { },
                () => StartCoroutine(DisableRoomList(MenuType.CreateRoom)));
            
            createRoom.SetActiveEvents(
                () => StartCoroutine(EnableCreateRoom()),
                () => StartCoroutine(DisableCreateRoom(MenuType.None)));
            createRoom.SetupPointEvents(CreateRoom.ButtonType.Close,
                () =>
                {
                    if (createRoom.closeButtonCoroutine != null)
                    {
                        StopCoroutine(createRoom.closeButtonCoroutine);
                        createRoom.closeButtonCoroutine = null;
                    }

                    createRoom.closeButtonCoroutine = StartCoroutine(createRoom.PointerEnterCloseButton());
                },
                () =>
                {
                    if (createRoom.closeButtonCoroutine != null)
                    {
                        StopCoroutine(createRoom.closeButtonCoroutine);
                        createRoom.closeButtonCoroutine = null;
                    }

                    createRoom.closeButtonCoroutine = StartCoroutine(createRoom.PointerExitCloseButton());
                },
                () => StartCoroutine(DisableCreateRoom(MenuType.Start)));
            
            createRoom.SetupPointEvents(CreateRoom.ButtonType.CreateRoom,
                () => { },
                () => { },
                () => StartCoroutine(DisableCreateRoom(MenuType.None)));
        }

        public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>() ?? gameObject.gameObject.AddComponent<T>();

            return component;
        }

        private void OnEnable()
        {
            StartCoroutine(OnEnableMenu(MenuType.Start));
        }

        private IEnumerator EnableStartMenu()
        {
            if (startMenu.coroutineButtons != null)
            {
                StopCoroutine(startMenu.coroutineButtons);
                startMenu.coroutineButtons = null;
            }

            startMenu.coroutineButtons = StartCoroutine(startMenu.PlayEnableAnimation());

            if (startMenu.coroutineTitle != null)
            {
                StopCoroutine(startMenu.coroutineTitle);
                startMenu.coroutineTitle = null;
            }

            startMenu.textTitle.gameObject.SetActive(true);
            
            startMenu.coroutineTitle = StartCoroutine(startMenu.PlayEnableTitleText());

            yield return startMenu.coroutineButtons;
            yield return startMenu.coroutineTitle;
            
            currentMenuType = MenuType.Start;
        }

        private IEnumerator DisableStartMenu()
        {
            if (startMenu.coroutineButtons != null)
            {
                StopCoroutine(startMenu.coroutineButtons);
                startMenu.coroutineButtons = null;
            }

            startMenu.coroutineButtons = StartCoroutine(startMenu.PlayDisableAnimation());

            if (startMenu.blurBackgroundCoroutine != null)
            {
                StopCoroutine(startMenu.blurBackgroundCoroutine);
                startMenu.blurBackgroundCoroutine = null;
            }
            startMenu.blurBackgroundCoroutine = StartCoroutine(startMenu.BlurBackground());

            yield return startMenu.blurBackgroundCoroutine;
            yield return startMenu.coroutineButtons;
            yield return startMenu.coroutineTitle;

            startMenu.root.gameObject.SetActive(false);

            currentMenuType = MenuType.None;
        }

        private IEnumerator EnableRoomList()
        {
            if (roomList.coroutineButtons != null)
            {
                StopCoroutine(roomList.coroutineButtons);
                roomList.coroutineButtons = null;
            }

            roomList.coroutineButtons = StartCoroutine(roomList.PlayEnableAnimation());

            yield return roomList.coroutineButtons;

            currentMenuType = MenuType.RoomList;
        }

        private IEnumerator DisableRoomList(MenuType menuType)
        {
            if (roomList.coroutineButtons != null)
            {
                StopCoroutine(roomList.coroutineButtons);
                roomList.coroutineButtons = null;
            }

            roomList.coroutineButtons = StartCoroutine(roomList.PlayDisableAnimation());
            yield return roomList.coroutineButtons;

            currentMenuType = MenuType.None;

            roomList.root.gameObject.SetActive(false);

            yield return OnEnableMenu(menuType);
        }

        private IEnumerator EnableSignIn()
        {
            if (signIn.coroutineButtons != null)
            {
                StopCoroutine(signIn.coroutineButtons);
                signIn.coroutineButtons = null;
            }

            signIn.coroutineButtons = StartCoroutine(signIn.PlayEnableAnimation());

            yield return signIn.coroutineButtons;

            currentMenuType = MenuType.SignIn;
        }

        private IEnumerator DisableSignIn(MenuType menuType)
        {
            if (signIn.coroutineButtons != null)
            {
                StopCoroutine(signIn.coroutineButtons);
                signIn.coroutineButtons = null;
            }

            signIn.coroutineButtons = StartCoroutine(signIn.PlayDisableAnimation());
            yield return signIn.coroutineButtons;

            currentMenuType = MenuType.None;

            signIn.root.gameObject.SetActive(false);

            yield return OnEnableMenu(menuType);
        }

        private IEnumerator EnableSignUp()
        {
            if (signUp.coroutineButtons != null)
            {
                StopCoroutine(signUp.coroutineButtons);
                signUp.coroutineButtons = null;
            }

            signUp.coroutineButtons = StartCoroutine(signUp.PlayEnableAnimation());

            yield return signUp.coroutineButtons;

            currentMenuType = MenuType.SignUp;
        }

        private IEnumerator DisableSignUp()
        {
            if (signUp.coroutineButtons != null)
            {
                StopCoroutine(signUp.coroutineButtons);
                signUp.coroutineButtons = null;
            }

            signUp.coroutineButtons = StartCoroutine(signUp.PlayDisableAnimation());
            yield return signUp.coroutineButtons;

            currentMenuType = MenuType.None;

            signUp.root.gameObject.SetActive(false);

            yield return OnEnableMenu(MenuType.SignIn);
        }

        private IEnumerator EnableCreateRoom()
        {
            if (createRoom.coroutineButtons != null)
            {
                StopCoroutine(createRoom.coroutineButtons);
                createRoom.coroutineButtons = null;
            }

            createRoom.coroutineButtons = StartCoroutine(createRoom.PlayEnableAnimation());

            yield return createRoom.coroutineButtons;

            currentMenuType = MenuType.CreateRoom;
        }

        private IEnumerator DisableCreateRoom(MenuType menuType)
        {
            if (createRoom.coroutineButtons != null)
            {
                StopCoroutine(createRoom.coroutineButtons);
                createRoom.coroutineButtons = null;
            }

            createRoom.coroutineButtons = StartCoroutine(createRoom.PlayDisableAnimation());
            
            
            if (startMenu.coroutineTitle != null)
            {
                StopCoroutine(startMenu.coroutineTitle);
                startMenu.coroutineTitle = null;
            }
            
            startMenu.coroutineTitle = StartCoroutine(startMenu.PlayDisableTitleText());

            if (startMenu.blurBackgroundCoroutine != null)
            {
                StopCoroutine(startMenu.blurBackgroundCoroutine);
                startMenu.blurBackgroundCoroutine = null;
            }
            startMenu.blurBackgroundCoroutine = StartCoroutine(startMenu.ClearBackground());

            
            yield return createRoom.coroutineButtons;

            currentMenuType = MenuType.None;

            createRoom.root.gameObject.SetActive(false);

            if (menuType != MenuType.None) yield return OnEnableMenu(menuType);
        }

        private IEnumerator OnEnableMenu(MenuType menuType)
        {
            switch (menuType)
            {
                case MenuType.Start:
                    // roomList.SetActive(false);
                    // TODO (OSS) Disable current menu and turn on start menu

                    startMenu.SetActive(true);
                    break;
                case MenuType.RoomList:
                    signIn.SetActive(false);

                    yield return new WaitUntil(() => currentMenuType == MenuType.None);

                    roomList.SetActive(true);
                    break;
                case MenuType.None:
                    break;
                case MenuType.SignIn:
                    startMenu.SetActive(false);

                    yield return new WaitUntil(() => currentMenuType == MenuType.None);

                    signIn.SetActive(true);
                    break;
                case MenuType.SignUp:
                    startMenu.SetActive(false);

                    yield return new WaitUntil(() => currentMenuType == MenuType.None);

                    signUp.SetActive(true);
                    break;
                case MenuType.CreateRoom:
                    roomList.SetActive(false);
                    
                    yield return new WaitUntil(() => currentMenuType == MenuType.None);

                    createRoom.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(menuType), menuType, null);
            }
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
                RectTransform roomButtonRectTransform = (RectTransform)roomButtonGameObject.transform;
                // roomButtonRectTransform.SetParent(roomListScrollViewContent.transform);
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

        // private IEnumerator OnEnableMenu(GameObject gameObject)
        // {
        //     Vector3 startScale = Vector3.zero;
        //     Vector3 targetScale = Vector3.one;
        //
        //     yield return ChangeGameObjectScale(gameObject, startScale, targetScale);
        // }
        //
        // private IEnumerator OnDisableMenu(GameObject gameObject)
        // {
        //     Vector3 startScale = Vector3.one;
        //     Vector3 targetScale = Vector3.zero;
        //
        //     yield return ChangeGameObjectScale(gameObject, startScale, targetScale);
        // }

        private IEnumerator OnPointerEnter(Button button)
        {
            if (startMenu.buttonCoroutines.TryGetValue(button, out Coroutine coroutine))
            {
                StopCoroutine(coroutine);
            }

            Vector3 startScale = button.gameObject.transform.localScale;
            Vector3 targetScale = new Vector3(1.2f, 1.2f, 1.2f);

            coroutine = StartCoroutine(ChangeGameObjectScale(button.gameObject, startScale, targetScale));

            yield return coroutine;
        }

        private IEnumerator OnPointerExit(Button button)
        {
            if (startMenu.buttonCoroutines.TryGetValue(button, out Coroutine coroutine))
            {
                StopCoroutine(coroutine);
            }

            Vector3 startScale = button.gameObject.transform.localScale;
            Vector3 targetScale = Vector3.one;

            coroutine = StartCoroutine(ChangeGameObjectScale(button.gameObject, startScale, targetScale));

            yield return coroutine;
        }

        private IEnumerator ChangeGameObjectScale(GameObject targetGameObject, Vector3 startScale, Vector3 targetScale)
        {
            Vector3 currentScale = startScale;

            float t = 0;
            while (t <= 1.0f)
            {
                float scale = Mathf.Lerp(startScale.x, targetScale.x, t);
                t += Time.deltaTime * 10;

                currentScale.x = currentScale.y = currentScale.y = scale;

                targetGameObject.transform.localScale = currentScale;

                yield return null;
            }

            targetGameObject.transform.localScale = targetScale;
        }
    }
}