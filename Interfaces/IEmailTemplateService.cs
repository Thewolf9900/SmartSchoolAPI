using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    public interface IEmailTemplateService
    {
        Task<string> GetTemplateAsync(string templateName, Dictionary<string, string> placeholders);
    }
}
