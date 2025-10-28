using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TonysMcpTools.Excepctions
{
    public class NuGetPackageException : Exception
    {
        public string PackageName { get; }

        public NuGetPackageException(string packageName, string message)
            : base(message)
        {
            PackageName = packageName;
        }

        public NuGetPackageException(string packageName, string message, Exception innerException)
            : base(message, innerException)
        {
            PackageName = packageName;
        }
    }
}
