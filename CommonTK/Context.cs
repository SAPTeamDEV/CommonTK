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
        bool disposing;
        bool disposed;

        string[] allowedGroups;
        string[] ownedGroups;
        string[] contextGroups;

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
        public virtual string[] Groups { get; } = new string[0];

        /// <summary>
        /// Gets the neutral action groups that this context need to access them.
        /// This action groups won't be automatically locked, but can be locked or temporarily unlocked by this context.
        /// </summary>
        public virtual string[] NeutralGroups { get; } = new string[0];

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
            disposing = false;
            disposed = false;

            if (global && !IsGlobal)
            {
                if (Exists(Name))
                {
                    throw new InvalidOperationException("An instance of this context already exists");
                }

                allowedGroups = Groups.Concat(NeutralGroups).ToArray();
                ownedGroups = Groups.Concat(DefaultGroups).ToArray();
                contextGroups = ownedGroups.Concat(NeutralGroups).ToArray();

                foreach (var group in ownedGroups)
                {
                    if (groups.ContainsKey(group) && groups[group].IsSuppressed)
                    {
                        throw new ActionGroupException($"The action group \"{group}\" is suppressed.");
                    }
                }

                lock (contextLockObj)
                {
                    contexts[Name] = this;
                }
            }

            if (!IsRunning)
            {
                IsRunning = true;

                try
                {
                    CreateContext();
                }
                catch (Exception)
                {
                    KnockUp();
                    throw;
                }
            }

            if (IsGlobal)
            {
                foreach (string group in ownedGroups)
                {
                    RegisterAction(group, false, true);
                }
            }
        }

        void RegisterAction(string group, bool doRelock, bool addContext)
        {
            if (!groups.ContainsKey(group))
            {
                groups[group] = new ActionGroupContainer(group);
            }

            if (doRelock && groups[group].IsSuppressor(this))
            {
                groups[group].Relock(this);
            }
            else if (addContext)
            {
                groups[group].Add(this);
            }
        }

        /// <summary>
        /// Locks an action group. if the <paramref name="group"/> has already suppressed by this context, it will be relocked.
        /// The action group must be already declared in <see cref="Groups"/> or <see cref="NeutralGroups"/> properties.
        /// </summary>
        /// <param name="group">
        /// The action group name.
        /// </param>
        /// <exception cref="ActionGroupException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        protected void LockGroup(string group)
        {
            if (!IsGlobal) throw new InvalidOperationException("The action group feature only available in global contexts.");
            if (disposing) throw new ActionGroupException("A disposing context can't interact with action groups.");

            if (allowedGroups.Contains(group))
            {
                RegisterAction(group, true, true);
            }
            else
            {
                throw new ActionGroupException($"The action group operations for \"{group}\" is not permitted.");
            }
        }

        /// <summary>
        /// Suppresses the lock state of an action group. The suppression state automatically removed in the finalizer.
        /// The action group must be already declared in <see cref="Groups"/> or <see cref="NeutralGroups"/> properties.
        /// </summary>
        /// <param name="group">
        /// The action group name.
        /// </param>
        /// <exception cref="ActionGroupException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        protected void SuppressLock(string group)
        {
            if (!IsGlobal) throw new InvalidOperationException("The action group feature only available in global contexts.");
            if (disposing) throw new ActionGroupException("A disposing context can't interact with action groups.");

            if (allowedGroups.Contains(group))
            {
                RegisterAction(group, false, false);

                lock (groupLockObj)
                {
                    groups[group].Suppress(this);
                }
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
            if (!disposed)
            {
                disposing = true;

                if (IsGlobal)
                {
                    foreach (string group in contextGroups)
                    {
                        if (groups.ContainsKey(group) && groups[group].HasRegistered(this))
                        {
                            groups[group].Remove(this);
                        }
                    }
                }

                try
                {
                    if (IsRunning)
                    {
                        DisposeContext();
                    }
                }
                finally
                {
                    KnockUp();
                }
            }
        }

        void KnockUp()
        {
            if (IsGlobal)
            {
                lock (contextLockObj)
                {
                    contexts.Remove(Name);
                }
            }

            IsRunning = false;
            disposed = true;
        }
    }
}