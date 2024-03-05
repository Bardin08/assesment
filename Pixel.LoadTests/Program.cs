using NBomber.CSharp;

var scenario = Scenario.Create("pixel_request_scenario", async _ =>
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "load-tests-agent");
        httpClient.DefaultRequestHeaders.Add("Referer",
            "https://nbomber.com/docs/getting-started/hello_world_tutorial/");
        var response = await httpClient.GetAsync("http://localhost:5123/track");

        return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
    })
    .WithoutWarmUp()
    .WithLoadSimulations(
        Simulation.RampingConstant(1000, during: TimeSpan.FromSeconds(60))
    );

NBomberRunner
    .RegisterScenarios(scenario)
    .Run();