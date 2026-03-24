using Ecom.ProductService.Core.Models.Db;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Infrastructure.DbContexts
{
    public partial class EcomProductDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Đưa logic dùng biến hằng của bạn vào đây
                optionsBuilder.UseSqlServer(ConnectionStrings.ProductDb);
            }
        }
    }
}
