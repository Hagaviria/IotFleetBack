using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var pgPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddPostgres(
                    name: "postgres",
                    password: pgPassword,
                    port: 5432)          
                .WithDataVolume();


var db = postgres.AddDatabase("myappdb");


var api = builder.AddProject<Projects.IotFleet>("api")
                 .WithReference(db);

builder.Build().Run();
