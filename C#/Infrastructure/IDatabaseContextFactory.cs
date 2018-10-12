using System;
using Microsoft.Extensions.Options;

namespace ODAL.Infrastructure
{
    public interface IDatabaseContextFactory : IDisposable
    {
        IDatabaseContext Init(IOptions<ConnectionString> connectionStringSettings);
    }
}

