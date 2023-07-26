using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace dgt.poc.webadfsdocker.Pages
{
    [Authorize]
    public class AuthorizeModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public AuthorizeModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}
