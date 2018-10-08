using System;

namespace Domain0.Desktop.Services
{
    public interface ILoginService
    {
        /// <summary>
        /// Shows Login dialog
        /// </summary>
        /// <param name="onSuccess">Will be called if login was successful</param>
        void ShowLogin(Action onSuccess);
    }
}
