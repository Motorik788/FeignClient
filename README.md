# FeignClient
My realisation of FeignClient is a declarative web service client

using FeignRestClient;
using Newtonsoft.Json;

[FeignClient("")]
    public interface ITest
    {
        [RequestMapping(Method = RequestMappingAttribute.HttpMethod.GET, path = "http://date.jsontest.com/")]
        string GetStringResponse();

        [RequestMapping(Method = RequestMappingAttribute.HttpMethod.GET, path = "http://echo.jsontest.com/key1/{value1}/key2/{value2}")]
        string GetStringResponse2([PathVariable]string value1, [PathVariable] int value2);

        [RequestMapping(Method = RequestMappingAttribute.HttpMethod.GET, path = "http://echo.jsontest.com/key1/{value1}/key2/{value2}")]
        object GetJObjectResponse([PathVariable]string value1, [PathVariable] int value2);

        [RequestMapping(Method = RequestMappingAttribute.HttpMethod.GET, path = "http://date.jsontest.com/")]
        DateTimeResponse GetObjectResponse();

        [RequestMapping(Method = RequestMappingAttribute.HttpMethod.GET, path = "http://md5.jsontest.com/")] //http://md5.jsontest.com/?text=<text-to-md5>
        string GetStringResponseMd5(string text);

        [RequestMapping(Method = RequestMappingAttribute.HttpMethod.GET, path = "http://headers.jsontest.com/")]
        string getHeaders([RequestHeader("Custom-Header")]string someHeader);
    }

    [FeignClient("https://jsonplaceholder.typicode.com")]
    public interface ITest2
    {
        [RequestMapping(Method = RequestMappingAttribute.HttpMethod.POST, path = "posts/")]
        string createResource([RequestBody] PostResource resource);
    }

    public class PostResource
    {
        public string Title { get; set; }
        public string Body { get; set; }

        public int UserId { get; set; }
    }

    public class DateTimeResponse
    {

        public DateTime date;
        [JsonProperty(PropertyName = "milliseconds_since_epoch")]
        public long linuxTime;

        public override string ToString()
        {
            return date + "\n" + linuxTime;
        }
    }
	
	 static void Main(string[] args)
    {
	
		 var k = FeignBuilder.Build<ITest>();

            Console.WriteLine(k.GetObjectResponse());
            Console.WriteLine(k.GetStringResponse());
            Console.WriteLine(k.GetStringResponse2("test-value", 228));
            Console.WriteLine(k.GetJObjectResponse("kek", 12).GetType());
            Console.WriteLine(k.GetStringResponseMd5("hello world"));
            Console.WriteLine(k.getHeaders("custom-value-header"));

            var k2 = FeignBuilder.Build<ITest2>();
            Console.WriteLine(k2.createResource(new PostResource() { Body = "some big text", Title = "some title", UserId = 2 }));

            Console.WriteLine(k.GetObjectResponse());
			
	}
