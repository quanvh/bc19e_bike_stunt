using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kamgam.BikeAndCharacter25D
{
    [CreateAssetMenu(fileName = "CharacterConfig", menuName = "BikeAndCharacter25D/CharacterConfig", order = 1)]
    public class CharacterConfig : ScriptableObject
    {
        public float CharacterHeadBodyMass = 3;
        public float CharacterTorsoBodyMass = 10;
        public float CharacterUpperArmBodyMass = 2;
        public float CharacterLowerArmBodyMass = 2;
        public float CharacterUpperLegBodyMass = 2;
        public float CharacterLowerLegBodyMass = 2;

        public CharacterConfig Copy()
        {
            return (CharacterConfig) this.MemberwiseClone();
        }
    }
}