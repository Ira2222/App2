namespace App2.Api.Constants;

public static class AppConstants
{
    public static class Cache
    {
        public const string TodosTag = "todos";
        public const string TodosPolicy = "Todos";
    }

    public static class RateLimiting
    {
        public const string FixedPolicy = "fixed";
    }

    public static class Routes
    {
        public const string TodosBase = "/api/todos";
    }
}
