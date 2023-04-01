using System.Collections.Generic;

namespace SAPTeam.CommonTK
{
    /// <summary>
    /// Provides methods for Register, Query and Manage the session Contexts.
    /// </summary>
    public class ContextContainer
    {
        private readonly List<IContext> contexts = new List<IContext>();

        /// <summary>
        /// Checks that the current session has the specified type of context.
        /// </summary>
        /// <typeparam name="Context">
        /// A class type that implements the <see cref="Context"/> as base class.
        /// </typeparam>
        /// <returns>
        /// returns <see langword="true"/> if the current session has an instance of <typeparamref name="Context"/>. otherwise it return <see langword="false"/>.
        /// </returns>
        public bool HasContext<Context>()
            where Context : CommonTK.Context
        {
            return GetContext<Context>() != null;
        }

        /// <summary>
        /// Registers a new Context.
        /// </summary>
        /// <param name="context">
        /// A Context object.
        /// </param>
        public void SetContect(IContext context)
        {
            contexts.Add(context);
        }

        /// <summary>
        /// Get the context object that maches with <typeparamref name="Context"/> type.
        /// </summary>
        /// <typeparam name="Context">
        /// A class type that implements the <see cref="Context"/> as base class.
        /// </typeparam>
        /// <returns>
        /// An existing instance of matching <typeparamref name="Context"/> type. if there is no matching contexts it returns <see href="default"/>.
        /// </returns>
        public Context GetContext<Context>()
            where Context : CommonTK.Context
        {
            foreach (var context in contexts)
            {
                if (context is Context obj)
                {
                    return obj;
                }
            }

            return default;
        }

        internal void DisposeContext(IContext context)
        {
            contexts.Remove(context);
        }
    }
}