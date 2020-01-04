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
namespace devMobile.IoT.Sht31TestHarness
{
   using System;
   using Meadow;
   using Meadow.Devices;
   using Meadow.Foundation.Sensors.Atmospheric;

   public class MeadowApp : App<F7Micro, MeadowApp>
   {
      private readonly Sht31D sensor;

      public MeadowApp()
      {
         try
         {
            Console.WriteLine("Initialising SHT31D...");

            sensor = new Sht31D(Device.CreateI2cBus());

            sensor.Updated += Sensor_Updated;

            sensor.StartUpdating();
         }
         catch( Exception ex)
         {
            Console.WriteLine(ex.Message);
         }
      }

      private void Sensor_Updated(object sender, Meadow.Peripherals.Sensors.Atmospheric.AtmosphericConditionChangeResult e)
      {
         Console.WriteLine($"Temperature: {e.New.Temperature}C Humidity: {e.New.Humidity}%");
      }
   }
}
