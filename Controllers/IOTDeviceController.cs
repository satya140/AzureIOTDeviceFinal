using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AzureIOTDevice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IOTDeviceController : ControllerBase
    {

        private readonly string _connectionString = "HostName=iothub23rdjune.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=LTK2d+c/RHurWEoEz5L0oPC55yEn1Jsj1/0PGxZ2qO8=";
       // private readonly string _deviceId = "deviceid-01";

        private readonly ManageDevice manageDevice;

        public IOTDeviceController()
        {
            manageDevice = new ManageDevice();
        }
        // GET: api/<IOTDeviceController>
        [HttpGet("GetAllDeviceList")]
        public async Task<IActionResult> GetAllDeviceList()
        {
            return Ok(await manageDevice.GetDeviceListAsync(_connectionString));
        }

        //// GET api/<IOTDeviceController>/5
        //[HttpGet("")]
        //public async Task<IActionResult> Get(string id)
        //{
        //    await manageDevice.GetDeviceTwinAsync(_connectionString, id);
        //}

        // POST api/<IOTDeviceController>
        [HttpPost("CreateDevice")]
        public async Task<IActionResult> CreateDevice( string deviceId)
        {
           return Ok(await manageDevice.CreateDeviceAsync(_connectionString, deviceId));
        }

        // PUT api/<IOTDeviceController>/5
        [HttpPut("UpdatedesiredProperties")]
        public async Task<IActionResult> UpdatedesiredProperties(string deviceid)
        {
            return Ok(await manageDevice.UpdateDesiredPropertiesAsync(_connectionString, deviceid));
        }


        // PUT api/<IOTDeviceController>/5
        [HttpPut("UpdateReportedProperties")]
        public async Task<IActionResult> UpdateReportedProperties(string deviceid)
        {
            return Ok(await manageDevice.UpdateReportedPropertiesAsync(_connectionString, deviceid));
        }

        // DELETE api/<IOTDeviceController>/5
        [HttpDelete("DeleteDevice")]
        public async Task<IActionResult> DeleteDevice(string deviceid)
        {
            return Ok(await manageDevice.DeleteDeviceAsync(_connectionString, deviceid));
        }


        [HttpPost("Sendtelemetry")]
        public async Task<IActionResult> SendTelemetry(string deviceid)
        {
            return Ok(await manageDevice.SendDeviceTocloudMessageAsync(_connectionString, deviceid));
        }
    }
}
