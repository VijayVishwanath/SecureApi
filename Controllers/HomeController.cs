using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SecureApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private IConfiguration configuration;
        private string generatedTocken;      
        public HomeController(IConfiguration iConfig, ILogger<HomeController> logger)
        {
            configuration = iConfig;
            _logger = logger;
        }

        [HttpGet]       
        public string GetTocken()
        {
            using (AesManaged aes = new AesManaged())
            {
                var tickCount = DateTime.UtcNow.Ticks;
                string plaintext = "";
                byte[] iv = Convert.FromBase64String(configuration.GetSection("Credentials").GetSection("iv").Value);
                byte[] key = Convert.FromBase64String(configuration.GetSection("Credentials").GetSection("key").Value);
                byte[] cipherText = Encoding.ASCII.GetBytes(configuration.GetSection("Credentials").GetSection("password").Value + tickCount);

                ICryptoTransform decryptor = aes.CreateEncryptor(key, iv);
                
                using (MemoryStream opStream = new MemoryStream())
                {
                    using (MemoryStream ms = new MemoryStream(cipherText))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            cs.CopyTo(opStream);
                        }
                    }
                    plaintext = Convert.ToBase64String(opStream.ToArray());                    
                }

                generatedTocken = Convert.ToBase64String(Encoding.ASCII.GetBytes(configuration.GetSection("Credentials").GetSection("username").Value + ":" + plaintext + ":" + tickCount));                
            }
            return generatedTocken;         
        }
    }                          
}
