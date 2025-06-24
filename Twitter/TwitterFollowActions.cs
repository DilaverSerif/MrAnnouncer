public class TwitterFollowActions
{
    private TwitterMotor twitterMotor;
    public TwitterFollowActions(TwitterMotor twitterMotor)
    {
        this.twitterMotor = twitterMotor;
    }
    
    public async Task FollowUserAsync(string username)
    {
        await twitterMotor.GoURL($"https://x.com/{username}");
        var followButton = await twitterMotor.Page.FindElement("button:has-text('Takip et'), button:has-text('Follow')");
        await twitterMotor.Page.HumanLikeClick(followButton);
    }

    public async Task UnfollowUserAsync(string username)
    {
        await twitterMotor.GoURL($"https://x.com/{username}");
        var unfollowButton = await twitterMotor.Page.FindElement("button:has-text('Takip etmeyi bırak'), button:has-text('Unfollow')");
        await twitterMotor.Page.HumanLikeClick(unfollowButton);
    }
}