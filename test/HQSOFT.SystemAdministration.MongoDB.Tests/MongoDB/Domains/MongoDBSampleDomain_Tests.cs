using HQSOFT.SystemAdministration.Samples;
using Xunit;

namespace HQSOFT.SystemAdministration.MongoDB.Domains;

[Collection(MongoTestCollection.Name)]
public class MongoDBSampleDomain_Tests : SampleManager_Tests<SystemAdministrationMongoDbTestModule>
{

}
