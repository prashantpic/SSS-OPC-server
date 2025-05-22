using IndustrialAutomation.OpcClient.Domain.Models; // For BufferedDataItem

namespace IndustrialAutomation.OpcClient.Infrastructure.DataHandling
{
    public interface IDataBufferer
    {
        void Add(BufferedDataItem item);
        bool TryTake(out BufferedDataItem? item);
        int Count();
        void Clear();
        IEnumerable<BufferedDataItem> Peek(int count); // To view items without removing
    }
}