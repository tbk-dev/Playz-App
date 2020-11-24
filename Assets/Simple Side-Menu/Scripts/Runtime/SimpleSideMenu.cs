﻿// Simple Side-Menu - https://assetstore.unity.com/packages/tools/gui/simple-side-menu-143623
// Version: 1.0.3
// Author: Daniel Lochner

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DanielLochner.Assets.SimpleSideMenu
{
    [AddComponentMenu("UI/Simple Side-Menu")]
    public class SimpleSideMenu : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IInitializePotentialDragHandler
    {
        #region Fields
        public Placement placement = Placement.Left;
        public State defaultState = State.Closed;
        public float transitionSpeed = 10f;
        public float thresholdDragSpeed = 0f;
        public float thresholdDraggedFraction = 0.5f;
        public GameObject handle = null;
        public bool handleDraggable = true;
        public bool menuDraggable = false;
        public bool handleToggleStateOnPressed = true;
        public bool useOverlay = true;
        public Color overlayColour = new Color(0, 0, 0, 0.25f);
        public bool useBlur = false;
        public int blurRadius = 10;
        public bool overlayCloseOnPressed = true;
        public UnityEvent onStateChanged, onStateSelected, onStateChanging, onStateSelecting;
        public Material blurMaterial;

        private float previousTime;
        private bool dragging, potentialDrag;
        private Vector2 closedPosition, openPosition, startPosition, releaseVelocity, dragVelocity, menuSize;
        private Vector3 previousPosition;
        private GameObject overlay, blur;
        private RectTransform rectTransform, canvasRectTransform;
        private Image overlayImage, blurImage;
        private CanvasScaler canvasScaler;
        private Canvas canvas;
        #endregion

        #region Properties
        public State CurrentState { get; set; }
        public State TargetState { get; set; }

        public float StateProgress { get { return ((rectTransform.anchoredPosition - closedPosition).magnitude / ((placement == Placement.Left || placement == Placement.Right) ? rectTransform.rect.width : rectTransform.rect.height)); } }
        #endregion

        #region Enumerators
        public enum Placement
        {
            Left,
            Right,
            Right2,
            Right3,
            Top,
            TopLeft,
            TopRight,
            Bottom
        }
        public enum State
        {
            Closed,
            Open
        }
        #endregion

        #region Methods
        private void Start()
        {
            Initialize();

            if (Validate())
            {
                Setup();

                //StartCoroutine(Rotate());
            }
            else
            {
                throw new Exception("Invalid inspector input.");
            }
        }
        private void Update()
        {
            OnStateUpdate();
            OnOverlayUpdate();
        }
        #if UNITY_EDITOR
        private void OnValidate()
        {
            Initialize();
        }
        #endif

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            potentialDrag = (handleDraggable && eventData.pointerEnter == handle) || (menuDraggable && eventData.pointerEnter == gameObject);
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            dragging = potentialDrag;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, eventData.position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 mouseLocalPosition))
            {
                startPosition = mouseLocalPosition;
            }
            previousPosition = rectTransform.position;
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (dragging && RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, eventData.position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 mouseLocalPosition))
            {
                Vector2 displacement = ((TargetState == State.Closed) ? closedPosition : openPosition) + (mouseLocalPosition - startPosition);

                float x = (placement == Placement.Left || placement == Placement.Right) ? displacement.x : rectTransform.anchoredPosition.x;
                float y = (placement == Placement.Top || placement == Placement.Bottom) ? displacement.y : rectTransform.anchoredPosition.y;

                Vector2 min = new Vector2(Math.Min(closedPosition.x, openPosition.x), Math.Min(closedPosition.y, openPosition.y));
                Vector2 max = new Vector2(Math.Max(closedPosition.x, openPosition.x), Math.Max(closedPosition.y, openPosition.y));

                rectTransform.anchoredPosition = new Vector2(Mathf.Clamp(x, min.x, max.x), Mathf.Clamp(y, min.y, max.y));

                onStateSelecting.Invoke();
            }

            dragVelocity = (rectTransform.position - previousPosition) / (Time.time - previousTime);
            previousPosition = rectTransform.position;
            previousTime = Time.time;
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            dragging = false;

            releaseVelocity = dragVelocity;
            OnTargetUpdate();
        }

        private void Initialize()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();

            if (canvas != null)
            {
                canvasScaler = canvas.GetComponent<CanvasScaler>();
                canvasRectTransform = canvas.GetComponent<RectTransform>();
            }
        }
        private bool Validate()
        {
            bool valid = true;

            if (transitionSpeed <= 0)
            {
                Debug.LogError("<b>[SimpleSideMenu]</b> Transition speed cannot be less than or equal to zero.", gameObject);
                valid = false;
            }
            if (handle != null && handleDraggable && handle.transform.parent != rectTransform)
            {
                Debug.LogError("<b>[SimpleSideMenu]</b> The drag handle must be a child of the side menu in order for it to be draggable.", gameObject);
                valid = false;
            }
            if (handleToggleStateOnPressed && handle.GetComponent<Button>() == null)
            {
                Debug.LogError("<b>[SimpleSideMenu]</b> The handle must have a \"Button\" component attached to it in order for it to be able to toggle the state of the side menu when pressed.", gameObject);
                valid = false;
            }

            return valid;
        }
        private void Setup()
        {
            //Canvas & Camera
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                canvas.planeDistance = (canvasRectTransform.rect.height / 2f) / Mathf.Tan((canvas.worldCamera.fieldOfView / 2f) * Mathf.Deg2Rad);
                if (canvas.worldCamera.farClipPlane < canvas.planeDistance)
                {
                    canvas.worldCamera.farClipPlane = Mathf.Ceil(canvas.planeDistance);
                }
            }

            //Placement
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.zero;
            Vector2 pivot = Vector2.zero;

            //float recPosx = rectTransform.localPosition.x * rectTransform.transform.parent.transform.localScale.x;
            //float recPosy = rectTransform.localPosition.y * rectTransform.transform.parent.transform.localScale.y;
            float rectWidth = rectTransform.rect.width;
            float rectheight = rectTransform.rect.height;
            float rectPosx = rectTransform.localPosition.x;
            float rectPosy = rectTransform.localPosition.y;

            switch (placement)
            {
                case Placement.Left:
                    anchorMin = new Vector2(0, 0.5f);
                    anchorMax = new Vector2(0, 0.5f);
                    pivot = new Vector2(1, 0.5f);
                    closedPosition = new Vector2(0, rectPosy);
                    openPosition = new Vector2(rectWidth, rectPosy);
                    break;

                case Placement.TopLeft:
                    anchorMin = new Vector2(0, 1);
                    anchorMax = new Vector2(0, 1);
                    pivot = new Vector2(1, 0);
                    closedPosition = new Vector2(0, 0);
                    openPosition = new Vector2(rectWidth, -1 * (rectheight - (rectheight * 0.1f)));
                    break;

                case Placement.Right:
                    anchorMin = new Vector2(1, 0.5f);
                    anchorMax = new Vector2(1, 0.5f);
                    pivot = new Vector2(0, 0.5f);
                    closedPosition = new Vector2(0, rectPosy);
                    openPosition = new Vector2(-1 * rectPosx, rectPosy);
                    break;
                case Placement.Right2:
                    anchorMin = new Vector2(0,1);
                    anchorMax = new Vector2(0,1);
                    pivot = new Vector2(0, 1);

                    closedPosition = new Vector2(0, 0);

                    openPosition = new Vector2(rectWidth, 0);
                    break;
                case Placement.Top:
                    anchorMin = new Vector2(0.5f, 1);
                    anchorMax = new Vector2(0.5f, 1);
                    pivot = new Vector2(0.5f, 0);
                    closedPosition = new Vector2(rectPosx, 0);
                    openPosition = new Vector2(rectPosx, -1 * rectheight);
                    break;

                case Placement.TopRight:
                    anchorMin = new Vector2(1, 1f);
                    anchorMax = new Vector2(1, 1f);
                    pivot = new Vector2(0, 1f);
                    closedPosition = new Vector2(0, rectheight - (rectheight * 0.1f));
                    openPosition = new Vector2(-1 * rectWidth, 0);
                    break;

                case Placement.Bottom:
                    anchorMin = new Vector2(0.5f, 0);
                    anchorMax = new Vector2(0.5f, 0);
                    pivot = new Vector2(0.5f, 1);
                    closedPosition = new Vector2(rectPosx, 0);
                    openPosition = new Vector2(rectPosx, rectheight);
                    break;
            }

            //rectTransform.sizeDelta = rectTransform.rect.size;
            
            //rectTransform.sizeDelta = new Vector2(Screen.width, (int)(Screen.height * 0.9));
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;

            //Default State
            SetState(CurrentState = defaultState);
            rectTransform.anchoredPosition = (defaultState == State.Closed) ? closedPosition : openPosition;

            //Drag Handle
            if (handle != null)
            {
                //Toggle State on Pressed
                if (handleToggleStateOnPressed)
                {
                    handle.GetComponent<Button>().onClick.AddListener(delegate { ToggleState(); });
                }
                foreach (Text text in handle.GetComponentsInChildren<Text>())
                {
                    if (text.gameObject != handle) text.raycastTarget = false;
                }
            }

            //Overlay
            if (useOverlay)
            {
                overlay = new GameObject(gameObject.name + " (Overlay)");
                overlay.transform.parent = transform.parent;
                overlay.transform.localScale = Vector3.one;
                overlay.transform.SetSiblingIndex(transform.GetSiblingIndex());

                if (useBlur)
                {
                    blur = new GameObject(gameObject.name + " (Blur)");
                    blur.transform.parent = transform.parent;
                    blur.transform.SetSiblingIndex(transform.GetSiblingIndex());

                    RectTransform blurRectTransform = blur.AddComponent<RectTransform>();
                    blurRectTransform.anchorMin = Vector2.zero;
                    blurRectTransform.anchorMax = Vector2.one;
                    blurRectTransform.offsetMin = Vector2.zero;
                    blurRectTransform.offsetMax = Vector2.zero;
                    blurImage = blur.AddComponent<Image>();
                    blurImage.raycastTarget = false;
                    blurImage.material = new Material(blurMaterial);
                    blurImage.material.SetInt("_Radius", 0);
                }

                RectTransform overlayRectTransform = overlay.AddComponent<RectTransform>();
                overlayRectTransform.anchorMin = Vector2.zero;
                overlayRectTransform.anchorMax = Vector2.one;
                overlayRectTransform.offsetMin = Vector2.zero;
                overlayRectTransform.offsetMax = Vector2.zero;
                overlayImage = overlay.AddComponent<Image>();
                overlayImage.color = (defaultState == State.Open) ? overlayColour : Color.clear;
                overlayImage.raycastTarget = overlayCloseOnPressed;
                Button overlayButton = overlay.AddComponent<Button>();
                overlayButton.transition = Selectable.Transition.None;
                overlayButton.onClick.AddListener(delegate { Close(); });
            }
        }

        private void OnTargetUpdate()
        {
            if (releaseVelocity.magnitude > thresholdDragSpeed)
            {
                if (placement == Placement.Left)
                {
                    if (releaseVelocity.x > 0)
                    {
                        Open();
                    }
                    else
                    {
                        Close();
                    }
                }
                else if (placement == Placement.Right)
                {
                    if (releaseVelocity.x < 0)
                    {
                        Open();
                    }
                    else
                    {
                        Close();
                    }
                }
                else if (placement == Placement.Top)
                {
                    if (releaseVelocity.y < 0)
                    {
                        Open();
                    }
                    else
                    {
                        Close();
                    }
                }
                else
                {
                    if (releaseVelocity.y > 0)
                    {
                        Open();
                    }
                    else
                    {
                        Close();
                    }
                }
            }
            else
            {
                float nextStateProgress = (TargetState == State.Open) ? 1 - StateProgress : StateProgress;

                if (nextStateProgress > thresholdDraggedFraction)
                {
                    ToggleState();
                }
                else
                {
                    SetState(CurrentState);
                }
            }   
        }
        private void OnStateUpdate()
        {
            if (!dragging)
            {
                Vector2 targetPosition = (TargetState == State.Closed) ? closedPosition : openPosition;
                rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, Time.unscaledDeltaTime * transitionSpeed);

                if (CurrentState != TargetState)
                {
                    if ((rectTransform.anchoredPosition - targetPosition).magnitude <= rectTransform.rect.width / 10f)
                    {
                        CurrentState = TargetState;
                        onStateChanged.Invoke();
                    }
                    else
                    {
                        onStateChanging.Invoke();
                    }
                }
            }
        }
        private void OnOverlayUpdate()
        {
            if (useOverlay)
            {
                overlayImage.raycastTarget = overlayCloseOnPressed && (TargetState == State.Open);
                overlayImage.color = new Color(overlayColour.r, overlayColour.g, overlayColour.b, overlayColour.a * StateProgress);

                if (useBlur)
                {
                    blurImage.material.SetInt("_Radius", (int)(blurRadius * StateProgress));
                }
            }
        }

        public void SetState(State state)
        {
            TargetState = state;

            onStateSelected.Invoke();
        }
        public void ToggleState()
        {
            SetState((State)(((int)TargetState + 1) % 2));
        }
        public void Open()
        {
            SetState(State.Open);
        }
        public void Close()
        {
            SetState(State.Closed);
        }     
        #endregion

        enum SCREENROTATIONSTATE
        {
            init,
            portrait,
            landscapeLeft,
        }

        public int screenWidth = 0;
        public int screenHeight = 0;

        public IEnumerator Rotate()
        {
            while (true)
            {
                if (screenWidth == Screen.width || screenHeight == Screen.height)
                    yield return null;
                else
                {
                    var currentState = TargetState;

                    // Portrait mode
                    if (Screen.width < Screen.height)
                    {
                        //if (screenRotationState != SCREENROTATIONSTATE.portrait)
                        {
                            //Screenheight = Screen.height;
                            //Screenwidth = Screen.width;

                            //Debug.Log($"Not Portrait -- height { Screen.height}, width  { Screen.width}");


                            //screenRotationState = SCREENROTATIONSTATE.portrait;
                            rectTransform.sizeDelta = canvas.GetComponent<RectTransform>().sizeDelta;

                            //rectTransform.sizeDelta = new Vector2(Screenwidth/*Screen.width*/, (int)(Screenheight * 0.9));//(int)(Screen.height * 0.9));

                            rectTransform.sizeDelta = new Vector2(Screen.width, (int)(Screen.height * 0.9));
                        }
                    }
                    else
                    {
                        //if (screenRotationState != SCREENROTATIONSTATE.landscapeLeft)
                        {
                            //Screenheight = Screen.width;
                            //Screenwidth = Screen.height;

                            //Debug.Log($"Not landscapeLeft --  height { Screen.height}, width  { Screen.width}");


                            //screenRotationState = SCREENROTATIONSTATE.landscapeLeft;
                            rectTransform.sizeDelta = canvas.GetComponent<RectTransform>().sizeDelta;

                            //rectTransform.sizeDelta = new Vector2(Screenwidth/*Screen.width*/, Screenheight - 192);//(int)(Screen.height - 192));

                            rectTransform.sizeDelta = new Vector2(Screen.width, (int)(Screen.height * 0.9));
                        }
                    }

                    Setup();
                    TargetState = currentState;

                    screenWidth =  Screen.width;
                    screenHeight = Screen.height;
                    Debug.Log(rectTransform.sizeDelta);

                }
                yield return new WaitForSeconds(0.5f);
            }

        }
    }
}