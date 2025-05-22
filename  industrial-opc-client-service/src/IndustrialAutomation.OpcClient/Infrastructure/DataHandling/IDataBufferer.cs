using IndustrialAutomation.OpcClient.Domain.Models;
using System.Collections.Generic; // For List if GetItems was used

namespace IndustrialAutomation.OpcClient.Infrastructure.DataHandling
{
    // Sticking to the specific instruction for this file:
    // Include methods Add(BufferedDataItem item), TryTake(out BufferedDataItem item), Count(), Clear().

    public interface IDataBufferer
    {
        void Add(BufferedDataItem item);
        bool TryTake(out BufferedDataItem? item); // Nullable if it might not find one
        int Count();
        void Clear();
    }
}