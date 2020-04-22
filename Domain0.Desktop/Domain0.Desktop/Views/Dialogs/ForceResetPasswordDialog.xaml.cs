using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Domain0.Desktop.Views.Dialogs
{
    public enum ResetWayType
    {
        [Description("By User Id")]
        UserId,
        [Description("By Phone Number")]
        Phone,
        [Description("By Email")]
        Email
    }

    public class ResetWayItem
    {
        public ResetWayType Type { get; set; }
        public string Name { get; set; }
    }

    public class ForceResetPasswordDialogData
    {
        public string Locale { get; internal set; }
        public ResetWayType ResetWay { get; internal set; }
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

            var resetWayType = typeof(ResetWayType);
            this.ResetWays = Enum.GetValues(resetWayType)
                .OfType<ResetWayType>()
                .Select(x =>
                {
                    var name = resetWayType.GetEnumName(x);
                    var memInfo = resetWayType.GetMember(name);
                    var attribute = memInfo[0]
                        .GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .FirstOrDefault();
                    if (attribute is DescriptionAttribute descriptionAttribute)
                        name = descriptionAttribute.Description;

                    return new ResetWayItem {Name = name, Type = x};
                })
                .ToList();
            this.ResetWay = ResetWays.FirstOrDefault();
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
                        Locale = this.Locale,
                        ResetWay = this.ResetWay.Type
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
                    Locale = this.Locale,
                    ResetWay = this.ResetWay.Type
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
        public static readonly DependencyProperty ResetWayProperty = DependencyProperty.Register("ResetWay", typeof(ResetWayItem), typeof(ForceResetPasswordDialog), new PropertyMetadata(default(ResetWayItem)));
        public static readonly DependencyProperty ResetWaysProperty = DependencyProperty.Register("ResetWays", typeof(List<ResetWayItem>), typeof(ForceResetPasswordDialog), new PropertyMetadata(default(List<ResetWayItem>)));
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

        public ResetWayItem ResetWay
        {
            get { return (ResetWayItem)this.GetValue(ResetWayProperty); }
            set { this.SetValue(ResetWayProperty, value); }
        }

        public List<ResetWayItem> ResetWays
        {
            get { return (List<ResetWayItem>)this.GetValue(ResetWaysProperty); }
            set { this.SetValue(ResetWaysProperty, value); }
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
