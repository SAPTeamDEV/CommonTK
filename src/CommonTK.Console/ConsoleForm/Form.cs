using System;
using System.Collections.Generic;

using SAPTeam.CommonTK.Console.ConsoleForm.Controls;

namespace SAPTeam.CommonTK.Console.ConsoleForm
{
    public partial class Form
    {
        public Interface Platform { get; set; }

        public Dictionary<string, Dictionary<string, string>> ExecutableItems { get; } = new Dictionary<string, Dictionary<string, string>>();
        public Dictionary<string, List<string>> Items { get; } = new Dictionary<string, List<string>>();

        public Dictionary<int, IControl> Container { get; } = new Dictionary<int, IControl>();

        public int First { get; set; }
        public int Last { get; set; }
        public int Current { get; set; }

        public virtual bool FocusToTop => false;
        public virtual bool IsClosable => true;
        public virtual bool UseDisposableWriter => false;

        public Form(bool rootForm = true)
        {
            CreateItems();

            if (rootForm)
            {
                Platform = new Interface(this);
                SetRootProperties();
                SetReceivers();
                CreateObjects();
            }

            SetProperties();
        }

        protected virtual void SetProperties() { }
        protected virtual void SetRootProperties() { }

        public virtual void SetReceivers()
        {
            Platform.ClearEvents();
            Platform.Title += OnTitle;
            Platform.OnClose += OnClose;
            Platform.OnStart += OnStart;
            Platform.KeyPressed += OnKeyPressed;
            Platform.OnEnter += OnEnter;
        }

        protected virtual void CreateItems() { }

        public void Start()
        {
            Platform.Start();
        }

        public Action Title => OnTitle;

        protected virtual void OnTitle() { }
        protected virtual void OnClose() { }
        protected virtual void OnStart() { }
        protected virtual void OnKeyPressed(ConsoleKeyInfo keyInfo) { }
        protected virtual void OnEnter(ConsoleOption option) { }
    }
}