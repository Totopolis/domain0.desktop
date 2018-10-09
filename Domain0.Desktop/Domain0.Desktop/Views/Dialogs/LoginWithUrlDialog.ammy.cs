using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Domain0.Desktop.Views.Dialogs
{
    public class LoginWithUrlDialogData
    {
        public string Url { get; internal set; }

        public string Username { get; internal set; }

        public string Password
        {
            [SecurityCritical]
            get
            {
                IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(this.SecurePassword);
                try
                {
                    return System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
                }
            }
        }

        public SecureString SecurePassword { get; internal set; }

        public bool ShouldRemember { get; internal set; }
    }

    public class LoginWithUrlDialogSettings : LoginDialogSettings
    {
        private const string DefaultUrlWatermark = "Url...";

        public LoginWithUrlDialogSettings()
        {
            UrlWatermark = DefaultUrlWatermark;
            ShouldHideUrl = true;
        }

        public string InitialUrl { get; set; }
        public string UrlWatermark { get; set; }
        public bool ShouldHideUrl { get; set; }
    }

    public partial class LoginWithUrlDialog : CustomDialog
    {
        public LoginWithUrlDialog()
            : this(null, null)
        {
        }

        public LoginWithUrlDialog(MetroWindow parentWindow)
            : this(parentWindow, null)
        {
        }

        public LoginWithUrlDialog(MetroWindow parentWindow, LoginWithUrlDialogSettings settings)
            : base(parentWindow, settings)
        {
            InitializeComponent();

            this.Username = settings.InitialUsername;
            this.Password = settings.InitialPassword;
            this.UsernameCharacterCasing = settings.UsernameCharacterCasing;
            this.UsernameWatermark = settings.UsernameWatermark;
            this.PasswordWatermark = settings.PasswordWatermark;
            this.NegativeButtonButtonVisibility = settings.NegativeButtonVisibility;
            this.ShouldHideUsername = settings.ShouldHideUsername;
            this.RememberCheckBoxVisibility = settings.RememberCheckBoxVisibility;
            this.RememberCheckBoxText = settings.RememberCheckBoxText;

            this.Url = settings.InitialUrl;
            this.UrlWatermark = settings.UrlWatermark;
            this.ShouldHideUrl = settings.ShouldHideUrl;
        }
        

        public Task<LoginWithUrlDialogData> WaitForButtonPressAsync()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Focus();
                if (string.IsNullOrEmpty(this.PART_TextBox.Text) && !this.ShouldHideUsername)
                {
                    this.PART_TextBox.Focus();
                }
                else
                {
                    this.PART_TextBox2.Focus();
                }
            }));

            TaskCompletionSource<LoginWithUrlDialogData> tcs = new TaskCompletionSource<LoginWithUrlDialogData>();

            RoutedEventHandler negativeHandler = null;
            KeyEventHandler negativeKeyHandler = null;

            RoutedEventHandler affirmativeHandler = null;
            KeyEventHandler affirmativeKeyHandler = null;

            KeyEventHandler escapeKeyHandler = null;

            Action cleanUpHandlers = null;

            var cancellationTokenRegistration = this.DialogSettings.CancellationToken.Register(() =>
            {
                cleanUpHandlers();
                tcs.TrySetResult(null);
            });

            cleanUpHandlers = () =>
            {
                this.PART_TextBox.KeyDown -= affirmativeKeyHandler;
                this.PART_TextBox2.KeyDown -= affirmativeKeyHandler;

                this.KeyDown -= escapeKeyHandler;

                this.PART_NegativeButton.Click -= negativeHandler;
                this.PART_AffirmativeButton.Click -= affirmativeHandler;

                this.PART_NegativeButton.KeyDown -= negativeKeyHandler;
                this.PART_AffirmativeButton.KeyDown -= affirmativeKeyHandler;

                cancellationTokenRegistration.Dispose();
            };

            escapeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(null);
                }
            };

            negativeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(null);
                }
            };

            affirmativeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();
                    tcs.TrySetResult(new LoginWithUrlDialogData
                    {
                        Username = this.Username,
                        SecurePassword = this.PART_TextBox2.SecurePassword,
                        ShouldRemember = this.RememberCheckBoxChecked,
                        Url = this.Url
                    });
                }
            };

            negativeHandler = (sender, e) =>
            {
                cleanUpHandlers();

                tcs.TrySetResult(null);

                e.Handled = true;
            };

            affirmativeHandler = (sender, e) =>
            {
                cleanUpHandlers();

                tcs.TrySetResult(new LoginWithUrlDialogData
                {
                    Username = this.Username,
                    SecurePassword = this.PART_TextBox2.SecurePassword,
                    ShouldRemember = this.RememberCheckBoxChecked,
                    Url = this.Url
                });

                e.Handled = true;
            };

            this.PART_NegativeButton.KeyDown += negativeKeyHandler;
            this.PART_AffirmativeButton.KeyDown += affirmativeKeyHandler;

            this.PART_TextBox.KeyDown += affirmativeKeyHandler;
            this.PART_TextBox2.KeyDown += affirmativeKeyHandler;

            this.KeyDown += escapeKeyHandler;

            this.PART_NegativeButton.Click += negativeHandler;
            this.PART_AffirmativeButton.Click += affirmativeHandler;

            return tcs.Task;
        }
        
        protected override void OnLoaded()
        {
            if (this.DialogSettings is LoginWithUrlDialogSettings settings && settings.EnablePasswordPreview)
            {
                if (this.FindResource("Win8MetroPasswordBox") is Style win8MetroPasswordStyle)
                {
                    this.PART_TextBox2.Style = win8MetroPasswordStyle;
                    // apply template again to fire the loaded event which is necessary for revealed password
                    this.PART_TextBox2.ApplyTemplate();
                }
            }

            this.AffirmativeButtonText = this.DialogSettings.AffirmativeButtonText;
            this.NegativeButtonText = this.DialogSettings.NegativeButtonText;

            switch (this.DialogSettings.ColorScheme)
            {
                case MetroDialogColorScheme.Accented:
                    this.PART_NegativeButton.SetResourceReference(StyleProperty, "AccentedDialogHighlightedSquareButton");
                    this.PART_TextBox.SetResourceReference(ForegroundProperty, "BlackColorBrush");
                    this.PART_TextBox2.SetResourceReference(ForegroundProperty, "BlackColorBrush");
                    break;
            }
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty UrlProperty = DependencyProperty.Register("Url", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty UrlWatermarkProperty = DependencyProperty.Register("UrlWatermark", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty ShouldHideUrlProperty = DependencyProperty.Register("ShouldHideUrl", typeof(bool), typeof(LoginWithUrlDialog), new PropertyMetadata(true));
        public static readonly DependencyProperty UsernameProperty = DependencyProperty.Register("Username", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty UsernameWatermarkProperty = DependencyProperty.Register("UsernameWatermark", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty UsernameCharacterCasingProperty = DependencyProperty.Register("UsernameCharacterCasing", typeof(CharacterCasing), typeof(LoginWithUrlDialog), new PropertyMetadata(default(CharacterCasing)));
        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register("Password", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty PasswordWatermarkProperty = DependencyProperty.Register("PasswordWatermark", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty AffirmativeButtonTextProperty = DependencyProperty.Register("AffirmativeButtonText", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata("OK"));
        public static readonly DependencyProperty NegativeButtonTextProperty = DependencyProperty.Register("NegativeButtonText", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata("Cancel"));
        public static readonly DependencyProperty NegativeButtonButtonVisibilityProperty = DependencyProperty.Register("NegativeButtonButtonVisibility", typeof(Visibility), typeof(LoginWithUrlDialog), new PropertyMetadata(Visibility.Collapsed));
        public static readonly DependencyProperty ShouldHideUsernameProperty = DependencyProperty.Register("ShouldHideUsername", typeof(bool), typeof(LoginWithUrlDialog), new PropertyMetadata(false));
        public static readonly DependencyProperty RememberCheckBoxVisibilityProperty = DependencyProperty.Register("RememberCheckBoxVisibility", typeof(Visibility), typeof(LoginWithUrlDialog), new PropertyMetadata(Visibility.Collapsed));
        public static readonly DependencyProperty RememberCheckBoxTextProperty = DependencyProperty.Register("RememberCheckBoxText", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata("Remember"));
        public static readonly DependencyProperty RememberCheckBoxCheckedProperty = DependencyProperty.Register("RememberCheckBoxChecked", typeof(bool), typeof(LoginWithUrlDialog), new PropertyMetadata(false));

        public string Message
        {
            get { return (string)this.GetValue(MessageProperty); }
            set { this.SetValue(MessageProperty, value); }
        }

        public string Url
        {
            get { return (string)this.GetValue(UrlProperty); }
            set { this.SetValue(UrlProperty, value); }
        }

        public string UrlWatermark
        {
            get { return (string)this.GetValue(UrlWatermarkProperty); }
            set { this.SetValue(UrlWatermarkProperty, value); }
        }

        public bool ShouldHideUrl
        {
            get { return (bool)this.GetValue(ShouldHideUrlProperty); }
            set { this.SetValue(ShouldHideUrlProperty, value); }
        }

        public string Username
        {
            get { return (string)this.GetValue(UsernameProperty); }
            set { this.SetValue(UsernameProperty, value); }
        }

        public string Password
        {
            get { return (string)this.GetValue(PasswordProperty); }
            set { this.SetValue(PasswordProperty, value); }
        }

        public string UsernameWatermark
        {
            get { return (string)this.GetValue(UsernameWatermarkProperty); }
            set { this.SetValue(UsernameWatermarkProperty, value); }
        }

        public CharacterCasing UsernameCharacterCasing
        {
            get { return (CharacterCasing)this.GetValue(UsernameCharacterCasingProperty); }
            set { this.SetValue(UsernameCharacterCasingProperty, value); }
        }

        public string PasswordWatermark
        {
            get { return (string)this.GetValue(PasswordWatermarkProperty); }
            set { this.SetValue(PasswordWatermarkProperty, value); }
        }

        public string AffirmativeButtonText
        {
            get { return (string)this.GetValue(AffirmativeButtonTextProperty); }
            set { this.SetValue(AffirmativeButtonTextProperty, value); }
        }

        public string NegativeButtonText
        {
            get { return (string)this.GetValue(NegativeButtonTextProperty); }
            set { this.SetValue(NegativeButtonTextProperty, value); }
        }

        public Visibility NegativeButtonButtonVisibility
        {
            get { return (Visibility)this.GetValue(NegativeButtonButtonVisibilityProperty); }
            set { this.SetValue(NegativeButtonButtonVisibilityProperty, value); }
        }

        public bool ShouldHideUsername
        {
            get { return (bool)this.GetValue(ShouldHideUsernameProperty); }
            set { this.SetValue(ShouldHideUsernameProperty, value); }
        }

        public Visibility RememberCheckBoxVisibility
        {
            get { return (Visibility)this.GetValue(RememberCheckBoxVisibilityProperty); }
            set { this.SetValue(RememberCheckBoxVisibilityProperty, value); }
        }

        public string RememberCheckBoxText
        {
            get { return (string)this.GetValue(RememberCheckBoxTextProperty); }
            set { this.SetValue(RememberCheckBoxTextProperty, value); }
        }

        public bool RememberCheckBoxChecked
        {
            get { return (bool)this.GetValue(RememberCheckBoxCheckedProperty); }
            set { this.SetValue(RememberCheckBoxCheckedProperty, value); }
        }
    }
}
