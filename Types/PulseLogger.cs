using Rhinox.Perceptor;


namespace Rhinox.Pulse
{
    public class PulseLogger : CustomLogger
    {
        protected override ILogTarget[] GetTargets()
        {
            return new ILogTarget[] { new CoutLogTarget() };
        }
    }
}