﻿using System.Collections.Generic;

namespace SAPTeam.CommonTK
{
    public class ContextContainer
    {
        private readonly List<IContext> contexts = new List<IContext>();

        public bool HasContext<Context>()
        {
            return GetContext<Context>() != null;
        }

        public void SetContect(IContext context)
        {
            contexts.Add(context);
        }

        public Context GetContext<Context>()
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

        public void DisposeContext(IContext context)
        {
            contexts.Remove(context);
        }
    }
}