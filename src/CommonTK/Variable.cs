// ----------------------------------------------------------------------------
//  <copyright file="Variable.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

using SAPTeam.CommonTK.ExecutionPolicy;

using static SAPTeam.CommonTK.Context;

namespace SAPTeam.CommonTK;

/// <summary>
/// Provides methods for interacting with environment variables.
/// </summary>
public class Variable
{
    private string? cachedValue;

    /// <summary>
    /// Gets the name of the variable.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the cached value or Sets the new value of the variable.
    /// <para>
    /// Property setter Action Group: global.variable.<see href="Target"/>.<see href="Name"/>
    /// </para>
    /// </summary>
    public virtual string? Value
    {
        get => cachedValue;
        set
        {
            SetVariable(Name, value, Target);
            cachedValue = value;
        }
    }

    /// <summary>
    /// Gets the target location of the variable.
    /// </summary>
    public EnvironmentVariableTarget Target { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Variable"/>.
    /// </summary>
    /// <param name="name">
    /// The name of the environment variable.
    /// </param>
    /// <param name="acceptNonExistVariables">
    /// Determines whether this instance should accept a non-existence variable.
    /// </param>
    /// <exception cref="KeyNotFoundException"></exception>
    public Variable(string name, bool acceptNonExistVariables = true)
    {
        Name = name;

        if (!acceptNonExistVariables)
        {
            Exists(name, true);
        }

        cachedValue = GetVariable(name);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Variable"/>.
    /// </summary>
    /// <param name="name">
    /// The name of the environment variable.
    /// </param>
    /// <param name="target">
    /// Determines the target location of the variable.
    /// </param>
    /// <param name="acceptNonExistVariables">
    /// Determines whether this instance should accept a non-existence variable.
    /// </param>
    /// <exception cref="KeyNotFoundException"></exception>
    public Variable(string name, EnvironmentVariableTarget target, bool acceptNonExistVariables = true) : this(name, acceptNonExistVariables) => Target = target;

    /// <inheritdoc/>
    public override string ToString() => Value ?? string.Empty;

    /// <summary>
    /// Generates a variable action group name.
    /// </summary>
    /// <param name="name">
    /// The name of the variable.
    /// </param>
    /// <param name="target">
    /// The target location of the variable.
    /// </param>
    /// <returns>
    /// The standardized variable group name. The standard pattern is: global.variable.<paramref name="target"/>.<paramref name="name"/>
    /// </returns>
    public static string VariableActionGroup(string name, EnvironmentVariableTarget target) => ActionGroup(ActionScope.Global, "variable", target.ToString(), name);

    /// <summary>
    /// Gets the value of the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">
    /// The name of the environment variable.
    /// </param>
    /// <returns>
    /// The value of the <paramref name="name"/>.
    /// </returns>
    public static string? GetVariable(string name) => Environment.GetEnvironmentVariable(name);

    /// <summary>
    /// Sets the value of the given <paramref name="name"/>.
    /// <para>
    /// Method Action Group: global.variable.<paramref name="target"/>.<paramref name="name"/>
    /// </para>
    /// </summary>
    /// <param name="name">
    /// The name of the environment variable.
    /// </param>
    /// <param name="value">
    /// The new value of the variable.
    /// </param>
    /// <param name="target">
    /// Determines the target location of the variable.
    /// </param>
    public static void SetVariable(string name, string? value, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
    {
        QueryGroup(VariableActionGroup(name, target));
        Environment.SetEnvironmentVariable(name, value, target);
    }

    /// <summary>
    /// Sets the value of the given <paramref name="name"/> at the user variable store.
    /// <para>
    /// Method Action Group: global.variable.user.<paramref name="name"/>
    /// </para>
    /// </summary>
    /// <param name="name">
    /// The name of the environment variable.
    /// </param>
    /// <param name="value">
    /// The new value of the variable.
    /// </param>
    public static void SetUserVariable(string name, string value) => SetVariable(name, value, EnvironmentVariableTarget.User);

    /// <summary>
    /// Sets the value of the given <paramref name="name"/> at the system variable store.
    /// <para>
    /// Method Action Group: global.variable.machine.<paramref name="name"/>
    /// </para>
    /// </summary>
    /// <param name="name">
    /// The name of the environment variable.
    /// </param>
    /// <param name="value">
    /// The new value of the variable.
    /// </param>
    public static void SetSystemVariable(string name, string value) => SetVariable(name, value, EnvironmentVariableTarget.Machine);

    /// <summary>
    /// Checks whether the <paramref name="name"/> has exists.
    /// </summary>
    /// <param name="name">
    /// The name of the environment variable.
    /// </param>
    /// <param name="throwException">
    /// Determines whether this method will throw an exception when the variable is not found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="name"/> has exists, otherwise it returns <see langword="false"/>.
    /// </returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public static bool Exists(string name, bool throwException = false)
    {
        var value = GetVariable(name);

        return value == null && throwException ? throw new KeyNotFoundException($"The variable {name} is not found.") : value != null;
    }
}
