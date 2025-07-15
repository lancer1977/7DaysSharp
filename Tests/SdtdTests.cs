using System.Diagnostics;
using NUnit.Framework;
using PolyhydraGames.SdtdSharp.Models;

namespace PolyhydraGames.SdtdSharp.Tests;
[TestFixture]
public class SdtdTests
{
  public SdtdBridge SdtdBridge { get; }
  public SdtdApiClient SdtdApiClient { get;  }

  public SdtdTests()
  {
      var config = new SdtdServerConfig("192.168.0.169", 9090, false, "lancer1977", "toshiba");
      SdtdApiClient = new SdtdApiClient(config, new HttpClient());
      SdtdBridge = new SdtdBridge(SdtdApiClient);
    }

    [Test]
  public async Task ListPlayers()
  {
      var result = await SdtdBridge.ListPlayers();
      WriteLine(result);
      Assert.That(result != string.Empty);
    }
    protected void WriteLine(string line)
    {
        Console.WriteLine(line);
        Debug.WriteLine(line);
    }
}
