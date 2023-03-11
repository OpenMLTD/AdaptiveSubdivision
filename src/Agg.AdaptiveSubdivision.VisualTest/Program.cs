using System.Runtime.Versioning;
using System.Windows.Forms;

namespace Agg.AdaptiveSubdivision.VisualTest;

[SupportedOSPlatform("windows")]
internal static class Program
{

    private static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new ContainerForm());
    }

}
