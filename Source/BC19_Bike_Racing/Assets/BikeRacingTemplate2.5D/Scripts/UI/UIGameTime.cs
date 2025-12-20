using UnityEngine;
using TMPro;
using System.Text;

namespace Kamgam.BikeRacing25D
{
    public class UIGameTime : UIBase
    {
        public TextMeshProUGUI TimeTf;

        public override UIStack GetUIStack() => UIStack.Game;
        public override bool AllowParallelInput() => true;

        public void SetTime(float timeInSec)
        {
            TimeTf.text = FormatRaceTime(timeInSec);
        }

        protected static StringBuilder raceTimeBuilderMSS = new StringBuilder(9, 10);

        public static string FormatRaceTime(float timeInSeconds, bool forceShowMinutes = false)
        {
            if (timeInSeconds < 0)
            {
                timeInSeconds *= -1;
            }

            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds - minutes * 60);
            int subsec = Mathf.FloorToInt((timeInSeconds - seconds - minutes * 60) * 100); // 1/100 second

            // < 1 Min
            raceTimeBuilderMSS.Clear();
            if (minutes > 0 || forceShowMinutes)
            {
                raceTimeBuilderMSS.Append(minutes.ToString());

                raceTimeBuilderMSS.Append(':');

                if (seconds < 10) raceTimeBuilderMSS.Append('0');
                raceTimeBuilderMSS.Append(seconds.ToString());

                raceTimeBuilderMSS.Append(':');

                if (subsec < 10) raceTimeBuilderMSS.Append('0');
                raceTimeBuilderMSS.Append(subsec.ToString());
            }
            else if (seconds > 0)
            {
                if (seconds < 10) raceTimeBuilderMSS.Append('0');
                raceTimeBuilderMSS.Append(seconds.ToString());

                raceTimeBuilderMSS.Append(':');

                if (subsec < 10) raceTimeBuilderMSS.Append('0');
                raceTimeBuilderMSS.Append(subsec.ToString());
            }
            else
            {
                raceTimeBuilderMSS.Append('0');
                raceTimeBuilderMSS.Append(':');

                if (subsec < 10) raceTimeBuilderMSS.Append('0');
                raceTimeBuilderMSS.Append(subsec.ToString());
            }

            return raceTimeBuilderMSS.ToString();
        }
    }
}
