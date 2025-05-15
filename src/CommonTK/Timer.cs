// ----------------------------------------------------------------------------
//  <copyright file="Timer.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

namespace SAPTeam.CommonTK;

/// <summary>
/// Represents a timer that executes a callback after a specified delay.
/// </summary>
public sealed class Timer
{
    private readonly Action callback;
    private readonly Thread thread;

    /// <summary>
    /// Gets the delay in milliseconds.
    /// </summary>
    public int Delay { get; }

    /// <summary>
    /// Gets a value indicating whether the callback is executed indefinitely or just once.
    /// </summary>
    public bool Repeat { get; }

    /// <summary>
    /// Gets a value indicating whether the timer thread is running.
    /// </summary>
    public bool Alive { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the timer is running.
    /// </summary>
    public bool Paused { get; private set; }

    /// <summary>
    /// Gets the exceptions that occurred during the execution of the callback.
    /// </summary>
    public List<Exception> Exceptions { get; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Timer"/> class and starts the timer.
    /// </summary>
    /// <param name="milliseconds">
    /// The delay in milliseconds.
    /// </param>
    /// <param name="callback">
    /// The callback to be executed after the delay.
    /// </param>
    /// <param name="repeat">
    /// A value indicating whether the callback should be executed indefinitely or just once.
    /// </param>
    public Timer(int milliseconds, Action callback, bool repeat = false)
    {
        Delay = milliseconds;
        this.callback = callback;
        Repeat = repeat;

        thread = new Thread(Run)
        {
            Name = $"Timer {GetHashCode()} Thread"
        };

        thread.Start();
    }

    /// <summary>
    /// Runs the timer logic.
    /// </summary>
    private void Run()
    {
        Alive = true;

        while (true)
        {
            Thread.Sleep(Delay);

            if (!Alive)
            {
                break;
            }

            if (!Paused)
            {
                try
                {
                    callback();
                }
                catch (Exception ex)
                {
                    lock (Exceptions)
                    {
                        Exceptions.Add(ex);
                    }
                }
            }

            if (!Repeat)
            {
                break;
            }
        }

        Alive = false;
    }

    /// <summary>
    /// Pauses the timer if it is running.
    /// </summary>
    /// <remarks>
    /// Dead timers cannot be paused.
    /// </remarks>
    /// <exception cref="InvalidOperationException"></exception>
    public void Pause()
    {
        if (!Alive)
        {
            throw new InvalidOperationException("The timer thread is dead.");
        }

        Paused = true;
    }

    /// <summary>
    /// Resumes the timer if it is paused.
    /// </summary>
    /// <remarks>
    /// Dead timers cannot be resumed.
    /// </remarks>
    /// <exception cref="InvalidOperationException"></exception>
    public void Resume()
    {
        if (!Alive)
        {
            throw new InvalidOperationException("The timer thread is dead.");
        }

        Paused = false;
    }

    /// <summary>
    /// Stops the timer.
    /// </summary>
    public void Stop() => Alive = false;
}
