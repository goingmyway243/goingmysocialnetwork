namespace GoingMy.Shared;

public static class SharedServices
{
    public const string IdentityApi = "identity-api";
    public const string PostApi = "post-api";
    public const string ChatApi = "chat-api";
    public const string IdentityDb = "identity-db";
    public const string Postgresql = "postgresql";
    public const string MongoDB = "mongodb";
    public const string PostDb = "post-db";
    public const string ChatDb = "chat-db";
    public const string UserDb = "user-db";
    public const string UserApi = "user-api";
    public const string WebApp = "web-app";
    public const string Redis = "redis";
    public const string Kafka = "kafka";
    public const string RabbitMQ = "rabbitmq";

    public static class KafkaTopics
    {
        public const string UserRegistered = "goingmy.user.registered";
        public const string UserCreated = "goingmy.user.created";
        public const string UserUpdated = "goingmy.user.updated";
        public const string UserDeleted = "goingmy.user.deleted";
    }

    public static class KafkaConsumerGroups
    {
        public const string UserService = "user-service";
        public const string PostService = "post-service";
        public const string ChatService = "chat-service";
    }
}
