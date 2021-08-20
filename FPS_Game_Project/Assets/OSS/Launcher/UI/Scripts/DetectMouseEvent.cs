using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityStandardAssets.Utility.Inspector;

namespace OSS.Launcher.UI
{
    public class DetectMouseEvent : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private LauncherUI launcherUI;

        private Coroutine coroutine = null;
        private bool isNeedIgnored = false;

        private void Awake()
        {
            launcherUI = transform.root.GetComponent<LauncherUI>();
        }

        private void OnDisable()
        {
            isNeedIgnored = false;
            transform.localScale = Vector3.one;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("Called OnPointerClick function");
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            isNeedIgnored = true;
            coroutine = StartCoroutine(OnPointerClick());
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isNeedIgnored == true) return;
            
            Debug.Log("Called OnPointerEnter  function");

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            coroutine = StartCoroutine(OnPointerEnter());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isNeedIgnored == true) return;
            
            Debug.Log("Called OnPointerExit function");

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            coroutine = StartCoroutine(OnPointerExit());
        }

        private IEnumerator OnPointerEnter()
        {
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = new Vector3(1.2f, 1.2f, 1.2f);
            Vector3 currentScale = startScale;

            float t = 0;
            while (currentScale.x < 1.2f)
            {
                float scale = Mathf.Lerp(startScale.x, targetScale.x, t);
                t += Time.deltaTime * 10;

                currentScale.x = currentScale.y = currentScale.y = scale;

                transform.localScale = currentScale;

                yield return null;
            }

            transform.localScale = targetScale;
            Debug.Log("Finished OnPointerEnter Coroutine");
        }

        private IEnumerator OnPointerExit()
        {
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = Vector3.one;
            Vector3 currentScale = startScale;

            float t = 0;
            while (currentScale.x > 1)
            {
                float scale = Mathf.Lerp(startScale.x, targetScale.x, t);
                t += Time.deltaTime * 10;

                currentScale.x = currentScale.y = currentScale.y = scale;

                transform.localScale = currentScale;

                yield return null;
            }

            transform.localScale = targetScale;
            Debug.Log("Finished OnPointerExit Coroutine");
        }

        private IEnumerator OnPointerClick()
        {
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = Vector3.zero;
            Vector3 currentScale = startScale;

            float t = 0;
            while (currentScale.x > 0)
            {
                float scale = Mathf.Lerp(startScale.x, targetScale.x, t);
                t += Time.deltaTime * 10;

                currentScale.x = currentScale.y = currentScale.y = scale;

                transform.localScale = currentScale;

                yield return null;
            }

            transform.localScale = targetScale;
            Debug.Log("Finished OnPointerClick Coroutine");
            
            // launcherUI.SetMenuEnabled(menuState);
        }
    }
}