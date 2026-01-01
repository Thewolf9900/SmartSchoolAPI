using Microsoft.AspNetCore.Hosting;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IWebHostEnvironment _env;

        public EmailTemplateService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> GetTemplateAsync(string templateName, Dictionary<string, string> placeholders)
        {
            var path = Path.Combine(_env.WebRootPath, "EmailTemplates", templateName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Email template not found: {templateName}");
            }

            var template = await File.ReadAllTextAsync(path);

            if (placeholders != null)
            {
                foreach (var placeholder in placeholders)
                {
                    // Replace {{Key}} with Value
                    template = template.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
                }
            }

            return template;
        }
    }
}
