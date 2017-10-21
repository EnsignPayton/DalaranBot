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
        private bool _isVoting;
        private int _totalVotes;
        private List<Category> _categories = new List<Category>();

        #region Methods

        public string Start(string cmd)
        {
            if (_isVoting) return "Voting has already started.";

            var catTexts = cmd.Split('"').Where(s => !string.IsNullOrWhiteSpace(s));

            foreach (var cat in catTexts)
                _categories.Add(new Category { Text = cat });

            if (_categories.Count == 0)
                return "No categories specified.";

            var sb = new StringBuilder("Voting has started:");

            for (var i = 0; i < _categories.Count; i++)
                sb.Append($"\n{i + 1}. {_categories[i].Text}");

            _isVoting = true;

            return sb.ToString();
        }

        public string AddVote(string cmd)
        {
            if (!_isVoting) return "There are no votes in progress";

            int iCat;
            if (!int.TryParse(cmd, out iCat))
                return $"Invalid category: \"{cmd}\" is not an integer.";

            iCat -= 1;

            if (iCat < 0 || iCat >= _categories.Count)
                return $"Invalid category: {iCat + 1} is not a valid category number.";

            _categories[iCat].AddVote();

            _totalVotes++;

            return string.Empty;
        }

        public string Stop()
        {
            if (!_isVoting) return "There are no votes in progress to stop.";

            var maxCats = _categories.GetMaxCategories();
            var sb = new StringBuilder("Voting has ended.");

            for (var i = 0; i < _categories.Count; i++)
            {
                sb.Append("\n");
                if (maxCats.Contains(_categories[i])) sb.Append("**");

                sb.Append($"{i + 1}. \"{_categories[i].Text}\": ");
                sb.Append($"{_categories[i].Votes} Vote");
                if (_categories[i].Votes != 1) sb.Append("s");
                sb.Append($" ({(int)((double)_categories[i].Votes / (double)_totalVotes * 100.0)}%)");

                _isVoting = false;

                if (maxCats.Contains(_categories[i])) sb.Append("**");
            }

            _totalVotes = 0;
            _categories = new List<Category>();

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
