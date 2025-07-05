using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Opc.System.Services.Management.Domain.Aggregates;
using Opc.System.Services.Management.Domain.Repositories;

namespace Opc.System.Services.Management.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Provides data access methods for OpcClientInstance aggregates,
    /// translating repository calls into EF Core database queries.
    /// </summary>
    public class OpcClientInstanceRepository : IOpcClientInstanceRepository
    {
        private readonly ManagementDbContext _context;

        public OpcClientInstanceRepository(ManagementDbContext context)
        {
            _context = context;
        }

        public async Task<OpcClientInstance?> GetByIdAsync(ClientInstanceId id, CancellationToken cancellationToken)
        {
            return await _context.OpcClientInstances
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<OpcClientInstance>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.OpcClientInstances
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<OpcClientInstance>> GetByIdsAsync(IEnumerable<ClientInstanceId> ids, CancellationToken cancellationToken)
        {
            return await _context.OpcClientInstances
                .Where(c => ids.Contains(c.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(OpcClientInstance clientInstance, CancellationToken cancellationToken)
        {
            await _context.OpcClientInstances.AddAsync(clientInstance, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(OpcClientInstance clientInstance, CancellationToken cancellationToken)
        {
            // EF Core's change tracker automatically detects changes on the aggregate.
            // Calling Update is not strictly necessary if the entity is tracked, but is explicit.
            _context.OpcClientInstances.Update(clientInstance);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}