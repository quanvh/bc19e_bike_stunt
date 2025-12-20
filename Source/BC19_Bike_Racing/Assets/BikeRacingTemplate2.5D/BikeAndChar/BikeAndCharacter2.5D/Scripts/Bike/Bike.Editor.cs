#if UNITY_EDITOR
using UnityEngine;
using Kamgam.BikeAndCharacter25D.Helpers;

namespace Kamgam.BikeAndCharacter25D
{
    public partial class Bike // .Editor
    {
        public void OnValidate()
        {
            BackWheelGroundTouchTrigger.GroundLayers = GroundLayers;
            BackWheelOuterGroundTouchTrigger.GroundLayers = GroundLayers;
            FrontWheelGroundTouchTrigger.GroundLayers = GroundLayers;
        }
    }
}
#endif
