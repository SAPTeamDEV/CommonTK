﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace SAPTeam.CommonTK
{
    public abstract partial class Context
    {
        [DllImport("kernel32.dll")] private static extern IntPtr GetConsoleWindow();

        static readonly Dictionary<string, Context> contexts = new Dictionary<string, Context>();
        static readonly Dictionary<string, List<Context>> groups = new Dictionary<string, List<Context>>();
        static readonly object lockObj = new object();
        private static InteractInterface interactinterface = GetConsoleWindow() != IntPtr.Zero ? InteractInterface.Console : InteractInterface.UI;

        /// <summary>
        /// Gets or Sets the process interaction Interface.
        /// <para>
        /// Property setter Action Group: global.interface
        /// </para>
        /// </summary>
        public static InteractInterface Interface
        {
            get => interactinterface;
            set
            {
                QueryGroup(ActionGroup(ActionScope.Global, "interface"));
                interactinterface = value;
            }
        }
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
            var context = new TContext();
            if (!context.IsGlobal)
            {
                context.Initialize(true);
            }

            return context;
        }

        /// <summary>
        /// Queries all provided action groups and checks the locked status of each group.
        /// If at least one of the specified action groups is locked, it throws a <see cref="ActionGroupException"/>.
        /// </summary>
        /// <param name="actionGroups">
        /// The name of action group or groups that will be queried.
        /// </param>
        /// <exception cref="ActionGroupException"></exception>
        public static void QueryGroup(params string[] actionGroups)
        {
            foreach (var group in actionGroups)
            {
                if (groups.ContainsKey(group) && groups[group].Count > 0)
                {
                    if (groups[group].Count == 1)
                    {
                        throw new ActionGroupException($"The action group \"{group}\" is locked by {groups[group][0].Name} context.");
                    }
                    else
                    {
                        throw new ActionGroupException($"The action group \"{group}\" is locked by {groups[group].Count} contexts.");
                    }
                }
            }
        }

        /// <summary>
        /// Generates a new action group.
        /// </summary>
        /// <param name="scope">
        /// The scope of action group.
        /// </param>
        /// <param name="identifier">
        /// The unique identifier of action group.
        /// </param>
        /// <returns>
        /// The standardized action group name.
        /// </returns>
        public static string ActionGroup(ActionScope scope, string identifier)
        {
            return string.Join(".", scope, identifier)
                .Replace(' ', '_')
                .ToLower();
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
            where TContext : Context
        {
            return contexts.ContainsKey(typeof(TContext).Name);
        }

        /// <summary>
        /// Checks that the current session has the specified type of context.
        /// </summary>
        /// <param name="contextType">
        /// A context class type.
        /// </param>
        /// <returns>
        /// returns <see langword="true"/> if the current session has an instance of <paramref name="contextType"/>. otherwise it return <see langword="false"/>.
        /// </returns>
        public static bool Exists(Type contextType)
        {
            return contexts.ContainsKey(contextType.Name);
        }

        /// <summary>
        /// Checks that the current session has the specified name of context.
        /// </summary>
        /// <param name="contextName">
        /// Name of a context class.
        /// </param>
        /// <returns>
        /// returns <see langword="true"/> if the current session has an instance of <paramref name="contextName"/>. otherwise it return <see langword="false"/>.
        /// </returns>
        public static bool Exists(string contextName)
        {
            return contexts.ContainsKey(contextName);
        }

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
}