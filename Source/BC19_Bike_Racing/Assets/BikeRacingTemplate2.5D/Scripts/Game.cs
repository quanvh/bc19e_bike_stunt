using Kamgam.BikeAndCharacter25D;
using System;
using UnityEngine;

namespace Kamgam.BikeRacing25D
{
	/// <summary>
	/// The game uses a simple state machine:
	/// 
	/// Idle > WaitingToRace > Race > End > (back to Idle)
	///             ^            |
	///             \___________/
	///               paused (paused is not a state but a bool flag)
	/// 
	/// </summary>
	public class Game
	{
		// Callbacks
		public System.Action<int> OnUnloadLevel;
		public System.Action<bool,LevelData> OnLevelFinished;

		// References to things the Game needs permanently
		protected UIManager uiMananger;
		protected GameObject bikeAndCharPrefab;

		// References to things that are used while a level is loaded.
		protected BikeAndCharacter bikeAndCharacter;
		protected Cameraman2D cameraman;
		protected ILevel level;
		protected LevelData levelData;

		// States
		protected SimpleStateMachine<State,object> stateMachine;
		public enum State { Idle, WaitingToStart, Racing, End }

		/// <summary>
		/// Paused is a sub state flag which does not change the state of the stateMachine.
		/// </summary>
		protected bool paused;

		/// <summary>
		/// The race time in seconds.
		/// </summary>
		protected float raceTime;

		/// <summary>
		/// Quick reference to an often used ui (it's used every frame)
		/// </summary>
		protected UIGameTime cachedUIGameTime;

		public Game(UIManager uiMananger, GameObject bikeAndCharPrefab)
        {
            this.uiMananger = uiMananger;
			this.bikeAndCharPrefab = bikeAndCharPrefab;

			Physics2D.simulationMode = SimulationMode2D.Script;
			cameraman = new Cameraman2D();

			stateMachine = new SimpleStateMachine<State, object>("Game");
			// stateMachine.DebugLogStateChanges = true;
			stateMachine.AddState(State.Idle, stateIdleEnter);
			stateMachine.AddState(State.WaitingToStart, stateWaitingToStartEnter, stateWaitingToStartUpdate);
			stateMachine.AddState(State.Racing, stateRacingEnter, stateRacingUpdate);
			stateMachine.AddState(State.End, stateEndEnter);

			// start with idle state
			stateMachine.SetInitialState(State.Idle);

			// UI listeners
			uiMananger.GetUI<UIGamePause>().OnReturnToMenu += onReturnToMenu;
			uiMananger.GetUI<UIGamePause>().OnRestart += onRestart;
			uiMananger.GetUI<UIGamePause>().OnUnpause += onUnPause;

			uiMananger.GetUI<UIGameEndLoss>().OnReturnToMenu += onReturnToMenu;
			uiMananger.GetUI<UIGameEndLoss>().OnRestart += onRestart;

			uiMananger.GetUI<UIGameEndWin>().OnReturnToMenu += onReturnToMenu;
			uiMananger.GetUI<UIGameEndWin>().OnRestart += onRestart;

			uiMananger.GetUI<UIGameMenuForTouch>().OnRestart += onRestart;
			uiMananger.GetUI<UIGameMenuForTouch>().OnPause += onPause;
		}

		public void InitAfterLevelLoad(ILevel level, LevelData levelData)
		{
			this.level = level;
			this.levelData = levelData;

			level.GetGoal().OnGoalReached += onGoalReached;

			onRestart();
		}

		public void CleanUpAfterLevelUnload()
		{
			level = null;
			levelData = null;
			bikeAndCharacter = null;
		}

		void onReturnToMenu()
        {
			stateMachine.ScheduleStateChangeIfNecessary(State.Idle);
			stateMachine.Update();

			OnUnloadLevel?.Invoke(levelData.LevelNr);
		}

		void onRestart()
		{
			stateMachine.ScheduleStateChangeIfNecessary(State.Idle);
			stateMachine.Update();

			resetToStart();

			stateMachine.ScheduleStateChange(State.WaitingToStart);
			stateMachine.Update();
		}

		void onPause()
		{
			paused = true;
			bikeAndCharacter.HandleUserInput = false;
			uiMananger.Show<UIGamePause>();
			uiMananger.Hide<UIGameMenuForTouch>();
			uiMananger.Hide<UIGameMenuForKeyboard>();
		}

		void onUnPause()
		{
			uiMananger.Hide<UIGamePause>();
			stateMachine.ScheduleStateChange(State.WaitingToStart);
		}

		private void onGoalReached()
        {
            if (stateMachine.IsOrWillBeState(State.Racing))
            {
				stateMachine.ScheduleStateChange(State.End);
            }
        }

		protected void resetToStart()
        {
			paused = false;
			raceTime = 0f;
			respawnBike();
		}

