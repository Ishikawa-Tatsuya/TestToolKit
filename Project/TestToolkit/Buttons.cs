using Codeer.Friendly;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;
using Codeer.Friendly.Windows.NativeStandardControls;
using RM.Friendly.WPFStandardControls;
using System;

namespace TestToolkit
{
    public class InvokeModal<T>
    {
        AppVar _core;
        Action<Async> _invoke;
        Func<WindowControl, Async, T> _create;

        public InvokeModal(AppVar core, Action<Async> invoke, Func<WindowControl, Async, T> create)
        {
            _core = core;
            _invoke = invoke;
            _create = create;
        }

        public T Invoke()
        {
            var current = WindowControl.FromZTop((WindowsAppFriend)_core.App);
            var a = new Async();
            _invoke(a);
            var w = current.WaitForNextModal(a);
            return w == null ? default(T) : _create(w, a);
        }
    }

    public class InvokeModeless<T>
    {
        Action _invoke;
        Func<T> _find;

        public InvokeModeless(Action invoke, Func<T> find)
        {
            _invoke = invoke;
            _find = find;
        }

        public T Invoke()
        {
            _invoke();
            return _find();
        }
    }

    public class InvokeSync
    {
        Action _invoke;
        Action _sync;

        public InvokeSync(Action invoke, Action sync)
        {
            _invoke = invoke;
            _sync = sync;
        }

        public void Invoke()
        {
            _invoke();
            _sync();
        }
    }

    public class InvokeModalOrSync<T>
    {
        AppVar _core;
        Action<Async> _invoke;
        Func<WindowControl, Async, T> _create;
        Action _sync;
        public InvokeModalOrSync(AppVar core, Action<Async> invoke, Func<WindowControl, Async, T> create, Action sync)
        {
            _core = core;
            _invoke = invoke;
            _create = create;
            _sync = sync;
        }

        public InvokeModalOrSync(AppVar core, Action<Async> invoke, Func<WindowControl, Async, T> create,Async async) : 
            this(core, invoke, create, () => async.WaitForCompletion()) { }

        public T Invoke()
        {
            var current = WindowControl.FromZTop((WindowsAppFriend)_core.App);
            var a = new Async();
            _invoke(a);
            var w = current.WaitForNextModal(a);
            if (w == null)
            {
                _sync();
                return default(T);
            }
            return _create(w, a);
        }
    }

    public class InvokeModelessOrSync<T>
    {
        Action _invoke;
        Func<T> _find;
        Action _sync;
        public InvokeModelessOrSync(Action invoke, Func<T> find, Action sync)
        {
            _invoke = invoke;
            _find = find;
            _sync = sync;
        }

        public T Invoke()
        {
            _invoke();
            var w = _find();
            if (w == null)
            {
                _sync();
            }
            return w;
        }
    }

    public class WPFButtonModal<T> : InvokeModal<T>
    {
        public WPFButtonModal(AppVar core, Func<WindowControl, Async, T> create) :
            base(core, (a) => new WPFButtonBase(core).EmulateClick(a), create) { }
    }

    public class WPFButtonModeless<T> : InvokeModeless<T>
    {
        public WPFButtonModeless(AppVar core, Func<T> find) :
            base(() => new WPFButtonBase(core).EmulateClick(), find) { }
    }

    public class WPFButtonSync : InvokeSync
    {
        public WPFButtonSync(AppVar core, Action sync) :
            base(() => new WPFButtonBase(core).EmulateClick(), sync) { }

        public WPFButtonSync(AppVar core, Async async) :
            this(core, () => async.WaitForCompletion()) { }
    }

    public class WPFButtonModalOrClose<T> : InvokeModalOrSync<T>
    {
        public WPFButtonModalOrClose(AppVar core, Func<WindowControl, Async, T> create, Action sync) :
            base(core, (a) => new WPFButtonBase(core).EmulateClick(a), create, sync) { }

        public WPFButtonModalOrClose(AppVar core, Func<WindowControl, Async, T> create, Async async) :
            this(core, create, () => async.WaitForCompletion()) { }
    }

    public class WPFButtonModelessOrClose<T> : InvokeModelessOrSync<T>
    {
        public WPFButtonModelessOrClose(AppVar core, Func<T> find, Action sync) :
            base(() => new WPFButtonBase(core).EmulateClick(), find, sync) { }

        public WPFButtonModelessOrClose(AppVar core, Func<T> find, Async saync) :
            this(core, find, () => saync.WaitForCompletion()) { }
    }

    public class MessageBoxDriver
    {
        NativeMessageBox _core;

        public class Button : InvokeSync
        {
            public Button(NativeMessageBox core, string text, Action sync)
                : base(() => core.EmulateButtonClick(text), sync) { }
        }

        public Button Button_OK { get; private set; }

        public Button Button_Cancel { get; private set; }

        public string Message { get { return _core.Message; } }

        public string Title { get { return _core.Title; } }

        public MessageBoxDriver(WindowControl w, Action sync)
        {
            _core = new NativeMessageBox(w);
            Button_OK = new Button(_core, "OK", sync);
            Button_Cancel = new Button(_core, "キャンセル", sync);
        }

        public MessageBoxDriver(WindowControl w, Async async) : this(w, () => async.WaitForCompletion()) { }
    }
}
