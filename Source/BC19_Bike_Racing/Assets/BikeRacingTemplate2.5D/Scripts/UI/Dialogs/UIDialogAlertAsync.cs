using System.Threading;
using System.Threading.Tasks;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Kamgam.BikeRacing25D
{
    /// <summary>
    /// Dialog alert class.
    /// Usage:
    ///  await UIDialogAlertAsync.Spawn("This is the message", ct);
    /// </summary>
    public class UIDialogAlertAsync : UIDialogSceneAsync<bool>
    {
        public TextMeshProUGUI TextTf;
        public Button ConfirmButton;

        /// <summary>
        /// Bootstrap method which loads the dialog scene and shows the dialog within.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sceneName"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<bool> Spawn(string text, CancellationToken ct)
        {
            var dialog = await UtilsAsync.LoadScene<UIDialogAlertAsync>("DialogAlert", UnityEngine.SceneManagement.LoadSceneMode.Additive, ct);
            dialog.SetData(text);
            return await dialog.executeWrapper(ct);
        }

        public void SetData(string text)
        {
            TextTf.text = text;
        }

        protected override async Task<bool> execute(CancellationToken ct)
        {
            var tasksToWaitFor = new List<Task>();
            _ = AddButtonPressToTasks(ct, tasksToWaitFor, ConfirmButton);

            var finishedTask = await UtilsAsync.WhenAny(tasksToWaitFor);
            await finishedTask; // await for error propagation

            return true;
        }
    }
}
