using System;
using System.Collections;
using System.Collections.Generic;
using OSS.Launcher.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
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

        private MenuType currentMenuType = MenuType.Main;

        private void Awake()
        {
            Setup();
        }

        private void Setup()
        {
            mainMenu = new MainMenu(transform.Find("MainMenu"));
            roomList = new RoomList(transform.Find("RoomList"));

            SetupButtonEvents();
        }

        private void SetupButtonEvents()
        {
            mainMenu.SetupPointEvents(MainMenu.ButtonType.Start,
                () => StartCoroutine(OnPointerEnter(mainMenu.buttons[MainMenu.ButtonType.Start])),
                () => StartCoroutine(OnPointerExit(mainMenu.buttons[MainMenu.ButtonType.Start])),
                () => StartCoroutine(OnEnableMenu(MenuType.RoomList)));
            mainMenu.SetupPointEvents(MainMenu.ButtonType.Quit,
                () => StartCoroutine(OnPointerEnter(mainMenu.buttons[MainMenu.ButtonType.Quit])),
                () => StartCoroutine(OnPointerExit(mainMenu.buttons[MainMenu.ButtonType.Quit])),
                () =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                });

            mainMenu.SetActiveEvents(
                () => StartCoroutine(EnableMainMenu()),
                () => StartCoroutine(DisableMainMenu()));

            roomList.SetActiveEvents(
                () => StartCoroutine(EnableRoomList()),
                () => StartCoroutine(DisableRoomList()));
        }

        public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>() ?? gameObject.gameObject.AddComponent<T>();

            return component;
        }

        private void OnEnable()
        {
            StartCoroutine(OnEnableMenu(MenuType.Main));
        }

        private IEnumerator EnableMainMenu()
        {
            if (mainMenu.buttonCoroutines.TryGetValue(mainMenu.buttons[MainMenu.ButtonType.Start],
                out Coroutine startButtonCoroutine))
            {
                StopCoroutine(startButtonCoroutine);
            }
            startButtonCoroutine = StartCoroutine(OnEnableMenu(mainMenu.buttons[MainMenu.ButtonType.Start].gameObject));

            if (mainMenu.buttonCoroutines.TryGetValue(mainMenu.buttons[MainMenu.ButtonType.Start],
                out Coroutine quitButtonCoroutine))
            {
                StopCoroutine(quitButtonCoroutine);
            }
            quitButtonCoroutine =
                StartCoroutine(OnEnableMenu(mainMenu.buttons[MainMenu.ButtonType.Quit].gameObject));

            if (mainMenu.coroutineTitle != null)
            {
                StopCoroutine(mainMenu.coroutineTitle);
                mainMenu.coroutineTitle = null;
            }

            mainMenu.textTitle.gameObject.SetActive(true);
            mainMenu.coroutineTitle = StartCoroutine(mainMenu.PlayTextAnimation("Title_OnEnable"));

            yield return startButtonCoroutine;
            yield return quitButtonCoroutine;
            yield return mainMenu.coroutineTitle;

            Debug.Log("Finished enable start menu");

            currentMenuType = MenuType.Main;
        }

        private IEnumerator DisableMainMenu()
        {
            if (mainMenu.buttonCoroutines.TryGetValue(mainMenu.buttons[MainMenu.ButtonType.Start],
                out Coroutine startButtonCoroutine))
            {
                StopCoroutine(startButtonCoroutine);
            }
            startButtonCoroutine = StartCoroutine(OnDisableMenu(mainMenu.buttons[MainMenu.ButtonType.Start].gameObject));

            if (mainMenu.buttonCoroutines.TryGetValue(mainMenu.buttons[MainMenu.ButtonType.Start],
                out Coroutine quitButtonCoroutine))
            {
                StopCoroutine(quitButtonCoroutine);
            }
            quitButtonCoroutine =
                StartCoroutine(OnDisableMenu(mainMenu.buttons[MainMenu.ButtonType.Quit].gameObject));

            if (mainMenu.coroutineTitle != null)
            {
                StopCoroutine(mainMenu.coroutineTitle);
                mainMenu.coroutineTitle = null;
            }

            mainMenu.coroutineTitle = StartCoroutine(mainMenu.PlayTextAnimation("Title_OnDisable"));

            yield return startButtonCoroutine;
            yield return quitButtonCoroutine;
            yield return mainMenu.coroutineTitle;
            
            mainMenu.textTitle.gameObject.SetActive(false);
            currentMenuType = MenuType.None;
        }

        private IEnumerator EnableRoomList()
        {
            yield return ChangeGameObjectScale(roomList.root.gameObject, Vector3.zero, Vector3.one);

            currentMenuType = MenuType.RoomList;
        }

        private IEnumerator DisableRoomList()
        {
            yield return ChangeGameObjectScale(roomList.root.gameObject, Vector3.one, Vector3.zero);

            currentMenuType = MenuType.None;
        }

        private IEnumerator OnEnableMenu(MenuType menuType)
        {
            switch (menuType)
            {
                case MenuType.Main:
                    // roomList.SetActive(false);
                    // TODO (OSS) Disable current menu and turn on start menu
                    
                    mainMenu.SetActive(true);
                    break;
                case MenuType.RoomList:
                    mainMenu.SetActive(false);
                    
                    yield return new WaitUntil(() => currentMenuType == MenuType.None);
                    
                    roomList.SetActive(true);
                    break;
                case MenuType.None:
                    break;
                case MenuType.SignIn:
                    break;
                case MenuType.SignUp:
                    break;
                case MenuType.CreateRoom:
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

        private IEnumerator OnEnableMenu(GameObject gameObject)
        {
            Vector3 startScale = Vector3.zero;
            Vector3 targetScale = Vector3.one;

            yield return ChangeGameObjectScale(gameObject, startScale, targetScale);
        }

        private IEnumerator OnDisableMenu(GameObject gameObject)
        {
            Vector3 startScale = Vector3.one;
            Vector3 targetScale = Vector3.zero;

            yield return ChangeGameObjectScale(gameObject, startScale, targetScale);
        }

        private IEnumerator OnPointerEnter(Button button)
        {
            if (mainMenu.buttonCoroutines.TryGetValue(button, out Coroutine coroutine))
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
            if (mainMenu.buttonCoroutines.TryGetValue(button, out Coroutine coroutine))
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