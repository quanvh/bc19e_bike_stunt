namespace Kamgam.BikeRacing25D
{
    public interface IUI
    {
        /// <summary>
        /// Returns the LOGICAL visibility state of the ui.
        /// This shoud be true immediately after RequestShow()
        /// was called and false immediately after RequestHide() 
        /// was called.
        /// </summary>
        /// <returns></returns>
        bool IsShown();

        /// <summary>
        /// Asks the ui to show itself. It may do this asynchronously.
        /// </summary>
        void Show();

        /// <summary>
        /// Asks the ui to hide itself. It may do this asynchronously.
        /// </summary>
        void Hide();

        /// <summary>
        /// Shows the ui immediately. All async operations are aborted.
        /// </summary>
        void ShowImmediate();

        /// <summary>
        /// Hides the ui immediately. All async operations are aborted.
        /// </summary>
        void HideImmediate();

        /// <summary>
        /// True while the ui is transitioning (usually while it's animating).
        /// </summary>
        /// <returns></returns>
        bool IsTransitioning();

        void SetSortOrder(int sortOrder);
        int GetSortOrder();

        /// <summary>
        /// Some things (like adding triggers) should be done even before showing the UI for the very first time as it may cause hickups.<br />
        /// Call this to initialize the UI before shoing it for the first time.
        /// </summary>
        public void Initialize();
    }
}
