using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SAPTeam.CommonTK
{
    /// <summary>
    /// Defines a disposable Context that can be used to set some properties or do particular actions in a specified <see cref="Context"/>.
    /// </summary>
    public abstract partial class Context : IDisposable
    {
        /// <summary>
        /// Gets the context default action groups.
        /// This action groups applied and locked automatically.
        /// </summary>
        public string[] DefaultGroups => new string[]
        {
            ActionGroup(ActionScope.Application, "context", Name)
        };

        /// <summary>
        /// Gets the name identifier of this context.
        /// </summary>
        public string Name => GetType().Name;

        /// <summary>
        /// Gets a value indicating whether this context is globally registered.
        /// </summary>
        public bool IsGlobal => contexts.ContainsValue(this);

        /// <summary>
        /// Gets a value indicating whether this context is running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets the action groups associated with this context.
        /// This action groups will be locked immediately after calling the <see cref="CreateContext()"/>.
        /// </summary>
        public abstract string[] Groups { get; }

        /// <summary>
        /// Gets the neutral action groups that this context need to access them.
        /// This action groups won't be automatically locked, but can be locked or temporarily unlocked by this context.
        /// </summary>
        public virtual string[] NeutralGroups => new string[0];

        /// <summary>
        /// Initializes a new context.
        /// This method must be called in the end of context constructor.
        /// </summary>
        /// <param name="global">
        /// Determines whether to register this context globally.
        /// </param>
        /// <exception cref="InvalidOperationException"></exception>
        protected void Initialize(bool global)
        {
            if (global && !IsGlobal)
            {
                if (Exists(Name))
                {
                    throw new InvalidOperationException("An instance of this context already exists");
                }

                lock (lockObj)
                {
                    contexts[Name] = this;
                }
            }

            if (!IsRunning)
            {
                IsRunning = true;
                CreateContext();

                if (IsGlobal)
                {
                    foreach (string group in Groups.Concat(DefaultGroups))
                    {
                        RegisterAction(group);
                    }
                }
            }
        }

        void RegisterAction(string group)
        {
            if (groups.ContainsKey(group))
            {
                groups[group].Add(this);
            }
            else
            {
                groups[group] = new ActionGroupContainer(group) { this };
            }
        }

        /// <summary>
        /// Locks an action group.
        /// The action group must be already declared in <see cref="Groups"/> or <see cref="NeutralGroups"/> properties.
        /// </summary>
        /// <param name="group">
        /// The action group name.
        /// </param>
        /// <exception cref="ActionGroupException"></exception>
        protected void LockGroup(string group)
        {
            if (Groups.Concat(NeutralGroups).Contains(group))
            {
                RegisterAction(group);
            }
            else
            {
                throw new ActionGroupException($"The action group operations for \"{group}\" is not permitted.");
            }
        }

        /// <summary>
        /// Suppresses the lock state of an action group.
        /// The action group must be already declared in <see cref="Groups"/> or <see cref="NeutralGroups"/> properties.
        /// </summary>
        /// <param name="group">
        /// The action group name.
        /// </param>
        /// <exception cref="ActionGroupException"></exception>
        protected void SuppressLock(string group)
        {
            if (Groups.Concat(NeutralGroups).Contains(group))
            {
                var container = groups[group];
                container.IsSuppressed = true;
            }
            else
            {
                throw new ActionGroupException($"The action group operations for \"{group}\" is not permitted.");
            }
        }

        /// <summary>
        /// This method called after the constructor has successfully registers the Context.
        /// </summary>
        protected abstract void CreateContext();

        /// <summary>
        /// This method called when the context is disposing.
        /// </summary>
        protected abstract void DisposeContext();

        /// <inheritdoc/>
        public void Dispose()
        {
            if (IsRunning)
            {
                if (IsGlobal)
                {
                    foreach (string group in Groups.Concat(DefaultGroups).Concat(NeutralGroups))
                    {
                        if (groups.ContainsKey(group))
                        {
                            groups[group].Remove(this);
                        }
                    }
                }

                DisposeContext();
                IsRunning = false;
            }

            if (IsGlobal)
            {
                lock (lockObj)
                {
                    contexts.Remove(Name);
                }
            }
        }
    }
}