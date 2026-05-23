//using FluentMigrator;
//using O24OpenAPI.Core.Attributes;
//using O24OpenAPI.Data.Extensions;
//using O24OpenAPI.Data.Migrations;
//using O24OpenAPI.Framework.Domain;

//namespace O24OpenAPI.Framework.Migrations;

///// <summary>
///// The add table JobExecutionHistory
///// </summary>
///// <seealso cref="AutoReversingMigration"/>
//[O24OpenAPIMigration(
//    "2026/04/03 15:08:00:0000000",
//    "2. Add Table JobExecutionHistory",
//    MigrationProcessType.Installation
//)]
//[Environment(EnvironmentType.All)]
//public class AddTableJobExecutionHistory : AutoReversingMigration
//{
//    /// <summary>
//    /// Ups this instance
//    /// </summary>
//    public override void Up()
//    {
//        if (!Schema.Table(nameof(JobExecutionHistory)).Exists())
//        {
//            Create.TableFor<JobExecutionHistory>();
//            Create.Index().OnTable(nameof(JobExecutionHistory)).OnColumn(nameof(JobExecutionHistory.JobName));
//        }
//    }
//}
