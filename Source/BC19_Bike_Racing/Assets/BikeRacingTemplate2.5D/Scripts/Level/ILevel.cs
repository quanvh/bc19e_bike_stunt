using UnityEngine;

namespace Kamgam.BikeRacing25D
{
	/// <summary>
	/// Each level requires at least one object which implements this interface.<br />
	/// Typically that's a Level object at the root of the scene.
	/// </summary>
	public interface ILevel
	{
		/// <summary>
		/// Finds and returns the transform which is used as spawn position for the bike.
		/// </summary>
		/// <returns>Spawns position transform.</returns>
		Transform GetBikeSpawnPosition();

		/// <summary>
		/// Finds and returns the game which should be used for this level.
		/// </summary>
		/// <returns>Level camera</returns>
		Camera GetCamera();

		/// <summary>
		/// Finds and returns the goal of the level.
		/// </summary>
		/// <returns>Level goal</returns>
		Goal GetGoal();

		/// <summary>
		/// Called by the Game logic after the scene has loaded. Use this to initialize the level parts.
		/// </summary>
		void InitAfterLoad();

		/// <summary>
		/// Once all the parts of a level have been properly set up this will return true. Otherwise false.
		/// </summary>
		/// <returns></returns>
		bool IsReady();
	}
}

