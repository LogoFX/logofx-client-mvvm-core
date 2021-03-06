﻿using System;
using System.Collections.Generic;
#if NET || NETCORE
using System.Windows;
using System.Windows.Data;
#endif
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#endif

namespace LogoFX.Client.Mvvm.Core
{
    /// <summary>
    /// <see cref="DependencyObject"/> based notifier on property change 
    /// </summary>
    public class NotificationHelperDp : DependencyObject
    {
        private readonly Action<object, object> _callback;
        private bool _isDetached = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationHelperDp"/> class.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public NotificationHelperDp(Action<object, object> callback)
        {
            _callback = callback;
        }

        /// <summary>
        /// Gets or sets the bind value.
        /// </summary>
        /// <value>
        /// The bind value.
        /// </value>
        public object BindValue
        {
            get { return (object)GetValue(BindValueProperty); }
            set { SetValue(BindValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BindValue.  This enables animation, styling, binding, etc...        
        /// <summary>
        /// The bind value property
        /// </summary>
        public static readonly DependencyProperty BindValueProperty =
            DependencyProperty.Register("BindValue", typeof(object), typeof(NotificationHelperDp),
                new PropertyMetadata(null, OnBindValueChanged));

        private static void OnBindValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NotificationHelperDp that = (NotificationHelperDp)d;

            if (!that._isDetached && that._callback != null
#if NET || NETCORE
 && BindingOperations.IsDataBound(that, BindValueProperty)
#endif
)
                that._callback(e.NewValue, e.OldValue);
        }

        /// <summary>
        /// Clears the bound value.
        /// </summary>
        public void Detach()
        {
            _isDetached = true;
            this.ClearValue(BindValueProperty);
        }
    }

    /// <summary>
    /// Notifications observer
    /// </summary>
    public static class BindNotifier
    {
        static readonly WeakKeyDictionary<object, Dictionary<string, NotificationHelperDp>> _notifiers = new WeakKeyDictionary<object, Dictionary<string, NotificationHelperDp>>();

        /// <summary>
        /// Subscribes supplied object to property changed notifications and invokes the provided callback
        /// </summary>
        /// <typeparam name="T">Type of subject</typeparam>
        /// <param name="vmb">Subject</param>
        /// <param name="path">Property path</param>
        /// <param name="callback">Notification callback</param>
        public static void NotifyOn<T>(this T vmb, string path, Action<object, object> callback)
        {
            Dictionary<string, NotificationHelperDp> block;
            if (!_notifiers.TryGetValue(vmb, out block))
            {
                _notifiers.Add(vmb, block = new Dictionary<string, NotificationHelperDp>());
            }
            block.Remove(path);

            NotificationHelperDp binder = new NotificationHelperDp(callback);
            BindingOperations.SetBinding(binder, NotificationHelperDp.BindValueProperty,
#if NET || NETCORE
 new Binding(path) { Source = vmb });
#else
            new Binding { Source = vmb,Path = new PropertyPath(path)});
#endif
            block.Add(path, binder);
        }

        /// <summary>
        /// Unsubscribes supplied object from property changed notifications
        /// </summary>
        /// <typeparam name="T">Type of subject</typeparam>
        /// <param name="vmb">Subject</param>
        /// <param name="path">Property path</param>
        public static void UnNotifyOn<T>(this T vmb, string path)
        {
            Dictionary<string, NotificationHelperDp> block;
            if (!_notifiers.TryGetValue(vmb, out block) || !block.ContainsKey(path))
            {
                return;
            }

            block[path].Detach();
            block.Remove(path);
        }
    }
}

