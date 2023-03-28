using System;

using SAPTeam.CommonTK.Console.ConsoleForm.Controls;

namespace SAPTeam.CommonTK.Console.ConsoleForm
{
    public partial class Interface
    {
        public event Action Title;
        public event Action<ConsoleOption> OnEnter;
        public event Action OnStart;
        public event Action OnClose;
        public event Action<ConsoleKeyInfo> KeyPressed;

        public void ClearEvents()
        {
            Title = null;
            OnEnter = null;
            OnStart = null;
            OnClose = null;
            KeyPressed = null;
        }

        private void RaiseStart()
        {
            Action start = OnStart;
            start?.Invoke();
        }

        private void RaiseClose()
        {
            Action close = OnClose;
            close?.Invoke();
        }

        private void RaiseTitle()
        {
            Action title = Title;
            title?.Invoke();
        }

        private void RaiseKeyPressed(ConsoleKeyInfo key)
        {
            Action<ConsoleKeyInfo> keyPressed = KeyPressed;
            keyPressed?.Invoke(key);
        }

        private void RaiseEnter(ConsoleOption option)
        {
            Action<ConsoleOption> enter = OnEnter;
            enter?.Invoke(option);
        }
    }
}