using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Opc.System.Services.Management.Application.Shared;
using Opc.System.Services.Management.Domain.Aggregates;
using Opc.System.Services.Management.Domain.Repositories;

namespace Opc.System.Services.Management.Application.Migration.Commands.ImportConfiguration
{
    /// <summary>
    /// Placeholder factory interface to get a configuration parser.
    /// </summary>
    public interface IConfigurationParserFactory
    {
        IConfigurationParser GetParser(string fileName);
    }

    /// <summary>
    /// Placeholder parser interface.
    /// </summary>
    public interface IConfigurationParser
    {
        Task<ClientConfiguration> ParseAsync(Stream fileContent, CancellationToken cancellationToken);
    }
    
    /// <summary>
    /// Command to import a legacy configuration from a file.
    /// </summary>
    public record ImportConfigurationCommand(
        string FileName,
        Stream FileContent) : IRequest<Result<Guid>>;


    /// <summary>
    /// Orchestrates the process of parsing a legacy configuration file, creating a new client aggregate, and persisting it.
    /// </summary>
    public class ImportConfigurationCommandHandler : IRequestHandler<ImportConfigurationCommand, Result<Guid>>
    {
        private readonly IOpcClientInstanceRepository _clientInstanceRepository;
        private readonly IConfigurationParserFactory _parserFactory;

        public ImportConfigurationCommandHandler(IOpcClientInstanceRepository clientInstanceRepository, IConfigurationParserFactory parserFactory)
        {
            _clientInstanceRepository = clientInstanceRepository;
            _parserFactory = parserFactory;
        }

        /// <summary>
        /// Handles the configuration import logic.
        /// </summary>
        /// <param name="command">The command containing the file to import.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Result containing the new client instance's Guid on success.</returns>
        public async Task<Result<Guid>> Handle(ImportConfigurationCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var parser = _parserFactory.GetParser(command.FileName);
                if (parser is null)
                {
                    return Result<Guid>.Failure($"No parser found for file type: {Path.GetExtension(command.FileName)}");
                }

                var newConfig = await parser.ParseAsync(command.FileContent, cancellationToken);
                
                string instanceName = Path.GetFileNameWithoutExtension(command.FileName);
                var newInstance = OpcClientInstance.Create(instanceName, newConfig);

                await _clientInstanceRepository.AddAsync(newInstance, cancellationToken);

                return Result<Guid>.Success(newInstance.Id.Value);
            }
            catch (Exception ex)
            {
                // In a real app, log the exception
                return Result<Guid>.Failure($"Failed to import configuration: {ex.Message}");
            }
        }
    }
}