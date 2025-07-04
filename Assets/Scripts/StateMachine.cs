using System.Collections.Generic;
using System.Linq;
using System;

// 各State毎のdelagateを登録しておくクラス
public interface IState
{
    void OnEnter();
    void OnExit();
    void OnUpdate(float deltaTime);
}

public class StateMapping
{
    public IState stateClass;

    public Action onEnter;
    public Action onExit;
    public Action<float> onUpdate;

    public void Enter()
    {
        if (stateClass != null) stateClass.OnEnter();
        else onEnter?.Invoke();
    }

    public void Exit()
    {
        if (stateClass != null) stateClass.OnExit();
        else onExit?.Invoke();
    }

    public void Update(float deltaTime)
    {
        if (stateClass != null) stateClass.OnUpdate(deltaTime);
        else onUpdate?.Invoke(deltaTime);
    }
}

public class Transition<TState, TTrigger>
{
    public TState To { get; set; }
    public TTrigger Trigger { get; set; }
}

public class StateMachine<TState, TTrigger>
    where TState : struct, IConvertible, IComparable
    where TTrigger : struct, IConvertible, IComparable
{
    private TState _current;
    private StateMapping _currentMapping;

    private Dictionary<object, StateMapping> _mappings = new();
    private Dictionary<TState, List<Transition<TState, TTrigger>>> _transitions = new();

    public StateMachine(TState initialState)
    {
        // StateからStateMappingを作成
        foreach (TState state in Enum.GetValues(typeof(TState)))
        {
            _mappings[state] = new StateMapping();
        }
        ChangeState(initialState);
    }
    public void RegisterState(TState state, IState stateImpl)
    {
        _mappings[state].stateClass = stateImpl;
        
    }


    /// <summary>
    /// Stateを初期化する
    /// </summary>
    public void SetupState(TState state, Action onEnter, Action onExit, Action<float> onUpdate)
    {
        var mapping = _mappings[state];
        mapping.onEnter = onEnter;
        mapping.onExit = onExit;
        mapping.onUpdate = onUpdate;
    }

    /// <summary>
    /// トリガーを実行する
    /// </summary>
    public void ExecuteTrigger(TTrigger trigger)
    {
        if (_transitions.TryGetValue(_current, out var list))
        {
            foreach (var transition in list)
            {
                if (transition.Trigger.Equals(trigger))
                {
                    ChangeState(transition.To);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 遷移情報を登録する
    /// </summary>
    public void AddTransition(TState from, TState to, TTrigger trigger)
    {
        if (!_transitions.ContainsKey(from))
        {
            _transitions[from] = new List<Transition<TState, TTrigger>>();
        }
        _transitions[from].Add(new Transition<TState, TTrigger> { To = to, Trigger = trigger });
    }




    /// <summary>
    /// 更新する
    /// </summary>
    public void Update(float deltaTime)
    {
        _currentMapping?.Update(deltaTime);
    }


    /// <summary>
    /// Stateを直接変更する
    /// </summary>
    private void ChangeState(TState to)
    {
        _currentMapping?.Exit();
        _current = to;
        _currentMapping = _mappings[to];
        _currentMapping.Enter();
    }
}