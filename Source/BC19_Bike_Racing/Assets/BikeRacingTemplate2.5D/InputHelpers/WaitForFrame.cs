using UnityEngine;

namespace Kamgam.InputHelpers
{
    /// <summary>
    /// Waits for the given number of frames.<br />
    /// Research: It might be possible to simply write "yield return #", where # is the number of frames to wait. TODO: investigate.
    /// </summary>
    public class WaitForFrames : CustomYieldInstruction
    {
        private int _targetFrameCount;

        public WaitForFrames(int numberOfFrames)
        {
            _targetFrameCount = Time.frameCount + numberOfFrames;
        }

        public override bool keepWaiting
        {
            get
            {
                return Time.frameCount < _targetFrameCount;
            }
        }
    }
}
