using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace SaveFileInGitPOC.Services
{

    public class GitService
    {
        private readonly string _localRepoPath;
        private readonly string _remoteUrl;
        private readonly string _username;
        private readonly string _pat; // Personal Access Token

        public GitService(string localRepoPath, string remoteUrl, string username, string pat)
        {
            _localRepoPath = localRepoPath;
            _remoteUrl = remoteUrl;
            _username = username;
            _pat = pat;

            if (!Directory.Exists(_localRepoPath) || !Repository.IsValid(_localRepoPath))
            {
                var parent = Path.GetDirectoryName(_localRepoPath);
                if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
                {
                    Directory.CreateDirectory(parent);
                }

                if (Directory.Exists(_localRepoPath) && !Repository.IsValid(_localRepoPath))
                {
                    Directory.Delete(_localRepoPath, recursive: true);
                }

                var cloneOptions = new CloneOptions
                {
                    Checkout = true
                };
                cloneOptions.FetchOptions.CredentialsProvider = GetCredentialsHandler();

                try
                {
                    Repository.Clone(_remoteUrl, _localRepoPath, cloneOptions);
                }
                catch (LibGit2SharpException ex)
                {
                    // Avoid printing the token; include the libgit2 exception details for diagnosis
                    var message = $"Failed to clone '{_remoteUrl}' into '{_localRepoPath}'. LibGit2Sharp: {ex.Message}";
                    throw new InvalidOperationException(message, ex);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to clone '{_remoteUrl}' into '{_localRepoPath}'. See inner exception for details.", ex);
                }
            }
        }

        public void SaveAndCommitFile(string fileName, string content, string commitMessage)
        {
            using (var repo = new Repository(_localRepoPath))
            {
                var filePath = Path.Combine(_localRepoPath, fileName);
                File.WriteAllText(filePath, content);

                Commands.Stage(repo, fileName);

                // Check if there are changes to commit
                var status = repo.RetrieveStatus();
                if (status.IsDirty)
                {
                    try
                    {
                        var author = new Signature(_username, "devendra.kumar@trustage.com", DateTimeOffset.Now);
                        var committer = author;

                        repo.Commit(commitMessage, author, committer);

                        var options = new PushOptions
                        {
                            CredentialsProvider = GetCredentialsHandler()
                        };

                        repo.Network.Push(repo.Branches["savefilehere"], options);
                    }
                    catch (LibGit2SharpException ex)
                    {
                        throw new InvalidOperationException($"Failed to commit and push changes. LibGit2Sharp: {ex.Message}", ex);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Failed to commit and push changes. See inner exception for details.", ex);
                    }
                }
                // Optionally: return a message indicating no changes were detected
            }
        }


        private CredentialsHandler GetCredentialsHandler()
        {
            return (url, usernameFromUrl, types) =>
                new UsernamePasswordCredentials { Username = _username, Password = _pat };
        }
    }
}
