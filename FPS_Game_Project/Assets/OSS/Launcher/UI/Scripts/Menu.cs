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

    public enum ButtonType
    {
        Start,
        Close,
        CreateRoom,
        SignUp,
        SignIn,
        Check
    }

    public abstract class Menu
    {
        protected static MenuType CurrentMenuType = MenuType.None;
        protected static readonly Dictionary<MenuType, Menu> Menus = new Dictionary<MenuType, Menu>();

        protected readonly Transform Root;
        private readonly MonoBehaviour Mono;

        protected Dictionary<Button, Coroutine> ButtonCoroutines;
        protected Dictionary<ButtonType, Button> Buttons;

        protected Coroutine CoroutineRoot;
        private readonly Animator animatorRoot;

        private UnityAction onEnableAction;
        private UnityAction onDisableAction;

        protected abstract IEnumerator OnEnable();
        protected abstract IEnumerator OnDisable(bool all = false);

        protected Server server;

        protected Menu(MonoBehaviour monoBehaviour, Transform root)
        {
            this.Mono = monoBehaviour;
            this.Root = root;

            server = monoBehaviour.GetComponent<Server>();

            Menu.CurrentMenuType = MenuType.None;

            animatorRoot = this.Root.GetComponent<Animator>();
            if (animatorRoot == null) Debug.LogError("Root ui object doesn't have animator component");

            SetActiveEvents(
                () => StartCoroutine(ref CoroutineRoot, OnEnable()),
                () => StartCoroutine(ref CoroutineRoot, OnDisable()));
        }

        public static IEnumerator SetActiveMenu(MenuType menuType)
        {
            if (Menu.CurrentMenuType != MenuType.None) Menus[Menu.CurrentMenuType].SetActive(false);

            yield return new WaitUntil(() => Menu.CurrentMenuType == MenuType.None);

            Menus[menuType].SetActive(true);
        }

        protected void SetupPointEvents(ButtonType buttonType, MenuType menuType)
        {
            Button button = Buttons[buttonType];
            if (ButtonCoroutines.TryGetValue(button, out Coroutine coroutine) == false)
            {
                ButtonCoroutines[button] = null;
            }

            SetupPointEvents(buttonType,
                () => StartCoroutine(ref coroutine, OnPointerEnter(button)),
                () => StartCoroutine(ref coroutine, OnPointerExit(button)),
                () => StartCoroutine(ref CoroutineRoot, SetActiveMenu(menuType)));
        }

        protected void SetupPointEvents(ButtonType buttonType, UnityAction pointerEnterAction,
            UnityAction pointerExitAction, UnityAction pointerClickAction)
        {
            EventTrigger eventTrigger =
                LauncherUI.GetOrAddComponent<EventTrigger>(Buttons[buttonType].gameObject);

            EventTrigger.Entry entryPointerEnter =
                new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entryPointerEnter.callback.AddListener(delegate { pointerEnterAction(); });
            eventTrigger.triggers.Add(entryPointerEnter);

            EventTrigger.Entry entryPointerExit =
                new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryPointerExit.callback.AddListener(delegate { pointerExitAction(); });
            eventTrigger.triggers.Add(entryPointerExit);

            EventTrigger.Entry entryPointerClick = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entryPointerClick.callback.AddListener(delegate { pointerClickAction(); });
            eventTrigger.triggers.Add(entryPointerClick);
        }

        private void SetActiveEvents(UnityAction enableAction, UnityAction disableAction)
        {
            onEnableAction = enableAction;
            onDisableAction = disableAction;
        }

        protected virtual IEnumerator OnPointerEnter(Button button)
        {
            yield return null;
        }

        protected virtual IEnumerator OnPointerExit(Button button)
        {
            yield return null;
        }

        private void SetActive(bool active)
        {
            if (active)
            {
                onEnableAction();
            }
            else
            {
                onDisableAction();
            }
        }

        protected IEnumerator PlayEnableAnimation(string clipName)
        {
            while (animatorRoot.enabled == false || Root.gameObject.activeSelf == false) yield return null;

            animatorRoot.Play(clipName);
            RuntimeAnimatorController animatorController = animatorRoot.runtimeAnimatorController;
            AnimationClip animationClip =
                animatorController.animationClips.First(animationClip => animationClip.name == clipName);

            yield return new WaitForSeconds(animationClip.length);
        }

        protected void StartCoroutine(ref Coroutine coroutine, IEnumerator routine)
        {
            if (coroutine != null)
            {
                Mono.StopCoroutine(coroutine);
                coroutine = null;
            }

            coroutine = Mono.StartCoroutine(routine);
        }

        protected void SetupCloseButtonPointEvents(MenuType menuType)
        {
            Coroutine coroutineCloseButton = ButtonCoroutines[Buttons[ButtonType.Close]];
            SetupPointEvents(ButtonType.Close,
                () => StartCoroutine(ref coroutineCloseButton, OnPointerEnterCloseButton()),
                () => StartCoroutine(ref coroutineCloseButton, OnPointerExitCloseButton()),
                () => StartCoroutine(ref CoroutineRoot, SetActiveMenu(menuType)));
        }

        protected IEnumerator OnPointerEnterCloseButton()
        {
            Transform closeIconTransform = Buttons[ButtonType.Close].transform.Find("close button foreground");
            yield return SpinImage(closeIconTransform, closeIconTransform.rotation.eulerAngles,
                new Vector3(0, 0, 90.0f));
        }

        protected IEnumerator OnPointerExitCloseButton()
        {
            Transform closeIconTransform = Buttons[ButtonType.Close].transform.Find("close button foreground");
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

                targetObjectTransform.rotation = Quaternion.Euler(current);

                yield return null;
            }
        }
    }

    public class StartMenu : Menu
    {
        private readonly Text textTitle;
        private readonly Animator animatorTitle;

        internal Coroutine CoroutineTitle;
        internal Coroutine CoroutineBlurBackground;

        private readonly Image imageBlur;

        private static readonly int Radius = Shader.PropertyToID("_Radius");

        public StartMenu(MonoBehaviour monoBehaviour, Transform transformParent) :
            base(monoBehaviour, transformParent)
        {
            Menus[MenuType.Start] = this;

            Transform parent = Root.parent;
            Transform titleTransform = parent.Find("Title");
            textTitle = titleTransform.GetComponent<Text>();
            animatorTitle = titleTransform.GetComponent<Animator>();

            imageBlur = parent.Find("Blur Panel").GetComponent<Image>();

            materialBlur = imageBlur.material;
            materialBlur.SetFloat(Radius, 1);
            imageBlur.gameObject.SetActive(false);

            Buttons = new Dictionary<ButtonType, Button>
            {
                [ButtonType.Start] = transformParent.Find("Start").GetComponent<Button>(),
                [ButtonType.Close] = transformParent.Find("Quit").GetComponent<Button>()
            };

            ButtonCoroutines = new Dictionary<Button, Coroutine>
            {
                { Buttons[ButtonType.Start], default(Coroutine) },
                { Buttons[ButtonType.Close], default(Coroutine) }
            };

            SetupPointEvents(ButtonType.Start, MenuType.SignIn);

            Button buttonClose = Buttons[ButtonType.Close];
            Coroutine coroutineCloseButton = ButtonCoroutines[buttonClose];
            SetupPointEvents(ButtonType.Close,
                () => StartCoroutine(ref coroutineCloseButton, OnPointerEnter(buttonClose)),
                () => StartCoroutine(ref coroutineCloseButton, OnPointerExit(buttonClose)),
                () =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                });
        }

        private readonly Material materialBlur;

        private IEnumerator PlayEnableTitleText()
        {
            textTitle.gameObject.SetActive(true);
            yield return PlayTextAnimation("Title_OnEnable");
        }

        internal IEnumerator PlayDisableTitleText()
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
        }

        private IEnumerator BlurBackground()
        {
            const float maxValue = 4.0f;
            float currentValue = materialBlur.GetFloat(Radius);

            yield return BackgroundBlur(currentValue, maxValue);
        }

        public IEnumerator ClearBackgroundBlur()
        {
            const float maxValue = 1.0f;
            float currentValue = materialBlur.GetFloat(Radius);

            yield return BackgroundBlur(currentValue, maxValue);

            imageBlur.gameObject.SetActive(false);
        }

        private IEnumerator BackgroundBlur(float current, float max)
        {
            imageBlur.gameObject.SetActive(true);
            Material material = imageBlur.material;

            float t = 0;
            while (t <= 1.0f)
            {
                float radius = Mathf.Lerp(current, max, t);
                material.SetFloat(Radius, radius);

                t += Time.deltaTime * 10;

                yield return null;
            }
        }

        protected override IEnumerator OnPointerEnter(Button button)
        {
            ButtonCoroutines.TryGetValue(button, out Coroutine coroutine);

            GameObject buttonGameObject = button.gameObject;
            Vector3 startScale = buttonGameObject.transform.localScale;
            Vector3 targetScale = new Vector3(1.2f, 1.2f, 1.2f);

            StartCoroutine(ref coroutine, ChangeGameObjectScale(buttonGameObject, startScale, targetScale));

            yield return coroutine;
        }

        protected override IEnumerator OnPointerExit(Button button)
        {
            ButtonCoroutines.TryGetValue(button, out Coroutine coroutine);

            GameObject buttonGameObject = button.gameObject;
            Vector3 startScale = buttonGameObject.transform.localScale;
            Vector3 targetScale = Vector3.one;

            StartCoroutine(ref coroutine, ChangeGameObjectScale(buttonGameObject, startScale, targetScale));

            yield return coroutine;
        }

        protected override IEnumerator OnEnable()
        {
            Root.gameObject.SetActive(true);

            StartCoroutine(ref CoroutineRoot, PlayEnableAnimation("OnEnable_StartMenuButtons"));

            StartCoroutine(ref CoroutineTitle, PlayEnableTitleText());

            StartCoroutine(ref CoroutineBlurBackground, ClearBackgroundBlur());

            yield return CoroutineRoot;
            yield return CoroutineTitle;
            yield return CoroutineBlurBackground;

            Menu.CurrentMenuType = MenuType.Start;
        }

        protected override IEnumerator OnDisable(bool all = false)
        {
            StartCoroutine(ref CoroutineBlurBackground, BlurBackground());

            StartCoroutine(ref CoroutineRoot, PlayEnableAnimation("OnDisable_StartMenuButtons"));


            yield return CoroutineRoot;
            yield return CoroutineBlurBackground;

            Menu.CurrentMenuType = MenuType.None;

            Root.gameObject.SetActive(false);
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

    public class SignIn : Menu
    {
        private InputField id;
        private InputField password;

        public SignIn(MonoBehaviour monoBehaviour, Transform parent) :
            base(monoBehaviour, parent)
        {
            Menus[MenuType.SignIn] = this;

            Buttons = new Dictionary<ButtonType, Button>
            {
                [ButtonType.Close] = Root.Find("Close").GetComponent<Button>(),
                [ButtonType.SignIn] = Root.Find("Buttons/Sign In").GetComponent<Button>(),
                [ButtonType.SignUp] = Root.Find("Buttons/Sign Up").GetComponent<Button>()
            };

            id = Root.Find("Field/ID/InputField").GetComponent<InputField>();
            password = Root.Find("Field/PW/InputField").GetComponent<InputField>();

            ButtonCoroutines = new Dictionary<Button, Coroutine>
            {
                { Buttons[ButtonType.Close], default(Coroutine) },
                { Buttons[ButtonType.SignIn], default(Coroutine) },
                { Buttons[ButtonType.SignUp], default(Coroutine) }
            };

            SetupCloseButtonPointEvents(MenuType.Start);
            SetupPointEvents(ButtonType.SignUp, MenuType.SignUp);
            SetupPointEvents(ButtonType.SignIn, MenuType.RoomList); // TODO (OSS) : check success login
            SetupPointEvents(ButtonType.SignIn,
                                () => { },
                                () => { },
                                () =>
                                {
                                    if (string.IsNullOrWhiteSpace(id.text) == true || string.IsNullOrWhiteSpace(password.text) == true) return;

                                    if (server.SignIn(id.text, password.text) == false) return;

                                    StartCoroutine(ref CoroutineRoot, SetActiveMenu(MenuType.RoomList));
                                });

        }

        protected override IEnumerator OnEnable()
        {
            Root.gameObject.SetActive(true);

            StartCoroutine(ref CoroutineRoot, PlayEnableAnimation("OnEnable_SignIn"));

            yield return CoroutineRoot;

            Menu.CurrentMenuType = MenuType.SignIn;
        }

        protected override IEnumerator OnDisable(bool all = false)
        {
            StartCoroutine(ref CoroutineRoot, PlayEnableAnimation("OnDisable_SignIn"));

            yield return CoroutineRoot;

            Root.gameObject.SetActive(false);

            Menu.CurrentMenuType = MenuType.None;
        }
    }

    public class SignUp : Menu
    {
        private InputField id;
        private InputField password;
        private InputField passwordCheck;

        public SignUp(MonoBehaviour monoBehaviour, Transform transformParent) :
            base(monoBehaviour, transformParent)
        {
            Menus[MenuType.SignUp] = this;

            Buttons = new Dictionary<ButtonType, Button>
            {
                [ButtonType.Close] = Root.Find("Close").GetComponent<Button>(),
                [ButtonType.SignUp] = Root.Find("Sign Up").GetComponent<Button>()
            };

            id = Root.Find("Field/ID/InputField").GetComponent<InputField>();
            password = Root.Find("Field/PW/InputField").GetComponent<InputField>();
            passwordCheck = Root.Find("Field/PW check/InputField").GetComponent<InputField>();

            ButtonCoroutines = new Dictionary<Button, Coroutine>
            {
                { Buttons[ButtonType.Close], default(Coroutine) },
                { Buttons[ButtonType.SignUp], default(Coroutine) }
            };

            SetupCloseButtonPointEvents(MenuType.SignIn);
            // TODO (OSS) : Set ID check button events 
            SetupPointEvents(ButtonType.SignUp,
                () => { },
                () => { },
                () =>
                {
                    if (string.IsNullOrWhiteSpace(id.text) == true || password.text.CompareTo(passwordCheck.text) != 0) return;

                    server.SignUp(id.text, password.text);
                });
        }

        protected override IEnumerator OnEnable()
        {
            Root.gameObject.SetActive(true);

            StartCoroutine(ref CoroutineRoot, PlayEnableAnimation("OnEnable_SignUp"));

            yield return CoroutineRoot;

            Menu.CurrentMenuType = MenuType.SignUp;
        }

        protected override IEnumerator OnDisable(bool all = false)
        {
            StartCoroutine(ref CoroutineRoot, PlayEnableAnimation("OnDisable_SignUp"));

            yield return CoroutineRoot;

            Menu.CurrentMenuType = MenuType.None;

            Root.gameObject.SetActive(false);
        }
    }


    public class RoomList : Menu
    {
        public RoomList(MonoBehaviour monoBehaviour, Transform transformParent) :
            base(monoBehaviour, transformParent)
        {
            Menus[MenuType.RoomList] = this;

            Buttons = new Dictionary<ButtonType, Button>
            {
                [ButtonType.Close] = Root.Find("Close").GetComponent<Button>(),
                [ButtonType.CreateRoom] = Root.Find("ListView/Room List/Button CreateRoom").GetComponent<Button>()
            };

            ButtonCoroutines = new Dictionary<Button, Coroutine>
            {
                { Buttons[ButtonType.Close], default(Coroutine) },
                { Buttons[ButtonType.CreateRoom], default(Coroutine) }
            };

            SetupCloseButtonPointEvents(MenuType.SignIn);
            SetupPointEvents(ButtonType.CreateRoom, MenuType.CreateRoom);
        }

        protected override IEnumerator OnEnable()
        {
            Root.gameObject.SetActive(true);

            StartCoroutine(ref CoroutineRoot, PlayEnableAnimation("OnEnable_RoomListAnimation"));
            yield return CoroutineRoot;

            Menu.CurrentMenuType = MenuType.RoomList;
        }

        protected override IEnumerator OnDisable(bool all = false)
        {
            StartCoroutine(ref CoroutineRoot, PlayEnableAnimation("OnDisable_RoomListAnimation"));
            yield return CoroutineRoot;

            Root.gameObject.SetActive(false);

            Menu.CurrentMenuType = MenuType.None;
        }
    }

    public class CreateRoom : Menu
    {
        public CreateRoom(MonoBehaviour monoBehaviour, Transform transformParent) :
            base(monoBehaviour, transformParent)
        {
            Menus[MenuType.CreateRoom] = this;

            Buttons = new Dictionary<ButtonType, Button>
            {
                [ButtonType.Close] = Root.Find("Header/Close").GetComponent<Button>(),
                [ButtonType.CreateRoom] = Root.Find("Create").GetComponent<Button>()
            };

            ButtonCoroutines = new Dictionary<Button, Coroutine>
            {
                { Buttons[ButtonType.Close], default(Coroutine) },
                { Buttons[ButtonType.CreateRoom], default(Coroutine) }
            };

            SetupCloseButtonPointEvents(MenuType.RoomList);

            SetupPointEvents(ButtonType.CreateRoom,
                () => { },
                () => { },
                () => StartCoroutine(ref CoroutineRoot, OnDisable(true)));
        }

        protected override IEnumerator OnEnable()
        {
            Root.gameObject.SetActive(true);

            StartCoroutine(ref CoroutineRoot, PlayEnableAnimation("OnEnable_CreateRoom"));
            yield return CoroutineRoot;

            Menu.CurrentMenuType = MenuType.CreateRoom;
        }

        protected override IEnumerator OnDisable(bool all)
        {
            StartMenu startMenu = (StartMenu)Menus[MenuType.Start];

            StartCoroutine(ref CoroutineRoot, PlayEnableAnimation("OnDisable_CreateRoom"));

            if (all)
            {
                StartCoroutine(ref startMenu.CoroutineTitle, startMenu.PlayDisableTitleText());
                StartCoroutine(ref startMenu.CoroutineBlurBackground, startMenu.ClearBackgroundBlur());
            }

            yield return CoroutineRoot;

            if (all)
            {
                yield return startMenu.CoroutineTitle;
                yield return startMenu.CoroutineBlurBackground;
            }

            Root.gameObject.SetActive(false);

            Menu.CurrentMenuType = MenuType.None;
        }
    }
}