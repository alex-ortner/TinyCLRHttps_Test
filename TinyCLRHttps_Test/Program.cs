using GHIElectronics.TinyCLR.Native;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace TinyCLRHttps_Test
{
  internal class Program
  {
    static void Main()
    {
      byte[] servercert = Resource1.GetBytes(Resource1.BinaryResources.certificate_for_server);

      X509Certificate certificate = new X509Certificate(servercert)
      {
        PrivateKey = Resource1.GetBytes(Resource1.BinaryResources.server),
      };

      SystemTime.SetTime(new System.DateTime(2023, 08, 10, 18, 35, 0));

      Ethernet eth = new Ethernet();
      eth.ethernet_start(1000);

      WebServer web = new WebServer(certificate);
      web.Start();

      while (true)
      {
        Thread.Sleep(10000);
        Debug.WriteLine("Program running");
      }

    }
  }
}
