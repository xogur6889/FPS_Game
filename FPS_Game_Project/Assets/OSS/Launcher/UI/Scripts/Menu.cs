using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using OSS.Multiplay.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OSS.Launcher.UI
{
    public enum MenuType
    {
        None,
        Main,
        SignIn,
        SignUp,
        RoomList,
        CreateRoom
    }

    public class MainMenu
    {
        public enum ButtonType
        {
            Start,
            Quit
        }

        private readonly Transform root;

        public Dictionary<ButtonType, Button> buttons { get; private set; }
        public Dictionary<Button, Coroutine> buttonCoroutines { get; private set; }

        private readonly Animator animatorButtons;
        
        public Text textTitle { get; private set; }
        private readonly Animator animatorTitle;
        public Coroutine coroutineTitle;

        private UnityAction onEnableAction;

        private UnityAction onDisableAction;
        
        private bool isDisable = false;

        public MainMenu(Transform transformParent)
        {
            root = transformParent;
            
            Transform titleTransform = root.parent.Find("Title");
            textTitle = titleTransform.GetComponent<Text>();
            animatorTitle = titleTransform.GetComponent<Animator>();
            
            buttons = new Dictionary<ButtonType, Button>
            {
                [ButtonType.Start] = transformParent.Find("Start").GetComponent<Button>(),
                [ButtonType.Quit] = transformParent.Find("Quit").GetComponent<Button>()
            };
            
            buttonCoroutines = new Dictionary<Button, Coroutine>();
        }

        public void SetupPointEvents(ButtonType buttonType, UnityAction pointerEnterAction, UnityAction pointerExitAction, UnityAction pointerClickAction)
        {
            EventTrigger eventTrigger =
                LauncherUI.GetOrAddComponent<EventTrigger>(buttons[buttonType].gameObject);

            EventTrigger.Entry entryPointerEnter =
                new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entryPointerEnter.callback.AddListener(delegate
            {
                if (isDisable == false)
                    pointerEnterAction();
            });
            eventTrigger.triggers.Add(entryPointerEnter);

            EventTrigger.Entry entryPointerExit =
                new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryPointerExit.callback.AddListener(delegate
            {
                if (isDisable == false)
                    pointerExitAction();
            });
            eventTrigger.triggers.Add(entryPointerExit);

            EventTrigger.Entry entryPointerClick = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entryPointerClick.callback.AddListener(delegate
            {
                if (isDisable == false)
                    pointerClickAction();
            });
            eventTrigger.triggers.Add(entryPointerClick);
        }

        public void SetActiveEvents(UnityAction enableAction, UnityAction disableAction)
        {
            this.onEnableAction = enableAction;
            this.onDisableAction = disableAction;
        }

        public void SetActive(bool active)
        {
            if (active == true)
            {
                isDisable = false;
                onEnableAction();
            }
            else
            {
                isDisable = true;
                onDisableAction();
            }
        }

        public IEnumerator PlayTextAnimation(string clipName)
        {
            animatorTitle.Play(clipName);
            RuntimeAnimatorController animController = animatorTitle.runtimeAnimatorController;
            AnimationClip clip = animController.animationClips.First(animationClip => animationClip.name == clipName);

            yield return new WaitForSeconds(clip.length);
            

            coroutineTitle = null;
        }
    }

    public class RoomList
    {
        public Transform root { get; private set; }

        
        private UnityAction onEnableAction;
        private UnityAction onDisableAction;
        
        public RoomList(Transform root)
        {
            this.root = root;
        }
        
        public void SetActiveEvents(UnityAction enableAction, UnityAction disableAction)
        {
            this.onEnableAction = enableAction;
            this.onDisableAction = disableAction;
        }
        
        public void SetActive(bool active)
        {
            root.gameObject.SetActive(active);

            if (active)
                onEnableAction();
            else
                onDisableAction();
        }
    }
}