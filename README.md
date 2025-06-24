# Twitter Bot with Playwright

This is a simple Twitter bot created using Playwright with C#. The bot can perform basic Twitter operations such as logging in, posting tweets, liking tweets, and following users.

## Prerequisites

- .NET SDK (version 8.0 or higher)
- Playwright for .NET
- A Twitter/X account

## Setup

1. Install the required packages:
```bash
dotnet add package Microsoft.Playwright
dotnet tool install --global Microsoft.Playwright.CLI
```

2. Install browser dependencies:
```bash
playwright install
```

3. Install system dependencies (on Ubuntu/Debian):
```bash
sudo apt-get install -y libwoff1 libharfbuzz-icu0 libenchant-2-2 libhyphen0 libmanette-0.2-0
```

## Configuration

Edit the `Program.cs` file and replace the placeholder credentials with your own Twitter/X account details:

```csharp
private const string EMAIL = "your_email@example.com";
private const string PASSWORD = "your_password";
private const string USERNAME = "your_username";
```

## Usage

By default, the bot will:
1. Launch a browser
2. Log in to Twitter/X using your credentials
3. Wait for a few seconds
4. Exit

You can uncomment and use any of the following methods to perform additional actions:

- `PostTweet(page, "Your tweet text here")`: Posts a new tweet
- `LikeTweets(page, count)`: Likes a specified number of tweets on your home timeline
- `FollowUser(page, "username")`: Follows a specific user

## Running the Bot

```bash
dotnet run
```

## Customization

You can modify the bot to:
- Run in headless mode (no UI) by changing `Headless = false` to `Headless = true`
- Adjust the browser speed with the `SlowMo` parameter
- Add more functionality by implementing new methods

## Important Notes

- Running automated bots on Twitter may violate their terms of service.
- Use this bot responsibly and at your own risk.
- Add delays between actions to avoid rate limiting or account suspension.
- Consider implementing CAPTCHA handling logic for robust automation.

## Troubleshooting

If you encounter any issues:
1. Check that your credentials are correct
2. Verify that you have the latest Playwright versions
3. Look for any CSS selector changes on Twitter's website that might break the automation 