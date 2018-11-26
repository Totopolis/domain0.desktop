using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Domain0.Desktop.Views.Dialogs
{
    public class ForceResetPasswordDialogData
    {
        public string Locale { get; internal set; }
    }

    public class ForceResetPasswordDialogSettings : MetroDialogSettings
    {
        public ForceResetPasswordDialogSettings()
        {
        }

        public List<string> Locales { get; set; }
        public string LocaleInitial { get; set; }
    }

    public partial class ForceResetPasswordDialog : CustomDialog
    {
        public ForceResetPasswordDialog()
            : this(null, null)
        {
        }

        public ForceResetPasswordDialog(MetroWindow parentWindow)
            : this(parentWindow, null)
        {
        }

        public ForceResetPasswordDialog(MetroWindow parentWindow, ForceResetPasswordDialogSettings settings)
            : base(parentWindow, settings)
        {
            InitializeComponent();

            this.Locale = settings.LocaleInitial;
            this.Locales = settings.Locales;
        }


        public Task<ForceResetPasswordDialogData> WaitForButtonPressAsync()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Focus();
            }));

            TaskCompletionSource<ForceResetPasswordDialogData> tcs = new TaskCompletionSource<ForceResetPasswordDialogData>();

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
                    tcs.TrySetResult(new ForceResetPasswordDialogData
                    {
                        Locale = this.Locale
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

                tcs.TrySetResult(new ForceResetPasswordDialogData
                {
                    Locale = this.Locale
                });

                e.Handled = true;
            };

            this.PART_NegativeButton.KeyDown += negativeKeyHandler;
            this.PART_AffirmativeButton.KeyDown += affirmativeKeyHandler;
            
            this.KeyDown += escapeKeyHandler;

            this.PART_NegativeButton.Click += negativeHandler;
            this.PART_AffirmativeButton.Click += affirmativeHandler;

            return tcs.Task;
        }

        protected override void OnLoaded()
        {
            switch (this.DialogSettings.ColorScheme)
            {
                case MetroDialogColorScheme.Accented:
                    this.PART_NegativeButton.SetResourceReference(StyleProperty, "AccentedDialogHighlightedSquareButton");
                    break;
            }
        }

        public static readonly DependencyProperty LocaleProperty = DependencyProperty.Register("Locale", typeof(string), typeof(ForceResetPasswordDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty LocalesProperty = DependencyProperty.Register("Locales", typeof(List<string>), typeof(ForceResetPasswordDialog), new PropertyMetadata(default(List<string>)));
        public static readonly DependencyProperty AffirmativeButtonTextProperty = DependencyProperty.Register("AffirmativeButtonText", typeof(string), typeof(ForceResetPasswordDialog), new PropertyMetadata("OK"));
        public static readonly DependencyProperty NegativeButtonTextProperty = DependencyProperty.Register("NegativeButtonText", typeof(string), typeof(ForceResetPasswordDialog), new PropertyMetadata("Cancel"));

        public string Locale
        {
            get { return (string)this.GetValue(LocaleProperty); }
            set { this.SetValue(LocaleProperty, value); }
        }

        public List<string> Locales
        {
            get { return (List<string>)this.GetValue(LocalesProperty); }
            set { this.SetValue(LocalesProperty, value); }
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

    }
}
