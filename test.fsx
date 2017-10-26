#I @"TickSpec/bin/Debug"
#r @"TickSpec.dll"

let featureFileContent = [
    @"Feature: Web login testing";
    @"";
    @"    @scenariotag";
    @"    Scenario: Successful login using <browser>";
    @"    Given I use browser <browser>";
    @"    When I try to login using username <username> and password <password>";
    @"    Then I am logged in";
    @"";
    @"    @nonsharedtag";
    @"    Examples:";
    @"    | username      | password      |";
    @"    | test          | psw           |";
    @"    | test2         | psw2          |";
    @"";
    @"    Scenario: Show page using <browser>";
    @"    Given I use browser <browser>";
    @"    When I go to the main page";
    @"    Then The main page is displayed";
    @"";
    @"@Smoke @Smoke2";
    @"Shared Examples:";
    @"| browser       |";
    @"| Firefox       |";
    @"";
    @"@Regression @Regression2";
    @"Shared Examples of @scenariotag:";
    @"| browser       |";
    @"| Firefox       |";
    @"| Explorer      |";
    @"| Chrome        |";
    @"";
]

printfn "=============================================="

open TickSpec.FeatureParser


let featureSource =
    featureFileContent
    |> Seq.toArray
    |> parseFeature

printfn "FeatureSource name: %A" featureSource.Name

featureSource.Scenarios
    |> Seq.iter (fun scenario -> 
        printfn "-----------------"
        printfn "Scenario %A" scenario.Name
        scenario.Parameters
            |> Seq.iter ( fun paramPair ->
                printfn " * Param %s = %s" (fst paramPair) (snd paramPair)
            )
    )

printfn "=============================================="
