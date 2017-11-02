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
        let createTestCaseData (feature:Feature) (scenario:Scenario) =
            let tags = 
                scenario.Tags 
                |> Array.fold (fun tags tag -> tags + tag + " " ) ""
            (new TestCaseData(scenario))
                .SetName(scenario.Name)
                .SetProperty("Feature", feature.Name.Substring("Feature: ".Length))
                .SetProperty("Tags", tags.TrimEnd())
        
        let createFeatureData (feature:Feature) =
            feature.Scenarios
            |> Seq.map (createTestCaseData feature)

        let assembly = Assembly.GetExecutingAssembly() 
        let definitions = new StepDefinitions(assembly)
        let featureStream = assembly.GetManifestResourceStream(source)   
        let feature = definitions.GenerateFeature(source,featureStream)
        createFeatureData feature
