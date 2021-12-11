namespace RestTests
{
    // Enabling the echo function for FitNesse tests - don't want to take a dependency on the SupportFunctions fixture
    // By putting it in the test assembly which is not normally deployed in production, it won't interfere if SupportFunctions is used.
    public class EchoSupport
    {
        public static object Echo(object input) => input;
    }
}