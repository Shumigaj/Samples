using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureEducation.Models;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;

namespace AzureEducation.Services
{
    public class CourseStore
    {
        private readonly DocumentClient _client;
        private readonly Uri _coursesLink;
        private readonly FeedOptions _feedOptions;
        private readonly IConfiguration _configuration;

        public CourseStore(IConfiguration configuration)
        {
            _configuration = configuration;
            var uri = new Uri(_configuration["CosmosDbEndpoint"]);
            var key = _configuration["CosmosDbAccessKey"];
            _feedOptions = new FeedOptions { EnableCrossPartitionQuery = true };
            _client = new DocumentClient(uri, key);
            _coursesLink = UriFactory.CreateDocumentCollectionUri("azureeducationcosmosdb", "courses");
        }

        public async Task InsertCourses(IEnumerable<Course> courses)
        {
            foreach (var course in courses)
            {
                await _client.CreateDocumentAsync(_coursesLink, course);
            }
        }

        public IList<Course> GetAllCourses()
        {
            var courses = _client.CreateDocumentQuery<Course>(_coursesLink, _feedOptions)
                                .OrderBy(c => c.Title)
                                .ToList();

            return courses;
        }
    }
}
