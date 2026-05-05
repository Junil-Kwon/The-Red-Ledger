using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInputController : Singleton<PlayerInputController>
{
    /*
    // 다른 곳에서 evnt를 직접 Invoke 하는것을 막아준다
    public event Action OnClickEvent;
    public event Action OnReleaseEvent;
    public event Action<Ray> OnPointEvent;

    // 눌렀을 때와 뗐을 때 호출된다
    private void OnClick(InputValue value)
    {
        if (value.isPressed)
        {
            // 버튼을 누른 순간
            Debug.Log("마우스 버튼을 눌렀습니다.");
            OnClickEvent?.Invoke();
        }
        else
        {
            // 버튼에서 손을 뗀 순간
            Debug.Log("마우스 버튼을 뗐습니다.");
            OnReleaseEvent?.Invoke();
        }
    }

    // 마우스가 움직일 때마다 호출된다
    private void OnPoint(InputValue value)
    {
        // 설정해 두었던 mouse의 위치를 받아온다
        // 이때 이 위치를 Screen 위치므로 주의해야 한다
        Vector2 mousePosition = value.Get<Vector2>();
        //Debug.Log("마우스 위치: " + mousePosition);
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        OnPointEvent?.Invoke(ray);
    }
    */

    public enum ActionMap
    {
        Player,
        MenuUI
    }

    public PlayActions Input { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        
        Input = new PlayActions();

        // [이벤트 등록] 
        // Click 액션이 수행(performed)되었을 때만 PerformInteraction 함수를 실행해라!
        Input.Player.Click.performed += PerformInteraction;
    }

    private void OnEnable()
    {
        Input.Player.Enable();
    }

    private void OnDisable()
    {
        Input.Player.Disable();
        Input.MenuUI.Disable();
    }

    public void transitionActionMapTo(ActionMap actionMap)
    {
        Input.Player.Disable();
        Input.MenuUI.Disable();
        
        switch (actionMap)
        {
            case ActionMap.Player:
                Input.Player.Enable();
                Input.MenuUI.Disable();
                break;
            case ActionMap.MenuUI:
                Input.Player.Disable();
                Input.MenuUI.Enable();
                break;
        }
    }

    public bool IsPointerOverUIWhenClick()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.Player.Point.ReadValue<Vector2>();

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.tag != "NoneFunctionalUI")
            {
                return true; // UI 위에 마우스가 있는 것
            }
        }

        return false; // UI 위에 마우스가 없는 것 (월드 공간 클릭)
    }

    private void PerformInteraction(InputAction.CallbackContext ctx)
    {
        if (IsPointerOverUIWhenClick()) return; // UI 위에서 클릭한 경우 대화 진행 방지

        // 사용자가 월드 공간을 클릭함 -> 레이캐스트로 아이템 탐색
        Vector2 mousePosition = Input.Player.Point.ReadValue<Vector2>();
        Debug.Log($" 클릭 감지! 좌표: {mousePosition}");
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            /*
            // 맞은 물체에서 IInteractable 인터페이스를 가져옴
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                interactable.OnInteract(); // 해당 오브젝트가 구현한 로직이 알아서 실행됨!
            }
            */
        }
    }
}
