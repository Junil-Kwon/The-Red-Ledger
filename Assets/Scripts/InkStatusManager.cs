using UnityEngine;

public class InkStatusManager : MonoBehaviour
{
    public enum ObjectiveState
    {
        GREEN = 0,
        YELLOW = 1,
        RED = 2
    }

    [SerializeField] private ObjectiveState _currentObjectiveState = ObjectiveState.GREEN;
    [SerializeField] private int _statusRecoveryCount = 0;
    [SerializeField] private int _intelPoint = 0;
    [SerializeField] private int _creativeFlagCount = 0;

    public int StatusRecoveryCount => _statusRecoveryCount;
    public int IntelPoint => _intelPoint;
    public int CreativeFlagCount => _creativeFlagCount;
    public ObjectiveState CurrentObjectiveState => _currentObjectiveState;
    public string GetObjectiveState() => _currentObjectiveState.ToString().ToUpper();

    public void UpdateStatusRecoveryCount(bool isGoodChoice) // 연속 선택제
    {
        // RED 상태에서는 아무것도 하지 않음 (조기 리턴)
        if (_currentObjectiveState == ObjectiveState.RED) return;

        if (isGoodChoice)
        {
            // YELLOW 상태에서만 회복 카운트 누적
            if (_currentObjectiveState != ObjectiveState.YELLOW) return;

            // 기존의 음수 체크 및 증가 로직을 깔끔하게 정리
            _statusRecoveryCount = Mathf.Max(0, _statusRecoveryCount) + 1;

            if (_statusRecoveryCount >= 2)
            {
                _currentObjectiveState = ObjectiveState.GREEN;
                _statusRecoveryCount = 0;
            }
        }
        else
        {
            // 잘못된 선택 시 기존 양수 카운트는 날리고 음수로 내림
            _statusRecoveryCount = Mathf.Min(0, _statusRecoveryCount) - 1;

            if (_currentObjectiveState == ObjectiveState.GREEN)
            {
                _currentObjectiveState = ObjectiveState.YELLOW;
                _statusRecoveryCount = 0; // 상태 변화 시 카운트 초기화
            }
            else if (_statusRecoveryCount <= -2)
            {
                _currentObjectiveState = ObjectiveState.RED;
                _statusRecoveryCount = 0;
            }
        }
    }
    public void AddIntel(int points)
    {
        _intelPoint += points;
    }
    public void AddCreativeFlagCount()
    {
        _creativeFlagCount++;
    }
}
