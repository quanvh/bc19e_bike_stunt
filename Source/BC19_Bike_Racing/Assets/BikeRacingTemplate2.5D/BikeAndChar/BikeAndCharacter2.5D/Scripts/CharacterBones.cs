using UnityEngine;

namespace Kamgam.BikeAndCharacter25D
{

    public class CharacterBones : MonoBehaviour
    {
        [Header("Bones")]
        public Transform Head;
        public Transform Torso;
        public Transform UpperArm;
        public Transform LowerArm;
        public Transform UpperLeg;
        public Transform LowerLeg;

        protected float HeadMemory;
        protected float TorsoMemory;
        protected float UpperArmMemory;
        protected float LowerArmMemory;
        protected float UpperLegMemory;
        protected float LowerLegMemory;

        public Transform HeadBone;
        public Transform TorsoBone;
        public Transform RightUpperArmBone;
        public Transform RightLowerArmBone;
        public Transform RightUpperLegBone;
        public Transform RightLowerLegBone;
        public Transform LeftUpperArmBone;
        public Transform LeftLowerArmBone;
        public Transform LeftUpperLegBone;
        public Transform LeftLowerLegBone;

        protected Quaternion HeadBoneMemory;
        protected Quaternion TorsoBoneMemory;
        protected Quaternion RightUpperArmBoneMemory;
        protected Quaternion RightLowerArmBoneMemory;
        protected Quaternion RightUpperLegBoneMemory;
        protected Quaternion RightLowerLegBoneMemory;

        protected Quaternion LeftUpperArmBoneMemory;
        protected Quaternion LeftLowerArmBoneMemory;
        protected Quaternion LeftUpperLegBoneMemory;
        protected Quaternion LeftLowerLegBoneMemory;

        protected Character character;

        public bool boneLoaded;

        private void Start()
        {
            boneLoaded = false;
            //memorize();
            character = this.GetComponentInParent<Character>();
        }


        public void FixedUpdate()
        {
            if (boneLoaded)
            {
                updateBones();
            }
        }

        public void memorize()
        {
            HeadMemory = deltaAngle2D(Torso, Head);
            TorsoMemory = 0; // torso is the root and therefore delta is always 0
            UpperArmMemory = deltaAngle2D(Torso, UpperArm);
            LowerArmMemory = deltaAngle2D(UpperArm, LowerArm);
            UpperLegMemory = deltaAngle2D(Torso, UpperLeg);
            LowerLegMemory = deltaAngle2D(UpperLeg, LowerLeg);

            HeadBoneMemory = HeadBone.localRotation;
            TorsoBoneMemory = TorsoBone.localRotation;
            RightUpperArmBoneMemory = RightUpperArmBone.localRotation;
            RightLowerArmBoneMemory = RightLowerArmBone.localRotation;
            RightUpperLegBoneMemory = RightUpperLegBone.localRotation;
            RightLowerLegBoneMemory = RightLowerLegBone.localRotation;

            LeftUpperArmBoneMemory = LeftUpperArmBone.localRotation;
            LeftLowerArmBoneMemory = LeftLowerArmBone.localRotation;
            LeftUpperLegBoneMemory = LeftUpperLegBone.localRotation;
            LeftLowerLegBoneMemory = LeftLowerLegBone.localRotation;
        }

        protected float deltaAngle2D(Transform a, Transform b)
        {
            return Mathf.DeltaAngle(a.localRotation.eulerAngles.z, b.localRotation.eulerAngles.z);
        }

        protected void updateBones()
        {
            updateBone(HeadBone, HeadBoneMemory, deltaAngle2D(Torso, Head) - HeadMemory, true);

            updateBone(RightUpperArmBone, RightUpperArmBoneMemory, deltaAngle2D(Torso, UpperArm) - UpperArmMemory, true);
            updateBone(RightLowerArmBone, RightLowerArmBoneMemory, deltaAngle2D(UpperArm, LowerArm) - LowerArmMemory, true);
            updateBone(RightUpperLegBone, RightUpperLegBoneMemory, deltaAngle2D(Torso, UpperLeg) - UpperLegMemory, true);
            updateBone(RightLowerLegBone, RightLowerLegBoneMemory, deltaAngle2D(UpperLeg, LowerLeg) - LowerLegMemory, true);

            updateBone(LeftUpperArmBone, LeftUpperArmBoneMemory, deltaAngle2D(Torso, UpperArm) - UpperArmMemory, true);
            updateBone(LeftLowerArmBone, LeftLowerArmBoneMemory, deltaAngle2D(UpperArm, LowerArm) - LowerArmMemory, true);
            updateBone(LeftUpperLegBone, LeftUpperLegBoneMemory, deltaAngle2D(Torso, UpperLeg) - UpperLegMemory, true);
            updateBone(LeftLowerLegBone, LeftLowerLegBoneMemory, deltaAngle2D(UpperLeg, LowerLeg) - LowerLegMemory, true);
        }

        protected void updateBone(Transform bone, Quaternion boneMemory, float deltaAngle, bool rightSide)
        {
            // reset
            bone.localRotation = boneMemory;
            if (rightSide)
            {
                bone.Rotate(Vector3.forward, deltaAngle, Space.World);
            }
            else
            {
                bone.Rotate(Vector3.back, deltaAngle, Space.World);
            }
        }
    }
}