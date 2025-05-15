using System;
using System.Threading;

namespace SAPTeam.CommonTK;

/// <summary>
/// A Timer that runs a parameterless method after specified time in another thread.
/// </summary>
public class Timer
{
    private int delay;
    private Action callback;
    private Thread thread;
    private bool repeat;
    private bool paused;

    /// <summary>
    /// Gets running state of this timer.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Creates new <see cref="Timer"/> and starts it.
    /// </summary>
    /// <param name="milliseconds">
    /// Timer delay in Milliseconds.
    /// </param>
    /// <param name="callback">
    /// A parameterless method or lambda expression that runs after passing <paramref name="milliseconds"/>.
    /// </param>
    /// <param name="repeat">
    /// Determines that <see cref="Timer"/> will be stopped after passing <paramref name="milliseconds"/> or running until it stopped.
    /// </param>
    /// <returns>
    /// An instance of <see cref="Timer"/> class.
    /// </returns>
    public static Timer Set(int milliseconds, Action callback, bool repeat = false)
    {
        Timer timer = new Timer()
        {
            delay = milliseconds,
            callback = callback,
            repeat = repeat,
            IsRunning = true
        };

        timer.thread = new Thread(timer.Run)
        {
            Name = $"Timer {timer.GetHashCode()} Thread"
        };

        timer.thread.Start();
        return timer;
    }

    private void Run()
    {
        while (true)
        {
            Thread.Sleep(delay);

            if (!IsRunning)
            {
                break;
            }

            if (!paused)
            {
                callback();
            }

            if (!repeat)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Resumes timer if it already paused.
    /// </summary>
    public void Resume()
    {
        if (!IsRunning)
        {
            throw new InvalidOperationException("Timer is not running.");
        }

        if (!paused)
        {
            throw new InvalidOperationException("Timer is not paused.");
        }

        paused = false;
    }

    /// <summary>
    /// Stops or Pauses the timer. Once a Timer has stopped, it can''t be restarted because the underlying <see cref="Thread"/> is died.
    /// </summary>
    /// <param name="pause">
    /// Determines that this timer will be temporary paused or stopped.
    /// </param>
    public void Stop(bool pause = false)
    {
        if (!IsRunning)
        {
            throw new InvalidOperationException("Timer is not running.");
        }

        if (pause)
        {
            if (!paused)
            {
                throw new InvalidOperationException("Timer is already paused.");
            }

            paused = true;
        }
        else
        {
            IsRunning = false;
        }
    }
}