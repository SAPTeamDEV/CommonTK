using System;
using System.Collections.Generic;

namespace SAPTeam.CommonTK
{
    /// <summary>
    /// Provides methods for Register, Query and Manage the session Contexts.
    /// </summary>
    public class ContextContainer
    {
        private readonly Dictionary<string, Context> contexts = new Dictionary<string, Context>();
        object lockObj = new object();

        /// <summary>
        /// Checks that the current session has the specified type of context.
        /// </summary>
        /// <typeparam name="TContext">
        /// A class type that implements the <see cref="Context"/> as base class.
        /// </typeparam>
        /// <returns>
        /// returns <see langword="true"/> if the current session has an instance of <typeparamref name="TContext"/>. otherwise it return <see langword="false"/>.
        /// </returns>
        public bool HasContext<TContext>()
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
        public bool HasContext(Type contextType)
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
        public bool HasContext(string contextName)
        {
            return GetContext(contextName) != null;
        }

        /// <summary>
        /// Registers a new Context.
        /// </summary>
        /// <param name="context">
        /// A Context object.
        /// </param>
        public void SetContext(Context context)
        {
            if (contexts.ContainsKey(context.GetType().Name))
            {
                throw new InvalidOperationException("An instance of this context already exist");
            }

            lock (lockObj)
            {
                contexts[context.GetType().Name] = context;
            }
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
        public TContext SetContext<TContext>()
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
        public TContext GetContext<TContext>()
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
        public Context GetContext(Type contextType)
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
        public Context GetContext(string contextName)
        {
            if (contexts.ContainsKey(contextName))
            {
                return contexts[contextName];
            }

            return default;
        }

        internal void DisposeContext(Context context)
        {
            lock (lockObj)
            {
                contexts.Remove(context.GetType().Name);
            }
        }
    }
}