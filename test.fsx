#I @"TickSpec/bin/Debug"
#r @"TickSpec.dll"

open TickSpec.FeatureParser

printfn "=============================================="

[
    @"Feature: Optic SoftKeys Device";
    @"";
    @"Background:";
    @"    Given user connects to <GX> Optic Device Web Server";
    @"    And user resets all counters";
    @"    And user registers SoftKey press to do action: increment counter by 1 and send content of counters to display";
    @"";
    @"Scenario Outline: On <GX> Optic device enabling <x> SoftKey in single-shot mode";
    @"    When user enables SoftKey <x> in single-shot mode";
    @"    And user press button <x> on Optic device";
    @"    Then counter '<x>' has value 1";
    @"    And backlit of all buttons is off";
    @"";
    @"@SmokeSetSc1";
    @"Examples:";
    @"    | GX | x       |";
    @"    | G6 | Bottom1 |";
    @"    | G7 | Left1   |";
    @"";
    @"@SmokeSetSc2";
    @"Examples:";
    @"    | GX | x       |";
    @"    | G6 | Bottom1 |";
    @"    | G7 | Left1   |";
    @"";
    @"#Known Bug - As long as this scenario is red on G6 the problem is still there";
    @"@KnownBug";
    @"Scenario Outline: <GX> Trublemaking scenario enabling <x>";
    @"    When user enables SoftKey <x> in single-shot mode";
    @"    And user enables SoftKey <x> in continuous mode";
    @"    And user enables all SoftKeys in continuous mode";
    @"    And user press button <y> on Optic device";
    @"    And user press all buttons on Optic device";
    @"    And user press all buttons on Optic device";
    @"    Then all counters except '<y>' have value 2";
    @"    And counter '<y>' has value 3";
    @"";
    @"@SmokeSetSc3";
    @"Examples:";
    @"    | GX | x       | y       |";
    @"    | G6 | Bottom3 | Bottom2 |";
    @"    | G7 | Left3   | Right2  |";
    @"";
    @"Shared Examples:";
    @"    | GX | x       | y       |";
    @"    #| G7 | Right3  | Left4   |";
    @"    | G7 | Right4  | Left1   |";
    @"    | G6 | Bottom1 | Bottom2 |";
    @"    #| G6 | Bottom2 | Bottom3 |";
]
|> Seq.toArray
|> parseFeature
|> ignore

//printfn "FeatureSource name: %A" featureSource.Name

//featureSource.Scenarios
//    |> Seq.iter (fun scenario -> 
//        printfn "-----------------"
//        printfn "Scenario %A" scenario.Name
//        scenario.Parameters
//            |> Seq.iter ( fun paramPair ->
//                printfn " * Param %s = %s" (fst paramPair) (snd paramPair)
//            )
//    )

printfn "=============================================="
