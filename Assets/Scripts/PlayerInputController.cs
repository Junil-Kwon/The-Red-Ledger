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

    //private Texture2D crosshair;

    public PlayActions Input { get; private set; }
    public InputActionMap CurrentActionMap { get; private set; }

    private void InitializeInput()
    {
        if (Input != null) Input.Dispose();
        
        Input = new PlayActions();
        SwitchActionMap(ActionMap.Player); // 시작은 Player 맵으로
    }

    public void SwitchActionMap(ActionMap actionMap)
    {
        if (Input == null) return;
        if (CurrentActionMap != null && CurrentActionMap.name == actionMap.ToString()) return; // 이미 해당 액션 맵이 활성화되어 있다면 중복 실행 방지

        
        CurrentActionMap?.Disable(); // 현재 활성화된 액션 맵 비활성화

        switch (actionMap)
        {
            case ActionMap.Player:
                Input.Player.Enable();
                CurrentActionMap = Input.Player;

                /*
                // ★ Resources.Load는 최초 1회만 실행되도록 캐싱 처리
                if (crosshair == null)
                {
                    crosshair = Resources.Load<Texture2D>("Crosshair");
                }
                
                if (crosshair != null)
                {
                    Cursor.SetCursor(crosshair, new Vector2(crosshair.width / 2f, crosshair.height / 2f), CursorMode.Auto);
                }
                */
                break;

            case ActionMap.MenuUI:
                Input.MenuUI.Enable();
                CurrentActionMap = Input.MenuUI;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // 기본 커서로 변경
                break;
        }
        
        Debug.Log($"[InputManager] 액션 맵 전환 완료: {actionMap}");
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

    // 월드 공간 클릭 시 상호작용 수행
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

    // ============== Lifecycle Methods ==============
    protected override void Awake()
    {
        base.Awake();

        // 상위 싱글톤에서 중복으로 판정되어 내가 Instance가 아니라면 
        // 아래 코드를 실행하지 말고 즉시 함수를 빠져나갑니다.
        //if (Instance != this) return;
        
        InitializeInput();

        // [이벤트 등록] 
        // Click 액션이 수행(performed)되었을 때만 PerformInteraction 함수를 실행해라!
        Input.Player.Click.performed += PerformInteraction;
    }

    private void OnEnable()
    {
        //Input.Player.Enable();
    }

    private void OnDisable()
    {
        //Input.Player.Disable();
        //Input.MenuUI.Disable();
    }
}
