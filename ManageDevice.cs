
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIOTDevice
{
    public class ManageDevice
    {
        public async Task<string> CreateDeviceAsync(string connectionString, string deviceId)
        {
            // Create a new instance of the RegistryManager using the IoT Hub connection string
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);

            try
            {
                // Check if the device already exists
                Device device = await registryManager.GetDeviceAsync(deviceId);
                if (device != null)
                {
                    //Console.WriteLine("Device already exists!");
                    //Console.WriteLine($"Reading Device twin: ");

                    await GetDeviceTwinAsync(connectionString, deviceId);
                    return "Device already exists!"; ;
                }

                // Create a new device instance
                device = new Device(deviceId);

                // Register the device in the IoT Hub
                device = await registryManager.AddDeviceAsync(device);

                // Console.WriteLine("Device created successfully!");

                var deviceInfo = new
                {
                    Msg = "Device Created",
                    Id = device.Id,
                    PrimaryKey = device.Authentication.SymmetricKey.PrimaryKey
                };

                return JsonConvert.SerializeObject(deviceInfo);

                //await GetDeviceTwinAsync(connectionString, deviceId);
            }
            catch (DeviceAlreadyExistsException)
            {
                return "Device already exists!";
            }
            catch (Exception ex)
            {
                return "Error creating device: {ex.Message}" + ex.Message;
            }
            finally
            {
                await registryManager.CloseAsync();
            }
        }

        public async Task<string> GetDeviceTwinAsync(string connectionString, string deviceId)
        {

            DeviceClient dClient = DeviceClient.CreateFromConnectionString(connectionString, deviceId);

            try
            {
                Twin twin = await dClient.GetTwinAsync();
                return twin.ToJson();
                //var list = JsonConvert.DeserializeObject<List<ManageDevice>>(myJsonString);
                //list.Add(new Person(1234, "carl2");
                //var convertedJson = JsonConvert.SerializeObject(list, Formatting.Indented);

                //List<string> devicename= new List<string>();
                //ManageDevice m= new ManageDevice(); 
                //DesiredProperties d= new DesiredProperties();
                //d.DeviceName = twin.DeviceId;
                //d.DesiredProperty.Add(twin.Properties.Desired.ToJson());
                //d.ReportedProperty.Add(twin.Properties.Reported.ToJson());
                // devicename.Add(d);


                //Console.WriteLine($"Device Twin received. Device ID: {twin.DeviceId}");
                //Console.WriteLine($"Desired properties: {twin.Properties.Desired.ToJson()}");
                //Console.WriteLine($"Reported properties: {twin.Properties.Reported.ToJson()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return string.Empty;
            }
            finally
            {
                await dClient.CloseAsync();
            }
        }

        public async Task<IEnumerable<string>> GetDeviceListAsync(string connectionString)
        {

            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            List<string> iotdevice = new List<string>();
            try
            {
                var devices = registryManager.CreateQuery("SELECT * FROM devices", 100);

                while (devices.HasMoreResults)
                {
                    var page = await devices.GetNextAsTwinAsync();

                    foreach (var twin in page)
                    {
                        //Console.WriteLine($"Device ID: {twin.DeviceId}");
                        //Console.WriteLine($"Connection state: {twin.ConnectionState}");
                        //Console.WriteLine($"Last activity: {twin.LastActivityTime}");
                        //Console.WriteLine("-------------------------------------");

                        //iotdevice.Add(twin.ToJson());
                        iotdevice.Add(twin.DeviceId);
                    }
                }

            }
            catch (Exception ex)
            {
                //return "Error creating device: {ex.Message}" + ex.Message;
                //return null;
            }
            finally
            {
                await registryManager.CloseAsync();
            }
            return iotdevice;
        }

        public async Task<string> DeleteDeviceAsync(string connectionString, string deviceId)
        {
            Console.WriteLine($"Deleting device - {deviceId}");
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);

            try
            {
                await registryManager.RemoveDeviceAsync(deviceId);
                return "Device {deviceId} deleted successfully!";
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error: {ex.Message}");
                return ex.ToString();
            }
            finally
            {
                await registryManager.CloseAsync();
            }
        }

        public async Task<string> UpdateDesiredPropertiesAsync(string connectionString, string deviceId)
        {

            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            string message = string.Empty;
            try
            {
                Twin twin = await registryManager.GetTwinAsync(deviceId);


                // Update desired properties
                twin.Properties.Desired["temp"] = "20";

                // Update the twin in the IoT Hub
                await registryManager.UpdateTwinAsync(twin.DeviceId, twin, twin.ETag);

                message = "Desired properties ['temp':20]updated successfully!";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
            finally
            {
                await registryManager.CloseAsync();
            }
            return message;
        }

        public async Task<string> UpdateReportedPropertiesAsync(string connectionString, string deviceId)
        {

            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            string message = string.Empty;
            try
            {
                Twin twin = await registryManager.GetTwinAsync(deviceId);

                var reportedProperties = new TwinCollection();


                reportedProperties["DeviceLocation"] = new { lat = 32.768, lon = 97.023, alt = 0 };

                DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(connectionString, deviceId, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
                await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
                return message = "Reported properties updated successfully! " + JsonConvert.SerializeObject(reportedProperties);
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
            finally
            {
                await registryManager.CloseAsync();
            }
        }

        public async Task<string> SendDeviceTocloudMessageAsync(string connectionString, string deviceId)
        {

            // RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            DeviceClient dClient = DeviceClient.CreateFromConnectionString(connectionString, deviceId, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
            {
                try
                {
                    Random rand = new Random();
                    double minTemp = 15;
                    double minHumidity = 50;

                    while (true)
                    {
                        double currentTemp = minTemp + rand.NextDouble() * 15;
                        double currentHumidity = minHumidity + rand.NextDouble() * 20;

                        //Create Json Message
                        var telemetryDataPoint = new
                        {
                            temp = currentTemp,
                            humidity = currentHumidity

                        };
                        string messageString = JsonConvert.SerializeObject(telemetryDataPoint);

                        var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messageString));

                        await dClient.SendEventAsync(message);
                        return messageString;// = "{0} > Sending message: {1}" //+ DateTime.Now, messageString;
                        //await Task.Delay(1000);
                    }
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
                finally { await dClient.CloseAsync(); }

            }
        }
    }
}
