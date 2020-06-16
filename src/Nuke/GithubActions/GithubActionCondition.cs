namespace Rocket.Surgery.Nuke.GithubActions
{
    public class GithubActionCondition
    {
        public GithubActionCondition(string condition)
        {
            Condition = condition;
        }
        public string Condition { get; }
        public static implicit operator string(GithubActionCondition condition)
        {
            return condition?.Condition;
        }

        public static implicit operator GithubActionCondition(string condition)
        {
            return new GithubActionCondition(condition);
        }

        public static GithubActionCondition Success = new GithubActionCondition("success()");
        public static GithubActionCondition Always = new GithubActionCondition("always()");
        public static GithubActionCondition Cancelled = new GithubActionCondition("cancelled()");
        public static GithubActionCondition Failure = new GithubActionCondition("failure()");

        public override string ToString() => Condition;
    }
}