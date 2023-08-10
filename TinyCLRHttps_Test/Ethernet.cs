using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace TinyCLRHttps_Test
{

  internal class Ethernet
  {
    public bool linkReady { get; private set; }
    public bool DebugLog = true;

    NetworkController networkController;

    public Ethernet()
    {
      linkReady = false;
    }


    public void ethernet_start(int timeout)
    {
      //Reset external phy.
      var gpioController = GpioController.GetDefault();
      var resetPin = gpioController.OpenPin(SC20260.GpioPin.PG3);

      resetPin.SetDriveMode(GpioPinDriveMode.Output);

      resetPin.Write(GpioPinValue.Low);
      Thread.Sleep(100);

      resetPin.Write(GpioPinValue.High);
      Thread.Sleep(100);

      networkController = NetworkController.FromName(SC20260.NetworkController.EthernetEmac);

      var networkInterfaceSetting = new EthernetNetworkInterfaceSettings();

      var networkCommunicationInterfaceSettings = new BuiltInNetworkCommunicationInterfaceSettings();

      // Only Switch Settings
      //networkInterfaceSetting.Address = new IPAddress(new byte[] { 169, 254, 168, 50 });
      //networkInterfaceSetting.SubnetMask = new IPAddress(new byte[] { 255, 255, 0, 0 });
      //networkInterfaceSetting.GatewayAddress = new IPAddress(new byte[] { 169, 254, 168, 51 });

      networkInterfaceSetting.Address = new IPAddress(new byte[] { 192, 168, 0, 2 });
      networkInterfaceSetting.SubnetMask = new IPAddress(new byte[] { 255, 255, 255, 0 });
      networkInterfaceSetting.GatewayAddress = new IPAddress(new byte[] { 192, 168, 0, 249 });

      networkInterfaceSetting.DnsAddresses = new IPAddress[]
          {
                    new IPAddress(new byte[] { 75, 75, 75, 75}),
                    new IPAddress(new byte[] { 75, 75, 75, 76 })
          };

      networkInterfaceSetting.MacAddress = new byte[] { 0x01, 0x2, 0x03, 0x03, 0x02, 0x01 };
      networkInterfaceSetting.DhcpEnable = false;
      networkInterfaceSetting.DynamicDnsEnable = false;

      networkController.SetInterfaceSettings(networkInterfaceSetting);
      networkController.SetCommunicationInterfaceSettings(networkCommunicationInterfaceSettings);

      networkController.NetworkAddressChanged += NetworkController_NetworkAddressChanged;
      networkController.NetworkLinkConnectedChanged += NetworkController_NetworkLinkConnectedChanged;

      networkController.SetAsDefaultController();
      networkController.Enable();

      DateTime time = DateTime.Now;
      time.AddMilliseconds(timeout);
      while (linkReady == false && DateTime.Now < time) ;
      if (linkReady)
        DebugWrite("Network is ready to use");
      else
        DebugWrite("Network Connection ran into a timeout");
    }
    private void NetworkController_NetworkLinkConnectedChanged
        (NetworkController sender, NetworkLinkConnectedChangedEventArgs e)
    {
      //Raise connect/disconnect event.
    }

    private void NetworkController_NetworkAddressChanged
        (NetworkController sender, NetworkAddressChangedEventArgs e)
    {

      var ipProperties = sender.GetIPProperties();
      var address = ipProperties.Address.GetAddressBytes();

      linkReady = address[0] != 0;
      DebugWrite("IP: " + address[0] + "." + address[1] + "." + address[2] + "." + address[3]);
      if (linkReady)
      {
        DebugWrite("Network is ready to use");
      }
    }

    private void DebugWrite(string str)
    {
      if (DebugLog)
        Debug.WriteLine(DateTime.Now + " Ethernet  : " + str);
    }
  }
}
