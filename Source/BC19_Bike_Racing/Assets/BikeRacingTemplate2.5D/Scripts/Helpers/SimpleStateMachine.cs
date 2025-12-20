using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.BikeRacing25D
{
    /// <summary>
    /// A Simple state object which holds the references to the onEnter/Update/Exit actions and an id.
    /// </summary>
    /// <typeparam name="IdType">The type which is used to identify your State. Usually a custom Enum, Integer or String.</typeparam>
    /// <typeparam name="DataType">The data type which can be used to pass data to the onEnter and onExit methods.</typeparam>
    public class SimpleState<IdType, DataType>
    {
        public IdType Id;

        /// <summary>
        /// Called when the state is entered.
        /// First parameter (IdType) is the Id of the state which the state machine is coming from. Can be default(IdType) if undefined before.
        /// Second paramter (DataType) is the data which has been given to the state machine to hand over to the state (see Schedule.. methods).
        /// </summary>
        public Action<IdType, DataType[]> OnEnter;

        /// <summary>
        /// Called when the StateMachine Update() is called.
        /// First parameter (int) is the frame count since OnEnter starting with 0. It is supposed to help with sequential execution and wait. If you call Update() on a per frame basis then this is the frame count since OnEnter.
        /// </summary>
        public Action<int> OnUpdate;

        /// <summary>
        /// Called when the StateMachine FixedUpdate() is called.
        /// First parameter (int) is the frame count since OnEnter starting with 0. It is supposed to help with sequential execution and wait. If you call Update() on a per frame basis then this is the frame count since OnEnter.
        /// </summary>
        public Action<int> OnFixedUpdate;

        /// <summary>
        /// Called when the state is left.
        /// First parameter (IdType) is the Id of the state which the state machine will go to. Can be default(IdType) if undefined before.
        /// Second paramter (DataType) is the data which has been given to the state machine to hand over to the state (see Schedule.. methods).
        /// </summary>
        public Action<IdType, DataType[]> OnExit;

        public SimpleState(IdType id, Action<IdType, DataType[]> onEnter, Action<int> onUpdate, Action<IdType, DataType[]> onExit, Action<int> onFixedUpdate)
        {
            Id = id;
            OnEnter = onEnter;
            OnUpdate = onUpdate;
            OnExit = onExit;
            OnFixedUpdate = onFixedUpdate;
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }


    /// <summary>
    /// A simple state machine.
    /// </summary>
    /// <typeparam name="IdType">IdType is the Id by which a state is identified (usually an enum).</typeparam>
    /// <typeparam name="DataType">DataType is the base data type of parameters handed between states (usually object).</typeparam>
    public class SimpleStateMachine<IdType, DataType>
    {
        public struct ScheduledState
        {
            public IdType State;

            public ScheduledState(IdType state)
            {
                State = state;
            }
        }

        protected bool _scheduled = false;
        protected DataType[] _scheduledNewStateData;
        protected DataType[] _scheduledOldStateData;
        protected IdType _scheduledNewStateId;

        public bool DebugLogStateChanges = false;

        protected string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        protected bool _pauseUpdates;
        public bool IsPaused
        {
            get { return _pauseUpdates; }
            set
            {
                _pauseUpdates = value;
            }
        }

        protected Dictionary<IdType, SimpleState<IdType, DataType>> _states;
        protected SimpleState<IdType, DataType> _state;

        /// <summary>
        /// This will be reset with every OnEnter Call. It is handed to the OnUpdate() of each state to help with sequential execution and wait.
        /// </summary>
        protected int _currentUpdateCounter;

        protected IdType _previousStateId;
        /// <summary>
        /// The last known state which was DIFFERENT from the current state.
        /// </summary>
        public IdType PreviousState
        {
            get { return _previousStateId; }
        }

        public SimpleStateMachine() : this(null, null)
        { }

        public SimpleStateMachine(string name) : this(name, null)
        { }

        public SimpleStateMachine(string name, params SimpleState<IdType, DataType>[] states)
        {
            Name = name;

            _states = new Dictionary<IdType, SimpleState<IdType, DataType>>();
            if (states != null)
            {
                for (int i = 0; i < states.Length; ++i)
                {
                    AddState(states[i]);
                }
            }

            IsPaused = false;
        }

        public void SetInitialState(IdType id, params DataType[] dataForNewState)
        {
            ChangeImmediately(id, dataForNewState, null);
        }

        public string GetNameNotNull()
        {
            if (Name == null)
            {
                return "";
            }
            else
            {
                return Name;
            }
        }

        public SimpleState<IdType, DataType> AddState(IdType id, Action<IdType, DataType[]> onEnter)
        {
            return AddState(id, onEnter, null, null, null);
        }

        public SimpleState<IdType, DataType> AddState(IdType id, Action<IdType, DataType[]> onEnter, Action<int> onUpdate)
        {
            return AddState(id, onEnter, onUpdate, null, null);
        }

        public SimpleState<IdType, DataType> AddState(IdType id, Action<IdType, DataType[]> onEnter, Action<int> onUpdate, Action<IdType, DataType[]> onExit)
        {
            return AddState(id, onEnter, onUpdate, onExit, null);
        }

        public SimpleState<IdType, DataType> AddState(IdType id, Action<IdType, DataType[]> onEnter, Action<int> onUpdate, Action<IdType, DataType[]> onExit, Action<int> onFixedUpdate)
        {
            var state = new SimpleState<IdType, DataType>(id, onEnter, onUpdate, onExit, onFixedUpdate);
            return AddState(state);
        }

        public SimpleState<IdType, DataType> AddState(SimpleState<IdType, DataType> state)
        {
            if (_states.ContainsKey(state.Id) == false)
            {
                _states.Add(state.Id, state);
            }
            else
            {
                throw new Exception("SimpleStateMachine.AddState: The state '" + state.Id.ToString() + "' already exists and would be ambiguous.");
            }

            return state;
        }

        public void RemoveState(IdType id)
        {
            if (_states.ContainsKey(id))
            {
                _states.Remove(id);
            }
        }

        public void Update()
        {
            if (IsPaused == false && _state != null)
            {
                if (_scheduled)
                {
                    // Remembers scheduled data for ChangeImmediately(...)
                    // This is done because ChangeImmediately() itself may already
                    // change scheduled data by rescheduling a state change in onEnter or onExit.
                    var scheduledNewStateId = _scheduledNewStateId;
                    var scheduledNewStateData = _scheduledNewStateData;
                    var scheduledOldStateData = _scheduledOldStateData;

                    // reset scheduled data and flags
                    _scheduled = false;
                    _scheduledNewStateId = default(IdType);
                    _scheduledNewStateData = null;
                    _scheduledOldStateData = null;

                    ChangeImmediately(scheduledNewStateId, scheduledNewStateData, scheduledOldStateData);
                }
                else
                {
                    if (_state.OnUpdate != null)
                    {
                        _state.OnUpdate(_currentUpdateCounter);
                        _currentUpdateCounter++;
                    }
                }
            }
        }

        /// <summary>
        /// Use for physics updates in unity. Calls fixedUpdate on the state.
        /// Does not trigger any changes to scheduled states, use Update() for that.
        /// If a state change was scheduled then this will do nothing until the new state was entered.
        /// The "PauseUpdates" flag is taken into account.
        /// </summary>
        public void FixedUpdate()
        {
            if (IsPaused == false && _scheduled == false && _state != null)
            {
                if (_state.OnFixedUpdate != null)
                {
                    _state.OnFixedUpdate(_currentUpdateCounter);
                }
            }
        }

        /// <summary>
        /// If state is null then default(IdType) is returned.
        ///  READONLY, use ScheduleStateChange() to change the state.
        ///  Notice that another state may already be scheduled.
        /// </summary>
        public IdType State
        {
            get
            {
                if (_state != null)
                {
                    return _state.Id;
                }
                else
                {
                    return default(IdType);
                }
            }
        }

        /// <summary>
        /// State may be null.
        /// </summary>
        /// <returns></returns>
        public SimpleState<IdType, DataType> GetStateObject()
        {
            return _state;
        }

        /// <summary>
        /// If no state change is scheduled then NULL is returned.
        ///  READONLY, use ScheduleStateChange() to change the state.
        /// </summary>
        public ScheduledState? GetScheduledStateInfo()
        {
            if (_scheduled)
            {
                return new ScheduledState(_scheduledNewStateId);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Will the next state be any of the given ids?<br />
        /// True if the current state matches any of the ids and
        /// no other state being scheduled.<br />
        /// Or if any of the given ids is already scheduled.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool IsOrWillBeState(params IdType[] ids)
        {
            // The ids match the scheduled state.
            if (IsStateScheduled(ids))
                return true;

            // Current state matching the ids and no other state being scheduled.
            if (IsState(ids) && !IsStateScheduled())
                return true;

            return false;
        }

        /// <summary>
        /// Is the current state any of the given state id(s)?
        /// Notice that another state may already be scheduled.
        /// </summary>
        /// <param name="ids">one or more state ids</param>
        /// <returns></returns>
        public bool IsState(params IdType[] ids)
        {
            if (_state != null)
            {
                if (ids.Length == 1)
                {
                    return _state.Id.Equals(ids[0]);
                }

                foreach (var id in ids)
                {
                    if (_state.Id.Equals(id))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Is any of the given state id(s) scheduled?
        /// </summary>
        /// <param name="ids">one or more state ids</param>
        /// <returns></returns>
        public bool IsStateScheduled(params IdType[] ids)
        {
            if (_scheduled)
            {
                if (ids.Length == 1)
                {
                    return _scheduledNewStateId.Equals(ids[0]);
                }

                foreach (var id in ids)
                {
                    if (_scheduledNewStateId.Equals(id))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Is any state change scheduled?
        /// </summary>
        /// <returns></returns>
        public bool IsStateScheduled()
        {
            return _scheduled;
        }

        /// <summary>
        /// Schedules a state change only if the current or scheduled state does not match the id.
        /// </summary>
        /// <param name="id"></param>
        public void ScheduleStateChangeIfNecessary(IdType id)
        {
            if (IsOrWillBeState(id))
                return;

            ScheduleStateChange(id, null);
        }

        /// <inheritdoc cref="ScheduleStateChange" />
        public void ScheduleStateChange(IdType id)
        {
            ScheduleStateChange(id, null);
        }

        /// <summary>
        /// Use this if you want to change the state.
        /// 
        /// The state change will no happen immediately but at the next call to Update() to minimize
        /// side effects (code after a call to Schedule... should still be execute in the same state).
        /// The next call to Update() will Exit the current state and ENTER the new state, it will not
        /// call Update() on any of the two states (this will happen in the next but one call to Update()):
        ///     Update()#1 = Exit() old state, Enter() new state
        ///     Update()#2 = Update() of new state.
        /// 
        /// If you schedule a change to a state which is already the active state then OnExit and OnEnter will be
        /// called on the same state (in that order).
        /// </summary>
        /// <param name="state">The id of the state you wish to enter.</param>
        /// <param name="dataForNewState">Data you want to be handed over to the new state onEnter(..)</param>
        public void ScheduleStateChange(IdType id, params DataType[] dataForNewState)
        {
            if (DebugLogStateChanges)
            {
                if (this._scheduled == true)
                {
                    Debug.LogWarning("SSM(" + GetNameNotNull() + "): multiple calls to ScheduleStateChange() between Update() executions (last change was to " + id + "). Is this done on purpose? You should call Update() manually in between to ensure consistency.");
                }
            }

            _scheduled = true;
            _scheduledNewStateId = id;
            _scheduledNewStateData = dataForNewState;
            _scheduledOldStateData = null;

            if (DebugLogStateChanges)
            {
                if (_state != null && id.Equals(_state.Id))
                {
                    Debug.LogWarning("SSM(" + GetNameNotNull() + "): scheduled change from '" + _state.Id + "' to the same state '" + id.ToString() + "'. Is this done on purpose?");
                }
            }
        }

        /// <summary>
        /// Sets the data which will be handed over to the oldState.onExit(...).
        /// Works only if a new state has been scheduled to prevent dead references to data in the StateMachine.
        /// </summary>
        /// <param name="dataForNewState">Data you want to be handed over to the old state onExit(..)</param>
        public void SetDataForOldStateIfScheduled(params DataType[] dataForOldState)
        {
            if (_scheduled)
            {
                _scheduledOldStateData = dataForOldState;
            }
            else
            {
                Debug.LogError("SSM SetDataForOldScheduledSate(): Not state change scheduled, data will be ignored.");
            }
        }

        public void ChangeImmediately(IdType id)
        {
            ChangeImmediately(id, null, null);
        }

        /// <summary>
        /// In most cases it is advisable to use ScheduleChange() to avoid side effects.
        /// If you need to use it, it is best to call this at the end of another state's onUpdate() function.
        /// </summary>
        /// <param name="state">The id of the state you wish to enter.</param>
        /// <param name="dataForNewState">Data you want to be handed over to the new state onEnter(..)</param>
        /// <param name="dataForOldState">Data you want to be handed over to the old state onExit(..)</param>
        public void ChangeImmediately(IdType id, DataType[] dataForNewState, DataType[] dataForOldState)
        {
            if (DebugLogStateChanges)
            {
                if (_state != null && id.Equals(_state.Id))
                {
                    Debug.LogWarning("SSM(" + GetNameNotNull() + "): change from '" + _state.Id + "' to the same state '" + id.ToString() + "'. Is this done on purpose?");
                }
            }

            if (_states.ContainsKey(id) == false)
            {
                throw new Exception("SimpleState tries to enter an unknown state with id '" + id.ToString() + "'.");
            }

            if (DebugLogStateChanges)
            {
                if (_state != null)
                {
                    Debug.Log("SSM(" + GetNameNotNull() + "): " + _state.Id + " Exit (real time since start: " + Time.realtimeSinceStartup + " Sec., frame: " + Time.frameCount + ")");
                }
            }

            if (_state != null && _state.OnExit != null)
            {
                _state.OnExit(id, dataForOldState);
            }

            var previousStateId = _state != null ? _state.Id : default(IdType);
            _state = _states[id];

            // remember previous state id only if it really changed
            if (_previousStateId.Equals(id) == false)
            {
                _previousStateId = id;
            }

            if (DebugLogStateChanges)
            {
                if (_state != null)
                {
                    Debug.Log("SSM(" + GetNameNotNull() + "): " + _state.Id + " Enter (real time since start: " + Time.realtimeSinceStartup + " Sec., frame: " + Time.frameCount + ")");
                }
            }

            if (_state != null && _state.OnEnter != null)
            {
                _currentUpdateCounter = 0;
                _state.OnEnter(previousStateId, dataForNewState);
            }
        }

        /// <summary>
        /// Resets the state machine to the initial setup. No state will be active, thus nothing will happen.
        /// </summary>
        public void Reset()
        {
            _scheduled = false;
            _scheduledNewStateData = null;
            _scheduledOldStateData = null;
            _scheduledNewStateId = default(IdType);
            _state = null;
        }
    }
}
