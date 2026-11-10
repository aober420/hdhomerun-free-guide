using System.Net;
using FreeGuide;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Tasks;
using Moq;
using Xunit;

public class GuideRefreshTests {
 const string Original="<tv><channel id='old'/><programme channel='old'/></tv>";
 const string Fresh="<tv><channel id='new'/><programme channel='new'/></tv>";

 [Theory][InlineData(false)][InlineData(true)]
 public async Task SuccessfulDownloadsInvalidateOnlyTheirProviderAfterReplacingGuide(bool force){
  using var fixture=new Fixture();
  // A force download must also bypass a future download schedule.
  await fixture.Seed(force?40:-1);
  fixture.Listings.Setup(m=>m.SaveListingProvider(fixture.Provider,false,false))
   .Returns((ListingsProviderInfo provider,bool _,bool __)=>{
    Assert.Equal(Fresh,File.ReadAllText(fixture.Guide));
    Assert.Equal(fixture.PreviousSchedule,File.ReadAllText(fixture.Schedule));
    Assert.Equal("tuner1",Assert.Single(provider.EnabledTuners));
    Assert.Equal("local-channel",Assert.Single(provider.ChannelMappings).Name);
    return Task.FromResult(provider);
   });
  await fixture.Download(force,HttpStatusCode.OK,Fresh);
  fixture.Listings.Verify(m=>m.SaveListingProvider(fixture.Provider,false,false),Times.Once);
  fixture.Listings.VerifyNoOtherCalls();
  Assert.Same(fixture.Other,fixture.Options.ListingProviders[1]);
  Assert.Equal(Fresh,await File.ReadAllTextAsync(fixture.Guide));
 }

 [Theory][InlineData(false)][InlineData(true)]
 public async Task FailedDownloadsKeepPreviousGuideAndDoNotInvalidateCache(bool invalidXml){
  using var fixture=new Fixture();await fixture.Seed(-1);
  await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Download(true,
   invalidXml?HttpStatusCode.OK:HttpStatusCode.Forbidden,invalidXml?"<tv/>":"Forbidden"));
  Assert.Equal(Original,await File.ReadAllTextAsync(fixture.Guide));
  Assert.Equal(fixture.PreviousSchedule,await File.ReadAllTextAsync(fixture.Schedule));
  fixture.Listings.VerifyNoOtherCalls();
 }

 [Fact] public async Task HourlyCheckDoesNotInvalidateCacheWhenDownloadIsNotDue(){
  using var fixture=new Fixture();await fixture.Seed(24);
  await fixture.Download(false,HttpStatusCode.OK,Fresh);
  fixture.Listings.VerifyNoOtherCalls();
  Assert.Equal(Original,await File.ReadAllTextAsync(fixture.Guide));
 }

 sealed class Fixture:IDisposable {
  readonly string root=Path.Combine(Path.GetTempPath(),"freeguide-refresh-"+Guid.NewGuid());
  public string Guide {get;}
  public string Schedule {get;}
  public string PreviousSchedule {get;private set;}="";
  public ListingsProviderInfo Provider {get;}
  public ListingsProviderInfo Other {get;}=new(){Id="other",Type="xmltv",Path="other.xml"};
  public LiveTvOptions Options {get;}
  public Mock<IListingsManager> Listings {get;}=new(MockBehavior.Strict);
  readonly DownloadTask task;
  public Fixture(){
   var folder=Path.Combine(root,"hdhomerun-free-guide");Directory.CreateDirectory(folder);
   Guide=Path.Combine(folder,"guide.xml");Schedule=Path.Combine(folder,"next-download.txt");
   Provider=new(){Id="guide1",Type="xmltv",Path=Guide,EnableAllTuners=false,EnabledTuners=["tuner1"],ChannelMappings=[new(){Name="local-channel",Value="guide-channel"}]};
   Options=new(){TunerHosts=[new(){Id="tuner1",Type="hdhomerun",Url="http://tuner.test"}],ListingProviders=[Provider,Other]};
   var config=new Mock<IConfigurationManager>();config.Setup(m=>m.GetConfiguration("livetv")).Returns(Options);
   var paths=new Mock<IApplicationPaths>();paths.SetupGet(p=>p.DataPath).Returns(root);
   var tasks=new Mock<ITaskManager>();tasks.SetupGet(m=>m.ScheduledTasks).Returns([]);
   task=new(config.Object,paths.Object,tasks.Object,Listings.Object);
  }
  public async Task Seed(int hours){
   PreviousSchedule=DateTimeOffset.UtcNow.AddHours(hours).ToString("O");
   await File.WriteAllTextAsync(Guide,Original);await File.WriteAllTextAsync(Schedule,PreviousSchedule);
  }
  public async Task Download(bool force,HttpStatusCode status,string xml){
   using var client=new HttpClient(new Handler(status,xml));
   await task.ExecuteDownloadAsync(new Progress<double>(),CancellationToken.None,force,client);
  }
  public void Dispose()=>Directory.Delete(root,true);
 }
 sealed class Handler(HttpStatusCode status,string xml):HttpMessageHandler {
  protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)=>
   Task.FromResult(request.RequestUri!.AbsolutePath=="/discover.json"
    ?new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent("{\"DeviceID\":\"tuner1\",\"DeviceAuth\":\"test-secret\"}")}
    :new HttpResponseMessage(status){Content=new StringContent(xml)});
 }
}
