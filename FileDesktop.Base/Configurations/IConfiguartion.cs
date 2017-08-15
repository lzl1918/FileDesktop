using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Configurations
{
    public interface IConfiguartion
    {
        ScanFilterOptions GloabalExclude { get; }
        ItemIncludeOptions IncludeItems { get; }
        List<DirectoryScanOptions> ScanDirectories { get; }

        ItemsResult EnumerateItems();
    }
}
