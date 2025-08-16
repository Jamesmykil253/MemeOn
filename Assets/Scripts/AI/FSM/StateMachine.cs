using System.Collections.Generic;

namespace MemeArena.AI.FSM
{
    public class StateMachine
    {
        private readonly Dictionary<object, IState> _states = new();
        private IState _current;

        public void Add(object key, IState state) => _states[key] = state;
        public void SetInitial(object key) => _current = _states[key];

        public void ChangeState(object key)
        {
            if (_current == _states[key]) return;
            _current?.Exit();
            _current = _states[key];
            _current?.Enter();
        }

        public void Tick() => _current?.Tick();
    }
}
