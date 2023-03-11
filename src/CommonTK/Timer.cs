namespace SAPTeam.CommonTK;

public class Timer
{
    private int delay;
    private Action? callback;
    private Thread? thread;
    private bool running;

    public static Timer Set(int msec, Action callback)
    {
        Timer timer = new()
        {
            delay = msec,
            callback = callback,
            running = true
        };
        timer.thread = new Thread(timer.Run)
        {
            Name = "Timer Thread"
        };
        timer.thread.Start();
        return timer;
    }

    private void Run()
    {
        Thread.Sleep(delay);
        if (!running) return;
        callback();
    }

    public void Stop()
    {
        running = false;
    }
}
