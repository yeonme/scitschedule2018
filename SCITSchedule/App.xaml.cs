using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows;

namespace SCITSchedule
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            using (var appLock = new SingleInstanceApplicationLock())
            {
                if (!appLock.TryAcquireExclusiveLock())
                    return;

                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }
    }

    sealed class SingleInstanceApplicationLock : IDisposable
    {
        ~SingleInstanceApplicationLock()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool TryAcquireExclusiveLock()
        {
            try
            {
                if (!_mutex.WaitOne(1000, false))
                    return false;
            }
            catch (AbandonedMutexException)
            {
                // Abandoned mutex, just log? Multiple instances
                // may be executed in this condition...
            }

            return _hasAcquiredExclusiveLock = true;
        }

        private const string MutexId = @"Local\{1109F104-B4B4-4ED1-920C-F4D8EFE9E833}";
        private readonly Mutex _mutex = CreateMutex();
        private bool _hasAcquiredExclusiveLock, _disposed;

        private void Dispose(bool disposing)
        {
            if (disposing && !_disposed && _mutex != null)
            {
                try
                {
                    if (_hasAcquiredExclusiveLock)
                        _mutex.ReleaseMutex();

                    //_mutex.Dispose();
                }
                finally
                {
                    _disposed = true;
                }
            }
        }

        private static Mutex CreateMutex()
        {
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var allowEveryoneRule = new MutexAccessRule(sid,
                MutexRights.FullControl, AccessControlType.Allow);

            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            var mutex = new Mutex(false, MutexId);
            mutex.SetAccessControl(securitySettings);

            return mutex;
        }
    }
}
