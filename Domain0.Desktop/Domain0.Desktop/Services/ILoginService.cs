using System;

namespace Domain0.Desktop.Services
{
    public interface ILoginService
    {
        /// <summary>
        /// Shows Login dialog
        /// </summary>
        /// <param name="onSuccess">Will be called if login was successful</param>
        void ShowLogin(Action onSuccess = null);

        /// <summary>
        /// Try to load previously saved token
        /// </summary>
        /// <returns>True if token was loaded successfully</returns>
        bool LoadPreviousToken();
    }
}
