using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Domain0.Desktop.Views.Dialogs
{
    public class ChangePasswordDialogData
    {
        public string PasswordOld
        {
            [SecurityCritical]
            get
            {
                IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(this.SecurePasswordOld);
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

        public string PasswordNew
        {
            [SecurityCritical]
            get
            {
                IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(this.SecurePasswordNew);
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

        public SecureString SecurePasswordOld { get; internal set; }
        public SecureString SecurePasswordNew { get; internal set; }
    }

    public class ChangePasswordDialogSettings : MetroDialogSettings
    {
        private const string DefaultPasswordOldWatermark = "Old Password...";
        private const string DefaultPasswordNewWatermark = "New Password...";
        private const string DefaultPasswordNewRepeatWatermark = "Repeat New Password...";

        public ChangePasswordDialogSettings()
        {
            PasswordOldWatermark = DefaultPasswordOldWatermark;
            PasswordNewWatermark = DefaultPasswordNewWatermark;
            PasswordNewRepeatWatermark = DefaultPasswordNewRepeatWatermark;
        }

        public string PasswordOldWatermark { get; set; }
        public string PasswordNewWatermark { get; set; }
        public string PasswordNewRepeatWatermark { get; set; }
    }

    public partial class ChangePasswordDialog : CustomDialog, INotifyDataErrorInfo
    {
        public ChangePasswordDialog()
            : this(null, null)
        {
        }

        public ChangePasswordDialog(MetroWindow parentWindow)
            : this(parentWindow, null)
        {
        }

        public ChangePasswordDialog(MetroWindow parentWindow, ChangePasswordDialogSettings settings)
            : base(parentWindow, settings)
        {
            InitializeComponent();

            this.PasswordOldWatermark = settings.PasswordOldWatermark;
            this.PasswordNewWatermark = settings.PasswordNewWatermark;
            this.PasswordNewRepeatWatermark = settings.PasswordNewRepeatWatermark;

            this.IsValid = true;
        }


        public Task<ChangePasswordDialogData> WaitForButtonPressAsync()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Focus();
                this.PART_TextBox_PasswordOld.Focus();
            }));

            TaskCompletionSource<ChangePasswordDialogData> tcs = new TaskCompletionSource<ChangePasswordDialogData>();

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
                this.PART_TextBox_PasswordOld.KeyDown -= affirmativeKeyHandler;
                this.PART_TextBox_PasswordNew.KeyDown -= affirmativeKeyHandler;
                this.PART_TextBox_PasswordNewRepeat.KeyDown -= affirmativeKeyHandler;

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
                    tcs.TrySetResult(new ChangePasswordDialogData
                    {
                        SecurePasswordOld = this.PART_TextBox_PasswordOld.SecurePassword,
                        SecurePasswordNew = this.PART_TextBox_PasswordNew.SecurePassword
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

                tcs.TrySetResult(new ChangePasswordDialogData
                {
                    SecurePasswordOld = this.PART_TextBox_PasswordOld.SecurePassword,
                    SecurePasswordNew = this.PART_TextBox_PasswordNew.SecurePassword
                });

                e.Handled = true;
            };

            this.PART_NegativeButton.KeyDown += negativeKeyHandler;
            this.PART_AffirmativeButton.KeyDown += affirmativeKeyHandler;

            this.PART_TextBox_PasswordOld.KeyDown += affirmativeKeyHandler;
            this.PART_TextBox_PasswordNew.KeyDown += affirmativeKeyHandler;
            this.PART_TextBox_PasswordNewRepeat.KeyDown += affirmativeKeyHandler;

            this.KeyDown += escapeKeyHandler;

            this.PART_NegativeButton.Click += negativeHandler;
            this.PART_AffirmativeButton.Click += affirmativeHandler;

            return tcs.Task;
        }

        protected override void OnLoaded()
        {
                if (this.FindResource("Win8MetroPasswordBox") is Style win8MetroPasswordStyle)
                {
                    this.PART_TextBox_PasswordOld.Style = win8MetroPasswordStyle;
                    this.PART_TextBox_PasswordNew.Style = win8MetroPasswordStyle;
                    this.PART_TextBox_PasswordNewRepeat.Style = win8MetroPasswordStyle;
                    // apply template again to fire the loaded event which is necessary for revealed password
                    this.PART_TextBox_PasswordOld.ApplyTemplate();
                    this.PART_TextBox_PasswordNew.ApplyTemplate();
                    this.PART_TextBox_PasswordNewRepeat.ApplyTemplate();
                }
            
            this.AffirmativeButtonText = this.DialogSettings.AffirmativeButtonText;
            this.NegativeButtonText = this.DialogSettings.NegativeButtonText;

            switch (this.DialogSettings.ColorScheme)
            {
                case MetroDialogColorScheme.Accented:
                    this.PART_NegativeButton.SetResourceReference(StyleProperty, "AccentedDialogHighlightedSquareButton");
                    this.PART_TextBox_PasswordOld.SetResourceReference(ForegroundProperty, "BlackColorBrush");
                    this.PART_TextBox_PasswordNew.SetResourceReference(ForegroundProperty, "BlackColorBrush");
                    this.PART_TextBox_PasswordNewRepeat.SetResourceReference(ForegroundProperty, "BlackColorBrush");
                    break;
            }
        }

        public static readonly DependencyProperty PasswordOldProperty = DependencyProperty.Register("PasswordOld", typeof(string), typeof(ChangePasswordDialog), new PropertyMetadata(default(string), OnPasswordOldChangedCallBack));
        public static readonly DependencyProperty PasswordOldWatermarkProperty = DependencyProperty.Register("PasswordOldWatermark", typeof(string), typeof(ChangePasswordDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty PasswordNewProperty = DependencyProperty.Register("PasswordNew", typeof(string), typeof(ChangePasswordDialog), new PropertyMetadata(default(string), OnPasswordNewChangedCallBack));
        public static readonly DependencyProperty PasswordNewWatermarkProperty = DependencyProperty.Register("PasswordNewWatermark", typeof(string), typeof(ChangePasswordDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty PasswordNewRepeatProperty = DependencyProperty.Register("PasswordNewRepeat", typeof(string), typeof(ChangePasswordDialog), new PropertyMetadata(default(string), OnPasswordNewRepeatChangedCallBack));
        public static readonly DependencyProperty PasswordNewRepeatWatermarkProperty = DependencyProperty.Register("PasswordNewRepeatWatermark", typeof(string), typeof(ChangePasswordDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty IsValidProperty = DependencyProperty.Register("IsValid", typeof(bool), typeof(ChangePasswordDialog), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty AffirmativeButtonTextProperty = DependencyProperty.Register("AffirmativeButtonText", typeof(string), typeof(ChangePasswordDialog), new PropertyMetadata("OK"));
        public static readonly DependencyProperty NegativeButtonTextProperty = DependencyProperty.Register("NegativeButtonText", typeof(string), typeof(ChangePasswordDialog), new PropertyMetadata("Cancel"));


        public string PasswordOld
        {
            get { return (string)this.GetValue(PasswordOldProperty); }
            set { this.SetValue(PasswordOldProperty, value); }
        }
        public string PasswordOldWatermark
        {
            get { return (string)this.GetValue(PasswordOldWatermarkProperty); }
            set { this.SetValue(PasswordOldWatermarkProperty, value); }
        }
        public string PasswordNew
        {
            get { return (string)this.GetValue(PasswordNewProperty); }
            set { this.SetValue(PasswordNewProperty, value); }
        }
        public string PasswordNewWatermark
        {
            get { return (string)this.GetValue(PasswordNewWatermarkProperty); }
            set { this.SetValue(PasswordNewWatermarkProperty, value); }
        }
        public string PasswordNewRepeat
        {
            get { return (string)this.GetValue(PasswordNewRepeatProperty); }
            set { this.SetValue(PasswordNewRepeatProperty, value); }
        }
        public string PasswordNewRepeatWatermark
        {
            get { return (string)this.GetValue(PasswordNewRepeatWatermarkProperty); }
            set { this.SetValue(PasswordNewRepeatWatermarkProperty, value); }
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

        private static void OnPasswordOldChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ChangePasswordDialog)?.CheckOldPassword();
            (sender as ChangePasswordDialog)?.CheckNewPassword();
        }

        private static void OnPasswordNewChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ChangePasswordDialog)?.CheckNewPassword();
            (sender as ChangePasswordDialog)?.CheckRepeatPassword();
        }

        private static void OnPasswordNewRepeatChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ChangePasswordDialog)?.CheckRepeatPassword();
        }

        private void CheckOldPassword()
        {
            if (string.IsNullOrEmpty(PasswordOld))
            {
                _errors.TryAdd(nameof(PasswordOld),
                    new List<string> { "Enter old password" });
            }
            else
            {
                _errors.TryRemove(nameof(PasswordOld), out _);
            }

            CheckIsValid();
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(PasswordOld)));
        }

        private void CheckNewPassword()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(PasswordNew))
                errors.Add("Enter new password");
            if (PasswordNew == PasswordOld)
                errors.Add("Your old password and new password are the same");

            if (errors.Any())
            { 
                _errors.TryAdd(nameof(PasswordNew), errors);
            }
            else
            {
                _errors.TryRemove(nameof(PasswordNew), out _);
            }

            CheckIsValid();
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(PasswordNew)));
        }

        private void CheckRepeatPassword()
        {
            if (PasswordNew != PasswordNewRepeat)
            {
                _errors.TryAdd(nameof(PasswordNewRepeat),
                    new List<string> {"Your password and confirmation password do not match"});
            }
            else
            {
                _errors.TryRemove(nameof(PasswordNewRepeat), out _);
            }

            CheckIsValid();
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(PasswordNewRepeat)));
        }


        private ConcurrentDictionary<string, List<string>> _errors = new ConcurrentDictionary<string, List<string>>();

        public IEnumerable GetErrors(string propertyName)
        {
            _errors.TryGetValue(propertyName, out var errorsForName);
            return errorsForName;
        }

        public bool HasErrors => !IsValid;

        private void CheckIsValid()
        {
            IsValid = !_errors.Any(kv => kv.Value != null && kv.Value.Count > 0);
        }

        public bool IsValid
        {
            get { return (bool)this.GetValue(IsValidProperty); }
            set { this.SetValue(IsValidProperty, value); }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    }
}
