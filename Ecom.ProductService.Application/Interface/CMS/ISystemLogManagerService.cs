using Ecom.ProductService.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.Interface.CMS
{
    public interface ISystemLogManagerService
    {
        SystemLog CreateSystemLog();
    }
}
