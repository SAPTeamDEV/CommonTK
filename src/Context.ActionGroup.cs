using System.Collections.Generic;
using System.Linq;

using SAPTeam.CommonTK.ExecutionPolicy;

namespace SAPTeam.CommonTK;

// Action Groups is an integrated feature in Context.
public abstract partial class Context
{
    private static readonly Dictionary<string, ActionGroupContainer> groups = [];

    /// <summary>
    /// Queries all provided action groups and checks the locked state of each group.
    /// If at least one of the specified action groups is locked, it throws a <see cref="ActionGroupException"/>.
    /// </summary>
    /// <param name="groups">
    /// The name of action group or groups that will be queried.
    /// </param>
    /// <exception cref="ActionGroupException"></exception>
    public static void QueryGroup(params string[] groups)
    {
        foreach (string group in groups)
        {
            if (Context.groups.ContainsKey(group) && Context.groups[group].IsLocked)
            {
                if (Context.groups[group].Count == 1)
                {
                    throw new ActionGroupException($"The action group \"{group}\" is locked by {Context.groups[group][0].Name} context.", ActionGroupError.Locked);
                }
                else
                {
                    throw new ActionGroupException($"The action group \"{group}\" is locked by {Context.groups[group].Count} contexts.", ActionGroupError.Locked);
                }
            }
        }
    }

    /// <summary>
    /// Gets the state of the <paramref name="group"/>.
    /// </summary>
    /// <param name="group">
    /// The name of the action group.
    /// </param>
    /// <returns>
    /// A <see cref="ActionGroupState"/> represents the current state of the <paramref name="group"/>.
    /// </returns>
    public static ActionGroupState QueryGroupState(string group) => groups.ContainsKey(group) ? groups[group].State : ActionGroupState.Free;

    /// <summary>
    /// Generates a new action group.
    /// </summary>
    /// <param name="scope">
    /// The scope of action group.
    /// </param>
    /// <param name="identifier">
    /// The unique identifier of action group.
    /// </param>
    /// <param name="extras">
    /// The extra identifier factors.
    /// </param>
    /// <returns>
    /// The standardized action group name.
    /// </returns>
    public static string ActionGroup(ActionScope scope, string identifier, params string[] extras)
    {
        string groupName = string.Join(".", new string[] { scope.ToString(), identifier }.Concat(extras))
            .Replace(' ', '_')
            .ToLower();

        return groupName;
    }
}
