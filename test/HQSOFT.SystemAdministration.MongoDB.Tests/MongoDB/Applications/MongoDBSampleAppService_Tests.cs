using HQSOFT.SystemAdministration.MongoDB;
using HQSOFT.SystemAdministration.Samples;
using Xunit;

namespace HQSOFT.SystemAdministration.MongoDb.Applications;

[Collection(MongoTestCollection.Name)]
public class MongoDBSampleAppService_Tests : SampleAppService_Tests<SystemAdministrationMongoDbTestModule>
{

}
