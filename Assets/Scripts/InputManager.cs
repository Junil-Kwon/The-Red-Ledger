using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager
{
    public enum ActionMap { Player, MenuUI }

    private static PlayActions _input;
    public static InputActionMap CurrentActionMap { get; private set; }

    public static PlayActions Input
    {
        get { EnsureInitialized(); return _input; }
    }

    private static bool _isShutdown = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInitialized()
    {
        if (_input != null || _isShutdown) return;

        _input = new PlayActions();
        _input.Player.Click.performed += PerformInteraction;
        SwitchActionMap(ActionMap.Player);

        Application.quitting += Shutdown;
    }

    private static void Shutdown()
    {
        if (_input == null) return;
        _isShutdown = true;
        _input.Player.Click.performed -= PerformInteraction;
        _input.Dispose();
        _input = null;
        CurrentActionMap = null;
    }

    public static void SwitchActionMap(ActionMap actionMap)
    {
        EnsureInitialized();

        if (_input == null) return;
        if (CurrentActionMap != null && CurrentActionMap.name == actionMap.ToString()) return; // 이미 해당 액션 맵이 활성화되어 있다면 중복 실행 방지

        
        CurrentActionMap?.Disable(); // 현재 활성화된 액션 맵 비활성화

        switch (actionMap)
        {
            case ActionMap.Player:
                _input.Player.Enable();
                CurrentActionMap = _input.Player;

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
                _input.MenuUI.Enable();
                CurrentActionMap = _input.MenuUI;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // 기본 커서로 변경
                break;
        }
        
        Debug.Log($"[InputManager] 액션 맵 전환 완료: {actionMap}");
    }

    static readonly List<RaycastResult> s_results = new();
    static PointerEventData s_eventData;

    public static bool IsPointerOverUI()
    {
        s_eventData ??= new PointerEventData(EventSystem.current);
        s_eventData.position = _input.Player.Point.ReadValue<Vector2>();

        s_results.Clear();
        EventSystem.current.RaycastAll(s_eventData, s_results);
        
        foreach (var result in s_results) // 이미 앞에서부터 정렬됨
        {
            var go = result.gameObject;
            if (go.CompareTag("NoneFunctionalUI") || go.CompareTag("DialogueTextBox"))
                continue;

            return true; // 처음 만난 "의미 있는" UI가 최상단 → 이것이 클릭을 받는다
        }
        return false;
    }

    // 월드 공간 클릭 시 상호작용 수행
    private static void PerformInteraction(InputAction.CallbackContext ctx)
    {
        if (IsPointerOverUI()) return; // UI 위에서 클릭한 경우 대화 진행 방지

        // 사용자가 월드 공간을 클릭함 -> 레이캐스트로 아이템 탐색
        Vector2 mousePosition = _input.Player.Point.ReadValue<Vector2>();
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
