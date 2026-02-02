namespace SaveFileInGitPOC.Models
{
    public class FileCommitRequest
    {
        public string FileName { get; set; }
        public string Content { get; set; }
        public string CommitMessage { get; set; }
    }
}
