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
        MoveOrderMenu, PatrolOrderMenu, BuildOrderMenu, BuildOrderMenu2,
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
                EditWaypointMarkerControl();

                if (!ShapeSelectionControl(controlledSelector)
                    && Input.GetMouseButtonDown(1)
                    && NoUIAtScreenPositionExceptCanvas(pointerInfo.pointedPositionScreen, 2))
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
                EditWaypointMarkerControl();

                if (!ShapeSelectionControl(controlledSelector)
                    && Input.GetMouseButtonDown(1)
                    && NoUIAtScreenPositionExceptCanvas(pointerInfo.pointedPositionScreen, 2))
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
                EditWaypointMarkerControl();

                if (!ShapeSelectionControl(controlledSelector)
                && Input.GetMouseButtonDown(1)
                && NoUIAtScreenPositionExceptCanvas(pointerInfo.pointedPositionScreen, 2))
                    controllerStateMachine.CurrentState = ControllerStates.Menu;
            },
            () => {
                DestroyMenuButtons(menuCurrentButtons);
                controlledSelector.Deactivate();
            }
        );

        controllerStateMachine.AddState(ControllerStates.MoveOrderMenu,
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
                //if (CanEditMoveOrderForSelectedEntities(controlledSelector))

                if (!CurrentlyEditedMoveOrdersCreateWaypointsControl()
                && Input.GetMouseButtonDown(1)
                && NoUIAtScreenPositionExceptCanvas(pointerInfo.pointedPositionScreen, 2))
                    controllerStateMachine.CurrentState = ControllerStates.OrderConfirmationPrompt;
            },
            null,
            null,
            new Dictionary<ControllerStates, System.Action>() {
                { ControllerStates.OrderMenu, 
                    () => {
                        FetchCurrentlyEditedMoveOrderWrappersFromSelectedEntities(controlledSelector);
                    }
                }
            }
        );

        controllerStateMachine.AddState(ControllerStates.BuildOrderMenu,
            () => {
                controlledSelector.ActivateAndUnpause();

                CreateButton(menuCurrentButtons, sampleButtonPrefab, "FOB",
                    pointerInfo.pointedPositionScreen - Vector3.right * 50f,
                    GameObject.Find("ScreenUICanvas").GetComponent<RectTransform>(),
                    GetMyCamera()).onClick.AddListener(
                        () => {
                            controllerStateMachine.CurrentState = ControllerStates.OrderConfirmationPrompt;
                            /*new BuildingMarker(
                                pointerInfo.pointedPositionWorld - Vector3.right * 10 * i / c);
                            currentlyEditedOrderWrappers[i]
                                .GetWrappedAs<MoveOrder>()
                                .AddWaypoint(wpmrk.GetMyWrapper<WaypointMarker>());*/
                        });
                        
                CreateButton(menuCurrentButtons, sampleButtonPrefab, "Outpost",
                    pointerInfo.pointedPositionScreen + Vector3.right * 0f,
                    GameObject.Find("ScreenUICanvas").GetComponent<RectTransform>(),
                    GetMyCamera()).onClick.AddListener(
                        () => {
                            //dddddddddddddddddddddddddddddddddddddddddddddddd
                        });

                //PrepareButtonsForMenu(menuCurrentButtons);
            },
            () => {
                //if (CanEditMoveOrderForSelectedEntities(controlledSelector))

                CurrentlyEditedBuildOrdersCreateMarkersControl();
                /*if (!CurrentlyEditedBuildOrdersCreateMarkersControl()
                 && Input.GetMouseButtonDown(1)
                 && NoUIAtScreenPositionExceptCanvas(pointerInfo.pointedPositionScreen, 2))
                    controllerStateMachine.CurrentState = ControllerStates.OrderConfirmationPrompt*/;
            },
            null,
            null,
            new Dictionary<ControllerStates, System.Action>() {
                { ControllerStates.OrderMenu, 
                    () => {
                        FetchCurrentlyEditedBuildOrderWrappersFromSelectedEntities(controlledSelector);
                    }
                }
            }
        );

        controllerStateMachine.AddState(ControllerStates.BuildOrderMenu2,
            () => {
                controlledSelector.ActivateAndUnpause();

                CreateButton(menuCurrentButtons, sampleButtonPrefab, "FOB",
                    pointerInfo.pointedPositionScreen - Vector3.right * 50f,
                    GameObject.Find("ScreenUICanvas").GetComponent<RectTransform>(),
                    GetMyCamera()).onClick.AddListener(
                        () => {
                            controllerStateMachine.CurrentState = ControllerStates.OrderConfirmationPrompt;
                            /*new BuildingMarker(
                                pointerInfo.pointedPositionWorld - Vector3.right * 10 * i / c);
                            currentlyEditedOrderWrappers[i]
                                .GetWrappedAs<MoveOrder>()
                                .AddWaypoint(wpmrk.GetMyWrapper<WaypointMarker>());*/
                        });
                        
                CreateButton(menuCurrentButtons, sampleButtonPrefab, "Outpost",
                    pointerInfo.pointedPositionScreen + Vector3.right * 0f,
                    GameObject.Find("ScreenUICanvas").GetComponent<RectTransform>(),
                    GetMyCamera()).onClick.AddListener(
                        () => {
                            //dddddddddddddddddddddddddddddddddddddddddddddddd
                        });

                //PrepareButtonsForMenu(menuCurrentButtons);
            },
            () => {
                //if (CanEditMoveOrderForSelectedEntities(controlledSelector))

                CurrentlyEditedBuildOrdersCreateMarkersControl();
                /*if (!CurrentlyEditedBuildOrdersCreateMarkersControl()
                 && Input.GetMouseButtonDown(1)
                 && NoUIAtScreenPositionExceptCanvas(pointerInfo.pointedPositionScreen, 2))
                    controllerStateMachine.CurrentState = ControllerStates.OrderConfirmationPrompt*/;
            },
            null,
            null,
            new Dictionary<ControllerStates, System.Action>() {
                { ControllerStates.OrderMenu, 
                    () => {
                        FetchCurrentlyEditedBuildOrderWrappersFromSelectedEntities(controlledSelector);
                    }
                }
            }
        );
        
        ControllerStates _previousstate = default(ControllerStates);
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
                EditWaypointMarkerControl();

                if (Input.GetMouseButtonDown(1))
                {
                    //controllerStateMachine.CurrentState = ControllerStates.MoveOrderMenu;
                    controllerStateMachine.CurrentState = _previousstate;
                }
            },
            () => {
                DestroyMenuButtons(menuCurrentButtons);
            },
            null,
            new Dictionary<ControllerStates, System.Action>() 
            {
                { ControllerStates.MoveOrderMenu, () => { _previousstate = ControllerStates.MoveOrderMenu; } },
                { ControllerStates.BuildOrderMenu, () => { _previousstate = ControllerStates.BuildOrderMenu; } }
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

    /*private struct EditedOWBunchStruct
    {
        public List<OrderWrapper> orderWrappersList;
        public bool executable;
    }*/

    private List<OrderWrapper> currentlyEditedOWBunch = new List<OrderWrapper>();

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
    
    private void FetchCurrentlyEditedMoveOrderWrappersFromSelectedEntities(Selector selector)
    {
        var l = selector.GetCurrentlySelectedEntities();
        foreach (var v in l)
        {
            OrderFactory.CreateOrderWrapperAndSetReceiver<MoveOrder>((IOrderable<Unit>)v);                    
            //v.GetSelectableAsReferenceWrapperSpecific<UnitWrapper>().GetMostRecentAddedOrder();
            currentlyEditedOWBunch.Add(v.GetSelectableAsReferenceWrapperSpecific<UnitWrapper>().GetMostRecentAddedOrder());
            /*currentlyEditedOWBunch.Add(
                new EditedOWBunchStruct(){
                    orderWrappersList = new List<OrderWrapper>() { v.GetSelectableAsReferenceWrapperSpecific<UnitWrapper>().GetMostRecentAddedOrder() },
                    executable = true
                });*/
        }
    }

    private bool CurrentlyEditedMoveOrdersCreateWaypointsControl()
    {
        if (Input.GetMouseButtonDown(0)
            && NoUIAtScreenPositionExceptCanvas(pointerInfo.pointedPositionScreen, 2))
        {
            //wps.Add(pointedPositionInfo.pointedPositionWorld);            
            
            //var wpmrk = new WaypointMarker(pointedPositionInfo.pointedPositionWorld);
            //currentlyEditedOrderWrapper.GetWrappedAs<MoveOrder>()
            //    .AddWaypoint(wpmrk.GetMyWrapper<WaypointMarker>());

            int c = currentlyEditedOWBunch.Count;
            for(int i = 0; i < c; i++)
            {
                AddWaypointToMoveWrapperAtPosition(currentlyEditedOWBunch[i], pointerInfo.pointedPositionWorld - Vector3.right * 10 * i / c);
            }
        }

        return false;
    }

    private void AddWaypointToMoveWrapperAtPosition(OrderWrapper ow, Vector3 pos)
    {
        WaypointMarker wpmrk = new WaypointMarker(pos);
        ow.GetWrappedAs<MoveOrder>().AddWaypoint(wpmrk.GetMyWrapper<WaypointMarker>());

    }

    private void FetchCurrentlyEditedBuildOrderWrappersFromSelectedEntities(Selector selector)
    {
        var l = selector.GetCurrentlySelectedEntities();
        foreach (var v in l)
        {
            OrderFactory.CreateOrderWrapperAndSetReceiver<BuildOrder>((IOrderable<Unit>)v);                    
            //v.GetSelectableAsReferenceWrapperSpecific<UnitWrapper>().GetMostRecentAddedOrder();
            currentlyEditedOWBunch.Add(v.GetSelectableAsReferenceWrapperSpecific<UnitWrapper>().GetMostRecentAddedOrder());
        }
    }


    private bool CurrentlyEditedBuildOrdersCreateMarkersControl()
    {
        if (Input.GetMouseButtonDown(0)
            && NoUIAtScreenPositionExceptCanvas(pointerInfo.pointedPositionScreen, 2))
        {
            //wps.Add(pointedPositionInfo.pointedPositionWorld);            
            
            //var wpmrk = new WaypointMarker(pointedPositionInfo.pointedPositionWorld);
            //currentlyEditedOrderWrapper.GetWrappedAs<MoveOrder>()
            //    .AddWaypoint(wpmrk.GetMyWrapper<WaypointMarker>());

            BuildingMarker bm;
            bm = new BuildingMarker(pointerInfo.pointedPositionWorld);
            int c = currentlyEditedOWBunch.Count;
            for(int i = 0; i < c; i++)
            {
                currentlyEditedOWBunch[i]
                    .GetWrappedAs<BuildOrder>().SetBuildingToBuild(bm.GetMyWrapper<BuildingMarker>());
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
        int c = currentlyEditedOWBunch.Count;
        for(int i = c-1; i >= 0; i--)
        {
            /*if (Order.GetConfirmationFromReceiver(currentlyEditedOrderWrappers[i]))
            {
                Order.TryStartExecution(currentlyEditedOrderWrappers[i]);
                currentlyEditedOrderWrappers.RemoveAt(i);
            }*/
            //if (Order.TryStartExecution(Order.GetReceiver(currentlyEditedOWBunch[i]).GetCurrentOrderInQueue()))
            if (Order.TryStartExecution(currentlyEditedOWBunch[i]))
                currentlyEditedOWBunch.RemoveAt(i);
        }
    }

    private void CurrentlyEditedOrdersCancel()
    {
        int c = currentlyEditedOWBunch.Count;
        for(int i = c-1; i >= 0; i--)
        {
            currentlyEditedOWBunch[i].DestroyWrappedReference();
            currentlyEditedOWBunch.RemoveAt(i);
        }
    }

    private bool ShapeSelectionControl(Selector selector)
    {
        selector.UpdatePointerCurrentScreenPosition(pointerInfo.pointedPositionScreen);

        if (Input.GetMouseButtonDown(0) && selector.GetLowState()==Selector.LowStates.NotSelecting
            && NoUIAtScreenPositionExceptCanvas(pointerInfo.pointedPositionScreen, 2))
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

    private MapMarkerWrapper<WaypointMarker> _editedWaypointMarkerWrapper = null;
    private void EditWaypointMarkerControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_editedWaypointMarkerWrapper != null)
            {
                WaypointMarker.UpdateWaypointMarker(_editedWaypointMarkerWrapper, false, pointerInfo.pointedPositionScreen);
                _editedWaypointMarkerWrapper = null;
            }
            else if (_editedWaypointMarkerWrapper == null)
            {
                var ewp = GetUICloseToScreenPosition<WaypointMarkerComponent>(GetMyCamera(), pointerInfo.pointedPositionScreen, 7f);
                if(ewp != null
                    && ewp.associatedMarkerWrapper != null
                    && ewp.associatedMarkerWrapper.WrappedObject != null)
                {
                    _editedWaypointMarkerWrapper = ewp.associatedMarkerWrapper;
                }
                else if (NoUIAtScreenPositionExceptCanvas(pointerInfo.pointedPositionScreen, 0)) 
                {
                    
                } 
            }
        }

        if (_editedWaypointMarkerWrapper != null 
            && _editedWaypointMarkerWrapper.WrappedObject != null)
        {
            WaypointMarker.UpdateWaypointMarker(_editedWaypointMarkerWrapper, true, pointerInfo.pointedPositionScreen);
        }
    }

#endregion

#region Helper functions

    private GraphicRaycaster graphicRaycasterScreenUI;
    private GraphicRaycaster graphicRaycasterWorldUI;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;
    private List<RaycastResult> _raycastresults;

    private void InitCanvasRaycastingStuff()
    {
        graphicRaycasterScreenUI = GameObject.Find("ScreenUICanvas").GetComponent<GraphicRaycaster>();
        graphicRaycasterWorldUI = GameObject.Find("WorldUICanvas").GetComponent<GraphicRaycaster>();
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
    }

    ///<summary>
    /// mode 0 : screen ui
    /// mode 1 : world ui
    /// mode 2 : both
    ///</summary>
    private bool NoUIAtScreenPositionExceptCanvas(Vector3 screenPosition, int mode)
    {
        pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = screenPosition;

        _raycastresults = new List<RaycastResult>();
        int hitcount = 0;

        graphicRaycasterScreenUI.Raycast(pointerEventData, _raycastresults);
        hitcount += _raycastresults.Count;

        graphicRaycasterWorldUI.Raycast(pointerEventData, _raycastresults);
        hitcount += _raycastresults.Count;

        return hitcount == 0;
    }

    private T GetUICloseToScreenPositionCondition<T>(Camera camera, Vector3 screenPosition, float proximityDistance, System.Func<T,bool> condition) where T : Component
    {
        // TODO : very ugly, this of course is only temporary
        T[] list = GameObjectExtension.FindObjectsOfTypeAndLayer<T>(LayerMask.NameToLayer("UI"));
        foreach (var o in list)
        {
            if(camera.GetWorldPosCloseToScreenPos(o.transform.position, screenPosition, proximityDistance)
                && (condition == null ^ condition(o) == true)) 
            {
                return o;                
            }
        }
        return null;
    }

    private T GetUICloseToScreenPosition<T>(Camera camera, Vector3 screenPosition, float proximityDistance) where T : Component
    {
        // TODO : very ugly, this of course is only temporary
        T[] list = GameObjectExtension.FindObjectsOfTypeAndLayer<T>(LayerMask.NameToLayer("UI"));
        foreach (var o in list)
        {
            if(camera.GetWorldPosCloseToScreenPos(o.transform.position, screenPosition, proximityDistance))
                return o;                
        }
        return null;
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
