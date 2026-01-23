// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LondonFhirService.Core.Tests.Unit.DeleteMe.Brokers.HashBrokers
{
    public partial class HashBrokerTests
    {
        [Fact]
        public async Task ShouldGenerateHashValues()
        {
            // given
            string pepper = "pepper123!@";
            string orgCode = "TESTICB1";

            Dictionary<string, string> nhsNumbers =
                new()
                {
                    ["9435797881"] = "",
                    ["9435792103"] = "",
                    ["9435740820"] = "",
                    ["9435732992"] = "",
                    ["9435749895"] = "",
                    ["9435783309"] = "",
                    ["9435741894"] = "",
                    ["9435775039"] = "",
                    ["9435764428"] = "",
                    ["9435764479"] = "",
                    ["5560006033"] = "",
                    ["9435807194"] = "",
                    ["9435780156"] = "",
                    ["9435758649"] = "",
                    ["9435817777"] = "",
                    ["9435726097"] = "",
                    ["9435792170"] = "",
                    ["9435764126"] = "",
                    ["9435780334"] = "",
                    ["9435786766"] = "",
                    ["9435737048"] = "",
                    ["9435773982"] = "",
                    ["9435789102"] = "",
                    ["9435815065"] = "",
                    ["9435806430"] = "",
                    ["9435738540"] = "",
                    ["9435772846"] = "",
                    ["9435810624"] = "",
                    ["9435754422"] = "",
                    ["9435726755"] = "",
                    ["9435801684"] = "",
                    ["9435802508"] = "",
                    ["9435753868"] = "",
                    ["9435756018"] = "",
                    ["9435809812"] = "",
                    ["9435714463"] = "",
                    ["9435719104"] = "",
                    ["9435762263"] = "",
                    ["9435806236"] = "",
                    ["9435722067"] = "",
                    ["9435814743"] = "",
                    ["9435777066"] = "",
                    ["9435797237"] = "",
                    ["9435769047"] = "",
                    ["9435753973"] = "",
                    ["9999999999"] = "",
                    ["9999999998"] = "",
                    ["9999999997"] = "",
                    ["9999999996"] = "",
                    ["9999999995"] = "",
                    ["9999999994"] = "",
                    ["9999999993"] = "",
                    ["9999999992"] = "",
                    ["9999999991"] = "",
                    ["9999999990"] = "",
                    ["9999999982"] = "",
                    ["9999999981"] = "",
                    ["9999999980"] = ""
                };

            // when
            foreach (string nhsNumber in nhsNumbers.Keys.ToList())
            {
                nhsNumbers[nhsNumber] =
                    await this.hashBroker.GenerateSha256HashAsync(
                        nhsNumber,
                        pepper);
            }

            // then
            output.WriteLine("NHS Number, SHA256 Hash");

            //foreach ((string nhsNumber, string hash) in nhsNumbers)
            //{
            //    output.WriteLine($"{nhsNumber}, {hash}");
            //}

            output.WriteLine("");

            foreach ((string nhsNumber, string hash) in nhsNumbers)
            {
                output.WriteLine(
                    $"INSERT INTO [dbo].[PdsDatas] ([Id], [NhsNumber], [OrgCode]) " +
                    $"VALUES (NEWID(), '{hash}', '{orgCode}');");
            }
        }
    }
}
