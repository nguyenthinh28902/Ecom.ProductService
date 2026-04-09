using Ecom.ProductService.Core.Abstractions.Persistence;
using Ecom.ProductService.Infrastructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Infrastructure.Repositories
{
    public class ReadOnlyUnitOfWork : UnitOfWork, IReadOnlyUnitOfWork
    {
        public ReadOnlyUnitOfWork(ReadOnlyDbContext context, IServiceProvider serviceProvider)
            : base(context, serviceProvider)
        {
        }
    }
}
