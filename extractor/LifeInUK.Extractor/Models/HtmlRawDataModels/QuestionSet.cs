using System.Collections.Generic;

namespace LifeInUK.Extractor.Models.HtmlRawDataModels
{
    public class QuestionSet
    {
        public QuestionSet()
        {
            TimeLimitInSeconds = 2700;
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public List<int> QuestionIds { get; set; }
        public int TimeLimitInSeconds { get; }
        public List<Question> Questions { get; set; }
    }
}