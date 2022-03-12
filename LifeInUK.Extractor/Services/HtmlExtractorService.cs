using System;
using System.Collections.Concurrent;
using System.Text.Json;
using HtmlAgilityPack;
using LifeInUK.Extractor.Extensions;
using LifeInUK.Extractor.Extractors;
using LifeInUK.Extractor.Extractors.HtmlExtractors;
using LifeInUK.Extractor.Models;
using LifeInUK.Extractor.Options;
using LifeInUK.Extractor.Parsers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LifeInUK.Extractor.Services
{
    public class HtmlExtractorService : IExtractorService<HtmlDocument, HtmlNode>
    {
        private readonly ILogger<HtmlExtractorService> _logger;
        private readonly ExtractorOptions _extractorOptions;

        public HtmlExtractorService(
            ILoggerFactory loggerFactory,
            IOptions<ExtractorOptions> extractorOptions)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            if (extractorOptions == null || extractorOptions.Value == null)
                throw new ArgumentNullException(nameof(extractorOptions));

            _logger = loggerFactory.CreateLogger<HtmlExtractorService>();
            _extractorOptions = extractorOptions.Value;
            _questionMetadataExtractor = new QuestionMetadataHtmlExtractor(loggerFactory);
            _questionExtractor = new QuestionHtmlExtractor(loggerFactory, extractorOptions);
            _parser = new HtmlParser();
        }

        private readonly IParser<HtmlDocument> _parser;
        public IParser<HtmlDocument> Parser
        {
            get
            {
                return _parser;
            }
        }

        private readonly IExtractor<QuestionMetadataCollection, HtmlNode> _questionMetadataExtractor;
        public IExtractor<QuestionMetadataCollection, HtmlNode> QuestionMetadataExtractor
        {
            get
            {
                return _questionMetadataExtractor;
            }
        }

        private readonly IExtractor<Question, HtmlNode> _questionExtractor;

        public IExtractor<Question, HtmlNode> QuestionExtractor
        {
            get
            {
                return _questionExtractor;
            }
        }

        public void Extract(QuestionRawData rawData)
        {
            var htmlDoc = Parser.Parse(rawData.RawData);
            var questionBag = new ConcurrentBag<Question>();

            var questionMetaNode = htmlDoc.GetNode(_extractorOptions.XPath.QuestionMetadata);
            if (questionMetaNode == null)
            {
                _logger.LogWarning("QuestionMetadata node not found.");
                return;
            }

            var quesMetadata = QuestionMetadataExtractor.Extract(questionMetaNode);

            var questionNodes = htmlDoc.GetNodes(_extractorOptions.XPath.Questions);
            if (questionNodes.Count == 0)
            {
                _logger.LogWarning("QuestionMetadata node not found.");
                return;
            }

            var count = 1;
            foreach (var quesNode in questionNodes)
            {
                var question = QuestionExtractor.Extract(quesNode);
                if (quesMetadata.Metadata.TryGetValue(question.Id.ToString(), out var metadata))
                {
                    question.Metadata = metadata;
                }
                else
                {
                    question.Errors.Add("Metadata not found");
                }
                questionBag.Add(question);
                LogQuestion(question, rawData.FileName, count);
                count++;
            }
        }

        private void LogQuestion(
            Question question,
            string filename,
            int questionNumber)
        {
            _logger.LogInformation("Question {QuestionNumber} from {FileName}:\n{QuestionData}",
                                questionNumber,
                                filename,
                                JsonSerializer.Serialize(question,
                                new JsonSerializerOptions
                                {
                                    WriteIndented = true,
                                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                                }));
        }
    }
}