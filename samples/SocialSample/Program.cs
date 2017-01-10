using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace SocialSample
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = new WebHostBuilder();
            var url = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_TOKEN"))
                || string.IsNullOrEmpty(url))
            {
                // IIS/ANCM or no config
                builder.UseKestrel();
            }
            else
            {
                // Remove or Kestrel will complain
                builder.UseSetting(WebHostDefaults.ServerUrlsKey, string.Empty);

                var uri = new Uri(url);
                builder.UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, uri.Port, endpointOptions =>
                    {
                        if (string.Equals(uri.Scheme, "https", StringComparison.Ordinal))
                        {
                            endpointOptions.UseHttps(LoadCertificate());
                        }
                    });
                });
            }

            var host = builder.UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }

        private static X509Certificate2 LoadCertificate()
        {
            var socialSampleAssembly = typeof(Startup).GetTypeInfo().Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(socialSampleAssembly, "SocialSample");
            var certificateFileInfo = embeddedFileProvider.GetFileInfo("compiler/resources/cert.pfx");
            using (var certificateStream = certificateFileInfo.CreateReadStream())
            {
                byte[] certificatePayload;
                using (var memoryStream = new MemoryStream())
                {
                    certificateStream.CopyTo(memoryStream);
                    certificatePayload = memoryStream.ToArray();
                }

                return new X509Certificate2(certificatePayload, "testPassword");
            }
        }
    }
}
