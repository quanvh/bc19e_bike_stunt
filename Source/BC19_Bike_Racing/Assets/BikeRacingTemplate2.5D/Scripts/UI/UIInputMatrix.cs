namespace Kamgam.BikeRacing25D
{
    public enum UIStack
    {
        Debug, Menu, Loading, ModalDialog, Game
    }

    public class UIInputMatrix : Kamgam.InputHelpers.InputMatrix<UIStack>
    {
        public static void Init(Kamgam.InputHelpers.InputMatrix<UIStack> matrix)
        {
            matrix.SetMatrix(
                new UIStack?[,]
                {
                    { UIStack.Debug       },
                    { UIStack.Loading     },
                    { UIStack.ModalDialog },
                    { UIStack.Menu        },
                    { UIStack.Game        }
                }
            );
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Bike Racing Template/InputMatrix - Log Active", priority = 650)]
        public static void LogActiveListenersInEditor()
        {
            Instance.LogActiveListeners();
        }
#endif
    }
}
