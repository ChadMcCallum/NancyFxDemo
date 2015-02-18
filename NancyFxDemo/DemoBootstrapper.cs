using System;
using System.Linq;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Responses;

namespace NancyFxDemo
{
    public class DemoBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            pipelines.OnError += (context, exception) => HandleError(context, exception);
        }

        private Response HandleError(NancyContext context, Exception exception)
        {
            var response = new JsonResponse(new {
                Message = exception.Message,
                Description = exception.Message,
                Details = exception.StackTrace
            }, new DefaultJsonSerializer());
            response.StatusCode = HttpStatusCode.InternalServerError;

            return response;
        }
    }
}