using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OSS.Launcher.UI
{
    public enum MenuType
    {
        None,
        Start,
        SignIn,
        SignUp,
        RoomList,
        CreateRoom
    }

    public class StartMenu
    {
        public enum ButtonType
        {
            Start,
            Quit
        }

        public Transform root { get; private set; }

        public Dictionary<ButtonType, Button> buttons { get; private set; }
        public Dictionary<Button, Coroutine> buttonCoroutines { get; private set; }

        public Coroutine coroutineButtons;
        public Animator animatorButtons { get; private set; }

        public Coroutine blurBackgroundCoroutine;

        public Text textTitle { get; private set; }
        private readonly Animator animatorTitle;
        public Coroutine coroutineTitle;

        private Image blurImage;

        private UnityAction onEnableAction;

        private UnityAction onDisableAction;

        private bool isDisable = false;

        public StartMenu(Transform transformParent)
        {
            root = transformParent;

            Transform parent = root.parent;
            Transform titleTransform = parent.Find("Title");
            textTitle = titleTransform.GetComponent<Text>();
            animatorTitle = titleTransform.GetComponent<Animator>();

            blurImage = parent.Find("Blur Panel").GetComponent<Image>();
            
            Material material = blurImage.material; 
            material.SetFloat("_Radius", 1);
            blurImage.gameObject.SetActive(false);

            buttons = new Dictionary<ButtonType, Button>
            {
                [ButtonType.Start] = transformParent.Find("Start").GetComponent<Button>(),
                [ButtonType.Quit] = transformParent.Find("Quit").GetComponent<Button>()
            };

            animatorButtons = root.GetComponent<Animator>();
            
            buttonCoroutines = new Dictionary<Button, Coroutine>();
        }

        public void SetupPointEvents(ButtonType buttonType, UnityAction pointerEnterAction,
            UnityAction pointerExitAction, UnityAction pointerClickAction)
        {
            EventTrigger eventTrigger =
                LauncherUI.GetOrAddComponent<EventTrigger>(buttons[buttonType].gameObject);

            EventTrigger.Entry entryPointerEnter =
                new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entryPointerEnter.callback.AddListener(delegate
            {
                // if (isDisable == false)
                    pointerEnterAction();
            });
            eventTrigger.triggers.Add(entryPointerEnter);

            EventTrigger.Entry entryPointerExit =
                new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryPointerExit.callback.AddListener(delegate
            {
                // if (isDisable == false)
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
                root.gameObject.SetActive(true);
                onEnableAction();
            }
            else
            {
                isDisable = true;
                onDisableAction();
            }
        }

        public IEnumerator PlayEnableTitleText()
        {   
            textTitle.gameObject.SetActive(true);
            yield return PlayTextAnimation("Title_OnEnable");
        }

        public IEnumerator PlayDisableTitleText()
        {
            yield return PlayTextAnimation("Title_OnDisable");
            textTitle.gameObject.SetActive(false);
        }

        private IEnumerator PlayTextAnimation(string clipName)
        {
            while (animatorTitle.enabled == false || animatorTitle.gameObject.activeSelf == false) yield return null;
            
            animatorTitle.Play(clipName);
            RuntimeAnimatorController animController = animatorTitle.runtimeAnimatorController;
            AnimationClip clip = animController.animationClips.First(animationClip => animationClip.name == clipName);

            yield return new WaitForSeconds(clip.length);


            coroutineTitle = null;
        }

        public IEnumerator PlayEnableAnimation()
        {
            yield return PlayButtonAnimation("OnEnable_StartMenuButtons");
        }

        public IEnumerator PlayDisableAnimation()
        {
            yield return PlayButtonAnimation("OnDisable_StartMenuButtons");
        }

        private IEnumerator PlayButtonAnimation(string clipName)
        {
            while (animatorButtons.enabled == false || root.gameObject.activeSelf == false) yield return null;
            
            animatorButtons.Play(clipName);
            RuntimeAnimatorController animatorController = animatorButtons.runtimeAnimatorController;
            AnimationClip animationClip =
                animatorController.animationClips.First(animationClip => animationClip.name == clipName);

            yield return new WaitForSeconds(animationClip.length);

            coroutineButtons = null;
        }
        
        public IEnumerator BlurBackground()
        {
            Material material = blurImage.material;
            
            const float maxValue = 4.0f;
            float currentValue = material.GetFloat("_Radius");

            yield return BackgroundBlurred(currentValue, maxValue);
        }

        public IEnumerator ClearBackground()
        {
            Material material = blurImage.material;
            
            const float maxValue = 1.0f;
            float currentValue = material.GetFloat("_Radius");

            yield return BackgroundBlurred(currentValue, maxValue);
            
            blurImage.gameObject.SetActive(false);
        }

        private IEnumerator BackgroundBlurred(float current, float max)
        {
            blurImage.gameObject.SetActive(true);
            Material material = blurImage.material;
            
            float t = 0;
            while (t <= 1.0f)
            {
                float radius = Mathf.Lerp(current, max, t);
                material.SetFloat("_Radius", radius);

                t += Time.deltaTime * 10;

                yield return null;
            }
        }
    }

    public class SignIn
    {
        public enum ButtonType
        {
            Close,
            SignUp,
            SignIn
        }

        public Transform root { get; private set; }

        private Dictionary<ButtonType, Button> buttons { get; set; }

        public Coroutine coroutineButtons;

        public Coroutine closeButtonCoroutine;

        public Animator animator { get; private set; }
        
        private bool isDisable = false;
        private UnityAction onEnableAction;
        private UnityAction onDisableAction;

        public SignIn(Transform parent)
        {
            root = parent;
            animator = root.GetComponent<Animator>();

            buttons = new Dictionary<ButtonType, Button>
            {
                [ButtonType.Close] = root.Find("Close").GetComponent<Button>(),
                [ButtonType.SignIn] = root.Find("Buttons/Sign In").GetComponent<Button>(),
                [ButtonType.SignUp] = root.Find("Buttons/Sign Up").GetComponent<Button>()
            };
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                isDisable = false;
                root.gameObject.SetActive(active);
                onEnableAction();
            }
            else
            {
                isDisable = true;
                onDisableAction();
            }
        }

        public void SetActiveEvents(UnityAction enableAction, UnityAction disableAction)
        {
            this.onEnableAction = enableAction;
            this.onDisableAction = disableAction;
        }
        
        public void SetupPointEvents(ButtonType buttonType, UnityAction pointerEnterAction,
            UnityAction pointerExitAction, UnityAction pointerClickAction)
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
        
        public IEnumerator PlayEnableAnimation()
        {
            yield return PlayButtonAnimation("OnEnable_SignIn");
        }

        public IEnumerator PlayDisableAnimation()
        {
            yield return PlayButtonAnimation("OnDisable_SignIn");
        }

        private IEnumerator PlayButtonAnimation(string clipName)
        {
            while (animator.enabled == false || root.gameObject.activeSelf == false) yield return null;
            
            animator.Play(clipName);
            RuntimeAnimatorController animatorController = animator.runtimeAnimatorController;
            AnimationClip animationClip =
                animatorController.animationClips.First(animationClip => animationClip.name == clipName);

            yield return new WaitForSeconds(animationClip.length);

            coroutineButtons = null;
        }

        public IEnumerator PointerEnterCloseButton()
        {
            Transform closeIconTransform = buttons[ButtonType.Close].transform.Find("close button foreground");
            yield return SpinImage(closeIconTransform, closeIconTransform.rotation.eulerAngles,
                new Vector3(0, 0, 90.0f));
        }

        public IEnumerator PointerExitCloseButton()
        {
            Transform closeIconTransform = buttons[ButtonType.Close].transform.Find("close button foreground");
            yield return SpinImage(closeIconTransform, closeIconTransform.rotation.eulerAngles,
                Vector3.zero);
        }

        private IEnumerator SpinImage(Transform targetObjectTransform, Vector3 startAngle, Vector3 targetAngle)
        {
            float t = 0;
            while (t <= 1.0f)
            {
                Vector3 current = Vector3.Lerp(startAngle, targetAngle, t);
                t += Time.deltaTime * 10;

                targetObjectTransform.rotation= Quaternion.Euler(current);
                
                yield return null;
            }
        }
    }

    public class SignUp
    {
        public enum ButtonType
        {
            Close,
            SignUp,
            Check
        }

        public Transform root { get; private set; }

        private Dictionary<ButtonType, Button> buttons { get; set; }

        public Coroutine coroutineButtons;

        public Coroutine closeButtonCoroutine;

        public Animator animator { get; private set; }
        
        private bool isDisable = false;
        private UnityAction onEnableAction;
        private UnityAction onDisableAction;

        public SignUp(Transform parent)
        {
            root = parent;
            animator = root.GetComponent<Animator>();
            
            buttons = new Dictionary<ButtonType, Button>
            {
                [ButtonType.Close] = root.Find("Close").GetComponent<Button>(),
                [ButtonType.SignUp] = root.Find("Sign Up").GetComponent<Button>()
            };
        }
        
        public void SetupPointEvents(ButtonType buttonType, UnityAction pointerEnterAction,
            UnityAction pointerExitAction, UnityAction pointerClickAction)
        {
            EventTrigger eventTrigger =
                LauncherUI.GetOrAddComponent<EventTrigger>(buttons[buttonType].gameObject);

            EventTrigger.Entry entryPointerEnter =
                new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entryPointerEnter.callback.AddListener(delegate
            {
                pointerEnterAction();
            });
            eventTrigger.triggers.Add(entryPointerEnter);

            EventTrigger.Entry entryPointerExit =
                new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryPointerExit.callback.AddListener(delegate
            {
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
            if (active)
            {
                isDisable = false;
                root.gameObject.SetActive(active);
                onEnableAction();
            }
            else
            {
                isDisable = true;
                onDisableAction();
            }
        }
        
        public IEnumerator PlayEnableAnimation()
        {
            yield return PlayButtonAnimation("OnEnable_SignUp");
        }

        public IEnumerator PlayDisableAnimation()
        {
            yield return PlayButtonAnimation("OnDisable_SignUp");
        }

        private IEnumerator PlayButtonAnimation(string clipName)
        {
            while (animator.enabled == false || root.gameObject.activeSelf == false) yield return null;
            
            animator.Play(clipName);
            RuntimeAnimatorController animatorController = animator.runtimeAnimatorController;
            AnimationClip animationClip =
                animatorController.animationClips.First(animationClip => animationClip.name == clipName);

            yield return new WaitForSeconds(animationClip.length);

            coroutineButtons = null;
        }
        
        public IEnumerator PointerEnterCloseButton()
        {
            Transform closeIconTransform = buttons[ButtonType.Close].transform.Find("close button foreground");
            yield return SpinImage(closeIconTransform, closeIconTransform.rotation.eulerAngles,
                new Vector3(0, 0, 90.0f));
        }

        public IEnumerator PointerExitCloseButton()
        {
            Transform closeIconTransform = buttons[ButtonType.Close].transform.Find("close button foreground");
            yield return SpinImage(closeIconTransform, closeIconTransform.rotation.eulerAngles,
                Vector3.zero);
        }

        private IEnumerator SpinImage(Transform targetObjectTransform, Vector3 startAngle, Vector3 targetAngle)
        {
            float t = 0;
            while (t <= 1.0f)
            {
                Vector3 current = Vector3.Lerp(startAngle, targetAngle, t);
                t += Time.deltaTime * 10;

                targetObjectTransform.rotation= Quaternion.Euler(current);
                
                yield return null;
            }
        }
    }
    

    public class RoomList
    {
        public enum ButtonType
        {
            Close,
            CreateRoom
        }

        public Transform root { get; private set; }

        private Dictionary<ButtonType, Button> buttons { get; set; }

        public Coroutine coroutineButtons;

        public Coroutine closeButtonCoroutine;

        public Animator animator { get; private set; }
        
        private bool isDisable = false;
        private UnityAction onEnableAction;
        private UnityAction onDisableAction;

        public RoomList(Transform root)
        {
            this.root = root;
            animator = root.GetComponent<Animator>();
            
            buttons = new Dictionary<ButtonType, Button>
            {
                [ButtonType.Close] = root.Find("Close").GetComponent<Button>(),
                [ButtonType.CreateRoom] = root.Find("ListView/Room List/Button CreateRoom").GetComponent<Button>()
            };
        }
        
        public void SetupPointEvents(ButtonType buttonType, UnityAction pointerEnterAction,
            UnityAction pointerExitAction, UnityAction pointerClickAction)
        {
            EventTrigger eventTrigger =
                LauncherUI.GetOrAddComponent<EventTrigger>(buttons[buttonType].gameObject);

            EventTrigger.Entry entryPointerEnter =
                new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entryPointerEnter.callback.AddListener(delegate
            {
                pointerEnterAction();
            });
            eventTrigger.triggers.Add(entryPointerEnter);

            EventTrigger.Entry entryPointerExit =
                new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryPointerExit.callback.AddListener(delegate
            {
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
            if (active)
            {
                isDisable = false;
                root.gameObject.SetActive(active);
                onEnableAction();
            }
            else
            {
                isDisable = true;
                onDisableAction();
            }
        }
        
        public IEnumerator PlayEnableAnimation()
        {
            yield return PlayButtonAnimation("OnEnable_RoomListAnimation");
        }

        public IEnumerator PlayDisableAnimation()
        {
            yield return PlayButtonAnimation("OnDisable_RoomListAnimation");
        }

        private IEnumerator PlayButtonAnimation(string clipName)
        {
            while (animator.enabled == false || root.gameObject.activeSelf == false) yield return null;
            
            animator.Play(clipName);
            RuntimeAnimatorController animatorController = animator.runtimeAnimatorController;
            AnimationClip animationClip =
                animatorController.animationClips.First(animationClip => animationClip.name == clipName);

            yield return new WaitForSeconds(animationClip.length);

            coroutineButtons = null;
        }
        
        public IEnumerator PointerEnterCloseButton()
        {
            Transform closeIconTransform = buttons[ButtonType.Close].transform.Find("close button foreground");
            yield return SpinImage(closeIconTransform, closeIconTransform.rotation.eulerAngles,
                new Vector3(0, 0, 90.0f));
        }

        public IEnumerator PointerExitCloseButton()
        {
            Transform closeIconTransform = buttons[ButtonType.Close].transform.Find("close button foreground");
            yield return SpinImage(closeIconTransform, closeIconTransform.rotation.eulerAngles,
                Vector3.zero);
        }

        private IEnumerator SpinImage(Transform targetObjectTransform, Vector3 startAngle, Vector3 targetAngle)
        {
            float t = 0;
            while (t <= 1.0f)
            {
                Vector3 current = Vector3.Lerp(startAngle, targetAngle, t);
                t += Time.deltaTime * 10;

                targetObjectTransform.rotation= Quaternion.Euler(current);
                
                yield return null;
            }
        }
    }

    public class CreateRoom
    {
        public enum ButtonType
        {
            Close,
            CreateRoom
        }

        public Transform root { get; private set; }

        private Dictionary<ButtonType, Button> buttons { get; set; }

        public Coroutine coroutineButtons;

        public Coroutine closeButtonCoroutine;

        public Animator animator { get; private set; }
        
        private bool isDisable = false;
        private UnityAction onEnableAction;
        private UnityAction onDisableAction;

        public CreateRoom(Transform parent)
        {
            this.root = parent;
            animator = root.GetComponent<Animator>();
            
            buttons = new Dictionary<ButtonType, Button>
            {
                [ButtonType.Close] = root.Find("Header/Close").GetComponent<Button>(),
                [ButtonType.CreateRoom] = root.Find("Create").GetComponent<Button>()
            };
        }
        
         public void SetupPointEvents(ButtonType buttonType, UnityAction pointerEnterAction,
            UnityAction pointerExitAction, UnityAction pointerClickAction)
        {
            EventTrigger eventTrigger =
                LauncherUI.GetOrAddComponent<EventTrigger>(buttons[buttonType].gameObject);

            EventTrigger.Entry entryPointerEnter =
                new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entryPointerEnter.callback.AddListener(delegate
            {
                pointerEnterAction();
            });
            eventTrigger.triggers.Add(entryPointerEnter);

            EventTrigger.Entry entryPointerExit =
                new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryPointerExit.callback.AddListener(delegate
            {
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
            if (active)
            {
                isDisable = false;
                root.gameObject.SetActive(active);
                onEnableAction();
            }
            else
            {
                isDisable = true;
                onDisableAction();
            }
        }
        
        public IEnumerator PlayEnableAnimation()
        {
            yield return PlayButtonAnimation("OnEnable_CreateRoom");
        }

        public IEnumerator PlayDisableAnimation()
        {
            yield return PlayButtonAnimation("OnDisable_CreateRoom");
        }

        private IEnumerator PlayButtonAnimation(string clipName)
        {
            while (animator.enabled == false || root.gameObject.activeSelf == false) yield return null;
            
            animator.Play(clipName);
            RuntimeAnimatorController animatorController = animator.runtimeAnimatorController;
            AnimationClip animationClip =
                animatorController.animationClips.First(animationClip => animationClip.name == clipName);

            yield return new WaitForSeconds(animationClip.length);

            coroutineButtons = null;
        }
        
        public IEnumerator PointerEnterCloseButton()
        {
            Transform closeIconTransform = buttons[ButtonType.Close].transform.Find("close button foreground");
            yield return SpinImage(closeIconTransform, closeIconTransform.rotation.eulerAngles,
                new Vector3(0, 0, 90.0f));
        }

        public IEnumerator PointerExitCloseButton()
        {
            Transform closeIconTransform = buttons[ButtonType.Close].transform.Find("close button foreground");
            yield return SpinImage(closeIconTransform, closeIconTransform.rotation.eulerAngles,
                Vector3.zero);
        }

        private IEnumerator SpinImage(Transform targetObjectTransform, Vector3 startAngle, Vector3 targetAngle)
        {
            float t = 0;
            while (t <= 1.0f)
            {
                Vector3 current = Vector3.Lerp(startAngle, targetAngle, t);
                t += Time.deltaTime * 10;

                targetObjectTransform.rotation= Quaternion.Euler(current);
                
                yield return null;
            }
        }
    }
}