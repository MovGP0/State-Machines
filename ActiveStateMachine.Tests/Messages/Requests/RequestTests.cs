using ActiveStateMachine.Messages;
using ActiveStateMachine.Messages.Requests;

namespace ActiveStateMachine.Tests.Messages.Requests;

public sealed class RequestTests
{
    public static IEnumerable<object[]> RequestData => new List<object[]>
    {
        new object[] { new GetStateRequest("src", "tgt"), "get-state" },
        new object[] { new GetStateHistoryRequest("src", "tgt"), "get-state-history" },
        new object[] { new GetPossibleStatesRequest("src", "tgt"), "get-possible-states" },
        new object[] { new GetMessageHistoryRequest("src", "tgt"), "get-message-history" }
    };

    [Theory]
    [MemberData(nameof(RequestData))]
    public void ShouldHaveCorrectRequestName(StateMachineMessage req, string expectedName)
    {
        req.Name.ShouldBe(expectedName);
    }
}