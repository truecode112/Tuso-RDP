using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Remotely.Server.Attributes;
using Remotely.Server.Hubs;
using Remotely.Server.Models;
using Remotely.Server.Services;
using Remotely.Shared.Utilities;
using Remotely.Shared.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Remotely.Server.Auth;
using Immense.RemoteControl.Server.Services;
using Remotely.Server.Services.RcImplementations;
using Immense.RemoteControl.Server.Abstractions;
using Immense.RemoteControl.Shared.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Remotely.Server.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UninstallDevice : ControllerBase
    {
        private readonly IHubContext<ServiceHub> _serviceHub;
        private readonly IDesktopHubSessionCache _desktopSessionCache;
        private readonly IServiceHubSessionCache _serviceSessionCache;
        private readonly IApplicationConfig _appConfig;
        private readonly IOtpProvider _otpProvider;
        private readonly IHubEventHandlerEx _hubEvents;
        private readonly IDataService _dataService;
        private readonly SignInManager<RemotelyUser> _signInManager;

        public UninstallDevice(
            SignInManager<RemotelyUser> signInManager,
            IDataService dataService,
            IDesktopHubSessionCache desktopSessionCache,
            IHubContext<ServiceHub> serviceHub,
            IServiceHubSessionCache serviceSessionCache,
            IOtpProvider otpProvider,
            IHubEventHandlerEx hubEvents,
            IApplicationConfig appConfig)
        {
            _dataService = dataService;
            _serviceHub = serviceHub;
            _desktopSessionCache = desktopSessionCache;
            _serviceSessionCache = serviceSessionCache;
            _appConfig = appConfig;
            _otpProvider = otpProvider;
            _hubEvents = hubEvents;
            _signInManager = signInManager;
        }

        [HttpPost("{deviceID}")]
        public async Task<IActionResult> Post(string deviceID)
        {
            return await InitiateUninstall(deviceID);
        }

        private async Task<IActionResult> InitiateUninstall(string deviceID)
        {
            if (!_serviceSessionCache.TryGetByDeviceId(deviceID, out var targetDevice) ||
                !_serviceSessionCache.TryGetConnectionId(deviceID, out var serviceConnectionId))
            {
                return NotFound("The target device couldn't be found.");
            }

            if (User.Identity.IsAuthenticated &&
               !_dataService.DoesUserHaveAccessToDevice(targetDevice.ID, _dataService.GetUserByNameWithOrg(User.Identity.Name)))
            {
                return Unauthorized();
            }

            await _serviceHub.Clients.Client(serviceConnectionId).SendAsync("UninstallAgent");
            _dataService.RemoveDevices(new string[] { deviceID });
            return Ok("Uninstall Agent Success!");
        }

    }
}