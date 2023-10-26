namespace ImmGames.Publisher.Editor.Utils
{
    public class Github
    {
        public static string MakePagesLinkFromRepo(string repoLink)
        {
            var trimBeginning = repoLink.Replace("https://github.com/", "");
            var trimEnd = trimBeginning.TrimEnd('/'); 
            var userAndRepo = trimEnd.Split('/');
            return $"https://{userAndRepo[0]}.github.io/{userAndRepo[1]}/".ToLower();
        }
    }
}