		protected void respawnBike()
		{
			// destroy old bike
			if (bikeAndCharacter != null)
			{
				GameObject.Destroy(bikeAndCharacter.gameObject);
				bikeAndCharacter = null;
				cameraman.SetObjectToTrack(null);
			}

			// create new
			bikeAndCharacter = BikeAndCharacter25D.Helpers.Utils.SmartInstantiatePrefab<BikeAndCharacter>(bikeAndCharPrefab, null, false);
			bikeAndCharacter.transform.position = level.GetBikeSpawnPosition().position;
			bikeAndCharacter.gameObject.SetActive(true);

			// hook up touch input if needed
			if(Main.IsTouchEnabled())
				bikeAndCharacter.TouchInput = uiMananger.GetUI<UIGameMenuForTouch>();

			// disable input
			bikeAndCharacter.HandleUserInput = false;

			// brake by default
			bikeAndCharacter.Bike.IsBraking = true;

			// inform cameraman
			cameraman.SetObjectToTrack(bikeAndCharacter.Character.TorsoBody);
			cameraman.SetCameraToMove(level.GetCamera().transform);
		}

		#region Updates
		public void Update()
		{
			stateMachine.Update();

			if (Input.GetKey(KeyCode.Backspace))
			{
				onRestart();
			}
		}

		public void LateUpdate()
		{
			cameraman.LateUpdate();
		}

		public void FixedUpdate()
		{
			stateMachine.FixedUpdate();

			if (!paused)
				Physics2D.Simulate(Time.fixedDeltaTime);
		}
		#endregion

		// STATE: Idle
		///////////////////////////////////
		void stateIdleEnter(State comingFrom, object[] data)
        {
			uiMananger.Hide<UIGameWaitingToStart>();
			uiMananger.Hide<UIGamePause>();
			uiMananger.Hide<UIGameTime>();
			uiMananger.Hide<UIGameMenuForKeyboard>();
			uiMananger.Hide<UIGameMenuForTouch>();
			uiMananger.Hide<UIGameEndWin>();
			uiMananger.Hide<UIGameEndLoss>();
		}

		// STATE: WaitingToStart
		///////////////////////////////////
		#region State WaitingToStart

		void stateWaitingToStartEnter(State comingFrom, object[] data)
		{
			// hide uis
			uiMananger.Hide<UIGameEndWin>();
			uiMananger.Hide<UIGameEndLoss>();
			uiMananger.Hide<UIGamePause>();

			// show uis
			uiMananger.Show<UIGameWaitingToStart>();
			uiMananger.Show<UIGameTime>();
			if (Main.IsTouchEnabled())
				uiMananger.Show<UIGameMenuForTouch>();
			else
				uiMananger.Show<UIGameMenuForKeyboard>();

			// cache frequently used uis
			cachedUIGameTime = uiMananger.GetUI<UIGameTime>();
			cachedUIGameTime.SetTime(raceTime);
		}

		void stateWaitingToStartUpdate(int frameNr)
		{
			// Wait for the first user input to start the race
			bool start;
			if (Main.IsTouchEnabled())
				start = (uiMananger.GetUI<UIGameMenuForTouch>() as IBikeTouchInput).IsSpeedUpPressed();
			else
				start = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.Space);
			
			if (start)
				stateMachine.ScheduleStateChange(State.Racing);
		}
		#endregion


		// STATE: Racing
		///////////////////////////////////
		#region State Racing
		
		void stateRacingEnter(State comingFrom, object[] data)
		{
			paused = false;
			bikeAndCharacter.HandleUserInput = true;
			uiMananger.HideImmediate<UIGameWaitingToStart>();
		}

		void stateRacingUpdate(int frameNr)
		{
			raceTime += Time.unscaledDeltaTime;
			cachedUIGameTime.SetTime(raceTime);

			if (Input.GetKeyUp(KeyCode.Escape))
			{
				onPause();
			}
		}

		#endregion


		// STATE: End
		///////////////////////////////////
		#region State End

		void stateEndEnter(State comingFrom, object[] data)
		{
			cachedUIGameTime.SetTime(raceTime);

			// stop bike
			bikeAndCharacter.StopAllInput();
			bikeAndCharacter.Bike.IsBraking = true;

			// disable input
			bikeAndCharacter.HandleUserInput = false;

			// hide racing uis
			uiMananger.Hide<UIGameTime>();
			uiMananger.Hide<UIGameMenuForTouch>();
			uiMananger.Hide<UIGameMenuForKeyboard>();

			// show end ui
			bool win = raceTime <= levelData.TimeToBeat;
			if(win)
            {
				uiMananger.Show<UIGameEndWin>();
				var winUI = uiMananger.GetUI<UIGameEndWin>();
				winUI.SetTime(raceTime);
            }
			else
            {
				uiMananger.Show<UIGameEndLoss>();
				var lossUI = uiMananger.GetUI<UIGameEndLoss>();
				lossUI.SetTime(raceTime, levelData);
			}

			OnLevelFinished.Invoke(win, levelData);
		}

		#endregion
	}
}

