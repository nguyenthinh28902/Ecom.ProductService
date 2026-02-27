using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Application.Interface.Auth
{
    public interface IBaseService
    {
        void EnsurePermission(string permission);
    }
}
