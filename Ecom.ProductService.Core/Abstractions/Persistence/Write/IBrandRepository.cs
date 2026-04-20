using Ecom.ProductService.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Core.Abstractions.Persistence.Write
{
    public interface IBrandRepository
    {
        Task<bool> CheckNameAscii(string NameAscii);
    }
}
