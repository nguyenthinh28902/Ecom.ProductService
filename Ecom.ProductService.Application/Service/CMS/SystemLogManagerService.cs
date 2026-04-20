using Ecom.ProductService.Application.Interface.Auth;
using Ecom.ProductService.Application.Interface.CMS;
using Ecom.ProductService.Core.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.Service.CMS
{
    public class SystemLogManagerService : ISystemLogManagerService
    {
        private readonly ILogger<SystemLogManagerService> _logger;
        private readonly ICurrentUserService _currentUserService;
        public SystemLogManagerService(ILogger<SystemLogManagerService> logger, ICurrentUserService currentUserService)
        {
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public SystemLog CreateSystemLog()
        {
            var userId = _currentUserService.UserId;
            var systemLog = new SystemLog
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Ipaddress = _currentUserService.IpAddress, 
                UserAgent = _currentUserService.UserAgent, 
            };
            return systemLog;
        }

    }
}
