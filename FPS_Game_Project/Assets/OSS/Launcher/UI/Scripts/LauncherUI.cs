using System;
using System.Collections;
using System.Collections.Generic;
using OSS.Launcher.UI;
using UnityEngine;
using UnityEngine.UI;

namespace OSS.Launcher.UI
{
    public class LauncherUI : MonoBehaviour
    {
        [SerializeField] private GameObject roomButtonItemPrefab;
        private readonly List<Button> roomButtonList = new List<Button>();

        private Dictionary<MenuType, Menu> menus;

        private void Awake()
        {
            Setup();
        }

        private void Setup()
        {
            menus = new Dictionary<MenuType, Menu> {
                [MenuType.Start] = new StartMenu(this, transform.Find("StartMenu")), 
                [MenuType.SignIn] = new SignIn(this, transform.Find("Sign In")),
                [MenuType.SignUp] = new SignUp(this, transform.Find("Sign Up")),
                [MenuType.RoomList] = new RoomList(this, transform.Find("RoomList")),
                [MenuType.CreateRoom] = new CreateRoom(this, transform.Find("Create Room"))
            };
        }

        private IEnumerator Start()
        {
            yield return Menu.SetActiveMenu(MenuType.Start);
        }

        public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>() ?? gameObject.gameObject.AddComponent<T>();

            return component;
        }

        // public void UpdateRoomList(in List<RoomInfo> roomInfoList)
        // {
        //     foreach (RoomInfo roomInfo in roomInfoList)
        //     {
        //         int index = roomButtonList.FindIndex(roomButton => roomButton.name == roomInfo.Name);
        //         if (index != -1 && roomInfo.RemovedFromList)
        //         {
        //             Destroy(roomButtonList[index].gameObject);
        //             roomButtonList.RemoveAt(index);
        //         }

        //         if (index != -1 || roomInfo.IsOpen != true || roomInfo.PlayerCount >= roomInfo.MaxPlayers) continue;

        //         GameObject roomButtonGameObject = Instantiate(roomButtonItemPrefab);
        //         RectTransform roomButtonRectTransform = (RectTransform)roomButtonGameObject.transform;
        //         // roomButtonRectTransform.SetParent(roomListScrollViewContent.transform);
        //         int lastIndex = roomButtonList.Count;
        //         Vector3 roomButtonPosition = roomButtonRectTransform.position;
        //         roomButtonPosition.y = lastIndex * roomButtonRectTransform.rect.height * -1;
        //         roomButtonRectTransform.position = roomButtonPosition;
        //         RectTransform roomButtonParentRectTransform = (RectTransform)roomButtonRectTransform.parent.transform;
        //         LayoutRebuilder.ForceRebuildLayoutImmediate(roomButtonParentRectTransform);

        //         Button roomButton = roomButtonGameObject.GetComponent<Button>();
        //         roomButton.name = roomInfo.Name;
        //         roomButton.GetComponentInChildren<Text>().text = roomInfo.Name;
        //         roomButton.onClick.AddListener(delegate { launcher.JoinRoom(roomInfo.Name); });
        //         roomButtonList.Add(roomButton);
        //     }
        // }

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