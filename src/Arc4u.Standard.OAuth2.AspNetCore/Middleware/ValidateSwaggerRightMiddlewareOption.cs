namespace Arc4u.Standard.OAuth2.Middleware
{
    public class ValidateSwaggerRightMiddlewareOption
    {
        public int Access { get; set; }

        public string Path { get; set; }

        public string ContentToDisplay { get; set; } = "{\"swagger\": \"2.0\",  \"info\": { \"title\": \"You are not authorized!\", \"version\": \"1.0.0\" }, \"consumes\": [ \"application/json\"  ],  \"produces\": [ \"application/json\" ]}";
    }
}
