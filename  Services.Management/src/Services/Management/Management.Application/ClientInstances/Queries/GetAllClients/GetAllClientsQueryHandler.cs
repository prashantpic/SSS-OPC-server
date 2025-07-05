using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Opc.System.Services.Management.Domain.Repositories;

namespace Opc.System.Services.Management.Application.ClientInstances.Queries.GetAllClients
{
    /// <summary>
    /// Query to retrieve all client summaries. This is a simple marker record.
    /// </summary>
    public record GetAllClientsQuery() : IRequest<IEnumerable<ClientSummaryDto>>;

    /// <summary>
    /// Data Transfer Object for a summary view of a client instance.
    /// </summary>
    public record ClientSummaryDto(
        Guid Id,
        string Name,
        string HealthStatus,
        DateTimeOffset LastSeen);

    /// <summary>
    /// Retrieves all managed client instances and maps them to a summary DTO for display in monitoring dashboards.
    /// </summary>
    public class GetAllClientsQueryHandler : IRequestHandler<GetAllClientsQuery, IEnumerable<ClientSummaryDto>>
    {
        private readonly IOpcClientInstanceRepository _clientInstanceRepository;

        public GetAllClientsQueryHandler(IOpcClientInstanceRepository clientInstanceRepository)
        {
            _clientInstanceRepository = clientInstanceRepository;
        }

        /// <summary>
        /// Handles the request to get all client summaries.
        /// </summary>
        /// <param name="query">The query object.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of client summary DTOs.</returns>
        public async Task<IEnumerable<ClientSummaryDto>> Handle(GetAllClientsQuery query, CancellationToken cancellationToken)
        {
            var clientInstances = await _clientInstanceRepository.GetAllAsync(cancellationToken);

            var dtos = clientInstances.Select(instance => new ClientSummaryDto(
                instance.Id.Value,
                instance.Name,
                instance.HealthStatus.ToString(),
                instance.LastSeen
            )).ToList();

            return dtos;
        }
    }
}