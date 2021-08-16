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
                () => OnPointerEnter(mainMenu.buttons[MainMenu.ButtonType.Start]),
                () => OnPointerExit(mainMenu.buttons[MainMenu.ButtonType.Start]),
                () => StartCoroutine(OnEnableMenu(MenuType.RoomList)));
            mainMenu.SetupPointEvents(MainMenu.ButtonType.Quit,
                () => OnPointerEnter(mainMenu.buttons[MainMenu.ButtonType.Quit]),
                () => OnPointerExit(mainMenu.buttons[MainMenu.ButtonType.Quit]),
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
            OnEnable(MenuType.Main, mainMenu.buttons[MainMenu.ButtonType.Start]);
            OnEnable(MenuType.Main, mainMenu.buttons[MainMenu.ButtonType.Quit]);

            if (mainMenu.coroutineTitle != null)
            {
                StopCoroutine(mainMenu.coroutineTitle);
                mainMenu.coroutineTitle = null;
            }

            mainMenu.textTitle.gameObject.SetActive(true);
            yield return null;
            
            yield return mainMenu.PlayTextAnimation("Title_OnEnable");

            currentMenuType = MenuType.Main;
        }

        private IEnumerator DisableMainMenu()
        {
            Coroutine startButtonCoroutine = StartCoroutine(OnDisable(mainMenu.buttons[MainMenu.ButtonType.Start].gameObject));
            Coroutine quitButtonCoroutine = StartCoroutine(OnDisable(mainMenu.buttons[MainMenu.ButtonType.Quit].gameObject));
                    
            if (mainMenu.coroutineTitle != null)
            {
                StopCoroutine(mainMenu.coroutineTitle);
                mainMenu.coroutineTitle = null;
            }

            Coroutine titleTextCoroutine = StartCoroutine(mainMenu.PlayTextAnimation("Title_OnDisable"));
            mainMenu.textTitle.gameObject.SetActive(false);

            currentMenuType = MenuType.None;

            while (startButtonCoroutine != null && quitButtonCoroutine != null && titleTextCoroutine != null) yield return null;
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
                    roomList.SetActive(false);

                    yield return new WaitUntil(() => currentMenuType == MenuType.None);
                    
                    mainMenu.SetActive(true);
                    break;
                case MenuType.RoomList:
                    mainMenu.SetActive(false);
                    // roomList.SetActive(true);
                    break;
            }
        }

//         public IEnumerator SetMenuEnabled(MenuState menuState)
//         {
//             mainMenuAnimator.Play("OnDisable");
//
//             yield return new WaitForSeconds(0.1f);
//             
//             switch (menuState)
//             {
//                 case MenuState.MainMenu:
//                     roomListRootTransform.gameObject.SetActive(false);
//
//                     mainMenuRootTransform.gameObject.SetActive(true);
//                     break;
//                 case MenuState.LaunchStart:
//                     mainMenuRootTransform.gameObject.SetActive(false);
//
//                     roomListRootTransform.gameObject.SetActive(true);
//                     break;
//                 case MenuState.Quit:
// #if UNITY_EDITOR
//                     UnityEditor.EditorApplication.isPlaying = false;
// #else
//                     Application.Quit();
// #endif
//                     break;
//                 default:
//                     throw new ArgumentOutOfRangeException(nameof(menuState), menuState, null);
//             }
//         }

        private void OnEnable(in MenuType menuType, in Button button)
        {
            Coroutine coroutine;
            
            switch (menuType)
            {
                case MenuType.Main:
                    if (mainMenu.buttonCoroutines.TryGetValue(button, out coroutine))
                    {
                        StopCoroutine(coroutine);
                    }
                    break;
                case MenuType.RoomList:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(menuType), menuType, null);
            }

            coroutine = StartCoroutine(OnEnable(button.gameObject));
        }

        private void OnDisable(in MenuType menuType, in Button button)
        {
            Coroutine coroutine;
            
            switch (menuType)
            {
                case MenuType.Main:
                    if (mainMenu.buttonCoroutines.TryGetValue(button, out coroutine))
                    {
                        StopCoroutine(coroutine);
                    }
                    break;
                case MenuType.RoomList:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(menuType), menuType, null);
            }

            coroutine = StartCoroutine(OnDisable(button.gameObject));
        }

        private void OnPointerEnter(in Button button)
        {
            if (mainMenu.buttonCoroutines.TryGetValue(button, out Coroutine coroutine))
            {
                StopCoroutine(coroutine);
            }

            coroutine = StartCoroutine(OnPointerEnter(button.gameObject));
        }

        private void OnPointerExit(in Button button)
        {
            if (mainMenu.buttonCoroutines.TryGetValue(button, out Coroutine coroutine))
            {
                StopCoroutine(coroutine);
            }

            coroutine = StartCoroutine(OnPointerExit(button.gameObject));
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

        private IEnumerator OnEnable(GameObject gameObject)
        {
            Vector3 startScale = Vector3.zero;
            Vector3 targetScale = Vector3.one;

            yield return ChangeGameObjectScale(gameObject, startScale, targetScale);
        }

        private IEnumerator OnDisable(GameObject gameObject)
        {
            Vector3 startScale = Vector3.one;
            Vector3 targetScale = Vector3.one;

            yield return ChangeGameObjectScale(gameObject, startScale, targetScale);
        }

        private IEnumerator OnPointerEnter(GameObject gameObject)
        {
            Vector3 startScale = gameObject.transform.localScale;
            Vector3 targetScale = new Vector3(1.2f, 1.2f, 1.2f);

            yield return ChangeGameObjectScale(gameObject, startScale, targetScale);
        }

        private IEnumerator OnPointerExit(GameObject gameObject)
        {
            Vector3 startScale = gameObject.transform.localScale;
            Vector3 targetScale = Vector3.one;

            yield return ChangeGameObjectScale(gameObject, startScale, targetScale);
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