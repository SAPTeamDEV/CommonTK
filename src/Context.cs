using System;
using System.Collections.Generic;
using System.Linq;

using SAPTeam.CommonTK.ExecutionPolicy;

namespace SAPTeam.CommonTK;

/// <summary>
/// Defines a disposable Context that can be used to set some properties or do particular actions in a specified <see cref="Context"/>.
/// </summary>
public abstract partial class Context : IDisposable
{
    private bool disposing;
    private bool disposed;

    private string[] allowedGroups = [];
    private string[] ownedGroups = [];

    private readonly List<ActionGroupContainer> affectedGroups = [];

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
    /// <para>
    /// Method Action Groups are declared in the <see cref="DefaultGroups"/> property.
    /// </para>
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

            QueryGroup(DefaultGroups);

            allowedGroups = Groups.Concat(NeutralGroups).ToArray();
            ownedGroups = Groups.Concat(DefaultGroups).ToArray();

            foreach (string group in ownedGroups)
            {
                if (groups.ContainsKey(group) && groups[group].IsSuppressed)
                {
                    throw new ActionGroupException($"The action group \"{group}\" is suppressed.", ActionGroupError.Suppressed);
                }
            }

            lock (contextLockObj)
            {
                contexts[Name] = this;
            }
        }

        try
        {
            if (!IsRunning)
            {
                IsRunning = true;
                CreateContext();
            }

            if (IsGlobal)
            {
                foreach (string group in ownedGroups)
                {
                    RegisterAction(group, false, true);
                }
            }
        }
        catch (ActionGroupException age)
        {
            if (age.ErrorCode != (int)ActionGroupError.AlreadyLocked)
            {
                throw;
            }
        }
        catch (Exception)
        {
            FreeGroups();
            KnockUp();
            throw;
        }
    }

    private void RegisterAction(string group, bool doRelock, bool addContext)
    {
        if (!groups.ContainsKey(group))
        {
            groups[group] = new ActionGroupContainer(group);
        }

        if (doRelock && groups[group].IsSuppressor(this))
        {
            groups[group].Relock(this);

            if (affectedGroups.Contains(groups[group]))
            {
                affectedGroups.Remove(groups[group]);
            }
        }
        else if (addContext)
        {
            groups[group].Add(this);

            if (!affectedGroups.Contains(groups[group]))
            {
                affectedGroups.Add(groups[group]);
            }
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
        if (!IsGlobal) throw new ActionGroupException(ActionGroupError.NotGlobal);
        if (disposing) throw new ActionGroupException(ActionGroupError.Disposing);

        if (allowedGroups.Contains(group))
        {
            RegisterAction(group, true, true);
        }
        else
        {
            throw new ActionGroupException($"The action group operations for the \"{group}\" is not permitted.", ActionGroupError.AccessDenied);
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
        if (!IsGlobal) throw new ActionGroupException(ActionGroupError.NotGlobal);
        if (disposing) throw new ActionGroupException(ActionGroupError.Disposing);

        if (allowedGroups.Contains(group))
        {
            RegisterAction(group, false, false);
            groups[group].Suppress(this);

            if (!affectedGroups.Contains(groups[group]))
            {
                affectedGroups.Add(groups[group]);
            }
        }
        else
        {
            throw new ActionGroupException($"The action group operations for \"{group}\" is not permitted.", ActionGroupError.AccessDenied);
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

            FreeGroups();

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

    private void FreeGroups()
    {
        foreach (ActionGroupContainer actionGroup in affectedGroups)
        {
            actionGroup.Remove(this);
        }

        affectedGroups.Clear();
    }

    private void KnockUp()
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