namespace SAPTeam.CommonTK.Console.ConsoleForm
{
    public partial class Interface
    {
        public static ConsoleCoords ScreenMessageCoords => new ConsoleCoords()
        {
            X = 3,
            Position = ConsolePosition.Bottom,
            IsStatic = false,
            Center = true
        };

        public void ScreenMessage(string message, int msec = 3000)
        {
            ScreenMessage(message, msec, ScreenMessageCoords.ResolveX() == Index ? ColorSet.InvertedScreenMesage : ColorSet.ScreenMesage, ScreenMessageCoords);
        }

        public void ScreenMessage(string message, int msec, ColorSet colors, ConsoleCoords coordinates)
        {
            if (coordinates.Center)
            {
                coordinates.Y = Utils.GetCenterPosition(message.Length);
            }
            coordinates.Focus();
            colors.ChangeColor();
            Utils.Echo(message, false);
            Utils.ResetColor();
            Timer.Set(msec, onEnd);

            void onEnd()
            {
                coordinates.Focus();
                Utils.ResetColor();
                Utils.ClearLine(true);
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
}