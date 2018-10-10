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
    public enum LoginMode
    {
        Phone,
        Email
    }

    public class LoginWithUrlDialogData
    {
        public string Url { get; internal set; }

        public string Email { get; internal set; }
        public string Phone { get; internal set; }
        public LoginMode LoginMode { get; internal set; }

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

    public class LoginWithUrlDialogSettings : MetroDialogSettings
    {
        private const string DefaultUrlWatermark = "Url...";
        private const string DefaultEmailWatermark = "Email...";
        private const string DefaultPhoneWatermark = "Phone...";
        private const string DefaultPasswordWatermark = "Password...";
        private const string DefaultRememberCheckBoxText = "Remember";

        public LoginWithUrlDialogSettings()
        {
            UrlWatermark = DefaultUrlWatermark;
            ShouldHideUrl = true;

            EmailWatermark = DefaultEmailWatermark;
            PhoneWatermark = DefaultPhoneWatermark;
            PasswordWatermark = DefaultPasswordWatermark;

            RememberCheckBoxText = DefaultRememberCheckBoxText;
        }

        public string InitialUrl { get; set; }
        public string UrlWatermark { get; set; }
        public bool ShouldHideUrl { get; set; }

        public LoginMode LoginMode { get; set; }

        public bool EnablePasswordPreview { get; set; }

        public string EmailWatermark { get; set; }
        public string PhoneWatermark { get; set; }
        public string PasswordWatermark { get; set; }

        public string RememberCheckBoxText { get; set; }

        public Visibility NegativeButtonVisibility { get; set; }
        public Visibility RememberCheckBoxVisibility { get; set; }
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

            this.EmailWatermark = settings.EmailWatermark;
            this.PhoneWatermark = settings.PhoneWatermark;
            this.PasswordWatermark = settings.PasswordWatermark;
            this.NegativeButtonButtonVisibility = settings.NegativeButtonVisibility;
            this.RememberCheckBoxVisibility = settings.RememberCheckBoxVisibility;
            this.RememberCheckBoxText = settings.RememberCheckBoxText;

            this.Url = settings.InitialUrl;
            this.UrlWatermark = settings.UrlWatermark;
            this.ShouldHideUrl = settings.ShouldHideUrl;

            this.LoginMode = (int)settings.LoginMode;
        }
        

        public Task<LoginWithUrlDialogData> WaitForButtonPressAsync()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Focus();
                if (ShouldHideUrl)
                    this.PART_TextBox_Phone.Focus();
                else
                    this.PART_TextBox_Url.Focus();
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
                this.PART_TextBox_Email.KeyDown -= affirmativeKeyHandler;
                this.PART_TextBox_Phone.KeyDown -= affirmativeKeyHandler;
                this.PART_TextBox_Password.KeyDown -= affirmativeKeyHandler;

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
                        Email = this.Email,
                        Phone = this.Phone,
                        LoginMode = (LoginMode)this.LoginMode,
                        SecurePassword = this.PART_TextBox_Password.SecurePassword,
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
                    Email = this.Email,
                    Phone = this.Phone,
                    LoginMode = (LoginMode)this.LoginMode,
                    SecurePassword = this.PART_TextBox_Password.SecurePassword,
                    ShouldRemember = this.RememberCheckBoxChecked,
                    Url = this.Url
                });

                e.Handled = true;
            };

            this.PART_NegativeButton.KeyDown += negativeKeyHandler;
            this.PART_AffirmativeButton.KeyDown += affirmativeKeyHandler;

            this.PART_TextBox_Email.KeyDown += affirmativeKeyHandler;
            this.PART_TextBox_Phone.KeyDown += affirmativeKeyHandler;
            this.PART_TextBox_Password.KeyDown += affirmativeKeyHandler;

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
                    this.PART_TextBox_Password.Style = win8MetroPasswordStyle;
                    // apply template again to fire the loaded event which is necessary for revealed password
                    this.PART_TextBox_Password.ApplyTemplate();
                }
            }

            this.AffirmativeButtonText = this.DialogSettings.AffirmativeButtonText;
            this.NegativeButtonText = this.DialogSettings.NegativeButtonText;

            switch (this.DialogSettings.ColorScheme)
            {
                case MetroDialogColorScheme.Accented:
                    this.PART_NegativeButton.SetResourceReference(StyleProperty, "AccentedDialogHighlightedSquareButton");
                    this.PART_TextBox_Email.SetResourceReference(ForegroundProperty, "BlackColorBrush");
                    this.PART_TextBox_Phone.SetResourceReference(ForegroundProperty, "BlackColorBrush");
                    this.PART_TextBox_Password.SetResourceReference(ForegroundProperty, "BlackColorBrush");
                    break;
            }
        }

        public static readonly DependencyProperty UrlProperty = DependencyProperty.Register("Url", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty UrlWatermarkProperty = DependencyProperty.Register("UrlWatermark", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty ShouldHideUrlProperty = DependencyProperty.Register("ShouldHideUrl", typeof(bool), typeof(LoginWithUrlDialog), new PropertyMetadata(true));
        public static readonly DependencyProperty EmailProperty = DependencyProperty.Register("Email", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty EmailWatermarkProperty = DependencyProperty.Register("EmailWatermark", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty PhoneProperty = DependencyProperty.Register("Phone", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty PhoneWatermarkProperty = DependencyProperty.Register("PhoneWatermark", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty UsernameCharacterCasingProperty = DependencyProperty.Register("UsernameCharacterCasing", typeof(CharacterCasing), typeof(LoginWithUrlDialog), new PropertyMetadata(default(CharacterCasing)));
        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register("Password", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty PasswordWatermarkProperty = DependencyProperty.Register("PasswordWatermark", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty AffirmativeButtonTextProperty = DependencyProperty.Register("AffirmativeButtonText", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata("OK"));
        public static readonly DependencyProperty NegativeButtonTextProperty = DependencyProperty.Register("NegativeButtonText", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata("Cancel"));
        public static readonly DependencyProperty NegativeButtonButtonVisibilityProperty = DependencyProperty.Register("NegativeButtonButtonVisibility", typeof(Visibility), typeof(LoginWithUrlDialog), new PropertyMetadata(Visibility.Collapsed));
        public static readonly DependencyProperty RememberCheckBoxVisibilityProperty = DependencyProperty.Register("RememberCheckBoxVisibility", typeof(Visibility), typeof(LoginWithUrlDialog), new PropertyMetadata(Visibility.Collapsed));
        public static readonly DependencyProperty RememberCheckBoxTextProperty = DependencyProperty.Register("RememberCheckBoxText", typeof(string), typeof(LoginWithUrlDialog), new PropertyMetadata("Remember"));
        public static readonly DependencyProperty RememberCheckBoxCheckedProperty = DependencyProperty.Register("RememberCheckBoxChecked", typeof(bool), typeof(LoginWithUrlDialog), new PropertyMetadata(false));
        public static readonly DependencyProperty LoginModeProperty = DependencyProperty.Register("LoginMode", typeof(int), typeof(LoginWithUrlDialog), new PropertyMetadata(default(int)));

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

        public string Email
        {
            get { return (string)this.GetValue(EmailProperty); }
            set { this.SetValue(EmailProperty, value); }
        }

        public string Phone
        {
            get { return (string)this.GetValue(PhoneProperty); }
            set { this.SetValue(PhoneProperty, value); }
        }

        public string Password
        {
            get { return (string)this.GetValue(PasswordProperty); }
            set { this.SetValue(PasswordProperty, value); }
        }

        public string EmailWatermark
        {
            get { return (string)this.GetValue(EmailWatermarkProperty); }
            set { this.SetValue(EmailWatermarkProperty, value); }
        }

        public string PhoneWatermark
        {
            get { return (string)this.GetValue(PhoneWatermarkProperty); }
            set { this.SetValue(PhoneWatermarkProperty, value); }
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

        public int LoginMode
        {
            get { return (int)this.GetValue(LoginModeProperty); }
            set { this.SetValue(LoginModeProperty, LoginMode); }
        }
    }
}
