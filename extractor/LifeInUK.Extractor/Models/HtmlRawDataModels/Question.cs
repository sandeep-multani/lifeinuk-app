using System.Collections.Generic;
using System.Linq;
using LifeInUK.Extractor.Extensions;

namespace LifeInUK.Extractor.Models.HtmlRawDataModels
{
    public class Question
    {
        public Question()
        {
            Options = new List<QuestionOption>();
            Errors = new List<string>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public List<QuestionOption> Options { get; set; }
        public QuestionMetadata Metadata { get; set; }
        public List<string> Errors { get; set; }
        public string Hash
        {
            get
            {
                var hashes = Options.ConvertAll(x => x.Label.Base64Encode().ToLower());
                hashes.Add(Title.Base64Encode().ToLower());
                return string.Join(",", hashes.ToArray());
            }
        }
    }
}