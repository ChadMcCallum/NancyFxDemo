using System;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;

namespace NancyFxDemo.v1
{
    public class DemoModule : NancyModule
    {
        public DemoModule() : base("/v1/hello")
        {
            Before += context => Authenticate(context);

            Get["/"] = x => HelloWorld();
            Get["/{name}"] = x => HelloName(x.Name);
            Put["/"] = x => HelloPut();
            Post["/create"] = x => CreateName();
            Delete["/{id}"] = x => Del();
            Get["/exception"] = x => { throw new ApplicationException("LOLERROR"); };
            Get["/data"] = x => GetData();
            Get["/moredata"] = x => GetRateLimitedData();

            After += context => AddExpiry(context);
        }

        private object GetRateLimitedData()
        {
            Context.NegotiationContext.Headers.Add("X-Rate-Limit-Limit", "5");
            return Enumerable.Range(1, 10);
        }

        private object GetData()
        {
            if (this.Request.Headers.IfModifiedSince.HasValue)
            {
                if (this.Request.Headers.IfModifiedSince.Value < DateTime.UtcNow.AddMinutes(1))
                    return HttpStatusCode.NotModified;
            }
            var pagedRequest = new PagedRequest();
            if(Request.Query.ContainsKey("$top"))
                pagedRequest.top = Request.Query["$top"];
            if(Request.Query.ContainsKey("$skip"))
                pagedRequest.skip = Request.Query["$skip"];
            return Enumerable.Range(pagedRequest.skip, pagedRequest.top);
        }

        private void AddExpiry(NancyContext context)
        {
            context.Response.Headers.Add("Expires", DateTime.UtcNow.AddHours(1).ToString("r"));
        }

        private Response Authenticate(NancyContext context)
        {
            if (string.IsNullOrEmpty(context.Request.Headers.Authorization))
            {
                return new Response() {StatusCode = HttpStatusCode.Unauthorized};
            }
            return null;
        }

        private object Del()
        {
            return 204;
        }

        private object CreateName()
        {
            var request = this.Bind<HelloWorldRequest>();
            return new CreateResponse {Id = new Random().Next()};
        }

        private object HelloPut()
        {
            var request = this.Bind<HelloWorldRequest>();
            return new HelloWorldResponse {Message = "Hello " + request.Name + ", how are you?"};
        }

        private object HelloName(string name)
        {
            return new HelloWorldResponse {Message = "Hello " + name};
        }

        private object HelloWorld()
        {
            return new HelloWorldResponse {Message = "Hello World!"};
        }
    }

    public class HelloWorldRequest
    {
        public string Name { get; set; }
    }

    public class HelloWorldResponse
    {
        public string Message { get; set; }
    }

    public class CreateResponse
    {
        public int Id { get; set; }
    }

    public class PagedRequest
    {
        public PagedRequest()
        {
            top = 20;
            skip = 0;
        }

        public int top { get; set; }
        public int skip { get; set; }
    }
}