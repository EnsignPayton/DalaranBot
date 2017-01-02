using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DalaranBot
{
    /// <summary>
    /// Handles the user voting system
    /// </summary>
    public class VotingManager
    {
        private bool isVoting;
        private int totalVotes;
        private List<Category> categories = new List<Category>();

        #region Methods
        public string Start(string cmd)
        {
            if (isVoting) return "Voting has already started.";

            var catTexts = cmd.Split('"').Where(s => !string.IsNullOrWhiteSpace(s));

            foreach (var cat in catTexts)
                categories.Add(new Category { Text = cat });

            if (categories.Count == 0)
                return "No categories specified.";

            var sb = new StringBuilder("Voting has started:");

            for (var i = 0; i < categories.Count; i++)
                sb.Append($"\n{i + 1}. {categories[i].Text}");

            isVoting = true;

            return sb.ToString();
        }

        public string AddVote(string cmd)
        {
            if (!isVoting) return "There are no votes in progress";

            int iCat;
            if (!int.TryParse(cmd, out iCat))
                return $"Invalid category: \"{cmd}\" is not an integer.";

            iCat -= 1;

            if (iCat < 0 || iCat >= categories.Count)
                return $"Invalid category: {iCat + 1} is not a valid category number.";

            categories[iCat].AddVote();

            totalVotes++;

            return string.Empty;
        }

        public string Stop()
        {
            if (!isVoting) return "There are no votes in progress to stop.";

            var maxCats = categories.GetMaxCategories();
            var sb = new StringBuilder("Voting has ended.");

            for (var i = 0; i < categories.Count; i++)
            {
                sb.Append("\n");
                if (maxCats.Contains(categories[i])) sb.Append("**");

                sb.Append($"{i + 1}. \"{categories[i].Text}\": ");
                sb.Append($"{categories[i].Votes} Vote");
                if (categories[i].Votes != 1) sb.Append("s");
                sb.Append($" ({(int)((double)categories[i].Votes / (double)totalVotes * 100.0)}%)");

                isVoting = false;

                if (maxCats.Contains(categories[i])) sb.Append("**");
            }

            totalVotes = 0;
            categories = new List<Category>();

            return sb.ToString();
        }
        #endregion
    }

    /// <summary>
    /// Provides extension methods useful for the voting manager
    /// </summary>
    internal static class VotingUtils
    {
        public static List<Category> GetMaxCategories(this List<Category> thisOne)
        {
            var maxVotes = thisOne.Select(cat => cat.Votes).Concat(new[] {0}).Max();

            return thisOne.Where(cat => cat.Votes == maxVotes).ToList();
        }
    }

    /// <summary>
    /// Represents a category which may be voted for
    /// </summary>
    internal class Category
    {
        public string Text { get; set; }
        public int Votes { get; set; }

        public void AddVote()
        {
            Votes++;
        }
    }
}
