module TickSpec.NUnit

open System.Reflection
open NUnit.Framework
open TickSpec

/// Inherit from FeatureFixture to define a feature fixture
[<AbstractClass>]
[<TestFixture>]
type FeatureFixture (source:string) =
    [<Test>]
    [<TestCaseSource("Scenarios")>]
    member this.TestScenario (scenario:Scenario) =
        if scenario.Tags |> Seq.exists ((=) "ignore") then
            raise (IgnoreException("Ignored: " + scenario.Name))
        scenario.Action.Invoke()

    member this.Scenarios =
        let replaceParameterInScenarioName (scenarioName:string) parameter =
            scenarioName.Replace("<" + fst parameter + ">", snd parameter)
        let enhanceScenarioName parameters scenarioName =
            parameters
            |> Seq.fold replaceParameterInScenarioName scenarioName
        let createTestCaseData (feature:Feature) (scenario:Scenario) =
            let testCaseData = new TestCaseData(scenario)
            testCaseData.SetName(enhanceScenarioName scenario.Parameters scenario.Name) |> ignore
            testCaseData.SetProperty("Feature", feature.Name.Substring(9)) |> ignore
            scenario.Tags
                |> Array.iteri (fun i tag -> 
                    testCaseData.SetProperty(sprintf "Tag%d" i, tag) |> ignore
                )
            testCaseData
        let createFeatureData (feature:Feature) =
            feature.Scenarios
            |> Seq.map (createTestCaseData feature)

        let assembly = Assembly.GetExecutingAssembly() 
        let definitions = new StepDefinitions(assembly)
        let featureStream = assembly.GetManifestResourceStream(source)   
        let feature = definitions.GenerateFeature(source,featureStream)
        createFeatureData feature
