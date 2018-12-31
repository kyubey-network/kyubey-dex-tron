using Andoromeda.Kyubey.Dex.Lib;
using Microsoft.AspNetCore.NodeServices;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Lib
{
    public class EosSignatureValidator
    {
        private readonly INodeServices node;
        private const string nodeFileSrc = "./eos-ecc-wrap";
        public EosSignatureValidator(INodeServices node)
        {
            this.node = node;
        }

        public async Task<bool> Verify(string signature, string data, string publicKey)
        {
            return await node.InvokeExportAsync<bool>(nodeFileSrc, "verify", signature, data, publicKey);
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EosSignatureValidatorExtensions
    {
        public static IServiceCollection AddEosSignatureValidator(this IServiceCollection self)
        {
            return self.AddSingleton<EosSignatureValidator>();
        }
    }
}