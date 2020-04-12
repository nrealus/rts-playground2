﻿using System.Collections;
using System.Collections.Generic;
using Core.Helpers;
using Core.Selection;
using Core.Units;
using UnityEngine;
using UnityEngine.EventSystems;
using VariousUtilsExtensions;
using UnityEngine.InputSystem;
using Gamelogic.Extensions;
using Core.Orders;
using UnityEngine.UI;
using Core.MapMarkers;

public class InputController : MonoBehaviour, 
    IHasCameraRef
{
    public struct PointerInfo
    {
        public PointerEventData pointerEventData;
        public Vector3 pointedPositionWorld;
        public Vector3 pointedPositionScreen;
    }

    public PointerInfo pointerInfo = new PointerInfo();

    [SerializeField] private Camera _cam;
    public Camera GetMyCamera() { return _cam; }
    
    public Selector controlledSelector;

    /*private List<OrderWrapper> GetCurrentlyEditedOrderWrappers()
    {
        return currentlyEditedOrderWrappersList;
    }*/

    /////////////

    private enum ControllerStates { Neutral, Menu, OrderMenu,
        MoveOrderMenu, PatrolOrderMenu, BuildOrderMenu,
        OrderConfirmationPrompt , OrderCancel, }

    private ExtendedStateMachine<ControllerStates> controllerStateMachine;

    /////////////

    public UnityEngine.UI.Button sampleButtonPrefab;

    public List<UnityEngine.UI.Button> menuCurrentButtons = new List<UnityEngine.UI.Button>();

    private void Awake()
    {

        InitCanvasRaycastingStuff();

        controllerStateMachine = new ExtendedStateMachine<ControllerStates>();
        
        controllerStateMachine.AddState(ControllerStates.Neutral,
            () => {
                controlledSelector.ActivateAndUnpause();
            },
            () => {
                if (!ShapeSelectionControl(controlledSelector) && Input.GetMouseButtonDown(1))
                    controllerStateMachine.CurrentState = ControllerStates.Menu;
            },
            () => {
                controlledSelector.Deactivate();
            }
        );

        controllerStateMachine.AddState(ControllerStates.Menu,
            () => {
                controlledSelector.ActivateAndUnpause();
            
                CreateButton(menuCurrentButtons, sampleButtonPrefab, "Other",
                    pointerInfo.pointedPositionScreen - Vector3.right * 50f,
                    GameObject.Find("ScreenUICanvas").GetComponent<RectTransform>(),
                    GetMyCamera()).onClick.AddListener(
                        () => {
                            //controllerStateMachine.CurrentState = ControllerStates.OrderEditing;
                        });
            
                CreateButton(menuCurrentButtons, sampleButtonPrefab, "Orders",
                    pointerInfo.pointedPositionScreen + Vector3.right * 50f,
                    GameObject.Find("ScreenUICanvas").GetComponent<RectTransform>(),
                    GetMyCamera()).onClick.AddListener(
                        () => {
                            controllerStateMachine.CurrentState = ControllerStates.OrderMenu;
                        });
            
                //PrepareButtonsForMenu(menuCurrentButtons);
            },
            () => {
                if (!ShapeSelectionControl(controlledSelector) && Input.GetMouseButtonDown(1))
                    controllerStateMachine.CurrentState = ControllerStates.Neutral;
            },
            () => {
                DestroyMenuButtons(menuCurrentButtons);
                controlledSelector.Deactivate();
            }
        );

        controllerStateMachine.AddState(ControllerStates.OrderMenu,
            () => {
                controlledSelector.ActivateAndUnpause();

                CreateButton(menuCurrentButtons, sampleButtonPrefab, "Move",
                    pointerInfo.pointedPositionScreen - Vector3.right * 50f,
                    GameObject.Find("ScreenUICanvas").GetComponent<RectTransform>(),
                    GetMyCamera()).onClick.AddListener(
                        () => {
                            if (CanEditMoveOrderForSelectedEntities(controlledSelector))
                                controllerStateMachine.CurrentState = ControllerStates.MoveOrderMenu;
                        });
                        
                CreateButton(menuCurrentButtons, sampleButtonPrefab, "Patrol",
                    pointerInfo.pointedPositionScreen + Vector3.right * 0f,
                    GameObject.Find("ScreenUICanvas").GetComponent<RectTransform>(),
                    GetMyCamera()).onClick.AddListener(
                        () => {
                            controllerStateMachine.CurrentState = ControllerStates.PatrolOrderMenu;
                        });

                CreateButton(menuCurrentButtons, sampleButtonPrefab, "Build",
                    pointerInfo.pointedPositionScreen + Vector3.right * 50f,
                    GameObject.Find("ScreenUICanvas").GetComponent<RectTransform>(),
                    GetMyCamera()).onClick.AddListener(
                        () => {
                            controllerStateMachine.CurrentState = ControllerStates.BuildOrderMenu;
                        });

                //PrepareButtonsForMenu(menuCurrentButtons);
            },
            () => {
                if (!ShapeSelectionControl(controlledSelector) && Input.GetMouseButtonDown(1))
                    controllerStateMachine.CurrentState = ControllerStates.Menu;
            },
            () => {
                DestroyMenuButtons(menuCurrentButtons);
                controlledSelector.Deactivate();
            }
        );

        controllerStateMachine.AddState(ControllerStates.MoveOrderMenu,
            null,
            () => {
                if (CanEditMoveOrderForSelectedEntities(controlledSelector))

                if (!CurrentlyEditedMoveOrdersCreateWaypoints() && Input.GetMouseButtonDown(1))
                    controllerStateMachine.CurrentState = ControllerStates.OrderConfirmationPrompt;
            },
            null,
            null,
            new Dictionary<ControllerStates, System.Action>() {
                { ControllerStates.OrderMenu, 
                    () => {
                        FetchCurrentlyEditedOrderWrappersFromSelectedEntities(controlledSelector);
                    }
                }
            }
        );
        
        controllerStateMachine.AddState(ControllerStates.OrderConfirmationPrompt,
            () => {
                CreateButton(menuCurrentButtons, sampleButtonPrefab, "Save/Confirm",
                    pointerInfo.pointedPositionScreen - Vector3.right * 50f,
                    GameObject.Find("ScreenUICanvas").GetComponent<RectTransform>(),
                    GetMyCamera()).onClick.AddListener(
                        () => {
                            CurrentlyEditedOrdersConfirm();
                            controllerStateMachine.CurrentState = ControllerStates.Neutral;
                        });
                        
                CreateButton(menuCurrentButtons, sampleButtonPrefab, "Start Hour",
                    pointerInfo.pointedPositionScreen - Vector3.right * 25f,
                    GameObject.Find("ScreenUICanvas").GetComponent<RectTransform>(),
                    GetMyCamera()).onClick.AddListener(
                        () => {
                            // example, this is a placeholder
                        });

                CreateButton(menuCurrentButtons, sampleButtonPrefab, "Other options",
                    pointerInfo.pointedPositionScreen + Vector3.right * 25f,
                    GameObject.Find("ScreenUICanvas").GetComponent<RectTransform>(),
                    GetMyCamera()).onClick.AddListener(
                        () => {
                            // example, this is a placeholder 
                        });

                CreateButton(menuCurrentButtons, sampleButtonPrefab, "Cancel",
                    pointerInfo.pointedPositionScreen + Vector3.right * 50f,
                    GameObject.Find("ScreenUICanvas").GetComponent<RectTransform>(),
                    GetMyCamera()).onClick.AddListener(
                        () => {
                            CurrentlyEditedOrdersCancel();
                            controllerStateMachine.CurrentState = ControllerStates.OrderMenu;
                        });

                //PrepareButtonsForMenu(menuCurrentButtons);
            },
            () => {
                if (Input.GetMouseButtonDown(1))
                {
                    // To be replaced by a generic : go back to "previous order" editing menu (see "pushdown automata")
                    controllerStateMachine.CurrentState = ControllerStates.MoveOrderMenu;
                }
            },
            () => {
                DestroyMenuButtons(menuCurrentButtons);
            }
        );

        controllerStateMachine.CurrentState = ControllerStates.Neutral;

    }

    //////////////



    //////////////


    private void Update()
    {   

        UpdatePointerInfo();

        controllerStateMachine.Update();

    }

    private void UpdatePointerInfo()
    {
        pointerInfo.pointedPositionScreen = Input.mousePosition;
        pointerInfo.pointedPositionWorld = GetMyCamera().GetPointedPositionPhysRaycast(pointerInfo.pointedPositionScreen);
    }

#region Menu Actions

    private List<OrderWrapper> currentlyEditedOrderWrappers = new List<OrderWrapper>();

    private bool CanEditMoveOrderForSelectedEntities(Selector selector)
    {
        if (selector.GetCurrentlySelectedEntities().Count > 0)
        {
            return true;
        }
        else
        {
            Debug.Log("No selected entities to move.");
            return false;
        }
    }
    
    private void FetchCurrentlyEditedOrderWrappersFromSelectedEntities(Selector selector)
    {
        var l = selector.GetCurrentlySelectedEntities();
        foreach (var v in l)
        {
            OrderFactory.CreateOrderWrapperAndSetReceiver<MoveOrder>((IOrderable<Unit>)v);                    
            v.GetSelectableAsReferenceWrapperSpecific<UnitWrapper>().GetMostRecentAddedOrder();
            //currentlyEditedOrderWrappers.Add(v.GetSelectableAsReferenceWrapperSpecific<UnitWrapper>().GetMostRecentAddedOrder());
        }
    }

    private bool CurrentlyEditedMoveOrdersCreateWaypoints()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //wps.Add(pointedPositionInfo.pointedPositionWorld);            
            
            //var wpmrk = new WaypointMarker(pointedPositionInfo.pointedPositionWorld);
            //currentlyEditedOrderWrapper.GetWrappedAs<MoveOrder>()
            //    .AddWaypoint(wpmrk.GetMyWrapper<WaypointMarker>());

            WaypointMarker wpmrk;
            int c = currentlyEditedOrderWrappers.Count;
            for(int i = 0; i < c; i++)
            {
                wpmrk = new WaypointMarker(
                    pointerInfo.pointedPositionWorld - Vector3.right * 10 * i / c);
                currentlyEditedOrderWrappers[i]
                    .GetWrappedAs<MoveOrder>()
                    .AddWaypoint(wpmrk.GetMyWrapper<WaypointMarker>());
            }
        }

        /*if (Input.GetMouseButtonDown(1)
            &&) 
        {
            return true;
        }*/

        return false;
    }

    private void CurrentlyEditedOrdersConfirm()
    {
        int c = currentlyEditedOrderWrappers.Count;
        for(int i = c-1; i >= 0; i--)
        {
            if (currentlyEditedOrderWrappers[i].GetConfirmationFromReceiver())
            {
                currentlyEditedOrderWrappers[i].TryStartExecution();
                currentlyEditedOrderWrappers.RemoveAt(i);
            }
        }
    }

    private void CurrentlyEditedOrdersCancel()
    {
        int c = currentlyEditedOrderWrappers.Count;
        for(int i = c-1; i >= 0; i--)
        {
            currentlyEditedOrderWrappers[i].DestroyWrappedReference();
            currentlyEditedOrderWrappers.RemoveAt(i);
        }
    }

    private bool ShapeSelectionControl(Selector selector)
    {
        selector.UpdatePointerCurrentScreenPosition(pointerInfo.pointedPositionScreen);

        if (Input.GetMouseButtonDown(0) && selector.GetLowState()==Selector.LowStates.NotSelecting
            && NoScreenUIAtScreenPositionExceptCanvas(pointerInfo.pointedPositionScreen))
            selector.StartSelecting();

        if (Input.GetMouseButtonUp(0) && selector.GetLowState()==Selector.LowStates.Selecting)
            selector.ConfirmSelecting();

        if (Input.GetMouseButtonDown(1) && selector.GetLowState()==Selector.LowStates.Selecting)
        {
            selector.CancelSelecting();
            return true;
        }

        if (Input.GetKey(KeyCode.LeftShift))
            selector.selectionMode = Selector.SelectionModes.Additive;
        else if (Input.GetKey(KeyCode.LeftControl))
            selector.selectionMode = Selector.SelectionModes.Subtractive;
        else
            selector.selectionMode = Selector.SelectionModes.Default;

        return false;
    }

