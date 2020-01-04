//---------------------------------------------------------------------------------
// Copyright (c) January 2020, devMobile Software
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//---------------------------------------------------------------------------------
namespace devMobile.IoT.FieldGateway.Client
{
   using System;
   using System.Text;
   using System.Threading;

   using devMobile.IoT.Rfm9x;

   using Meadow;
   using Meadow.Devices;
   using Meadow.Foundation.Leds;
   using Meadow.Foundation.Sensors.Atmospheric;
   using Meadow.Hardware;
   using Meadow.Peripherals.Leds;

   public class MeadowClient : App<F7Micro, MeadowClient>
   {
      private const double Frequency = 915000000.0;
      private readonly byte[] fieldGatewayAddress = Encoding.UTF8.GetBytes("LoRaIoT1");
      private readonly byte[] deviceAddress = Encoding.UTF8.GetBytes("Meadow");
      private readonly Rfm9XDevice rfm9XDevice;
      private readonly TimeSpan periodTime = new TimeSpan(0, 0, 60);
      private readonly Sht31D sensor;
      private readonly ILed Led;

      public MeadowClient()
      {
         Led = new Led(Device, Device.Pins.OnboardLedGreen);

         try
         {
            sensor = new Sht31D(Device.CreateI2cBus());

            ISpiBus spiBus = Device.CreateSpiBus(500);

            rfm9XDevice = new Rfm9XDevice(Device, spiBus, Device.Pins.D09, Device.Pins.D10, Device.Pins.D12);

            rfm9XDevice.Initialise(Frequency, paBoost: true, rxPayloadCrcOn: true);
#if DEBUG
            rfm9XDevice.RegisterDump();
#endif
            rfm9XDevice.OnReceive += Rfm9XDevice_OnReceive;
            rfm9XDevice.Receive(deviceAddress);
            rfm9XDevice.OnTransmit += Rfm9XDevice_OnTransmit;
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
         }

         while (true)
         {
            sensor.Update();

            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss}-TX T:{sensor.Temperature:0.0}C H:{sensor.Humidity:0}%");

            string payload = $"t {sensor.Temperature:0.0},h {sensor.Humidity:0}";

            Led.IsOn = true;

            rfm9XDevice.Send(fieldGatewayAddress, Encoding.UTF8.GetBytes(payload));

            Thread.Sleep(periodTime);
         }
      }

      private void Rfm9XDevice_OnReceive(object sender, Rfm9XDevice.OnDataReceivedEventArgs e)
      {
         try
         {
            string addressText = UTF8Encoding.UTF8.GetString(e.Address);
            string addressHex = BitConverter.ToString(e.Address);
            string messageText = UTF8Encoding.UTF8.GetString(e.Data);

            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss}-RX PacketSnr {e.PacketSnr:0.0} Packet RSSI {e.PacketRssi}dBm RSSI {e.Rssi}dBm = {e.Data.Length} byte message {messageText}");
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
         }
      }

      private void Rfm9XDevice_OnTransmit(object sender, Rfm9XDevice.OnDataTransmitedEventArgs e)
      {
         Led.IsOn = false;

         Console.WriteLine("{0:HH:mm:ss}-TX Done", DateTime.UtcNow);
      }
   }
}
