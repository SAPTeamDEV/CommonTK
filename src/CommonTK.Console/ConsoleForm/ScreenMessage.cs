using SAPTeam.CommonTK.Console;
using SAPTeam.CommonTK.Console.ConsoleForm;

using Timer = SAPTeam.CommonTK.Timer;

namespace WindowsPro.ConsoleForm;

public partial class Interface
{
    public void ScreenMessage(string message, int msec = 3000)
    {
        ScreenMessage(message, msec, ConsoleCoords.ScreenMessage.ResolveX() == Index ? ColorSet.InvertedScreenMesage : ColorSet.ScreenMesage, ConsoleCoords.ScreenMessage);
    }

    public void ScreenMessage(string message, int msec, ColorSet colors, ConsoleCoords coordinates)
    {
        if (coordinates.Center)
        {
            coordinates.Y = GetCenterPosition(message.Length);
        }
        coordinates.Focus();
        colors.ChangeColor();
        Echo(message, false);
        ResetColor();
        Timer.Set(msec, onEnd);

        void onEnd()
        {
            coordinates.Focus();
            ResetColor();
            ClearLine(true);
            int actX = coordinates.ResolveX();
            if (actX == Index && activeForm.Container[Index] is ISelectableControl opClass)
            {
                opClass.Select();
            }
            else if (activeForm.Container.ContainsKey(actX))
            {
                activeForm.Container[actX].Write();
            }
        }
    }
}
