namespace ImageSteganography
{
    public abstract class Arguments
    {
        public const string CommandHide = "hide";
        public const string CommandDiscover = "discover";

        public string Command { get; private set; }
        public string Key { get; private set; }
        public string ImageFilePath { get; private set; }

        protected Arguments(string[] arguments)
        {
            Command = GetAtIndexOrDefault(arguments, 0);
            Key = GetAtIndexOrDefault(arguments, 1);
            ImageFilePath = GetAtIndexOrDefault(arguments, 2);
        }

        public static Arguments FromArgs(string[] arguments)
        {
            var command = GetAtIndexOrDefault(arguments, 0).ToLower();

            switch (command)
            {
                case CommandHide:
                    return new HideArguments(arguments);
                case CommandDiscover:
                    return new DiscoverArguments(arguments);
            }

            return null;
        }

        protected static string GetAtIndexOrDefault(string[] arguments, int index)
        {
            return index < arguments.Length ? arguments[index] : string.Empty;
        }
    }

    public class HideArguments : Arguments
    {
        public string MessageFilePath { get; private set; }
        public string OutputImageFilePath { get; private set; }

        public HideArguments(string[] arguments) : base(arguments)
        {
            MessageFilePath = GetAtIndexOrDefault(arguments, 3);
            OutputImageFilePath = GetAtIndexOrDefault(arguments, 4);
        }
    }

    public class DiscoverArguments : Arguments
    {
        public string OutputMessageFilePath { get; private set; }

        public DiscoverArguments(string[] arguments) : base(arguments)
        {
            OutputMessageFilePath = GetAtIndexOrDefault(arguments, 3);
        }
    }
}