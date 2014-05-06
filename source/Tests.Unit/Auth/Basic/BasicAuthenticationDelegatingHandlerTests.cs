using Common.Auth.Basic;

namespace Tests.Unit.Auth
{
    public class BasicAuthenticationDelegatingHandlerTests : BasicAuthenticationTestBase
    {
        public BasicAuthenticationDelegatingHandlerTests()
            : base(config => config.MessageHandlers.Add(new BasicAuthenticationDelegatingHandler(
                "myrealm", BasicAuthenticationTestBase.TestValidator)))
        { }
    }


}
