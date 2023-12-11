using System;
using System.Text;
using Rhinox.Perceptor;


namespace Rhinox.Pulse
{
    public class CoutLogTarget : BaseLogTarget
    {
        internal static bool Silence = false;

        public override void ApplySettings(LoggerSettings settings)
        {
            base.ApplySettings(settings);

            Console.OutputEncoding = Encoding.UTF8;
        }

        protected override void OnLog(LogLevels level, string message, UnityEngine.Object associatedObject = null)
        {
            if (Silence)
                return;

            switch (level)
            {
                case LogLevels.Trace:
                case LogLevels.Debug:
                case LogLevels.Info:
                case LogLevels.Warn:
                    Console.WriteLine(message, associatedObject);
                    break;
                case LogLevels.Error:
                case LogLevels.Fatal:
                    if (ShouldThrowErrors)
                        throw new Exception(message);
                    else
                        Console.WriteLine(message, associatedObject);
                    break;
            }
        }
    }
}