using System;
using System.Collections.Generic;

namespace SAPTeam.CommonTK
{
    /// <summary>
    /// Provides methods for Register, Query and Manage the session Contexts.
    /// </summary>
    public class ContextContainer
    {
        private readonly Dictionary<string, IContext> contexts = new Dictionary<string, IContext>();
        object lockObj = new object();

        /// <summary>
        /// Checks that the current session has the specified type of context.
        /// </summary>
        /// <typeparam name="Context">
        /// A class type that implements the <see cref="IContext"/>.
        /// </typeparam>
        /// <returns>
        /// returns <see langword="true"/> if the current session has an instance of <typeparamref name="Context"/>. otherwise it return <see langword="false"/>.
        /// </returns>
        public bool HasContext<Context>()
            where Context : IContext
        {
            return GetContext<Context>() != null;
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
        public void SetContext(IContext context)
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
        /// <typeparam name="Context">
        /// A class type that implements the <see cref="IContext"/>.
        /// </typeparam>
        /// <returns>
        /// A new instance of <typeparamref name="Context"/>.
        /// </returns>
        public Context SetContext<Context>()
            where Context : IContext, new()
        {
            return new Context();
        }

        /// <summary>
        /// Gets the context object that matches with <typeparamref name="Context"/> name.
        /// </summary>
        /// <typeparam name="Context">
        /// A class type that implements the <see cref="IContext"/>.
        /// </typeparam>
        /// <returns>
        /// An existing instance of matching <typeparamref name="Context"/> type. if there is no matching contexts it returns <see href="default"/>.
        /// </returns>
        public Context GetContext<Context>()
            where Context : IContext
        {
            if (contexts.ContainsKey(typeof(Context).Name))
            {
                return (Context)contexts[typeof(Context).Name];
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
        public IContext GetContext(Type contextType)
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
        public IContext GetContext(string contextName)
        {
            if (contexts.ContainsKey(contextName))
            {
                return contexts[contextName];
            }

            return default;
        }

        internal void DisposeContext(IContext context)
        {
            lock (lockObj)
            {
                contexts.Remove(context.GetType().Name);
            }
        }
    }
}