#endregion

#region Helper functions

    private GraphicRaycaster graphicRaycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;
    private List<RaycastResult> _raycastresults;

    private void InitCanvasRaycastingStuff()
    {
        graphicRaycaster = GameObject.Find("ScreenUICanvas").GetComponent<GraphicRaycaster>();
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
    }

    private bool NoScreenUIAtScreenPositionExceptCanvas(Vector3 screenPosition)
    {
        pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = screenPosition;

        _raycastresults = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, _raycastresults);

        return _raycastresults.Count == 0;
    }

#endregion

#region Button stuff

    private UnityEngine.UI.Button CreateButton(List<UnityEngine.UI.Button> buttonsMenuList, UnityEngine.UI.Button buttonPrefab, string text, Vector3 screenPosition, RectTransform canvasRect, Camera camera)
    {

        var button = Instantiate<UnityEngine.UI.Button>(buttonPrefab, canvasRect);
        button.GetComponent<RectTransform>().SetPositionOfPivotFromViewportPosition(canvasRect, 
            camera.ScreenToViewportPoint(screenPosition));
        button.GetComponentInChildren<UnityEngine.UI.Text>().text = text;

        buttonsMenuList.Add(button);

        return button;

    }

    private void PrepareButtonsForMenu(List<UnityEngine.UI.Button> buttons)
    {
        foreach (var b in buttons)
        {
            b.onClick.AddListener(
                () => {
                    DestroyMenuButtons(buttons);
                }
            );
        }
    }

    private void DestroyButton(UnityEngine.UI.Button button)
    {
        button.onClick.RemoveAllListeners();
        Destroy(button.gameObject);
    }

    private void DestroyMenuButtons(List<UnityEngine.UI.Button> buttons)
    {
        foreach (var b in buttons)
            DestroyButton(b);
        buttons.Clear();
    }

#endregion

}