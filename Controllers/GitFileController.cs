using Microsoft.AspNetCore.Mvc;
using SaveFileInGitPOC.Models;
using SaveFileInGitPOC.Services;

namespace SaveFileInGitPOC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GitFileController
    {
        [HttpPost("savefile")]
        public string SaveFile([FromBody] FileCommitRequest request)
        {
            try
            {
                //_gitService.SaveAndCommitFile(request.FileName, request.Content, request.CommitMessage);
                return "File saved, committed, and pushed successfully.";
            }
            catch (Exception ex)
            {
                // Log the exception
                return "An error occurred: {ex.Message}";
            }
        }
    }
}
