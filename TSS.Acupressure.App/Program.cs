using System.Threading;
using TSS.Acupressure.App;

namespace Cod.IoT.Bootloader
{
    public class Program
    {
        private static readonly IApp app = new AcupressureApp();

        public static void Main()
        {
            // apply delay to ensure all hardware has been initialized
            Thread.Sleep(2000);

            app.Launch();
        }
    }
}
