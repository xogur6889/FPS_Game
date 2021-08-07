using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OSS.Launcher.UI
{
    public enum MenuType
    {
        Main,
        RoomList
    }
    
    public struct MainMenu
    {
        private Button buttonStart;
        private Button buttonQuit;
        private readonly Animator animatorMainMenu;

        public MainMenu(Transform transformParent)
        {
            buttonStart = transformParent.Find("Start").GetComponent<Button>();
            buttonQuit = transformParent.Find("Quit").GetComponent<Button>();

            animatorMainMenu = transformParent.GetComponent<Animator>();
        }

        public IEnumerator OnEnable()
        {
            yield return PlayAnimation("OnEnable");
        }
        
        public IEnumerator OnDisable()
        {
            yield return PlayAnimation("OnDisable");
        }

        private IEnumerator PlayAnimation(string animationClipName)
        {
            animatorMainMenu.Play(animationClipName);
            yield return new WaitForEndOfFrame();

            int length = animatorMainMenu.GetCurrentAnimatorClipInfo(0).Length;

            yield return new WaitForSeconds(length);
        }
    }

    public struct RoomList
    {
        public UnityEngine.UIElements.ListView ListViewRoomList;
        public Animator animatorRoomList;
    }
}