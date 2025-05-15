// ----------------------------------------------------------------------------
//  <copyright file="Context.Manage.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

namespace SAPTeam.CommonTK;

public abstract partial class Context
{
    private static readonly Dictionary<string, Context> contexts = [];
    private static readonly object contextLockObj = new();

    /// <summary>
    /// Creates a new Context and registers it globally.
    /// </summary>
    /// <typeparam name="TContext">
    /// A class type that implements the <see cref="Context"/> as base class.
    /// </typeparam>
    /// <returns>
    /// A new instance of <typeparamref name="TContext"/>.
    /// </returns>
    public static TContext Register<TContext>()
        where TContext : Context, new()
    {
        TContext context = new();
        if (!context.IsGlobal)
        {
            context.Initialize(true);
        }

        return context;
    }

    /// <summary>
    /// Checks that the current session has the specified type of context.
    /// </summary>
    /// <typeparam name="TContext">
    /// A class type that implements the <see cref="Context"/> as base class.
    /// </typeparam>
    /// <returns>
    /// returns <see langword="true"/> if the current session has an instance of <typeparamref name="TContext"/>. otherwise it return <see langword="false"/>.
    /// </returns>
    public static bool Exists<TContext>()
        where TContext : Context => contexts.ContainsKey(typeof(TContext).Name);

    /// <summary>
    /// Checks that the current session has the specified type of context.
    /// </summary>
    /// <param name="contextType">
    /// A context class type.
    /// </param>
    /// <returns>
    /// returns <see langword="true"/> if the current session has an instance of <paramref name="contextType"/>. otherwise it return <see langword="false"/>.
    /// </returns>
    public static bool Exists(Type contextType) => contexts.ContainsKey(contextType.Name);

    /// <summary>
    /// Checks that the current session has the specified name of context.
    /// </summary>
    /// <param name="contextName">
    /// Name of a context class.
    /// </param>
    /// <returns>
    /// returns <see langword="true"/> if the current session has an instance of <paramref name="contextName"/>. otherwise it return <see langword="false"/>.
    /// </returns>
    public static bool Exists(string contextName) => contexts.ContainsKey(contextName);

    /// <summary>
    /// Gets the context object that matches with <typeparamref name="TContext"/> name.
    /// </summary>
    /// <typeparam name="TContext">
    /// A class type that implements the <see cref="Context"/> as base class.
    /// </typeparam>
    /// <returns>
    /// An existing instance of matching <typeparamref name="TContext"/> type. if there is no matching contexts it throws an <see cref="KeyNotFoundException"/>.
    /// </returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public static TContext GetContext<TContext>()
        where TContext : Context => (TContext)contexts[typeof(TContext).Name];

    /// <summary>
    /// Gets the context object that matches with <paramref name="contextType"/> type name.
    /// </summary>
    /// <param name="contextType">
    /// A context class type.
    /// </param>
    /// <returns>
    /// An existing instance of matching <paramref name="contextType"/> type. if there is no matching contexts it throws an <see cref="KeyNotFoundException"/>.
    /// </returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public static Context GetContext(Type contextType) => contexts[contextType.Name];

    /// <summary>
    /// Gets the context object that matches with <paramref name="contextName"/> key.
    /// </summary>
    /// <param name="contextName">
    /// Name of a context class.
    /// </param>
    /// <returns>
    /// An existing instance of matching <paramref name="contextName"/> type. if there is no matching contexts it throws an <see cref="KeyNotFoundException"/>.
    /// </returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public static Context GetContext(string contextName) => contexts[contextName];
}