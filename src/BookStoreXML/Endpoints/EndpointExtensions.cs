using System.Reflection;

namespace BookStoreXML.Endpoints
{
    public static class EndpointExtensions
    {
        public static void RegisterEndpoints(this WebApplication app)
        {
            var endpointTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in endpointTypes)
            {
                var instance = Activator.CreateInstance(type) as IEndpoint;
                instance?.MapEndpoints(app);
            }
        }
    }
}
