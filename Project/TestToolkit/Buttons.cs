using Codeer.Friendly;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;
using Codeer.Friendly.Windows.NativeStandardControls;
using RM.Friendly.WPFStandardControls;
using System;

namespace TestToolkit
{
    public interface IClickable
    {
        void EmulateClick();
    }

    public interface IClickable<T>
    {
        T EmulateClick();
    }

    public class ClickModal<T> : IClickable<T>
    {
        WindowsAppFriend _app;
        ShowModal _show;
        CreateWindowDriver _create;
        public delegate T CreateWindowDriver(WindowControl window, Async async);
        public delegate void ShowModal(Async async);

        public ClickModal(WindowsAppFriend app, ShowModal show, CreateWindowDriver create)
        {
            _app = app;
            _show = show;
            _create = create;
        }

        public T EmulateClick()
        {
            var current = WindowControl.FromZTop(_app);
            var a = new Async();
            _show(a);
            var w = current.WaitForNextModal(a);
            return w == null ? default(T) : _create(w, a);
        }
    }

    public class ClickModeless<T> : IClickable<T>
    {
        public delegate T CreateWindowDriver();
        public delegate void ShowModeless();

        ShowModeless _show;
        CreateWindowDriver _create;

        public ClickModeless(ShowModeless show, CreateWindowDriver create)
        {
            _show = show;
            _create = create;
        }

        public T EmulateClick()
        {
            _show();
            return _create();
        }
    }

    public class ClickSync : IClickable
    {
        public delegate void Finish();
        public delegate void Sync();

        Finish _finish;
        Sync _sync;

        public ClickSync(Finish finish, Sync sync)
        {
            _finish = finish;
            _sync = sync;
        }

        public void EmulateClick()
        {
            _finish();
            _sync();
        }
    }

    public class ClickModalOrSync<T> : IClickable<T>
    {
        WindowsAppFriend _app;
        ClickModal<T>.ShowModal _show;
        ClickModal<T>.CreateWindowDriver _create;
        ClickSync.Sync _sync;
        public ClickModalOrSync(WindowsAppFriend app, ClickModal<T>.ShowModal show, ClickModal<T>.CreateWindowDriver create, ClickSync.Sync sync)
        {
            _app = app;
            _show = show;
            _create = create;
            _sync = sync;
        }

        public T EmulateClick()
        {
            var current = WindowControl.FromZTop(_app);
            var a = new Async();
            _show(a);
            var w = current.WaitForNextModal(a);
            if (w == null)
            {
                _sync();
                return default(T);
            }
            return _create(w, a);
        }
    }

    public static class Ex
    {
        public static IClickable<T> MakeModal<T>(this NativeButton button, Func<WindowControl, Async, T> create)
        {
            return new ClickModal<T>(button.App, button.EmulateClick, (w, a) => create(w, a));
        }
        public static IClickable<T> MakeModaleless<T>(this NativeButton button, Func<T> create)
        {
            return new ClickModeless<T>(button.EmulateClick, () => create());
        }
        public static IClickable MakeSync(this NativeButton button, Async async)
        {
            return new ClickSync(button.EmulateClick, async.WaitForCompletion);
        }
        public static IClickable<T> MakeModalOrSync<T>(this NativeButton button, Func<WindowControl, Async, T> create, Async async)
        {
            return new ClickModalOrSync<T>(button.App, button.EmulateClick, (w, a) => create(w, a), async.WaitForCompletion);
        }
    }
}
