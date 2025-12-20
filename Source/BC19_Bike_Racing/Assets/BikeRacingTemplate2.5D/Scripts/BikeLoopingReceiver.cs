using Kamgam.BikeAndCharacter25D;
using Kamgam.Looping25D;
using UnityEngine;

namespace Kamgam.BikeRacing25D
{
	public class BikeLoopingReceiver : MonoBehaviour, ILoopingReceiver
	{
		public Bike Bike;
		public Character Character;

		public Transform[] VisualTransforms;

		public bool IsValid()
		{
			return this != null && gameObject != null;
		}

		public Vector3 GetPosition()
		{
			return Bike.transform.position;
		}

		public void SetPositionZ(float posZ)
		{
			foreach (var t in VisualTransforms)
			{
				var pos = t.position;
				pos.z = posZ;
				t.position = pos; 
			}
		}
	}
}