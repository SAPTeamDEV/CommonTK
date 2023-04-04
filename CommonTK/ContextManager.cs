using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SAPTeam.CommonTK
{
    public abstract partial class Context
    {
        [DllImport("kernel32.dll")] private static extern IntPtr GetConsoleWindow();

        static readonly Dictionary<string, Context> contexts = new Dictionary<string, Context>();
        static readonly object lockObj = new object();

        /// <summary>
        /// Gets or Sets the process interaction Interface.
        /// </summary>
        public static InteractInterface Interface { get; set; } = GetConsoleWindow() != IntPtr.Zero ? InteractInterface.Console : InteractInterface.UI;

        /// <summary>
        /// Checks that the current session has the specified type of context.
        /// </summary>
        /// <typeparam name="TContext">
        /// A class type that implements the <see cref="Context"/> as base class.
        /// </typeparam>
        /// <returns>
        /// returns <see langword="true"/> if the current session has an instance of <typeparamref name="TContext"/>. otherwise it return <see langword="false"/>.
        /// </returns>
        public static bool HasContext<TContext>()
            where TContext : Context
        {
            return GetContext<TContext>() != null;
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
        public static bool HasContext(Type contextType)
        {
            return GetContext(contextType) != null;
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
        public static bool HasContext(string contextName)
        {
            return GetContext(contextName) != null;
        }

        /// <summary>
        /// Creates a new Context and registers it.
        /// </summary>
        /// <typeparam name="TContext">
        /// A class type that implements the <see cref="Context"/> as base class.
        /// </typeparam>
        /// <returns>
        /// A new instance of <typeparamref name="TContext"/>.
        /// </returns>
        public static TContext SetContext<TContext>()
            where TContext : Context, new()
        {
            return new TContext();
        }

        /// <summary>
        /// Gets the context object that matches with <typeparamref name="TContext"/> name.
        /// </summary>
        /// <typeparam name="TContext">
        /// A class type that implements the <see cref="Context"/> as base class.
        /// </typeparam>
        /// <returns>
        /// An existing instance of matching <typeparamref name="TContext"/> type. if there is no matching contexts it returns <see href="default"/>.
        /// </returns>
        public static TContext GetContext<TContext>()
            where TContext : Context
        {
            if (contexts.ContainsKey(typeof(TContext).Name))
            {
                return (TContext)contexts[typeof(TContext).Name];
            }

            return default;
        }

        /// <summary>
        /// Gets the context object that matches with <paramref name="contextType"/> type name.
        /// </summary>
        /// <param name="contextType">
        /// A context class type.
        /// </param>
        /// <returns>
        /// An existing instance of matching <paramref name="contextType"/> type. if there is no matching contexts it returns <see href="default"/>.
        /// </returns>
        public static Context GetContext(Type contextType)
        {
            if (contexts.ContainsKey(contextType.Name))
            {
                return contexts[contextType.Name];
            }

            return default;
        }

        /// <summary>
        /// Gets the context object that matches with <paramref name="contextName"/> key.
        /// </summary>
        /// <param name="contextName">
        /// Name of a context class.
        /// </param>
        /// <returns>
        /// An existing instance of matching <paramref name="contextName"/> type name. if there is no matching contexts it returns <see href="default"/>.
        /// </returns>
        public static Context GetContext(string contextName)
        {
            if (contexts.ContainsKey(contextName))
            {
                return contexts[contextName];
            }

            return default;
        }
    }
}