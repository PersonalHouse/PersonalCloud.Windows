namespace NSPersonalCloud.WindowsContract
{
    /// <summary>
    /// This interface defines IPC contract for showing UI pop-ups.
    /// </summary>
    public interface IPopupPresenter
    {
        /// <summary>
        /// Invoke to show an informational message where caller expects no response from the user.
        /// </summary>
        /// <param name="title">Window title for the alert.</param>
        /// <param name="message">Message body for the alert.</param>
        void ShowAlert(string title, string message);

        /*
        /// <summary>
        /// Invoke to show a message with 2 choice, one continues the action, and the other dismisses the message.
        /// </summary>
        /// <param name="title">Window title for the alert.</param>
        /// <param name="message">Message body for the alert.</param>
        /// <param name="positiveAction">Text for the button that continues the action.</param>
        /// <param name="neutralAction">Text for the button that dismisses the alert.</param>
        /// <param name="switchPositions">Switch positive and neutral buttons; usually used in scenarios where neutral action is preferred.</param>
        /// <returns>Whether user chose the positive action.</returns>
        bool ShowAlert(string title, string message, string positiveAction, string neutralAction, bool switchPositions = false);

        /// <summary>
        /// Invoke to show a fatal message. The receiving end terminates its app/process after user acknowledges the alert.
        /// </summary>
        /// <param name="title">Window title for the alert.</param>
        /// <param name="message">Message body for the alert.</param>
        void ShowFatalAlert(string title, string message);
        */
    }
}
